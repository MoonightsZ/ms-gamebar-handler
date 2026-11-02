$ErrorActionPreference = 'Stop'
$data = Join-Path $env:LOCALAPPDATA 'ms-gamebar-handler'
$target = Join-Path $data 'bin'
$cli = Join-Path $target 'ms-gamebar-handler-cli.exe'
if (!(Test-Path -LiteralPath $cli)) { throw '找不到已安裝的管理工具' }
& $cli --unregister
if ($LASTEXITCODE -ne 0) { throw '協議還原失敗；已保留執行檔與備份供檢查' }
Remove-Item -LiteralPath (Join-Path $target 'ms-gamebar-handler.exe') -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $cli -ErrorAction SilentlyContinue
Write-Output '已解除註冊；配置與日誌保留於資料目錄'
