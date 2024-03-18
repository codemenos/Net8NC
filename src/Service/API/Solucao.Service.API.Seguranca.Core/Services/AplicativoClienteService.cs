namespace Solucao.Service.API.Seguranca.Core.Services;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Infrastructure.Shared.Models;

public class AplicativoClienteService : IAplicativoClienteService
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public AplicativoClienteService(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
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

    public async ValueTask<IEnumerable<ClienteModel>> ObterTodosOsClientesAsync()
    {
        var clienteModels = await ObterTodosSecurityApplications();

        return clienteModels;
    }

    public async Task<SecurityApplication> ObterClientePorClientIdAsync(string clientid)
    {
        var existente = await _applicationManager.FindByClientIdAsync(clientid);

        return (SecurityApplication)existente;
    }

    public async Task<SecurityApplication> ObterClientePorIdAsync(Guid id)
    {
        var existente = await _applicationManager.FindByIdAsync(id.ToString());

        return (SecurityApplication)existente;
    }

    public async Task SalvarClienteAsync(SecurityApplication editado, CancellationToken cancellationToken)
    {
        if (editado.Id.Equals(default))
        {
            await _applicationManager.CreateAsync(editado, cancellationToken);
        }
        else
        {
            var existente = await _applicationManager.FindByIdAsync(editado.Id.ToString());
            if (existente is not null)
            {
                var moficado = Modificar(existente, editado, cancellationToken);

                await _applicationManager.UpdateAsync(moficado, cancellationToken);
            }
        }

        await Task.CompletedTask;
    }

    public async Task RemoverClienteAsync(Guid id)
    {
        var cliente = await ObterClientePorIdAsync(id);
        if (cliente != null)
        {
            await _applicationManager.DeleteAsync(cliente);
        }

        await Task.CompletedTask;
    }

    private SecurityApplication Modificar(object original, SecurityApplication editado, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var modificado = (SecurityApplication)original;
            modificado.ClientSecret = editado.ClientSecret;
            modificado.Settings = editado.Settings;
            modificado.Requirements = editado.Requirements;
            modificado.ApplicationType = editado.ApplicationType;
            modificado.ClientId = editado.ClientId;
            modificado.ClientSecret = editado.ClientSecret;
            modificado.ClientType = editado.ClientType;
            modificado.ConsentType = editado.ConsentType;
            modificado.DisplayName = editado.DisplayName;
            modificado.DisplayNames = editado.DisplayNames;
            modificado.JsonWebKeySet = editado.JsonWebKeySet;
            modificado.Permissions = editado.Permissions;
            modificado.PostLogoutRedirectUris = editado.PostLogoutRedirectUris;
            modificado.Properties = editado.Properties;
            modificado.RedirectUris = editado.RedirectUris;
            modificado.Requirements = editado.Requirements;
            modificado.Settings = editado.Settings;

            return modificado;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao modificar o aplicativo de segurança.", ex);
        }
    }

    private async Task<List<ClienteModel>> ObterTodosSecurityApplications()
    {
        var applications = await _applicationManager.ListAsync().ToListAsync();
        var securityApplications = applications.Select(app => app as SecurityApplication)
                                               .Where(app => app != null)
                                               .ToList();

        var clients = securityApplications.Select(app => new ClienteModel
        {
            ClienteId = app.ClientId,
            NomeExibicao = app.DisplayName
        });

        return clients.ToList();
    }

}