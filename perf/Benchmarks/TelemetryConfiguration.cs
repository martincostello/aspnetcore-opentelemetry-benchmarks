// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public sealed record TelemetryConfiguration(bool EnableLogs, bool EnableMetrics, bool EnableTraces)
{
    public static TelemetryConfiguration None { get; } = new(false, false, false);

    public static TelemetryConfiguration All { get; } = new(true, true, true);

    public static TelemetryConfiguration Logs { get; } = new(EnableLogs: true, false, false);

    public static TelemetryConfiguration Metrics { get; } = new(false, EnableMetrics: true, false);

    public static TelemetryConfiguration Traces { get; } = new(false, false, EnableTraces: true);

    public bool Disabled => !EnableLogs && !EnableMetrics && !EnableTraces;

    public bool EnableAll => EnableLogs && EnableMetrics && EnableTraces;

    public bool EnableAny => EnableLogs || EnableMetrics || EnableTraces;
}
