namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages.Security.Applications.Scopes;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Domain.Seguranca.Entities;
using Solucao.Service.API.Seguranca.Core.Services;
using Solucao.Service.API.Seguranca.ViewModels;

public class NewModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly IEscopoService _aplicativoEscopoService;

    public NewModel(UserManager<SecurityUser> userManager, IEscopoService aplicativoEscopoService)
    {
        _userManager = userManager;
        _aplicativoEscopoService = aplicativoEscopoService;
    }

    [BindProperty]
    public ScopeViewModel Scope { get; set; }

    public string Username { get; private set; }

    public string StatusMessage { get; set; }

    private const string SaveSuccessMessage = "Salvo com sucesso";
    private const string OperationCancelledMessage = "Operação cancelada.";
    private const string SaveErrorMessage = "Erro ao salvar: {0}";
    private const string AuthenticationErrorMessage = "É preciso estar autenticado.";
    private const string ScopeNullErrorMessage = "Não tem dados para o escopo.";
    private const string IndexPage = "Index";

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound(AuthenticationErrorMessage);

        await LoadUsernameAsync(user);
        Scope = new ScopeViewModel();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound(AuthenticationErrorMessage);

        if (!ModelState.IsValid)
            return await HandleInvalidModelStateAsync(user);

        try
        {
            await SaveDataAsync();
            StatusMessage = SaveSuccessMessage;

            return RedirectToIndexPage();
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

    private async Task LoadUsernameAsync(SecurityUser user)
    {
        Username = await _userManager.GetUserNameAsync(user);
    }

    private async Task<IActionResult> HandleInvalidModelStateAsync(SecurityUser user)
    {
        await LoadUsernameAsync(user);
        return Page();
    }

    private async Task SaveDataAsync()
    {
        var securityScope = MapViewModelToDomain();
        await _aplicativoEscopoService.SalvarAsync(securityScope, CancellationToken.None);
    }

    private IActionResult RedirectToIndexPage()
    {
        return RedirectToPage(IndexPage);
    }

    private SecurityScope MapViewModelToDomain()
    {
        if (Scope == null)
            throw new ArgumentNullException(nameof(Scope), ScopeNullErrorMessage);

        return new SecurityScope
        {
            Id = default,
            Name = Scope.Nome,
            DisplayName = Scope.NomeExibicao,
            DisplayNames = Scope.NomesExibicoes,
            Description = Scope.Descricao,
            Descriptions = Scope.Descricoes,
            Properties = Scope.Propriedades,
            Resources = Scope.Recursos
        };
    }
}
