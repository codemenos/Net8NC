namespace Solucao.Service.API.Seguranca;

using System.Reflection;

public class HealthCheckerConfiguration
{
    public static void CarregarFonte(WebApplication app)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fontResourceName = "Solucao.Service.API.Seguranca.Assets.1ae4e3706fe3f478fcc1.woff2";

        app.MapGet("/ui/resources/1ae4e3706fe3f478fcc1.woff2", async context =>
        {
            try
            {
                using var fontStream = assembly.GetManifestResourceStream(fontResourceName);
                
                if (fontStream == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Arquivo de fonte não encontrado.");
                    return;
                }

                // Define o tipo de conteúdo como font/woff2
                context.Response.ContentType = "font/woff2";

                await fontStream.CopyToAsync(context.Response.Body);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Erro ao carregar a fonte: {ex.Message}");
            }
        });
    }
}
