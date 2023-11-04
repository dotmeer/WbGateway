using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using WbGateway.Infrastructure.Metrics.Abstractions;
using WbGateway.Interfaces;

namespace WbGateway.Implementations;

internal sealed class MqttTopicsMetricsBackgroundJob : IHostedService
{
    private readonly IMqttClientFactory _mqttClientFactory;

    private readonly ILogger<MqttTopicsMetricsBackgroundJob> _logger;

    private readonly IMetricsService _metricsService;

    private IMqttClient? _mqttClient;

    public MqttTopicsMetricsBackgroundJob(
        IMqttClientFactory mqttClientFactory,
        ILogger<MqttTopicsMetricsBackgroundJob> logger,
        IMetricsService metricsService)
    {
        _mqttClientFactory = mqttClientFactory;
        _logger = logger;
        _metricsService = metricsService;
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
                        _metricsService.SetGauge(
                            "mqtt_topic_values",
                            value,
                            new Dictionary<string, string>
                            {
                                ["device_name"] = deviceName,
                                ["control_name"] = controlName
                            },
                            "Values from mqtt topics");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while reading device controls");
                }

                return Task.CompletedTask;
            },
            cancellationToken);
        ;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mqttClient?.Dispose();

        return Task.CompletedTask;
    }
}