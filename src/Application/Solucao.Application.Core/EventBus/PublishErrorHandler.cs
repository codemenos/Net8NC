namespace Solucao.Application.Core;

using MassTransit;


public class PublishErrorHandler : IConsumer<ReceiveFault>
{
    public async Task Consume(ConsumeContext<ReceiveFault> context)
    {
        var exception = context.Message.Exceptions.First();

        await context.Publish(exception);
    }
}