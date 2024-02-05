namespace Solucao.Service.API.Seguranca.Registers;

public static class ApplicationRegisterLogging
{
    public static void RegisterLogging(this ILoggingBuilder logging)
    {
        logging.AddRinLogger();
        logging.AddConsole();
        logging.AddDebug();
    }
}
