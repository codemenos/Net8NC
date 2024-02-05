namespace Solucao.Infrastructure.Shared.Common;

/// <summary>
/// Representa informações de paginação para consulta de dados.
/// </summary>
public class PaginacaoInfo
{
    /// <summary>
    /// Obtém ou define o número da página atual.
    /// </summary>
    public int Pagina { get; set; }

    /// <summary>
    /// Obtém ou define a quantidade de itens por página.
    /// </summary>
    public int TamanhoPagina { get; set; }

    /// <summary>
    /// Obtém ou define o número total de registros.
    /// </summary>
    public int TotalRegistro { get; set; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="PaginacaoInfo"/>.
    /// </summary>
    /// <param name="pagina">Número da página atual.</param>
    /// <param name="tamanhoPagina">Quantidade de itens por página.</param>
    /// <param name="totalRegistro">Número total de registros.</param>
    public PaginacaoInfo(int pagina, int tamanhoPagina, int totalRegistro)
    {
        Pagina = pagina;
        TamanhoPagina = tamanhoPagina;
        TotalRegistro = totalRegistro;
    }
}