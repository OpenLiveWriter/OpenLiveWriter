# Cause powershell to fail on errors rather than keep going
$ErrorActionPreference = "Stop";

# Build configuration:
# - Builds x64 (64-bit) only - x86/32-bit is no longer supported
# - C++ toolset auto-detected from installed VS version (VS2019=v142, VS2022=v143, VS2026=v180)
# - Required VS components: see .vsconfig file in repo root

@"

=======================================================
 Checking .NET 10 SDK
=======================================================
"@

$dotnetVersion = $null
try {
    $dotnetVersion = & dotnet --version 2>$null
} catch {
    # dotnet command not found
}

if (-Not $dotnetVersion) {
    "ERROR: .NET SDK not found. Please install .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0"
    exit 99
}

$majorVersion = [int]($dotnetVersion.Split('.')[0])
if ($majorVersion -lt 10) {
    "ERROR: .NET 10 SDK required. Found: $dotnetVersion"
    "Please install .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0"
    "Visual Studio 2026 includes .NET 10 SDK by default."
    exit 99
}

".NET SDK found: $dotnetVersion"

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

# Use vswhere to find the latest Visual Studio installation
$vswherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vswherePath) {
    $visualStudioLocation = & $vswherePath -prerelease -latest -property installationPath
} else {
    # Fallback: try VSSetup module
    Install-Module VSSetup -Scope CurrentUser -Force
    $visualStudioLocation = (Get-VSSetupInstance | Select-VSSetupInstance -Latest).InstallationPath
}

# Try "Current" path first (VS2019/VS2022/VS2026+), then fall back to versioned paths
$msBuildExe = $visualStudioLocation + "\MSBuild\Current\Bin\msbuild.exe"
IF (-Not (Test-Path -LiteralPath "$msBuildExe" -PathType Leaf))
{
    # Try VS2017 path
    $msBuildExe = $visualStudioLocation + "\MSBuild\15.0\Bin\msbuild.exe"
}
IF (-Not (Test-Path -LiteralPath "$msBuildExe" -PathType Leaf))
{
	"MSBuild not found at '$msBuildExe'"
	"In order to build OpenLiveWriter, Visual Studio 2017 or later must be installed."
	"Supported versions: VS2017, VS2019, VS2022, VS2026"
	"These can be downloaded from https://visualstudio.microsoft.com/downloads/"
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

# Determine platform target (default x64)
$platformTarget = "x64"
foreach ($arg in $ARGS) {
    if ($arg -match "PlatformTarget=(\w+)") {
        $platformTarget = $matches[1]
    }
}
"Building for platform: $platformTarget"

# Build the native C++ Ribbon project first (x64 only)
$ribbonProject = "$PSSCRIPTROOT\src\unmanaged\OpenLiveWriter.Ribbon\OpenLiveWriter.Ribbon.vcxproj"
@"

=======================================================
 Building native Ribbon DLL (x64)
=======================================================
"@
$ribbonBuildCommand = "`"$msBuildExe`" `"$ribbonProject`" /nologo /maxcpucount /verbosity:minimal /p:Configuration=$env:OLW_CONFIG /p:Platform=x64 $ARGS"
"Running: $ribbonBuildCommand"
Invoke-Expression "& $ribbonBuildCommand"

@"

=======================================================
 Building managed solution
=======================================================
"@
$buildCommand = "`"$msBuildExe`" $solutionFile /nologo /maxcpucount /verbosity:minimal /p:Configuration=$env:OLW_CONFIG $ARGS"
"Running build command '$buildCommand'"
Invoke-Expression "& $buildCommand"