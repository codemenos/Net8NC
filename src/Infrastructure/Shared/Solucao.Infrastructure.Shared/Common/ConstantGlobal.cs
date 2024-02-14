namespace Solucao.Infrastructure.Shared.Common;

public static class ConstantGlobal
{
    public const string RouteApiController = "api/[controller]";

    public const string StringConnectionDefault = "DefaultConnection";
    public const string StringConnectionSeguranca = "SegurancaConnection";
    public const string StringConnectionCadastro = "CadastroConnection";
    public const string StringConnectionHangFire = "HangfireConnection";
    public const string StringConnectionHealthChecker = "HealthCheckerConnection";

    public const string CULTURE = "pt-BR";
    public const string HANGFIRE_DB = "HangFireDB";
    public const string HEALTH_CHECKER_DB = "HealthCheckerDB";
    public const string SEGURANCA_DB = "SegurancaDB";
    public const string CADASTRO_DB = "CadastroDB";
}
