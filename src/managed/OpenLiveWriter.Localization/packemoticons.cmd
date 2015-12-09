@echo off
set emoLocPath=%inetroot%\client\writer\src\managed\OpenLiveWriter.localization\Emoticons
set emoExe=%inetroot%\target\debug\i386\Writer\EmoticonGenerator.exe
pushd %inetroot%\client\writer\utilities\emoticongenerator
rem Build emoticongenerator
call build /c

rem Check if the build was successful
IF NOT EXIST "%emoExe%" goto error1
call sd edit %emoLocPath%\Emoticon_All_Strip.png
call %emoExe% -i %emoLocPath%\emoticonList.txt -b %emoLocPath% -o %emoLocPath%\Emoticon_All_Strip.png
goto end
:error1
echo Cannot find %emoExe%
:end
popd

