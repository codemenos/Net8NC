namespace Solucao.Service.API.Core.Configurations;

/// <summary>
/// Constantes das Sections do appsettings.json
/// </summary>
public static class ConstantSection
{
    /// <summary>
    /// Para o logging
    /// </summary>
    public const string LOGGING = "Logging";

    /// <summary>
    /// Para o RabbitMQ
    /// </summary>
    public const string RABBITMQ = "RabbitMqConfiguration";
    
    /// <summary>
    /// Para o Redis
    /// </summary>
    public const string REDIS = "RedisConfiguration";
    
    /// <summary>
    /// Para o Cors policy
    /// </summary>
    public const string CORSPOLICY = "CorsPolicyConfiguration";
    
    /// <summary>
    /// Para o CachingBehavior do MediatR
    /// </summary>
    public const string CACHINGBEHAVIORCONFIGURATION = "CachingBehaviorConfiguration";
}