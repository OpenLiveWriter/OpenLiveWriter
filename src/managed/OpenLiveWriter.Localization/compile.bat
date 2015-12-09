@echo on
setlocal


call ..\..\..\..\..\tools\path1st\myenv.cmd > nul
pushd %inetroot%\client\writer\src\managed\OpenLiveWriter.localization\

call sd edit CommandId.cs MessageId.cs StringId.cs Properties.resx PropertiesNonLoc.resx Strings.resx > nul

..\..\..\..\..\target\debug\i386\Writer\locutil.exe /c:Commands.xml /cenum:CommandId.cs /r:Ribbon.xml /d:DisplayMessages.xml /denum:MessageId.cs /s:Strings.csv /senum:StringId.cs /props:Properties.resx /propsnonloc:PropertiesNonLoc.resx /strings:Strings.resx
IF ERRORLEVEL   1 goto End

:End
popd

endlocal