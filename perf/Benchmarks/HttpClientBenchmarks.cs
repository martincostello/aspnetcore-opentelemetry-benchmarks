// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public class HttpClientBenchmarks : Benchmarks, IScenario
{
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
        app.MapGet("/httpclient", async (IServer server, HttpClient httpClient) =>
        {
            var serverAddresses = server.Features.Get<IServerAddressesFeature>();
            var baseAddress = serverAddresses!.Addresses.Select((p) => new Uri(p)).Last();
            var requestUri = new Uri(baseAddress, "/echo");

            var response = await httpClient.GetStringAsync(requestUri);

            return TypedResults.Text(response);
        });

        app.MapGet("/echo", () => TypedResults.Text("e c h o"));
    }
}
