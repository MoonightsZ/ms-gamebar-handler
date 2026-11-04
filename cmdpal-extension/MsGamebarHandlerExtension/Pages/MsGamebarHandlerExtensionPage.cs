// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using MsGamebarHandler;

namespace MsGamebarHandlerExtension;

internal sealed partial class MsGamebarHandlerExtensionPage : ListPage
{
    public MsGamebarHandlerExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = L.Get("AppName");
        Name = L.Get("ManageProtocols");
    }

    public override IListItem[] GetItems()
    {
        return
        [
            new ListItem(new HandlerAction(L.Get("InstallTools"), HandlerOperations.Install)) { Title = L.Get("InstallTools"), Subtitle = L.Get("InstallToolsDescription") },
            new ListItem(new HandlerAction(L.Get("RestoreLegacy"), HandlerOperations.RestoreLegacyRegistration)) { Title = L.Get("RestoreLegacy"), Subtitle = L.Get("RestoreLegacyDescription") },
            new ListItem(new RecentEventsPage()) { Title = L.Get("RecentEvents") },
        ];
    }
}
