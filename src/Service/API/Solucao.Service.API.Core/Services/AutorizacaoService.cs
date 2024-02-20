namespace Solucao.Service.API.Core.Services;

using System.Net;
using System.Security.Claims;
using System.Web;
using k8s.KubeConfigModels;
using MassTransit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Infrastructure.Shared.Common;
using static OpenIddict.Abstractions.OpenIddictConstants;

public class AutorizacaoService
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<SecurityUser> _signInManager;
    private readonly UserManager<SecurityUser> _userManager;

    public AutorizacaoService(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager,
            SignInManager<SecurityUser> signInManager,
            UserManager<SecurityUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }


    public async Task<IActionResult> Autorizar(ControllerBase controller, HttpContext httpContext)
    {
        var (solicitacao, aplicacao) = await ObterSolicitacaoEAplicacaoAsync(httpContext);

        var erroDeConsentimento = await VerificarTipoConsentimentoAsync(solicitacao.ClientId);
        if (!string.IsNullOrEmpty(erroDeConsentimento))
        {
            return RetornarErroDeConsentimento(controller, erroDeConsentimento);
        }

        var parametros = AnalisarParametrosOAuth(httpContext, [OpenIddictConstants.Parameters.Prompt]);

        var (usuario, tentarAutenticar) = await ObterUsuarioOuDesafiarAutenticacaoAsync(controller.User, controller);
        if (usuario is null)
        {
            return tentarAutenticar;
        }

        var tentarAutenticarSolicitacaoLogin = await GerarDesafioAutenticacaoSeNecessarioAsync(httpContext, solicitacao, parametros);
        if (tentarAutenticarSolicitacaoLogin is not null)
        {
            return tentarAutenticarSolicitacaoLogin;
        }

        var resultadoRedirecionamentoConsentimento = await VerificarReivindicacaoConsentimentoERedirecionarAsync(controller, httpContext, controller.User, solicitacao, parametros);
        if (resultadoRedirecionamentoConsentimento is not null)
        {
            return resultadoRedirecionamentoConsentimento;
        }

        var (usuarioId, reivindicacoes) = await ObterIdentidadeUsuarioEReivindicacoesAsync(usuario);
        var reivindicacoesIdentidade = new ClaimsIdentity(reivindicacoes, TokenValidationParameters.DefaultAuthenticationType, Claims.Email, Claims.Role);
        reivindicacoesIdentidade.SetScopes(solicitacao.GetScopes());
        reivindicacoesIdentidade.SetResources(await _scopeManager.ListResourcesAsync(reivindicacoesIdentidade.GetScopes()).ToListAsync());
        reivindicacoesIdentidade.SetDestinations(c => DeterminarDestinosPermitidos(reivindicacoesIdentidade, c));

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(reivindicacoesIdentidade));

        return controller.SignIn(new ClaimsPrincipal(reivindicacoesIdentidade), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> AceitarConsentimento(ControllerBase controller, HttpContext httpContext)
    {
        var (solicitacao, aplicacao) = await ObterSolicitacaoEAplicacaoAsync(httpContext);

        // Obter o usuário atualmente autenticado
        var (usuario, tentarAutenticar) = await ObterUsuarioOuDesafiarAutenticacaoAsync(httpContext.User, controller);
        if (usuario is null)
        {
            return tentarAutenticar;
        }

        // Recupera as autorizações permanentes associadas ao usuário e à aplicação cliente chamadora.
        var autorizacoes = await _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(usuario),
            client: await _applicationManager.GetIdAsync(aplicacao),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: solicitacao.GetScopes()).ToListAsync();

        // Verificar se há autorização externa
        var (respostaValida, resultadoForbid) = await VerificarAutorizacaoExternaAsync(controller, autorizacoes, aplicacao);
        if (respostaValida)
        {
            return resultadoForbid;
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(usuario);
        //TODO: ver isso no futuro (restringir a lista de escopos da autorização)
        // Os escopos concedidos correspondem aos escopos solicitados
        // mas você pode querer permitir que o usuário desmarque escopos específicos.
        // Para isso, basta restringir a lista de escopos antes de chamar SetScopes.
        principal.SetScopes(solicitacao.GetScopes());
        principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

        // Cria automaticamente uma autorização permanente para evitar exigir consentimento explícito
        // para futuras solicitações de autorização ou token contendo os mesmos escopos.
        var authorization = autorizacoes.LastOrDefault();
        authorization ??= await _authorizationManager.CreateAsync(
              principal: principal,
              subject: await _userManager.GetUserIdAsync(usuario),
              client: await _applicationManager.GetIdAsync(aplicacao),
              type: AuthorizationTypes.Permanent,
              scopes: principal.GetScopes());

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(ObterDestinos(principal, claim));
        }

        // Retornar um SignInResult solicitará ao OpenIddict que emita os tokens de acesso/identidade apropriados.
        return controller.SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> EncerrarSessao(ControllerBase controller)
    {
        return controller.RedirectToPage("/Account/Logout", new
        {
            logoutId = controller.Request.Query["id_token_hint"],
            redirectUri = controller.Request.Query["post_logout_redirect_uri"]
        });
    }

    public async Task<IActionResult> EncerrarSessaoPost(ControllerBase controller)
    {
        await _signInManager.SignOutAsync();

        return controller.SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    public async Task<IActionResult> ObterOuTrocaToken(ControllerBase controller, HttpContext httpContext)
    {
        var request = httpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("A solicitação OpenID Connect não pode ser recuperada.");

        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user is null)
            {
                return controller.Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O par de nome de usuário/senha é inválido."
                    }));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return controller.Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O par de nome de usuário/senha é inválido."
                    }));
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
            principal.AddClaim(ClaimTypes.NameIdentifier, user.Id.ToString());
            principal.AddClaim(Claims.Subject, user.Id.ToString());
            principal.AddClaim(Claims.Email, user.Email);
            principal.AddClaim(Claims.Name, user.UserName);

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(ObterDestinos(principal, claim));
            }

            // Retornar um SignInResult solicitará ao OpenIddict que emita os tokens de acesso/identidade apropriados.
            return controller.SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        else if (request.IsAuthorizationCodeGrantType() || request.IsDeviceCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var principal = (await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            var user = await _userManager.GetUserAsync(principal);
            if (user is null)
            {
                return controller.Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O usuário associado ao código de autorização/dispositivo/código de atualização não pôde ser encontrado."
                    }));
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                return controller.Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O usuário não pode trocar tokens."
                    }));
            }

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("Detalhes da aplicação cliente chamadora não podem ser encontrados.");

            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            {
                return controller.Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "O usuário logado não tem permissão para acessar esta aplicação cliente."
                    }));
            }

            principal = await _signInManager.CreateUserPrincipalAsync(user);
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
            principal.AddClaim(ClaimTypes.NameIdentifier, user.Id.ToString());
            principal.AddClaim(Claims.Subject, user.Id.ToString());
            principal.AddClaim(Claims.Email, user.Email);
            principal.AddClaim(Claims.Name, user.UserName);

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(ObterDestinos(principal, claim));
            }

            // Retornar um SignInResult solicitará ao OpenIddict que emita os tokens de acesso/identidade apropriados.
            return controller.SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("Tipo de concessão não suportado.");
    }

    //------------------------------------------------------------------------------------

    public async Task<(OpenIddictRequest request, object application)> ObterSolicitacaoEAplicacaoAsync(HttpContext httpContext)
    {
        var request = httpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("A solicitação OpenID Connect não pode ser recuperada.");

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                          throw new InvalidOperationException("Detalhes do aplicativo cliente chamador não podem ser encontrados.");

        return (request, application);
    }

    public async Task<string> VerificarTipoConsentimentoAsync(string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return "ClientId não pode ser nulo ou vazio.";
        }

        // Buscar informações do aplicativo cliente
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application == null)
        {
            return "Detalhes do aplicativo cliente não encontrados.";
        }

        // Verificar o tipo de consentimento do aplicativo cliente
        var consentType = await _applicationManager.GetConsentTypeAsync(application);
        if (consentType != ConsentTypes.Explicit)
        {
            return "Apenas clientes com tipo de consentimento explícito são permitidos.";
        }

        return string.Empty;
    }

    public IActionResult RetornarErroDeConsentimento(ControllerBase controller, string erroDeConsentimento)
    {
        return controller.Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = erroDeConsentimento
            }));
    }

    public async Task<(bool RespostaValida, ForbidResult ResultadoForbid)> VerificarAutorizacaoExternaAsync(ControllerBase controller, List<object> autorizacoes, object aplicacao)
    {    
        if (!autorizacoes.Any() && await _applicationManager.HasConsentTypeAsync(aplicacao, ConsentTypes.External))
        {
            var propriedades = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "O usuário logado não tem permissão para acessar esta aplicação cliente."
            });

            var resultadoForbid = new ForbidResult(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: propriedades);

            return (true, resultadoForbid);
        }

        return (false, null);
    }

    public Dictionary<string, StringValues> AnalisarParametrosOAuth(HttpContext httpContext, List<string> excluding = null)
    {
        excluding ??= [];

        var parameters = ObterParametros(httpContext, excluding);

        return parameters;
    }

    private static IEnumerable<string> ObterDestinos(ClaimsPrincipal principal, Claim reivindicacao)
    {
        yield return Destinations.AccessToken;

        if ((reivindicacao.Type == principal.GetClaim(Claims.Role)) ||
            (reivindicacao.Type == Claims.Role && principal.HasClaim(Claims.Role, reivindicacao.Value)))
        {
            yield return Destinations.AccessToken;
            yield return Destinations.IdentityToken;
        }
    }

    public static List<string> DeterminarDestinosPermitidos(ClaimsIdentity identidade, Claim reivindicacao)
    {
        List<string> destinosPermitidos = [];

        if (reivindicacao.Type == OpenIddictConstants.Claims.Name || reivindicacao.Type == OpenIddictConstants.Claims.Email)
        {
            destinosPermitidos.Add(OpenIddictConstants.Destinations.AccessToken);
        }

        return destinosPermitidos;
    }

    public Dictionary<string, StringValues> ObterParametros(HttpContext httpContext, List<string> excluindo)
    {
        if (httpContext.Request.HasFormContentType)
        {
            return ObterParametrosDoFormulario(httpContext, excluindo);
        }
        else
        {
            return ObterParametrosDaConsulta(httpContext, excluindo);
        }
    }

    public Dictionary<string, StringValues> ObterParametrosDoFormulario(HttpContext httpContext, List<string> excluindo)
    {
        var resultado = httpContext.Request.Form
            .Where(v => !excluindo.Contains(v.Key))
            .ToDictionary(v => v.Key, v => v.Value);

        return resultado;
    }

    public Dictionary<string, StringValues> ObterParametrosDaConsulta(HttpContext httpContext, List<string> excluindo)
    {
        var resultado = httpContext.Request.Query
            .Where(v => !excluindo.Contains(v.Key))
            .ToDictionary(v => v.Key, v => v.Value);

        return resultado;
    }

    public async Task<(SecurityUser usuario, IActionResult resultadoDesafio)> ObterUsuarioOuDesafiarAutenticacaoAsync(ClaimsPrincipal usuarioPrincipal, ControllerBase controllerBase)
    {
        // Verificar se o usuário está autenticado
        if (usuarioPrincipal.Identity != null && usuarioPrincipal.Identity.IsAuthenticated)
        {
            // Obter o ID do usuário autenticado
            var userId = _userManager.GetUserId(usuarioPrincipal);

            // Obter o usuário correspondente ao ID
            var user = await _userManager.FindByIdAsync(userId);
            return (user, null); // Não há necessidade de desafiar a autenticação
        }
        else
        {
            // Se o usuário não estiver autenticado, desafie a autenticação
            var properties = new Dictionary<string, string?>
            {
                ["RedirectUri"] = controllerBase.Url.Action("Authorize", "Authorization")
            };

            var resultadoDesafio = new ChallengeResult(
                authenticationSchemes: new[] { CookieAuthenticationDefaults.AuthenticationScheme },
                properties: new AuthenticationProperties(properties));

            return (null, resultadoDesafio);
        }
    }

    public async Task<IActionResult> GerarDesafioAutenticacaoSeNecessarioAsync(HttpContext contexto, OpenIddictRequest solicitacao, IDictionary<string, StringValues> parametros)
    {
        if (solicitacao.HasPrompt(Prompts.Login))
        {
            // Fazer logoff se o prompt for de login
            await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new ChallengeResult(
                authenticationSchemes: new[] { CookieAuthenticationDefaults.AuthenticationScheme },
                properties: new AuthenticationProperties
                {
                    RedirectUri = ConstruirUrlRedirecionamento(contexto.Request, parametros)
                });
        }

        return null;
    }

    public string ConstruirUrlRedirecionamento(HttpRequest request, IDictionary<string, StringValues> parametrosOAuth)
    {
        var url = request.PathBase + request.Path + QueryString.Create(parametrosOAuth);

        return url;
    }

    public string ObterValorReivindicacaoUsuario(ClaimsPrincipal usuario, string nomeClaimConsentimento)
    {
        return usuario.FindFirstValue(nomeClaimConsentimento);
    }

    public async Task<IActionResult> VerificarReivindicacaoConsentimentoERedirecionarAsync(ControllerBase controller, HttpContext contexto, ClaimsPrincipal usuario, OpenIddictRequest solicitacao, IDictionary<string, StringValues> parametrosOAuth)
    {
        const string ConsentNaming = "consent";
        const string GrantAccessValue = "Grant";

        var reivindicacaoUsuario = ObterValorReivindicacaoUsuario(usuario, ConsentNaming);

        if (reivindicacaoUsuario != GrantAccessValue || solicitacao.HasPrompt(Prompts.Consent))
        {
            // Redirecionar para a página de consentimento
            await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var returnUrl = HttpUtility.UrlEncode(ConstruirUrlRedirecionamento(contexto.Request, parametrosOAuth));
            var consentRedirectUrl = $"/Consent?returnUrl={returnUrl}";
            return controller.Redirect(consentRedirectUrl);
        }

        return null;
    }

    public async Task<(string UserId, List<Claim> Reivindicacoes)> ObterIdentidadeUsuarioEReivindicacoesAsync(SecurityUser usuario)
    {
        var userId = await _userManager.GetUserIdAsync(usuario);
        var reivindicacoes = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(Claims.Subject, userId),
        new Claim(Claims.Email, usuario.Email),
        new Claim(Claims.Name, usuario.UserName)
    };

        return (userId, reivindicacoes);
    }
}
