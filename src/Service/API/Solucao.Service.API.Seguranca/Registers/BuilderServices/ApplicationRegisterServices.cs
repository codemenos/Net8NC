namespace Solucao.Service.API.Seguranca.Registers;

public static class ApplicationRegisterServices
{

    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSQLServer(configuration);
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddRin();
        services.AddLogging();
    }

}
