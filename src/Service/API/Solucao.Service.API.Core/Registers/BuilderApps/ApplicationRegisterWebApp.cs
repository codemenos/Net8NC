namespace Solucao.Service.API.Core.Registers;

using System.Reflection;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Solucao.Service.API.Core.Configurations;

/// <summary>
/// Configurações e registros da aplicação web
/// </summary>
public static class ApplicationRegisterWebApp
{

    /// <summary>
    /// Registra a aplicação web
    /// </summary>
    /// <param name="app"></param>
    public static void RegisterWebApp(this WebApplication app)
    {
        const string IMG_PATH = "/img";
        const string ASSETS_PATH = "Assets";
        
        app.UseRin();
        app.UseRinDiagnosticsHandler();

        // Obter o caminho para a DLL atual
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Obter o diretório onde a DLL está localizada
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);

        // Construir o caminho para a pasta "Assets" dentro do diretório da DLL
        var assetsDirectory = Path.Combine(assemblyDirectory, ASSETS_PATH);

        // Verificar se o diretório de ativos existe
        if (Directory.Exists(assetsDirectory))
        {
            // Use o provedor de arquivos físicos com o caminho dos ativos
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(assetsDirectory),
                RequestPath = IMG_PATH
            });
        }

        app.RunDevelopment();

        app.ConfigureSwaggerUI();

        app.UseHttpsRedirection();

        app.ConfigureCors();

        app.MapControllers();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.ConfigureEndpoints();

        app.UseHangfireDashboard();
    }

    /// <summary>
    /// Registra e configura o Swagger UI
    /// </summary>
    /// <param name="app"></param>
    public static void ConfigureSwaggerUI(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => { SwaggerConfiguration.ConfigureSwaggerUI(app, options); });
        HealthCheckerConfiguration.CarregarFonte(app);
    }

    /// <summary>
    /// Registra e configura o CORS
    /// </summary>
    /// <param name="app"></param>
    public static void ConfigureCors(this WebApplication app)
    {
        app.UseCorsPolicy();
    }

    /// <summary>
    /// Registra e configura para quando for excluisivo em desenvolvimento
    /// </summary>
    /// <param name="app"></param>
    public static void RunDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
        }
    }

    /// <summary>
    /// Registra e configura os endpoints do HealthCheck
    /// </summary>
    /// <param name="app"></param>
    public static void ConfigureEndpoints(this WebApplication app)
    {
        // Obter o caminho para a DLL atual
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Obter o diretório onde a DLL está localizada
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);

        // Construir o caminho para a pasta "Assets" dentro do diretório da DLL
        var assetsDirectory = Path.Combine(assemblyDirectory, "Assets");

        app.UseEndpoints(endpoint =>
        {
            _ = endpoint.MapHealthChecks("/hc", new HealthCheckOptions 
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            _ = endpoint.MapHealthChecksUI(setup =>
            {
                setup.UIPath = "/hc-ui";
                setup.AddCustomStylesheet($"{assetsDirectory}\\HealthChecks_Dark.css");
            });
        });
    }
}
