using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Prometheus;
using WbGateway.Infrastructure.Metrics.Abstractions;

namespace WbGateway.Infrastructure.Metrics;

internal sealed class MetricsService : IMetricsService
{
    private readonly IDictionary<string, Counter> _counters = new ConcurrentDictionary<string, Counter>();

    private readonly IDictionary<string, Gauge> _gauges = new ConcurrentDictionary<string, Gauge>();

    public void IncrementCounter(
        string name,
        IDictionary<string, string>? labels = null,
        string? description = null)
    {
        if (!_counters.TryGetValue(name, out var counter))
        {
            counter = Prometheus.Metrics.CreateCounter(
                name,
                description ?? name,
                labels?.Keys.ToArray() ?? Array.Empty<string>());

            _counters[name] = counter;
        }

        counter
            .WithLabels(labels?.Values.ToArray() ?? Array.Empty<string>())
            .Inc();
    }

    public void SetGauge(
        string name,
        double value,
        IDictionary<string, string>? labels = null,
        string? description = null)
    {
        if (!_gauges.TryGetValue(name, out var gauge))
        {
            gauge = Prometheus.Metrics.CreateGauge(
                name,
                description ?? name,
                labels?.Keys.ToArray() ?? Array.Empty<string>());

            _gauges[name] = gauge;
        }

        gauge
            .WithLabels(labels?.Values.ToArray() ?? Array.Empty<string>())
            .Set(value);
    }
}