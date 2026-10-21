param(
    [string]$GameDir = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($GameDir)) {
    $GameDir = Read-Host "Path to the How to Fish folder (the one containing How to Fish.exe)"
}

$GameDir = $GameDir.Trim().Trim('"')
if (-not (Test-Path -LiteralPath $GameDir)) {
    throw "Folder not found: $GameDir"
}

$GameDir = (Resolve-Path -LiteralPath $GameDir).Path
$exe = Join-Path $GameDir "How to Fish.exe"
if (-not (Test-Path -LiteralPath $exe)) {
    throw "How to Fish.exe was not found in: $GameDir"
}

$source = $PSScriptRoot
$items = @("BepInEx", "winhttp.dll", "doorstop_config.ini", ".doorstop_version")

foreach ($item in $items) {
    $sourcePath = Join-Path $source $item
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "Package is incomplete; missing: $item"
    }

    $destination = Join-Path $GameDir $item
    if ((Get-Item -LiteralPath $sourcePath).PSIsContainer) {
        New-Item -ItemType Directory -Force -Path $destination | Out-Null
        Copy-Item -Path (Join-Path $sourcePath "*") -Destination $destination -Recurse -Force
    }
    else {
        Copy-Item -LiteralPath $sourcePath -Destination $destination -Force
    }
}

Write-Host ""
Write-Host "Angler's Toolkit installed to:" -ForegroundColor Green
Write-Host "  $GameDir"
Write-Host ""
Write-Host "Launch How to Fish and press F8 to open the menu."
