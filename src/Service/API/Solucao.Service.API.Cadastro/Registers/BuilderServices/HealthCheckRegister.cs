namespace Solucao.Service.API.Cadastro.Registers;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Solucao.Service.API.Cadastro.Configurations;

public static class HealthCheckRegister
{
    public static IServiceCollection AddAddHealthChecksService(this IServiceCollection services, IConfiguration configuration)
    {
        const string AMQP = "{0}://{1}:{2}/{3}";

        var healthCheckerConnectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionHealthChecker);
        var sqlServerConnectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionName);
        var rabbitMqConfiguration = configuration.GetSection(ConstantSection.RABBITMQ).Get<RabbitMqConfiguration>();
        var rabbitConnectionString = string.Format(AMQP, rabbitMqConfiguration.Protocol, rabbitMqConfiguration.HostName, rabbitMqConfiguration.Port, rabbitMqConfiguration.VirtualHost);
        var redisConfig = configuration.GetSection(ConstantSection.REDIS).Get<RedisConfiguration>();

        services
            .AddHealthChecks()
                .AddSqlServer(
                    connectionString: sqlServerConnectionString,
                    healthQuery: ConstantRegister.HealthCheck.HealthQuery,
                    name: ConstantRegister.HealthCheck.SegurancaContext,
                    failureStatus: HealthStatus.Degraded,
                    tags: new string[]
                    {
                        ConstantRegister.HealthCheck.SQLServer,
                        ConstantRegister.HealthCheck.DataBase
                    })
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
            //.AddInMemoryStorage();

        return services;
    }
}
