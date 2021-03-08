# Run this script from the Developer PowerShell found in Visual Studio 2019 
# or the start menu to get the correct msbuild version on the path
#
# GitVersion is also required and must be available on the path
# Inno Setup 5 is used to compile the setup, it's path is specified below
#
$innoSetupCompiler = "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"





$versionJson = & GitVersion | Out-String
try
{
    $version = ConvertFrom-Json $versionJson
}
catch
{
    Write-Host "Error while parsing GitVersion output: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $versionJson -ForegroundColor Gray
    exit 1
}

Write-Host "GitVersion: $($version.LegacySemVer)"
$env:BUILD_VERSION = $version.LegacySemVer


& msbuild MassiveKnob.sln /t:Clean /t:Build /p:Configuration=Release
if (!$?) 
{
    Write-Host "MSBuild failed, aborting..." -ForegroundColor Red
    exit 1
}

& $innoSetupCompiler "Setup\MassiveKnobSetup.iss"
if (!$?) 
{
    Write-Host "Inno Setup failed, aborting..." -ForegroundColor Red
    exit 1
}