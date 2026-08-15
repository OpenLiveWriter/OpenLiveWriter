# Quick rebuild script for WebView2Shim development
# Usage: .\rebuild-webview2.ps1 [-Run]
param(
    [switch]$Run
)

# Use script location as repo root - all paths are relative
$repoRoot = $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\managed\OpenLiveWriter.WebView2Shim\OpenLiveWriter.WebView2Shim.csproj"
$outputDll = Join-Path $repoRoot "src\managed\OpenLiveWriter.WebView2Shim\bin\Debug\OpenLiveWriter.WebView2Shim.dll"
$targetDir = Join-Path $repoRoot "src\managed\bin\Debug\x64\Writer\"
$exePath = Join-Path $repoRoot "src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe"

# Verify dotnet CLI is available
$dotnetExe = Get-Command dotnet -ErrorAction SilentlyContinue
if (-Not $dotnetExe) {
    Write-Host "dotnet CLI not found. Please install .NET SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

Write-Host "Building WebView2Shim..." -ForegroundColor Cyan
& dotnet msbuild $projectPath /p:Configuration=Debug /v:minimal /nologo
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Copying DLL..." -ForegroundColor Cyan
Copy-Item $outputDll $targetDir -Force
Write-Host "Done!" -ForegroundColor Green

if ($Run) {
    Write-Host "Starting OpenLiveWriter (WebView2 is default)..." -ForegroundColor Cyan
    Start-Process $exePath
}
