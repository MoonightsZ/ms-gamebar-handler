using System.Runtime.InteropServices;

namespace MsGamebarHandler;

internal static class ConsoleHost
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(uint processId);

    public static void AttachForCommand(string command)
    {
        if (command == "--invoke" || !OperatingSystem.IsWindows()) return;
        if (!AttachConsole(uint.MaxValue)) return;
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
    }
}
