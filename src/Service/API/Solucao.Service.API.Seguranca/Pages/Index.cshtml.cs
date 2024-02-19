namespace Solucao.Service.API.Seguranca.Pages;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Solucao.Domain.Seguranca.Aggregates;

public class IndexModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly SignInManager<SecurityUser> _signInManager;

    public IndexModel(UserManager<SecurityUser> userManager, SignInManager<SecurityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string UserName { get; set; }

    public async Task<IActionResult> OnGetAsync()   
    {
        if (!_signInManager.IsSignedIn(User))
        {
            return await RedirectToAuthenticateAsync();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            UserName = user.UserName; 
        }

        return Page();
    }

    private Task<IActionResult> RedirectToAuthenticateAsync()
    {
        return Task.FromResult<IActionResult>(RedirectToPage("/Authenticate", new { returnUrl = Url.Page("/Index") }));
    }
}
