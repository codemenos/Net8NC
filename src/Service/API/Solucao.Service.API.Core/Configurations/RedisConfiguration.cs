namespace Solucao.Service.API.Core.Configurations;

/// <summary>
/// Configurações do Redis
/// </summary>
public class RedisConfiguration
{
    /// <summary>
    /// String de conexão com o Redis
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Nome da instância do Redis
    /// </summary>
    public string InstanceName { get; set; }
}