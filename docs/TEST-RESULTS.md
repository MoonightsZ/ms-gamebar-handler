# Windows 11 測試帳戶驗收紀錄（2026-11-02）

## 0.0.2.0 修正驗證

- 使用者回報：從 CmdPal 執行安裝時，`.NET` 主機提示無法解析 `%LOCALAPPDATA%\ms-gamebar-handler\bin\ms-gamebar-handler-cli.exe` 的完整路徑。
- 檢查發現套件私有 `LocalCache\Local\ms-gamebar-handler\bin` 另有 CLI 副本；MSIX 對 AppData 的寫入虛擬化是差異來源。
- 0.0.2.0 的清單已排除該資料目錄與三個 HKCU 協議機碼的虛擬化；MSIX 建置及簽章驗證通過，已更新安裝至測試帳戶。
- 從新版 CmdPal 再次執行安裝並檢查實際路徑，仍待確認。

## 已驗證

- `dotnet run --project tests/HandlerTests.csproj`：規則、冷卻、配置校驗與隔離登錄機碼測試通過。
- `scripts/build-msix.ps1`：產生 x64 MSIX；內含兩個引擎執行檔、`AppxManifest.xml` 與 CmdPal 擴充宣告。`scripts/sign-msix.ps1` 簽章後，系統回報簽章 `Valid`。
- `Add-AppxPackage`：`MsGamebarHandlerExtension` 0.0.1.0 已安裝於測試帳戶。Command Palette 的 `commandProviderCache.json` 已列出 **ms-gamebar Handler**。
- 安裝前 HKCU／HKCR 三個協議均無處理命令。執行 `scripts/install.ps1` 後，三個 HKCU 命令都指向 `%LOCALAPPDATA%\ms-gamebar-handler\bin\ms-gamebar-handler.exe --invoke "%1"`。
- 透過 Windows Shell 分別呼叫 `ms-gamebar:`、`ms-gamingoverlay:`、`ms-gamebarservices:` 測試 URI；`events.log` 各新增一筆 `action=ignore`，未觀察到選擇應用程式視窗。
- 執行 `scripts/uninstall.ps1` 後，三個命令都回到 `(none)`，註冊備份被移除而配置保留；隨後重新安裝，三個協議目前保持啟用。
- 以已安裝管理工具新增一條 `gamepadConnect` 測試規則，`--test` 與 Shell 呼叫均命中該規則；停用後回到 `default`，刪除後 `events` 再次為空陣列。

## 使用者手動確認

- 在測試帳戶按 Win+G 或重新連接手把後，沒有出現「選擇應用程式」彈窗。
- PowerToys Command Palette 中可搜尋到 **ms-gamebar Handler**，並能進入規則管理頁。

CmdPal 表單儲存、啟停、排序、刪除及擴充套件中的安裝／解除安裝按鈕尚未逐項以 UI 操作；相應的管理工具與配置邏輯已通過自動及 Shell 測試。

## 本機狀態

- MSIX 與三個協議處理程式目前已安裝，預設配置沒有規則，會靜默忽略。
- 開發測試憑證 `CN=ms-gamebar-handler-dev` 需在測試機的 `LocalMachine\TrustedPeople` 受信任；正式發佈須改用正式簽章身分。
