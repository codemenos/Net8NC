namespace Solucao.Service.API.Seguranca.Areas.Manage.Pages.Security.Applications.Scopes;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Domain.Seguranca.Entities;
using Solucao.Service.API.Seguranca.Core.Services;
using Solucao.Service.API.Seguranca.ViewModels;

public class EditModel : PageModel
{
    private readonly UserManager<SecurityUser> _userManager;
    private readonly IEscopoService _escopoService;

    public EditModel(
        UserManager<SecurityUser> userManager,
        IEscopoService escopoService)
    {
        _userManager = userManager;
        _escopoService = escopoService;
    }

    [BindProperty]
    public ScopeViewModel Scope { get; set; }

    public Guid ScopeId { get; set; }
    public string Username { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    private const string UserNotFoundError = "Não tem dados para o usuário ID: '{0}'.";
    private const string SaveSuccessMessage = "Salvo com sucesso";
    private const string OperationCancelledMessage = "Operação cancelada.";
    private const string SaveErrorMessage = "Erro ao salvar: {0}";
    private const string AuthenticationErrorMessage = "É preciso estar autenticado.";
    private const string ScopeNotFoundError = "Escopo com ID '{0}' não encontrado.";
    private const string ScopeNullErrorMessage = "Não tem dados para o escopo.";
    private const string IndexPage = "Index";

    private async Task LoadAsync(SecurityUser user)
    {
        Username = await _userManager.GetUserNameAsync(user);
    }

    public async Task<IActionResult> OnGetAsync(Guid id = default, string nome = null)
    {
        ScopeId = id;

        if (id.Equals(default) && string.IsNullOrEmpty(nome))
        {
            throw new ArgumentNullException(nameof(id));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound(AuthenticationErrorMessage);
        }

        await LoadAsync(user);

        await ObterEscopoAsync(id, nome);

        if (Scope is null)
        {
            return NotFound(string.Format(ScopeNotFoundError, id));
        }

        return Page();
    }

    private async Task ObterEscopoAsync(Guid id = default, string nome = null)
    {
        Scope = null;
        SecurityScope escopo = null;

        if (id.Equals(default))
        {
            escopo = await _escopoService.ObterPorNomeAsync(nome);
        }
        else if (!string.IsNullOrEmpty(nome))
        {
            escopo = await _escopoService.ObterPorIdAsync(id);
        }

        if (escopo is not null)
        {
            Scope = new ScopeViewModel
            {
                Id = escopo.Id,
                Nome = escopo.Name,
                NomeExibicao = escopo.DisplayName,
                NomesExibicoes = escopo.DisplayNames,
                Descricao = escopo.Description,
                Descricoes = escopo.Descriptions,
                Propriedades = escopo.Properties,
                Recursos = escopo.Resources
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
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
                var securityScope = ModeloParaDominio();

                await _escopoService.SalvarAsync(securityScope, cancellationToken);

                StatusMessage = SaveSuccessMessage;
            }
            catch (OperationCanceledException)
            {
                StatusMessage = OperationCancelledMessage;
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(SaveErrorMessage, ex.Message);
            }
        }

        return RedirectToIndexPage();
    }

    private IActionResult RedirectToIndexPage()
    {
        return RedirectToPage(IndexPage);
    }

    private SecurityScope ModeloParaDominio()
    {
        if (Scope is null)
        {
            throw new ArgumentNullException(nameof(Scope), ScopeNullErrorMessage);
        }

        var securityScope = new SecurityScope
        {
            Id = Scope.Id,
            Name = Scope.Nome,
            DisplayName = Scope.NomeExibicao,
            DisplayNames = Scope.NomesExibicoes,
            Description = Scope.Descricao,
            Descriptions = Scope.Descricoes,
            Properties = Scope.Propriedades,
            Resources = Scope.Recursos
        };

        return securityScope;
    }
}
