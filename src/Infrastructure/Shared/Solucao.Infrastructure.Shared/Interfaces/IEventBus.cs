namespace Solucao.Infrastructure.Shared.Interfaces;

/// <summary>
/// Interface para um barramento de eventos.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica uma mensagem do tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser publicada.</typeparam>
    /// <param name="message">A mensagem a ser publicada.</param>
    /// <returns>Uma tarefa que representa a operação de publicação.</returns>
    Task Publish<T>(T message) where T : class;

    /// <summary>
    /// Publica um intervalo de mensagens.
    /// </summary>
    /// <param name="messages">Coleção de mensagens a serem publicadas.</param>
    /// <returns>Uma tarefa que representa a operação de publicação.</returns>
    Task PublishRange(IEnumerable<IEvent> messages);
}