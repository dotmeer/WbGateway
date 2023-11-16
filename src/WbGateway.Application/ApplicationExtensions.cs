using Microsoft.Extensions.DependencyInjection;
using WbGateway.Application.Jobs;

namespace WbGateway.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection SetupApplication(this IServiceCollection services)
    {
        services
            .AddSingleton<LogZigbee2MqttEventsJob>()
            .AddSingleton<MqttDevicesControlsMetricsJob>()
            .AddSingleton<ParseZigbee2MqttEventsJob>();

        return services;
    }
}