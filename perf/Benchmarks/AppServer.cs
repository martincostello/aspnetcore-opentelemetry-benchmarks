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
            MaxAutomaticRedirections = 0,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };

        return new(handler, disposeHandler: true)
        {
            BaseAddress = _baseAddress,
        };
    }

    public async Task StartAsync(IScenario scenario, bool enableTelemetry)
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

        builder.WebHost.UseUrls("https://127.0.0.1:0");

        var config = new List<KeyValuePair<string, string?>>()
        {
            KeyValuePair.Create<string, string?>("OTEL_SDK_DISABLED", (!enableTelemetry).ToString()),
        };

        if (enableTelemetry)
        {
            _collector = new CollectorFixture();
            await _collector.StartAsync();

            var container = _collector.TypedContainer;
            var endpoint = _collector.GetBaseAddress(4318);

            config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_ENDPOINT", endpoint.ToString()));
            config.Add(KeyValuePair.Create<string, string?>("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf"));

#if DEBUG
            // Export data more frequently for easier debugging with a UI
            config.Add(KeyValuePair.Create<string, string?>("OTEL_BLRP_SCHEDULE_DELAY", "5000"));
            config.Add(KeyValuePair.Create<string, string?>("OTEL_BSP_SCHEDULE_DELAY", "5000"));
            config.Add(KeyValuePair.Create<string, string?>("OTEL_METRIC_EXPORT_INTERVAL", "5000"));
#endif
        }

        scenario.Configure(config);

        builder.Configuration.AddInMemoryCollection(config);

        scenario.Configure(builder);

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
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(typeof(AppServer).Assembly.Location)!);

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
