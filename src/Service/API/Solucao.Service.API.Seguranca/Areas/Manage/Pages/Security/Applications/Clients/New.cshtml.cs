namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages.Security.Applications.Clients;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Service.API.Seguranca.Core.Services;
using Solucao.Service.API.Seguranca.Core.ViewModels;

public class NewModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly IAplicativoClienteService _aplicativoClienteService;

    public NewModel(
        UserManager<SecurityUser> userManager,
        IAplicativoClienteService aplicativoClienteService)
    {
        _userManager = userManager;
        _aplicativoClienteService = aplicativoClienteService;
    }

    [BindProperty]
    public ClientViewModel Client { get; set; }

    public string Username { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    private const string UserNotFoundError = "Não tem dados para o usuário ID: '{0}'.";
    private const string SaveSuccessMessage = "Salvo com sucesso";
    private const string OperationCancelledMessage = "Operação cancelada.";
    private const string SaveErrorMessage = "Erro ao salvar: {0}";
    private const string AuthenticationErrorMessage = "É preciso estar autenticado.";
    private const string IndexPage = "Index";

    private async Task LoadAsync(SecurityUser user)
    {
        Username = await _userManager.GetUserNameAsync(user);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(AuthenticationErrorMessage);
        }

        await LoadAsync(user);

        Client = new ClientViewModel
        {
            ClienteSecretKey = Guid.NewGuid().ToString(),
            JsonWebKeySet = Guid.NewGuid().ToString()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(string.Format(UserNotFoundError, _userManager.GetUserId(User)));
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            
            return Page();
        }

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                var securityApplication = ModeloParaDominio();

                await _aplicativoClienteService.SalvarClienteAsync(securityApplication, cancellationToken);

                StatusMessage = SaveSuccessMessage;
            }
            catch (OperationCanceledException)
            {
                StatusMessage = OperationCancelledMessage;
                
                return Page();
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(SaveErrorMessage, ex.Message);
                
                return Page();
            }
        }

        return RedirectToIndexPage;
    }

    private IActionResult RedirectToIndexPage => RedirectToPage(IndexPage);

    private SecurityApplication ModeloParaDominio()
    {
        var urlsRedirecionamento = JsonConvert.SerializeObject(Client.UrlsRedirecionamento);
        var urlsRedirecionamentoAposLogout = JsonConvert.SerializeObject(Client.UrlsRedirecionamentoAposLogout);

        var securityApplication = new SecurityApplication
        {
            Id = default,
            ClientId = Client.ClienteId,
            DisplayName = Client.ClienteNome,
            DisplayNames = Client.DisplayNames,
            ClientType = Client.TipoCliente,
            ApplicationType = Client.TipoAplicativoCliente,
            ConsentType = Client.TipoConsentimento,
            ClientSecret = Client.ClienteSecretKey,
            JsonWebKeySet = Client.JsonWebKeySet,
            RedirectUris = urlsRedirecionamento,
            PostLogoutRedirectUris = urlsRedirecionamentoAposLogout,
            Permissions = Client.Permissoes,
            Requirements = Client.Requisitos,
            Properties = Client.Propriedades,
            Settings = "[]"
        };

        return securityApplication;
    }
}
