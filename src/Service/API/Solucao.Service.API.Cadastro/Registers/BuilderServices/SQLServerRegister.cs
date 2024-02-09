namespace Solucao.Service.API.Cadastro.Registers;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Solucao.Infrastructure.Data.Cadastro.Contexts;
using System.Data;

public static class SQLServerRegister
{
    public static IServiceCollection AddSQLServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConstantGlobal.StringConnectionName);

        services.AddDbContext<CadastroContext>(options =>
        {           
            options.UseSqlServer(connectionString);
            options.LogTo(Console.WriteLine, LogLevel.Error);
        });

        services.AddScoped<IDbConnection>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connection = new SqlConnection(connectionString);
            
            return connection;
        });

        return services;
    }
}
