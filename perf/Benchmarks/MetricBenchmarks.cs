// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public class MetricBenchmarks : Benchmarks, IScenario
{
    internal static readonly string MeterName = typeof(MetricBenchmarks).FullName!;

    protected override Uri Endpoint { get; } = new("/metrics", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<CustomMetrics>();
    }

    public void Configure(MeterProviderBuilder metrics)
    {
        metrics.AddMeter(MeterName);
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/metrics", (CustomMetrics metrics) =>
        {
            metrics.Increment();
            return TypedResults.Ok();
        });
    }

    internal sealed class CustomMetrics
    {
        private readonly Counter<int> _counter;

        public CustomMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);
            _counter = meter.CreateCounter<int>(
                "custom.metric.increment",
                null,
                null,
                [new KeyValuePair<string, object?>("custom.metric.tag.a", 1)]);
        }

        public void Increment()
        {
            if (_counter.Enabled)
            {
                _counter.Add(1, new KeyValuePair<string, object?>("custom.metric.tag.b", 2));
            }
        }
    }
}
