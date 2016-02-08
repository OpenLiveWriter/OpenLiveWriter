// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    public interface IBlogPostSidebarContext
    {
        bool SidebarVisible { get; set; }
        event EventHandler SidebarVisibleChanged;
        SmartContentEditor CurrentEditor { get; }
    }
}
