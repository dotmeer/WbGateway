using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using WbGateway.Interfaces;

namespace WbGateway.Implementations;

internal sealed class TestMqttBackgroundJob : IHostedService
{
    private readonly IMqttClientFactory _mqttClientFactory;

    private readonly ILogger<TestMqttBackgroundJob> _logger;

    private IMqttClient? _mqttClient;

    public TestMqttBackgroundJob(
        IMqttClientFactory mqttClientFactory, 
        ILogger<TestMqttBackgroundJob> logger)
    {
        _mqttClientFactory = mqttClientFactory;
        _logger = logger;
        _mqttClient = null;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _mqttClient = await _mqttClientFactory.CreateAndSubscribeAsync(
            "test",
            new MqttTopicFilterBuilder()
                .WithTopic("zigbee2mqtt/+")
                .Build(),
            args =>
            {
                _logger.LogInformation(
                    $"{args.ApplicationMessage.Topic}: {args.ApplicationMessage.ConvertPayloadToString()}");

                return Task.CompletedTask;
            },
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mqttClient?.Dispose();

        return Task.CompletedTask;
    }
}