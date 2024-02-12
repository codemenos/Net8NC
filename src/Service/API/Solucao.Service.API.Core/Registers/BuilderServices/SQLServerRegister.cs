namespace Solucao.Service.API.Core.Registers;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Solucao.Infrastructure.Data.Cadastro.Contexts;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using Solucao.Infrastructure.Shared.Common;
using System.Data;

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

            services.AddDbContext<SegurancaContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.LogTo(Console.WriteLine, LogLevel.Error);
            });
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
