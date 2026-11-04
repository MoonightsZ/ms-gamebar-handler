# ms-gamebar Handler

接管目前使用者的 `ms-gamebar:`、`ms-gamingoverlay:`、`ms-gamebarservices:` 協議，預設靜默忽略。首版沒有 Steam 大螢幕預設規則。

## 建置與安裝

需要 Windows、.NET 10 SDK；管理工具執行時需要 .NET 8 Runtime。以 PowerShell 執行：

```powershell
.\scripts\build.ps1
.\scripts\install.ps1
```

安裝器將引擎放在 `%LOCALAPPDATA%\ms-gamebar-handler\bin`，只寫入 HKCU 的三個協議。若已有處理程式，安裝會拒絕覆寫。原始註冊狀態存於 `registry-backup.json`，重複安裝不覆蓋它。

## 驗證與解除安裝

```powershell
$cli = "$env:LOCALAPPDATA\ms-gamebar-handler\bin\ms-gamebar-handler-cli.exe"
& $cli --status
& $cli --test 'ms-gamebar://gamepadConnect/?VID=1118'
.\scripts\uninstall.ps1
```

`--test` 只顯示匹配結果，不執行動作。事件日誌位於 `%LOCALAPPDATA%\ms-gamebar-handler\events.log`，不記錄完整 URI；錯誤另記於 `errors.log`。規則功能仍在開發中，首版不提供使用者入口。

## 驗證

```powershell
dotnet run --project .\tests\HandlerTests.csproj
```

在目標 Windows 帳戶安裝後，插拔手把與按 Win+G，確認沒有「選擇應用程式」對話框；再執行解除安裝，確認原有協議狀態恢復。CmdPal 首版只顯示安裝、解除安裝與最近事件；規則管理暫不開放。MSIX 的建置、簽章和目標機驗收步驟見 [建置文件](docs/BUILD-AND-SIGN.md)。
