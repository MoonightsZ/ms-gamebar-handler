$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$output = Join-Path $projectRoot 'dist\win-x64'

dotnet publish (Join-Path $projectRoot 'engine\MsGamebarHandler.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -o $output
if ($LASTEXITCODE -ne 0) { throw '引擎建置失敗' }
dotnet publish (Join-Path $projectRoot 'cli\MsGamebarHandler.Cli.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -o $output
if ($LASTEXITCODE -ne 0) { throw '命令列工具建置失敗' }
Write-Output $output
