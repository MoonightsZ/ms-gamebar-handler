# 建置、簽章與部署

CmdPal 專案以 [PowerToys 官方擴充套件範本](https://github.com/microsoft/PowerToys/tree/main/src/modules/cmdpal/ExtensionTemplate/TemplateCmdPalExtension) 為基礎，範本檔案及圖像保留 MIT 授權。目標為 Windows x64；引擎與管理工具使用 .NET 8 自包含發佈，擴充套件以 .NET 10 自包含發佈。

## 建置

需安裝 .NET 10 SDK；NuGet 還原會取得 Command Palette、Windows App SDK 與 Windows SDK BuildTools。無須先安裝系統層級 Windows SDK。

```powershell
.\scripts\build-msix.ps1
```

輸出位於 `dist\msix\MsGamebarHandlerExtension_0.7.0.0_x64_Test\`。檔名沿用專案名稱，但包內身分是 `Moonights.ms-gamebar-handler`。其中未簽章的 `.msix` 可提交到 Microsoft Store；不要把它當作一般使用者的直接下載安裝包。MSIX 內應有 `Payload\ms-gamebar-handler.exe`、`Payload\ms-gamebar-handler-cli.exe`，並在 `AppxManifest.xml` 中宣告 `com.microsoft.commandpalette` 及三個 `windows.protocol`。擴充套件專用的 Windows 應用入口以 `AppListEntry="none"` 隱藏，避免與 CmdPal 擴充命令同時出現在搜尋結果。

## Microsoft Store 提交

Partner Center 已保留產品 `9P2LHK1M4M0G`。清單使用商店分配的 `Identity Name="Moonights.ms-gamebar-handler"`、`Publisher="CN=A70DFBBD-5B3C-41A7-BF72-66C10479A4CD"` 和顯示名稱 `Moonights`；預期 PFN 為 `Moonights.ms-gamebar-handler_7btbqs5mh4z4t`。提交前請核對建置產物內的 `AppxManifest.xml` 與這些值一致，並在 Partner Center 對應產品的「套件」頁上傳未簽章的 `.msix`。商店通過認證後會以微軟憑證重新簽章；使用者從商店安裝無需匯入開發憑證。參見[微軟的 MSIX 上傳說明](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/upload-app-packages)和[套件簽章說明](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/app-certification-process)。

## 開發簽章

清單 Publisher 為商店分配的 `CN=A70DFBBD-5B3C-41A7-BF72-66C10479A4CD`。以下命令在目前使用者的個人憑證存放區產生**同名自簽測試憑證**，並把公開憑證輸出到 `dist\msix\ms-gamebar-handler-dev.cer`。私鑰不寫入專案。這一步僅供本機測試；提交商店的 `.msix` 不要執行此簽章步驟。

```powershell
.\scripts\sign-msix.ps1
```

自簽憑證只適合測試。要在目標 Windows 裝置安裝測試包，須先以系統管理員權限執行 `scripts\trust-msix-devcert.ps1 -PackagePath <MSIX 路徑>`，核對套件與公開憑證後匯入 `Cert:\LocalMachine\TrustedPeople`；省略路徑時會選擇 `dist\msix` 中最新建置的 MSIX。這會變更目標裝置的信任存放區。正式商店發佈由微軟重新簽章；若另行提供商店外下載，仍需受信任的正式簽章。參見 [微軟的 MSIX 簽章指南](https://learn.microsoft.com/en-us/windows/msix/package/sign-msix-package-guide) 與 [安裝疑難排解](https://learn.microsoft.com/en-us/windows/msix/msix-troubleshooting-guide)。

## 安裝與升級

PowerToys Command Palette 擴充套件本身是獨立的 MSIX 應用；CmdPal 透過套件清單發現它。安裝 MSIX 便會註冊三個協議，並以套件內的引擎處理 URI。首頁的「安裝管理工具」只複製命令列工具並初始化配置；「還原舊版手動註冊」只清理舊版 HKCU 登錄設定。參見[微軟的協議擴充文件](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-extensions)與[使用者提供的擴充套件文件](https://aka.ms/building-cmdpal-extensions)。開發時也可依[微軟的擴充套件入門文件](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/creating-an-extension)在 Visual Studio 使用 **Deploy**，再於 CmdPal 執行 **Reload Command Palette Extension**；單純 Build 不會更新已部署套件。

同一個 Windows 帳戶安裝較高版本時，直接執行 `Add-AppxPackage` 即可升級，毋須先解除安裝。升級要求新舊 MSIX 的套件家族名稱一致；從 0.7.0.0 起需維持新的商店 Name、Publisher，並提高 `Version`。**0.5.0 及更早的測試包使用舊套件家族，不能原地升級到商店版。**首次改用 0.7.0.0 或商店版前，先從 Windows「已安裝的應用」解除安裝舊測試包；使用者設定和事件紀錄保存在 `%LOCALAPPDATA%\ms-gamebar-handler`，卸載 MSIX 不會主動刪除它們。參見 [Add-AppxPackage 官方說明](https://learn.microsoft.com/en-us/powershell/module/appx/add-appxpackage)。

```powershell
$package = 'C:\path\to\MsGamebarHandlerExtension_0.7.0.0_x64.msix'
Add-AppxPackage -Path $package -ForceApplicationShutdown
Get-AppxPackage -Name Moonights.ms-gamebar-handler |
    Select-Object Name, PackageFamilyName, Version, PackageFullName
```

安裝後應只有一筆 `Moonights.ms-gamebar-handler`，其 `PackageFamilyName` 應為 `Moonights.ms-gamebar-handler_7btbqs5mh4z4t`。後續更新保持此 PFN、提高版本即可。`Get-StartApps | Where-Object Name -like '*gamebar*'` 可檢查開始功能表入口。如看到多筆同名應用，先比較各筆的 `PackageFamilyName`；不要僅憑安裝視窗再次顯示「安裝」判定為新應用。

在已信任測試憑證的開發機，可執行 `scripts\dev-deploy.ps1` 順序完成建置、簽章與當前使用者更新。腳本會比對專案及 MSIX 清單版本，並檢查更新後套件家族名稱與安裝數量；更新時直接使用 `Add-AppxPackage`，不先卸載舊版本。升級後請完全退出並重新啟動 Command Palette，避免舊版命令仍留在執行中的搜尋結果。微軟的[擴充套件入門文件](https://learn.microsoft.com/zh-cn/windows/powertoys/command-palette/creating-an-extension)亦指出，單純建置不會更新已部署套件，修改後須重新部署並手動重新載入；本機實測顯示升級 MSIX 後完整重啟才能清除舊命令。這一流程參考 [Home Assistant Command Palette](https://github.com/nickknissen/HomeAssistantCommandPalette) 的本機部署腳本；發行與安裝文件的分層參考 [Process Killer for CmdPal](https://github.com/8LWXpg/CmdPal-ProcessKiller)。

## 實機驗收

1. 在有 PowerToys Command Palette 的目標帳戶安裝已簽章 MSIX，重新載入 CmdPal，確認可找到 **ms-gamebar-handler**。
2. 確認 MSIX 清單含三個 `windows.protocol` 宣告；不需要另行執行手動註冊。
3. 連接手把及按 Win+G，確認不再出現「選擇應用程式」對話框，並查看最近事件。
4. 若有舊版手動註冊，執行「還原舊版手動註冊」並確認登錄設定恢復；要移除協議接管，從 Windows 設定卸載 MSIX。

套件宣告 `unvirtualizedResources`，讓 CmdPal 與套件內引擎存取使用者配置和事件記錄。CmdPal 首版只顯示安裝管理工具、還原舊版手動註冊與最近事件；規則管理暫不開放。實機驗收結果見 [驗收紀錄](TEST-RESULTS.md)。
