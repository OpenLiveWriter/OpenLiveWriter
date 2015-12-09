// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    public class SectionHeaderControl : Control
    {
        private UITheme _uiTheme;
        private readonly Font _font;
        public SectionHeaderControl()
        {
            TabStop = false;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            AccessibleRole = AccessibleRole.Grouping;

            _uiTheme = new UITheme(this);
            _font = Res.GetFont(FontSize.Large, FontStyle.Regular);
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                AccessibleName = ControlHelper.ToAccessibleName(value);
                Invalidate();
            }
        }
        private string _headerText;

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            using (Graphics g = CreateGraphics())
                Height = Convert.ToInt32(g.MeasureString(HeaderText, _font).Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);

            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            Rectangle rectangle = ClientRectangle;

            // draw text
            g.DrawText(HeaderText, _font, rectangle, ColorizedResources.Instance.SidebarHeaderTextColor, TextFormatFlags.VerticalCenter);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        private class UITheme : ControlUITheme
        {
            public Color TextColor;
            public bool DrawGradient;
            public UITheme(Control c) : base(c, true)
            {

            }

            protected override void ApplyTheme(bool highContrast)
            {
                DrawGradient = !highContrast;
                if (highContrast)
                {
                    TextColor = SystemColors.ControlText;
                    Control.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
                }
                else
                {
                    TextColor = Color.White;
                }
                base.ApplyTheme(highContrast);
            }
        }
    }
}
