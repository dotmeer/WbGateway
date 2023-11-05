namespace WbGateway.Infrastructure.Mqtt.Abstractions;

public sealed class QueueConnection
{
    public string ClientName { get; }

    public string Topic { get; }

    public QueueConnection(string topic, string? clientName = null)
    {
        Topic = topic;
        ClientName = clientName ?? topic;
    }
}