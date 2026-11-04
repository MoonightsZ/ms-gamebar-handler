# v0.8.0

新增簡體中文（`zh-CN`，預設回退語言）、繁體中文（`zh-TW`）及美式英語（`en-US`）。Command Palette 首頁、最近事件、管理工具操作結果，以及命令列輸出會依 Windows 介面語言顯示；未支援的語言回退至簡體中文。

MSIX 清單的應用名稱和說明也提供三種語言，包內 `resources.pri` 包含全部翻譯。此版本使用單一 x64 MSIX，避免語言資源包在建置時遺漏。套件版本為 `0.8.0.0`，沿用 0.7.0 起的 Microsoft Store 套件家族，不改變三個 Game Bar 協議的處理方式。

商店上架文案見 [STORE-LISTING.md](STORE-LISTING.md)；需在 Partner Center 分別填入對應語言的商店頁面。規則管理與 Steam 大螢幕自動啟動仍未開放。
