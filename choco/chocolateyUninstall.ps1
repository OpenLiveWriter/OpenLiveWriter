$ErrorActionPreference = 'Stop';

$packageName = 'openlivewriter'
$softwareName = 'open live writer*'
$installerType = 'EXE'
$file = "$env:localappdata\OpenLiveWriter\Update.exe"

$silentArgs = '--uninstall -s'

if ($installerType -ne 'MSI') {
  $validExitCodes = @(0)
}

$uninstalled = $false
$local_key     = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
$machine_key   = 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
$machine_key6432 = 'HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'
$main_key      = 'HKCU:\Software\OpenLiveWriter'
$local_profile = '$env:localappdata\OpenLiveWriter'
$roam_profile  = '$env:appdata\OpenLiveWriter'

$key = Get-ItemProperty -Path @($machine_key6432,$machine_key, $local_key) `
                        -ErrorAction SilentlyContinue `
         | ? { $_.DisplayName -like "$softwareName" }

if (!(Test-Path "$env:localappdata\openlivewriter\openlivewriter.exe"))
{
   if (!(Test-Path $file))
   {
      throw "Could not find $file - because this software is always installed to a user profile folder, it must be uninstalled in the context of the user who installed it."
   }
   else
   {
    Try {
      Uninstall-ChocolateyPackage -PackageName $packageName `
                                  -FileType $installerType `
                                  -SilentArgs "$silentArgs" `
                                  -ValidExitCodes $validExitCodes `
                                  -File "$file"

      #Remove leftovers so new install will succeed
      if (test-path "$env:localappdata\openlivewriter\.dead")
      {
        remove-item "$env:localappdata\openlivewriter" -Recurse -Force -ErrorAction 'SilentlyContinue'
      }
      if (test-path "C:\ProgramData\SquirrelMachineInstalls\OpenLiveWriter.exe")
      {
        remove-item "C:\ProgramData\SquirrelMachineInstalls\OpenLiveWriter.exe" -Force -ErrorAction 'SilentlyContinue'
      }
      #Remove the registry keys so that the next installation will prompt for reconfiguration
      Push-Location
      if (Test-Path $main_key)
      {
          Remove-Item $main_key
      } else {
          Write-Warning "The $main_key registry key does not exist."
      }
      Pop-Location
      #Remove the user profile folders in AppData
      if (test-path $local_profile)
      {
          Remove-Item $local_profile -Recurse -Force -ErrorAction 'SilentlyContinue'
      }
      if (Test-Path $roam_profile)
      {
          Remove-Item $roam_profile -Recurse -Force -ErrorAction 'SilentlyContinue'
      }
      Write-Output "The install log is at `"$env:localappdata\SquirrelTemp\SquirrelSetup.log`""
    }
    Catch
    {
      throw $_.Exception
    }
   }
} else {
  Write-Warning "$packageName has already been uninstalled by other means or it is installed to a user's profile who is not the current user ($env:username)."
}
