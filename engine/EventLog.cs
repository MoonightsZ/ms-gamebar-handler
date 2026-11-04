namespace MsGamebarHandler;

public static class EventLog
{
    public static void Append(string path, string line)
    {
        // 三個 Game Bar 協議可能同時啟動三個程序。事件檔短暫被占用時，等待後重試，
        // 避免只記下第一個事件。其他 I/O 錯誤仍交給入口程式記錄到 errors.log。
        const int attempts = 20;
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                File.AppendAllText(path, line);
                return;
            }
            catch (IOException ex) when (IsSharingViolation(ex) && attempt < attempts - 1)
            {
                Thread.Sleep(50);
            }
        }
    }

    private static bool IsSharingViolation(IOException ex) =>
        ex.HResult is unchecked((int)0x80070020) or unchecked((int)0x80070021);
}
