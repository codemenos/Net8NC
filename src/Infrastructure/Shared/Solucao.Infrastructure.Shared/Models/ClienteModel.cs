namespace Solucao.Infrastructure.Shared.Models;

#nullable enable

public class ClienteModel
{
    public string? TipoAplicativo { get; set; }
    public string? ClienteId { get; set; }
    public string? SegredoCliente { get; set; }
    public string? TipoCliente { get; set; }
    public string? TipoConsentimento { get; set; }
    public string? NomeExibicao { get; set; }
    public string? NomesExibicao { get; set; }
    public string? ConjuntoChavesJsonWeb { get; set; }
    public string? Permissoes { get; set; }
    public string? UrlsRedirecionamentoAposLogout { get; set; }
    public string? Propriedades { get; set; }
    public string? UrlsRedirecionamento { get; set; }
    public string? Requisitos { get; set; }
    public string? Configuracoes { get; set; }
}