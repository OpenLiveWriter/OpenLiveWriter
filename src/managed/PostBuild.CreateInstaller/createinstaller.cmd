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

.\src\managed\packages\squirrel.windows.1.2.1\tools\Squirrel.exe -i .\src\managed\OpenLiveWriter.PostEditor\Images\Writer.ico %OLW_SIGN% --no-msi --releasify .\OpenLiveWriter.%dottedVersion%.nupkg 
ECHO Created Open Live Writer setup file.

:end
POPD
