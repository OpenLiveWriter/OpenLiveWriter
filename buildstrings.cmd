@echo off
REM Build Strings resource and StringId enum from Strings.csv.
REM locutil.exe is produced by the LocUtil project; build it on demand so this
REM script works before the first full build (and from a clean checkout).
setlocal

if "%OLW_CONFIG%"=="" (set CONFIG=Debug) else (set CONFIG=%OLW_CONFIG%)

REM Actual LocUtil output path (dotnet build, AppendTargetFrameworkToOutputPath=false).
REM The legacy solution-level path is kept as a fallback for older build trees.
set LOCUTIL=src\managed\LocUtil\bin\%CONFIG%\locutil.exe
set LOCUTIL_LEGACY=src\managed\bin\%CONFIG%\x64\Writer\locutil.exe

if not exist "%LOCUTIL%" if exist "%LOCUTIL_LEGACY%" set LOCUTIL=%LOCUTIL_LEGACY%

if not exist "%LOCUTIL%" (
  echo locutil.exe not found - building LocUtil (%CONFIG%) first...
  dotnet build src\managed\LocUtil\LocUtil.csproj -c %CONFIG% --nologo -v minimal
  if errorlevel 1 exit /b 1
)

if not exist "%LOCUTIL%" (
  echo ERROR: locutil.exe still not found at %LOCUTIL% after building LocUtil.
  exit /b 1
)

echo Building Strings resource and StringId enum from Strings.csv
"%LOCUTIL%" /s:src\managed\OpenLiveWriter.Localization\Strings.csv /senum:src\managed\OpenLiveWriter.Localization\StringId.cs /strings:src\managed\OpenLiveWriter.Localization\Strings.resx /props:src\managed\OpenLiveWriter.Localization\Properties.resx /propsnonloc:src\managed\OpenLiveWriter.Localization\PropertiesNonLoc.resx
