namespace Solucao.Service.API.Core.Registers;

using AutoMapper;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solucao.Application.Core.Behaviors;
using Solucao.Infrastructure.Shared.Common;
using Solucao.Service.API.Core.Configurations;
using Solucao.Service.API.Core.Services;


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
        services.AddSQLServer(type, configuration);

        services.AddKestrelControl();

        services.AddHttpContextAccessor();

        services.AddControllers();

        if (type.AssemblyQualifiedName.Contains("Seguranca"))
        {
            services.AddOpenIdDict(type, configuration);
            services.AddRazorPages();
            services.AddTransient<AuthorizationService>();

            services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(c =>
            {
                c.LoginPath = "/Authenticate";
            });

            services.AddTransient<ClientsSeeder>();
        }
        
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options => { SwaggerConfiguration.ConfigureSwaggerGen(options, configuration); });

        var mappingProfiles = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract);

        //services.AddAutoMapper(mappingProfiles.ToArray());

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(type.Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        services.Configure<CachingBehaviorConfiguration>(configuration.GetSection(ConstantSection.CACHINGBEHAVIORCONFIGURATION));

        services.AddAddHealthChecksService(type, configuration);

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
