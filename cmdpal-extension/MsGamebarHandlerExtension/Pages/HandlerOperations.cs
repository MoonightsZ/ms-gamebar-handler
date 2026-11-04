using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace MsGamebarHandlerExtension;

internal sealed partial class HandlerAction(string title, Func<string> action) : InvokableCommand
{
    public override ICommandResult Invoke()
    {
        try { return CommandResult.ShowToast(action()); }
        catch (Exception ex) { return CommandResult.ShowToast($"{title}失敗：{ex.Message}"); }
    }
}

internal sealed partial class ConfirmHandlerAction(string title, string description, Func<string> action) : InvokableCommand
{
    public override ICommandResult Invoke() => CommandResult.Confirm(new ConfirmationArgs
    {
        Title = title,
        Description = description,
        PrimaryCommand = new HandlerAction(title, action),
    });
}

internal static class HandlerOperations
{
    private static string Data => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-gamebar-handler");
    private static string Bin => Path.Combine(Data, "bin");
    private static string Cli => Path.Combine(Bin, "ms-gamebar-handler-cli.exe");
    private static string Config => Path.Combine(Data, "config.json");

    public static string Install()
    {
        Directory.CreateDirectory(Bin);
        foreach (var name in new[] { "ms-gamebar-handler.exe", "ms-gamebar-handler-cli.exe" })
        {
            var source = Path.Combine(AppContext.BaseDirectory, "Payload", name);
            if (!File.Exists(source)) throw new FileNotFoundException("套件未包含引擎", source);
            File.Copy(source, Path.Combine(Bin, name), true);
        }
        RunCli("--init");
        RunCli("--register");
        return "已接管三個 Game Bar 協議";
    }

    public static string Uninstall()
    {
        RunCli("--unregister");
        return "已還原協議；配置與日誌保留";
    }

    public static IReadOnlyList<RuleView> ReadRules()
    {
        if (!File.Exists(Config)) return [];
        var root = JsonNode.Parse(File.ReadAllText(Config)) ?? throw new InvalidDataException("配置為空");
        var events = root["events"]?.AsArray() ?? throw new InvalidDataException("缺少 events 欄位");
        return events.Select(item => new RuleView(
            item?["id"]?.ToString() ?? "",
            bool.TryParse(item?["enabled"]?.ToString(), out var enabled) && enabled,
            int.TryParse(item?["cooldownSeconds"]?.ToString(), out var cooldown) ? cooldown : 0,
            item?["match"]?["type"]?.ToString() ?? "any",
            item?["match"]?["value"]?.ToString() ?? "",
            item?["action"]?["type"]?.ToString() ?? "ignore",
            item?["action"]?["value"]?.ToString() ?? "",
            item?["action"]?["arguments"]?.ToString() ?? "")).ToArray();
    }

    public static string SaveRule(RuleView rule) => RunCli("--rule-save", rule.Id, rule.Enabled.ToString(), rule.CooldownSeconds.ToString(CultureInfo.InvariantCulture), rule.MatchType, rule.MatchValue, rule.ActionType, rule.ActionValue, rule.Arguments);
    public static string DeleteRule(string id) => RunCli("--rule-delete", id);
    public static string SetRuleEnabled(string id, bool enabled) => RunCli("--rule-enable", id, enabled.ToString());
    public static string MoveRule(string id, string direction) => RunCli("--rule-move", id, direction);

    private static string RunCli(params string[] arguments)
    {
        if (!File.Exists(Cli)) throw new FileNotFoundException("尚未安裝管理工具", Cli);
        var start = new ProcessStartInfo(Cli) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start);
        if (process is null) throw new InvalidOperationException("無法啟動管理工具");
        var error = process.StandardError.ReadToEnd();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "管理工具回報錯誤" : error.Trim());
        return output.Trim();
    }
}

internal sealed record RuleView(string Id, bool Enabled, int CooldownSeconds, string MatchType, string MatchValue, string ActionType, string ActionValue, string Arguments);
