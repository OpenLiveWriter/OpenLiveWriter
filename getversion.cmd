@ECHO OFF

SET /P version=<"%~dp0version.txt"

FOR /F "tokens=1-3 delims=." %%V in ("%version%") DO (
    SET "dottedVersion=%%V.%%W.%%X"
    SET "rcVersion=%%V,%%W,%%X"
)
