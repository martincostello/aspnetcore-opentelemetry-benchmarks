# ASP.NET Core and OpenTelemetry Benchmarks

[![Build status][build-badge]][build-status]

## Introduction

Benchmarks for [ASP.NET Core][aspnetcore] and [OpenTelemetry][opentelemetry-dotnet]
that illustrate the performance overhead of using OpenTelemetry for logging, metrics
and traces.

You can see the latest results of the benchmarks in the job summary [of the latest run here][results].

## Building and Running

Compiling the benchmarks yourself requires Git and the [.NET SDK][dotnet-sdk] to be installed.

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
| OpenTelemetry.Extensions.Hosting | [![OpenTelemetry.Extensions.Hosting version][badge-hosting]][package-hosting] |
| OpenTelemetry.Instrumentation.AspNetCore | [![OpenTelemetry.Instrumentation.AspNetCore version][badge-aspnetcore]][package-aspnetcore] |
| OpenTelemetry.Instrumentation.AWS | [![OpenTelemetry.Instrumentation.AWS version][badge-aws]][package-aws] |
| OpenTelemetry.Instrumentation.Http | [![OpenTelemetry.Instrumentation.Http version][badge-http]][package-http] |
| OpenTelemetry.Instrumentation.SqlClient | [![OpenTelemetry.Instrumentation.SqlClient version][badge-sqlclient]][package-sqlclient] |
| OpenTelemetry.Instrumentation.StackExchangeRedis | [![OpenTelemetry.Instrumentation.StackExchangeRedis version][badge-redis]][package-redis] |

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repo]: <https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[aspnetcore]: https://github.com/dotnet/aspnetcore
[badge-dotnet]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2Fglobal.json&query=%24.sdk.version&logo=.net&label=version
[badge-aspnetcore]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.AspNetCore'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-aws]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.AWS'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-benchmarkdotnet]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'BenchmarkDotNet'%5D%2F%40Version&logo=nuget&label=version
[badge-hosting]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Extensions.Hosting'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-http]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.Http'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-otlp]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Exporter.OpenTelemetryProtocol'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-sqlclient]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.SqlClient'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-redis]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.StackExchangeRedis'%5D%2F%40Version&logo=opentelemetry&label=version
[benchmarkdotnet]: https://benchmarkdotnet.org/
[build-badge]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/actions/workflows/build.yml/badge.svg?branch=main&event=push
[build-status]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush
[dotnet-sdk]: https://dotnet.microsoft.com/download
[issues]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/issues
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt
[opentelemetry-dotnet]: https://github.com/open-telemetry/opentelemetry-dotnet
[package-aspnetcore]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNetCore
[package-aws]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWS
[package-hosting]: https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting
[package-http]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.Http
[package-otlp]: https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol
[package-sqlclient]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.SqlClient
[package-redis]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.StackExchangeRedis
[repo]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks
[results]: https://github.com/martincostello/aspnetcore-opentelemetry-benchmarks/actions/workflows/benchmark.yml?query=branch%3Amain+event%3Apush
