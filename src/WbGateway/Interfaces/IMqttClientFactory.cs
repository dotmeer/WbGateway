using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Packets;

namespace WbGateway.Interfaces;

public interface IMqttClientFactory
{
    Task<IMqttClient> CreateMqttClientAsync(string clientName, CancellationToken cancellationToken);

    Task<IMqttClient> CreateAndSubscribeAsync(
        string clientName,
        MqttTopicFilter topicFilter,
        Func<MqttApplicationMessageReceivedEventArgs, Task> applicationMessageReceivedFunc,
        CancellationToken cancellationToken);
}