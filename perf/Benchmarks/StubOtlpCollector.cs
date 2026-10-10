// Copyright (c) Martin Costello, 2026. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MartinCostello.AspNetCoreOpenTelemetry.Benchmarks;

internal sealed class StubOtlpCollector : IAsyncDisposable
{
    private const string PortArgument = "--port";
    private const string ReceiveOtlpArgument = "--receive-otlp";

    private readonly Process _process;
    private bool _disposed;

    private StubOtlpCollector(Process process, Uri endpoint)
    {
        _process = process;
        Endpoint = endpoint;
    }

    public Uri Endpoint { get; }

    public static bool IsReceiverMode(string[] args)
        => args.Contains(ReceiveOtlpArgument, StringComparer.Ordinal);

    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            int port = GetPort(args);
            var builder = WebApplication.CreateSlimBuilder();

            builder.Logging.SetMinimumLevel(LogLevel.Warning);
            builder.WebHost.UseUrls($"http://localhost:{port}");

            var app = builder.Build();

            app.Map("/{**path}", Handler);

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                cancellationToken.Register(app.Lifetime.StopApplication);
                await app.RunAsync();
            }
            finally
            {
                await app.DisposeAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(ex.ToString(), cancellationToken);
            return ex.HResult;
        }

        return 0;

        static async Task Handler([FromRoute] string? path, HttpContext context)
        {
            await context.Request.BodyReader.CopyToAsync(Stream.Null, context.RequestAborted);
            context.Response.StatusCode = StatusCodes.Status200OK;
        }
    }

    public static async Task<StubOtlpCollector> StartAsync()
    {
        int port = GetAvailablePort();
        var startInfo = CreateStartInfo(port);

        var process =
            Process.Start(startInfo) ??
            throw new InvalidOperationException("Failed to start the OTLP receiver process.");

        var receiver = new StubOtlpCollector(process, new($"http://localhost:{port}/", UriKind.Absolute));
        await receiver.WaitUntilReadyAsync();

        return receiver;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);

        try
        {
            if (!_process.HasExited)
            {
                try
                {
                    _process.Kill(entireProcessTree: true);
                    await _process.WaitForExitAsync();
                }
                catch (InvalidOperationException)
                {
                    // The process exited after HasExited was checked.
                }
            }
        }
        finally
        {
            _process.Dispose();
            _disposed = true;
        }
    }

    private static ProcessStartInfo CreateStartInfo(int port)
    {
#pragma warning disable IL3000
        string assemblyPath = typeof(StubOtlpCollector).Assembly.Location;
#pragma warning restore IL3000

        string windowsAppHostPath = Path.ChangeExtension(assemblyPath, ".exe");
        string nonWindowsAppHostPath = Path.ChangeExtension(assemblyPath, null);

        string fileName;
        string? assemblyArgument = null;

        if (File.Exists(windowsAppHostPath))
        {
            fileName = windowsAppHostPath;
        }
        else if (File.Exists(nonWindowsAppHostPath))
        {
            fileName = nonWindowsAppHostPath;
        }
        else
        {
            fileName = "dotnet";
            assemblyArgument = assemblyPath;
        }

        var startInfo = new ProcessStartInfo(fileName)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        if (assemblyArgument is not null)
        {
            startInfo.ArgumentList.Add(assemblyArgument);
        }

        startInfo.ArgumentList.Add(ReceiveOtlpArgument);
        startInfo.ArgumentList.Add(PortArgument);
        startInfo.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));

        return startInfo;
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port: 0);
        listener.Start();

        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static int GetPort(string[] args)
    {
        int index = Array.IndexOf(args, PortArgument);

        if (index < 0 || index == args.Length - 1)
        {
            return 4318; // Default OTLP port.
        }

        if (!int.TryParse(args[index + 1], CultureInfo.InvariantCulture, out int port) ||
            port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
        {
            throw new InvalidOperationException($"The OTLP receiver port '{args[index + 1]}' is invalid.");
        }

        return port;
    }

    private async Task WaitUntilReadyAsync()
    {
        using var client = new HttpClient()
        {
            BaseAddress = Endpoint,
            Timeout = TimeSpan.FromMilliseconds(250),
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        while (!cts.IsCancellationRequested)
        {
            if (_process.HasExited)
            {
                string error = await _process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException(
                    $"The OTLP receiver process exited unexpectedly with code {_process.ExitCode}.{Environment.NewLine}{error.Trim()}");
            }

            try
            {
                using var response = await client.GetAsync(string.Empty);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Wait for the listener to start.
            }
            catch (TaskCanceledException)
            {
                // Wait for the listener to start.
            }

            await Task.Delay(100);
        }

        await DisposeAsync();

        throw new TimeoutException("Timed out waiting for the OTLP receiver process to start.");
    }
}
