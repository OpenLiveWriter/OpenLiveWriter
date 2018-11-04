# Cause powershell to fail on errors rather than keep going
$ErrorActionPreference = "Stop";

@"

=======================================================
 Checking solution exists
=======================================================
"@

$solutionFile = "$PSSCRIPTROOT\src\managed\writer.sln"
if (-Not (Test-Path "$solutionFile" -PathType Leaf))
{
	"Unable to find solution file at $solutionFile"
	exit 100
}
"Solution found at '$solutionFile'"

@"

=======================================================
 Fetching MSBuild location
=======================================================
"@

# Install module to allow us to find MSBuild
# See https://github.com/Microsoft/vssetup.powershell
Install-Module VSSetup -Scope CurrentUser

$visualStudioLocation = (Get-VSSetupInstance `
  | Select-VSSetupInstance -Version '[15.0,16.0)' -Latest).InstallationPath

$msBuildExe = $visualStudioLocation + "\MSBuild\15.0\Bin\msbuild.exe"
IF (-Not (Test-Path -LiteralPath "$msBuildExe" -PathType Leaf))
{
	"MSBuild not found at '$msBuildExe'"
	"In order to build OpenLiveWriter either Visual Studio 2017 (any edition) or Build "
	"Tools for Visual Studio 2017 must be installed."
	"These can be downloadd from https://visualstudio.microsoft.com/downloads/"
	exit 101
}

"MSBuild.exe found at: '$msBuildExe'"

@"

=======================================================
 Ensureing nuget.exe exists
=======================================================
"@

$nugetPath = "$env:LocalAppData\NuGet"
$nugetExe = "$nugetPath\NuGet.exe"
if (-Not (Test-Path -LiteralPath "$nugetExe" -PathType Leaf))
{
	if (-Not (Test-Path -LiteralPath "$nugetPath" -PathType Container))
	{
		"Creating Directory '$nugetPath'"
		New-Item "$nugetPath" -Type Directory
	}
	"Downloading nuget.exe"
	Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile "$nugetExe"
}

"Nuget.exe found at: '$nugetExe'"

@"

=======================================================
 Ensure nuget packages exist
=======================================================
"@

$packageFolder = "$PSSCRIPTROOT\src\managed\packages"
if (Test-Path -LiteralPath $packageFolder)
{
    "Packages found at '$packageFolder'"
}
else
{
	"Running nuget restore"
	& $nugetExe restore $solutionFile
}

@"

=======================================================
 Check build type
=======================================================
"@

if (-Not (Test-Path env:OLW_CONFIG))
{
    "Environment variable OWL_CONFIG not set, setting to 'Debug'"
	$env:OLW_CONFIG = 'Debug'
}

"Using build '$env:OLW_CONFIG'"

@"

=======================================================
 Starting build
=======================================================
"@
Get-Date
$buildCommand = "`"$msBuildExe`" $solutionFile /nologo /maxcpucount /verbosity:minimal /p:Configuration=$env:OLW_CONFIG $ARGS"
"Running build command '$buildCommand'"
Invoke-Expression "& $buildCommand"