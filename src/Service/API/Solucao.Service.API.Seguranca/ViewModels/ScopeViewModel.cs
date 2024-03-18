namespace Solucao.Service.API.Seguranca.ViewModels;
using System.Globalization;

public class ScopeViewModel
{
    private readonly Dictionary<CultureInfo, string> Idiomas = new()
    {
        { new CultureInfo("pt-BR"), "Português (Brasil)" },
        { new CultureInfo("en-US"), "Inglês (EUA)" }
    };

    public Guid Id { get; set; } = default;
    
    public string Nome { get; set; } = string.Empty;
    public string NomeExibicao { get; set; }
    public string NomesExibicoes { get; set; }
    public Dictionary<CultureInfo, string> NomesExibicoesOpcoes { get { return Idiomas; } }
    
    public string Descricao { get; set; }    
    public string Descricoes { get; set; }
    public Dictionary<CultureInfo, string> DescricoesOpcoes { get { return Idiomas; } }

    public string Recursos { get; set; }
    public List<string> RecursosOpcoes { get; set; } = ["ft:pkce"];

    public string Propriedades { get; set; }
    public List<string> PropriedadesOpcoes { get; set; } = [];
}