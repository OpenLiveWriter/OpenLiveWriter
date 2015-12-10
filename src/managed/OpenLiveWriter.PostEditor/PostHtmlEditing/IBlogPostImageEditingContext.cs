// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.PostEditor.Emoticons;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Interface for managing post-related image data.
    /// </summary>
    public interface IBlogPostImageEditingContext : IImageTargetEditor
    {
        string CurrentAccountId { get; }
        string ImageServiceId { get; }
        IEditorOptions EditorOptions { get; }
        BlogPostImageDataList ImageList { get; }
        ImageDecoratorsManager DecoratorsManager { get; }
        EmoticonsManager EmoticonsManager { get; }
        ISupportingFileService SupportingFileService { get; }
        void ActivateDecoratorsManager();
        void DeactivateDecoratorsManager();
    }
}
