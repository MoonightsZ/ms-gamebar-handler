$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$packagePath = Join-Path $projectRoot 'dist\msix\MsGamebarHandlerExtension_0.0.1.0_x64_Test\MsGamebarHandlerExtension_0.0.1.0_x64.msix'
$certificatePath = Join-Path $projectRoot 'dist\msix\ms-gamebar-handler-dev.cer'
if (!(Test-Path -LiteralPath $packagePath) -or !(Test-Path -LiteralPath $certificatePath)) { throw '找不到已簽章套件或公開憑證' }
$signer = (Get-AuthenticodeSignature -LiteralPath $packagePath).SignerCertificate
$certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certificatePath)
if (!$signer -or $signer.Thumbprint -ne $certificate.Thumbprint -or $certificate.Subject -ne 'CN=ms-gamebar-handler-dev') { throw '套件簽章與公開憑證不一致' }
Import-Certificate -FilePath $certificatePath -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' -ErrorAction Stop | Out-Null
Write-Output "已信任測試憑證：$($certificate.Thumbprint)"
