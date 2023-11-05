using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using WbGateway.Infrastructure.Mqtt.Abstractions;

namespace WbGateway.Infrastructure.Mqtt;

internal sealed class MqttService : IMqttService, IDisposable
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<MqttService> _logger;

    private readonly MqttFactory _mqttFactory;

    private readonly IDictionary<string, IMqttClient> _mqttClients;

    public MqttService(
        IConfiguration configuration, 
        ILogger<MqttService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _mqttFactory = new MqttFactory();
        _mqttClients = new ConcurrentDictionary<string, IMqttClient>();
    }

    public async Task PublishAsync(
        QueueConnection connection, 
        string payload, 
        CancellationToken cancellationToken)
    {
        if (!_mqttClients.TryGetValue(connection.ClientName, out var mqttClient))
        {
            mqttClient = _mqttFactory.CreateMqttClient();

            var options = CreateOptions(connection.ClientName);

            mqttClient.ConnectedAsync += _ =>
            {
                _logger.LogInformation("Connected to topic {Topic}", connection.Topic);
                return Task.CompletedTask;
            };

            mqttClient.DisconnectedAsync += _ =>
                ReconnectAsync(connection.ClientName, mqttClient, options, cancellationToken);

            await mqttClient.ConnectAsync(options, cancellationToken);

            _mqttClients[connection.ClientName] = mqttClient;
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(connection.Topic)
            .WithPayload(payload)
            .WithRetainFlag()
            .Build();

        await mqttClient.PublishAsync(message, cancellationToken);
    }

    public async Task SubscribeAsync(
        QueueConnection connection, 
        Func<QueueMessage, CancellationToken, Task> receiveHandler, 
        CancellationToken cancellationToken)
    {
        if (!_mqttClients.TryGetValue(connection.ClientName, out var mqttClient))
        {
            mqttClient = _mqttFactory.CreateMqttClient();

            var options = CreateOptions(connection.ClientName);

            mqttClient.ConnectedAsync += _ =>
            {
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic(connection.Topic)
                    .Build();
                _logger.LogInformation("Connected to topic {Topic}", connection.Topic);
                return mqttClient.SubscribeAsync(topicFilter, cancellationToken);
            };

            mqttClient.DisconnectedAsync += _ =>
                ReconnectAsync(connection.ClientName, mqttClient, options, cancellationToken);

            mqttClient.ApplicationMessageReceivedAsync += _ =>
                receiveHandler(
                    new QueueMessage(_.ApplicationMessage.Topic, _.ApplicationMessage.ConvertPayloadToString()),
                    cancellationToken);

            await mqttClient.ConnectAsync(options, cancellationToken);

            _mqttClients[connection.ClientName] = mqttClient;
        }
    }

    public void Dispose()
    {
        foreach (var mqttClient in _mqttClients.Values)
        {
            mqttClient.Dispose();
        }
    }
    
    private MqttClientOptions CreateOptions(string clientName)
    {
        return new MqttClientOptionsBuilder()
            .WithClientId($"{_configuration["MqttClientPrefix"]}_{clientName}")
            .WithTcpServer(_configuration["mqtt_host"], 1883)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
            .WithCleanSession()
            .Build();
    }

    private async Task ReconnectAsync(
        string topic,
        IMqttClient mqttClient,
        MqttClientOptions options,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("Reconnecting to topic {Topic}.", topic);

        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);

        try
        {
            await mqttClient.ConnectAsync(options, cancellationToken);
        }
        catch
        {
            _logger.LogError("Reconnect failed to topic {Topic}.", topic);
        }
    }
}