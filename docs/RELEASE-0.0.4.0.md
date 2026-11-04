# ms-gamebar Handler v0.0.4.0 預覽版

此版本收斂 PowerToys Command Palette 首頁，只顯示「安裝處理程式」「解除安裝處理程式」「最近事件」。配置、協議狀態和規則管理入口暫不開放。後續開放新增規則時，程式會自動產生規則 ID，無需使用者輸入。

協議處理器仍支援 `ms-gamebar:`、`ms-gamingoverlay:`、`ms-gamebarservices:`，預設靜默忽略。Steam 大螢幕預設規則尚未實作。

## 安裝

下載 x64 MSIX、`ms-gamebar-handler-dev.cer` 與 `SHA256SUMS.txt`。套件使用開發測試憑證簽章，只供測試；依[建置與部署文件](https://gitee.com/Moonights_gitee/ms-gamebar-handler/blob/main/docs/BUILD-AND-SIGN.md)信任公開憑證並安裝 MSIX，再從 Command Palette 執行「安裝」。

## 驗證

25 項自動測試通過，MSIX 已更新安裝至 Windows 11 測試帳戶。使用者確認首頁只顯示上述三個入口。安裝與解除安裝按鈕仍待逐項 UI 驗收，詳見[驗收紀錄](https://gitee.com/Moonights_gitee/ms-gamebar-handler/blob/main/docs/TEST-RESULTS.md)。
