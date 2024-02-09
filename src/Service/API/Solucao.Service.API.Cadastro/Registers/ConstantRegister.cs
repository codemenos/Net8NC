namespace Solucao.Service.API.Cadastro.Registers;

public static class ConstantRegister
{
    public static class HealthCheck
    {
        public const string HealthQuery = "SELECT 1;";
        public const string SegurancaContext = "segurancacontext-db";
        public const string RabbitmqBroker = "rabbitmq-broker";
        public const string RedisCache = "redis-cache";
        public const string DataBase = "database";
        public const string Redis = "redis";
        public const string Cache = "cache";
        public const string SQLServer = "sqlserver";
        public const string SQL = "sql";
        public const string RabbitMq = "rabbitmq";
        public const string Broker = "broker";
        public const string Self = "self";
    }
}