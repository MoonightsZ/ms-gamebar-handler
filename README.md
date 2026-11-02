# ms-gamebar Handler

接管目前使用者的 `ms-gamebar:`、`ms-gamingoverlay:`、`ms-gamebarservices:` 協議，預設靜默忽略。首版沒有 Steam 大螢幕預設規則。

## 建置與安裝

需要 Windows、.NET 10 SDK；管理工具執行時需要 .NET 8 Runtime。以 PowerShell 執行：

```powershell
.\scripts\build.ps1
.\scripts\install.ps1
```

安裝器將引擎放在 `%LOCALAPPDATA%\ms-gamebar-handler\bin`，只寫入 HKCU 的三個協議。若已有處理程式，安裝會拒絕覆寫。原始註冊狀態存於 `registry-backup.json`，重複安裝不覆蓋它。

## 管理與測試

```powershell
$cli = "$env:LOCALAPPDATA\ms-gamebar-handler\bin\ms-gamebar-handler-cli.exe"
& $cli --status
& $cli --test 'ms-gamebar://gamepadConnect/?VID=1118'
& $cli --config
.\scripts\uninstall.ps1
```

`--test` 只顯示匹配結果，不執行動作。配置在 `%LOCALAPPDATA%\ms-gamebar-handler\config.json`，可直接編輯；引擎每次呼叫時重新讀取。預設內容為：

```json
{
  "version": 1,
  "log": true,
  "allowRun": false,
  "allowedExecutables": [],
  "defaultAction": { "type": "ignore", "value": "", "arguments": "" },
  "events": []
}
```

規則支援 `any`、`contains`、`equals`、`protocol`、`regex` 匹配，依陣列順序取第一條啟用且命中的規則。動作支援 `ignore`、`launch-uri`、`run`；`run` 同時需要 `allowRun: true` 和 `allowedExecutables` 中列明完整程式路徑。事件日誌只記錄協議、規則與動作，不記錄完整 URI；錯誤另記於 `errors.log`。

## 驗證

```powershell
dotnet run --project .\tests\HandlerTests.csproj
```

在目標 Windows 帳戶安裝後，插拔手把與按 Win+G，確認沒有「選擇應用程式」對話框；再執行解除安裝，確認原有協議狀態恢復。CmdPal 擴充套件提供安裝、解除安裝、開啟配置、狀態、最近事件與規則管理；MSIX 的建置、簽章和目標機驗收步驟見 [建置文件](docs/BUILD-AND-SIGN.md)。
