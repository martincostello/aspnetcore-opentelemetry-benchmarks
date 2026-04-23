// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

internal sealed class AppServer : IAsyncDisposable
{
    private WebApplication? _app;
    private Uri? _baseAddress;
    private CollectorFixture? _collector;
    private bool _disposed;

    public HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            CheckCertificateRevocationList = true,
        };

        return new(handler, disposeHandler: true)
        {
            BaseAddress = _baseAddress,
        };
    }

    public async Task StartAsync(IScenario scenario, TelemetryConfiguration configuration)
    {
        if (_app is not null)
        {
            throw new InvalidOperationException("The server is already running.");
        }

        foreach (var container in scenario.Containers)
        {
            await container.StartAsync();
        }

        var builder = WebApplication.CreateBuilder([$"--contentRoot={GetContentRoot()}"]);

        builder.WebHost.UseUrls("http://127.0.0.1:0");

        var config = new List<KeyValuePair<string, string?>>()
        {
            KeyValuePair.Create<string, string?>("OTEL_SDK_DISABLED", configuration.Disabled.ToString()),
        };

        if (configuration.EnableAny)
        {
            _collector = new CollectorFixture();
            await _collector.StartAsync();

            var endpoint = _collector.GetBaseAddress(4318);
            var protocol = "http/protobuf";

            if (configuration.EnableAll)
            {
                config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_ENDPOINT", endpoint.ToString()));
                config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_PROTOCOL", protocol));
            }
            else
            {
                if (configuration.EnableLogs)
                {
                    config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", GetEndpoint("/v1/logs")));
                    config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_LOGS_PROTOCOL", protocol));
                }

                if (configuration.EnableMetrics)
                {
                    config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT", GetEndpoint("/v1/metrics")));
                    config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_METRICS_PROTOCOL", protocol));
                }

                if (configuration.EnableTraces)
                {
                    config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", GetEndpoint("/v1/traces")));
                    config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_TRACES_PROTOCOL", protocol));
                }

                string GetEndpoint(string path)
                    => new UriBuilder(endpoint) { Path = path }.Uri.ToString();
            }

#if DEBUG
            // Export data more frequently for easier debugging with a UI
            config.Add(KeyValuePair.Create<string, string?>("OTEL_BLRP_SCHEDULE_DELAY", "5000"));
            config.Add(KeyValuePair.Create<string, string?>("OTEL_BSP_SCHEDULE_DELAY", "5000"));
            config.Add(KeyValuePair.Create<string, string?>("OTEL_METRIC_EXPORT_INTERVAL", "5000"));
#endif
        }

        scenario.Configure(config);

        builder.Configuration.AddInMemoryCollection(config);

        scenario.Configure(builder, configuration);

        _app = builder.Build();

        scenario.Configure(_app);

        await _app.StartAsync();

        var server = _app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        _baseAddress = addresses!.Addresses
            .Select((p) => new Uri(p))
            .Last();
    }

    public async Task StopAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }

        if (_collector is not null)
        {
            await _collector.DisposeAsync();
            _collector = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            if (_app is not null)
            {
                await _app.DisposeAsync();
                _app = null;
            }

            if (_collector is not null)
            {
                await _collector.DisposeAsync();
                _collector = null;
            }
        }

        _disposed = true;
    }

    private static string? GetRepositoryPath()
    {
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(AppContext.BaseDirectory)!);

        do
        {
            string? solutionPath = Directory.EnumerateFiles(directoryInfo.FullName, "Benchmarks.slnx").FirstOrDefault();

            if (solutionPath is not null)
            {
                return Path.GetDirectoryName(solutionPath);
            }

            directoryInfo = directoryInfo.Parent;
        }
        while (directoryInfo is not null);

        return null;
    }

    private static string GetContentRoot() =>
        GetRepositoryPath() is { } repoPath ? Path.GetFullPath(Path.Join(repoPath, "perf", "Benchmarks")) : string.Empty;
}
