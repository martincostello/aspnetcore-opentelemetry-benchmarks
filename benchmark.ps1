#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][string] $Filter = "*",
    [Parameter(Mandatory = $false)][string] $Job = "",
    [Parameter(Mandatory = $false)][switch] $EnableProfiler
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

$installDotNetSdk = $false

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Information "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Information "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk) {
    ${env:DOTNET_INSTALL_DIR} = Join-Path $PSScriptRoot ".dotnet"
    $sdkPath = Join-Path ${env:DOTNET_INSTALL_DIR} "sdk" $dotnetVersion

    if (-Not (Test-Path $sdkPath)) {
        if (-Not (Test-Path ${env:DOTNET_INSTALL_DIR})) {
            mkdir ${env:DOTNET_INSTALL_DIR} | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        if (($PSVersionTable.PSVersion.Major -ge 6) -And (-Not $IsWindows)) {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --jsonfile $sdkFile --install-dir ${env:DOTNET_INSTALL_DIR} --no-path --skip-non-versioned-files
        }
        else {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -JsonFile $sdkFile -InstallDir ${env:DOTNET_INSTALL_DIR} -NoPath -SkipNonVersionedFiles
        }
    }
}
else {
    ${env:DOTNET_INSTALL_DIR} = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path ${env:DOTNET_INSTALL_DIR} "dotnet"

if ($installDotNetSdk) {
    $separator = $IsWindows ? ";" : ":"
    ${env:PATH} = "${env:DOTNET_INSTALL_DIR}${separator}${env:PATH}"
}

$benchmarks = (Join-Path $solutionPath "perf" "Benchmarks" "Benchmarks.csproj")

Write-Information "Running benchmarks..."

$additionalArgs = @(
    "--consumeTasksSynchronously" # For backwards compatibility with BenchmarkDotNet versions before 0.16.0
)

if (-Not [string]::IsNullOrEmpty($Filter)) {
    $additionalArgs += "--filter"
    $additionalArgs += $Filter
}

if (-Not [string]::IsNullOrEmpty($Job)) {
    $additionalArgs += "--job"
    $additionalArgs += $Job
}

if (-Not [string]::IsNullOrEmpty(${env:GITHUB_SHA})) {
    $additionalArgs += "--exporters"
    $additionalArgs += "json"
}

if ($EnableProfiler) {
    $additionalArgs += "--profiler"
    $additionalArgs += "EP"
}

if ($installDotNetSdk) {
    $additionalArgs += "--cli"
    $additionalArgs += $dotnet
}

$dotnetArgs = @(
    "run"
    "--project", $benchmarks
    "--configuration", "Release"
    "--"
) + $additionalArgs

$p = Start-Process -FilePath $dotnet -ArgumentList $dotnetArgs -NoNewWindow -Wait -PassThru

if ($p.ExitCode -ne 0) {
    throw "Benchmarks failed with exit code $($p.ExitCode)."
}
