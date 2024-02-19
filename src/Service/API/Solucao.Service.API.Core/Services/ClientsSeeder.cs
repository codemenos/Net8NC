namespace Solucao.Service.API.Core;

using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using static OpenIddict.Abstractions.OpenIddictConstants;

public class ClientsSeeder
{
    private readonly IServiceProvider _serviceProvider;

    public ClientsSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task AddScopes()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        var apiScope = await manager.FindByNameAsync("api1");

        if (apiScope != null)
        {
            await manager.DeleteAsync(apiScope);
        }

        await manager.CreateAsync(new OpenIddictScopeDescriptor
        {
            DisplayName = "Api scope",
            Name = "api1",
            Resources =
                    {
                        "resource_server_1"
                    }
        });


        var apiSegurancaScope = await manager.FindByNameAsync("API-Seguranca");

        if (apiSegurancaScope != null)
        {
            await manager.DeleteAsync(apiSegurancaScope);
        }

        await manager.CreateAsync(new OpenIddictScopeDescriptor
        {
            DisplayName = "API Seguranca",
            Name = "API-Seguranca",
            Resources =
                {
                    "resource_server_seguranca"
                }
        });

        var apiCadastroScope = await manager.FindByNameAsync("API-Cadastro");

        if (apiCadastroScope != null)
        {
            await manager.DeleteAsync(apiCadastroScope);
        }

        await manager.CreateAsync(new OpenIddictScopeDescriptor
        {
            DisplayName = "API Cadastro",
            Name = "API-Cadastro",
            Resources =
                {
                    "resource_server_cadastro"
                }
        });
    }

    public async Task AddClients()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<SegurancaContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var clientSeguranca = await manager.FindByClientIdAsync("seguranca-client");
        if (clientSeguranca != null)
        {
            await manager.DeleteAsync(clientSeguranca);
        }

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "seguranca-client",
            ClientSecret = "00000000-0000-0000-0000-000000000000",
            ConsentType = ConsentTypes.Explicit,
            DisplayName = "Segurança",
            RedirectUris =
                {
                    new Uri("https://localhost:7000/swagger/oauth2-redirect.html")
                },
            PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:7000/logout")
                },
            Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                   $"{Permissions.Prefixes.Scope}api1"
                }
        });

        var clientCadastro = await manager.FindByClientIdAsync("cadastro-client");
        if (clientCadastro != null)
        {
            await manager.DeleteAsync(clientCadastro);
        }

        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "cadastro-client",
            ClientSecret = "00000000-0000-0000-0000-000000000001",
            ConsentType = ConsentTypes.Explicit,
            DisplayName = "Cadastro",
            RedirectUris =
                {
                    new Uri("https://localhost:8000/swagger/oauth2-redirect.html")
                },
            PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:8000/logout")
                },
            Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                   $"{Permissions.Prefixes.Scope}api1"
                }
        });
    }
}