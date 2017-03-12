// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    public interface ISidebar
    {
        bool AppliesToSelection(object htmlSelection);

        SidebarControl CreateSidebarControl(ISidebarContext sidebarContext);
    }
}
