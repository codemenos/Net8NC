namespace Solucao.Service.API.Core.Registers;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Register Kestrel
/// </summary>
public static class KestrelRegister
{
    /// <summary>
    /// Adciona o Kestrel
    /// </summary>
    /// <param name="services"></param>
    public static void AddKestrelControl(this IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });
    }
}