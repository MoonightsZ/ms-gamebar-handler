# ms-gamebar-handler

[更新日志](CHANGELOG.md)

![ms-gamebar-handler 图标](cmdpal-extension/MsGamebarHandlerExtension/Assets/Square150x150Logo.scale-200.png)

让 Windows 接收游戏手柄或 `Win + G` 发出的 Game Bar 请求时，不再反复弹出“选择应用”窗口。安装后，这些请求默认被安静地忽略。项目通过 [PowerToys Command Palette](https://learn.microsoft.com/windows/powertoys/command-palette/overview) 提供操作入口。

> 当前是预览版，只支持 Windows x64。规则编辑和 Steam 大屏幕自动启动尚未开放。

> 0.8.0 已采用 Microsoft Store 分配的正式包身份，并提供简体中文（默认）、繁体中文和美式英语界面。发行版附件仍使用测试证书签名，安装前需要信任随附的 `.cer`；商店提交包则单独保持未签名。0.5.0 及更早的测试包使用旧身份，切换到 0.8.0 前需先卸载旧包。

## 快速开始

### 1. 准备环境

- Windows 11 x64，以及已启用 Command Palette 的 [Microsoft PowerToys](https://learn.microsoft.com/windows/powertoys/install)。
- 当前预览包已包含运行所需的 .NET 组件，无需另外安装 .NET Runtime。

### 2. 下载并安装扩展

打开 [发行版页面](https://gitee.com/Moonights_gitee/ms-gamebar-handler/releases)，选择最新预览版，下载同一版本下的三个附件：

1. `MsGamebarHandlerExtension_…_x64.msix`：Windows 安装包。
2. `ms-gamebar-handler-dev.cer`：供预览版使用的测试证书。
3. `SHA256SUMS.txt`：用于核对下载文件是否完整。

先核对下载文件的 SHA-256 值与 `SHA256SUMS.txt` 一致。可以在下载文件夹打开 PowerShell，运行 `Get-FileHash .\文件名 -Algorithm SHA256`。

这个预览版使用测试证书签名。双击 `.cer` 文件，选择“安装证书”→“本地计算机”→“将所有的证书都放入下列存储”→“受信任人”，按 Windows 提示完成操作。然后双击 `.msix` 安装。信任测试证书会影响整台电脑；只对从本项目发行页下载且已核对的文件执行这一步。详细步骤见[建置与部署说明](docs/BUILD-AND-SIGN.md)。

### 3. 检查是否生效

安装 MSIX 后，Windows 就会把三个 Game Bar 协议交给处理程序；无需再点击启用按钮。按 `Win + G` 或重新连接手柄，检查是否还出现“选择应用”窗口。

打开 Command Palette，搜索 **ms-gamebar-handler**，进入后可选择“最近事件”查看触发记录。“安裝管理工具”只用于准备命令行工具和配置文件，不影响 MSIX 的协议接管。

### 4. 移除或更新

- 移除：如果以前使用过本项目的“安裝處理程式”，先在扩展中选择“還原舊版手動註冊”；然后在 Windows“设置 → 应用 → 已安装的应用”中卸载 **ms-gamebar-handler**。
- 更新：从发行版页面下载较高版本的 MSIX，直接安装即可覆盖旧版。更新后完全退出并重新启动 Command Palette。

扩展本身是一个独立的 Windows 安装包，和 PowerToys 配合使用，但不是 PowerToys 主程序的一部分。它没有单独的开始菜单入口，请从 Command Palette 打开。

## 常见问题

**为什么还会弹出“选择应用”？** 0.5.0 及更早的测试版可在 PowerShell 执行 `Get-AppxPackage -Name MsGamebarHandlerExtension`；0.7.0 起执行 `Get-AppxPackage -Name Moonights.ms-gamebar-handler`，确认安装版本后再断开并重新连接手柄。如果仍弹出，请在[项目 Issues](https://gitee.com/Moonights_gitee/ms-gamebar-handler/issues)中附上 Windows 版本、扩展版本和弹窗截图。

**每次更新都会新增一个应用吗？** 同一个包家族的较高版本会覆盖旧版。0.7.0 改用商店身份，与 0.5.0 及更早测试版属于不同包家族；首次切换时需卸载旧测试版。后续商店版本会沿用 0.7.0 的包家族。

**搜索时出现两个同名结果或旧名称？** 更新后请完全退出并重新启动 Command Palette。仅关闭搜索窗口，后台进程仍可能保留升级前的命令。如果重启后仍重复，请在[项目 Issues](https://gitee.com/Moonights_gitee/ms-gamebar-handler/issues)反馈截图和版本号。

**如何切换语言？** 扩展跟随 Windows 的界面语言。支持简体中文（`zh-CN`）、繁体中文（`zh-TW`）和美式英语（`en-US`）；其他语言回退到简体中文。更改 Windows 语言后，请完全退出并重新启动 Command Palette。

## 从源码构建（开发者）

需要 Windows 和 .NET 10 SDK。以下方式不依赖 Command Palette，适合检查协议处理逻辑：

```powershell
.\scripts\build.ps1
.\scripts\install.ps1
```

`install.ps1` 会将引擎复制到当前用户的 `%LOCALAPPDATA%\ms-gamebar-handler\bin`，并注册三个传统 Windows 协议命令，供源码开发测试。要解决手柄触发的系统弹窗，仍须安装包含协议宣告的 MSIX。需要恢复手动注册的原状态时执行 `.\scripts\uninstall.ps1`。构建、签章与部署 MSIX 的步骤见[建置与部署说明](docs/BUILD-AND-SIGN.md)，自动测试运行 `dotnet run --project .\tests\HandlerTests.csproj`。

开发机已信任测试证书时，可以运行 `.\scripts\dev-deploy.ps1`，顺序完成 MSIX 构建、签名和当前用户升级；脚本会核对版本及套件身份，不会先卸载旧版。

项目处理 `ms-gamebar:`、`ms-gamingoverlay:` 和 `ms-gamebarservices:`。默认不启动任何程序；事件记录存于 `%LOCALAPPDATA%\ms-gamebar-handler\events.log`，不保存完整请求地址。

## 工作原理（接手项目时可先读这里）

1. Windows 根据 MSIX 的 `Package.appxmanifest` 找到三个 Game Bar 协议。手柄或 `Win + G` 发出请求时，Windows 启动包内的 `ms-gamebar-handler.exe`，传入 `--invoke` 和请求地址。
2. `engine/Program.cs` 先验证协议，再读取 `config.json`。没有配置文件时使用内置默认值：忽略请求，不启动别的程序。
3. 若配置了规则，`engine/Rules.cs` 按列表顺序选中第一条匹配规则；冷却时间可避免短时间重复执行。当前 Command Palette 尚未开放规则编辑。
4. 处理完成后，程序只把时间、协议名称、规则和动作写入 `events.log`。Command Palette 的“最近事件”读取这个文件。

`engine/ProtocolRegistry.cs` 用于旧版手动注册及源码调试。正式 MSIX 的协议接管由清单完成；要完全停止接管，需要卸载 MSIX。图标可以用 `python scripts/generate-icons.py` 重新生成，运行该脚本需要 Pillow；安装包已经包含生成好的 PNG。
