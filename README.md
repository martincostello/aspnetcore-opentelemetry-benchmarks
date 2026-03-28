# ASP.NET Core and OpenTelemetry Benchmarks

Benchmarks for ASP.NET Core and OpenTelemetry.

## Package Versions

| **Package** | **Version** |
| :------------ | :------------ |
| .NET SDK | [![.NET SDK version][badge-dotnet]][dotnet-sdk] |
| BenchmarkDotNet | [![BenchmarkDotNet version][badge-benchmarkdotnet]][benchmarkdotnet] |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | [![OpenTelemetry.Exporter.OpenTelemetryProtocol version][badge-otlp]][package-otlp] |
| OpenTelemetry.Extensions.Hosting | [![OpenTelemetry.Extensions.Hosting version][badge-hosting]][package-hosting] |
| OpenTelemetry.Instrumentation.AspNetCore | [![OpenTelemetry.Instrumentation.AspNetCore version][badge-aspnetcore]][package-aspnetcore] |
| OpenTelemetry.Instrumentation.Http | [![OpenTelemetry.Instrumentation.Http version][badge-http]][package-http] |
| OpenTelemetry.Instrumentation.SqlClient | [![OpenTelemetry.Instrumentation.SqlClient version][badge-sqlclient]][package-sqlclient] |
| OpenTelemetry.Instrumentation.StackExchangeRedis | [![OpenTelemetry.Instrumentation.StackExchangeRedis version][badge-redis]][package-redis] |

[badge-dotnet]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2Fglobal.json&query=%24.sdk.version&logo=.net&label=version
[badge-aspnetcore]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.AspNetCore'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-benchmarkdotnet]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'BenchmarkDotNet'%5D%2F%40Version&logo=nuget&label=version
[badge-hosting]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Extensions.Hosting'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-http]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.Http'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-otlp]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Exporter.OpenTelemetryProtocol'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-sqlclient]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.SqlClient'%5D%2F%40Version&logo=opentelemetry&label=version
[badge-redis]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fmartincostello%2Faspnetcore-opentelemetry-benchmarks%2Frefs%2Fheads%2Fmain%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'OpenTelemetry.Instrumentation.StackExchangeRedis'%5D%2F%40Version&logo=opentelemetry&label=version
[benchmarkdotnet]: https://benchmarkdotnet.org/
[dotnet-sdk]: https://dotnet.microsoft.com/download
[package-aspnetcore]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AspNetCore
[package-hosting]: https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting
[package-http]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.Http
[package-otlp]: https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol
[package-sqlclient]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.SqlClient
[package-redis]: https://www.nuget.org/packages/OpenTelemetry.Instrumentation.StackExchangeRedis
