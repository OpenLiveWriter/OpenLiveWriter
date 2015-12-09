// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class InsertablePluginsGalleryCommand : GalleryCommand<string>
    {
        public InsertablePluginsGalleryCommand()
            : base(CommandId.PluginsGallery, false)
        {
            AllowSelection = false;
        }

        public override void LoadItems()
        {
            items.Clear();
            foreach (var v in ContentSourceManager.PluginInsertableContentSources)
            {
                string clippedName = TextHelper.GetTitleFromText(v.Name, RibbonHelper.GalleryItemTextMaxChars, TextHelper.Units.Characters);

                // Be robust to WinLive 60961.
                // We can't transfer a ReadOnly image to the ribbon.
                Bitmap bitmap = v.Image;
                if (bitmap != null &&
                    Convert.ToBoolean(bitmap.Flags & (int)ImageFlags.ReadOnly))
                {
                    bitmap = v.Image.Clone(new Rectangle(0, 0, v.Image.Width, v.Image.Height), PixelFormat.Format32bppArgb);
                }

                items.Add(new GalleryItem(clippedName, bitmap, v.Id));
            }

            base.LoadItems();
        }
    }

}
