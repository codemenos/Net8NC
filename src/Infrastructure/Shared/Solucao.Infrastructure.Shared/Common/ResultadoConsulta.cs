namespace Solucao.Infrastructure.Shared.Common;

/// <summary>
/// Representa o resultado de uma consulta, incluindo dados, informações de paginação/ordenação e status de sucesso.
/// </summary>
/// <typeparam name="T">Tipo dos dados da consulta.</typeparam>
public class ResultadoConsulta<T>
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida.
    /// </summary>
    public bool ComSucesso { get; private set; }

    /// <summary>
    /// Obtém o tipo de dados da consulta.
    /// </summary>
    public string TipoDeDados { get; private set; }

    /// <summary>
    /// Obtém os dados da consulta.
    /// </summary>
    public T Dados { get; private set; }

    /// <summary>
    /// Obtém ou define informações de paginação/ordenação para os dados consultados.
    /// </summary>
    public DataInfo PaginacaoOrdenacao { get; private set; }

    /// <summary>
    /// Obtém a mensagem de erro em caso de falha na consulta.
    /// </summary>
    public IList<string> MensagemErro { get; private set; } = null;

    private ResultadoConsulta(bool sucesso, string tipoDeDados, T dados, DataInfo paginacaoOrdenacao, string mensagemErro)
    {
        ComSucesso = sucesso;
        TipoDeDados = tipoDeDados;
        Dados = dados;
        PaginacaoOrdenacao = paginacaoOrdenacao;
        MensagemErro = new List<string>() { mensagemErro };
    }

    private ResultadoConsulta(bool sucesso, string tipoDeDados, T dados, DataInfo paginacaoOrdenacao, IList<string> mensagemErro = null)
    {
        ComSucesso = sucesso;
        TipoDeDados = tipoDeDados;
        Dados = dados;
        PaginacaoOrdenacao = paginacaoOrdenacao;
        MensagemErro = mensagemErro;
    }

    /// <summary>
    /// Cria um resultado de consulta bem-sucedido.
    /// </summary>
    /// <param name="dados">Dados da consulta.</param>
    /// <param name="paginacaoOrdenacao">Informações de paginação/ordenação.</param>
    /// <returns>O resultado de consulta bem-sucedido.</returns>
    public static ResultadoConsulta<T> Sucesso(T dados, DataInfo paginacaoOrdenacao)
    {
        var tipoDeDados = typeof(T).Name;
        return new ResultadoConsulta<T>(true, tipoDeDados, dados, paginacaoOrdenacao);
    }

    /// <summary>
    /// Cria um resultado de consulta falho com uma mensagem de erro.
    /// </summary>
    /// <param name="mensagemErro">Mensagem de erro.</param>
    /// <returns>O resultado de consulta falho.</returns>
    public static ResultadoConsulta<T> Falha(string mensagemErro)
    {
        var tipoDeDados = typeof(T).Name;
        return new ResultadoConsulta<T>(false, tipoDeDados, default, null, mensagemErro);
    }

    /// <summary>
    /// Cria um resultado de consulta falho com uma mensagem de erro.
    /// </summary>
    /// <param name="mensagemErro">Mensagem de erro.</param>
    /// <returns>O resultado de consulta falho.</returns>
    public static ResultadoConsulta<T> Falhas(IList<string> mensagemErro)
    {
        var tipoDeDados = typeof(T).Name;
        return new ResultadoConsulta<T>(false, tipoDeDados, default, null, mensagemErro);
    }

}