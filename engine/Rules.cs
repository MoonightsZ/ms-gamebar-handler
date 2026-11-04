using System.Text.Json;
using System.Text.RegularExpressions;

namespace MsGamebarHandler;

public sealed record ActionRule(string Type, string Value, string Arguments);
public sealed record MatchRule(string Type, string Value);
public sealed record EventRule(string Id, bool Enabled, int CooldownSeconds, MatchRule Match, ActionRule Action);

public sealed class HandlerConfig
{
    // 首次執行時未必已有 config.json；預設配置讓協議請求直接靜默結束。
    public int Version { get; set; } = 1;
    public bool Log { get; set; } = true;
    public bool AllowRun { get; set; }
    public List<string> AllowedExecutables { get; set; } = [];
    public ActionRule DefaultAction { get; set; } = new("ignore", "", "");
    public List<EventRule> Events { get; set; } = [];

    public static HandlerConfig Default() => new();

    public static HandlerConfig Load(string path)
    {
        if (!File.Exists(path)) return Default();
        var result = JsonSerializer.Deserialize<HandlerConfig>(File.ReadAllText(path), JsonOptions) ?? throw new InvalidDataException("配置為空");
        if (result.Version != 1 || result.Events is null || result.DefaultAction is null || result.AllowedExecutables is null) throw new InvalidDataException("配置版本或欄位無效");
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in result.Events)
        {
            RuleEditor.Validate(rule, result);
            if (!ids.Add(rule.Id)) throw new InvalidDataException($"重複的規則 ID：{rule.Id}");
        }
        ValidateAction(result.DefaultAction, result.AllowRun, result.AllowedExecutables);
        return result;
    }

    public static void ValidateAction(ActionRule action, bool allowRun, IReadOnlyCollection<string>? allowedExecutables = null)
    {
        // 自訂動作須通過配置檢查，避免 URI 再次開啟 Game Bar 或任意執行本機程式。
        if (action.Type == "ignore") return;
        if (action.Type == "launch-uri" && Uri.TryCreate(action.Value, UriKind.Absolute, out var uri) && !Protocols.Names.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase)) return;
        if (action.Type == "run" && allowRun && Path.IsPathFullyQualified(action.Value) && File.Exists(action.Value)
            && allowedExecutables?.Contains(action.Value, StringComparer.OrdinalIgnoreCase) == true) return;
        throw new InvalidDataException($"無效或未允許的動作：{action.Type}");
    }

    public static JsonSerializerOptions JsonOptions { get; } = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
}

public static class Protocols
{
    public static readonly string[] Names = ["ms-gamebar", "ms-gamingoverlay", "ms-gamebarservices"];
    public static bool IsSupported(string raw) => Uri.TryCreate(raw, UriKind.Absolute, out var uri) && Names.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase);
}

public static class Matcher
{
    // 第一條啟用且符合條件的規則生效；因此列表順序也是優先順序。
    public static EventRule? Select(HandlerConfig config, string url) => config.Events.FirstOrDefault(r => r.Enabled && IsMatch(r.Match, url));

    public static bool IsMatch(MatchRule rule, string url)
    {
        if (rule is null) return false;
        switch (rule.Type)
        {
            case "any": return true;
            case "contains": return url.Contains(rule.Value, StringComparison.OrdinalIgnoreCase);
            case "equals": return string.Equals(url, rule.Value, StringComparison.OrdinalIgnoreCase);
            case "protocol": return Uri.TryCreate(url, UriKind.Absolute, out var uri) && string.Equals(uri.Scheme, rule.Value.TrimEnd(':'), StringComparison.OrdinalIgnoreCase);
            case "regex":
                try { return Regex.IsMatch(url, rule.Value, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)); }
                catch (ArgumentException) { return false; }
                catch (RegexMatchTimeoutException) { return false; }
            default: return false;
        }
    }
}

public static class Cooldown
{
    public static bool IsActive(EventRule? rule, IReadOnlyDictionary<string, DateTimeOffset> state, DateTimeOffset now) =>
        rule is not null && rule.CooldownSeconds > 0 && state.TryGetValue(rule.Id, out var last) && now - last < TimeSpan.FromSeconds(rule.CooldownSeconds);
}
