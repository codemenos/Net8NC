namespace Solucao.Service.API.Seguranca.Core.Services;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using Solucao.Domain.Seguranca.Entities;
using Solucao.Infrastructure.Shared.Models;

public class EscopoService : IEscopoService
{
    private readonly IOpenIddictScopeManager _scopeManager;

    public EscopoService(IOpenIddictScopeManager scopeManager)
    {
        _scopeManager = scopeManager;
    }

    public List<string> ObterStrings(string input)
    {
        if (input != null)
        {
            var matches = Regex.Matches(input, "\"(.*?)\"");
            var resultList = matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();

            return resultList;
        }

        return [];
    }

    public async Task<string> CriarAsync(EscopoModel escopo)
    {
        await Task.Delay(0);

        return "";
    }

    public async ValueTask<IEnumerable<EscopoModel>> ObterTodosAsync()
    {
        var escopos = await ObterTodosEscopos();

        return escopos;
    }

    private async Task<List<EscopoModel>> ObterTodosEscopos()
    {
        var escopos = await _scopeManager.ListAsync().ToListAsync();
        var securityScopes = escopos.Select(p => p as SecurityScope)
                                               .Where(p => p != null)
                                               .ToList();

        var scopes = securityScopes.Select(p => new EscopoModel
        {
            Id = p.Id,
            Name = p.Name,
            DisplayName = p.DisplayName
        });

        return scopes.ToList();
    }

    public async Task<SecurityScope> ObterPorNomeAsync(string nome)
    {
        var existente = await _scopeManager.FindByNameAsync(nome);

        return (SecurityScope)existente;
    }

    public async Task<SecurityScope> ObterPorNomesAsync(List<string> nomes)
    {
        var nomesImutable = nomes.ToImmutableArray();
        var enumarables = _scopeManager.FindByNamesAsync(nomesImutable);
        var existente = await enumarables.FirstOrDefaultAsync();

        return (SecurityScope)existente;
    }

    public async Task<SecurityScope> ObterPorIdAsync(Guid id)
    {
        var existente = await _scopeManager.FindByIdAsync(id.ToString());

        return (SecurityScope)existente;
    }

    public async Task SalvarAsync(SecurityScope editado, CancellationToken cancellationToken)
    {
        if (editado.Id.Equals(default))
        {
            await _scopeManager.CreateAsync(editado, cancellationToken);

            await Task.CompletedTask;

            return;
        }

        var existente = await _scopeManager.FindByIdAsync(editado.Id.ToString());
        if (existente is not null)
        {
            var moficado = Modificar(existente, editado, cancellationToken);

            await _scopeManager.UpdateAsync(moficado, cancellationToken);
        }

        await Task.CompletedTask;
    }

    private SecurityScope Modificar(object original, SecurityScope editado, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var modificado = (SecurityScope)original;
            modificado.Name = editado.Name;
            modificado.DisplayName = editado.DisplayName;
            modificado.DisplayNames = editado.DisplayNames;
            modificado.Description = editado.Description;
            modificado.Descriptions = editado.Descriptions;
            modificado.Properties = editado.Properties;
            modificado.Resources = editado.Resources;

            return modificado;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao modificar o escopo de segurança.", ex);
        }
    }

    public async Task RemoverAsync(Guid id)
    {
        var escopo = await ObterPorIdAsync(id);
        if (escopo != null)
        {
            await _scopeManager.DeleteAsync(escopo);
        }

        await Task.CompletedTask;
    }
}
