namespace Solucao.Service.API.Cadastro.Registers;

using AutoMapper;
using Hangfire;
using MediatR;
using Solucao.Application.Core.Behaviors;
using Solucao.Service.API.Cadastro.Configurations;

public static class ApplicationRegisterServices
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSQLServer(configuration);

        services.AddKestrelControl();

        services.AddHttpContextAccessor();

        services.AddControllers();
        
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options => { SwaggerConfiguration.ConfigureSwaggerGen(options, configuration); });

        var mappingProfiles = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract);

        //services.AddAutoMapper(mappingProfiles.ToArray());

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        services.Configure<CachingBehaviorConfiguration>(configuration.GetSection(ConstantSection.CACHINGBEHAVIORCONFIGURATION));

        services.AddAddHealthChecksService(configuration);

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
