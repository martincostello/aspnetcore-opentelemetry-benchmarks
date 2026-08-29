// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

[BenchmarkCategory("Specification")]
[BenchmarkCategory("Traces")]
[MemoryDiagnoser]
public class SpecificationBenchmarks : IAsyncDisposable
{
    private const int ExportTimeoutMilliseconds = 30_000;
    private const int OperationsPerInvoke = 32;

    private const string ServiceName = "service-01";
    private const string ServiceVersion = "version-01";

    private static readonly ActivitySource Source = new(typeof(SpecificationBenchmarks).FullName!);

    private static readonly KeyValuePair<string, object>[] ResourceAttributes =
    [
        new("service.instance.id", Guid.NewGuid().ToString()),
        new("service.name", ServiceName),
        new("service.version", ServiceVersion),
    ];

    private static readonly KeyValuePair<string, object>[] SpanAttributes =
    [
        new("attribute-name-00001", "attribute-value-0001"),
        new("attribute-name-00002", "attribute-value-0002"),
        new("attribute-name-00003", "attribute-value-0003"),
        new("attribute-name-00004", "attribute-value-0004"),
        new("attribute-name-00005", "attribute-value-0005"),
        new("attribute-name-00006", "attribute-value-0006"),
        new("attribute-name-00007", "attribute-value-0007"),
        new("attribute-name-00008", "attribute-value-0008"),
        new("attribute-name-00009", "attribute-value-0009"),
        new("attribute-name-00010", "attribute-value-0010"),
    ];

    private StubOtlpCollector? _collector;
    private OtlpTraceExporter? _exporter;
    private TracerProvider? _provider;

    public enum ProcessorType
    {
        Simple,
        Batch,
    }

    public enum SpanProfile
    {
        Configuration,
        Throughput,
    }

    [Params(ProcessorType.Simple, ProcessorType.Batch)]
    public ProcessorType Processor { get; set; }

    [Params(SpanProfile.Configuration, SpanProfile.Throughput)]
    public SpanProfile Profile { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _collector = await StubOtlpCollector.StartAsync();

        var endpoint = new Uri(_collector.Endpoint, "v1/traces");
        _exporter = new OtlpTraceExporter(new OtlpExporterOptions()
        {
            Endpoint = endpoint,
            Protocol = OtlpExportProtocol.HttpProtobuf,
        });

        _provider = Sdk.CreateTracerProviderBuilder()
            .AddProcessor(CreateProcessor(_exporter))
            .AddSource(Source.Name)
            .ConfigureResource((p) => p.AddAttributes(ResourceAttributes))
            .SetSampler(new AlwaysOnSampler())
            .Build();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        _provider?.Dispose();
        _provider = null;

        _exporter?.Dispose();
        _exporter = null;

        if (_collector is not null)
        {
            await _collector.DisposeAsync();
            _collector = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await Cleanup();
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int CreateSpan()
    {
        for (int i = 0; i < OperationsPerInvoke; i++)
        {
            if (Profile == SpanProfile.Throughput)
            {
                CreateThroughputSpan();
            }
            else
            {
                CreateConfigurationSpan();
            }
        }

        if (!_provider!.ForceFlush(ExportTimeoutMilliseconds))
        {
            throw new InvalidOperationException("Failed to flush exported spans.");
        }

        return OperationsPerInvoke;
    }

    private static void CreateConfigurationSpan()
    {
        using var activity = Source.StartActivity("benchmark-span");

        activity!.AddEvent(new ActivityEvent("span.event"));
        activity.SetTag("span.attribute", 42L);
    }

    private static void CreateThroughputSpan()
    {
        using var activity = Source.StartActivity("benchmark-span");

        foreach (var attribute in SpanAttributes)
        {
            activity!.SetTag(attribute.Key, attribute.Value);
        }
    }

    private BaseProcessor<Activity> CreateProcessor(OtlpTraceExporter exporter) => Processor switch
    {
        ProcessorType.Batch => new BatchActivityExportProcessor(exporter),
        _ => new SimpleActivityExportProcessor(exporter),
    };
}
