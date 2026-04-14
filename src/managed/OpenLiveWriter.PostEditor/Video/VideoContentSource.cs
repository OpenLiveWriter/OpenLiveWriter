// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Platform;

namespace OpenLiveWriter.PostEditor.Video
{
    [WriterPlugin(VideoContentSource.ID, "Video (Deprecated)",
        ImagePath = "Images.InsertVideo.png",
        PublisherUrl = "https://github.com/OpenLiveWriter/OpenLiveWriter",
        Description = "Video embedding feature - no longer available (Flash-based embeds deprecated).")]

    [InsertableContentSource("Video (Deprecated)", SidebarText = "Video")]
    [CustomLocalizedPlugin("Videos")]

    public class VideoContentSource : SmartContentSource
    {
        public const string ID = "0ABB7CC8-30EB-4F34-8080-22DA77ED20C3";

        // Keep Tab enum for backward compatibility with callers
        public enum Tab
        {
            Web,
            File,
            Service
        }

        public override OpenLiveWriter.Api.DialogResult CreateContent(IBlogClientUIContext dialogOwner, ISmartContent content)
        {
            var ownerWindow = new Win32WindowImpl(dialogOwner.NativeWindowHandle);
            MessageBox.Show(
                ownerWindow,
                "The Video embed feature is no longer available.\n\n" +
                "This feature relied on Flash-based video embeds (YouTube, Google Video, etc.) " +
                "which are no longer supported by browsers since Flash was discontinued in 2020.\n\n" +
                "To embed a video, copy the iframe embed code from YouTube and paste it directly " +
                "into your post using the Source/HTML view.",
                "Video Feature Deprecated",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return OpenLiveWriter.Api.DialogResult.Cancel;
        }

        public override ISmartContentEditor CreateEditor(ISmartContentEditorSite contentEditorSite)
        {
            return null;
        }

        public override string GenerateEditorHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            return "<p><em>[Video content no longer supported - Flash embeds deprecated]</em></p>";
        }

        public override string GeneratePublishHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            return "<p><em>[Video content no longer supported - Flash embeds deprecated]</em></p>";
        }

        // Stub for backward compatibility - called when pasting video embed code
        public void CreateContentFromEmbed(string embed, ISmartContent content)
        {
            // No-op - feature deprecated
        }
    }
}
