namespace Solucao.Service.API.Seguranca;

public class RabbitMqConfiguration
{
    public string Protocol { get; set; } = "amqp";
    public string HostName { get; set; }
    public ushort Port { get; set; }
    public string VirtualHost { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
