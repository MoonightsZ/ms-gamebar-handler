# ms-gamebar-handler v0.0.6.0 預覽版

修正 Command Palette 搜尋結果同時列出應用入口與擴充命令的問題。擴充套件專用的 Windows 開始功能表入口現已隱藏；CmdPal 擴充命令仍保留。

先前版本的搜尋歷史可能繼續顯示舊名稱。本機已清理該工具的舊歷史項目，並保留其他命令的使用記錄。更新套件時維持相同 MSIX 身分，直接安裝即可升級。

下載 x64 MSIX、`ms-gamebar-handler-dev.cer` 與 `SHA256SUMS.txt`，依 [README](https://gitee.com/Moonights_gitee/ms-gamebar-handler#readme) 安裝。此版本仍使用開發測試憑證；Command Palette 中的安裝及解除安裝按鈕仍待逐項 UI 驗收。
