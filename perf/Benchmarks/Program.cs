// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

var summaries = BenchmarkSwitcher.FromAssembly(typeof(Benchmarks).Assembly).Run(args: args);
return summaries.SelectMany(p => p.Reports).Any((p) => !p.Success) ? 1 : 0;
