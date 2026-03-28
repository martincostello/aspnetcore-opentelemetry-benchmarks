// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public interface IScenario
{
    IReadOnlyCollection<ContainerFixture> Containers { get; }

    void Configure(ILoggingBuilder builder)
    {
        builder.ClearProviders();
    }

    void Configure(IServiceCollection services)
    {
        // No-op
    }

    void Configure(List<KeyValuePair<string, string?>> configuration)
    {
        // No-op
    }

    void Configure(OpenTelemetryBuilder telemetry)
    {
        telemetry.ConfigureResource((resource) => resource.AddService("Benchmarks"))
                 .UseOtlpExporter();

        telemetry.WithMetrics(Configure);

        telemetry.WithTracing(Configure);
    }

    void Configure(OpenTelemetryLoggerOptions options)
    {
        // No-op
    }

    void Configure(MeterProviderBuilder metrics)
    {
        // No-op
    }

    void Configure(TracerProviderBuilder tracing)
    {
        // No-op
    }

    void Configure(WebApplicationBuilder builder)
    {
        Configure(builder.Logging);

        if (!string.Equals(builder.Configuration["OTEL_SDK_DISABLED"], bool.TrueString, StringComparison.OrdinalIgnoreCase))
        {
            builder.Logging.AddOpenTelemetry(Configure);

            var telemetry = builder.Services.AddOpenTelemetry();

            Configure(telemetry);
        }

        Configure(builder.Services);
    }

    void Configure(WebApplication app)
    {
        app.MapGet("/", () => TypedResults.Text("ASP.NET Core OpenTelemetry Benchmarks"));
    }
}
