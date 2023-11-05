using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using WbGateway.Infrastructure.Metrics.Abstractions;
using WbGateway.Infrastructure.Mqtt.Abstractions;

namespace WbGateway.Implementations;

internal sealed class MqttTopicsMetricsBackgroundJob : BackgroundService
{
    private readonly ILogger<MqttTopicsMetricsBackgroundJob> _logger;

    private readonly IMetricsService _metricsService;

    private readonly IMqttService _mqttService;

    public MqttTopicsMetricsBackgroundJob(
        ILogger<MqttTopicsMetricsBackgroundJob> logger,
        IMetricsService metricsService,
        IMqttService mqttService)
    {
        _logger = logger;
        _metricsService = metricsService;
        _mqttService = mqttService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _mqttService.SubscribeAsync(
            new QueueConnection("/devices/+/controls/+", "prometheus"),
            (message, token) =>
            {
                try
                {
                    var topic = message.Topic.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    var deviceName = topic[1];
                    var controlName = topic[3];

                    if (double.TryParse(message.Payload, out var value))
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
            stoppingToken);
    }
}