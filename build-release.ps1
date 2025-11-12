#!/usr/bin/env powershell
<#
.SYNOPSIS
    Build script for Simple Backup - creates a release build and prepares for installer
.DESCRIPTION
    This script:
    1. Cleans previous build artifacts
    2. Builds the release version for .NET 10 Windows
    3. Publishes self-contained or framework-dependent app
    4. Prepares files for the installer
#>

param(
    [switch]$SelfContained = $false,
    [switch]$BuildInstaller = $false
)

$ErrorActionPreference = "Stop"

$projectName = "Simple Backup"
$projectFile = "Simple Backup.csproj"
$framework = "net10.0-windows"
$configuration = "Release"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Simple Backup - Build Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
Remove-Item -Path "bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "? Clean complete" -ForegroundColor Green
Write-Host ""

# Step 2: Build
Write-Host "[2/4] Building release version (.NET 10)..." -ForegroundColor Yellow
$buildArgs = @(
    "build",
    $projectFile,
    "--configuration", $configuration,
    "--framework", $framework
)

dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "? Build successful" -ForegroundColor Green
Write-Host ""

# Step 3: Publish
Write-Host "[3/4] Publishing application..." -ForegroundColor Yellow
$publishDir = "bin\$configuration\$framework\publish"
$publishArgs = @(
    "publish",
    $projectFile,
    "--configuration", $configuration,
    "--framework", $framework,
    "--output", $publishDir
)

if ($SelfContained) {
    $publishArgs += "--self-contained", "true"
    $publishArgs += "-r", "win-x64"
    Write-Host "  Mode: Self-contained (includes .NET 10 runtime)"
} else {
    $publishArgs += "--self-contained", "false"
    Write-Host "  Mode: Framework-dependent (requires .NET 10 runtime)"
}

dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Publish failed" -ForegroundColor Red
    exit 1
}
Write-Host "? Publish successful" -ForegroundColor Green
Write-Host ""

# Step 4: Show results
Write-Host "[4/4] Build results:" -ForegroundColor Yellow
if (Test-Path $publishDir) {
    $publishedFiles = Get-ChildItem -Path $publishDir -Recurse
    $totalSize = ($publishedFiles | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "  Location: $publishDir"
    Write-Host "  Files: $($publishedFiles.Count)"
    Write-Host "  Size: $([math]::Round($totalSize, 2)) MB"
} else {
    Write-Host "  ? Publish directory not found: $publishDir" -ForegroundColor Red
}
Write-Host ""

# Step 5: Optional - Build installer
if ($BuildInstaller) {
    Write-Host "[5/5] Building installer..." -ForegroundColor Yellow
    
    $issFile = "SimpleBackup.iss"
    if (-not (Test-Path $issFile)) {
        Write-Host "? InnoSetup script not found: $issFile" -ForegroundColor Red
        exit 1
    }
    
    # Try to find InnoSetup
    $innoSetupPaths = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
    )
    
    $isccPath = $null
    foreach ($path in $innoSetupPaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }
    
    if ($isccPath) {
        Write-Host "  Found InnoSetup at: $isccPath"
        & $isccPath $issFile
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Installer created in .\installer\" -ForegroundColor Green
        } else {
            Write-Host "? Installer build failed" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "? InnoSetup not found. Install from: https://jrsoftware.org/isdl.php" -ForegroundColor Red
        Write-Host "  You can still use the published files in: $publishDir" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Green
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  - Framework-dependent: Distribute the published folder" -ForegroundColor White
Write-Host "  - Self-contained: Use -SelfContained to include .NET 10 runtime" -ForegroundColor White
Write-Host "  - Installer: Use -BuildInstaller (requires InnoSetup)" -ForegroundColor White
Write-Host ""
Write-Host "Example commands:" -ForegroundColor Cyan
Write-Host "  .\build-release.ps1                         # Build framework-dependent" -ForegroundColor White
Write-Host "  .\build-release.ps1 -SelfContained          # Build with .NET 10 included" -ForegroundColor White
Write-Host "  .\build-release.ps1 -BuildInstaller         # Build installer (requires InnoSetup)" -ForegroundColor White
