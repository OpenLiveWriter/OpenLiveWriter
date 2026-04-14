# Cause powershell to fail on errors rather than keep going
$ErrorActionPreference = "Stop";

# Build script for Open Live Writer
# Requirements:
#   - .NET 10 SDK (for managed projects)
#   - Visual Studio 2022/2026 with C++ build tools (for Ribbon project, optional)
#
# Supported Visual Studio versions: VS2017, VS2019, VS2022, VS2026
# To override the C++ platform toolset, pass: /p:PlatformToolset=v142 (or v141, v143, v144)

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
 Check .NET SDK
=======================================================
"@

# Check for dotnet CLI
$dotnetExe = Get-Command dotnet -ErrorAction SilentlyContinue
if (-Not $dotnetExe) {
    "dotnet CLI not found. Please install .NET 10 SDK from https://dotnet.microsoft.com/download"
    exit 102
}

$dotnetVersion = & dotnet --version
"dotnet CLI found, version: $dotnetVersion"

# Check if .NET 10 SDK is available
$sdks = & dotnet --list-sdks
if (-Not ($sdks -match "10\.")) {
    "Warning: .NET 10 SDK not found. This project targets .NET 10."
    "Available SDKs:"
    $sdks
    "Download .NET 10 SDK from https://dotnet.microsoft.com/download"
}

@"

=======================================================
 Ensure Velopack CLI (vpk) is installed
=======================================================
"@

# Install vpk global tool if not already installed
$vpkInstalled = & dotnet tool list -g 2>&1 | Select-String "vpk"
if (-Not $vpkInstalled) {
    "Installing vpk (Velopack CLI) as a global .NET tool..."
    & dotnet tool install -g vpk
} else {
    "vpk (Velopack CLI) is already installed."
}

@"

=======================================================
 Check build type
=======================================================
"@

if (-Not (Test-Path env:OLW_CONFIG))
{
    "Environment variable OLW_CONFIG not set, setting to 'Debug'"
	$env:OLW_CONFIG = 'Debug'
}

"Using build '$env:OLW_CONFIG'"

@"

=======================================================
 Building managed projects with dotnet msbuild
=======================================================
"@
Get-Date

# Use dotnet msbuild for managed projects (uses .NET SDK's MSBuild which supports .NET 10)
$buildCommand = "dotnet msbuild `"$solutionFile`" /nologo /maxcpucount /verbosity:minimal /p:Configuration=$env:OLW_CONFIG $ARGS"
"Running build command: $buildCommand"
Invoke-Expression "& $buildCommand"
$managedBuildResult = $LASTEXITCODE

if ($managedBuildResult -ne 0) {
    @"

=======================================================
 Build completed with errors
=======================================================
"@
    "Managed build exit code: $managedBuildResult"
    
    # Check if only the C++ project failed
    if ($managedBuildResult -eq 1) {
        @"

Note: If only the OpenLiveWriter.Ribbon (C++) project failed:
  - The C++ Ribbon project requires Visual Studio C++ build tools
  - Install via VS Installer: 'MSVC v143 - VS 2022 C++ x64/x86 build tools'
  - All managed (.NET) projects may have built successfully
  
To build only managed projects, you can also run:
  dotnet build src\managed\writer.sln
"@
    }
    exit $managedBuildResult
}

@"

=======================================================
 Build completed successfully!
=======================================================
"@
Get-Date
