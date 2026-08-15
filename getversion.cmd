@ECHO OFF

SET /P version=<"%~dp0version.txt"

FOR /F "tokens=1-4 delims=." %%V in ("%version%") DO (
    ::NuGet now normalizes versions, so need to remove build number if 0
    IF %%Y EQU 0 (SET "dottedVersion=%%V.%%W.%%X") ELSE (SET "dottedVersion=%%V.%%W.%%X.%%Y")
    SET "rcVersion=%%V,%%W,%%X,%%Y"
    :: Velopack rejects a 4-part version outright ("must be a 3-part SemVer2
    :: compliant version string"), so a build number becomes a prerelease
    :: suffix. It also keeps each build sorting above the last, which is what
    :: lets an installed alpha see a newer alpha.
    IF %%Y EQU 0 (SET "packVersion=%%V.%%W.%%X") ELSE (SET "packVersion=%%V.%%W.%%X-alpha.%%Y")
)
