param(
    [string]$MySqlConnectionString = "",
    [switch]$SkipRestore
)

$ErrorActionPreference = "Stop"

function Get-MySqlConnectionStringFromLocalConfig {
    param(
        [string]$BackendRoot
    )

    $candidateFiles = @(
        (Join-Path $BackendRoot "src/ConstructionPayment.Api/appsettings.Development.local.json"),
        (Join-Path $BackendRoot "src/ConstructionPayment.Api/appsettings.local.json")
    )

    foreach ($filePath in $candidateFiles) {
        if (-not (Test-Path $filePath)) {
            continue
        }

        try {
            $json = Get-Content -Raw -Path $filePath | ConvertFrom-Json
            $value = $json.ConnectionStrings.MySqlConnection
            if ($value -and -not [string]::IsNullOrWhiteSpace([string]$value)) {
                return [string]$value
            }
        }
        catch {
            Write-Warning "[CPMS] Khong doc duoc file config local: $filePath ($($_.Exception.Message))"
        }
    }

    return $null
}

function Stop-ProcessSafe {
    param(
        [int]$ProcessId,
        [string]$Reason
    )

    if ($ProcessId -le 0) {
        return
    }

    try {
        Stop-Process -Id $ProcessId -Force -ErrorAction Stop
        Write-Host "[CPMS] Da dung process $ProcessId ($Reason)."
    }
    catch {
        Write-Warning "[CPMS] Khong the dung process $ProcessId ($Reason): $($_.Exception.Message)"
    }
}

function Stop-ConflictingProcesses {
    # Giai phong port app de tranh loi bind/address-in-use.
    try {
        $portOwners = Get-NetTCPConnection -LocalPort 5000 -State Listen -ErrorAction Stop |
            Select-Object -ExpandProperty OwningProcess -Unique

        foreach ($ownerPid in $portOwners) {
            Stop-ProcessSafe -ProcessId $ownerPid -Reason "dang chiem port 5000"
        }
    }
    catch {
        # Khong co process nao dang listen port 5000.
    }

    # Dung cac process API cu de tranh lock apphost.exe khi build.
    $apiProcesses = Get-Process -Name "ConstructionPayment.Api" -ErrorAction SilentlyContinue
    foreach ($apiProcess in $apiProcesses) {
        Stop-ProcessSafe -ProcessId $apiProcess.Id -Reason "dang giu ConstructionPayment.Api.exe"
    }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendDir = Resolve-Path (Join-Path $scriptDir "..")

$resolvedConnectionString = $MySqlConnectionString
if (-not $resolvedConnectionString) {
    $resolvedConnectionString = $env:ConnectionStrings__MySqlConnection
}
if (-not $resolvedConnectionString) {
    $resolvedConnectionString = Get-MySqlConnectionStringFromLocalConfig -BackendRoot $backendDir
}

if (-not $resolvedConnectionString) {
    throw "Thieu MySqlConnectionString. Hay truyen tham so -MySqlConnectionString, dat env ConnectionStrings__MySqlConnection, hoac tao file backend/src/ConstructionPayment.Api/appsettings.Development.local.json."
}

if ($resolvedConnectionString.Contains("<PASSWORD>")) {
    throw "MySqlConnectionString vẫn chứa <PASSWORD>. Vui lòng thay bằng mật khẩu thật."
}

# Tránh giá trị env cũ ghi đè.
Remove-Item Env:ConnectionStrings__MySqlConnection -ErrorAction SilentlyContinue
Remove-Item Env:DatabaseProvider -ErrorAction SilentlyContinue

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DatabaseProvider = "MySql"
$env:Database__SeedDemoData = "true"
$env:Database__AllowSqliteFallbackInDevelopment = "false"
$env:ConnectionStrings__MySqlConnection = $resolvedConnectionString
$env:ASPNETCORE_URLS = "http://localhost:5000"

Push-Location $backendDir
try {
    Stop-ConflictingProcesses

    if (-not $SkipRestore) {
        & (Join-Path $scriptDir "dotnetw.ps1") restore ConstructionPayment.sln
    }

    & (Join-Path $scriptDir "dotnetw.ps1") run --project src/ConstructionPayment.Api/ConstructionPayment.Api.csproj -- --bootstrap-db
    Write-Host "[CPMS] TiDB bootstrap thành công (code-first migrate + seed demo data)."
}
finally {
    Pop-Location
}
