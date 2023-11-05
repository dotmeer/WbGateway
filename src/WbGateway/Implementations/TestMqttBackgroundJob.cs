using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WbGateway.Infrastructure.Mqtt.Abstractions;

namespace WbGateway.Implementations;

internal sealed class TestMqttBackgroundJob : BackgroundService
{
    private readonly ILogger<TestMqttBackgroundJob> _logger;

    private readonly IMqttService _mqttService;

    public TestMqttBackgroundJob(
        ILogger<TestMqttBackgroundJob> logger,
        IMqttService mqttService)
    {
        _logger = logger;
        _mqttService = mqttService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _mqttService.SubscribeAsync(
            new QueueConnection("zigbee2mqtt/+", "test"),
            (message, token) =>
            {
                _logger.LogInformation(
                    $"{message.Topic}: {message.Payload}");

                return Task.CompletedTask;
            },
            stoppingToken);
    }
}