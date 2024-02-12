namespace Solucao.Service.API.Seguranca;

using System.Globalization;
using Solucao.Infrastructure.Shared.Common;
using Solucao.Service.API.Core.Registers;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        CultureInfo.CurrentCulture = new CultureInfo(ConstantGlobal.CULTURE);

        var defaultConnection = builder.Configuration.GetConnectionString(ConstantGlobal.StringConnectionDefault);
        var nomesDosBancosDados = new List<string> { ConstantGlobal.HANGFIRE_DB, ConstantGlobal.HEALTH_CHECKER_DB, ConstantGlobal.SEGURANCA_DB };

        CriadorBancoDados.CriarBancosDadosSeNaoExistirem(defaultConnection, nomesDosBancosDados);

        builder.Logging.RegisterLogging();

        builder.Services.RegisterServices(typeof(Program), builder.Configuration);

        var app = builder.Build();

        app.RegisterWebApp();

        app.Run();
    }
}
