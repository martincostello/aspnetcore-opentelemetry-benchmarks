// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public class AwsBenchmarks : Benchmarks, IScenario
{
    public override IReadOnlyCollection<ContainerFixture> Containers { get; } = [new LocalStackFixture()];

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
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_ACCESS_KEY_ID", "localstack"));
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_SECRET_ACCESS_KEY", "localstack"));
        configuration.Add(KeyValuePair.Create<string, string?>("AWS_REGION", "us-east-1"));

        var localStack = Containers.OfType<LocalStackFixture>().Single();
        var endpoint = localStack.GetBaseAddress(4566).ToString();

        configuration.Add(KeyValuePair.Create<string, string?>("AWS_ENDPOINT_URL_S3", endpoint));
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddAWSInstrumentation();
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/s3", async (IAmazonS3 client) =>
        {
            var response = await client.ListBucketsAsync();

            var buckets = response.Buckets?.Select((p) => p.BucketName).ToArray() ?? [];

            return TypedResults.Ok(buckets);
        });
    }
}
