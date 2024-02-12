namespace Solucao.Service.API.Core.Registers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solucao.Service.API.Core.Configurations;

/// <summary>
/// Registra o StackExchangeRedis no container de injeção de dependência.
/// </summary>
public static class StackExchangeRedisRegister
{
    /// <summary>
    /// Adiciona o StackExchangeRedis no container de injeção de dependência.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddStackExchangeRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection(ConstantSection.REDIS).Get<RedisConfiguration>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig.ConnectionString;
            options.InstanceName = redisConfig.InstanceName;
        });

        return services;
    }
}