namespace Solucao.Service.API.Core.Registers;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Solucao.Infrastructure.Data.Cadastro.Contexts;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using Solucao.Infrastructure.Shared.Common;

/// <summary>
/// Registra o SQL Server no container de injeção de dependência.
/// </summary>
public static class SQLServerRegister
{
    /// <summary>
    /// Adiciona o SQL Server no container de injeção de dependência.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddSQLServer(this IServiceCollection services, Type type, IConfiguration configuration)
    {
        const string SEGURANCA = "Seguranca";
        const string CADASTRO = "Cadastro";

        var connectionString = string.Empty;

        if (type.AssemblyQualifiedName.Contains(SEGURANCA))
        {
            connectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionSeguranca);

            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDbContext<SegurancaContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.UseOpenIddict();
                    options.LogTo(Console.WriteLine, LogLevel.Error);
                });

                services.AddScoped<IDbConnection>(provider =>
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    var connection = new SqlConnection(connectionString);

                    return connection;
                });

                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SegurancaContext>();
                context.Database.Migrate();
            }
        }

        if (type.AssemblyQualifiedName.Contains(CADASTRO))
        {
            connectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionCadastro);

            services.AddDbContext<CadastroContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.LogTo(Console.WriteLine, LogLevel.Error);
            });
        }

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddScoped<IDbConnection>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var connection = new SqlConnection(connectionString);

                return connection;
            });
        }

        return services;
    }
}
