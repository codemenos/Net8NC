namespace Solucao.Service.API.Cadastro.Registers;

using Microsoft.AspNetCore.Server.Kestrel.Core;

public static class KestrelRegister
{
    public static void AddKestrelControl(this IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });
    }
}