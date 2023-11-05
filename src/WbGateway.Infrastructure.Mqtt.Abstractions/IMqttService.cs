using System.Threading.Tasks;
using System.Threading;
using System;

namespace WbGateway.Infrastructure.Mqtt.Abstractions;

public interface IMqttService
{
    Task PublishAsync(
        QueueConnection connection,
        string payload,
        CancellationToken cancellationToken);

    Task SubscribeAsync(
        QueueConnection connection,
        Func<QueueMessage, CancellationToken, Task> receiveHandler,
        CancellationToken cancellationToken);
}