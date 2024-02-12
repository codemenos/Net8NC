namespace Solucao.Service.API.Core.Registers;

using Microsoft.Extensions.Logging;

/// <summary>
/// Registros da aplicação para logging
/// </summary>
public static class ApplicationRegisterLogging
{
    /// <summary>
    /// Registra o logging
    /// </summary>
    /// <param name="logging"></param>
    public static void RegisterLogging(this ILoggingBuilder logging)
    {
        logging.AddRinLogger();
        logging.AddConsole();
        logging.AddDebug();
    }
}
