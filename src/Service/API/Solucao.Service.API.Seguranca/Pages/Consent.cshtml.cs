namespace Solucao.Service.API.Seguranca.Pages;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Solucao.Domain.Seguranca.Aggregates;

public class ConsentModel : PageModel
{
    public const string CONSENTNAMING = "consent";

    private readonly UserManager<SecurityUser> _userManager;
    private readonly SignInManager<SecurityUser> _signInManager;

    public ConsentModel(UserManager<SecurityUser> userManager, SignInManager<SecurityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public string ReturnUrl { get; set; } = null;

    public IActionResult OnGet(string returnUrl)
    {
        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string grant)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Error");
        }

        // Adiciona a nova reivindicação ao usuário
        var result = await _userManager.AddClaimAsync(user, new Claim(CONSENTNAMING, grant));
        if (!result.Succeeded)
        {
            return RedirectToPage("/Error");
        }

        // atualiza o login
        await _signInManager.RefreshSignInAsync(user);

        return Redirect(ReturnUrl);
    }
}
