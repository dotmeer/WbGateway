using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WbGateway.Infrastructure.Logging;

public static class InfrastructureLoggingExtensions
{
    public static ILoggingBuilder AddMetricsLogger(
        this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, MetricsLoggerProvider>());

        return builder;
    }

    public static IHostBuilder SetupLogging(
        this IHostBuilder builder)
    {
        builder.ConfigureLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
            loggingBuilder.AddMetricsLogger();
        });

        return builder;
    }
}