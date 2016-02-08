// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    public class XPBitmapButton : Button
    {
        public XPBitmapButton()
        {
            FlatStyle = FlatStyle.Standard;
            UseCompatibleTextRendering = false;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateImages();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            UpdateImages();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            UpdateImages();
            Debug.Assert(FlatStyle == FlatStyle.Standard, "XPBitmapButton only works when FlatStyle is Standard");
        }

        public void Initialize(Bitmap normalImage, Bitmap disabledImage, ContentAlignment alignment)
        {
            Initialize(normalImage, disabledImage);
            ImageAlign = alignment;
        }

        public void Initialize(Bitmap normalImage, Bitmap disabledImage)
        {
            if (normalImage == null)
                throw new ArgumentNullException("normalImage");

            _normalImage = normalImage;
            _disabledImage = disabledImage;
            mirrored = false;

            UpdateImages();

            // subscribe to enabled changed events to update image
            EnabledChanged += new EventHandler(XPBitmapButton_EnabledChanged);
        }

        public int GetPreferredWidth()
        {
            int width = DisplayHelper.MeasureButton(this);
            int imageWidth = _normalImage.Width;
            return width + imageWidth + (int)DisplayHelper.ScaleX(5) + Padding.Horizontal;
        }

        private void XPBitmapButton_EnabledChanged(object sender, EventArgs e)
        {
            UpdateImages();
        }

        private void UpdateImages()
        {
            if (_normalImage == null)
                return;

            if (RightToLeft == RightToLeft.Yes ^ mirrored)
            {
                _normalImage = BidiHelper.Mirror(_normalImage);
                if (_disabledImage != null)
                    _disabledImage = BidiHelper.Mirror(_disabledImage);
                mirrored = !mirrored;
            }
            Image = Enabled ? _normalImage : (_disabledImage ?? _normalImage);
        }

        private Bitmap _normalImage;
        private Bitmap _disabledImage;
        private bool mirrored;
    }
}
