@ECHO OFF

PUSHD "%~dp0..\..\..\"

CALL getversion.cmd

IF "%OLW_CONFIG%" == "" (
  echo %%OLW_CONFIG%% not set, will default to 'Debug'
  set OLW_CONFIG=Debug
)

IF EXIST "%LocalAppData%\Nuget\Nuget.exe" (GOTO package) ELSE (
   echo Nuget.exe missing from %LocalAppData%\Nuget\Nuget.exe
   GOTO end
)

:package
"%LocalAppData%\Nuget\Nuget.exe" pack .\OpenLiveWriter.nuspec -version %dottedVersion% -basepath src\managed\bin\%OLW_CONFIG%\i386\Writer
ECHO Created Writer NuGet package.

.\src\managed\packages\squirrel.windows.1.4.4\tools\SyncReleases.exe -url=https://olw.blob.core.windows.net/stable/Releases/ -r=.\Releases
.\src\managed\packages\squirrel.windows.1.4.4\tools\Squirrel.exe -i .\src\managed\OpenLiveWriter.PostEditor\Images\Writer.ico %OLW_SIGN% --no-msi --releasify .\OpenLiveWriter.%dottedVersion%.nupkg 
MOVE .\Releases\Setup.exe .\Releases\OpenLiveWriterSetup.exe
ECHO Created Open Live Writer setup file.

::Build Chocolatey package. Suppress package analysis since Chocolatey powershell generates verbose warnings.
"%LocalAppData%\Nuget\Nuget.exe" pack .\OpenLiveWriter.Install.nuspec -version %dottedVersion% -basepath Releases -nopackageanalysis
ECHO Created Writer Chocolatey Package

:end

POPD
