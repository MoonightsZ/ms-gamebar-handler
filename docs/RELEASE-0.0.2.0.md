# ms-gamebar Handler v0.0.2.0 測試版

首個 Windows x64 測試版。安裝後可接管目前使用者的 `ms-gamebar:`、`ms-gamingoverlay:` 和 `ms-gamebarservices:` 協議；預設靜默忽略。Steam 大螢幕規則不在首版範圍內。

## 內容

- 獨立協議處理引擎與命令列管理工具；支援規則匹配、冷卻、事件日誌、安全註冊與原狀還原。
- PowerToys Command Palette 擴充套件，提供安裝、解除安裝、狀態、最近事件及規則管理入口。
- 針對從 MSIX 擴充套件執行安裝時的 CLI 路徑錯誤，排除專案資料目錄及三個協議機碼的虛擬化。

## 安裝

下載 x64 MSIX 與 `ms-gamebar-handler-dev.cer`，並用 `SHA256SUMS.txt` 核對檔案。此套件使用開發測試憑證簽章，只供測試；先核對來源，依[建置與部署文件](https://gitee.com/Moonights_gitee/ms-gamebar-handler/blob/main/docs/BUILD-AND-SIGN.md)將公開憑證匯入測試機的 `LocalMachine\TrustedPeople`，再以目標使用者安裝 MSIX。在 PowerToys Command Palette 找到 **ms-gamebar Handler**，執行「安裝」以註冊三個協議。

## 驗收狀態

25 項自動測試通過；Windows 11 測試帳戶已驗證三協議 Shell 呼叫、回滾及使用者手動觸發無彈窗。0.0.2.0 的 CmdPal 安裝操作及其他表單按鈕尚待逐項 UI 驗收，詳見[驗收紀錄](https://gitee.com/Moonights_gitee/ms-gamebar-handler/blob/main/docs/TEST-RESULTS.md)。
