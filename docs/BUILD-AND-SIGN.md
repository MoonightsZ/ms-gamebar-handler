# 建置、簽章與部署

CmdPal 專案以 [PowerToys 官方擴充套件範本](https://github.com/microsoft/PowerToys/tree/main/src/modules/cmdpal/ExtensionTemplate/TemplateCmdPalExtension) 為基礎，範本檔案及圖像保留 MIT 授權。目標為 Windows x64；引擎與管理工具使用 .NET 8 自包含發佈，擴充套件以 .NET 10 自包含發佈。

## 建置

需安裝 .NET 10 SDK；NuGet 還原會取得 Command Palette、Windows App SDK 與 Windows SDK BuildTools。無須先安裝系統層級 Windows SDK。

```powershell
.\scripts\build-msix.ps1
```

輸出位於 `dist\msix\MsGamebarHandlerExtension_0.2.0.0_x64_Test\`。MSIX 內應有 `Payload\ms-gamebar-handler.exe`、`Payload\ms-gamebar-handler-cli.exe`，並在 `AppxManifest.xml` 中宣告 `com.microsoft.commandpalette`。擴充套件專用的 Windows 應用入口以 `AppListEntry="none"` 隱藏，避免與 CmdPal 擴充命令同時出現在搜尋結果。

## 開發簽章

清單 Publisher 為 `CN=ms-gamebar-handler-dev`。以下命令在目前使用者的個人憑證存放區產生測試簽章憑證，並把公開憑證輸出到 `dist\msix\ms-gamebar-handler-dev.cer`。私鑰不寫入專案。

```powershell
.\scripts\sign-msix.ps1
```

自簽憑證只適合測試。要在目標 Windows 裝置安裝，須先以系統管理員權限執行 `scripts\trust-msix-devcert.ps1 -PackagePath <MSIX 路徑>`，核對套件與公開憑證後匯入 `Cert:\LocalMachine\TrustedPeople`；省略路徑時會選擇 `dist\msix` 中最新建置的 MSIX。這會變更目標裝置的信任存放區。正式發佈應換成符合發佈身分的簽章方式，並同步修改清單 Publisher。參見 [微軟的 MSIX 簽章指南](https://learn.microsoft.com/en-us/windows/msix/package/sign-msix-package-guide) 與 [安裝疑難排解](https://learn.microsoft.com/en-us/windows/msix/msix-troubleshooting-guide)。

## 安裝與升級

PowerToys Command Palette 擴充套件本身是獨立的 MSIX 應用；CmdPal 透過套件清單發現它。擴充套件首頁的「安裝處理程式」只部署協議引擎並註冊三個協議，不負責安裝或升級 MSIX。參見[使用者提供的微軟擴充套件文件](https://aka.ms/building-cmdpal-extensions)。開發時也可依[微軟的擴充套件入門文件](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/creating-an-extension)在 Visual Studio 使用 **Deploy**，再於 CmdPal 執行 **Reload Command Palette Extension**；單純 Build 不會更新已部署套件。

同一個 Windows 帳戶安裝較高版本時，直接執行 `Add-AppxPackage` 即可升級，毋須先解除安裝。升級要求新舊 MSIX 的套件家族名稱一致；本專案必須維持 `Identity Name="MsGamebarHandlerExtension"`、`Publisher="CN=ms-gamebar-handler-dev"`，並提高 `Version`。若換用正式簽章並變更 Publisher，Windows 會視為另一套件家族，需另行規劃遷移。參見 [Add-AppxPackage 官方說明](https://learn.microsoft.com/en-us/powershell/module/appx/add-appxpackage)。

```powershell
$package = 'C:\path\to\MsGamebarHandlerExtension_0.2.0.0_x64.msix'
Add-AppxPackage -Path $package -ForceApplicationShutdown
Get-AppxPackage -Name MsGamebarHandlerExtension |
    Select-Object Name, PackageFamilyName, Version, PackageFullName
```

安裝後應只有一筆 `MsGamebarHandlerExtension`，其中 `PackageFamilyName` 保持不變、`Version` 更新。`Get-StartApps | Where-Object Name -like '*gamebar*'` 可檢查開始功能表入口。如看到多筆同名應用，先比較各筆的 `PackageFamilyName`；不要僅憑安裝視窗再次顯示「安裝」判定為新應用。

在已信任測試憑證的開發機，可執行 `scripts\dev-deploy.ps1` 順序完成建置、簽章與當前使用者更新。腳本會比對專案及 MSIX 清單版本，並檢查更新後套件家族名稱與安裝數量；更新時直接使用 `Add-AppxPackage`，不先卸載舊版本。部署後在 Command Palette 執行 `Reload Command Palette Extension`。微軟的[擴充套件入門文件](https://learn.microsoft.com/zh-cn/windows/powertoys/command-palette/creating-an-extension)亦指出，單純建置不會更新已部署套件，修改後須重新部署並手動重新載入。這一流程參考 [Home Assistant Command Palette](https://github.com/nickknissen/HomeAssistantCommandPalette) 的本機部署腳本；發行與安裝文件的分層參考 [Process Killer for CmdPal](https://github.com/8LWXpg/CmdPal-ProcessKiller)。

## 實機驗收

1. 在有 PowerToys Command Palette 的目標帳戶安裝已簽章 MSIX，重新載入 CmdPal，確認可找到 **ms-gamebar-handler**。
2. 在擴充套件中執行安裝，確認三個 HKCU 協議命令指向 `%LOCALAPPDATA%\ms-gamebar-handler\bin\ms-gamebar-handler.exe`。
3. 連接手把及按 Win+G，確認不再出現「選擇應用程式」對話框，並查看最近事件。
4. 執行解除安裝，確認三個協議回到原有狀態。

套件宣告 `unvirtualizedResources`，讓 CmdPal 寫入的引擎檔案、配置及三個 HKCU 協議命令可被套件外的 Windows Shell 存取。CmdPal 首版只顯示安裝、解除安裝與最近事件；規則管理暫不開放。先前已驗證三協議 Shell 呼叫，以及使用者手動觸發無彈窗。新版 CmdPal 按鈕仍待 UI 驗收，詳見 [驗收紀錄](TEST-RESULTS.md)。
