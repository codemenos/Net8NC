namespace Solucao.Service.API.Seguranca.Core.ViewModels;

using System.Globalization;

public class ClientViewModel : IClientViewModel
{
    public Guid Id { get; set; }

    public string ClienteId { get; set; }

    public string ClienteNome { get; set; }

    public string TipoAplicativoCliente { get; set; }

    public string TipoCliente { get; set; }

    public string TipoConsentimento { get; set; }

    public string ClienteSecretKey { get; set; }

    public string JsonWebKeySet { get; set; }

    public List<string> UrlsRedirecionamento { get; set; } = [];

    public List<string> UrlsRedirecionamentoAposLogout { get; set; } = [];

    public string DisplayNames { get; set; }
    public Dictionary<CultureInfo, string> DisplayNamesOpcoes { get; set; } = new Dictionary<CultureInfo, string>
    {
        { new CultureInfo("pt-BR"), "Português (Brasil)" },
        { new CultureInfo("en-US"), "Inglês (EUA)" }
    };


    public string Propriedades { get; set; }
    public List<string> PropriedadesOpcoes { get; set; } = [];

    public string Permissoes { get; set; }
    public List<string> PermissoesOpcoes { get; set; } = [
        "ept:authorization",
        "ept:device",
        "ept:introspection",
        "ept:logout",
        "ept:revocation",
        "ept:token",
        "gt:authorization_code",
        "gt:client_credentials",
        "gt:urn:ietf:params:oauth:grant-type:device_code",
        "gt:implicit",
        "gt:password",
        "gt:refresh_token",
        "rst:code",
        "rst:code id_token",
        "rst:code id_token token",
        "rst:code token",
        "rst:id_token",
        "rst:id_token token",
        "rst:none",
        "rst:token",
        "scp:address",
        "scp:email",
        "scp:phone",
        "scp:profile",
        "scp:roles"
    ];

    public string Requisitos { get; set; }
    public List<string> RequisitosOpcoes { get; set; } = ["ft:pkce"];

    public Dictionary<string, string> ClientsTypes = new()
    {
        { "confidential", "Confidencial" },
        { "public", "Público" }
    };

    public Dictionary<string, string> ApplicationsTypes = new()
    {
        { "native", "Nativo" },
        { "Web", "Web" }
    };

    public Dictionary<string, string> ConsentsTypes = new()
    {
        { "explicit", "Explícito" },
        { "implicit", "Implícito" },
        { "external", "Externo" },
        { "systematic", "Consistente" }
    };
}
