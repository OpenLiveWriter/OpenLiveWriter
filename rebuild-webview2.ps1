# Quick rebuild script for WebView2Shim development
# Usage: .\rebuild-webview2.ps1 [-Run]
param(
    [switch]$Run
)

$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\msbuild.exe"
$projectPath = "D:\github\OpenLiveWriter\src\managed\OpenLiveWriter.WebView2Shim\OpenLiveWriter.WebView2Shim.csproj"
$outputDll = "D:\github\OpenLiveWriter\src\managed\OpenLiveWriter.WebView2Shim\bin\Debug\OpenLiveWriter.WebView2Shim.dll"
$targetDir = "D:\github\OpenLiveWriter\src\managed\bin\Debug\x64\Writer\"
$exePath = "D:\github\OpenLiveWriter\src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe"

Write-Host "Building WebView2Shim..." -ForegroundColor Cyan
& $msbuild $projectPath /p:Configuration=Debug /v:minimal /nologo
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
