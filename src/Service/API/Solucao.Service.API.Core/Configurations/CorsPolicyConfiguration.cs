namespace Solucao.Service.API.Core.Configurations;

/// <summary>
/// Classe de configuração para CORS
/// </summary>
public class CorsPolicyConfiguration
{
    /// <summary>
    /// Nome da política de CORS
    /// </summary>
    public string PolicyName { get; set; }

    /// <summary>
    /// Lista de origens permitidas
    /// </summary>
    public List<string> AllowedOrigins { get; set; }

    /// <summary>
    /// Lista de cabeçalhos permitidos
    /// </summary>
    public List<string> AllowedHeaders { get; set; }

    /// <summary>
    /// Lista de métodos permitidos
    /// </summary>
    public List<string> AllowedMethods { get; set; }

    /// <summary>
    /// Lista de cabeçalhos expostos
    /// </summary>
    public List<string> ExposedHeaders { get; set; }
}