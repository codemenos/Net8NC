namespace Solucao.Service.API.Core.Registers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Solucao.Infrastructure.Shared.Common;
using Solucao.Service.API.Core.Configurations;

/// <summary>
/// Registros da aplicação para Health Check
/// </summary>
public static class HealthCheckRegister
{
    /// <summary>
    /// Adiciona o serviço de Health Check
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddHealthChecksService(this IServiceCollection services, Type type, IConfiguration configuration)
    {
        const string AMQP = "{0}://{1}:{2}/{3}";
        const string SEGURANCA = "Seguranca";
        const string CADASTRO = "Cadastro";

        var healthCheckerConnectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionHealthChecker);
        var sqlServerSegurancaConnectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionSeguranca);
        var sqlServerCadastroConnectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionCadastro);
        var rabbitMqConfiguration = configuration.GetSection(ConstantSection.RABBITMQ).Get<RabbitMqConfiguration>();
        var rabbitConnectionString = string.Format(AMQP, rabbitMqConfiguration.Protocol, rabbitMqConfiguration.HostName, rabbitMqConfiguration.Port, rabbitMqConfiguration.VirtualHost);
        var redisConfig = configuration.GetSection(ConstantSection.REDIS).Get<RedisConfiguration>();

        if (type.AssemblyQualifiedName.Contains(SEGURANCA))
        {
            services
            .AddHealthChecks()
            .AddSqlServer(
                connectionString: sqlServerSegurancaConnectionString,
                healthQuery: ConstantRegister.HealthCheck.HealthQuery,
                name: ConstantRegister.HealthCheck.SegurancaContext,
                failureStatus: HealthStatus.Degraded,
                tags: new string[]
                {
                        ConstantRegister.HealthCheck.SQLServer,
                        ConstantRegister.HealthCheck.DataBase
                });
        }

        if (type.AssemblyQualifiedName.Contains(CADASTRO))
        {
            services
            .AddHealthChecks()
            .AddSqlServer(
                connectionString: sqlServerCadastroConnectionString,
                healthQuery: ConstantRegister.HealthCheck.HealthQuery,
                name: ConstantRegister.HealthCheck.CadastroContext,
                failureStatus: HealthStatus.Degraded,
                tags: new string[]
                {
                        ConstantRegister.HealthCheck.SQLServer,
                        ConstantRegister.HealthCheck.DataBase
                });
        }

        services
        .AddHealthChecks()
        .AddRabbitMQ(
            rabbitConnectionString: rabbitConnectionString,
            name: ConstantRegister.HealthCheck.RabbitmqBroker,
            tags: new string[] { ConstantRegister.HealthCheck.RabbitMq, ConstantRegister.HealthCheck.Broker })
        .AddRedis(
            redisConnectionString: redisConfig.ConnectionString,
            name: ConstantRegister.HealthCheck.RedisCache,
            failureStatus: HealthStatus.Unhealthy,
            tags: new string[] { ConstantRegister.HealthCheck.Redis, ConstantRegister.HealthCheck.Cache })
        .AddCheck(
            name: ConstantRegister.HealthCheck.Self,
            () => HealthCheckResult.Healthy(),
            tags: new[] { ConstantRegister.HealthCheck.Self });


        services
        .AddHealthChecksUI(setupSettings: setup =>
        {
            const string baseUriFormat = "{0}://{1}:{2}/hc";
            const string payloadTemplate = "{{\"servico\": \"[[LIVENESS]]\", \"uri\": \"{0}\", \"status\": \"[[FAILURE]]\", \"description\": \"[[DESCRIPTION]]\", \"timestamp\": \"[[TIMESTAMP]]\"}}";
            const string restorePayloadTemplate = "{{\"servico\": \"[[LIVENESS]]\", \"uri\": \"{0}\", \"status\": \"recovered\"}}";

            setup.AddHealthCheckEndpoint("SystemCheck", "/hc");
            setup.MaximumHistoryEntriesPerEndpoint(500);
            setup.SetMinimumSecondsBetweenFailureNotifications(4);
            setup.SetEvaluationTimeInSeconds(5);
            setup.SetApiMaxActiveRequests(5);

            setup.SetHeaderText("Dashboard");

            var formattedBaseUri = string.Format(baseUriFormat, "https", "localhost", "20000");

            setup.AddWebhookNotification(
                name: ConstantRegister.HealthCheck.Self,
                uri: "https://localhost:20000/webhook/api_X",
                payload: string.Format(payloadTemplate, formattedBaseUri),
                restorePayload: string.Format(restorePayloadTemplate, formattedBaseUri)
            );
        })
        .AddSqlServerStorage(healthCheckerConnectionString);

        return services;
    }
}
