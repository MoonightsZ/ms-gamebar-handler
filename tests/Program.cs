using MsGamebarHandler;
using System.Globalization;

var failures = 0;
void Check(string name, bool okay)
{
    Console.WriteLine($"{(okay ? "PASS" : "FAIL")} {name}");
    if (!okay) failures++;
}

var config = HandlerConfig.Default();
Check("簡體中文為預設語言", L.ForCulture("InstallTools", CultureInfo.GetCultureInfo("fr-FR")) == "安装管理工具");
Check("繁體中文翻譯", L.ForCulture("InstallTools", CultureInfo.GetCultureInfo("zh-TW")) == "安裝管理工具");
Check("英文翻譯", L.ForCulture("InstallTools", CultureInfo.GetCultureInfo("en-US")) == "Install management tools");
Check("三語應用名稱", L.ForCulture("AppName", CultureInfo.GetCultureInfo("zh-CN")) == "MS 游戏栏处理器"
    && L.ForCulture("AppName", CultureInfo.GetCultureInfo("zh-TW")) == "MS 遊戲列處理器"
    && L.ForCulture("AppName", CultureInfo.GetCultureInfo("en-US")) == "MS Game Bar Handler");
Check("預設忽略", config.DefaultAction.Type == "ignore");
Check("首版沒有預設規則", config.Events.Count == 0);
Check("支援三個協議", Protocols.IsSupported("ms-gamebar://gamepadConnect/") && Protocols.IsSupported("ms-gamingoverlay:") && Protocols.IsSupported("ms-gamebarservices:"));
Check("拒絕外部協議", !Protocols.IsSupported("https://example.com"));

config.Events.Add(new EventRule("first", true, 0, new MatchRule("contains", "gamepadConnect"), new ActionRule("ignore", "", "")));
config.Events.Add(new EventRule("second", true, 0, new MatchRule("any", ""), new ActionRule("ignore", "", "")));
Check("依序選首條規則", Matcher.Select(config, "ms-gamebar://gamepadConnect/")?.Id == "first");
Check("未命中時使用後續規則", Matcher.Select(config, "ms-gamebar://other/")?.Id == "second");
Check("無效正則不崩潰", !Matcher.IsMatch(new MatchRule("regex", "["), "ms-gamebar://x"));
var timed = new EventRule("timed", true, 15, new MatchRule("any", ""), new ActionRule("ignore", "", ""));
var now = DateTimeOffset.UtcNow;
Check("冷卻期內跳過", Cooldown.IsActive(timed, new Dictionary<string, DateTimeOffset> { ["timed"] = now.AddSeconds(-5) }, now));
Check("冷卻期滿可執行", !Cooldown.IsActive(timed, new Dictionary<string, DateTimeOffset> { ["timed"] = now.AddSeconds(-15) }, now));
Check("不允許遞迴開啟 Game Bar URI", Throws(() => HandlerConfig.ValidateAction(new ActionRule("launch-uri", "ms-gamebar://x", ""), false)));
Check("預設拒絕執行程式", Throws(() => HandlerConfig.ValidateAction(new ActionRule("run", Environment.ProcessPath!, ""), false)));
Check("開啟執行權限仍須白名單", Throws(() => HandlerConfig.ValidateAction(new ActionRule("run", Environment.ProcessPath!, ""), true)));
Check("白名單可允許指定程式", !Throws(() => HandlerConfig.ValidateAction(new ActionRule("run", Environment.ProcessPath!, ""), true, [Environment.ProcessPath!])));

