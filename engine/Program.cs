using System.Diagnostics;
using System.Text.Json;
using MsGamebarHandler;

try
{
    ConsoleHost.AttachForCommand(args.Length > 0 ? args[0] : "");
    if (args.Length == 0) { Console.Error.WriteLine("用法：--invoke URL | --test URL | --register | --unregister | --status | --config | --rules-list | --rule-save ..."); return 2; }
    var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-gamebar-handler");
    var configPath = Path.Combine(root, "config.json");
    var registry = new ProtocolRegistry(root);
    // MSIX 的 windows.protocol 宣告會以 --invoke 傳入 URI；其他命令供管理工具與原始碼測試使用。
    switch (args[0])
    {
        case "--init":
            Directory.CreateDirectory(root);
            if (!File.Exists(configPath)) File.WriteAllText(configPath, JsonSerializer.Serialize(HandlerConfig.Default(), HandlerConfig.JsonOptions));
            Console.WriteLine(configPath);
            return 0;
        case "--config": Console.WriteLine(configPath); return 0;
        case "--rules-list":
            Console.WriteLine(JsonSerializer.Serialize(HandlerConfig.Load(configPath).Events, HandlerConfig.JsonOptions));
            return 0;
        case "--rule-save":
            if (args.Length != 9 || !bool.TryParse(args[2], out var enabled) || !int.TryParse(args[3], out var cooldown)) return 2;
            RuleEditor.Save(configPath, new EventRule(args[1], enabled, cooldown, new MatchRule(args[4], args[5]), new ActionRule(args[6], args[7], args[8])));
            Console.WriteLine("規則已儲存");
            return 0;
        case "--rule-delete":
            if (args.Length != 2) return 2;
            RuleEditor.Delete(configPath, args[1]);
            Console.WriteLine("規則已刪除");
            return 0;
        case "--rule-enable":
            if (args.Length != 3 || !bool.TryParse(args[2], out var ruleEnabled)) return 2;
            RuleEditor.SetEnabled(configPath, args[1], ruleEnabled);
            Console.WriteLine("規則狀態已更新");
            return 0;
        case "--rule-move":
            if (args.Length != 3 || (args[2] != "up" && args[2] != "down")) return 2;
            RuleEditor.Move(configPath, args[1], args[2] == "up" ? -1 : 1);
            Console.WriteLine("規則順序已更新");
            return 0;
        case "--status":
            Console.WriteLine($"配置：{configPath}");
            foreach (var line in registry.Status()) Console.WriteLine(line);
            return 0;
        case "--register":
            // 傳統登錄方式只供原始碼部署使用。正式 MSIX 的協議關聯由套件清單管理。
            #if MANAGEMENT_CLI
            var executable = Path.Combine(AppContext.BaseDirectory, "ms-gamebar-handler.exe");
            #else
            var executable = Environment.ProcessPath ?? throw new InvalidOperationException("無法取得程式路徑");
            if (Path.GetFileName(executable).Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("請先發佈獨立 exe，再進行註冊");
            #endif
            registry.Register(executable);
            Console.WriteLine("已註冊三個協議");
            return 0;
        case "--unregister": registry.Unregister(); Console.WriteLine("已還原三個協議"); return 0;
        case "--invoke" or "--test":
            // URI 可能來自任何應用，先限制為本專案支援的三個協議，再讀取配置。
            if (args.Length != 2 || !Protocols.IsSupported(args[1])) { Console.Error.WriteLine("僅接受三個 Game Bar 協議的 URI"); return 2; }
            var config = HandlerConfig.Load(configPath);
            // 規則依列表順序比對；沒有命中時使用 defaultAction（預設靜默忽略）。
            var rule = Matcher.Select(config, args[1]);
            var action = rule?.Action ?? config.DefaultAction;
            if (args[0] == "--test") { Console.WriteLine($"規則：{rule?.Id ?? "default"}；動作：{action.Type}"); return 0; }
            Directory.CreateDirectory(root);
            var statePath = Path.Combine(root, "state.json");
            var state = File.Exists(statePath) ? JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset>>(File.ReadAllText(statePath)) ?? [] : new Dictionary<string, DateTimeOffset>();
            var now = DateTimeOffset.UtcNow;
            // 冷卻期避免同一次手把連接在短時間內重複執行動作。
            var cooled = Cooldown.IsActive(rule, state, now);
            if (!cooled)
            {
                if (action.Type == "launch-uri") Process.Start(new ProcessStartInfo(action.Value) { UseShellExecute = true });
                if (action.Type == "run") Process.Start(new ProcessStartInfo(action.Value, action.Arguments) { UseShellExecute = false });
                if (rule is not null && rule.CooldownSeconds > 0)
                {
                    state[rule.Id] = now;
                    File.WriteAllText(statePath, JsonSerializer.Serialize(state, HandlerConfig.JsonOptions));
                }
            }
            // 只記協議、規則與動作，不保存可能含使用者資料的完整 URI。
            if (config.Log) EventLog.Append(Path.Combine(root, "events.log"), $"{now:O} protocol={new Uri(args[1]).Scheme} rule={rule?.Id ?? "default"} action={(cooled ? "cooldown" : action.Type)}{Environment.NewLine}");
            return 0;
        default: Console.Error.WriteLine("未知命令"); return 2;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    try
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-gamebar-handler");
        Directory.CreateDirectory(root);
        File.AppendAllText(Path.Combine(root, "errors.log"), $"{DateTimeOffset.UtcNow:O} {ex.GetType().Name}: {ex.Message}{Environment.NewLine}");
    }
    catch { }
    return 1;
}
