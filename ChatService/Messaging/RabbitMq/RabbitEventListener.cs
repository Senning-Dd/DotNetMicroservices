using System.Reflection;
using EasyNetQ;
using EasyNetQ.Topology;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Messaging.RabbitMq;

public class RabbitEventListener
{
    private readonly IBus busClient;
    private readonly IServiceProvider serviceProvider;
    private readonly Exchange exchange;

    public RabbitEventListener(IBus busClient, IServiceProvider serviceProvider)
    {
        this.busClient = busClient;
        this.serviceProvider = serviceProvider;
        exchange = busClient.Advanced.ExchangeDeclare("lab-dotnet-micro", ExchangeType.Topic);
    }

    public void ListenTo(List<Type> eventsToSubscribe)
    {
        foreach (var eventType in eventsToSubscribe)
        {
            GetType()
                .GetMethod(nameof(Subscribe), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(eventType)
                .Invoke(this, []);
        }
    }

    private void Subscribe<T>() where T : INotification
    {
        var queue = busClient.Advanced.QueueDeclare("lab-chat-service-" + typeof(T).Name);
        busClient.Advanced.Bind(exchange, queue, typeof(T).Name.ToLowerInvariant());
        busClient.Advanced.Consume(queue, (IMessage<T> message, MessageReceivedInfo _) =>
        {
            using var scope = serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMediator>().Publish(message.Body);
        });
    }
}
