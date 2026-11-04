# ms-gamebar-handler v0.2.0 預覽版

此版本的 MSIX 版本號為 `0.2.0.0`。安裝套件中的引擎與管理工具均改為自包含發佈，使用者無須另外安裝 .NET Runtime。套件身分維持不變，可直接從 `0.0.6.0` 更新。

開發流程新增 `scripts\dev-deploy.ps1`，順序完成建置、簽章和升級，並檢查版本與套件家族名稱。使用者安裝方式見 [README](https://gitee.com/Moonights_gitee/ms-gamebar-handler#readme)。此預覽版仍以開發測試憑證簽章。

25 項自動測試通過。本機已升級至 `0.2.0.0`，僅有一筆 MSIX 安裝記錄；開始功能表無多餘入口。Command Palette 中安裝與解除安裝按鈕仍待逐項 UI 驗收。
