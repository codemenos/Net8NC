﻿namespace Solucao.Service.API.Core.Registers;

using System.Reflection;
using System.Reflection.Metadata;
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

        app.UseHttpsRedirection();
        app.UseStaticFiles();
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
            app.UseDeveloperExceptionPage();
        }
    }

    /// <summary>
    /// Registra e configura os endpoints do HealthCheck
    /// </summary>
    /// <param name="app"></param>
    public static void ConfigureEndpoints(this WebApplication app)
    {
        const string ASSETS = "Assets";

        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
        var assetsDirectory = Path.Combine(assemblyDirectory, ASSETS);

        app.UseEndpoints(endpoint =>
        {
            const string HC_PATH = "/hc";
            const string HC_UI_PATH = "/hc-ui";
            const string CSS_DEFAULT = "HealthChecks_Dark.css";

            _ = endpoint.MapHealthChecks(HC_PATH, new HealthCheckOptions 
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            _ = endpoint.MapHealthChecksUI(setup =>
            {
                var cssFilePath = Path.Combine(assetsDirectory, CSS_DEFAULT);
                
                setup.UIPath = HC_UI_PATH;
                setup.AddCustomStylesheet(cssFilePath);
            });
        });
    }
}
