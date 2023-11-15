using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WbGateway.Application.Jobs;

namespace WbGateway.BackgroundServices;

internal sealed class MqttTopicsMetricsBackgroundJob : BackgroundService
{
    private readonly MqttDevicesControlsMetricsJob _mqttDevicesControlsMetricsJob;

    public MqttTopicsMetricsBackgroundJob(MqttDevicesControlsMetricsJob mqttDevicesControlsMetricsJob)
    {
        _mqttDevicesControlsMetricsJob = mqttDevicesControlsMetricsJob;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _mqttDevicesControlsMetricsJob.ExecuteAsync(stoppingToken);
    }
}