// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Containers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public abstract class ContainerFixture : IAsyncDisposable
{
    protected abstract IContainer Container { get; }

    protected abstract string DockerfileName { get; }

    public virtual async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async Task StartAsync()
    {
        const int MaxAttempts = 3;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await this.Container.StartAsync();
                return;
            }
            catch when (attempt < MaxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }

    public Uri GetBaseAddress(int port) =>
        new UriBuilder(Uri.UriSchemeHttp, Container.Hostname, Container.GetMappedPublicPort(port)).Uri;

    protected string GetImage()
    {
        var assembly = GetType().Assembly;

        using var stream = assembly.GetManifestResourceStream(DockerfileName);
        using var reader = new StreamReader(stream!);

        var raw = reader.ReadToEnd();

        return raw[4..].Trim();
    }
}
