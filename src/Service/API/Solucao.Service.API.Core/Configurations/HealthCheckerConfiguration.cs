namespace Solucao.Service.API.Core.Configurations;

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Classe de configuração para o HealthChecker.
/// </summary>
public class HealthCheckerConfiguration
{

    /// <summary>
    /// Método para configurar o HealthChecker.
    /// </summary>
    /// <param name="app"></param>
    public static void CarregarFonte(WebApplication app)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fontResourceName = "1ae4e3706fe3f478fcc1.woff2";

        app.MapGet("/ui/resources/1ae4e3706fe3f478fcc1.woff2", async context =>
        {
            try
            {
                var assemblyNamespace = assembly.GetName().Name;
                var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
                var fontFilePath = Path.Combine(assemblyDirectory, "Assets", fontResourceName);

                if (!File.Exists(fontFilePath))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Arquivo de fonte não encontrado.");
                    return;
                }

                // Lê o conteúdo do arquivo como um array de bytes
                var fontBytes = await File.ReadAllBytesAsync(fontFilePath);

                // Define o tipo de conteúdo como font/woff2
                context.Response.ContentType = "font/woff2";

                // Escreve os bytes do arquivo diretamente no corpo da resposta
                await context.Response.Body.WriteAsync(fontBytes);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Erro ao carregar a fonte: {ex.Message}");
            }
        });
    }
}
