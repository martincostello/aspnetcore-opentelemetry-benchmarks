# ASP.NET Core and OpenTelemetry Benchmarks

[![Build status][build-badge]][build-status]

## Introduction

Benchmarks for [ASP.NET Core][aspnetcore] and [OpenTelemetry][opentelemetry-dotnet]
that illustrate the performance overhead of using OpenTelemetry for logging, metrics
and traces.

You can see the latest results of the benchmarks in the job summary [of the latest run that can be found here][results].

> [!NOTE]
> These benchmarks are unofficial and are not affiliated with or endorsed by the OpenTelemetry project.

## Rationale

The benchmarks in this project are intended to provide a realistic example of the end-to-end
performance overhead of using OpenTelemetry in an ASP.NET Core application. The benchmarks are not
intended to be a comprehensive performance analysis of OpenTelemetry, but rather to provide a simple
comparison of the overhead of using different combinations of OpenTelemetry telemetry signals
(logs, metrics, traces) in a typical ASP.NET Core application for real workloads.

These benchmarks are not intended to be microbenchmarks of the individual OpenTelemetry components,
such microbenchmarks are left to the respective OpenTelemetry projects themselves.

A number of the benchmarks include instrumentation for remote services, such as AWS, SQL Server and Redis.
These are included to provide a more realistic example of the overhead of using OpenTelemetry in a typical
application, albeit run locally as Docker containers.

Where telemetry signals are enabled, the benchmarks are configured to export telemetry data to a
local [OpenTelemetry Collector][opentelemetry-collector] instance (using [grafana/otel-lgtm][grafana-lgtm])
with the OpenTelemetry Protocol (OTLP) exporter. This is intended to provide a realistic example of the
overhead of exporting telemetry data to a remote service, such as [Grafana Cloud][grafana-cloud] or
[Prometheus][prometheus].

Due to this external dependency, the collector configuration could induce backpressure on the SDK exporter
during benchmarks and skew the results. This side-effect is accepted as part of the benchmarks, as it is
intended to mirror a real application's configuration rather than to solely measure the OpenTelemetry-related code.

## Implementation

The benchmarks are implemented using [BenchmarkDotNet][benchmarkdotnet] as a .NET console application.
The benchmarks self-host an ASP.NET Core application using Kestrel and make HTTP requests to the application
using the `HttpClient` class. Each benchmark implements a "scenario" that exercises different code paths
that affect different common use cases for a web application.

Each scenario is run for 5 benchmarks that cover no telemetry, logs only, metrics only, traces only and all
three telemetry signals enabled. The benchmark results can then be used to infer the overhead of each telemetry
signal type for the workload that a particular scenario exercises.

When one or more telemetry signals are enabled, the benchmarks are configured to export telemetry to a local
OpenTelemetry Collector instance running in Docker using the OpenTelemetry Protocol (OTLP) exporter.

Additional scenarios that depend on other external services such as AWS, SQL Server and Redis are also included.
These scenarios similarly run those workloads as container images locally in Docker.

The current scenarios included in the benchmarks are:

| **Scenario** | **Description** |
| :------------ | :------------ |
| Default | Implements a no-op endpoint with no additional instrumentation. |
| Logs | Logs a custom log message using [`ILogger`][ilogger]. |
| Metrics | Increments a custom metric using [`Counter<int>`][counter]. |
| Traces | Creates a custom [`Activity`][activity] using [`ActivitySource`][activitysource]. |
| ASP.NET Core | Implements a no-op endpoint with [ASP.NET Core instrumentation][aspnetcore-instrumentation] enabled. |
| AWS | Uses the AWS S3 SDK with [AWS instrumentation][aws-instrumentation] enabled. |
| EFCore | Executes an SQL query using [`EFCore`][efcore] with [EFCore instrumentation][efcore-instrumentation] enabled. |
| HTTP Client | Performs a loopback request to itself using [`HttpClient`][httpclient] with [HTTP instrumentation][http-instrumentation] enabled. |
| Prometheus | Uses a [Prometheus][prometheus] exporter for metrics instead of OTLP. |
| Redis | Pings a Redis instance using [StackExchange.Redis][stackexchange.redis] with [Redis instrumentation][redis-instrumentation] enabled. |
| SQL Server | Executes an SQL query using [`SqlClient`][sqlclient] with [SQL Client instrumentation][sqlclient-instrumentation] enabled. |
| Kitchen Sink | A scenario that combines all of the above scenarios (except Prometheus). |

## Building and Running

Compiling the benchmarks yourself requires [Docker][docker], Git and the [.NET SDK][dotnet-sdk] to be installed.

To run the benchmarks locally from a terminal/command-line, run the
following set of commands:

```terminal
git clone https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks.git
cd aspnetcore-opentelemetry-benchmarks
./benchmark.ps1
```

