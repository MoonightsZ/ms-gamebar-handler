# v0.7.0

此版本將 MSIX 身分改為 Microsoft Store 中已保留的 `Moonights.ms-gamebar-handler`，發佈者為 `CN=A70DFBBD-5B3C-41A7-BF72-66C10479A4CD`，套件版本為 `0.7.0.0`，產品 Store ID 為 `9P2LHK1M4M0G`。

本次更新準備商店提交，建置腳本產生未簽章 MSIX 供 Partner Center 上傳；本機開發測試仍可使用同名自簽憑證簽章，該測試包不適合直接分發給一般使用者。通過商店認證後，微軟會重新簽章商店套件。

舊版測試套件的 Name 為 `MsGamebarHandlerExtension`，與新版包家族不同。首次切換到商店版須先卸載舊測試套件，避免兩個同名 Command Palette 擴充套件及協議處理程式並存；使用者配置與事件紀錄位於 `%LOCALAPPDATA%\ms-gamebar-handler`。

規則管理及 Steam 大螢幕自動啟動仍未開放。
