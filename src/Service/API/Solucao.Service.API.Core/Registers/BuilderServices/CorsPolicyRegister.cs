namespace Solucao.Service.API.Core.Registers;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Solucao.Service.API.Core.Configurations;

/// <summary>
/// Registros da aplicação para CORS Policy
/// </summary>
public static class CorsPolicyRegister
{
    /// <summary>
    /// Registra a política de CORS
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCorsPolicy(this WebApplication app)
    {
        var corsSettings = app.Configuration.GetSection(ConstantSection.CORSPOLICY).Get<List<CorsPolicyConfiguration>>();
        
        var possuiConfiguracoes = corsSettings is not null && corsSettings.Any();
        if (possuiConfiguracoes)
        {
            app.UseCors(builder =>
            {
                foreach (var corsPolicy in corsSettings)
                {
                    builder.WithOrigins([.. corsPolicy.AllowedOrigins]);

                    var possuiCabecalhosPermitidos = corsPolicy.AllowedHeaders is not null && corsPolicy.AllowedHeaders.Any();
                    if (possuiCabecalhosPermitidos)
                    {
                        builder.WithHeaders([.. corsPolicy.AllowedHeaders]);
                    }

                    var possuiMetodosPermitidos = corsPolicy.AllowedMethods is not null && corsPolicy.AllowedMethods.Any();
                    if (possuiMetodosPermitidos)
                    {
                        builder.WithMethods([.. corsPolicy.AllowedMethods]);
                    }

                    var possuiCabecalhosExpostos = corsPolicy.ExposedHeaders is not null && corsPolicy.ExposedHeaders.Any();
                    if (possuiCabecalhosExpostos)
                    {
                        builder.WithExposedHeaders([.. corsPolicy.ExposedHeaders]);
                    }
                }
            });
        }

        return app;
    }

}