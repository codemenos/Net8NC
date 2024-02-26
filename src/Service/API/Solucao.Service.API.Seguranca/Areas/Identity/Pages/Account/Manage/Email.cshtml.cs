namespace Solucao.Service.API.Seguranca.Areas.Identity.Pages.Account.Manage;

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Solucao.Domain.Seguranca.Aggregates;

public class EmailModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly SignInManager<SecurityUser> _signInManager;
    private readonly IEmailSender<SecurityUser> _emailSender;

    public EmailModel(
        UserManager<SecurityUser> userManager,
        SignInManager<SecurityUser> signInManager,
        IEmailSender<SecurityUser> emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    public string Email { get; set; }

    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Novo email")]
        public string NewEmail { get; set; }
    }

    private async Task LoadAsync(SecurityUser user)
    {
        var email = await _userManager.GetEmailAsync(user);
        Email = email;

        Input = new InputModel
        {
            NewEmail = email,
        };

        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Não foi possível obter o usuário com o ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Não foi possível obter o usuário com o ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var email = await _userManager.GetEmailAsync(user);
        if (Input.NewEmail != email)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendConfirmationLinkAsync(user,
                Input.NewEmail,
                $"Confirme sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

            StatusMessage = "Link de confirmação para alterar o email enviado. Por favor, verifique seu email.";

            return RedirectToPage();
        }

        StatusMessage = "Seu email não foi alterado.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Não foi possível obter o usuário com o ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);

            return Page();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var email = await _userManager.GetEmailAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId, code },
            protocol: Request.Scheme);
        await _emailSender.SendConfirmationLinkAsync(user, email, "Por favor, confirme sua conta: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

        StatusMessage = "Email de verificação enviado. Por favor, verifique seu email.";

        return RedirectToPage();
    }
}
