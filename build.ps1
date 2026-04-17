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

# If MSBuild is already on PATH (e.g. on CI where an action has added it),
# use that directly and skip the VSSetup lookup.
$msBuildOnPath = Get-Command msbuild.exe -ErrorAction SilentlyContinue
if ($msBuildOnPath)
{
	$msBuildExe = $msBuildOnPath.Source
}
else
{
	# Install module to allow us to find MSBuild
	# See https://github.com/Microsoft/vssetup.powershell
	Install-Module VSSetup -Scope CurrentUser -Force

	$visualStudioLocation = (Get-VSSetupInstance `
	  | Select-VSSetupInstance -Version '[15.0,18.0)' -Latest).InstallationPath

	# VS2019+ places MSBuild under MSBuild\Current\Bin; VS2017 uses MSBuild\15.0\Bin.
	# Prefer Current, fall back to the legacy path so local VS2017 builds keep working.
	$msBuildCandidates = @(
		(Join-Path $visualStudioLocation "MSBuild\Current\Bin\msbuild.exe"),
		(Join-Path $visualStudioLocation "MSBuild\15.0\Bin\msbuild.exe")
	)
	$msBuildExe = $msBuildCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1

	IF (-Not $msBuildExe)
	{
		"MSBuild not found. Checked:"
		$msBuildCandidates | ForEach-Object { "  $_" }
		"In order to build OpenLiveWriter, Visual Studio 2017, 2019, or 2022 (any edition) or"
		"the matching Build Tools must be installed."
		"These can be downloaded from https://visualstudio.microsoft.com/downloads/"
		exit 101
	}
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