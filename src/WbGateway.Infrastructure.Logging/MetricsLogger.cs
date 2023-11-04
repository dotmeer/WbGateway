using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WbGateway.Infrastructure.Metrics.Abstractions;

namespace WbGateway.Infrastructure.Logging;

internal sealed class MetricsLogger : ILogger
{
    private readonly IMetricsService _metricsService;

    public MetricsLogger(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        _metricsService.IncrementCounter(
            "log_event",
            new Dictionary<string, string>
            {
                ["level"] = logLevel.ToString()
            },
            "Log event");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

#pragma warning disable CS8603
    public IDisposable BeginScope<TState>(TState state) => default;
#pragma warning restore CS8603
}