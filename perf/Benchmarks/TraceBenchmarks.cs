// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public class TraceBenchmarks : Benchmarks, IScenario
{
    internal static readonly ActivitySource CustomSource = new(typeof(TraceBenchmarks).FullName!);

    protected override Uri Endpoint { get; } = new("/traces", UriKind.Relative);

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddSource(CustomSource.Name);
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/traces", () =>
        {
            using var activity = CustomSource.StartActivity("CustomActivity");
            activity?.SetTag("custom.trace.tag", "value");

            return TypedResults.Ok();
        });
    }
}
