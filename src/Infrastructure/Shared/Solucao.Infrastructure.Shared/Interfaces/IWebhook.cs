namespace Solucao.Infrastructure.Shared.Interfaces;

/// <summary>
/// Define uma interface para lidar com Webhooks.
/// </summary>
/// <typeparam name="T">O tipo de evento que será enviado pelo Webhook.</typeparam>
public interface IWebhook<T>
{
    /// <summary>
    /// Adiciona um novo inscrito para receber eventos do Webhook.
    /// </summary>
    /// <param name="webhookUrl">A URL do inscrito a ser adicionado.</param>
    void AdicionarInscrito(string webhookUrl);

    /// <summary>
    /// Remove um inscrito para não receber mais eventos do Webhook.
    /// </summary>
    /// <param name="webhookUrl">A URL do inscrito a ser removido.</param>
    void RemoverInscrito(string webhookUrl);

    /// <summary>
    /// Envia um evento assíncrono para todos os inscritos no Webhook.
    /// </summary>
    /// <param name="evento">O evento a ser enviado.</param>
    /// <returns>Uma tarefa representando o envio do evento.</returns>
    Task EnviarEventoAsync(T evento);
}
