// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ILinkOptions.
    /// </summary>
    public interface ILinkOptions
    {
        bool ShowInNewWindow { get; set; }
        bool UseImageViewer { get; set; }
        string ImageViewerGroupName { get; set; }
    }
}
