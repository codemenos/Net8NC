namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages.Security.Applications.Clients;

using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Infrastructure.Shared.Models.Module.Seguranca;
using Solucao.Service.API.Seguranca.Core.Services;
using Solucao.Service.API.Seguranca.Core.ViewModels;

public class EditModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly IAplicativoClienteService _aplicativoClienteService;
    private readonly IMapper _mapper;

    public EditModel(
        IMapper mapper,
        UserManager<SecurityUser> userManager,
        IAplicativoClienteService aplicativoClienteService)
    {
        _mapper = mapper;
        _userManager = userManager;
        _aplicativoClienteService = aplicativoClienteService;
    }

    [BindProperty]
    public ClientViewModel Client { get; set; }

    public string ClienteId { get; set; }
    public string Username { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    private const string UserNotFoundError = "Não tem dados para o usuário ID: '{0}'.";
    private const string SaveSuccessMessage = "Salvo com sucesso";
    private const string OperationCancelledMessage = "Operação cancelada.";
    private const string SaveErrorMessage = "Erro ao salvar: {0}";
    private const string AuthenticationErrorMessage = "É preciso estar autenticado.";
    private const string ClientNotFoundError = "Cliente com ID '{0}' não encontrado.";
    private const string IndexPage = "Index";

    private async Task LoadAsync(SecurityUser user)
    {
        Username = await _userManager.GetUserNameAsync(user);
    }

    public async Task<IActionResult> OnGetAsync(string clientId)
    {
        ClienteId = clientId;

        if (string.IsNullOrEmpty(clientId))
        {
            throw new ArgumentNullException(nameof(clientId));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(AuthenticationErrorMessage);
        }

        await LoadAsync(user);

        var securityApplication = await _aplicativoClienteService.ObterClientePorClientIdAsync(clientId);
        if (securityApplication == null)
        {
            return NotFound(string.Format(ClientNotFoundError, clientId));
        }

        //var x = _mapper.Map<IClientViewModel>(securityApplication);

        MapToViewModel(securityApplication);

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
                var x = _mapper.Map<ISecurityApplication>(Client);

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
            Id = Client.Id,
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

    private void MapToViewModel(SecurityApplication securityApplication)
    {
        Client = new ClientViewModel
        {
            Id = securityApplication.Id,
            ClienteId = securityApplication.ClientId,
            ClienteNome = securityApplication.DisplayName ?? string.Empty,
            TipoCliente = securityApplication.ClientType,
            TipoAplicativoCliente = securityApplication.ApplicationType,
            TipoConsentimento = securityApplication.ConsentType,
            ClienteSecretKey = securityApplication.ClientSecret,
            JsonWebKeySet = securityApplication.JsonWebKeySet,
            UrlsRedirecionamento = _aplicativoClienteService.ObterStrings(securityApplication.RedirectUris),
            UrlsRedirecionamentoAposLogout = _aplicativoClienteService.ObterStrings(securityApplication.PostLogoutRedirectUris),
            Permissoes = securityApplication.Permissions ?? string.Empty,
            Requisitos = securityApplication.Requirements ?? string.Empty,
            Propriedades = securityApplication.Properties ?? string.Empty
        };
    }
}
