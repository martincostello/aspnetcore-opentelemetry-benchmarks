// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Testcontainers.MsSql;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public sealed class SqlServerFixture : ContainerFixture<MsSqlContainer>
{
    protected override string DockerfileName => "sqlserver.Dockerfile";

    protected override MsSqlContainer CreateContainer() => new MsSqlBuilder(GetImage()).Build();
}
