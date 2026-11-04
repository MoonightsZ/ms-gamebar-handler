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
$certificatePath = Join-Path $projectRoot 'dist\msix\ms-gamebar-handler-dev.cer'
if (!(Test-Path -LiteralPath $certificatePath)) { throw '找不到公開憑證' }
$signature = Get-AuthenticodeSignature -LiteralPath $PackagePath
if ($signature.Status -ne 'Valid') { throw "套件簽章無效：$($signature.Status)" }
$signer = $signature.SignerCertificate
$certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certificatePath)
if (!$signer -or $signer.Thumbprint -ne $certificate.Thumbprint -or $certificate.Subject -ne 'CN=A70DFBBD-5B3C-41A7-BF72-66C10479A4CD') { throw '套件簽章與公開憑證不一致' }
Import-Certificate -FilePath $certificatePath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -ErrorAction Stop | Out-Null
Write-Output "已信任測試憑證：$($certificate.Thumbprint)"
