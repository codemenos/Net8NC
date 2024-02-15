namespace Solucao.Service.API.Seguranca.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
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

[ApiExplorerSettings(GroupName = "Autorizacao")]
public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<SecurityUser> _signInManager;
    private readonly UserManager<SecurityUser> _userManager;

    /// <summary>
    /// Construtor da classe AuthorizationController.
    /// </summary>
    /// <param name="applicationManager">O gerenciador de aplicações OpenIddict.</param>
    /// <param name="authorizationManager">O gerenciador de autorizações OpenIddict.</param>
    /// <param name="scopeManager">O gerenciador de escopos OpenIddict.</param>
    /// <param name="signInManager">O gerenciador de logins.</param>
    /// <param name="userManager">O gerenciador de usuários.</param>
    public AuthorizationController(
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
            throw new InvalidOperationException("A solicitação OpenID Connect não pode ser recuperada.");

        // Recupera o principal do usuário armazenado no cookie de autenticação.
        // Se não puder ser extraído, redireciona o usuário para a página de login.
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result is null || !result.Succeeded)
        {
            // Se a aplicação cliente solicitou autenticação sem prompt,
            // retorna um erro indicando que o usuário não está logado.
            if (request.HasPrompt(Prompts.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O usuário não está logado."
                    }));
            }

            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Se prompt=login foi especificado pela aplicação cliente,
        // retorna imediatamente o agente do usuário para a página de login.
        if (request.HasPrompt(Prompts.Login))
        {
            // Para evitar redirecionamentos infinitos entre login -> autorização,
            // a flag prompt=login é removida da carga da solicitação de autorização antes de redirecionar o usuário.
            var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

            var parameters = Request.HasFormContentType ?
                Request.Form.Where(parameter =>
                {
                    return parameter.Key != Parameters.Prompt;
                }).ToList() :
                Request.Query.Where(parameter =>
                {
                    return parameter.Key != Parameters.Prompt;
                }).ToList();

            parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                });
        }

        // Se um parâmetro max_age foi fornecido, garante que o cookie não seja muito antigo.
        // Se for muito antigo, redireciona automaticamente o agente do usuário para a página de login.
        if (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
            DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))
        {
            if (request.HasPrompt(Prompts.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "O usuário não está logado."
                    }));
            }

            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Recupera o perfil do usuário logado.
        var user = await _userManager.GetUserAsync(result.Principal) ??
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

        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            // Se o consentimento for externo (por exemplo, quando as autorizações são concedidas por um sysadmin),
            // retorna imediatamente um erro se nenhuma autorização for encontrada no banco de dados.
            case ConsentTypes.External when !authorizations.Any():
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "O usuário logado não tem permissão para acessar esta aplicação cliente."
                    }));

            // Se o consentimento for implícito ou se uma autorização for encontrada,
            // retorna uma resposta de autorização sem exibir o formulário de consentimento.
            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Any():
            case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
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

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Neste ponto, nenhuma autorização foi encontrada no banco de dados e um erro deve ser retornado
            // se a aplicação cliente especificou prompt=none na solicitação de autorização.
            case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
            case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "O consentimento do usuário é necessário."
                    }));

            // Em todos os outros casos, renderiza o formulário de consentimento.
            default:

                // TODO: neste momento, o exemplo não possui nenhuma página de consentimento...
                throw new NotImplementedException("Tela de consentimento ainda não implementada!");

                // return View(new AuthorizeViewModel
                // {
                //   ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
                //   Scope = request.Scope
                // });
        }
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

            // Valida os parâmetros de nome de usuário/senha e garante que a conta não esteja bloqueada.
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

            // Observação: neste exemplo, os escopos concedidos correspondem aos escopos solicitados
            // mas você pode querer permitir que o usuário desmarque escopos específicos.
            // Para isso, basta restringir a lista de escopos antes de chamar SetScopes.
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            // Retornar um SignInResult solicitará ao OpenIddict que emita os tokens de acesso/identidade apropriados.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        else if (request.IsAuthorizationCodeGrantType() || request.IsDeviceCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Recupera o principal de reivindicações armazenado no código de autorização/dispositivo/código de atualização.
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            // Recupera o perfil de usuário correspondente ao código de autorização/atualização.
            // Observação: se você deseja invalidar automaticamente o código de autorização/atualização
            // quando a senha/funções do usuário forem alteradas, use a seguinte linha em vez disso:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
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

            // Garante que o usuário esteja autorizado a trocar tokens.
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

            // Garante que o usuário ainda esteja permitido a trocar tokens.
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

            // Cria um novo principal contendo a identidade do usuário.
            principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Observação: neste exemplo, os escopos concedidos correspondem aos escopos solicitados
            // mas você pode querer permitir que o usuário desmarque escopos específicos.
            // Para isso, basta restringir a lista de escopos antes de chamar SetScopes.
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

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

