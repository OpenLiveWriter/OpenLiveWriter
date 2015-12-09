@echo off
setlocal
pushd "%0\.."
del *.fo-fo.resx
rd /s/q obj
rd /s/q objd
FauxLocalizationResourceGenerator.exe .
popd
endlocal