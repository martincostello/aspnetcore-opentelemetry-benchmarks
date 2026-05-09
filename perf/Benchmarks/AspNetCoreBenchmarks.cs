// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

[BenchmarkCategory("ASP.NET Core")]
public class AspNetCoreBenchmarks : Benchmarks, IScenario
{
    protected override Uri Endpoint { get; } = new("/ping", UriKind.Relative);

    public void Configure(MeterProviderBuilder metrics)
    {
        metrics.AddAspNetCoreInstrumentation();
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddAspNetCoreInstrumentation();
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/ping", () => TypedResults.NoContent());
    }

    protected override async Task StartServer(TelemetryConfiguration configuration)
    {
        // TODO Depends on https://github.com/open-telemetry/opentelemetry-dotnet-contrib/pull/4376
        ////AppContext.SetSwitch("Microsoft.AspNetCore.Hosting.SuppressActivityOpenTelemetryData", false);
        await base.StartServer(configuration);
    }
}
