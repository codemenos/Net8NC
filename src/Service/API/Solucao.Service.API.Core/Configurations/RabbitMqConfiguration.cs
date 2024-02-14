namespace Solucao.Service.API.Core.Configurations;

/// <summary>
/// Configurações para conexão com o RabbitMQ
/// </summary>
public class RabbitMqConfiguration
{
    /// <summary>
    /// Protocolo de comunicação com o RabbitMQ
    /// </summary>
    public string Protocol { get; set; } = "amqp";

    /// <summary>
    /// Nome do host do RabbitMQ
    /// </summary>
    public string HostName { get; set; }

    /// <summary>
    /// Porta de comunicação com o RabbitMQ
    /// </summary>
    public ushort Port { get; set; }

    /// <summary>
    /// VirtualHost do RabbitMQ
    /// </summary>
    public string VirtualHost { get; set; }

    /// <summary>
    /// Nome do usuário para autenticação no RabbitMQ
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Senha para autenticação no RabbitMQ
    /// </summary>
    public string Password { get; set; }
}
