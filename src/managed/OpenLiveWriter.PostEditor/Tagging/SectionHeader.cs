// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;

namespace OpenLiveWriter.PostEditor.Tagging
{
    class SectionHeader : Control
    {
        public SectionHeader()
        {
            _uiTheme = new UITheme(this);
        }
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                Invalidate();
            }
        }
        private string _headerText;

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // supress background painting to avoid flicker
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // draw background
            using (Brush brush = new SolidBrush(_uiTheme.BackgroundColor))
                e.Graphics.FillRectangle(brush, ClientRectangle);

            // draw corner
            if(_uiTheme.DrawImages)
                e.Graphics.DrawImage(HeaderCornerImage, 0, 0);

            // draw text
            if (_headerText != null)
                using (Brush brush = new SolidBrush(_uiTheme.TextColor))
                    e.Graphics.DrawString(HeaderText, Font, brush, 3, 1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        private static Bitmap HeaderCornerImage
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("Tagging.Images.HeaderCorner.png") ;
            }
        }

        private UITheme _uiTheme;
        private class UITheme : ControlUITheme
        {
            public Color TextColor;
            public Color BackgroundColor;
            public bool DrawImages;
            public UITheme(Control c) : base(c, true)
            {
            }

            protected override void ApplyTheme(bool highContrast)
            {
                DrawImages = !highContrast;
                if(highContrast)
                {
                    TextColor = SystemColors.ControlText;
                }
                else
                {
                    TextColor = Color.FromArgb(74, 101, 149);
                    BackgroundColor = Color.FromArgb(147, 176, 235);
                }
                base.ApplyTheme(highContrast);
            }
        }
    }
}
