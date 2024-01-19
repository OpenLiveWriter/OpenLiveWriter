$currentDirectory = split-path $MyInvocation.MyCommand.Definition

# See if we have the ClientSecret available
if([string]::IsNullOrEmpty($env:SignClientSecret)){
	Write-Host "Client Secret not found, not signing packages"
	return;
}

# Setup Variables we need to pass into the sign client tool

$appSettings = "$currentDirectory\appsettings.json"

$appPath = "$currentDirectory\..\packages\SignClient\tools\netcoreapp2.0\SignClient.dll"

$releases = ls $currentDirectory\..\Releases\*.exe | Select -ExpandProperty FullName

foreach ($release in $releases){
	Write-Host "Submitting $release for signing"

	dotnet $appPath 'sign' -c $appSettings -i $release -r $env:SignClientUser -s $env:SignClientSecret -n 'Open Live Writer' -d 'Open Live Writer' -u 'http://openlivewriter.com' 

	Write-Host "Finished signing $release"
}

Write-Host "Sign-package complete"
