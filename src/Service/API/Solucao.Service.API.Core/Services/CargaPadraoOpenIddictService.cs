namespace Solucao.Service.API.Core;

using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using System;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;

public class CargaPadraoOpenIddictService
{
    private readonly IServiceProvider _serviceProvider;

    public CargaPadraoOpenIddictService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task AdicionarEscoposDeAutorizacao(bool requerExclusaoDoLegado = false)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        await CriarOuAtualizarEscopo(manager, "API-Seguranca", "API Segurança - Acesso Total", ["Todos"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Seguranca-Autenticacao", "API Segurança - Autenticação", ["Todos"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Seguranca-Identidade", "API Segurança - Identidade", ["Todos"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Cadastro", "API Cadastro - Acesso Total", ["Todos"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Cadastro-Pessoa", "API Cadastro - Pessoas", ["Pessoa", "PessoaFisica", "PessoaJuridica"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Cadastro-Usuario", "API Cadastro - Usuarios", ["Usuario", "Pessoa", "PessoaFisica", "PessoaJuridica"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Cadastro-Funcionario", "API Cadastro - Funcionarios", ["Funcionario", "Usuario", "Pessoa", "PessoaFisica"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Cadastro-Fornecedor", "API Cadastro - Fornecedores", ["Fornecedor", "Usuario", "Pessoa", "PessoaFisica", "PessoaJuridica"], requerExclusaoDoLegado);
        await CriarOuAtualizarEscopo(manager, "API-Cadastro-Cliente", "API Cadastro - Clientes", ["Cliente", "Usuario", "Pessoa", "PessoaFisica", "PessoaJuridica"], requerExclusaoDoLegado);
    }

    public async Task AdicionarClientesDeAutenticacao(bool requerExclusaoDoLegado = false)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SegurancaContext>();
        await context.Database.EnsureCreatedAsync();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        List<string> Escopos_Seguranca = ["API-Seguranca", "API-Seguranca-Autenticacao", "API-Seguranca-Identidade"];
        List<string> Escopos_Cadastro = ["API-Cadastro", "API-Cadastro-Pessoa", "API-Cadastro-Usuario", "API-Cadastro-Funcionario", "API-Cadastro-Fornecedor", "API-Cadastro-Cliente"];

        await CriarOuAtualizarCliente(manager, "seguranca-client", "00000000-0000-0000-0000-000000000000", "Segurança", "https://localhost:7000/swagger/oauth2-redirect.html", "https://localhost:7000/logout", Escopos_Seguranca, requerExclusaoDoLegado);
        await CriarOuAtualizarCliente(manager, "cadastro-client", "00000000-0000-0000-0000-000000000001", "Cadastro", "https://localhost:8000/swagger/oauth2-redirect.html", "https://localhost:8000/logout", Escopos_Cadastro, requerExclusaoDoLegado);
    }

    private async Task CriarOuAtualizarEscopo(IOpenIddictScopeManager manager, string nome, string displayName, List<string> resources, bool requerExclusao)
    {
        if (requerExclusao)
        {
            var escopoRemover = await manager.FindByNameAsync(nome);
            if (escopoRemover is not null)
            {
                await manager.DeleteAsync(escopoRemover);
            }
        }

        var escopo = await manager.FindByNameAsync(nome);
        if (escopo is null)
        {
            var descriptor = new OpenIddictScopeDescriptor
            {
                DisplayName = displayName,
                Name = nome
            };

            foreach (var resource in resources)
            {
                descriptor.Resources.Add(resource);
            }

            await manager.CreateAsync(descriptor);
        }
    }

    private async Task CriarOuAtualizarCliente(IOpenIddictApplicationManager manager, string clientId, string clientSecret, string displayName, string redirectUri, string postLogoutRedirectUri, List<string> apiScopes, bool requerExclusao)
    {
        if (requerExclusao)
        {
            var clienteRemover = await manager.FindByClientIdAsync(clientId);
            if (clienteRemover is not null)
            {
                await manager.DeleteAsync(clienteRemover);
            }
        }

        var cliente = await manager.FindByClientIdAsync(clientId);
        if (cliente is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ConsentType = ConsentTypes.Explicit,
                DisplayName = displayName,
                RedirectUris = { new Uri(redirectUri) },
                PostLogoutRedirectUris = { new Uri(postLogoutRedirectUri) },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                }
            };

            foreach (var scope in apiScopes)
            {
                descriptor.Permissions.Add($"{Permissions.Prefixes.Scope}{scope}");
            }

            await manager.CreateAsync(descriptor);
        }
    }
}
