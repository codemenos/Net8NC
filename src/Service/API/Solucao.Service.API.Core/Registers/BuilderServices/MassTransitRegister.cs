namespace Solucao.Service.API.Core.Registers;

using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Solucao.Application.Core;
using Solucao.Service.API.Core.Configurations;

/// <summary>
/// Registra o MassTransit na aplicação
/// </summary>
public static class MassTransitRegister
{
    private const string LOCALAPIPRINCIPAL = "_CadastroApi";
    private const string CONFIGURATIONCONSUMERS = "RabbitMqConfiguration:Consumers";

    /// <summary>
    /// Adiciona o MassTransit na aplicação
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
    {

        var rabbitMqConfiguration = configuration.GetSection(ConstantSection.RABBITMQ).Get<RabbitMqConfiguration>();
        var consumerConfigs = configuration.GetSection(CONFIGURATIONCONSUMERS).Get<List<MassTransitConsumerConfiguration>>();

        services.AddMassTransit(cfgBus =>
        {
            cfgBus.AddLogging(cfgLog =>
            {
                cfgLog.AddConfiguration(configuration);
                cfgLog.AddConsole();
            });

            cfgBus.UsingRabbitMq((context, cfgRabbit) =>
            {
                cfgRabbit.Host(new Uri($"{rabbitMqConfiguration.Protocol}://{rabbitMqConfiguration.HostName}:{rabbitMqConfiguration.Port}/{rabbitMqConfiguration.VirtualHost}"), h =>
                {
                    h.Username(rabbitMqConfiguration.UserName);
                    h.Password(rabbitMqConfiguration.Password);
                    if (rabbitMqConfiguration.Protocol.ToUpper().Equals("AMQPS"))
                    {
                        h.UseSsl(ssl =>
                        {
                            ssl.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                        });
                    }
                    h.Heartbeat(10);
                });

                cfgRabbit.ExchangeType = ExchangeType.Fanout;

                foreach (var consumerConfig in consumerConfigs)
                {
                    var assembly = Assembly.Load("Solucao.Application.Cadastro");
                    var consumerType = assembly.GetTypes().FirstOrDefault(t => t.Name == consumerConfig.ConsumerName);
                    if (consumerType is not null)
                    {
                        ConfigureReceiveEndpoint(cfgRabbit, context, LOCALAPIPRINCIPAL, consumerConfig.EventName, consumerConfig.RetryCount, consumerConfig.RetryInterval, consumerConfig.Durable, consumerType);
                    }
                }
            });

        });

        return services;
    }

    private static void ConfigureReceiveEndpoint(
    IRabbitMqBusFactoryConfigurator configurator,
    IRegistrationContext context,
    string eventLocal,
    string eventName,
    int retryCount,
    int interval,
    bool durable,
    Type consumerType)
    {
        var method = typeof(MassTransitRegister)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == nameof(ConfigureReceiveEndpoint) && m.IsGenericMethod);

        if (method != null)
        {
            var genericMethod = method.MakeGenericMethod(consumerType);
            var messageRetry = (retryCount, interval); // Cria uma tupla (ValueTuple) a partir dos valores inteiros

            genericMethod?.Invoke(null, new object[] { configurator, context, eventLocal, eventName, messageRetry, durable });
        }
    }

    private static void ConfigureReceiveEndpoint<TConsumer>(IRabbitMqBusFactoryConfigurator configurator, IRegistrationContext context, string eventLocal, string eventName, (int retryCount, int interval)? messageRetry = null, bool durable = true)
    where TConsumer : class, IConsumer
    {
        var (retryCount, interval) = messageRetry ?? (2, 1000);

        configurator.ReceiveEndpoint($"{eventName}{eventLocal}", ep =>
        {
            ep.UseMessageRetry(x => x.Interval(retryCount, interval));
            ep.Durable = durable;
            ep.Bind(eventName);
            ep.Consumer<TConsumer>(context);
            ep.Consumer<PublishErrorHandler>(context);
        });
    }
}