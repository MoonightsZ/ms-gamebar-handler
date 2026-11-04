$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $projectRoot 'cmdpal-extension\MsGamebarHandlerExtension\Package.appxmanifest'
$projectPath = Join-Path $projectRoot 'cmdpal-extension\MsGamebarHandlerExtension\MsGamebarHandlerExtension.csproj'
[xml]$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8
[xml]$project = Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8
$identity = $manifest.Package.Identity
$version = $identity.Version
$projectVersions = @($project.Project.PropertyGroup | ForEach-Object { $_.AppxPackageVersion } | Where-Object { $_ })
if ($projectVersions.Count -ne 1 -or $projectVersions[0] -ne $version) {
    throw 'MSIX 清單與專案版本不一致'
}
$before = @(Get-AppxPackage -Name $identity.Name)
if ($before.Count -gt 1) { throw '目前使用者有多筆同名套件，請先檢查' }
if ($before.Count -eq 1 -and [version]$before[0].Version -ge [version]$version) {
    throw "已安裝 $($before[0].Version)；請提高 MSIX 版本後再部署"
}

$packagePath = Join-Path $projectRoot "dist\msix\$($identity.Name)_${version}_x64_Test\$($identity.Name)_${version}_x64.msix"
& (Join-Path $PSScriptRoot 'build-msix.ps1')
if ($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $packagePath)) { throw 'MSIX 建置失敗' }
& (Join-Path $PSScriptRoot 'sign-msix.ps1') -PackagePath $packagePath
if ($LASTEXITCODE -ne 0) { throw 'MSIX 簽章失敗' }

Add-AppxPackage -Path $packagePath -ForceApplicationShutdown -ErrorAction Stop
$after = @(Get-AppxPackage -Name $identity.Name)
if ($after.Count -ne 1 -or $after[0].Version.ToString() -ne $version) {
    throw '安裝後的套件版本或數量不符'
}
if ($before.Count -eq 1 -and $before[0].PackageFamilyName -ne $after[0].PackageFamilyName) {
    throw '套件家族名稱改變，無法視為升級'
}
Write-Output "已部署：$($after[0].PackageFullName)"
Write-Output '請在 Command Palette 執行 Reload Command Palette Extension，以重新載入擴充套件。'
