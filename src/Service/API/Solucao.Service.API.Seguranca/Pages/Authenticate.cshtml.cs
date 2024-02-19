namespace Solucao.Service.API.Seguranca.Pages;

using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Solucao.Domain.Seguranca.Aggregates;

public class AuthenticateModel : PageModel
{
    private const string INVALID_USER_PASSWORD = "Usuário ou senha inválido.";

    private readonly UserManager<SecurityUser> _userManager;
    private readonly SignInManager<SecurityUser> _signInManager;

    public AuthenticateModel(
        UserManager<SecurityUser> userManager,
        SignInManager<SecurityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public string Email { get; set; }

    [BindProperty]
    public string Password { get; set; }

    [BindProperty]
    public bool RememberMe { get; set; } = true;

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        const string DEFAULT_PATH = "/";
        const string INDEX_PATH = "/Index";

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, INVALID_USER_PASSWORD);
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var decodedReturnUrl = WebUtility.UrlDecode(ReturnUrl);

            var claims = await _userManager.GetClaimsAsync(user);

            var userNameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userNameClaim != null)
            {
                await _userManager.ReplaceClaimAsync(user, userNameClaim, new Claim(ClaimTypes.Name, user.UserName));
            }
            else
            {
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.UserName));
            }

            user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                
                var temAspectoDeRedirecionamento = !string.IsNullOrEmpty(ReturnUrl) && !ReturnUrl.Equals(DEFAULT_PATH);
                return temAspectoDeRedirecionamento ? Redirect(ReturnUrl) : RedirectToPage(INDEX_PATH);
            }
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new
            {
                ReturnUrl,
                RememberMe
            });
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, INVALID_USER_PASSWORD);
        
        return Page();
    }
}

