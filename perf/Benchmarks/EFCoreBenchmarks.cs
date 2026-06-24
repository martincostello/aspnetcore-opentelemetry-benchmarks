// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

[BenchmarkCategory("EFCore")]
public class EFCoreBenchmarks : Benchmarks, IScenario
{
    public override IReadOnlyCollection<ContainerFixture> Containers { get; } = [new SqlServerFixture()];

    protected override Uri Endpoint { get; } = new("/efcore", UriKind.Relative);

    public void Configure(IServiceCollection services)
    {
        services.AddDbContext<BenchmarkContext>((provider, options) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            options.UseSqlServer(configuration.GetConnectionString("EFCore"));
        });
    }

    public void Configure(List<KeyValuePair<string, string?>> configuration)
    {
        var sqlServer = Containers.OfType<SqlServerFixture>().Single();
        configuration.Add(KeyValuePair.Create<string, string?>("ConnectionStrings:EFCore", sqlServer.TypedContainer.GetConnectionString()));
    }

    public void Configure(TracerProviderBuilder tracing)
    {
        tracing.AddEntityFrameworkCoreInstrumentation();
    }

    public void Configure(WebApplication app)
    {
        using (var context = app.Services.GetRequiredService<BenchmarkContext>())
        {
#pragma warning disable IL3050
            context.Database.EnsureCreated();
#pragma warning restore IL3050
        }

        app.MapGet("/efcore", async ([FromServices] BenchmarkContext context) =>
        {
            var items = await context.Items.OrderBy((p) => p.Name).ToListAsync();
            return TypedResults.Ok(items.Count);
        });
    }

    public sealed class BenchmarkItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Not a concern here.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Not a concern here.")]
    public sealed class BenchmarkContext(DbContextOptions<BenchmarkContext> options)
        : DbContext(options)
    {
        public DbSet<BenchmarkItem> Items { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                SeedItems(context.Set<BenchmarkItem>());
                await context.SaveChangesAsync(cancellationToken);
            });

            optionsBuilder.UseSeeding((context, _) =>
            {
                SeedItems(context.Set<BenchmarkItem>());
                context.SaveChanges();
            });

            static void SeedItems(DbSet<BenchmarkItem> set)
            {
                set.AddRange(
                    new() { Name = "Alpha" },
                    new() { Name = "Beta" },
                    new() { Name = "Gamma" });
            }
        }
    }
}
