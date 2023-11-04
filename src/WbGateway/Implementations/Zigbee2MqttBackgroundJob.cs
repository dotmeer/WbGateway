using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using WbGateway.Infrastructure.Metrics.Abstractions;
using WbGateway.Interfaces;

namespace WbGateway.Implementations;

internal sealed class Zigbee2MqttBackgroundJob : IHostedService
{
    private readonly IMqttClientFactory _mqttClientFactory;

    private readonly ILogger<Zigbee2MqttBackgroundJob> _logger;

    private readonly IMetricsService _metricsService;

    private readonly ConcurrentDictionary<string, IMqttClient> _mqttClients;

    private readonly IDictionary<string, string?> _cachedValues;

    public Zigbee2MqttBackgroundJob(
        IMqttClientFactory mqttClientFactory,
        ILogger<Zigbee2MqttBackgroundJob> logger,
        IMetricsService metricsService)
    {
        _mqttClientFactory = mqttClientFactory;
        _logger = logger;
        _metricsService = metricsService;
        _mqttClients = new ConcurrentDictionary<string, IMqttClient>(StringComparer.OrdinalIgnoreCase);
        _cachedValues = new ConcurrentDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var topic = "zigbee2mqtt/+";

        var mqttClient = await _mqttClientFactory.CreateAndSubscribeAsync(
            topic,
            new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build(),
            args => MqttDeviceMessageHandler(args, cancellationToken),
            cancellationToken);

        _mqttClients[topic] = mqttClient;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var mqttClient in _mqttClients.Values)
        {
            mqttClient.Dispose();
        }

        return Task.CompletedTask;
    }

    private async Task MqttDeviceMessageHandler(
        MqttApplicationMessageReceivedEventArgs deviceArgs,
        CancellationToken cancellationToken)
    {
        var sourceTopic = deviceArgs.ApplicationMessage.Topic.Split("/");
        var friendlyName = sourceTopic[1];
        var deviceMessagePayload = deviceArgs.ApplicationMessage.ConvertPayloadToString();

        var zigbeeMessage = JsonSerializer.Deserialize<IDictionary<string, object>>(deviceMessagePayload);

        if (zigbeeMessage is not null)
        {
            foreach (var value in zigbeeMessage)
            {
                var topic = $"wbgateway/{friendlyName}/{value.Key}";
                try
                {
                    if (!_mqttClients.ContainsKey(topic))
                    {
                        _mqttClients[topic] = await _mqttClientFactory.CreateMqttClientAsync(topic, cancellationToken);
                    }

                    var client = _mqttClients[topic];

                    if (client.IsConnected)
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

                        if (send)
                        {
                            var message = new MqttApplicationMessageBuilder()
                                .WithTopic(topic)
                                .WithPayload(topicValue)
                                .WithRetainFlag()
                                .Build();

                            await client.PublishAsync(message, cancellationToken);

                            _logger.LogInformation("Topic '{Topic}' with value {Value}",
                                topic, topicValue);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Client to topic {Topic} not connected, message lost", topic);
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