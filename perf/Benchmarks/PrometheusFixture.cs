// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public sealed class PrometheusFixture : ContainerFixture<IContainer>
{
    private const string DockerInternalHost = "host.docker.internal";

    private readonly string _serviceDiscoveryFilePath;
    private readonly HashSet<string> _temporaryFiles;

    public PrometheusFixture()
    {
        _serviceDiscoveryFilePath = Path.GetTempFileName();
        _temporaryFiles = [_serviceDiscoveryFilePath];
    }

    public int TargetPort
    {
        get;
        set
        {
            field = value;
            UpdateServiceDiscoveryConfiguration(field);
        }
    }

    protected override string DockerfileName => "prometheus.Dockerfile";

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        foreach (var path in _temporaryFiles)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        GC.SuppressFinalize(this);
    }

    protected override IContainer CreateContainer()
    {
        UpdateServiceDiscoveryConfiguration(TargetPort);

        var prometheusConfigurationPath = Path.GetTempFileName();
        _temporaryFiles.Add(prometheusConfigurationPath);

        WriteConfigurationFile(prometheusConfigurationPath, CreatePrometheusConfiguration());

        return new ContainerBuilder(GetImage())
            .WithBindMount(prometheusConfigurationPath, "/etc/prometheus/prometheus.yml")
            .WithBindMount(_serviceDiscoveryFilePath, "/etc/prometheus/targets/targets.json")
            .WithCommand("--config.file=/etc/prometheus/prometheus.yml")
            .WithPortBinding(3000, assignRandomHostPort: true)
            .WithPortBinding(4317, assignRandomHostPort: true)
            .WithPortBinding(4318, assignRandomHostPort: true)
            .WithPortBinding(9090, assignRandomHostPort: true)
            .WithExtraHost(DockerInternalHost, "host-gateway")
            .Build();
    }

    private static string CreatePrometheusConfiguration() =>
        $"""
         scrape_configs:
           - job_name: "prometheus-target"
             file_sd_configs:
               - files:
                   - /etc/prometheus/targets/targets.json
                 refresh_interval: 1s
         """;

    private static string CreateServiceDiscoveryConfiguration(int port) =>
        $$"""
          [
            {
              "labels": { "job": "prometheus-target" },
              "targets": ["{{DockerInternalHost}}:{{port}}"]
            }
          ]
          """;

    private static void WriteConfigurationFile(string path, string contents, bool readOnly = true)
    {
        File.WriteAllText(path, contents);

#if NET
        if (OperatingSystem.IsLinux())
        {
            var mode = (readOnly ? UnixFileMode.UserRead : UnixFileMode.UserWrite) | UnixFileMode.GroupRead | UnixFileMode.OtherRead;
            File.SetUnixFileMode(path, mode);
        }
#endif
    }

    private void UpdateServiceDiscoveryConfiguration(int port)
        => WriteConfigurationFile(_serviceDiscoveryFilePath, CreateServiceDiscoveryConfiguration(port), readOnly: false);
}
