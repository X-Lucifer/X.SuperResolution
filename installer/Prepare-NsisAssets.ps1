param(
    [Parameter(Mandatory = $true)]
    [string]$LogoPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$logoFullPath = (Resolve-Path -LiteralPath $LogoPath).Path
$outputDir = Split-Path -Parent $OutputPath
if (![string]::IsNullOrWhiteSpace($outputDir)) {
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
}

$source = [System.Drawing.Image]::FromFile($logoFullPath)
$targetWidth = 150
$targetHeight = 314
$target = New-Object System.Drawing.Bitmap $targetWidth, $targetHeight, ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$graphics = [System.Drawing.Graphics]::FromImage($target)

try {
    $graphics.Clear([System.Drawing.Color]::FromArgb(104, 33, 122))
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

    $scale = [Math]::Max($targetWidth / $source.Width, $targetHeight / $source.Height)
    $width = [int][Math]::Ceiling($source.Width * $scale)
    $height = [int][Math]::Ceiling($source.Height * $scale)
    $x = [int](($targetWidth - $width) / 2)
    $y = [int](($targetHeight - $height) / 2)
    $graphics.DrawImage($source, $x, $y, $width, $height)

    $target.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Bmp)
}
finally {
    $graphics.Dispose()
    $target.Dispose()
    $source.Dispose()
}
