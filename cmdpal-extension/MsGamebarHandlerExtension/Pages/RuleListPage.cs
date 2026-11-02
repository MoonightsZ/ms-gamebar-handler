using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace MsGamebarHandlerExtension;

internal sealed partial class RuleListPage : ListPage
{
    public RuleListPage() { Title = "規則管理"; Name = "查看"; }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>
        {
            new ListItem(new RuleFormPage(null)) { Title = "新增規則" },
        };
        try
        {
            var rules = HandlerOperations.ReadRules();
            for (var index = 0; index < rules.Count; index++)
            {
                var rule = rules[index];
                items.Add(new ListItem(new RuleFormPage(rule))
                {
                    Title = rule.Id,
                    Subtitle = $"{(rule.Enabled ? "啟用" : "停用")} · {rule.MatchType}: {rule.MatchValue} → {rule.ActionType}",
                });
                items.Add(new ListItem(new HandlerAction("切換規則", () => HandlerOperations.SetRuleEnabled(rule.Id, !rule.Enabled)))
                {
                    Title = $"{(rule.Enabled ? "停用" : "啟用")} {rule.Id}",
                });
                if (index > 0) items.Add(new ListItem(new HandlerAction("上移規則", () => HandlerOperations.MoveRule(rule.Id, "up"))) { Title = $"上移 {rule.Id}" });
                if (index + 1 < rules.Count) items.Add(new ListItem(new HandlerAction("下移規則", () => HandlerOperations.MoveRule(rule.Id, "down"))) { Title = $"下移 {rule.Id}" });
                items.Add(new ListItem(new ConfirmHandlerAction("刪除規則", $"確定刪除 {rule.Id}？", () => HandlerOperations.DeleteRule(rule.Id)))
                {
                    Title = $"刪除 {rule.Id}",
                });
            }
        }
        catch (Exception ex)
        {
            items.Add(new ListItem(new NoOpCommand()) { Title = $"無法讀取規則：{ex.Message}" });
        }
        return [.. items];
    }
}
