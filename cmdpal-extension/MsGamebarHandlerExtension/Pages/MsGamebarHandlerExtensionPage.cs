// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace MsGamebarHandlerExtension;

internal sealed partial class MsGamebarHandlerExtensionPage : ListPage
{
    public MsGamebarHandlerExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "ms-gamebar Handler";
        Name = "管理 Game Bar 協議";
    }

    public override IListItem[] GetItems()
    {
        return
        [
            new ListItem(new HandlerAction("安裝處理程式", HandlerOperations.Install)) { Title = "安裝處理程式", Subtitle = "接管三個 Game Bar 協議" },
            new ListItem(new HandlerAction("解除安裝處理程式", HandlerOperations.Uninstall)) { Title = "解除安裝處理程式", Subtitle = "還原安裝前的協議狀態" },
            new ListItem(new HandlerAction("開啟配置", HandlerOperations.OpenConfig)) { Title = "開啟配置", Subtitle = "使用預設編輯器修改 config.json" },
            new ListItem(new RuleListPage()) { Title = "管理規則", Subtitle = "新增、修改、啟停、刪除及排序" },
            new ListItem(new StatusPage()) { Title = "查看協議狀態" },
            new ListItem(new RecentEventsPage()) { Title = "最近事件" },
        ];
    }
}
