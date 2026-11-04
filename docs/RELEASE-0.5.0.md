# v0.5.0

此版加入參考 Windows Game Bar 視窗輪廓繪製的原創圖示，更新 MSIX 的各尺寸圖像與 Command Palette 圖示，並在主要程式路徑補充繁體中文註解。README 新增給初學者看的請求處理流程。

協議接管沿用已驗證的 MSIX 宣告：安裝套件即可處理 `ms-gamebar:`、`ms-gamingoverlay:` 與 `ms-gamebarservices:`，預設靜默忽略並記錄事件。規則介面與 Steam 大螢幕自動啟動仍未開放。

修正三個協議幾乎同時啟動時，事件檔短暫被占用而漏記事件的問題。處理程序現在會對短暫的檔案分享衝突重試；其他 I/O 錯誤仍會回報。

下載同一發行版中的 MSIX、測試憑證及 SHA-256 清單，依 [README](../README.md) 安裝。這是使用開發測試憑證的預覽版。
