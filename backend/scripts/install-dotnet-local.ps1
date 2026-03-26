param(
    [string]$Channel = "8.0"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendDir = Resolve-Path (Join-Path $scriptDir "..")
$installDir = Join-Path $backendDir ".tools/dotnet"
$dotnetExe = Join-Path $installDir "dotnet.exe"
$installScript = Join-Path $scriptDir ".dotnet-install.ps1"

if (Test-Path $dotnetExe) {
    Write-Host "[CPMS] Local .NET already installed at $installDir"
    & $dotnetExe --info
    exit 0
}

Write-Host "[CPMS] Downloading dotnet-install.ps1..."
Invoke-WebRequest -UseBasicParsing -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript

Write-Host "[CPMS] Installing .NET SDK channel $Channel to $installDir..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $installScript -Channel $Channel -InstallDir $installDir -NoPath

Remove-Item $installScript -Force -ErrorAction SilentlyContinue

Write-Host "[CPMS] Local .NET installed successfully."
& $dotnetExe --info
