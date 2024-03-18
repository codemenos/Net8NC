namespace Solucao.Service.API.Seguranca.Core.ViewModels;

public interface IClientViewModel
{
    Guid Id { get; set; }

    string ClienteId { get; set; }

    string ClienteNome { get; set; }

    string TipoAplicativoCliente { get; set; }

    string TipoCliente { get; set; }

    string TipoConsentimento { get; set; }

    string ClienteSecretKey { get; set; }

    string JsonWebKeySet { get; set; }

    List<string> UrlsRedirecionamento { get; set; }

    List<string> UrlsRedirecionamentoAposLogout { get; set; }

    string DisplayNames { get; set; }

    string Propriedades { get; set; }

    string Permissoes { get; set; }

    string Requisitos { get; set; }
}