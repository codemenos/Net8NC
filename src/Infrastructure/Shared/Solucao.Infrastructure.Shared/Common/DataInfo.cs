namespace Solucao.Infrastructure.Shared.Common;

/// <summary>
/// Representa informações de dados, incluindo informações de paginação e ordenação.
/// </summary>
public class DataInfo
{
    /// <summary>
    /// Obtém ou define as informações de paginação.
    /// </summary>
    public PaginacaoInfo Paginacao { get; set; }

    /// <summary>
    /// Obtém ou define a lista de informações de ordenação.
    /// </summary>
    public IList<OrdenacaoInfo> Ordenacao { get; set; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DataInfo"/>.
    /// </summary>
    /// <param name="paginacao">Informações de paginação.</param>
    /// <param name="ordenacao">Lista de informações de ordenação.</param>
    public DataInfo(PaginacaoInfo paginacao, IList<OrdenacaoInfo> ordenacao)
    {
        Paginacao = paginacao;
        Ordenacao = ordenacao;
    }
}
