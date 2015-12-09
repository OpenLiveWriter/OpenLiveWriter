@echo off
setlocal

set providers=BlogProvidersB5.xml
set config=Config.xml
set output=Output.xml
set errorlog=Errors.txt

BlogRunner.exe "/providers:%providers%" "/config:%config%" "/output:%output%" "/errorlog:%errorlog%"
BlogRunnerReporter.exe "%providers%" "%output%" "%errorlog%" diff.htm