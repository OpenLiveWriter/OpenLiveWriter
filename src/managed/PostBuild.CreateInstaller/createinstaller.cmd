@ECHO OFF

PUSHD "%~dp0..\..\..\"

CALL getversion.cmd

IF "%OLW_CONFIG%" == "" (
  echo %%OLW_CONFIG%% not set, will default to 'Debug'
  set OLW_CONFIG=Debug
)

REM .NET 10 SDK builds use per-project output (no shared bin\x64\Writer dir),
REM so the app and all its dependencies land in the project's own bin folder.
SET PUBLISH_DIR=src\managed\OpenLiveWriter\bin\%OLW_CONFIG%
SET ICON_PATH=src\managed\OpenLiveWriter.PostEditor\Images\Writer.ico

:: Create Velopack installer package
vpk pack ^
  --packId OpenLiveWriter ^
  --packVersion %dottedVersion% ^
  --packTitle "Open Live Writer" ^
  --packDir %PUBLISH_DIR% ^
  --mainExe OpenLiveWriter.exe ^
  --icon %ICON_PATH% ^
  --channel stable ^
  --outputDir Releases ^
  --shortcuts Desktop,StartMenuRoot ^
  --skipVeloAppCheck

IF %ERRORLEVEL% NEQ 0 (
   echo Velopack packaging failed.
   GOTO end
)

MOVE .\Releases\OpenLiveWriter-stable-Setup.exe .\Releases\OpenLiveWriterSetup.exe
IF %ERRORLEVEL% NEQ 0 (
   echo Failed to rename OpenLiveWriter-Setup.exe. The file may not have been created by Velopack.
   GOTO end
)
ECHO Created Open Live Writer Velopack installer.

:: Build Chocolatey package
IF EXIST "%LocalAppData%\Nuget\Nuget.exe" (
  "%LocalAppData%\Nuget\Nuget.exe" pack .\OpenLiveWriter.Install.nuspec -version %dottedVersion% -basepath Releases -nopackageanalysis
  ECHO Created Writer Chocolatey Package
) ELSE (
  echo Nuget.exe missing from %LocalAppData%\Nuget\Nuget.exe - skipping Chocolatey package
)

:end

POPD
