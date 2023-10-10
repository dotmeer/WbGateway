using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using Prometheus;
using WbGateway.Interfaces;

namespace WbGateway.Implementations;

internal sealed class MqttToPrometheusBackgroundJob : IHostedService
{
    private readonly IMqttClientFactory _mqttClientFactory;

    private readonly ILogger<MqttToPrometheusBackgroundJob> _logger;

    private IMqttClient? _mqttClient;

    public MqttToPrometheusBackgroundJob(IMqttClientFactory mqttClientFactory,
        ILogger<MqttToPrometheusBackgroundJob> logger)
    {
        _mqttClientFactory = mqttClientFactory;
        _logger = logger;
        _mqttClient = null;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _mqttClient = await _mqttClientFactory.CreateAndSubscribeAsync(
            "prometheus",
            new MqttTopicFilterBuilder()
                .WithTopic("/devices/+/controls/+")
                .Build(),
            args =>
            {
                try
                {
                    var payload = args.ApplicationMessage.ConvertPayloadToString();
                    var topic = args.ApplicationMessage.Topic.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    var deviceName = topic[1];
                    var controlName = topic[3];

                    if (double.TryParse(payload, out var value))
                    {
                        Metrics.CreateGauge(
                                "mqtt_topic_values",
                                "Mqtt topic values",
                                "device_name", "control_name")
                            .WithLabels(deviceName, controlName)
                            .Set(value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while reading device controls");
                }

                return Task.CompletedTask;
            },
            cancellationToken);;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mqttClient?.Dispose();

        return Task.CompletedTask;
    }
}