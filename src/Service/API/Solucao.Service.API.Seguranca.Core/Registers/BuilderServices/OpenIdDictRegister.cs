namespace Solucao.Service.API.Seguranca.Core;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Domain.Seguranca.Entities;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using static OpenIddict.Abstractions.OpenIddictConstants;
using SecurityToken = Domain.Seguranca.Aggregates.SecurityToken;

public static class OpenIdDictRegister
{
    public static IServiceCollection AddOpenIdDict(this IServiceCollection services, Type type, IConfiguration configuration)
    {
        //TODO Remover
        var x = configuration.GetConnectionString("DefaultConnection");

        if (type.AssemblyQualifiedName.Contains("Seguranca"))
        {
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<SegurancaContext>()
                        .ReplaceDefaultEntities<SecurityApplication, SecurityAuthorization, SecurityScope, SecurityToken, Guid>();
                })
                .AddServer(options =>
                {
                    options
                        .SetAuthorizationEndpointUris("connect/authorize")
                        .SetLogoutEndpointUris("connect/logout")
                        .SetTokenEndpointUris("connect/token")
                        .SetUserinfoEndpointUris("connect/userinfo")
                        .SetIntrospectionEndpointUris("connect/introspect")
                        .SetVerificationEndpointUris("connect/verify");

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    options.AllowPasswordFlow();
                    options.AllowAuthorizationCodeFlow();
                    options.AllowClientCredentialsFlow();

                    options.AcceptAnonymousClients();

                    options.AddEncryptionKey(new SymmetricSecurityKey(
                        Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // by default tokens are decrypted. If you would like to take a look in the claims - you can disable it
                    //options.DisableAccessTokenEncryption();

                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableStatusCodePagesIntegration();

                    //options.AdicionarManipuladoresDeSolicitacaoDeToken();

                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });
        }

        services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
        {
            //Encaminhe as respostas de login retornadas pelo ponto de extremidade da API de login da Identidade do ASP.NET Core
            //para a pilha de servidor do OpenIddict, para que o OpenIddict possa gerar uma resposta de token com base no principal
            //preparado pela Identidade.
            options.ForwardSignIn = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;

            // Encaminhe as operações de autenticação de token para a pilha de validação do OpenIddict.
            options.ForwardAuthenticate = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.ForwardChallenge = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.ForwardForbid = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        return services;
    }
}