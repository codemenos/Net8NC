namespace Solucao.Service.API.Seguranca.Registers;

public static class ApplicationRegisterWebApp
{

    public static void RegisterWebApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseRin();
            app.UseRinDiagnosticsHandler();
        }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
    }

}
