using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using MsGamebarHandler;

namespace MsGamebarHandlerExtension;

internal sealed partial class HandlerAction(string title, Func<string> action) : InvokableCommand
{
    public override ICommandResult Invoke()
    {
        try { return CommandResult.ShowToast(action()); }
        catch (Exception ex) { return CommandResult.ShowToast(L.Get("ActionFailed", title, ex.Message)); }
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
        // MSIX 安裝時已註冊協議；這裡只準備可由使用者直接執行的管理工具。
        Directory.CreateDirectory(Bin);
        foreach (var name in new[] { "ms-gamebar-handler.exe", "ms-gamebar-handler-cli.exe" })
        {
            var source = Path.Combine(AppContext.BaseDirectory, "Payload", name);
            if (!File.Exists(source)) throw new FileNotFoundException(L.Get("MissingEngine"), source);
            File.Copy(source, Path.Combine(Bin, name), true);
        }
        RunCli("--init");
        return L.Get("ToolsInstalled");
    }

    public static string RestoreLegacyRegistration()
    {
        // 清除舊版手動登錄不會移除 MSIX。要停止接管協議須從 Windows 設定卸載套件。
        if (!File.Exists(Path.Combine(Data, "registry-backup.json"))) return L.Get("NoLegacyRegistration");
        RunCli("--unregister");
        return L.Get("LegacyRestored");
    }

    public static IReadOnlyList<RuleView> ReadRules()
    {
        if (!File.Exists(Config)) return [];
        var root = JsonNode.Parse(File.ReadAllText(Config)) ?? throw new InvalidDataException(L.Get("EmptyConfig"));
        var events = root["events"]?.AsArray() ?? throw new InvalidDataException(L.Get("MissingEvents"));
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
        // 使用 ArgumentList 傳參數，避免自己拼接命令列時把引號或空白處理錯。
        if (!File.Exists(Cli)) throw new FileNotFoundException(L.Get("ToolsNotInstalled"), Cli);
        var start = new ProcessStartInfo(Cli) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start);
        if (process is null) throw new InvalidOperationException(L.Get("CliStartFailed"));
        var error = process.StandardError.ReadToEnd();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? L.Get("CliFailed") : error.Trim());
        return output.Trim();
    }
}

internal sealed record RuleView(string Id, bool Enabled, int CooldownSeconds, string MatchType, string MatchValue, string ActionType, string ActionValue, string Arguments);
