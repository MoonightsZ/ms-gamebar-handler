# Windows 11 測試帳戶驗收紀錄

## 0.2.6.0 手把彈窗修正（2026-11-04）

- 舊版只在 HKCU 註冊 `shell\open\command`。`Start-Process 'ms-gamebar:'` 可觸發處理程式，但 `Windows.System.Launcher.QueryUriSupportAsync` 回報 `NotSupported`；使用者連接手把仍看到「取得打開此 ms-gamebar 鏈接的應用」。
- 參考兩個社群專案的傳統登錄方式後，改由 MSIX 清單宣告三個 `windows.protocol`，直接啟動套件內的處理程式。測試帳戶升級後，使用者確認重新連接手把不再出現彈窗。
- 在最終 0.2.6.0 套件中，三個協議的 `QueryUriSupportAsync` 均為 `Available`，`LaunchUriAsync` 均為 `True`，`events.log` 各新增一筆預設忽略事件。還原舊版 HKCU 手動註冊後，再測三個協議仍均可啟動並記錄事件。
- 開發機舊版 `events.log` 曾出現寫入視圖不一致；重新建立檔案後已可記錄 MSIX 協議啟動事件，原有記錄保留。
- 25 項自動測試通過；MSIX 建置 0 錯誤、3 項既有警告，簽章為 `Valid`；目前使用者僅安裝一筆 0.2.6.0，套件家族名稱保持 `MsGamebarHandlerExtension_t2xz08fyn448r`。

## 升級後搜尋結果重複（2026-11-04）

- 使用者截圖顯示兩筆同名的 CmdPal 搜尋結果；系統只有一筆 `MsGamebarHandlerExtension` 0.2.0.0 安裝記錄，開始功能表沒有此套件入口。
- CmdPal 同一份執行日誌先載入 0.0.6.0 的一條命令，升級後又載入 0.2.0.0 的一條命令。完整重啟後只載入 0.2.0.0 的一條命令；使用者確認搜尋結果只剩一筆。
- 已把升級後完整退出並重新啟動 CmdPal 的步驟加入使用者及開發者說明。

## 0.2.0.0 自包含安裝與部署流程（2026-11-04）

- 參考 Process Killer 與 Home Assistant Command Palette 的發行和本機部署方式；本專案保留相同套件家族的直接升級流程。
- 引擎與 CLI 均使用 .NET 8 自包含發佈；不需目標機另行安裝 .NET 8 Runtime。
- `scripts/dev-deploy.ps1` 在測試帳戶完成建置、簽章與更新安裝；套件家族名稱仍為 `MsGamebarHandlerExtension_t2xz08fyn448r`，目前只有一筆 0.2.0.0 安裝記錄，開始功能表入口為 0。
- 已簽章 MSIX 為約 73.5 MB，簽章狀態 `Valid`。獨立執行 CLI `--status` 成功，顯示三個協議指向原資料目錄中的處理程式。
- 25 項自動測試通過；再次執行部署腳本時，腳本在建置前正確拒絕同版本重複部署。

## 0.0.6.0 搜尋結果修正（2026-11-04）

- 使用者截圖顯示舊名稱與兩筆新名稱結果。當前帳戶只安裝一個 MSIX；CmdPal `state.json` 含舊名稱與新名稱的歷史項目，Windows 開始功能表另有應用入口。
- 套件的 `uap:VisualElements` 設為 `AppListEntry="none"`，使擴充套件只透過 CmdPal 擴充命令開啟。清理本機與此工具相關的歷史項目時保留其他命令的記錄。
- 25 項自動測試通過；0.0.6.0 的 MSIX 建置、簽章及更新安裝成功。`Get-StartApps` 中此套件入口為 0；CmdPal 日誌顯示 0.0.6.0 擴充套件載入 1 個命令，快取中的名稱為 `ms-gamebar-handler`。搜尋結果的最終畫面仍待 UI 複核。

## 0.0.5.0 名稱與升級驗證（2026-11-04）

- 對外顯示名稱改為 `ms-gamebar-handler`；MSIX `Identity Name` 與 `Publisher` 保持不變。
- 25 項自動測試通過；x64 MSIX 建置與簽章驗證通過。
- 從 0.0.4.0 直接升級至 0.0.5.0 後，`PackageFamilyName` 均為 `MsGamebarHandlerExtension_t2xz08fyn448r`，目前帳戶只有一筆安裝記錄；開始功能表僅有一個 `ms-gamebar-handler` 入口。
- Command Palette 中的新顯示名稱、安裝及解除安裝按鈕仍待逐項 UI 驗收。

## 0.0.4.0 介面調整（2026-11-04）

- CmdPal 首頁只保留「安裝處理程式」「解除安裝處理程式」「最近事件」；配置、協議狀態及規則入口已隱藏。
- 規則表單保留供後續開放；新增規則時自動產生 ID，編輯時沿用原 ID。
- 25 項自動測試通過；x64 MSIX 建置與簽章驗證通過，已更新安裝至測試帳戶。
- 使用者手動確認 CmdPal 首頁只顯示上述三個入口。安裝與解除安裝按鈕仍待逐項 UI 驗收。

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
