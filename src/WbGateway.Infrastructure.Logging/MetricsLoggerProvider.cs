using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WbGateway.Infrastructure.Metrics.Abstractions;

namespace WbGateway.Infrastructure.Logging;

internal sealed class MetricsLoggerProvider : ILoggerProvider
{
    private readonly IMetricsService _metricsService;

    private readonly ConcurrentDictionary<string, ILogger> _loggers;

    public MetricsLoggerProvider(IMetricsService metricsService)
    {
        _metricsService = metricsService;
        _loggers = new ConcurrentDictionary<string, ILogger>();
    }

    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, new MetricsLogger(_metricsService));

    public void Dispose()
        => _loggers.Clear();
}