<#
.SYNOPSIS
    Builds Shared Upgrades in Release and packages a distribution zip.

.DESCRIPTION
    Produces dist/SharedUpgrades-<version>.zip with the layout
    BepInEx/plugins/SharedUpgrades/SharedUpgrades.dll, ready for users to extract
    directly into their game folder.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$csproj = Join-Path $root 'src\SharedUpgrades.csproj'

# Resolve dotnet: prefer PATH, fall back to standard install locations.
$dotnet = $null
$cmd = Get-Command dotnet -ErrorAction SilentlyContinue
if ($cmd) { $dotnet = $cmd.Source }
if (-not $dotnet) {
    foreach ($candidate in @(
        "$env:ProgramFiles\dotnet\dotnet.exe",
        "${env:ProgramFiles(x86)}\dotnet\dotnet.exe",
        "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe",
        "$env:USERPROFILE\.dotnet\dotnet.exe"
    )) {
        if (Test-Path $candidate) { $dotnet = $candidate; break }
    }
}
if (-not $dotnet) { throw "Could not find dotnet. Install the .NET 8 SDK from https://dotnet.microsoft.com/download" }

Write-Host "Building $Configuration..." -ForegroundColor Cyan
& $dotnet build $csproj -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)" }

# Read version straight out of the csproj so it stays in lockstep with the assembly.
[xml]$xml = Get-Content $csproj
$version = ($xml.Project.PropertyGroup | Where-Object { $_.Version }).Version
if (-not $version) { throw 'Could not read <Version> from csproj' }

$builtDll = Join-Path $root "src\bin\$Configuration\SharedUpgrades.dll"
if (-not (Test-Path $builtDll)) { throw "Built DLL not found at $builtDll" }

$dist = Join-Path $root 'dist'
$staging = Join-Path $dist 'staging\BepInEx\plugins\SharedUpgrades'
if (Test-Path (Join-Path $dist 'staging')) { Remove-Item (Join-Path $dist 'staging') -Recurse -Force }
New-Item -ItemType Directory -Path $staging -Force | Out-Null
Copy-Item $builtDll $staging
Copy-Item (Join-Path $root 'README.md') $staging
Copy-Item (Join-Path $root 'LICENSE') $staging -ErrorAction SilentlyContinue

$zip = Join-Path $dist "SharedUpgrades-$version.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $dist 'staging\BepInEx') -DestinationPath $zip
Remove-Item (Join-Path $dist 'staging') -Recurse -Force

Write-Host "Wrote $zip" -ForegroundColor Green
