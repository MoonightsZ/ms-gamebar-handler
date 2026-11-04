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
$signer = $signature.SignerCertificate
$certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certificatePath)
if (!$signer -or $signer.Thumbprint -ne $certificate.Thumbprint -or $certificate.Subject -ne 'CN=A70DFBBD-5B3C-41A7-BF72-66C10479A4CD') { throw '套件簽章與公開憑證不一致' }
# 首次安裝時簽章狀態會因自簽根憑證尚未受信任而顯示 UnknownError。
# 只在驗證期間暫時信任這張已核對的憑證；無論驗證結果如何，都恢復原本的根憑證存放區。
$rootStore = [System.Security.Cryptography.X509Certificates.X509Store]::new([System.Security.Cryptography.X509Certificates.StoreName]::Root, [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser)
$rootStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
$alreadyTrusted = @($rootStore.Certificates.Find([System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint, $certificate.Thumbprint, $false)).Count -gt 0
try {
    if (!$alreadyTrusted) { $rootStore.Add($certificate) }
    $verified = Get-AuthenticodeSignature -LiteralPath $PackagePath
    if ($verified.Status -ne 'Valid') { throw "套件簽章無效：$($verified.Status)" }
} finally {
    if (!$alreadyTrusted) { $rootStore.Remove($certificate) }
    $rootStore.Close()
}
Import-Certificate -FilePath $certificatePath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -ErrorAction Stop | Out-Null
Write-Output "已信任測試憑證：$($certificate.Thumbprint)"
