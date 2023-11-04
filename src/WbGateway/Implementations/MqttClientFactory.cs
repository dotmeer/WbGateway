using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using WbGateway.Interfaces;

namespace WbGateway.Implementations;

internal sealed class MqttClientFactory : IMqttClientFactory
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<MqttClientFactory> _logger;

    private readonly MqttFactory _mqttFactory;

    public MqttClientFactory(
        IConfiguration configuration, 
        ILogger<MqttClientFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _mqttFactory = new MqttFactory();
    }

    public async Task<IMqttClient> CreateMqttClientAsync(string clientName, CancellationToken cancellationToken)
    {
        var mqttClient = _mqttFactory.CreateMqttClient();

        var options = CreateOptions(clientName);

        mqttClient.ConnectedAsync += _ =>
        {
            _logger.LogInformation("Connected to topic {Topic}", clientName);
            return Task.CompletedTask;
        };

        mqttClient.DisconnectedAsync += _ => ReconnectAsync(clientName, mqttClient, options, cancellationToken);

        await mqttClient.ConnectAsync(options, cancellationToken);

        return mqttClient;
    }

    public async Task<IMqttClient> CreateAndSubscribeAsync(
        string clientName, 
        MqttTopicFilter topicFilter, 
        Func<MqttApplicationMessageReceivedEventArgs, Task> applicationMessageReceivedFunc,
        CancellationToken cancellationToken)
    {
        var mqttClient = _mqttFactory.CreateMqttClient();

        var options = CreateOptions(clientName);

        mqttClient.ConnectedAsync += async _ =>
        {
            _logger.LogInformation("Connected to topic {Topic}", clientName);
            await mqttClient.SubscribeAsync(topicFilter, cancellationToken);
        };

        mqttClient.DisconnectedAsync += _ => ReconnectAsync(clientName, mqttClient, options, cancellationToken);

        mqttClient.ApplicationMessageReceivedAsync += applicationMessageReceivedFunc;

        await mqttClient.ConnectAsync(options, cancellationToken);

        return mqttClient;
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