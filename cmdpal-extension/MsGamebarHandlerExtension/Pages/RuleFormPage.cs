using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace MsGamebarHandlerExtension;

internal sealed partial class RuleFormPage : ContentPage
{
    private readonly RuleFormContent _form;

    public RuleFormPage(RuleView? original)
    {
        Title = original is null ? "新增規則" : $"編輯 {original.Id}";
        Name = "開啟";
        _form = new RuleFormContent(original);
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class RuleFormContent : FormContent
{
    private readonly string? _originalId;

    public RuleFormContent(RuleView? original)
    {
        _originalId = original?.Id;
        var rule = original ?? new RuleView("", true, 0, "any", "", "ignore", "", "");
        DataJson = new JsonObject
        {
            ["rule"] = new JsonObject
            {
                ["id"] = rule.Id,
                ["enabled"] = rule.Enabled ? "true" : "false",
                ["cooldown"] = rule.CooldownSeconds.ToString(CultureInfo.InvariantCulture),
                ["matchType"] = rule.MatchType,
                ["matchValue"] = rule.MatchValue,
                ["actionType"] = rule.ActionType,
                ["actionValue"] = rule.ActionValue,
                ["arguments"] = rule.Arguments,
            },
        }.ToJsonString();

        TemplateJson = """
            {
              "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
              "type": "AdaptiveCard",
              "version": "1.6",
              "body": [
                { "type": "Input.Text", "id": "id", "label": "規則 ID", "value": "${rule.id}", "isRequired": true },
                { "type": "Input.Toggle", "id": "enabled", "title": "啟用", "value": "${rule.enabled}", "valueOn": "true", "valueOff": "false" },
                { "type": "Input.Text", "id": "cooldown", "label": "冷卻秒數（0–86400）", "value": "${rule.cooldown}", "isRequired": true },
                { "type": "Input.ChoiceSet", "id": "matchType", "label": "匹配方式", "style": "compact", "value": "${rule.matchType}", "choices": [
                  { "title": "任何 URI", "value": "any" },
                  { "title": "包含文字", "value": "contains" },
                  { "title": "完全相同", "value": "equals" },
                  { "title": "協議名稱", "value": "protocol" },
                  { "title": "正則表達式", "value": "regex" }
                ] },
                { "type": "Input.Text", "id": "matchValue", "label": "匹配值", "value": "${rule.matchValue}" },
                { "type": "Input.ChoiceSet", "id": "actionType", "label": "動作", "style": "compact", "value": "${rule.actionType}", "choices": [
                  { "title": "忽略", "value": "ignore" },
                  { "title": "開啟 URI", "value": "launch-uri" },
                  { "title": "執行程式（需白名單）", "value": "run" }
                ] },
                { "type": "Input.Text", "id": "actionValue", "label": "動作值（URI 或程式完整路徑）", "value": "${rule.actionValue}" },
                { "type": "Input.Text", "id": "arguments", "label": "程式參數", "value": "${rule.arguments}" }
              ],
              "actions": [{ "type": "Action.Submit", "title": "儲存規則" }]
            }
            """;
    }

    public override CommandResult SubmitForm(string payload)
    {
        try
        {
            var input = JsonNode.Parse(payload)?.AsObject() ?? throw new InvalidDataException("表單內容無效");
            string Value(string name) => input[name]?.ToString() ?? "";
            var id = Value("id");
            if (_originalId is not null && !string.Equals(id, _originalId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("修改規則 ID 請先刪除舊規則，再新增規則");
            if (!int.TryParse(Value("cooldown"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var cooldown))
                throw new InvalidDataException("冷卻秒數須為整數");
            var rule = new RuleView(id, Value("enabled") == "true", cooldown, Value("matchType"), Value("matchValue"), Value("actionType"), Value("actionValue"), Value("arguments"));
            if (rule.ActionType == "run")
                return CommandResult.Confirm(new ConfirmationArgs
                {
                    Title = "確認執行程式規則",
                    Description = $"此規則可能啟動 {rule.ActionValue}。請確認配置已啟用 run 並加入白名單。",
                    PrimaryCommand = new HandlerAction("儲存規則", () => HandlerOperations.SaveRule(rule)),
                });
            return CommandResult.ShowToast(HandlerOperations.SaveRule(rule));
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast($"儲存失敗：{ex.Message}");
        }
    }
}
