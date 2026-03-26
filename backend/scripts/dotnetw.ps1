param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$DotnetArgs
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendDir = Resolve-Path (Join-Path $scriptDir "..")
$localDotnet = Join-Path $backendDir ".tools/dotnet/dotnet.exe"
$channel = if ($env:DOTNET_CHANNEL) { $env:DOTNET_CHANNEL } else { "8.0" }

if (Test-Path $localDotnet) {
    & $localDotnet @DotnetArgs
    exit $LASTEXITCODE
}

$globalDotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($globalDotnet) {
    & dotnet @DotnetArgs
    exit $LASTEXITCODE
}

Write-Host "[CPMS] dotnet is not found. Installing local SDK (channel $channel)..."
& (Join-Path $scriptDir "install-dotnet-local.ps1") -Channel $channel

if (-not (Test-Path $localDotnet)) {
    Write-Error "[CPMS] Local .NET installation failed: $localDotnet not found."
    exit 1
}

& $localDotnet @DotnetArgs
exit $LASTEXITCODE
