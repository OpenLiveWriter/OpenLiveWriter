@ECHO OFF

SETLOCAL

IF "%OLW_CONFIG%" == "" (
  echo %%OLW_CONFIG%% not set, will default to 'Debug'
  set OLW_CONFIG=Debug
)

SET WRITER_EXE_PATH="%~dp0src\managed\bin\%OLW_CONFIG%\i386\Writer\OpenLiveWriter.exe"

IF NOT EXIST "%WRITER_EXE_PATH%" (
  ECHO ERROR: %WRITER_EXE_PATH% does not exist.
  EXIT /B 1
)

CALL %WRITER_EXE_PATH% %*
