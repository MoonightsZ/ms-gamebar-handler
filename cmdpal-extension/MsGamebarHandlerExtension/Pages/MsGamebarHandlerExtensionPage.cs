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
        Title = "ms-gamebar-handler";
        Name = "管理 Game Bar 協議";
    }

    public override IListItem[] GetItems()
    {
        return
        [
            new ListItem(new HandlerAction("安裝管理工具", HandlerOperations.Install)) { Title = "安裝管理工具", Subtitle = "複製命令列工具並初始化配置" },
            new ListItem(new HandlerAction("還原舊版手動註冊", HandlerOperations.RestoreLegacyRegistration)) { Title = "還原舊版手動註冊", Subtitle = "清理舊版登錄設定；不會移除 MSIX" },
            new ListItem(new RecentEventsPage()) { Title = "最近事件" },
        ];
    }
}
