// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class CommandWeblogPicker : Command, CommandBarButtonLightweightControl.ICustomButtonBitmapPaint
    {
        public CommandWeblogPicker() : base(CommandId.WeblogPicker)
        {
        }

        private WeblogPicker _weblogPickerHelper;
        public WeblogPicker WeblogPickerHelper
        {
            set
            {
                _weblogPickerHelper = value;
            }
        }

        public int Width
        {
            get { return _weblogPickerHelper == null ? 0 : _weblogPickerHelper.Width; }
        }

        public int Height
        {
            get { return _weblogPickerHelper == null ? 0 : _weblogPickerHelper.Height; }
        }

        public void Paint(BidiGraphics g, Rectangle bounds, CommandBarButtonLightweightControl.DrawState drawState)
        {
            _weblogPickerHelper.Paint(g, bounds);
        }

        /// <summary>
        /// Knows how to draw the weblog picker.
        ///
        /// This is not a control--it has no behavior. All it does is
        /// perform layout and paint.
        /// </summary>
        public class WeblogPicker
        {
            private const int IMAGE_PADDING_RIGHT = 4;
            private const int MAX_WIDTH = 175;
            private const int PADDING_LEFT = 4;
            private const int PADDING_RIGHT = 8;

            private Image _image;
            private Icon _icon;
            private string _providerName = string.Empty;
            private string _blogName = string.Empty;

            private TextFormatFlags _textFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
                                                       TextFormatFlags.Right;
            private int _providerNameLeft;
            private int _providerNameWidth;
            private int _providerNameHeight;
            private int _blogNameWidth;
            private int _imageLeft;
            private int _width;
            private int _height;
            private Font _font;

            public WeblogPicker(Font font, Image image, Icon icon, string providerName, string blogName)
            {
                _font = font;
                _icon = icon;
                _image = image;
                _providerName = providerName;
                _blogName = blogName;
            }

            public int Width
            {
                get { return _width; }
            }

            public int Height
            {
                get { return _height; }
            }

            private int IconWidth { get { return 16; } }
            private int IconHeight { get { return 16; } }

            public void Layout(Graphics g)
            {
                _height = SystemButtonHelper.LARGE_BUTTON_TOTAL_SIZE;

                float providerNameFullWidth;
                using (Font boldFont = new Font(_font, FontStyle.Bold))
                {
                    Size providerNameFullSize = TextRenderer.MeasureText(g, _providerName, boldFont, Size.Empty, _textFormatFlags);
                    _providerNameHeight = (int)Math.Ceiling((double)providerNameFullSize.Height);
                    providerNameFullWidth = providerNameFullSize.Width;
                }
                int imageWidth = 0;
                if (_image != null)
                    imageWidth = _image.Width + IMAGE_PADDING_RIGHT;
                else if (_icon != null)
                    imageWidth = IconWidth + IMAGE_PADDING_RIGHT;

                float width = providerNameFullWidth + imageWidth;

                width = Math.Max(width, TextRenderer.MeasureText(_blogName, _font, Size.Empty, _textFormatFlags).Width);

                width += PADDING_RIGHT + PADDING_LEFT;
                width = Math.Min(width, MAX_WIDTH);
                _width = (int)Math.Ceiling(width);

                _providerNameWidth = (int)Math.Ceiling(Math.Min(providerNameFullWidth, _width - PADDING_RIGHT - PADDING_LEFT - imageWidth));
                _providerNameLeft = _width - PADDING_RIGHT - _providerNameWidth;
                _imageLeft = _providerNameLeft - imageWidth;
                _blogNameWidth = _width - PADDING_RIGHT - PADDING_LEFT;
            }

            public void Paint(BidiGraphics g, Rectangle bounds)
            {
                using (g.Container(bounds.X, bounds.Y))
                {
                    g.Graphics.CompositingMode = CompositingMode.SourceOver;
                    g.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                    using (Font boldFont = new Font(_font, FontStyle.Bold))
                    {
                        Rectangle providerNameRect = new Rectangle(_providerNameLeft, 4, _providerNameWidth, _providerNameHeight);

                        if (_image != null)
                            g.DrawImage(false, _image, (int)(providerNameRect.Left - _image.Width - IMAGE_PADDING_RIGHT), (int)(providerNameRect.Top - 1));

                        g.DrawText(_providerName, boldFont, providerNameRect, Color.White, _textFormatFlags);
                    }

                    g.DrawText(_blogName, _font, new Rectangle(PADDING_LEFT, 23, _blogNameWidth, _providerNameHeight), Color.White, _textFormatFlags);
                }

                // This is down here because DrawIcon has a bug (at least with .NET 1.1) where it
                // doesn't respect any translation transforms that have been applied to the Graphics
                // object in GDI+.
                if (_image == null && _icon != null)
                    g.DrawIcon(false, _icon, new Rectangle(bounds.Left + _imageLeft, bounds.Top + 3, IconWidth, IconHeight));
            }
        }
    }
}
