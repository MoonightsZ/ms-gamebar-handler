# ms-gamebar-handler

让 Windows 接收游戏手柄或 `Win + G` 发出的 Game Bar 请求时，不再反复弹出“选择应用”窗口。启用处理程序后，这些请求默认被安静地忽略。项目通过 [PowerToys Command Palette](https://learn.microsoft.com/windows/powertoys/command-palette/overview) 提供操作入口。

> 当前是预览版，只支持 Windows x64。规则编辑和 Steam 大屏幕自动启动尚未开放。

## 快速开始

### 1. 准备环境

- Windows 11 x64，以及已启用 Command Palette 的 [Microsoft PowerToys](https://learn.microsoft.com/windows/powertoys/install)。
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)。它用于运行随扩展提供的管理工具；如果电脑已经安装，可跳过。

### 2. 下载并安装扩展

打开 [发行版页面](https://gitee.com/Moonights_gitee/ms-gamebar-handler/releases)，选择最新预览版，下载同一版本下的三个附件：

1. `MsGamebarHandlerExtension_…_x64.msix`：Windows 安装包。
2. `ms-gamebar-handler-dev.cer`：供预览版使用的测试证书。
3. `SHA256SUMS.txt`：用于核对下载文件是否完整。

先核对下载文件的 SHA-256 值与 `SHA256SUMS.txt` 一致。可以在下载文件夹打开 PowerShell，运行 `Get-FileHash .\文件名 -Algorithm SHA256`。

这个预览版使用测试证书签名。双击 `.cer` 文件，选择“安装证书”→“本地计算机”→“将所有的证书都放入下列存储”→“受信任人”，按 Windows 提示完成操作。然后双击 `.msix` 安装。信任测试证书会影响整台电脑；只对从本项目发行页下载且已核对的文件执行这一步。详细步骤见[建置与部署说明](docs/BUILD-AND-SIGN.md)。

### 3. 启用处理程序

打开 Command Palette，搜索 **ms-gamebar-handler**，进入后选择“安裝處理程式”。这一步才会接管当前 Windows 用户的 Game Bar 请求；只安装 MSIX 还不会接管。

现在按 `Win + G` 或重新连接手柄，检查是否还出现“选择应用”窗口。需要查看触发记录时，在扩展中选择“最近事件”。

### 4. 停用或更新

- 停用：在扩展中选择“解除安裝處理程式”，恢复安装前的协议状态。
- 更新：从发行版页面下载较高版本的 MSIX，直接安装即可覆盖旧版。更新后如有需要，在扩展中再次选择“安裝處理程式”。

扩展本身是一个独立的 Windows 安装包，和 PowerToys 配合使用，但不是 PowerToys 主程序的一部分。它没有单独的开始菜单入口，请从 Command Palette 打开。

## 常见问题

**为什么还会弹出“选择应用”？** 确认已在扩展中执行“安裝處理程式”，而不只是安装了 MSIX。当前预览版的这个按钮仍在逐项界面验收中；如果操作失败，请参考下面的命令行安装方式，并在[项目 Issues](https://gitee.com/Moonights_gitee/ms-gamebar-handler/issues)中附上错误信息。

**安装时提示已有其他处理程序？** 项目不会覆盖其他应用已注册的同名协议。请先确认原应用的用途，再决定是否调整。

**每次更新都会新增一个应用吗？** 不会。保持相同套件身份的较高版本会更新已有安装。可在 PowerShell 执行 `Get-AppxPackage -Name MsGamebarHandlerExtension` 查看当前版本；正常情况下当前用户只会看到一个版本。

**搜索时还看到旧名称？** 更新到 0.0.6.0 后重新打开 Command Palette。旧名称可能来自之前的使用记录；如果仍出现，请在[项目 Issues](https://gitee.com/Moonights_gitee/ms-gamebar-handler/issues)反馈截图和版本号。

## 从源码构建（开发者）

需要 Windows 和 .NET 10 SDK。以下方式不依赖 Command Palette，适合检查协议处理逻辑：

```powershell
.\scripts\build.ps1
.\scripts\install.ps1
```

`install.ps1` 会将引擎复制到当前用户的 `%LOCALAPPDATA%\ms-gamebar-handler\bin`，并注册三个协议。需要恢复原状态时执行 `.\scripts\uninstall.ps1`。构建、签章与部署 MSIX 的步骤见[建置与部署说明](docs/BUILD-AND-SIGN.md)，自动测试运行 `dotnet run --project .\tests\HandlerTests.csproj`。

项目处理 `ms-gamebar:`、`ms-gamingoverlay:` 和 `ms-gamebarservices:`。默认不启动任何程序；事件记录存于 `%LOCALAPPDATA%\ms-gamebar-handler\events.log`，不保存完整请求地址。
