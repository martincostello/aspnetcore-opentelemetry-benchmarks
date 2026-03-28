// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
public abstract class Benchmarks : IAsyncDisposable, IScenario
{
    private AppServer? _app = new();
    private HttpClient? _client;
    private bool _disposed;

    public virtual IReadOnlyCollection<ContainerFixture> Containers => [];

    protected abstract Uri Endpoint { get; }

    [GlobalSetup(Target = nameof(Baseline))]
    public Task StartServerWithoutTelemetry() => StartServer(enableTelemetry: false);

    [GlobalSetup(Target = nameof(Telemetry))]
    public Task StartServerWithTelemetry() => StartServer(enableTelemetry: true);

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
    public async Task<byte[]> Telemetry()
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

    private async Task StartServer(bool enableTelemetry)
    {
        if (_app is not null)
        {
            await _app.StartAsync(this, enableTelemetry);
            _client = _app.CreateHttpClient();
        }
    }
}
