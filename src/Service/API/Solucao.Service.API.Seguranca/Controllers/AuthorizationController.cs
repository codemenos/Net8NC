namespace Solucao.Service.API.Seguranca.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Service.API.Core.Attributes;
using Solucao.Service.API.Core.Services;

[ApiExplorerSettings(GroupName = "Autorizacao")]
public class AuthorizationController : Controller
{
    private const string APPLICATION_JSON = "application/json";
    private const string ConsentNaming = "consent";
    private const string GrantAccessValue = "Grant";
    private const string DenyAccessValue = "Deny";
    private const string PATH_CONNECT_AUTHORIZE = "~/connect/authorize";
    private const string PATH_CONNECT_AUTHORIZE_ACCEPT = "~/connect/authorize/accept";
    private const string FORM_SUBMIT_ACCEPT = "submit.Accept";
    private const string PATH_CONNECT_AUTHORIZE_DENY = "~/connect/authorize/deny";
    private const string FORM_SUBMIT_DENY = "submit.Deny";
    private const string PATH_CONNECT_LOGOUT_GET = "~/connect/logout";
    private const string PATH_CONNECT_LOGOUT_POST = "~/connect/logout";
    private const string PATH_CONNECT_TOKEN_POST = "~/connect/token";

    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<SecurityUser> _signInManager;
    private readonly UserManager<SecurityUser> _userManager;
    private readonly AutorizacaoService _autorizacaoService;

    /// <summary>
    /// Construtor da classe AuthorizationController.
    /// </summary>
    /// <param name="applicationManager">O gerenciador de aplicações OpenIddict.</param>
    /// <param name="authorizationManager">O gerenciador de autorizações OpenIddict.</param>
    /// <param name="scopeManager">O gerenciador de escopos OpenIddict.</param>
    /// <param name="autorizacaoService"></param>
    /// <param name="signInManager">O gerenciador de logins.</param>
    /// <param name="userManager">O gerenciador de usuários.</param>
    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        AutorizacaoService autorizacaoService,
        SignInManager<SecurityUser> signInManager,
        UserManager<SecurityUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _autorizacaoService = autorizacaoService;
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
    [HttpGet(PATH_CONNECT_AUTHORIZE)]
    [HttpPost(PATH_CONNECT_AUTHORIZE)]
    public async Task<IActionResult> Authorize()
    {
        return await _autorizacaoService.Autorizar(this, HttpContext);
    }

    /// <summary>
    /// Endpoint utilizado para aceitar o consentimento.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado do consentimento.</returns>
    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost(PATH_CONNECT_AUTHORIZE_ACCEPT)]
    [FormValueRequired(FORM_SUBMIT_ACCEPT)]
    public async Task<IActionResult> Accept()
    {
        return await _autorizacaoService.AceitarConsentimento(this, HttpContext);
    }

    /// <summary>
    /// Endpoint utilizado para negar o consentimento.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado da negação do consentimento.</returns>
    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost(PATH_CONNECT_AUTHORIZE_DENY)]
    [FormValueRequired(FORM_SUBMIT_DENY)]
    public IActionResult Deny() 
        => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    /// <summary>
    /// Endpoint utilizado para logout.
    /// </summary>
    /// <returns>Um IActionResult representando o redirecionamento para a página de logout.</returns>
    [HttpGet(PATH_CONNECT_LOGOUT_GET)]
    public async Task<IActionResult> Logout()
    {
        return await _autorizacaoService.EncerrarSessao(this);
    }

    /// <summary>
    /// Endpoint utilizado para realizar o logout.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado do logout.</returns>
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Logout))]
    [HttpPost(PATH_CONNECT_LOGOUT_POST)]
    public async Task<IActionResult> LogoutPost()
    {
        return await _autorizacaoService.EncerrarSessaoPost(this);
    }

    /// <summary>
    /// Endpoint utilizado para trocar tokens.
    /// </summary>
    /// <returns>Um IActionResult representando o resultado da troca de tokens.</returns>
    [IgnoreAntiforgeryToken]
    [Produces(APPLICATION_JSON)]
    [HttpPost(PATH_CONNECT_TOKEN_POST)]
    public async Task<IActionResult> Exchange()
    {
        return await _autorizacaoService.ObterOuTrocaToken(this, HttpContext);
    }
}

