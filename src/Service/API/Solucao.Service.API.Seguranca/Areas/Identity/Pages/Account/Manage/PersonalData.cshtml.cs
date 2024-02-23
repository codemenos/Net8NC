namespace Solucao.Service.API.Seguranca.Areas.Identity.Pages.Account.Manage;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Solucao.Domain.Seguranca.Aggregates;

public class PersonalDataModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly ILogger<PersonalDataModel> _logger;

    public PersonalDataModel(
        UserManager<SecurityUser> userManager,
        ILogger<PersonalDataModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        return Page();
    }
}
