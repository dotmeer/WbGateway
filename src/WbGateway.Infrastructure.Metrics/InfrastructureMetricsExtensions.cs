using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using WbGateway.Infrastructure.Metrics.Abstractions;

namespace WbGateway.Infrastructure.Metrics;

public static class InfrastructureMetricsExtensions
{
    public static IEndpointRouteBuilder AddMetricsPullHost(
        this IEndpointRouteBuilder builder)
    {
        builder.MapMetrics();

        return builder;
    }

    public static IServiceCollection SetupMetrics(
        this IServiceCollection services)
    {
        Prometheus.Metrics.DefaultRegistry.SetStaticLabels(
            new Dictionary<string, string>
            {
                ["service"] = "wbgateway"
            });

        services.AddSingleton<IMetricsService, MetricsService>();

        return services;
    }
}