namespace Solucao.Service.API.Cadastro.Configurations;

public class CorsPolicyConfiguration
{
    public string PolicyName { get; set; }
    public List<string> AllowedOrigins { get; set; }
    public List<string> AllowedHeaders { get; set; }
    public List<string> AllowedMethods { get; set; }
    public List<string> ExposedHeaders { get; set; }
}