## Package Versions

| **Package** | **Version** |
| :------------ | :------------ |
| .NET SDK | [![.NET SDK version][badge-dotnet]][dotnet-sdk] |
| BenchmarkDotNet | [![BenchmarkDotNet version][badge-benchmarkdotnet]][benchmarkdotnet] |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | [![OpenTelemetry.Exporter.OpenTelemetryProtocol version][badge-otlp]][package-otlp] |
| OpenTelemetry.Exporter.Prometheus.AspNetCore | [![OpenTelemetry.Exporter.Prometheus.AspNetCore version][badge-prometheus]][package-prometheus] |
| OpenTelemetry.Extensions.Hosting | [![OpenTelemetry.Extensions.Hosting version][badge-hosting]][package-hosting] |
| OpenTelemetry.Instrumentation.AspNetCore | [![OpenTelemetry.Instrumentation.AspNetCore version][badge-aspnetcore]][package-aspnetcore] |
| OpenTelemetry.Instrumentation.AWS | [![OpenTelemetry.Instrumentation.AWS version][badge-aws]][package-aws] |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | [![OpenTelemetry.Instrumentation.EntityFrameworkCore version][badge-efcore]][package-efcore] |
| OpenTelemetry.Instrumentation.Http | [![OpenTelemetry.Instrumentation.Http version][badge-http]][package-http] |
| OpenTelemetry.Instrumentation.SqlClient | [![OpenTelemetry.Instrumentation.SqlClient version][badge-sqlclient]][package-sqlclient] |
| OpenTelemetry.Instrumentation.StackExchangeRedis | [![OpenTelemetry.Instrumentation.StackExchangeRedis version][badge-redis]][package-redis] |

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repo]: <https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[activity]: https://learn.microsoft.com/dotnet/api/system.diagnostics.activity
[activitysource]: https://learn.microsoft.com/dotnet/api/system.diagnostics.activitysource
[aspnetcore]: https://github.com/dotnet/aspnetcore
[aws-instrumentation]: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.AWS#readme
[aspnetcore-instrumentation]: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.AspNetCore#readme
[badge-dotnet]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2Fglobal.json&query=%24.sdk.version&logo=.net&label=version
[badge-aspnetcore]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.AspNetCore'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-aws]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.AWS'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-benchmarkdotnet]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'BenchmarkDotNet'%5D%2F%40Version&logo=nuget&label=version
[badge-efcore]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.EntityFrameworkCore'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-hosting]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Extensions.Hosting'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-http]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.Http'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-otlp]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Exporter.OpenTelemetryProtocol'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-prometheus]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Exporter.Prometheus.AspNetCore'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-sqlclient]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.SqlClient'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-redis]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.StackExchangeRedis'%5D%2F%40Version&logo=opentelemetry&label=version
[benchmarkdotnet]: https://benchmarkdotnet.org/
[build-badge]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/actions/workflows/build.yml/badge.svg?branch=main&event=push
[build-status]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush
[counter]: https://learn.microsoft.com/dotnet/api/system.diagnostics.metrics.counter-1
[docker]: https://docs.docker.com/get-started/
[dotnet-sdk]: https://dotnet.microsoft.com/download
[efcore]: https://learn.microsoft.com/ef/core/
[efcore-instrumentation]: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.EntityFrameworkCore#readme
[grafana-cloud]: https://grafana.com/solutions/opentelemetry/
[grafana-lgtm]: https://github.com/grafana/docker-otel-lgtm
[http-instrumentation]: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.Http#readme
[httpclient]: https://learn.microsoft.com/dotnet/api/system.net.http.httpclient
[ilogger]: https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.ilogger
[issues]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/issues
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt
[opentelemetry-collector]: https://github.com/open-telemetry/opentelemetry-collector
[opentelemetry-dotnet]: https://github.com/open-telemetry/opentelemetry-dotnet
[package-aspnetcore]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNetCore
[package-aws]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWS
[package-efcore]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.EntityFrameworkCore
[package-hosting]: https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting
[package-http]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.Http
[package-otlp]: https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol
[package-prometheus]: https://www.nuget.org/packages/OpenTelemetry.Exporter.Prometheus.AspNetCore
[package-sqlclient]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.SqlClient
[package-redis]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.StackExchangeRedis
[prometheus]: https://prometheus.io/docs/guides/opentelemetry/
[redis-instrumentation]: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.StackExchangeRedis#readme
[repo]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks
[results]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/actions/workflows/benchmark.yml?query=branch%3Amain+event%3Apush
[sqlclient]: https://github.com/dotnet/sqlclient
[sqlclient-instrumentation]: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.SqlClient#readme
[stackexchange.redis]: https://www.nuget.org/packages/StackExchange.Redis/
