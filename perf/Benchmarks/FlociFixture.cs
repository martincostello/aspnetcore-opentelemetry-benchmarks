// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public sealed class FlociFixture : ContainerFixture<IContainer>
{
    protected override string DockerfileName => "floci.Dockerfile";

    protected override IContainer CreateContainer() =>
        new ContainerBuilder(GetImage())
            .WithPortBinding(4566, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(4566))
            .Build();
}
