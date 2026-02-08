# OpenLiveWriter Complete Build Script
# This script checks prerequisites and builds the project from scratch
#
# Usage: .\build-complete.ps1
#        .\build-complete.ps1 -SkipPrereqCheck   # Skip VS component check
#        .\build-complete.ps1 -Run               # Build and run with WebView2

param(
    [switch]$SkipPrereqCheck,
    [switch]$Run,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host " OpenLiveWriter Build Script (feature/webview2 branch)" -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# STEP 1: Check Prerequisites
# ============================================================
if (-not $SkipPrereqCheck) {
    Write-Host "[1/5] Checking prerequisites..." -ForegroundColor Yellow
    
    # Find Visual Studio
    $vswherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswherePath)) {
        Write-Host "ERROR: Visual Studio not found. Please install Visual Studio 2022 or later." -ForegroundColor Red
        Write-Host "       Download from: https://visualstudio.microsoft.com/" -ForegroundColor Red
        exit 1
    }
    
    $vsInstall = & $vswherePath -latest -format json | ConvertFrom-Json
    if (-not $vsInstall) {
        Write-Host "ERROR: No Visual Studio installation found." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "  Found: $($vsInstall.displayName) at $($vsInstall.installationPath)" -ForegroundColor Green
    
    # Check for C++ build tools (required for OpenLiveWriter.Ribbon)
    $vcToolsPath = Join-Path $vsInstall.installationPath "VC\Tools\MSVC"
    if (-not (Test-Path $vcToolsPath)) {
        Write-Host ""
        Write-Host "ERROR: C++ build tools not found!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install via Visual Studio Installer:" -ForegroundColor Yellow
        Write-Host "  1. Open Visual Studio Installer" -ForegroundColor White
        Write-Host "  2. Click 'Modify' on your VS installation" -ForegroundColor White
        Write-Host "  3. Select 'Desktop development with C++' workload" -ForegroundColor White
        Write-Host "  4. Ensure these are checked:" -ForegroundColor White
        Write-Host "     - MSVC v143 (or latest) C++ build tools" -ForegroundColor White
        Write-Host "     - Windows 10/11 SDK" -ForegroundColor White
        Write-Host ""
        exit 1
    }
    
    # Detect platform toolset version
    $toolsetVersions = Get-ChildItem $vcToolsPath -Directory | Sort-Object Name -Descending
    if ($toolsetVersions.Count -eq 0) {
        Write-Host "ERROR: No MSVC toolset found in $vcToolsPath" -ForegroundColor Red
        exit 1
    }
    
    $latestToolset = $toolsetVersions[0].Name
    Write-Host "  C++ Toolset: $latestToolset" -ForegroundColor Green
    
    # Map toolset version to platform toolset
    # 14.4x = v144 (VS2026), 14.3x = v143 (VS2022), 14.2x = v142 (VS2019)
    $majorMinor = $latestToolset.Substring(0, 4)  # e.g., "14.4"
    $platformToolset = switch ($majorMinor) {
        "14.4" { "v144" }
        "14.3" { "v143" }
        "14.2" { "v142" }
        "14.1" { "v141" }
        default { "v143" }  # Default to v143
    }
    
    Write-Host "  Platform Toolset: $platformToolset" -ForegroundColor Green
    
    # Check for .NET Framework 4.7.2 targeting pack
    $dotnetPath = "${env:ProgramFiles(x86)}\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2"
    if (-not (Test-Path $dotnetPath)) {
        Write-Host ""
        Write-Host "WARNING: .NET Framework 4.7.2 targeting pack not found." -ForegroundColor Yellow
        Write-Host "         Build may fail. Install via Visual Studio Installer:" -ForegroundColor Yellow
        Write-Host "         Individual components -> .NET Framework 4.7.2 targeting pack" -ForegroundColor Yellow
        Write-Host ""
    } else {
        Write-Host "  .NET Framework 4.7.2: Found" -ForegroundColor Green
    }
    
    Write-Host ""
} else {
    Write-Host "[1/5] Skipping prerequisite check..." -ForegroundColor Yellow
    $platformToolset = "v143"  # Default
    Write-Host ""
}

