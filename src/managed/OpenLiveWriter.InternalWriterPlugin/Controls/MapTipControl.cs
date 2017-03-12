// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using OpenLiveWriter.CoreServices;
using System.Windows.Forms;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{
    internal class MapTipControl : UserControl
    {
        private Bitmap _tipIcon;

        public MapTipControl()
        {
            _tipIcon = ResourceHelper.LoadAssemblyResourceBitmap("Images.TipIcon.png");

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        public string TipText
        {
            get
            {
                return _tipText;
            }
            set
            {
                _tipText = value;
                Invalidate();
            }
        }
        private string _tipText = String.Empty;

        public bool ShowIcon
        {
            get
            {
                return _showIcon;
            }
            set
            {
                _showIcon = value;
                Invalidate();
            }
        }
        private bool _showIcon = true;

        public int TextOpacityPct
        {
            get
            {
                return _textOpacityPct;
            }
            set
            {
                _textOpacityPct = value;
                Invalidate();
            }
        }
        private int _textOpacityPct = 70;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);

            int textX = 0;
            if (ShowIcon)
            {
                g.DrawImage(false, _tipIcon, new Point(0, (Height / 2) - (_tipIcon.Height / 2)));
                textX = _tipIcon.Width + 2;
            }

            if (TipText != null)
            {
                TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;

                // setup text rect
                Rectangle textRectangle = new Rectangle(textX, 0, Width - textX - 3, Height);

                // draw string
                g.DrawText(TipText, Font, textRectangle, Color.FromArgb(GraphicsHelper.Opacity(TextOpacityPct), ForeColor), flags);
            }

        }
    }
}
