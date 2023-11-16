using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WbGateway.Application.Jobs;

namespace WbGateway.Zigbee2Mqtt.BackgroundServices;

internal sealed class Zigbee2MqttBackgroundJob : BackgroundService
{
    private readonly ParseZigbee2MqttEventsJob _parseZigbee2MqttEventsJob;

    public Zigbee2MqttBackgroundJob(ParseZigbee2MqttEventsJob parseZigbee2MqttEventsJob)
    {
        _parseZigbee2MqttEventsJob = parseZigbee2MqttEventsJob;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _parseZigbee2MqttEventsJob.ExecuteAsync(stoppingToken);
    }
}