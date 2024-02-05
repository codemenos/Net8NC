namespace Solucao.Infrastructure.Shared.Common;

using System.Collections;

/// <summary>
/// Representa uma resposta genérica para uma operação, incluindo dados, tipo de dados, status de sucesso e mensagens associadas.
/// </summary>
/// <typeparam name="T">Tipo dos dados na resposta.</typeparam>
public class Resposta<T>
{
    private const string OperacaoBemSucedida = "Operação bem-sucedida.";
    private const string Desconhecido = "Desconhecido";

    /// <summary>
    /// Indica se a operação foi bem-sucedida.
    /// </summary>
    public bool Sucesso { get; private set; }

    /// <summary>
    /// Obtém o tipo de dados da operação.
    /// </summary>
    public string TipoDeDados { get; private set; }

    /// <summary>
    /// Obtém os dados da operação.
    /// </summary>
    public T Dados { get; private set; }

    /// <summary>
    /// Obtém a mensagem associada à operação.
    /// </summary>
    public IList<string> Mensagem { get; private set; }


    /// <summary>
    /// Cria uma resposta de operação bem-sucedida.
    /// </summary>
    /// <param name="dados">Dados da operação.</param>
    public Resposta(T dados)
    {
        Sucesso = true;
        TipoDeDados = ObterNomeDoTipo(dados);
        Dados = dados;
        Mensagem = new List<string>() { OperacaoBemSucedida };
    }

    /// <summary>
    /// Cria uma resposta de operação com falha, fornecendo uma mensagem de erro.
    /// </summary>
    /// <param name="mensagemErro">Mensagem de erro.</param>
    public Resposta(string mensagemErro)
    {
        Sucesso = false;
        TipoDeDados = typeof(T).Name;
        Dados = default;
        Mensagem = new List<string>() { mensagemErro };
    }

    /// <summary>
    /// Cria uma resposta de operação com falha, fornecendo uma lista de mensagens de erro.
    /// </summary>
    /// <param name="mensagemErro">Lista de mensagens de erro.</param>
    public Resposta(IList<string> mensagemErro)
    {
        Sucesso = false;
        TipoDeDados = typeof(T).Name;
        Dados = default;
        Mensagem = mensagemErro;
    }

    private static (IEnumerable Enumerable, object Objeto, bool Result) IsEnumerable(object obj) =>
        obj is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext() 
        ? new (enumerable, obj, true) 
        : new (null, obj, false);

    private static string ObterNomeDoTipo(object obj)
    {
        if (obj is not null)
        { 
            var objeto = IsEnumerable(obj);
            if (objeto.Result)
            {
                var tipoDoElemento = objeto.Enumerable.Cast<object>().First().GetType();
                return tipoDoElemento.Name;
            }

            var tipo = obj.GetType();
            return tipo.Name;
        }
    
        return Desconhecido;
    }
}