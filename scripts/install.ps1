$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$source = Join-Path $projectRoot 'dist\win-x64'
$data = Join-Path $env:LOCALAPPDATA 'ms-gamebar-handler'
$target = Join-Path $data 'bin'
$handlerSource = Join-Path $source 'ms-gamebar-handler.exe'
$cliSource = Join-Path $source 'ms-gamebar-handler-cli.exe'
if (!(Test-Path -LiteralPath $handlerSource) -or !(Test-Path -LiteralPath $cliSource)) { throw '請先執行 scripts\build.ps1' }
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -LiteralPath $handlerSource -Destination (Join-Path $target 'ms-gamebar-handler.exe') -Force
Copy-Item -LiteralPath $cliSource -Destination (Join-Path $target 'ms-gamebar-handler-cli.exe') -Force
$cli = Join-Path $target 'ms-gamebar-handler-cli.exe'
& $cli --init
if ($LASTEXITCODE -ne 0) { throw '無法初始化配置' }
& $cli --register
if ($LASTEXITCODE -ne 0) { throw '註冊失敗；請檢查現有協議處理程式' }
& $cli --status
if ($LASTEXITCODE -ne 0) { throw '狀態檢查失敗' }
