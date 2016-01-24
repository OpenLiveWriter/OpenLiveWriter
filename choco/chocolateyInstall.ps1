$packageName= 'openlivewriter'
$fileType = 'EXE'
$silentArgs = '--silent'
$scriptPath =  $(Split-Path $MyInvocation.MyCommand.Path)
$fileFullPath = Join-Path $scriptPath 'OpenLiveWriterSetup.exe'

Install-ChocolateyInstallPackage $packageName $fileType $silentArgs $fileFullPath

Write-Output "The install log is at `"$env:localappdata\SquirrelTemp\SquirrelSetup.log`""