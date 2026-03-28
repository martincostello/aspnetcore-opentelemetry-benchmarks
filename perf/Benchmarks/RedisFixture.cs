// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Testcontainers.Redis;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public sealed class RedisFixture : ContainerFixture<RedisContainer>
{
    protected override string DockerfileName => "redis.Dockerfile";

    protected override RedisContainer CreateContainer() => new RedisBuilder(GetImage()).Build();
}
