namespace Solucao.Infrastructure.Shared.Common;

/// <summary>
/// Representa informações de ordenação para consulta de dados.
/// </summary>
public class OrdenacaoInfo
{
    /// <summary>
    /// Obtém ou define o nome do campo utilizado para ordenação.
    /// </summary>
    public string Campo { get; set; }

    /// <summary>
    /// Obtém ou define se a ordenação é ascendente ou descendente.
    /// </summary>
    public bool Ascendente { get; set; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="OrdenacaoInfo"/>.
    /// </summary>
    /// <param name="campo">Nome do campo utilizado para ordenação.</param>
    /// <param name="ascendente">Indica se a ordenação é ascendente (true) ou descendente (false).</param>
    public OrdenacaoInfo(string campo, bool ascendente)
    {
        Campo = campo;
        Ascendente = ascendente;
    }
}
