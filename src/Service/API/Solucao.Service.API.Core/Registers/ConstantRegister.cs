namespace Solucao.Service.API.Core.Registers;

/// <summary>
/// Constantes utilizadas no registros do projeto.
/// </summary>
public static class ConstantRegister
{
    /// <summary>
    /// Constantes de HealthCheck.
    /// </summary>
    public static class HealthCheck
    {
        /// <summary>
        /// Query de HealthCheck.
        /// </summary>
        public const string HealthQuery = "SELECT 1;";

        /// <summary>
        /// Nome do HealthCheck do SQL Server para api segurança.
        /// </summary>
        public const string SegurancaContext = "segurancacontext-db";

        /// <summary>
        /// Nome do HealthCheck do SQL Server para api cadastro.
        /// </summary>
        public const string CadastroContext = "cadastrocontext-db";

        /// <summary>
        /// Nome do HealthCheck do RabbitMQ.
        /// </summary>
        public const string RabbitmqBroker = "rabbitmq-broker";

        /// <summary>
        /// Noem do HealthCheck do Redis.
        /// </summary>
        public const string RedisCache = "redis-cache";

        /// <summary>
        /// Nome do HealthCheck para Database.
        /// </summary>
        public const string DataBase = "database";

        /// <summary>
        /// Nome do HealthCheck para Redis.
        /// </summary>
        public const string Redis = "redis";

        /// <summary>
        /// Nome do HealthCheck para Cache.
        /// </summary>
        public const string Cache = "cache";

        /// <summary>
        /// Nome do HealthCheck para SQL Server.
        /// </summary>
        public const string SQLServer = "sqlserver";

        /// <summary>
        /// Nome do HealthCheck para SQL.
        /// </summary>
        public const string SQL = "sql";

        /// <summary>
        /// Nome do HealthCheck para RabbitMQ.
        /// </summary>
        public const string RabbitMq = "rabbitmq";

        /// <summary>
        /// Nome do HealthCheck para Broker.
        /// </summary>
        public const string Broker = "broker";

        /// <summary>
        /// Nome do HealthCheck para Self.
        /// </summary>
        public const string Self = "self";
    }
}