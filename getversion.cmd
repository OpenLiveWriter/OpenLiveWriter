@ECHO OFF

SET /P version=<"%~dp0version.txt"

FOR /F "tokens=1-4 delims=." %%V in ("%version%") DO (
    SET "dottedVersion=%%V.%%W.%%X.%%Y"
    SET "rcVersion=%%V,%%W,%%X,%%Y"
)
