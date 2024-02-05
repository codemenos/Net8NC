namespace Solucao.Infrastructure.Shared.Common;

/// <summary>
/// Representa o resultado de uma operação, incluindo dados, tipo de dados e status de sucesso.
/// </summary>
/// <typeparam name="T">Tipo dos dados da operação.</typeparam>
public class ResultadoOperacao<T>
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida.
    /// </summary>
    public bool ComSucesso { get; private set; }

    /// <summary>
    /// Obtém o tipo de dados da operação.
    /// </summary>
    public string TipoDeDados { get; private set; }

    /// <summary>
    /// Obtém os dados da operação.
    /// </summary>
    public T Dados { get; private set; }

    /// <summary>
    /// Obtém a mensagem de erro em caso de falha na operação.
    /// </summary>
    public IList<string> MensagemErro { get; private set; } = null;

    private ResultadoOperacao(bool sucesso, string tipoDeDaos, T dados, string mensagemErro)
    {
        ComSucesso = sucesso;
        TipoDeDados = tipoDeDaos;
        Dados = dados;
        MensagemErro = new List<string> { mensagemErro };
    }

    private ResultadoOperacao(bool sucesso, string tipoDeDaos, T dados, IList<string> mensagemErro = null)
    {
        ComSucesso = sucesso;
        TipoDeDados = tipoDeDaos;
        Dados = dados;
        MensagemErro = mensagemErro;
    }

    /// <summary>
    /// Cria um resultado de operação bem-sucedido.
    /// </summary>
    /// <param name="dados">Dados da operação.</param>
    /// <returns>O resultado de operação bem-sucedido.</returns>
    public static ResultadoOperacao<T> Sucesso(T dados)
    {
        var tipoDeDados = typeof(T).Name;
        return new ResultadoOperacao<T>(true, tipoDeDados, dados);
    }

    /// <summary>
    /// Cria um resultado de operação falho com uma mensagem de erro.
    /// </summary>
    /// <param name="mensagemErro">Mensagem de erro.</param>
    /// <returns>O resultado de operação falho.</returns>
    public static ResultadoOperacao<T> Falha(string mensagemErro)
    {
        var tipoDeDados = typeof(T).Name;
        return new ResultadoOperacao<T>(false, tipoDeDados, default, mensagemErro);
    }

    /// <summary>
    /// Cria um resultado de operação falho com uma mensagem de erro.
    /// </summary>
    /// <param name="mensagemErro">Lista de mensagens de erros.</param>
    /// <returns>O resultado de operação falho.</returns>
    public static ResultadoOperacao<T> Falhas(IList<string> mensagemErro)
    {
        var tipoDeDados = typeof(T).Name;
        return new ResultadoOperacao<T>(false, tipoDeDados, default, mensagemErro);
    }
}