﻿namespace Solucao.Service.API.Seguranca;

public class MassTransitConsumerConfiguration
{
    public string ConsumerName { get; set; }
    public string EventName { get; set; }
    public int RetryCount { get; set; }
    public int RetryInterval { get; set; }
    public bool Durable { get; set; }
}