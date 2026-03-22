@ECHO OFF

PUSHD "%~dp0..\..\..\"

CALL getversion.cmd

IF "%OLW_CONFIG%" == "" (
  echo %%OLW_CONFIG%% not set, will default to 'Debug'
  set OLW_CONFIG=Debug
)

SET PUBLISH_DIR=src\managed\bin\%OLW_CONFIG%\i386\Writer
SET ICON_PATH=src\managed\OpenLiveWriter.PostEditor\Images\Writer.ico

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

ECHO Created Open Live Writer Velopack installer.

POPD
