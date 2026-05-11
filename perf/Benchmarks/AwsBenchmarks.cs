// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

[BenchmarkCategory("AWS")]
public class AwsBenchmarks : Benchmarks, IScenario
{
    private const string BucketName = "benchmarks";

    public override IReadOnlyCollection<ContainerFixture> Containers { get; } = [new FlociFixture()];

    protected override Uri Endpoint { get; } = new("/s3", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
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
    }

    public void Configure(List<KeyValuePair<string, string?>> configuration)
    {
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_ACCESS_KEY_ID", "floci"));
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_SECRET_ACCESS_KEY", "floci"));
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_REGION", "us-east-1"));

        var floci = Containers.OfType<FlociFixture>().Single();
        var endpoint = floci.GetBaseAddress(4566).ToString();

        configuration.Add(KeyValuePair.Create<string, string?>("AWS_ENDPOINT_URL_S3", endpoint));
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddAWSInstrumentation();
    }

    public void Configure(WebApplication app, TelemetryConfiguration configuration)
    {
        app.MapGet("/s3", async (IAmazonS3 client) =>
        {
            _ = await client.GetBucketLocationAsync(BucketName);

            return TypedResults.NoContent();
        });
    }

    protected override async Task OnServerStartedAsync()
    {
        var client = Services.GetRequiredService<IAmazonS3>();
        await client.PutBucketAsync(new PutBucketRequest() { BucketName = BucketName });
    }
}
