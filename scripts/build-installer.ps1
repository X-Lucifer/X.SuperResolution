param(
    [string]$Configuration = "Release",
    [switch]$SkipFullPublish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptRoot = $PSScriptRoot
$RepoRoot = (Resolve-Path -LiteralPath (Join-Path $ScriptRoot "..")).Path
$FullPublishScript = Join-Path $ScriptRoot "publish-win-x64-full.ps1"
$NsisScript = Join-Path $RepoRoot "installer\X.SuperResolution.nsi"
$NsisScriptRelative = "installer\X.SuperResolution.nsi"
$InstallerOutputDir = Join-Path $RepoRoot "artifacts\installer"
$FinalInstallerName = "X.SuperResolution-win-x64.exe"
$FinalInstallerPath = Join-Path $InstallerOutputDir $FinalInstallerName

function Require-Command {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        throw "Required command '$Name' was not found in PATH."
    }

    return $command.Source
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $LASTEXITCODE`: $FilePath $($Arguments -join ' ')"
    }
}

if (-not (Test-Path -LiteralPath $NsisScript)) {
    throw "NSIS script not found: $NsisScript"
}

if (-not $SkipFullPublish) {
    if (-not (Test-Path -LiteralPath $FullPublishScript)) {
        throw "Full publish script not found: $FullPublishScript"
    }

    Write-Host "Building full self-contained publish output for installer..."
    & $FullPublishScript -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Full publish script failed with exit code $LASTEXITCODE."
    }
}

$MakeNsis = Require-Command "makensis"
New-Item -ItemType Directory -Path $InstallerOutputDir -Force | Out-Null

if (Test-Path -LiteralPath $FinalInstallerPath) {
    Remove-Item -LiteralPath $FinalInstallerPath -Force
}

Write-Host "Building NSIS installer..."
Push-Location $RepoRoot
try {
    Invoke-Checked $MakeNsis @($NsisScriptRelative)
}
finally {
    Pop-Location
}

$GeneratedInstaller = Get-ChildItem -LiteralPath $InstallerOutputDir -Filter "*.exe" |
    Where-Object { $_.Name -ne $FinalInstallerName } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $GeneratedInstaller) {
    throw "NSIS completed, but no generated installer exe was found in: $InstallerOutputDir"
}

Move-Item -LiteralPath $GeneratedInstaller.FullName -Destination $FinalInstallerPath -Force
Write-Host "Done: $FinalInstallerPath"
