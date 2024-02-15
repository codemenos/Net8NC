namespace Solucao.Service.API.Core;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using static OpenIddict.Abstractions.OpenIddictConstants;


public static class OpenIdDictRegister
{
    public static IServiceCollection AddOpenIdDict(this IServiceCollection services, Type type, IConfiguration configuration)
    {
        if (type.AssemblyQualifiedName.Contains("Seguranca"))
        {
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<SegurancaContext>()
                        .ReplaceDefaultEntities<Guid>();
                })
                .AddServer(options =>
                {
                    options.SetAuthorizationEndpointUris("connect/authorize")
                        .SetLogoutEndpointUris("connect/logout")
                        .SetTokenEndpointUris("connect/token");

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    options.AllowAuthorizationCodeFlow();

                    options.AddEncryptionKey(new SymmetricSecurityKey(
                        Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // by default tokens are decrypted. If you would like to take a look in the claims - you can disable it
                    //options.DisableAccessTokenEncryption();

                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableTokenEndpointPassthrough();
                        
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });
        }

        return services;
    }
}
