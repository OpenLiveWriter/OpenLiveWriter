@ECHO OFF

PUSHD "%~dp0..\..\..\"

CALL getversion.cmd

REM The configuration comes from MSBuild as %1 ($(Configuration) in the project's
REM PostBuildEvent), so the installer is always packed from the binaries that
REM were just built. %OLW_CONFIG% is only a fallback for direct invocation; it
REM used to be the sole source, and when it was unset this packed bin\Debug even
REM though MSBuild had just built Release.
SET BUILD_CONFIG=%~1
IF "%BUILD_CONFIG%" == "" SET BUILD_CONFIG=%OLW_CONFIG%
IF "%BUILD_CONFIG%" == "" (
  echo createinstaller: no configuration ^(pass one as an argument or set %%OLW_CONFIG%%^).
  POPD
  EXIT /B 1
)

REM Velopack output is what users install, so only ever pack Release. A Debug
REM configuration is a developer build: skip packaging rather than produce an
REM installer that must not ship.
IF /I NOT "%BUILD_CONFIG%" == "Release" (
  echo createinstaller: skipping Velopack packaging for '%BUILD_CONFIG%' ^(Release only^).
  POPD
  EXIT /B 0
)

REM .NET 10 SDK builds use per-project output (no shared bin\x64\Writer dir),
REM so the app and all its dependencies land in the project's own bin folder.
SET PUBLISH_DIR=src\managed\OpenLiveWriter\bin\%BUILD_CONFIG%
SET ICON_PATH=src\managed\OpenLiveWriter.PostEditor\Images\Writer.ico

REM vpk is a global dotnet tool and is not part of a source checkout. Local
REM builds without it should still succeed; CI installs it before building.
WHERE vpk >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
  echo createinstaller: vpk not found on PATH, skipping Velopack packaging.
  POPD
  EXIT /B 0
)

:: Create Velopack installer package
vpk pack ^
  --packId OpenLiveWriter ^
  --packVersion %packVersion% ^
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
   GOTO fail
)

MOVE .\Releases\OpenLiveWriter-stable-Setup.exe .\Releases\OpenLiveWriterSetup.exe
IF %ERRORLEVEL% NEQ 0 (
   echo Failed to rename OpenLiveWriter-Setup.exe. The file may not have been created by Velopack.
   GOTO fail
)
ECHO Created Open Live Writer Velopack installer from %PUBLISH_DIR%.

:: Build Chocolatey package
IF EXIST "%LocalAppData%\Nuget\Nuget.exe" (
  "%LocalAppData%\Nuget\Nuget.exe" pack .\OpenLiveWriter.Install.nuspec -version %dottedVersion% -basepath Releases -nopackageanalysis
  ECHO Created Writer Chocolatey Package
) ELSE (
  echo Nuget.exe missing from %LocalAppData%\Nuget\Nuget.exe - skipping Chocolatey package
)

POPD
EXIT /B 0

:fail
POPD
EXIT /B 1
