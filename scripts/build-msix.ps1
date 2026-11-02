$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw '引擎建置失敗' }
$project = Join-Path $projectRoot 'cmdpal-extension\MsGamebarHandlerExtension\MsGamebarHandlerExtension.csproj'
$output = (Join-Path $projectRoot 'dist\msix').Replace('\', '/') + '/'
dotnet build $project -c Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false "-p:AppxPackageDir=$output"
if ($LASTEXITCODE -ne 0) { throw 'MSIX 建置失敗' }
