#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot "build.ps1") -Configuration Release

$iscc = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    $cmd = Get-Command iscc -ErrorAction SilentlyContinue
    if ($cmd) { $iscc = $cmd.Source }
}

if (-not $iscc) {
    throw "Inno Setup 6 не найден. Установите с https://jrsoftware.org/isinfo.php"
}

$iss = Join-Path $root "installer\setup.iss"
& $iscc $iss
if ($LASTEXITCODE -ne 0) { throw "Сборка установщика не удалась." }

$setup = Join-Path $root "dist\Avtokolonna1456_Setup.exe"
Write-Host "Установщик: $setup"
