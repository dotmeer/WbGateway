using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WbGateway.Infrastructure.Metrics.Abstractions;
using WbGateway.Infrastructure.Mqtt.Abstractions;

namespace WbGateway.Implementations;

internal sealed class Zigbee2MqttBackgroundJob : BackgroundService
{
    private readonly ILogger<Zigbee2MqttBackgroundJob> _logger;

    private readonly IMetricsService _metricsService;

    private readonly IMqttService _mqttService;

    private readonly IDictionary<string, string?> _cachedValues;

    public Zigbee2MqttBackgroundJob(
        ILogger<Zigbee2MqttBackgroundJob> logger,
        IMetricsService metricsService, 
        IMqttService mqttService)
    {
        _logger = logger;
        _metricsService = metricsService;
        _mqttService = mqttService;
        _cachedValues = new ConcurrentDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _mqttService.SubscribeAsync(
            new QueueConnection("zigbee2mqtt/+"),
            (message, token) => ReceivedMessageHandler(message, stoppingToken),
            stoppingToken);
    }
    
    private async Task ReceivedMessageHandler(
        QueueMessage message,
        CancellationToken cancellationToken)
    {
        var sourceTopic = message.Topic.Split("/");
        var friendlyName = sourceTopic[1];
        var deviceMessagePayload = message.Payload;

        var zigbeeMessage = JsonSerializer.Deserialize<IDictionary<string, object>>(deviceMessagePayload);

        if (zigbeeMessage is not null)
        {
            foreach (var value in zigbeeMessage)
            {
                var topic = $"wbgateway/{friendlyName}/{value.Key}";
                try
                {
                    var topicValue = value.Value.ToString();
                    var send = true;

                    if (_cachedValues.TryGetValue(topic, out var cachedValue))
                    {
                        if (cachedValue == topicValue)
                        {
                            send = false;
                        }
                        else
                        {
                            send = true;
                            _cachedValues[topic] = topicValue;
                        }
                    }
                    else
                    {
                        _cachedValues.Add(topic, topicValue);
                    }

                    if (send && !string.IsNullOrEmpty(topicValue))
                    {
                        await _mqttService.PublishAsync(
                            new QueueConnection(topic),
                            topicValue,
                            cancellationToken);

                        _logger.LogInformation("Topic '{Topic}' with value {Value}",
                            topic, topicValue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing topic {Topic}", topic);
                }
            }

            _metricsService.IncrementCounter(
                "zigbee2mqtt_read",
                new Dictionary<string, string>
                {
                    ["friendly_name"] = friendlyName
                },
                "Message from zigbee2mqtt was read");
        }
    }
}