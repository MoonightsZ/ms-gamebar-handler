param([string]$PackagePath)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$packageRoot = Join-Path $projectRoot 'dist\msix'
if (!$PackagePath) {
    $package = Get-ChildItem -LiteralPath $packageRoot -Recurse -Filter '*.msix' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (!$package) { throw '找不到 MSIX；請先執行 scripts\build-msix.ps1' }
    $PackagePath = $package.FullName
}
$PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
$subject = 'CN=ms-gamebar-handler-dev'
$cert = Get-ChildItem -Path 'Cert:\CurrentUser\My' | Where-Object { $_.Subject -eq $subject -and $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date).AddDays(30) } | Sort-Object NotAfter -Descending | Select-Object -First 1
if (!$cert) {
    $cert = New-SelfSignedCertificate -Type Custom -Subject $subject -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -CertStoreLocation 'Cert:\CurrentUser\My' -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
}
$publicPath = Join-Path $packageRoot 'ms-gamebar-handler-dev.cer'
Export-Certificate -Cert $cert -FilePath $publicPath -Force | Out-Null
$packages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' }
$signTool = Join-Path $packages 'microsoft.windows.sdk.buildtools\10.0.26100.4188\bin\10.0.26100.0\x64\signtool.exe'
if (!(Test-Path -LiteralPath $signTool)) { throw '找不到 SignTool；請先建置 CmdPal 專案以還原 NuGet 套件' }
& $signTool sign /fd SHA256 /sha1 $cert.Thumbprint /s My $PackagePath
if ($LASTEXITCODE -ne 0) { throw 'MSIX 簽章失敗' }
$signature = Get-AuthenticodeSignature -LiteralPath $PackagePath
if ($signature.SignerCertificate.Thumbprint -ne $cert.Thumbprint) { throw '簽章憑證與預期不符' }
Write-Output "已簽章：$PackagePath"
Write-Output "公開憑證：$publicPath"
