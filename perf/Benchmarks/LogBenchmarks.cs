// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public partial class LogBenchmarks : Benchmarks, IScenario
{
    protected override Uri Endpoint { get; } = new("/logs", UriKind.Relative);

    public void Configure(WebApplication app)
    {
        app.MapGet("/logs", () =>
        {
            var result = Random.Shared.Next(1, 7);

            Log.DiceRoll(app.Logger, result);

            return TypedResults.Ok(result.ToString(CultureInfo.InvariantCulture));
        });
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Rolled a {Value}.")]
        public static partial void DiceRoll(ILogger logger, int value);
    }
}
