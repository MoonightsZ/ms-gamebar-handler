using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace MsGamebarHandlerExtension;

internal sealed partial class RecentEventsPage : ListPage
{
    public RecentEventsPage() { Title = "最近事件"; Name = "查看"; }

    public override IListItem[] GetItems()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-gamebar-handler", "events.log");
        if (!File.Exists(path)) return [new ListItem(new NoOpCommand()) { Title = "尚無事件" }];
        return File.ReadLines(path).TakeLast(30).Reverse()
            .Select(line => (IListItem)new ListItem(new NoOpCommand()) { Title = line }).ToArray();
    }
}
