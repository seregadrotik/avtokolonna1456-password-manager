#Requires -Version 5.1
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$sln = Join-Path $root "Avtokolonna1456.sln"
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    | Select-Object -First 1

if (-not $msbuild) {
    $msbuild = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
}

if (-not (Test-Path $msbuild)) {
    throw "MSBuild не найден. Установите Visual Studio 2022 или Build Tools с компонентом .NET desktop development."
}

Write-Host "MSBuild: $msbuild"
& $msbuild $sln /t:Build /p:Configuration=$Configuration /p:Platform=x64 /m
if ($LASTEXITCODE -ne 0) { throw "Сборка завершилась с ошибкой." }

$out = Join-Path $root "src\Avtokolonna1456.PasswordManager\bin\x64\$Configuration"
Write-Host "Готово: $out"
