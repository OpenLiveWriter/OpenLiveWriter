$ErrorActionPreference = 'Stop'

$packageName = 'openlivewriter'
$installerType = 'EXE'
$silentArgs = '--uninstall'
$validExitCodes = @(0)

$installDir = "$env:localappdata\OpenLiveWriter"
$updateExe = Join-Path $installDir 'Update.exe'

if (Test-Path $updateExe) {
    Uninstall-ChocolateyPackage -PackageName $packageName `
                                -FileType $installerType `
                                -SilentArgs $silentArgs `
                                -ValidExitCodes $validExitCodes `
                                -File $updateExe

    # Clean up any remaining files after uninstall
    if (Test-Path $installDir) {
        Remove-Item $installDir -Recurse -Force -ErrorAction SilentlyContinue
    }
} else {
    Write-Warning "$packageName does not appear to be installed at `"$installDir`", or it was installed under a different user account ($env:username)."
}
