// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class ImageSizePickerControl : ComboBox
    {
        public ImageSizePickerControl()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            SelectedIndexChanged += new EventHandler(ImageSizePickerComboBox_SelectedIndexChanged);
            AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.ImgSBImageSize));
        }

        public void Resynchronize(GetScaledImageSizeDelegate scaledImageSizeDelegate)
        {
            // record scaled image size delegate
            _scaledImageSizeDelegate = scaledImageSizeDelegate;

            // force a full refresh of the items and selection (will cause new callbacks to get
            // fired for toString -- required to get the scaling right)
            SelectedImageSize = SelectedImageSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SelectedIndexChanged -= new EventHandler(ImageSizePickerComboBox_SelectedIndexChanged);
            }
            base.Dispose(disposing);
        }

        public event EventHandler SelectedImageSizeChanged;

        public ImageSize SelectedImageSize
        {
            get
            {
                if (SelectedItem != null)
                    return (SelectedItem as ImageSizeComboItem).ImageSize;
                else
                    return ImageSize.Unknown;
            }
            set
            {
                InitializeComboItems();

                if (value != ImageSize.Unknown)
                {
                    foreach (ImageSizeComboItem item in Items)
                    {
                        if (item.ImageSize == value)
                        {
                            SelectedItem = item;
                            return;
                        }
                    }
                }

                // unknown is valid, but other items that don't match are a programming error
                SelectedItem = null;
            }
        }

        private void InitializeComboItems()
        {
            Items.Clear();
            if (!DesignMode)
            {
                Items.Add(new ImageSizeComboItem(ImageSize.Small, Res.Get(StringId.ImgSBSizerSmall), _scaledImageSizeDelegate));
                Items.Add(new ImageSizeComboItem(ImageSize.Medium, Res.Get(StringId.ImgSBSizerMedium), _scaledImageSizeDelegate));
                Items.Add(new ImageSizeComboItem(ImageSize.Large, Res.Get(StringId.ImgSBSizerLarge), _scaledImageSizeDelegate));
                Items.Add(new ImageSizeComboItem(ImageSize.Original, Res.Get(StringId.ImgSBSizerOriginal), _scaledImageSizeDelegate));
            }
        }

        private void ImageSizePickerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedImageSizeChanged != null)
                SelectedImageSizeChanged(this, e);
        }

        private class ImageSizeComboItem
        {
            public ImageSizeComboItem(ImageSize imageSize, string displayName, GetScaledImageSizeDelegate scaledImageSizeDelegate)
            {
                _imageSize = imageSize;
                _displayName = displayName;
                _scaledImageSizeDelegate = scaledImageSizeDelegate;
            }

            public ImageSize ImageSize
            {
                get { return _imageSize; }
            }

            public override string ToString()
            {
                if (_scaledImageSizeDelegate != null)
                {
                    Size scaledSize = _scaledImageSizeDelegate(ImageSize);
                    return String.Format(CultureInfo.InvariantCulture, "{0} ({1}x{2})", _displayName, scaledSize.Width, scaledSize.Height);
                }
                else
                {
                    return _displayName;
                }
            }

            private ImageSize _imageSize;
            private string _displayName;
            private GetScaledImageSizeDelegate _scaledImageSizeDelegate;
        }

        private GetScaledImageSizeDelegate _scaledImageSizeDelegate;
    }

    internal enum ImageSize { Unknown, Small, Medium, Large, Original };

    internal delegate Size GetScaledImageSizeDelegate(ImageSize baseSize);

}
