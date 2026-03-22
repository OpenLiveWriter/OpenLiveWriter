// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Represents an item in a gallery.
    /// </summary>
    public class RibbonGalleryItem
    {
        public string Label { get; set; }
        public Image Image { get; set; }
        public string Tooltip { get; set; }
        public object Tag { get; set; }

        public RibbonGalleryItem() { }

        public RibbonGalleryItem(string label, Image image = null)
        {
            Label = label;
            Image = image;
        }
    }

    /// <summary>
    /// Event args for gallery item clicks.
    /// </summary>
    public class GalleryItemClickEventArgs : EventArgs
    {
        public RibbonGalleryItem Item { get; }
        public int Index { get; }

        public GalleryItemClickEventArgs(RibbonGalleryItem item, int index)
        {
            Item = item;
            Index = index;
        }
    }
}
