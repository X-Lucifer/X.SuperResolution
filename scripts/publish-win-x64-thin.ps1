param(
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptRoot = $PSScriptRoot
$RepoRoot = (Resolve-Path -LiteralPath (Join-Path $ScriptRoot "..")).Path
$ProjectPath = Join-Path $RepoRoot "X.SuperResolution\X.SuperResolution.csproj"
$VcompResolver = Join-Path $ScriptRoot "Resolve-Vcomp140.ps1"
$PublishDir = Join-Path $RepoRoot "X.SuperResolution\bin\$Configuration\win-x64\publish"
$PackageDir = Join-Path $RepoRoot "artifacts\packages"
$ArchivePath = Join-Path $PackageDir "X.SuperResolution-win-x64-thin.7z"

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

$DotNet = Require-Command "dotnet"
$SevenZip = Require-Command "7z"
$VcompPath = & $VcompResolver

if (-not (Test-Path -LiteralPath $ProjectPath)) {
    throw "Project file not found: $ProjectPath"
}

if (Test-Path -LiteralPath $PublishDir) {
    Remove-Item -LiteralPath $PublishDir -Recurse -Force
}

New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null
New-Item -ItemType Directory -Path $PackageDir -Force | Out-Null

Write-Host "Publishing framework-dependent single-file win-x64 build..."
Invoke-Checked $DotNet @(
    "publish",
    $ProjectPath,
    "-c", $Configuration,
    "-r", "win-x64",
    "--self-contained", "false",
    "-o", $PublishDir,
    "/p:PublishSingleFile=true",
    "/p:CopyOutputSymbolsToPublishDirectory=false",
    "/p:IncludeNativeLibrariesForSelfExtract=true",
    "/p:DebugType=None",
    "/p:DebugSymbols=false",
    "/p:AvaloniaBuildServicesEnabled=false"
)

Write-Host "Adding Microsoft Visual C++ OpenMP runtime..."
Copy-Item -LiteralPath $VcompPath -Destination (Join-Path $PublishDir "vcomp140.dll") -Force

Write-Host "Removing model folders from thin publish output..."
Get-ChildItem -LiteralPath $PublishDir -Directory |
    Where-Object { $_.Name -eq "models" -or $_.Name -like "models-*" } |
    ForEach-Object {
        Write-Host "Removing $($_.FullName)"
        Remove-Item -LiteralPath $_.FullName -Recurse -Force
    }

Write-Host "Removing PDB files..."
Get-ChildItem -LiteralPath $PublishDir -Filter "*.pdb" -Recurse | 
    ForEach-Object {
        Write-Host "Removing PDB: $($_.FullName)"
        Remove-Item -LiteralPath $_.FullName -Force
    }


if (Test-Path -LiteralPath $ArchivePath) {
    Remove-Item -LiteralPath $ArchivePath -Force
}

Write-Host "Creating package: $ArchivePath"
Push-Location $PublishDir
try {
    Invoke-Checked $SevenZip @("a", "-t7z", "-mx=9", $ArchivePath, ".\*")
}
finally {
    Pop-Location
}

Write-Host "Done: $ArchivePath"
