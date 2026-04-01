// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

#if ENABLE_CPU_SAMPLING
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
#endif
[MemoryDiagnoser]
public abstract class Benchmarks : IAsyncDisposable, IScenario
{
    private AppServer? _app = new();
    private HttpClient? _client;
    private bool _disposed;

    public virtual IReadOnlyCollection<ContainerFixture> Containers => [];

    protected abstract Uri Endpoint { get; }

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

    [Benchmark(Baseline = true)]
    public async Task<byte[]> Baseline()
        => await _client!.GetByteArrayAsync(Endpoint);

    [Benchmark]
    public async Task<byte[]> Logs()
        => await _client!.GetByteArrayAsync(Endpoint);

    [Benchmark]
    public async Task<byte[]> Metrics()
        => await _client!.GetByteArrayAsync(Endpoint);

    [Benchmark]
    public async Task<byte[]> Traces()
        => await _client!.GetByteArrayAsync(Endpoint);

    [Benchmark]
    public async Task<byte[]> AllTelemetry()
        => await _client!.GetByteArrayAsync(Endpoint);

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

    private async Task StartServer(TelemetryConfiguration configuration)
    {
        if (_app is not null)
        {
            await _app.StartAsync(this, configuration);
            _client = _app.CreateHttpClient();
        }
    }
}
