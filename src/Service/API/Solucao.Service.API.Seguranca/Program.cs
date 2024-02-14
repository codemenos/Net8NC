namespace Solucao.Service.API.Seguranca;

using System.Globalization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using Solucao.Infrastructure.Shared.Common;
using Solucao.Service.API.Core.Registers;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Domain.Seguranca.Entities;

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

        builder.Services
        .AddIdentityApiEndpoints<SecurityUser>()
        .AddEntityFrameworkStores<SegurancaContext>();

        builder.Services.RegisterServices(typeof(Program), builder.Configuration);
        
        builder.Services.AddDbContext<SegurancaContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SegurancaConnection"));
        });


        var app = builder.Build();

        app.MapGroup("/Identidade").WithGroupName("Identidade").MapIdentityApi<SecurityUser>().WithGroupName("Identidade");

        app.RegisterWebApp();

        app.Run();
    }
}
