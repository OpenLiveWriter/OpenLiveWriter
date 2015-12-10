$packageName = 'openLiveWriter.install'
$fileType = 'exe'
$silentArgs = '--silent'
$scriptPath =  $(Split-Path $MyInvocation.MyCommand.Path)
$fileFullPath = Join-Path $scriptPath 'Setup.exe'

Install-ChocolateyInstallPackage $packageName $fileType $silentArgs $fileFullPath