# ms-gamebar-handler v0.0.5.0 預覽版

此版本將 Windows 應用及 PowerToys Command Palette 中的顯示名稱統一為 **ms-gamebar-handler**。MSIX 套件身分維持不變，可直接從 0.0.4.0 升級；無須先解除安裝。

README 現以一般使用者的下載、安裝、啟用與驗證流程為主，並說明 MSIX 安裝與「安裝處理程式」的差別。規則管理及 Steam 大螢幕預設規則仍未開放。

## 下載與安裝

下載 x64 MSIX、`ms-gamebar-handler-dev.cer` 及 `SHA256SUMS.txt`，依 [README](https://gitee.com/Moonights_gitee/ms-gamebar-handler#readme) 完成測試憑證信任、MSIX 安裝及協議處理程式啟用。套件使用開發測試憑證簽章，只供測試。

## 驗證

25 項自動測試通過；本機從 0.0.4.0 升級至 0.0.5.0 後，套件家族名稱不變，當前帳戶只有一筆安裝記錄，開始功能表顯示新名稱。Command Palette 中的安裝及解除安裝按鈕仍待逐項 UI 驗收。
