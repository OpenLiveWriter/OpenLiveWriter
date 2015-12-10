// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;

namespace OpenLiveWriter.PostEditor.ContentSources
{
    // Provides classes that implement this with what they need to create instances of IInternalSmartContentContext.
    public interface IInternalSmartContentContextSource : ISmartContentEditorCache
    {
        Size BodySize { get; }
    }
}
