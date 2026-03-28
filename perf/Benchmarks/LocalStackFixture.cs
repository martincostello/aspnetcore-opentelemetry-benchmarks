// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Testcontainers.LocalStack;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public sealed class LocalStackFixture : ContainerFixture<LocalStackContainer>
{
    protected override string DockerfileName => "localstack.Dockerfile";

    protected override LocalStackContainer CreateContainer() => new LocalStackBuilder(GetImage()).Build();
}
