// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    public interface ISupportsDragDropFile
    {
        DialogResult CreateContentFromFile(IWin32Window dialogOwner, ISmartContent content, string[] files, object context);
    }

    public interface ITabbedInsertDialogContentSource
    {
        DialogResult CreateContentFromTabbedDialog(IWin32Window dialogOwner, ISmartContent content, int selectedTab);
    }
}
