// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public class SqlServerBenchmarks : Benchmarks, IScenario
{
    public override IReadOnlyCollection<ContainerFixture> Containers { get; } = [new SqlServerFixture()];

    protected override Uri Endpoint { get; } = new("/sqlserver", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
        services.AddScoped((provider) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("SqlServer");

            return new SqlConnection(connectionString);
        });
    }

    public void Configure(List<KeyValuePair<string, string?>> configuration)
    {
        var sqlServer = Containers.OfType<SqlServerFixture>().Single();
        configuration.Add(KeyValuePair.Create<string, string?>("ConnectionStrings:SqlServer", sqlServer.TypedContainer.GetConnectionString()));
    }

#pragma warning disable IL2026
    public void Configure(MeterProviderBuilder metrics)
    {
        metrics.AddSqlClientInstrumentation();
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddSqlClientInstrumentation();
    }
#pragma warning restore IL2026

    public void Configure(WebApplication app)
    {
        app.MapGet("/sqlserver", async (SqlConnection connection) =>
        {
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

            return TypedResults.Ok(tables);
        });
    }
}
