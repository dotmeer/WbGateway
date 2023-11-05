using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using WbGateway.Infrastructure.Mqtt.Abstractions;

namespace WbGateway.Application.Jobs;

public sealed class LogZigbee2MqttEventsJob
{
    private readonly ILogger<LogZigbee2MqttEventsJob> _logger;

    private readonly IMqttService _mqttService;

    public LogZigbee2MqttEventsJob(
        ILogger<LogZigbee2MqttEventsJob> logger, 
        IMqttService mqttService)
    {
        _logger = logger;
        _mqttService = mqttService;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return _mqttService.SubscribeAsync(
            new QueueConnection("zigbee2mqtt/+", "test"),
            (message, ct) =>
            {
                _logger.LogInformation(
                    $"{message.Topic}: {message.Payload}");

                return Task.CompletedTask;
            },
            cancellationToken);
    }
}