// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Containers;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

public abstract class ContainerFixture<T> : ContainerFixture
    where T : IContainer
{
    public T TypedContainer
    {
        get => field ??= CreateContainer();
    }

    protected override IContainer Container => TypedContainer;

    protected abstract T CreateContainer();
}
