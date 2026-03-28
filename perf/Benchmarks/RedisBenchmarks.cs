// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public class RedisBenchmarks : Benchmarks, IScenario
{
    public override IReadOnlyCollection<ContainerFixture> Containers { get; } = [new RedisFixture()];

    protected override Uri Endpoint { get; } = new("/redis", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
        services.AddScoped((provider) => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        services.AddSingleton<IConnectionMultiplexer>((provider) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Redis");

            return ConnectionMultiplexer.Connect(connectionString!);
        });
    }

    public void Configure(List<KeyValuePair<string, string?>> configuration)
    {
        var redis = Containers.OfType<RedisFixture>().Single();
        configuration.Add(KeyValuePair.Create<string, string?>("ConnectionStrings:Redis", redis.TypedContainer.GetConnectionString()));
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddRedisInstrumentation();
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/redis", async (IDatabase database) =>
        {
            var value = await database.ListLengthAsync("list");

            return TypedResults.Ok(value);
        });
    }
}
