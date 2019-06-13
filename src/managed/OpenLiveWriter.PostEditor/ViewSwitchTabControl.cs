// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor
{
    public partial class ViewSwitchTabControl : Control
    {
        private bool selected;
        private bool clipLeftBorder;
        private bool clipRightBorder;

        public ViewSwitchTabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.DoubleBuffer
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.SupportsTransparentBackColor,
                true);

            InitializeComponent();
        }

        public event EventHandler SelectedChanged;

        protected override void OnTextChanged(EventArgs e)
        {
            PerformLayout();
            Invalidate();
            base.OnTextChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Space || e.KeyData == Keys.Enter)
                Selected = true;
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value != selected)
                {
                    selected = value;
                    AccessibilityObject.Name = Text + (value ? "*" : "");

                    if (selected)
                        Select();

                    PerformLayout();
                    Invalidate();

                    if (SelectedChanged != null)
                        SelectedChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool ClipLeftBorder
        {
            get { return clipLeftBorder; }
            set
            {
                if (value != clipLeftBorder)
                {
                    clipLeftBorder = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        public bool ClipRightBorder
        {
            get { return clipRightBorder; }
            set
            {
                if (value != clipRightBorder)
                {
                    clipRightBorder = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        public string Shortcut
        {
            internal get; set;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            Size size = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPrefix);
            if (size.Height == 0 || size.Width == 0)
            {
                // We're in some weird state, do nothing
                return;
            }

            size.Width += (int)Math.Ceiling(DisplayHelper.ScaleX(20)); // Scale tab width for DPI, however height doesn't look quite right
            size.Height += 8;

            // Extra height for Selected state, which we won't use if we aren't selected
            size.Height += 2;

            if (clipLeftBorder)
                size.Width -= 1;
            if (clipRightBorder)
                size.Width -= 1;

            Size = size;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Color bgColor, borderColor, textColor;
            Color lineColor = new HCColor(0xA5A5A5, SystemColors.ControlDarkDark);

            if (Selected)
            {
                bgColor = new HCColor(Color.White, SystemColors.Control);
                textColor = new HCColor(0x333333, SystemColors.ControlText);
                borderColor = new HCColor(0xA5A5A5, SystemColors.ControlDarkDark);
            }
            else
            {
                bgColor = new HCColor(0xF7F7F7, SystemColors.Control);
                textColor = new HCColor(0x6E6E6E, SystemColors.ControlText);
                borderColor = new HCColor(0xC9C9C9, SystemColors.ControlDark);
            }

            BidiGraphics g = new BidiGraphics(pe.Graphics, ClientRectangle);

            DrawTabFace(g, bgColor, borderColor, lineColor);
            DrawTabContents(g, textColor);
        }

        private void DrawTabFace(BidiGraphics g, Color bgColor, Color borderColor, Color lineColor)
        {
            Rectangle borderRect = ClientRectangle;

            if (!Selected)
                borderRect.Height -= 2;

            // Remove one pixel for the bottom edge of the tab.
            // We don't want it filled in by the face color as
            // that would cause the corner pixels of the tab
            // to be filled in.
            borderRect.Height -= 1;

            using (Brush b = new SolidBrush(bgColor))
                g.Graphics.FillRectangle(b, borderRect);

            borderRect.Width -= 1;

            if (Selected)
                borderRect.Y -= 1;
            else
            {
                borderRect.Height -= 1;
            }

            if (clipLeftBorder)
            {
                borderRect.X -= 1;
                borderRect.Width += 1;
            }

            if (clipRightBorder)
                borderRect.Width += 1;

            Region clip = g.Graphics.Clip;
            clip.Exclude(g.TranslateRectangle(new Rectangle(borderRect.X, borderRect.Bottom, 1, 1)));
            clip.Exclude(g.TranslateRectangle(new Rectangle(borderRect.Right, borderRect.Bottom, 1, 1)));
            g.Graphics.Clip = clip;

            using (Pen p = new Pen(borderColor))
                g.DrawRectangle(p, borderRect);

            if (!Selected)
                using (Pen p = new Pen(lineColor))
                    g.DrawLine(p, 0, 0, ClientSize.Width, 0);
        }

        private void DrawTabContents(BidiGraphics g, Color textColor)
        {
            Rectangle logicalClientRect = ClientRectangle;

            if (!Selected)
                logicalClientRect.Height -= 2;

            // Make up for the top "border" being just off the top of the control
            logicalClientRect.Y -= 1;
            logicalClientRect.Height += 1;

            if (clipLeftBorder)
            {
                logicalClientRect.X -= 1;
                logicalClientRect.Width += 1;
            }

            if (clipRightBorder)
                logicalClientRect.Width += 1;

            g.DrawText(Text, Font, logicalClientRect, textColor, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            //            if (Focused)
            //            {
            //                Rectangle rect = logicalClientRect;
            //                rect.Inflate(-2, -2);
            //                g.DrawFocusRectangle(rect);
            //            }
        }
    }
}
