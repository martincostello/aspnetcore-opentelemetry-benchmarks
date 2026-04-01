// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public partial class KitchenSinkBenchmarks : Benchmarks, IScenario
{
    public override IReadOnlyCollection<ContainerFixture> Containers { get; } =
    [
        new LocalStackFixture(),
        new RedisFixture(),
        new SqlServerFixture(),
    ];

    protected override Uri Endpoint { get; } = new("/everything", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<MetricBenchmarks.CustomMetrics>();

        services.AddSingleton<IAmazonS3>((provider) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var credentials = new Amazon.Runtime.BasicAWSCredentials(
                configuration["AWS_ACCESS_KEY_ID"],
                configuration["AWS_SECRET_ACCESS_KEY"]);

            var config = new AmazonS3Config()
            {
                ForcePathStyle = true,
                ServiceURL = configuration["AWS_ENDPOINT_URL_S3"],
            };

            return new AmazonS3Client(credentials, config);
        });

        services.AddScoped((provider) => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        services.AddSingleton<IConnectionMultiplexer>((provider) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Redis");

            return ConnectionMultiplexer.Connect(connectionString!);
        });

        services.AddScoped((provider) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("SqlServer");

            return new SqlConnection(connectionString);
        });
    }

    public void Configure(List<KeyValuePair<string, string?>> configuration)
    {
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_ACCESS_KEY_ID", "localstack"));
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_SECRET_ACCESS_KEY", "localstack"));
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_REGION", "us-east-1"));

        var localStack = Containers.OfType<LocalStackFixture>().Single();
        var endpoint = localStack.GetBaseAddress(4566).ToString();

        configuration.Add(KeyValuePair.Create<string, string?>("AWS_ENDPOINT_URL_S3", endpoint));

        var redis = Containers.OfType<RedisFixture>().Single();
        configuration.Add(KeyValuePair.Create<string, string?>("ConnectionStrings:Redis", redis.TypedContainer.GetConnectionString()));
    }

    public void Configure(MeterProviderBuilder metrics)
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddSqlClientInstrumentation()
               .AddMeter(MetricBenchmarks.MeterName);
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddAWSInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRedisInstrumentation()
               .AddSqlClientInstrumentation()
               .AddSource(TraceBenchmarks.CustomSource.Name);
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/everything", async (IAmazonS3 client, Microsoft.AspNetCore.Hosting.Server.IServer server, HttpClient httpClient, MetricBenchmarks.CustomMetrics metrics, IDatabase database, SqlConnection connection) =>
        {
            using var activity = TraceBenchmarks.CustomSource.StartActivity("CustomActivity");
            activity?.SetTag("custom.trace.tag", "value");

            var serverAddresses = server.Features.Get<IServerAddressesFeature>();
            var baseAddress = serverAddresses!.Addresses.Select((p) => new Uri(p)).Last();
            var requestUri = new Uri(baseAddress, "/echo");

            var response = await httpClient.GetStringAsync(requestUri);

            _ = await client.ListBucketsAsync();

            var result = Random.Shared.Next(1, 7);

            Log.DiceRoll(app.Logger, result);

            metrics.Increment();

            _ = await database.ListLengthAsync("list");

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";

            var tables = new List<string>();

            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            return TypedResults.Ok();
        });
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Rolled a {Value}.")]
        public static partial void DiceRoll(ILogger logger, int value);
    }
}
