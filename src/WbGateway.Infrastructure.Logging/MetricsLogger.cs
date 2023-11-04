using System;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace WbGateway.Infrastructure.Logging;

internal sealed class MetricsLogger : ILogger
{
    private readonly Counter _logEventCounter = Metrics.CreateCounter(
        "log_event",
        "Log event",
        "level");

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        _logEventCounter
            .WithLabels(logLevel.ToString())
            .Inc();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

#pragma warning disable CS8603
    public IDisposable BeginScope<TState>(TState state) => default;
#pragma warning restore CS8603
}