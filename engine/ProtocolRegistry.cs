using System.Text.Json;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace MsGamebarHandler;

[SupportedOSPlatform("windows")]
public sealed class ProtocolRegistry(string dataDirectory, string classesRoot = @"Software\Classes")
{
    // 此類別只管理舊版與原始碼部署的 HKCU 註冊；MSIX 協議宣告不經過這裡。
    private string BackupPath => Path.Combine(dataDirectory, "registry-backup.json");

    public IEnumerable<string> Status()
    {
        foreach (var name in Protocols.Names)
        {
            using var key = Registry.CurrentUser.OpenSubKey($@"{classesRoot}\{name}\shell\open\command");
            yield return $"{name}: {key?.GetValue("") ?? $"({L.Get("NotRegistered")})"}";
        }
    }

    public void Register(string executable)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        if (!File.Exists(executable)) throw new FileNotFoundException(L.Get("EngineMissing"), executable);
        Directory.CreateDirectory(dataDirectory);
        var command = $"\"{executable}\" --invoke \"%1\"";
        List<RegistrySnapshot> backup;
        if (File.Exists(BackupPath))
        {
            backup = ReadBackup();
            if (backup.Any(s => s.InstalledCommand != command))
                throw new InvalidOperationException(L.Get("PathChanged"));
            foreach (var name in Protocols.Names)
            {
                var current = ReadCommand(name);
                if (current is not null && !backup.Any(s => s.Name == name && s.InstalledCommand == current))
                    throw new InvalidOperationException(L.Get("ProtocolClaimed", name));
            }
        }
        else
        {
            // 第一次寫入前備份三個協議，且拒絕覆蓋其他程式的處理命令。
            backup = [];
            foreach (var name in Protocols.Names)
            {
                using var commandKey = Registry.CurrentUser.OpenSubKey($@"{classesRoot}\{name}\shell\open\command");
                if (commandKey is not null) throw new InvalidOperationException(L.Get("ProtocolCommandExists", name));
                if (classesRoot == @"Software\Classes")
                {
                    using var effective = Registry.ClassesRoot.OpenSubKey($@"{name}\shell\open\command");
                    if (effective is not null) throw new InvalidOperationException(L.Get("ProtocolSystemHandlerExists", name));
                }
                var existing = ReadCommand(name);
                if (existing is not null) throw new InvalidOperationException(L.Get("ProtocolHandlerExists", name));
                using var key = Registry.CurrentUser.OpenSubKey($@"{classesRoot}\{name}");
                backup.Add(new RegistrySnapshot(name, key is not null, key?.GetValueNames().Contains("URL Protocol") == true, key?.GetValue("URL Protocol")?.ToString(), existing, command));
            }
            var temp = BackupPath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(backup, HandlerConfig.JsonOptions));
            File.Move(temp, BackupPath, false);
        }

        foreach (var name in Protocols.Names)
        {
            using var root = Registry.CurrentUser.CreateSubKey($@"{classesRoot}\{name}");
            root.SetValue("URL Protocol", "");
            using var key = root.CreateSubKey(@"shell\open\command");
            key.SetValue("", command);
        }
    }

    public void Unregister()
    {
        if (!File.Exists(BackupPath)) throw new InvalidOperationException(L.Get("BackupMissing"));
        var backup = ReadBackup();
        // 若使用者或其他程式已改動命令，停止還原，避免刪掉別人的設定。
        foreach (var item in backup)
        {
            var current = ReadCommand(item.Name);
            if (current == item.InstalledCommand) continue;
            using var commandKey = Registry.CurrentUser.OpenSubKey($@"{classesRoot}\{item.Name}\shell\open\command");
            if (current is not null || commandKey is not null || item.CommandValue is not null)
                throw new InvalidOperationException(L.Get("ProtocolChanged", item.Name));
        }

        foreach (var item in backup)
        {
            using var root = Registry.CurrentUser.OpenSubKey($@"{classesRoot}\{item.Name}", true)!;
            if (item.CommandValue is null)
            {
                root.DeleteSubKey(@"shell\open\command", false);
                DeleteIfEmpty(root, @"shell\open");
                DeleteIfEmpty(root, "shell");
            }
            else
            {
                using var command = root.CreateSubKey(@"shell\open\command");
                command.SetValue("", item.CommandValue);
            }
            if (item.UrlProtocolExisted) root.SetValue("URL Protocol", item.UrlProtocolValue ?? "");
            else root.DeleteValue("URL Protocol", false);
        }
        foreach (var item in backup.Where(s => !s.RootExisted))
        {
            using var root = Registry.CurrentUser.OpenSubKey($@"{classesRoot}\{item.Name}");
            if (root is not null && root.SubKeyCount == 0 && root.ValueCount == 0)
                Registry.CurrentUser.DeleteSubKey($@"{classesRoot}\{item.Name}", false);
        }
        File.Delete(BackupPath);
    }

    private static void DeleteIfEmpty(RegistryKey root, string relative)
    {
        using var key = root.OpenSubKey(relative);
        if (key is not null && key.SubKeyCount == 0 && key.ValueCount == 0) root.DeleteSubKey(relative, false);
    }

    private string? ReadCommand(string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey($@"{classesRoot}\{name}\shell\open\command");
        return key?.GetValue("")?.ToString();
    }

    private List<RegistrySnapshot> ReadBackup()
    {
        var value = JsonSerializer.Deserialize<List<RegistrySnapshot>>(File.ReadAllText(BackupPath), HandlerConfig.JsonOptions);
        if (value?.Count != Protocols.Names.Length || value.Any(s => !Protocols.Names.Contains(s.Name)))
            throw new InvalidDataException(L.Get("InvalidBackup"));
        return value;
    }
}

public sealed record RegistrySnapshot(string Name, bool RootExisted, bool UrlProtocolExisted, string? UrlProtocolValue, string? CommandValue, string InstalledCommand);
