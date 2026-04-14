// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Platform;
using DialogResult = OpenLiveWriter.Api.DialogResult;

namespace OpenLiveWriter.InternalWriterPlugin
{
    [WriterPlugin("C62021F8-9D77-4E84-BD14-18CE70F02159", "Map (Deprecated)",
         ImagePath = "Images.InsertMap.png",
         PublisherUrl = "https://github.com/OpenLiveWriter/OpenLiveWriter",
         Description = "Map insertion feature - no longer available (Bing Maps API deprecated).")]

    [InsertableContentSource("Map (Deprecated)", SidebarText = "Map")]

    [CustomLocalizedPlugin("Map")]
    public class MapContentSource : SmartContentSource
    {
        public const string ID = "C62021F8-9D77-4E84-BD14-18CE70F02159";

        public override DialogResult CreateContent(IBlogClientUIContext dialogOwner, ISmartContent content)
        {
            var ownerWindow = new Win32WindowImpl(dialogOwner.NativeWindowHandle);
            MessageBox.Show(
                ownerWindow,
                "The Map feature is no longer available.\n\n" +
                "The Bing Maps/Virtual Earth API that this feature relied on has been deprecated.\n\n" +
                "Consider using a web-based map service and inserting it as HTML or an image.",
                "Map Feature Deprecated",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return DialogResult.Cancel;
        }

        public override ISmartContentEditor CreateEditor(ISmartContentEditorSite contentEditorSite)
        {
            return null;
        }

        public override string GenerateEditorHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            return "<p><em>[Map content no longer supported]</em></p>";
        }

        public override string GeneratePublishHtml(ISmartContent content, IPublishingContext publishingContext)
        {
            // For existing posts with maps, try to preserve the link
            MapSettings settings = new MapSettings(content.Properties);
            if (!string.IsNullOrEmpty(settings.LiveMapUrl))
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "<p><a href=\"{0}\">View Map</a></p>",
                    HtmlServices.HtmlEncode(settings.LiveMapUrl));
            }
            return "<p><em>[Map content no longer supported]</em></p>";
        }
    }
}
