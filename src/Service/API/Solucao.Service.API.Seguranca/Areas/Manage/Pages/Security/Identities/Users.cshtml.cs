namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages.Security.Identities;

#nullable disable

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Solucao.Domain.Seguranca.Aggregates;

public class UsersModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly SignInManager<SecurityUser> _signInManager;

    public UsersModel(
        UserManager<SecurityUser> userManager,
        SignInManager<SecurityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string Username { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }


    public class InputModel
    {

    }

    private async Task LoadAsync(SecurityUser user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        
        Username = userName;

        Input = new InputModel {};
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Não tem dados para o usuário ID: '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Não tem dados para o usuário ID: '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            
            return Page();
        }

        StatusMessage = "Atualizado";

        return RedirectToPage();
    }
}
