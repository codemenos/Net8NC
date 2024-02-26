namespace Solucao.Service.API.Seguranca.Areas.Identity.Pages.Account;

#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
[AllowAnonymous]
public class LockoutModel : PageModel
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public void OnGet()
    {
    }
}
