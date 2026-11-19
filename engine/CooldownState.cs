using System.Diagnostics;
using System.Text.Json;

namespace MsGamebarHandler;

public static class CooldownState
{
    public static bool Execute(string statePath, EventRule? rule, Action action)
    {
        if (rule is null || rule.CooldownSeconds <= 0)
        {
            action();
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(statePath) ?? ".");
        // FileShare.None 對所有處理程序生效，鎖住「讀取 → 判斷 → 執行 → 寫回」整段流程。
        // 鎖檔保持存在，避免刪除與重新建立之間讓不同程序鎖到不同的檔案。
        using var fileLock = AcquireLock(statePath + ".lock");
        var state = File.Exists(statePath)
            ? JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset>>(File.ReadAllText(statePath)) ?? []
            : new Dictionary<string, DateTimeOffset>();
        // 取得鎖後再取時間，避免等待鎖期間的舊時間覆蓋較新的冷卻紀錄。
        var now = DateTimeOffset.UtcNow;
        if (Cooldown.IsActive(rule, state, now)) return true;

        action();
        state[rule.Id] = now;
        // 暫存檔與 state.json 在同一目錄；程序中途退出時不會留下半份 JSON。
        var temporary = statePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(state, HandlerConfig.JsonOptions));
            File.Move(temporary, statePath, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
        return false;
    }

    private static FileStream AcquireLock(string path)
    {
        var elapsed = Stopwatch.StartNew();
        while (true)
        {
            try { return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException ex) when (ex.HResult is unchecked((int)0x80070020) or unchecked((int)0x80070021) && elapsed.Elapsed < TimeSpan.FromSeconds(5))
            {
                Thread.Sleep(25);
            }
        }
    }
}
