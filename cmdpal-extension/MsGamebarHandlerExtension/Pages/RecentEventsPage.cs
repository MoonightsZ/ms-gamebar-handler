using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using MsGamebarHandler;

namespace MsGamebarHandlerExtension;

internal sealed partial class RecentEventsPage : ListPage
{
    public RecentEventsPage() { Title = L.Get("RecentEvents"); Name = L.Get("View"); }

    public override IListItem[] GetItems()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-gamebar-handler", "events.log");
        if (!File.Exists(path)) return [new ListItem(new NoOpCommand()) { Title = L.Get("NoEvents") }];
        return File.ReadLines(path).TakeLast(30).Reverse()
            .Select(line => (IListItem)new ListItem(new NoOpCommand()) { Title = line }).ToArray();
    }
}
