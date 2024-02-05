namespace Solucao.Infrastructure.Shared.Interfaces;

/// <summary>
/// Interface para um serviço de cliente HTTP.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Envia uma solicitação HTTP POST assíncrona para o URI especificado com o conteúdo fornecido.
    /// </summary>
    /// <param name="requestUri">O URI do recurso.</param>
    /// <param name="content">Os dados a serem enviados no corpo da solicitação.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona. O resultado é uma mensagem HTTP.</returns>
    Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
}