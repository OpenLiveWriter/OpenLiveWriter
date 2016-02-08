// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    public interface ISidebarContext
    {
        IWin32Window Owner { get; }

        void UpdateStatusBar(string statusText);

        void UpdateStatusBar(Image image, string statusText);

        IUndoUnit CreateUndoUnit();

        CommandManager CommandManager { get; }
    }
}
