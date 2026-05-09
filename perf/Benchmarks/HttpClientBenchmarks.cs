// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

[BenchmarkCategory("HTTP")]
public class HttpClientBenchmarks : Benchmarks, IScenario
{
    private Uri? _echoUri;

    protected override Uri Endpoint { get; } = new("/httpclient", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
        services.AddHttpClient();
    }

    public void Configure(MeterProviderBuilder metrics)
    {
        metrics.AddHttpClientInstrumentation();
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddHttpClientInstrumentation();
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/httpclient", async (HttpClient httpClient) =>
        {
            using var response = await httpClient.GetAsync(_echoUri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return TypedResults.NoContent();
        });

        app.MapGet("/echo", () => TypedResults.NoContent());
    }

    protected override Task OnServerStartedAsync()
    {
        _echoUri = new Uri(BaseAddress, "/echo");
        return Task.CompletedTask;
    }
}
