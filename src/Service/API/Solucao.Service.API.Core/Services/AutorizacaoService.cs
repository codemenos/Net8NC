namespace Solucao.Service.API.Core.Services;

using System.Net;
using System.Net.Http;
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
        var temErroDeConsentimento = !string.IsNullOrEmpty(erroDeConsentimento);
        if (temErroDeConsentimento)
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
        var (autorizacaoExterna, resultadoForbid) = await VerificarAutorizacaoExternaAsync(controller, autorizacoes, aplicacao);
        if (autorizacaoExterna)
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
        const string ID_TOKEN_HINT = "id_token_hint";
        const string POST_LOGOUT_REDIRECT_URI = "post_logout_redirect_uri";
        const string PATH_LOGOUT = "/Account/Logout";

        await Task.Delay(0);

        var logoutId = controller.Request.Query[ID_TOKEN_HINT];
        var redirectUri = controller.Request.Query[POST_LOGOUT_REDIRECT_URI];

        return controller.RedirectToPage(PATH_LOGOUT, new { logoutId, redirectUri });
    }

    public async Task<IActionResult> EncerrarSessaoPost(ControllerBase controller)
    {
        const string PATH_REDIRECT = "/";

        await _signInManager.SignOutAsync();

        var authenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
        var properties = new AuthenticationProperties { RedirectUri = PATH_REDIRECT };

        return controller.SignOut(authenticationSchemes: authenticationSchemes, properties: properties);
    }

    public async Task<IActionResult> ObterOuTrocarToken(ControllerBase controller, HttpContext httpContext)
    {
        const string ErroRequisicaoInvalida = "A solicitação não pode ser recuperada para obter ou trocar token.";
        //const string ErroDescricaoPermissaoNegada = "O usuário logado não tem permissão para acessar esta aplicação cliente.";
        const string TipoConcessaoNaoSuportado = "Tipo de concessão não suportado.";

        var request = httpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException(ErroRequisicaoInvalida);

        var (EhConcessaoDeSenha, EhAutorizadoOuDispositivoOuAtualizarToken) = ObterObjetivoDaRequisicao(request);
        if (EhConcessaoDeSenha)
        {
            return await ProcederParaTipoConcessaoRequisicaoDeSenha(controller, request);
        }

        if (EhAutorizadoOuDispositivoOuAtualizarToken)
        {
            return await ProcederParaTipoConcessaoAutorizadoOuDispositivoOuAtualizarToken(controller, httpContext, request);
        }

        throw new NotImplementedException(TipoConcessaoNaoSuportado);
    }

    //------------------------------------------------------------------------------------

    private async Task<IActionResult> ProcederParaTipoConcessaoAutorizadoOuDispositivoOuAtualizarToken(ControllerBase controller, HttpContext httpContext, OpenIddictRequest request)
    {
        const string ErroDescricaoUsuarioNaoEncontrado = "O usuário associado ao código de autorização/dispositivo/código de atualização não pôde ser encontrado.";
        const string ErroDescricaoUsuarioNaoPodeTrocarTokens = "O usuário não pode trocar tokens.";
        const string ErroDetalhesAplicacaoNaoEncontrados = "Detalhes da aplicação cliente chamadora não podem ser encontrados.";

        var principal = (await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
        {
            return CriarRespostaUsuarioNulo(controller, ErroDescricaoUsuarioNaoEncontrado);
        }

        var usuarioNaoPodeFazerLogin = !await _signInManager.CanSignInAsync(user);
        if (usuarioNaoPodeFazerLogin)
        {
            return CriarRespostaUsuarioNaoPodeFazerLogin(controller, ErroDescricaoUsuarioNaoPodeTrocarTokens);
        }

        var aplicacao = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
            throw new InvalidOperationException(ErroDetalhesAplicacaoNaoEncontrados);

        var listaAutorizacoes = await _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(aplicacao),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var (autorizacaoExterna, ResultadoForbid) = await VerificarAutorizacaoExternaAsync(controller, listaAutorizacoes, aplicacao);
        if (autorizacaoExterna)
        {
            return ResultadoForbid;
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

    private async Task<IActionResult> ProcederParaTipoConcessaoRequisicaoDeSenha(ControllerBase controller, OpenIddictRequest request)
    {
        const string ErroDescricaoConcessaoInvalida = "O par de nome de usuário/senha é inválido.";

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            return controller.Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = ErroDescricaoConcessaoInvalida
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
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = ErroDescricaoConcessaoInvalida
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

    private (bool EhConcessaoDeSenha, bool EhAutorizadoOuDispositivoOuAtualizarToken) ObterObjetivoDaRequisicao(OpenIddictRequest request)
    {
        var ehConcessaoDeSenha = request.IsPasswordGrantType();
        var ehAutorizadoOuDispositivoOuAtualizarToken = request.IsAuthorizationCodeGrantType() || request.IsDeviceCodeGrantType() || request.IsRefreshTokenGrantType();

        return (ehConcessaoDeSenha, ehAutorizadoOuDispositivoOuAtualizarToken);
    }

    public IActionResult CriarRespostaComPropriedades(ControllerBase controller, string erro, string descricaoErro)
    {
        var propriedades = CriarPropriedadesDeAutenticacao(erro, descricaoErro);

        return controller.Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: propriedades);
    }

    private AuthenticationProperties CriarPropriedadesDeAutenticacao(string erro, string descricaoErro)
    {
        return new AuthenticationProperties(new Dictionary<string, string>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = erro,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = descricaoErro
        });
    }

    public IActionResult CriarRespostaUsuarioNulo(ControllerBase controller, string descricaoErro)
    {
        return CriarRespostaComPropriedades(controller, Errors.InvalidGrant, descricaoErro);
    }

    public IActionResult CriarRespostaUsuarioNaoPodeFazerLogin(ControllerBase controller, string descricaoErro)
    {
        return CriarRespostaComPropriedades(controller, Errors.InvalidGrant, descricaoErro);
    }

    public async Task<(OpenIddictRequest request, object application)> ObterSolicitacaoEAplicacaoAsync(HttpContext httpContext)
    {
        const string ErroSolicitacaoNaoRecuperada = "A solicitação OpenID Connect não pode ser recuperada.";
        const string ErroDetalhesAplicativoNaoEncontrados = "Detalhes do aplicativo cliente chamador não podem ser encontrados.";

        var request = httpContext.GetOpenIddictServerRequest() ??
                        throw new InvalidOperationException(ErroSolicitacaoNaoRecuperada);

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                            throw new InvalidOperationException(ErroDetalhesAplicativoNaoEncontrados);

        return (request, application);
    }

    public async Task<string> VerificarTipoConsentimentoAsync(string clientId)
    {
        const string ErroClientIdNuloOuVazio = "ClientId não pode ser nulo ou vazio.";
        const string ErroDetalhesAplicativoNaoEncontrados = "Detalhes do aplicativo cliente não encontrados.";
        const string ErroTipoConsentimentoNaoExplicito = "Apenas clientes com tipo de consentimento explícito são permitidos.";

        if (string.IsNullOrEmpty(clientId))
        {
            return ErroClientIdNuloOuVazio;
        }

        // Buscar informações do aplicativo cliente
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application == null)
        {
            return ErroDetalhesAplicativoNaoEncontrados;
        }

        // Verificar o tipo de consentimento do aplicativo cliente
        var consentType = await _applicationManager.GetConsentTypeAsync(application);
        if (consentType != ConsentTypes.Explicit)
        {
            return ErroTipoConsentimentoNaoExplicito;
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

    public async Task<(bool AutorizacaoExterna, ForbidResult ResultadoForbid)> VerificarAutorizacaoExternaAsync(ControllerBase controller, List<object> autorizacoes, object aplicacao)
    {
        const string ErroPermissaoNegada = "O usuário logado não tem permissão para acessar esta aplicação cliente.";

        var semAutorizacoes = !autorizacoes.Any();
        var consentimentoExterno = await _applicationManager.HasConsentTypeAsync(aplicacao, ConsentTypes.External);
        var autorizacaoExternaRequerida = semAutorizacoes && consentimentoExterno;
        if (autorizacaoExternaRequerida)
        {
            var propriedades = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = ErroPermissaoNegada
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

        var ReivindicacaoTipoIgualFuncaoPrincipal = reivindicacao.Type.Equals(principal.GetClaim(Claims.Role));
        var ReivindicacaoFuncaoPrincipalValor = reivindicacao.Type.Equals(Claims.Role) && principal.HasClaim(Claims.Role, reivindicacao.Value);
        var ReivindicacaoCorrespondeFuncaoPrincipal = ReivindicacaoTipoIgualFuncaoPrincipal || ReivindicacaoFuncaoPrincipalValor;
        if (ReivindicacaoCorrespondeFuncaoPrincipal)
        {
            yield return Destinations.AccessToken;
            yield return Destinations.IdentityToken;
        }
    }

    public static List<string> DeterminarDestinosPermitidos(ClaimsIdentity identidade, Claim reivindicacao)
    {
        List<string> destinosPermitidos = [];

        var reivindicacaoEhNome = reivindicacao.Type.Equals(OpenIddictConstants.Claims.Name);
        var reivindicacaoEhEmail = reivindicacao.Type.Equals(OpenIddictConstants.Claims.Email);
        var reivindicacaoCorresponde = reivindicacaoEhNome || reivindicacaoEhEmail;
        if (reivindicacaoCorresponde)
        {
            destinosPermitidos.Add(OpenIddictConstants.Destinations.AccessToken);
        }

        return destinosPermitidos;
    }

    public Dictionary<string, StringValues> ObterParametros(HttpContext httpContext, List<string> excluindo)
    {
        var possuiConteudoNoFormulario = httpContext.Request.HasFormContentType;
        if (possuiConteudoNoFormulario)
        {
            return ObterParametrosDoFormulario(httpContext, excluindo);
        }

        return ObterParametrosDaConsulta(httpContext, excluindo);
    }

    public Dictionary<string, StringValues> ObterParametrosDoFormulario(HttpContext httpContext, List<string> excluindo)
    {
        // Filtrar o conteúdo do formulário para que não tenha as chaves especificadas no excluindo 
        var formularioFiltrado = httpContext.Request.Form
            .Where(v => !excluindo.Contains(v.Key))
            .ToDictionary(v => v.Key, v => v.Value);

        return formularioFiltrado;
    }

    public Dictionary<string, StringValues> ObterParametrosDaConsulta(HttpContext httpContext, List<string> excluindo)
    {
        // Filtrar o conteúdo da consulta para que não tenha as chaves especificadas no excluindo
        var consultaFiltrada = httpContext.Request.Query
            .Where(v => !excluindo.Contains(v.Key))
            .ToDictionary(v => v.Key, v => v.Value);

        return consultaFiltrada;
    }

    public async Task<(SecurityUser usuario, IActionResult resultadoDesafio)> ObterUsuarioOuDesafiarAutenticacaoAsync(ClaimsPrincipal usuarioPrincipal, ControllerBase controllerBase)
    {
        const string ChaveRedirectUri = "RedirectUri";
        const string AcaoAutorizacao = "Authorize";
        const string ControladorAutorizacao = "Authorization";

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
                [ChaveRedirectUri] = controllerBase.Url.Action(AcaoAutorizacao, ControladorAutorizacao)
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

    public string ConstruirUrlRedirecionamento(HttpRequest requisicao, IDictionary<string, StringValues> parametrosOAuth)
    {
        var caminhoBase = requisicao.PathBase;
        var caminhoDaRequisicao = requisicao.Path;
        var parametrosDeConsulta = QueryString.Create(parametrosOAuth);
        var urlRedirecionamento = string.Concat(caminhoBase, caminhoDaRequisicao, parametrosDeConsulta);

        return urlRedirecionamento;
    }

    public string ObterValorReivindicacaoUsuario(ClaimsPrincipal usuario, string nomeClaimConsentimento)
    {
        return usuario.FindFirstValue(nomeClaimConsentimento);
    }

    public async Task<IActionResult> VerificarReivindicacaoConsentimentoERedirecionarAsync(ControllerBase controlador, HttpContext contexto, ClaimsPrincipal usuario, OpenIddictRequest solicitacao, IDictionary<string, StringValues> parametrosOAuth)
    {
        const string NomeConsentimento = "consent";
        const string ValorAcessoPermitido = "Grant";
        const string ParteCaminhoConsentimento = "/Consent?ReturnUrl=";

        var reivindicacaoUsuario = ObterValorReivindicacaoUsuario(usuario, NomeConsentimento);

        if (reivindicacaoUsuario != ValorAcessoPermitido || solicitacao.HasPrompt(Prompts.Consent))
        {
            // Redirecionar para a página de consentimento
            await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var retornoUrl = HttpUtility.UrlEncode(ConstruirUrlRedirecionamento(contexto.Request, parametrosOAuth));
            var urlRedirecionamentoConsentimento = string.Concat(ParteCaminhoConsentimento, retornoUrl);

            return controlador.Redirect(urlRedirecionamentoConsentimento);
        }

        return null;
    }

    public async Task<(string UserId, List<Claim> Reivindicacoes)> ObterIdentidadeUsuarioEReivindicacoesAsync(SecurityUser usuario)
    {
        var userId = await _userManager.GetUserIdAsync(usuario);
        var reivindicacoes = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userId),
            new (Claims.Subject, userId),
            new (Claims.Email, usuario.Email),
            new (Claims.Name, usuario.UserName)
        };

        return (userId, reivindicacoes);
    }
}
