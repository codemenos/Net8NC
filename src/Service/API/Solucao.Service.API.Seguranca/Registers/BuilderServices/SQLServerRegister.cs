namespace Solucao.Service.API.Seguranca.Registers;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Solucao.Infrastructure.Data.Seguranca.Contexts;
using System.Data;

public static class SQLServerRegister
{
    public static IServiceCollection AddSQLServer(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlServerPrincipalConnectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionName);

        services.AddDbContext<SegurancaContext>(options =>
        {           
            options.UseSqlServer(sqlServerPrincipalConnectionString);
            options.LogTo(Console.WriteLine, LogLevel.Error);
        });

        services.AddScoped<IDbConnection>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connection = new SqlConnection(sqlServerPrincipalConnectionString);
            
            return connection;
        });

        return services;
    }
}
