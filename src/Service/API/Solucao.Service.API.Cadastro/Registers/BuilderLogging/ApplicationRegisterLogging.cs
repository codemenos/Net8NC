namespace Solucao.Service.API.Cadastro.Registers;

public static class ApplicationRegisterLogging
{
    public static void RegisterLogging(this ILoggingBuilder logging)
    {
        logging.AddRinLogger();
        logging.AddConsole();
        logging.AddDebug();
    }
}
