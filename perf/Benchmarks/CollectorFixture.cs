// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public sealed class CollectorFixture : ContainerFixture<IContainer>
{
    protected override string DockerfileName => "lgtm.Dockerfile";

    protected override IContainer CreateContainer() =>
        new ContainerBuilder(GetImage())
            .WithPortBinding(3000)
            .WithPortBinding(4317)
            .WithPortBinding(4318)
            .WithPortBinding(9090)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(3000)))
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(4317))
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(4318))
            .Build();
}