# ============================================================
# STEP 2: Clean (optional)
# ============================================================
if ($Clean) {
    Write-Host "[2/5] Cleaning previous build..." -ForegroundColor Yellow
    
    $binPath = Join-Path $PSScriptRoot "src\managed\bin"
    $objPath = Join-Path $PSScriptRoot "src\managed\obj"
    
    if (Test-Path $binPath) { Remove-Item -Recurse -Force $binPath }
    if (Test-Path $objPath) { Remove-Item -Recurse -Force $objPath }
    
    Write-Host "  Cleaned bin/ and obj/ folders" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[2/5] Skipping clean (use -Clean to force)" -ForegroundColor Gray
    Write-Host ""
}

# ============================================================
# STEP 3: Restore NuGet packages
# ============================================================
Write-Host "[3/5] Restoring NuGet packages..." -ForegroundColor Yellow

$solutionFile = Join-Path $PSScriptRoot "src\managed\writer.sln"

# Find nuget.exe or use dotnet restore
$nugetExe = Join-Path $PSScriptRoot "utilities\nuget.exe"
if (Test-Path $nugetExe) {
    & $nugetExe restore $solutionFile -NonInteractive
} else {
    # Try dotnet restore as fallback
    dotnet restore $solutionFile
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: NuGet restore failed!" -ForegroundColor Red
    exit 1
}

Write-Host "  NuGet packages restored" -ForegroundColor Green
Write-Host ""

# ============================================================
# STEP 4: Build
# ============================================================
Write-Host "[4/5] Building OpenLiveWriter..." -ForegroundColor Yellow
Write-Host "  Platform Toolset: $platformToolset" -ForegroundColor Gray
Write-Host "  Configuration: Debug" -ForegroundColor Gray
Write-Host "  Platform: x64" -ForegroundColor Gray
Write-Host ""

# Call the main build script with the detected toolset
& "$PSScriptRoot\build.ps1" /p:PlatformToolset=$platformToolset

# Check if the executable was actually built (more reliable than exit code)
$exePath = Join-Path $PSScriptRoot "src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe"
if (-not (Test-Path $exePath)) {
    Write-Host ""
    Write-Host "=======================================================" -ForegroundColor Red
    Write-Host " BUILD FAILED" -ForegroundColor Red
    Write-Host "=======================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common fixes:" -ForegroundColor Yellow
    Write-Host "  1. Install 'Desktop development with C++' workload in VS Installer" -ForegroundColor White
    Write-Host "  2. Try: .\build-complete.ps1 -Clean" -ForegroundColor White
    Write-Host "  3. Check you have Windows 10/11 SDK installed" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "=======================================================" -ForegroundColor Green
Write-Host " BUILD SUCCEEDED" -ForegroundColor Green
Write-Host "=======================================================" -ForegroundColor Green

# ============================================================
# STEP 5: Run (optional)
# ============================================================

if ($Run) {
    Write-Host ""
    Write-Host "[5/5] Starting OpenLiveWriter (WebView2 is default)..." -ForegroundColor Yellow
    
    if (Test-Path $exePath) {
        Start-Process $exePath
        Write-Host "  Started! (WebView2 is the default engine)" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Executable not found at $exePath" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host ""
    Write-Host "To run (WebView2 is the default engine):" -ForegroundColor Cyan
    Write-Host "  $exePath" -ForegroundColor White
    Write-Host ""
    Write-Host "Or just run:" -ForegroundColor Cyan
    Write-Host "  .\build-complete.ps1 -Run" -ForegroundColor White
    Write-Host ""
    Write-Host "To fall back to legacy MSHTML/IE engine:" -ForegroundColor Gray
    Write-Host '  $env:OLW_USE_MSHTML = "1"' -ForegroundColor Gray
}

Write-Host ""
