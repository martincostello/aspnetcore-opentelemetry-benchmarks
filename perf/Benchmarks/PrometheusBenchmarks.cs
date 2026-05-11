// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

[BenchmarkCategory("Metrics")]
public class PrometheusBenchmarks : Benchmarks, IScenario
{
    public override IReadOnlyCollection<ContainerFixture> Containers { get; } = [new PrometheusFixture()];

    protected override Uri Endpoint { get; } = new("/prometheus", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<MetricBenchmarks.CustomMetrics>();
    }

    public void Configure(IWebHostBuilder builder)
    {
        builder.UseUrls("http://0.0.0.0:0");
        builder.UseSetting("AllowedHosts", "*");
    }

    public void Configure(OpenTelemetryBuilder telemetry, TelemetryConfiguration configuration)
    {
        telemetry.ConfigureResource((resource) => resource.AddService("Benchmarks"));

        if (configuration.EnableMetrics)
        {
            telemetry.WithMetrics(Configure);
        }

        if (configuration.EnableTraces)
        {
            telemetry.WithTracing((p) => p.AddOtlpExporter());
        }
    }

    public void Configure(MeterProviderBuilder metrics)
    {
        metrics.AddMeter(MetricBenchmarks.MeterName)
               .AddPrometheusExporter();
    }

    public void Configure(WebApplication app)
    {
        // TODO Depends on https://github.com/open-telemetry/opentelemetry-dotnet/pull/7273
        app.MapPrometheusScrapingEndpoint();

        app.MapGet("/prometheus", (MetricBenchmarks.CustomMetrics metrics) =>
        {
            metrics.Increment();
            return TypedResults.NoContent();
        });
    }

    protected override HttpClient CreateHttpClient(AppServer app)
    {
        var client = base.CreateHttpClient(app);

        client.BaseAddress = new($"http://127.0.0.1:{app.BaseAddress.Port}", UriKind.Absolute);

        return client;
    }

    protected override Task OnServerStartedAsync()
    {
        this.Containers.OfType<PrometheusFixture>().Single().TargetPort = BaseAddress.Port;
        return base.OnServerStartedAsync();
    }
}
