namespace WbGateway.Infrastructure.Mqtt.Abstractions;

public sealed class QueueMessage
{
    public string Topic { get; }

    public string Payload { get; }

    public QueueMessage(string topic, string payload)
    {
        Topic = topic;
        Payload = payload;
    }
}