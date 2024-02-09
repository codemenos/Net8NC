namespace Solucao.Service.API.Seguranca;

using System.Globalization;
using Solucao.Service.API.Seguranca.Registers;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");

        var defaultConnection = builder.Configuration.GetConnectionString(ConstantGlobal.StringConnectionDefault);
        var nomesDosBancosDados = new List<string> { "HangFireDB", "HealthCheckerDB", "SegurancaDB" };

        CriadorBancoDados.CriarBancosDadosSeNaoExistirem(defaultConnection, nomesDosBancosDados);

        builder.Logging.RegisterLogging();

        builder.Services.RegisterServices(builder.Configuration);

        var app = builder.Build();

        app.RegisterWebApp();

        app.Run();
    }
}
