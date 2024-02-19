namespace Solucao.Service.API.Seguranca.Pages;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Solucao.Domain.Seguranca.Aggregates;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly SignInManager<SecurityUser> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(SignInManager<SecurityUser> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();

        _logger.LogInformation("Desconectado.");
        
        if (returnUrl is not null)
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage();
    }
}
