namespace Solucao.Service.API.Seguranca.Controllers;

using System.Security.Claims;
using IdentityModel.Client;
using System.Web;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Infrastructure.Shared.Common;
using Solucao.Service.API.Core.Attributes;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Solucao.Service.API.Core.Services;
using Microsoft.IdentityModel.Tokens;
using k8s.KubeConfigModels;

[ApiExplorerSettings(GroupName = "Autorizacao")]
public class AuthorizationController : Controller
{
    private const string ConsentNaming = "consent";
    private const string GrantAccessValue = "Grant";
    private const string DenyAccessValue = "Deny";

    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<SecurityUser> _signInManager;
    private readonly UserManager<SecurityUser> _userManager;
    private readonly AuthorizationService _authorizationService;

    /// <summary>
    /// Construtor da classe AuthorizationController.
    /// </summary>
    /// <param name="applicationManager">O gerenciador de aplicações OpenIddict.</param>
    /// <param name="authorizationManager">O gerenciador de autorizações OpenIddict.</param>
    /// <param name="scopeManager">O gerenciador de escopos OpenIddict.</param>
    /// <param name="authorizationService"></param>
    /// <param name="signInManager">O gerenciador de logins.</param>
    /// <param name="userManager">O gerenciador de usuários.</param>
    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        AuthorizationService authorizationService,
        SignInManager<SecurityUser> signInManager,
        UserManager<SecurityUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _authorizationService = authorizationService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Endpoint utilizado para iniciar o processo de autorização.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado da autorização.</returns>
    /// <exception cref="InvalidOperationException">Exceção lançada quando a solicitação OpenID Connect não pode ser recuperada.</exception>
    /// <exception cref="NotImplementedException">Exceção lançada quando uma funcionalidade ainda não foi implementada.</exception>
    [IgnoreAntiforgeryToken]
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                          throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        if (await _applicationManager.GetConsentTypeAsync(application) != ConsentTypes.Explicit)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "Only clients with explicit consent type are allowed."
                }));
        }

        var parameters = _authorizationService.ParseOAuthParameters(HttpContext, new List<string> { OpenIddictConstants.Parameters.Prompt });

        // Obter o usuário atualmente autenticado
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // Se o usuário não estiver autenticado, desafie a autenticação
            return Challenge(properties: new AuthenticationProperties
            {
                RedirectUri = _authorizationService.BuildRedirectUrl(HttpContext.Request, parameters)
            }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
        }

        if (request.HasPrompt(Prompts.Login))
        {
            // Fazer logoff se o prompt for de login
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Challenge(properties: new AuthenticationProperties
            {
                RedirectUri = _authorizationService.BuildRedirectUrl(HttpContext.Request, parameters)
            }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
        }

        var consentClaim = User.FindFirstValue(ConsentNaming);

        // it might be extended in a way that consent claim will contain list of allowed client ids.
        if (consentClaim != GrantAccessValue || request.HasPrompt(Prompts.Consent))
        {
            // Redirecionar para a página de consentimento
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var returnUrl = HttpUtility.UrlEncode(_authorizationService.BuildRedirectUrl(HttpContext.Request, parameters));
            var consentRedirectUrl = $"/Consent?returnUrl={returnUrl}";
            return Redirect(consentRedirectUrl);
        }

        // Criar a identidade do usuário
        var userId = await _userManager.GetUserIdAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(Claims.Subject, userId),
            new Claim(Claims.Email, user.Email),
            new Claim(Claims.Name, user.UserName)
        };

        var identity = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType, Claims.Email, Claims.Role);

        // Adicionar os escopos e recursos à identidade
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity.SetDestinations(c => AuthorizationService.GetDestinations(identity, c));

        // Entrar com o usuário
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }


    /// <summary>
    /// Endpoint utilizado para aceitar o consentimento.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado do consentimento.</returns>
    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("~/connect/authorize/accept")]
    [FormValueRequired("submit.Accept")]
    public async Task<IActionResult> Accept()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("A solicitação OpenID Connect não pode ser recuperada.");

        // Recupera o perfil do usuário logado.
        var user = await _userManager.GetUserAsync(User) ??
            throw new InvalidOperationException("Os detalhes do usuário não podem ser recuperados.");

        // Recupera os detalhes da aplicação do banco de dados.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
            throw new InvalidOperationException("Detalhes da aplicação cliente chamadora não podem ser encontrados.");

        // Recupera as autorizações permanentes associadas ao usuário e à aplicação cliente chamadora.
        var authorizations = await _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        // Observação: a mesma verificação já é feita na outra ação, mas é repetida
        // aqui para garantir que um usuário mal-intencionado não possa abusar deste endpoint apenas POST e
        // forçá-lo a retornar uma resposta válida sem a autorização externa.
        if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "O usuário logado não tem permissão para acessar esta aplicação cliente."
                }));
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        // Observação: neste exemplo, os escopos concedidos correspondem aos escopos solicitados
        // mas você pode querer permitir que o usuário desmarque escopos específicos.
        // Para isso, basta restringir a lista de escopos antes de chamar SetScopes.
        principal.SetScopes(request.GetScopes());
        principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

        // Cria automaticamente uma autorização permanente para evitar exigir consentimento explícito
        // para futuras solicitações de autorização ou token contendo os mesmos escopos.
        var authorization = authorizations.LastOrDefault();
        authorization ??= await _authorizationManager.CreateAsync(
              principal: principal,
              subject: await _userManager.GetUserIdAsync(user),
              client: await _applicationManager.GetIdAsync(application),
              type: AuthorizationTypes.Permanent,
              scopes: principal.GetScopes());

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        // Retornar um SignInResult solicitará ao OpenIddict que emita os tokens de acesso/identidade apropriados.
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Endpoint utilizado para negar o consentimento.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado da negação do consentimento.</returns>
    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("~/connect/authorize/deny")]
    [FormValueRequired("submit.Deny")]
    public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    /// <summary>
    /// Endpoint utilizado para logout.
    /// </summary>
    /// <returns>Um IActionResult representando o redirecionamento para a página de logout.</returns>
    [HttpGet("~/connect/logout")]
    public IActionResult Logout()
    {
        return RedirectToPage("/Account/Logout", new
        {
            logoutId = Request.Query["id_token_hint"],
            redirectUri = Request.Query["post_logout_redirect_uri"]
        });
    }

    /// <summary>
    /// Endpoint utilizado para realizar o logout.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado do logout.</returns>
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Logout))]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    /// <summary>
    /// Endpoint utilizado para trocar tokens.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado da troca de tokens.</returns>
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("A solicitação OpenID Connect não pode ser recuperada.");

        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user is null)
            {
                return Forbid(
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
                return Forbid(
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
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            // Retornar um SignInResult solicitará ao OpenIddict que emita os tokens de acesso/identidade apropriados.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        else if (request.IsAuthorizationCodeGrantType() || request.IsDeviceCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            var user = await _userManager.GetUserAsync(principal);
            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O usuário associado ao código de autorização/dispositivo/código de atualização não pôde ser encontrado."
                    }));
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O usuário não pode trocar tokens."
                    }));
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
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
                return Forbid(
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
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            // Retornar um SignInResult solicitará ao OpenIddict que emita os tokens de acesso/identidade apropriados.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("Tipo de concessão não suportado.");
    }

    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        yield return Destinations.AccessToken;

        if ((claim.Type == principal.GetClaim(Claims.Role)) ||
            (claim.Type == Claims.Role && principal.HasClaim(Claims.Role, claim.Value)))
        {
            yield return Destinations.AccessToken;
            yield return Destinations.IdentityToken;
        }
    }
}

