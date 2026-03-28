// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public class DefaultBenchmarks : Benchmarks, IScenario
{
    protected override Uri Endpoint { get; } = new("/default", UriKind.Relative);

    public void Configure(WebApplication app)
    {
        app.MapGet("/default", () => TypedResults.Ok());
    }
}
