using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Win32;

namespace MsGamebarHandlerExtension;

internal sealed partial class StatusPage : ListPage
{
    private static readonly string[] ProtocolNames = ["ms-gamebar", "ms-gamingoverlay", "ms-gamebarservices"];
    public StatusPage() { Title = "協議狀態"; Name = "查看"; }

    public override IListItem[] GetItems() => ProtocolNames
        .Select(name =>
        {
            using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{name}\shell\open\command");
            return (IListItem)new ListItem(new NoOpCommand()) { Title = name, Subtitle = key?.GetValue("")?.ToString() ?? "未註冊" };
        }).ToArray();
}
