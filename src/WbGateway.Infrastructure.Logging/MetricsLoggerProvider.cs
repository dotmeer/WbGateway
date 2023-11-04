using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace WbGateway.Infrastructure.Logging;

internal sealed class MetricsLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, new MetricsLogger());

    public void Dispose()
    {
        _loggers.Clear();
    }
}