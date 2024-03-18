namespace Solucao.Service.API.Seguranca.Core.Registers.BuilderServices;

using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Solucao.Application.Core.Behaviors;
using Solucao.Application.Seguranca.ProfileMaps;
using Solucao.Infrastructure.Shared.Common;
using Solucao.Service.API.Core.Configurations;
using Solucao.Service.API.Core.Registers;
using Solucao.Service.API.Seguranca.Core.ProfileMaps;
using Solucao.Service.API.Seguranca.Core.Services;


/// <summary>
/// Registros de serviços da aplicação
/// </summary>
public static class ApplicationRegisterServices
{
    /// <summary>
    /// Registra os serviços da aplicação
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void RegisterServices(this IServiceCollection services, Type type, IConfiguration configuration)
    {
        services.AddScoped<IAplicativoClienteService, AplicativoClienteService>();
        services.AddScoped<IEscopoService, EscopoService>();

        services.AddSQLServer(type, configuration);

        services.AddKestrelControl();

        services.AddHttpContextAccessor();

        services.AddControllers();

        if (type.AssemblyQualifiedName.Contains("Seguranca"))
        {
            services.AddOpenIdDict(type, configuration);
            services.AddRazorPages();
            services.AddTransient<AutorizacaoService>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddOpenIddict()
                .AddValidation(options =>
                {
                    options.SetIssuer("https://localhost:7000/");
                    options.SetClientId("seguranca-client");
                    options.AddAudiences("Todos");
                    options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));
                    options.UseSystemNetHttp();
                    options.UseAspNetCore();
                });

            services.AddTransient<CargaPadraoOpenIddictService>();
        }

        services.AddAutoMapper(typeof(ModelViewProfile), typeof(ClienteProfile));

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options => { SwaggerConfiguration.ConfigureSwaggerGen(options, configuration); });

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(type.Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        services.Configure<CachingBehaviorConfiguration>(configuration.GetSection(ConstantSection.CACHINGBEHAVIORCONFIGURATION));

        services.AddHealthChecksService(type, configuration);

        services.AddHangfire(config =>
        {
            const string HangfireAutomaticRetryAttempts = "Hangfire:AutomaticRetryAttempts";

            var connectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionHangFire);
            var automaticRetryAttempts = configuration.GetValue<int>(HangfireAutomaticRetryAttempts);

            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
            config.UseSqlServerStorage(connectionString);
            config.UseFilter(new AutomaticRetryAttribute { Attempts = automaticRetryAttempts });
        });

        services.AddHangfireServer();

        services.AddRin();

        services.AddLogging();
    }

}