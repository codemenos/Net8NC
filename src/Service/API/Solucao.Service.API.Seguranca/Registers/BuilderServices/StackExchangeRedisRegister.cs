namespace Solucao.Service.API.Seguranca.Registers;

using Solucao.Service.API.Seguranca.Configurations;


public static class StackExchangeRedisRegister
{
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