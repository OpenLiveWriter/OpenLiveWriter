// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Api;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar
{
    /// <summary>
    /// Easy access to a SmartContentEditor given its identifier
    /// </summary>
    public interface ISmartContentEditorCache
    {
        SmartContentEditor GetSmartContentEditor(string contentSourceId);
    }
}
