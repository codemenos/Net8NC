namespace Solucao.Service.API.Cadastro.Registers;

using Solucao.Service.API.Cadastro.Configurations;

public static class CorsPolicyRegister
{
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