namespace Solucao.Service.API.Core.Configurations;

/// <summary>
/// Configuração de consumidor do MassTransit
/// </summary>
public class MassTransitConsumerConfiguration
{
    /// <summary>
    /// Nome do consumidor
    /// </summary>
    public string ConsumerName { get; set; }

    /// <summary>
    /// Nome do evento
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// Quantidade de retentativas
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Intervalo entre as retentativas
    /// </summary>
    public int RetryInterval { get; set; }

    /// <summary>
    /// Indica se a fila é durável
    /// </summary>
    public bool Durable { get; set; }
}