@echo off
REM Run OpenLiveWriter (WebView2 is the default editor and browser engine)
REM To fall back to legacy MSHTML/IE, set OLW_USE_MSHTML=1
"%~dp0src\managed\bin\Debug\x64\Writer\OpenLiveWriter.exe" %*
