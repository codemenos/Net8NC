namespace Solucao.Service.API.Cadastro.Registers;

using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Solucao.Infrastructure.Data.Cadastro.Contexts;
using Solucao.Service.API.Cadastro.Configurations;
using Solucao.Service.API.Cadastro.Registers;


public static class ApplicationRegisterWebApp
{
    public static void RegisterWebApp(this WebApplication app)
    {
        app.UseRin();
        app.UseRinDiagnosticsHandler();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Assets")), RequestPath = "/img" 
        });

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


    public static void ConfigureSwaggerUI(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => { SwaggerConfiguration.ConfigureSwaggerUI(app, options); });
        HealthCheckerConfiguration.CarregarFonte(app);
    }

    public static void ConfigureCors(this WebApplication app)
    {
        app.UseCorsPolicy();
    }

    public static void RunDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
        }
    }

    public static void ConfigureEndpoints(this WebApplication app)
    {
        app.UseEndpoints(endpoint =>
        {
            _ = endpoint.MapHealthChecks("/hc", new HealthCheckOptions 
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            _ = endpoint.MapHealthChecksUI(setup =>
            {
                setup.UIPath = "/hc-ui";
                setup.AddCustomStylesheet($"{app.Environment.ContentRootPath}/Assets/HealthChecks_Dark.css");
            });
        });
    }
}
