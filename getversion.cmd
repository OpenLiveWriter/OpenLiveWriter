@ECHO OFF

SET /P version=<"%~dp0version.txt"

FOR /F "tokens=1-4 delims=." %%V in ("%version%") DO (
    ::NuGet now normalizes versions, so need to remove build number if 0
    IF %%Y EQU 0 (SET "dottedVersion=%%V.%%W.%%X") ELSE (SET "dottedVersion=%%V.%%W.%%X.%%Y")
    SET "rcVersion=%%V,%%W,%%X,%%Y"
)