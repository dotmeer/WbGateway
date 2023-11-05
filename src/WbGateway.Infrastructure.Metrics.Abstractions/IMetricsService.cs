using System.Collections.Generic;

namespace WbGateway.Infrastructure.Metrics.Abstractions;

public interface IMetricsService
{
    void IncrementCounter(
        string name,
        IDictionary<string, string>? labels = null,
        string? description = null);

    void SetGauge(
        string name,
        double value,
        IDictionary<string, string>? labels = null,
        string? description = null);
}