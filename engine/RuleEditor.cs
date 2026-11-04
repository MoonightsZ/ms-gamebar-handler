using System.Text.Json;
using System.Text.RegularExpressions;

namespace MsGamebarHandler;

public static class RuleEditor
{
    public static void Save(string path, EventRule rule)
    {
        var config = HandlerConfig.Load(path);
        Validate(rule, config);
        var index = config.Events.FindIndex(item => string.Equals(item.Id, rule.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0) config.Events.Add(rule);
        else config.Events[index] = rule;
        Write(path, config);
    }

    public static void Delete(string path, string id)
    {
        var config = HandlerConfig.Load(path);
        var index = Find(config, id);
        config.Events.RemoveAt(index);
        Write(path, config);
    }

    public static void SetEnabled(string path, string id, bool enabled)
    {
        var config = HandlerConfig.Load(path);
        var index = Find(config, id);
        config.Events[index] = config.Events[index] with { Enabled = enabled };
        Write(path, config);
    }

    public static void Move(string path, string id, int direction)
    {
        if (direction is not (-1 or 1)) throw new ArgumentOutOfRangeException(nameof(direction));
        var config = HandlerConfig.Load(path);
        var index = Find(config, id);
        var target = index + direction;
        if (target < 0 || target >= config.Events.Count) return;
        (config.Events[index], config.Events[target]) = (config.Events[target], config.Events[index]);
        Write(path, config);
    }

    public static void Validate(EventRule rule, HandlerConfig config)
    {
        if (rule is null || !Regex.IsMatch(rule.Id ?? "", @"^[A-Za-z0-9._-]{1,64}$"))
            throw new InvalidDataException(L.Get("InvalidRuleId"));
        if (rule.CooldownSeconds is < 0 or > 86400) throw new InvalidDataException(L.Get("InvalidCooldown"));
        if (rule.Match is null || !new[] { "any", "contains", "equals", "protocol", "regex" }.Contains(rule.Match.Type))
            throw new InvalidDataException(L.Get("UnknownMatchType"));
        if (rule.Match.Type != "any" && string.IsNullOrWhiteSpace(rule.Match.Value))
            throw new InvalidDataException(L.Get("EmptyMatchValue"));
        if (rule.Match.Type == "protocol" && !Protocols.Names.Contains(rule.Match.Value.TrimEnd(':'), StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException(L.Get("OnlyGameBarProtocols"));
        if (rule.Match.Type == "regex")
        {
            try { _ = new Regex(rule.Match.Value, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)); }
            catch (ArgumentException ex) { throw new InvalidDataException(L.Get("InvalidRegex"), ex); }
        }
        if (rule.Action is null) throw new InvalidDataException(L.Get("MissingAction"));
        HandlerConfig.ValidateAction(rule.Action, config.AllowRun, config.AllowedExecutables);
    }

    private static int Find(HandlerConfig config, string id)
    {
        var index = config.Events.FindIndex(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        if (index < 0) throw new KeyNotFoundException(L.Get("RuleNotFound", id));
        return index;
    }

    private static void Write(string path, HandlerConfig config)
    {
        // 先寫臨時檔再替換，避免儲存中斷後留下半份 config.json。
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(directory);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(config, HandlerConfig.JsonOptions));
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