var testRoot = $@"Software\ms-gamebar-handler-tests\{Guid.NewGuid():N}\Classes";
var testData = Path.Combine(Path.GetTempPath(), "ms-gamebar-handler-tests", Guid.NewGuid().ToString("N"));
try
{
    using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"{testRoot}\ms-gamebar")) key.SetValue("URL Protocol", "original");
    var registry = new ProtocolRegistry(testData, testRoot);
    registry.Register(Environment.ProcessPath!);
    Check("註冊三個協議", registry.Status().Count(s => s.Contains("--invoke")) == 3);
    registry.Register(Environment.ProcessPath!);
    using (var root = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"{testRoot}\ms-gamingoverlay", true))
        root!.DeleteSubKey(@"shell\open\command");
    registry.Unregister();
    Check("部分註冊後仍可還原", registry.Status().All(s => s.Contains($"({L.Get("NotRegistered")})")));
    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"{testRoot}\ms-gamebar"))
        Check("保留原有協議值", key?.GetValue("URL Protocol")?.ToString() == "original");
    Check("移除新建的協議鍵", Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"{testRoot}\ms-gamingoverlay") is null);

    using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"{testRoot}\ms-gamebar\shell\open\command")) key.SetValue("custom", "keep");
    Check("拒絕覆寫他人的命令鍵", ThrowsOther(() => registry.Register(Environment.ProcessPath!)));
}
finally
{
    Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(testRoot[..testRoot.LastIndexOf('\\')], false);
    if (Directory.Exists(testData)) Directory.Delete(testData, true);
}

var rulesPath = Path.Combine(Path.GetTempPath(), "ms-gamebar-handler-tests", Guid.NewGuid().ToString("N"), "config.json");
try
{
    RuleEditor.Save(rulesPath, new EventRule("one", true, 0, new MatchRule("protocol", "ms-gamebar"), new ActionRule("ignore", "", "")));
    Check("新增規則並建立配置", HandlerConfig.Load(rulesPath).Events.Count == 1);
    RuleEditor.Save(rulesPath, new EventRule("two", true, 0, new MatchRule("contains", "gamepadConnect"), new ActionRule("ignore", "", "")));
    RuleEditor.Save(rulesPath, new EventRule("one", true, 5, new MatchRule("any", ""), new ActionRule("ignore", "", "")));
    Check("修改規則不改順序", HandlerConfig.Load(rulesPath).Events.Select(e => e.Id).SequenceEqual(["one", "two"]));
    RuleEditor.Move(rulesPath, "two", -1);
    Check("規則可上移", HandlerConfig.Load(rulesPath).Events.Select(e => e.Id).SequenceEqual(["two", "one"]));
    RuleEditor.SetEnabled(rulesPath, "two", false);
    Check("規則可停用", !HandlerConfig.Load(rulesPath).Events[0].Enabled);
    Check("拒絕無效正則", Throws(() => RuleEditor.Save(rulesPath, new EventRule("bad", true, 0, new MatchRule("regex", "["), new ActionRule("ignore", "", "")))));
    RuleEditor.Delete(rulesPath, "one");
    Check("規則可刪除", HandlerConfig.Load(rulesPath).Events.Select(e => e.Id).SequenceEqual(["two"]));
    File.WriteAllText(rulesPath, """
        {"version":1,"events":[{"id":"bad","enabled":true,"cooldownSeconds":0,"match":{"type":"contains","value":null},"action":{"type":"ignore","value":"","arguments":""}}]}
        """);
    Check("拒絕空匹配值的配置", Throws(() => HandlerConfig.Load(rulesPath)));
}
finally
{
    var dir = Path.GetDirectoryName(rulesPath)!;
    if (Directory.Exists(dir)) Directory.Delete(dir, true);
}

var eventPath = Path.Combine(Path.GetTempPath(), $"ms-gamebar-events-{Guid.NewGuid():N}.log");
try
{
    // 模擬手把一次送出多個協議時，另一個處理程序暫時占用事件檔。
    using var held = new FileStream(eventPath, FileMode.Create, FileAccess.Write, FileShare.None);
    var release = Task.Run(async () => { await Task.Delay(150); held.Dispose(); });
    EventLog.Append(eventPath, "event-one" + Environment.NewLine);
    release.GetAwaiter().GetResult();
    Check("事件檔暫時占用後仍寫入", File.ReadAllText(eventPath) == "event-one" + Environment.NewLine);
}
finally
{
    if (File.Exists(eventPath)) File.Delete(eventPath);
}

return failures == 0 ? 0 : 1;

static bool Throws(Action action)
{
    try { action(); return false; } catch (InvalidDataException) { return true; }
}

static bool ThrowsOther(Action action)
{
    try { action(); return false; } catch (InvalidOperationException) { return true; }
}
