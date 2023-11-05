using Microsoft.Extensions.DependencyInjection;
using WbGateway.Infrastructure.Mqtt.Abstractions;

namespace WbGateway.Infrastructure.Mqtt;

public static class InfrastructureMqttExtensions
{
    public static IServiceCollection AddMqtt(this IServiceCollection services)
    {
        services.AddSingleton<IMqttService, MqttService>();

        return services;
    }
}