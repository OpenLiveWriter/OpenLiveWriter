$ErrorActionPreference = 'Stop'

$packageName = 'openlivewriter'
$fileType = 'EXE'
$silentArgs = '--silent'
$scriptPath = $(Split-Path $MyInvocation.MyCommand.Path)
$fileFullPath = Join-Path $scriptPath 'OpenLiveWriterSetup.exe'

Install-ChocolateyInstallPackage $packageName $fileType $silentArgs $fileFullPath

Write-Output "Open Live Writer has been installed to `"$env:localappdata\OpenLiveWriter`""
