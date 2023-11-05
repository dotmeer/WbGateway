using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WbGateway.Application.Jobs;

namespace WbGateway.BackgroundServices;

internal sealed class TestMqttBackgroundJob : BackgroundService
{
    private readonly LogZigbee2MqttEventsJob _logZigbee2MqttEventsJob;

    public TestMqttBackgroundJob(LogZigbee2MqttEventsJob logZigbee2MqttEventsJob)
    {
        _logZigbee2MqttEventsJob = logZigbee2MqttEventsJob;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _logZigbee2MqttEventsJob.ExecuteAsync(stoppingToken);
    }
}