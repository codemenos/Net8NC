namespace Solucao.Service.API.Seguranca;

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using Solucao.Infrastructure.Shared.Common;
using Solucao.Service.API.Core.Registers;
using Solucao.Domain.Seguranca.Aggregates;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Solucao.Service.API.Core.Handlers;
using Solucao.Service.API.Core.Services;
using Solucao.Service.API.Core;
using OpenIddict.Server;

public partial class Program
{
    private const string IDENTIDADE = "Identidade";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        CultureInfo.CurrentCulture = new CultureInfo(ConstantGlobal.CULTURE);

        var defaultConnection = builder.Configuration.GetConnectionString(ConstantGlobal.StringConnectionDefault);
        var nomesDosBancosDados = new List<string> { ConstantGlobal.HANGFIRE_DB, ConstantGlobal.HEALTH_CHECKER_DB, ConstantGlobal.SEGURANCA_DB };

        CriadorBancoDados.CriarBancosDadosSeNaoExistirem(defaultConnection, nomesDosBancosDados);

        builder.Logging.RegisterLogging();

        builder.Services
            .AddIdentity<SecurityUser, SecurityRole>()
            .AddEntityFrameworkStores<SegurancaContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<SignInManager<SecurityUser>, SignInManagerHandler<SecurityUser>>();
        builder.Services.AddTransient<IEmailSender<SecurityUser>, EmailSenderService>();

        //builder.Services.AddIdentityApiEndpoints<SecurityUser>();

        builder.Services.RegisterServices(typeof(Program), builder.Configuration);

        builder.Services.AddDbContext<SegurancaContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString(ConstantGlobal.StringConnectionSeguranca));
        });



        var app = builder.Build();

        app.MapGroup($"/{IDENTIDADE}").WithGroupName(IDENTIDADE).MapIdentityApi<SecurityUser>().WithGroupName(IDENTIDADE).AllowAnonymous();

        //app.MapGet("/", () => "Olá Anonimo!").AllowAnonymous();
        //app.MapGet("/requires-auth", (ClaimsPrincipal user) => $"Olá, {user.Identity?.Name}!").RequireAuthorization();

        app.RegisterWebApp();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
        });

        using (var scope = app.Services.CreateScope())
        {
            var cargaPadrao = scope.ServiceProvider.GetRequiredService<CargaPadraoOpenIddictService>();
            cargaPadrao.AdicionarClientesDeAutenticacao(requerExclusaoDoLegado: true).GetAwaiter().GetResult();
            cargaPadrao.AdicionarEscoposDeAutorizacao(requerExclusaoDoLegado: true).GetAwaiter().GetResult();
        }

        app.Run();
    }
}
