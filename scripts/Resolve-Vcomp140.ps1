Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$searchRoots = @(
    [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles),
    [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

$candidates = foreach ($root in $searchRoots) {
    $visualStudioRoot = Join-Path $root "Microsoft Visual Studio"
    if (-not (Test-Path -LiteralPath $visualStudioRoot)) {
        continue
    }

    Get-ChildItem -LiteralPath $visualStudioRoot -Filter "vcomp140.dll" -File -Recurse -ErrorAction SilentlyContinue |
        Where-Object {
            ($_.FullName -match '[\\/]VC[\\/]Redist[\\/]MSVC[\\/].+[\\/]x64[\\/]Microsoft\.VC\d+\.OpenMP[\\/]vcomp140\.dll$') -and
            ($_.FullName -notmatch '[\\/]onecore[\\/]')
        }
}

$vcomp = $candidates |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if ($null -eq $vcomp) {
    throw "Redistributable x64 vcomp140.dll was not found under a Visual Studio VC Redist directory. Install the Desktop development with C++ workload before publishing."
}

$vcomp.FullName
