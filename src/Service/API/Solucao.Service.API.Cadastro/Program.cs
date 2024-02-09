namespace Solucao.Service.API.Cadastro;

using System.Globalization;
using Solucao.Service.API.Cadastro.Registers;

/// <summary>
/// Classe principal do projeto.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Método principal do projeto.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");

        var defaultConnection = builder.Configuration.GetConnectionString(ConstantGlobal.StringConnectionDefault);
        var nomesDosBancosDados = new List<string> { "HangFireDB", "HealthCheckerDB", "CadastroDB" };

        CriadorBancoDados.CriarBancosDadosSeNaoExistirem(defaultConnection, nomesDosBancosDados);

        builder.Logging.RegisterLogging();

        builder.Services.RegisterServices(builder.Configuration);

        var app = builder.Build();

        app.RegisterWebApp();

        app.Run();
    }
}
