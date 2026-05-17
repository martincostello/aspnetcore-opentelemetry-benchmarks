// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

#if ENABLE_CPU_SAMPLING
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
#endif
[MemoryDiagnoser]
public abstract class Benchmarks : IAsyncDisposable, IScenario
{
    private const int OperationsPerInvoke = 32;
    private const int WarmupRequestCount = 3;

    private AppServer? _app = new();
    private HttpClient? _client;
    private bool _disposed;

    public virtual IReadOnlyCollection<ContainerFixture> Containers => [];

    protected abstract Uri Endpoint { get; }

    protected Uri BaseAddress => _app?.BaseAddress ?? throw new InvalidOperationException("The server has not started.");

    protected IServiceProvider Services => _app?.Services ?? throw new InvalidOperationException("The server has not started.");

    [GlobalSetup(Target = nameof(Baseline))]
    public Task StartServerNoTelemetry() => StartServer(TelemetryConfiguration.None);

    [GlobalSetup(Target = nameof(Logs))]
    public Task StartServerWithLogs() => StartServer(TelemetryConfiguration.Logs);

    [GlobalSetup(Target = nameof(Metrics))]
    public Task StartServerWithMetrics() => StartServer(TelemetryConfiguration.Metrics);

    [GlobalSetup(Target = nameof(Traces))]
    public Task StartServerWithTraces() => StartServer(TelemetryConfiguration.Traces);

    [GlobalSetup(Target = nameof(AllTelemetry))]
    public Task StartServerWithAllTelemetry() => StartServer(TelemetryConfiguration.All);

    [GlobalCleanup]
    public async Task StopServer()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            _app = null;
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    [BenchmarkCategory("Baseline")]
    public ConfiguredTaskAwaitable<int> Baseline()
        => SendRequestsAsync().ConfigureAwait(false);

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    [BenchmarkCategory("Logs")]
    public ConfiguredTaskAwaitable<int> Logs()
        => SendRequestsAsync().ConfigureAwait(false);

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    [BenchmarkCategory("Metrics")]
    public ConfiguredTaskAwaitable<int> Metrics()
        => SendRequestsAsync().ConfigureAwait(false);

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    [BenchmarkCategory("Traces")]
    public ConfiguredTaskAwaitable<int> Traces()
        => SendRequestsAsync().ConfigureAwait(false);

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    [BenchmarkCategory("Logs")]
    [BenchmarkCategory("Metrics")]
    [BenchmarkCategory("Traces")]
    public ConfiguredTaskAwaitable<int> AllTelemetry()
        => SendRequestsAsync().ConfigureAwait(false);

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            _client?.Dispose();
            _client = null;

            if (_app is not null)
            {
                await _app.DisposeAsync();
                _app = null;
            }

            foreach (var container in Containers)
            {
                await container.DisposeAsync();
            }
        }

        _disposed = true;
    }

    protected virtual HttpClient CreateHttpClient(AppServer app) => app.CreateHttpClient();

    protected virtual async Task StartServer(TelemetryConfiguration configuration)
    {
        if (_app is not null)
        {
            await _app.StartAsync(this, configuration);
            _client = CreateHttpClient(_app);
            await OnServerStartedAsync();
            await WarmupAsync();
        }
    }

    protected virtual Task OnServerStartedAsync() => Task.CompletedTask;

    private async Task<int> SendRequestsAsync()
    {
        int statusCode = 0;

        for (int i = 0; i < OperationsPerInvoke; i++)
        {
            using var response = await _client!.GetAsync(Endpoint, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            statusCode += (int)response.StatusCode;
        }

        return statusCode;
    }

    private async Task WarmupAsync()
    {
        for (int i = 0; i < WarmupRequestCount; i++)
        {
            using var response = await _client!.GetAsync(Endpoint, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
        }
    }
}
