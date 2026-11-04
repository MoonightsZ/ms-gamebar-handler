using System.Globalization;

namespace MsGamebarHandler;

// 引擎、命令列工具及 CmdPal 共用文字；不依賴衛星組件，確保單檔 EXE 也能切換語言。
public static class L
{
    private sealed record Translation(string ZhCn, string ZhTw, string EnUs);

    private static readonly Dictionary<string, Translation> Strings = new(StringComparer.Ordinal)
    {
        ["AppName"] = new("MS 游戏栏处理器", "MS 遊戲列處理器", "MS Game Bar Handler"),
        ["ManageProtocols"] = new("管理 Game Bar 协议", "管理 Game Bar 協議", "Manage Game Bar protocols"),
        ["InstallTools"] = new("安装管理工具", "安裝管理工具", "Install management tools"),
        ["InstallToolsDescription"] = new("复制命令行工具并初始化配置", "複製命令列工具並初始化配置", "Copy command-line tools and initialize settings"),
        ["RestoreLegacy"] = new("还原旧版手动注册", "還原舊版手動註冊", "Restore legacy registration"),
        ["RestoreLegacyDescription"] = new("清理旧版注册表设置；不会移除 MSIX", "清理舊版登錄設定；不會移除 MSIX", "Clean up legacy registry settings; keep the MSIX installed"),
        ["RecentEvents"] = new("最近事件", "最近事件", "Recent events"),
        ["View"] = new("查看", "查看", "View"),
        ["NoEvents"] = new("暂无事件", "尚無事件", "No events yet"),
        ["ActionFailed"] = new("{0}失败：{1}", "{0}失敗：{1}", "{0} failed: {1}"),
        ["MissingEngine"] = new("安装包中缺少处理程序", "套件未包含引擎", "The package does not contain the handler"),
        ["ToolsInstalled"] = new("管理工具已安装；MSIX 已接管三个 Game Bar 协议", "管理工具已安裝；MSIX 已接管三個 Game Bar 協議", "Management tools installed; MSIX handles the three Game Bar protocols"),
        ["NoLegacyRegistration"] = new("没有需要还原的旧版手动注册", "沒有舊版手動註冊需要還原", "No legacy registration to restore"),
        ["LegacyRestored"] = new("已还原旧版手动注册；MSIX 仍在处理协议", "已還原舊版手動註冊；MSIX 仍在處理協議", "Legacy registration restored; MSIX still handles the protocols"),
        ["ToolsNotInstalled"] = new("管理工具尚未安装", "尚未安裝管理工具", "Management tools are not installed"),
        ["CliStartFailed"] = new("无法启动管理工具", "無法啟動管理工具", "Could not start the management tools"),
        ["CliFailed"] = new("管理工具返回错误", "管理工具回報錯誤", "Management tools returned an error"),
        ["EmptyConfig"] = new("配置为空", "配置為空", "Configuration is empty"),
        ["MissingEvents"] = new("缺少 events 字段", "缺少 events 欄位", "Missing events field"),
        ["Usage"] = new("用法：--invoke URL | --test URL | --register | --unregister | --status | --config | --rules-list | --rule-save ...", "用法：--invoke URL | --test URL | --register | --unregister | --status | --config | --rules-list | --rule-save ...", "Usage: --invoke URL | --test URL | --register | --unregister | --status | --config | --rules-list | --rule-save ..."),
        ["RuleSaved"] = new("规则已保存", "規則已儲存", "Rule saved"),
        ["RuleDeleted"] = new("规则已删除", "規則已刪除", "Rule deleted"),
        ["RuleStatusUpdated"] = new("规则状态已更新", "規則狀態已更新", "Rule status updated"),
        ["RuleOrderUpdated"] = new("规则顺序已更新", "規則順序已更新", "Rule order updated"),
        ["ConfigPath"] = new("配置：{0}", "配置：{0}", "Configuration: {0}"),
        ["ProcessPathMissing"] = new("无法获取程序路径", "無法取得程式路徑", "Could not determine the executable path"),
        ["PublishBeforeRegister"] = new("请先发布独立 EXE，再进行注册", "請先發佈獨立 exe，再進行註冊", "Publish a standalone EXE before registering the protocols"),
        ["ProtocolsRegistered"] = new("已注册三个协议", "已註冊三個協議", "Three protocols registered"),
        ["ProtocolsRestored"] = new("已还原三个协议", "已還原三個協議", "Three protocols restored"),
        ["UnsupportedProtocol"] = new("仅接受三个 Game Bar 协议的 URI", "僅接受三個 Game Bar 協議的 URI", "Only the three Game Bar protocol URIs are accepted"),
        ["TestResult"] = new("规则：{0}；操作：{1}", "規則：{0}；動作：{1}", "Rule: {0}; action: {1}"),
        ["UnknownCommand"] = new("未知命令", "未知命令", "Unknown command"),
        ["InvalidConfig"] = new("配置版本或字段无效", "配置版本或欄位無效", "Invalid configuration version or fields"),
        ["DuplicateRule"] = new("重复的规则 ID：{0}", "重複的規則 ID：{0}", "Duplicate rule ID: {0}"),
        ["InvalidAction"] = new("无效或不允许的操作：{0}", "無效或未允許的動作：{0}", "Invalid or disallowed action: {0}"),
        ["NotRegistered"] = new("未注册", "未註冊", "Not registered"),
        ["EngineMissing"] = new("找不到处理程序", "找不到引擎", "Handler executable not found"),
        ["PathChanged"] = new("处理程序路径已改变；请先从原安装位置解除注册", "處理程式路徑已改變；請先由原安裝位置解除註冊", "Handler path changed; unregister from the original location first"),
        ["ProtocolClaimed"] = new("{0} 已由其他程序接管", "{0} 已由其他程式接管", "{0} is handled by another application"),
        ["ProtocolCommandExists"] = new("{0} 已有命令键，拒绝覆盖", "{0} 已有命令鍵，拒絕覆寫", "{0} already has a command key; refusing to overwrite it"),
        ["ProtocolSystemHandlerExists"] = new("{0} 已有系统处理程序，拒绝覆盖", "{0} 已有系統處理程式，拒絕覆寫", "{0} already has a system handler; refusing to overwrite it"),
        ["ProtocolHandlerExists"] = new("{0} 已有处理程序，拒绝覆盖", "{0} 已有處理程式，拒絕覆寫", "{0} already has a handler; refusing to overwrite it"),
        ["BackupMissing"] = new("找不到原始注册备份，拒绝解除注册", "找不到原始註冊備份，拒絕解除註冊", "Original registration backup not found; refusing to unregister"),
        ["ProtocolChanged"] = new("{0} 已被其他程序修改，拒绝还原", "{0} 已被其他程式修改，拒絕還原", "{0} was changed by another application; refusing to restore it"),
        ["InvalidBackup"] = new("注册备份无效", "註冊備份無效", "Invalid registration backup"),
        ["InvalidRuleId"] = new("规则 ID 必须为 1–64 个字母、数字、点、下划线或连字符", "規則 ID 須為 1–64 個英數、點、底線或連字號", "Rule ID must contain 1–64 letters, digits, periods, underscores, or hyphens"),
        ["InvalidCooldown"] = new("冷却时间必须在 0–86400 秒之间", "冷卻時間須介於 0–86400 秒", "Cooldown must be between 0 and 86400 seconds"),
        ["UnknownMatchType"] = new("未知的匹配类型", "未知的匹配類型", "Unknown match type"),
        ["EmptyMatchValue"] = new("匹配值不能为空", "匹配值不可為空", "Match value cannot be empty"),
        ["OnlyGameBarProtocols"] = new("仅支持三个 Game Bar 协议", "僅支援三個 Game Bar 協議", "Only the three Game Bar protocols are supported"),
        ["InvalidRegex"] = new("正则表达式无效", "正則表達式無效", "Invalid regular expression"),
        ["MissingAction"] = new("缺少操作", "缺少動作", "Action is missing"),
        ["RuleNotFound"] = new("找不到规则：{0}", "找不到規則：{0}", "Rule not found: {0}"),
    };

    public static string Get(string key, params object?[] args) => ForCulture(key, CultureInfo.CurrentUICulture, args);

    public static string ForCulture(string key, CultureInfo culture, params object?[] args)
    {
        var value = Strings[key];
        var name = culture.Name;
        var template = name.Equals("zh-TW", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("zh-HK", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("zh-MO", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase)
            ? value.ZhTw
            : name.Equals("en", StringComparison.OrdinalIgnoreCase) || name.StartsWith("en-", StringComparison.OrdinalIgnoreCase)
                ? value.EnUs
                : value.ZhCn;
        return args.Length == 0 ? template : string.Format(culture, template, args);
    }
}
