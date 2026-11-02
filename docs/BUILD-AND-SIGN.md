# 建置、簽章與部署

CmdPal 專案以 [PowerToys 官方擴充套件範本](https://github.com/microsoft/PowerToys/tree/main/src/modules/cmdpal/ExtensionTemplate/TemplateCmdPalExtension) 為基礎，範本檔案及圖像保留 MIT 授權。目標為 Windows x64；引擎使用 .NET 8，擴充套件以 .NET 10 自包含發佈。

## 建置

需安裝 .NET 10 SDK；NuGet 還原會取得 Command Palette、Windows App SDK 與 Windows SDK BuildTools。無須先安裝系統層級 Windows SDK。

```powershell
.\scripts\build-msix.ps1
```

輸出位於 `dist\msix\MsGamebarHandlerExtension_0.0.1.0_x64_Test\`。MSIX 內應有 `Payload\ms-gamebar-handler.exe`、`Payload\ms-gamebar-handler-cli.exe`，並在 `AppxManifest.xml` 中宣告 `com.microsoft.commandpalette`。

## 開發簽章

清單 Publisher 為 `CN=ms-gamebar-handler-dev`。以下命令在目前使用者的個人憑證存放區產生測試簽章憑證，並把公開憑證輸出到 `dist\msix\ms-gamebar-handler-dev.cer`。私鑰不寫入專案。

```powershell
.\scripts\sign-msix.ps1
```

自簽憑證只適合測試。要在目標 Windows 裝置安裝，須先以系統管理員權限執行 `scripts\trust-msix-devcert.ps1`，核對套件與公開憑證後匯入 `Cert:\LocalMachine\TrustedPeople`，再以目標使用者執行 `Add-AppxPackage`；此步驟會變更目標裝置的信任存放區。正式發佈應換成符合發佈身分的簽章方式，並同步修改清單 Publisher。參見 [微軟的 MSIX 簽章指南](https://learn.microsoft.com/en-us/windows/msix/package/sign-msix-package-guide) 與 [安裝疑難排解](https://learn.microsoft.com/en-us/windows/msix/msix-troubleshooting-guide)。

## 實機驗收

1. 在有 PowerToys Command Palette 的目標帳戶安裝已簽章 MSIX，重新載入 CmdPal，確認可找到 **ms-gamebar Handler**。
2. 在擴充套件中執行安裝，確認三個 HKCU 協議命令指向 `%LOCALAPPDATA%\ms-gamebar-handler\bin\ms-gamebar-handler.exe`。
3. 新增、修改、啟停、排序及刪除一條測試規則；確認 `config.json` 有效且 `--test` 命中預期規則。
4. 連接手把及按 Win+G，確認不再出現「選擇應用程式」對話框，並查看最近事件。
5. 執行解除安裝，確認三個協議回到原有狀態。

建置、簽章、套件內容、測試帳戶安裝與三協議 Shell 呼叫已驗證；使用者亦確認 Win+G／手把沒有彈窗，且可從 Command Palette 進入規則管理頁。CmdPal 各項表單與按鈕尚待逐項 UI 驗收。詳見 [驗收紀錄](TEST-RESULTS.md)。
