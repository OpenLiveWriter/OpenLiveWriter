// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    class MiniTab : LightweightControl
    {
        private string text;
        private readonly MiniTabContext ctx;
        private bool selected;
        private string tooltip;
        private Color? borderColor = null;

        public MiniTab(MiniTabContext ctx)
        {
            this.ctx = ctx;
            TabStop = true;
        }

        private Color? BorderColor
        {
            get
            {
                if (borderColor == null)
                {
                    if (this.LightweightControlContainerControl is MiniTabsControl)
                    {
                        borderColor = ((MiniTabsControl)this.LightweightControlContainerControl).TopBorderColor;
                    }
                }
                return borderColor;
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                PerformLayout();
                Invalidate();
            }
        }

        public string ToolTip
        {
            get { return tooltip; }
            set { tooltip = value; }
        }

        public event EventHandler SelectedChanged;

        public bool Selected
        {
            get { return selected; }
        }

        public void Select()
        {
            selected = true;
            OnSelectedChanged();
            Invalidate();
        }

        internal void Unselect()
        {
            selected = false;
            Invalidate();
        }

        protected virtual void OnSelectedChanged()
        {
            if (SelectedChanged != null)
                SelectedChanged(this, EventArgs.Empty);
        }

        public override Size DefaultVirtualSize
        {
            get
            {
                if (Parent == null)
                    return Size.Empty;
                Size size = TextRenderer.MeasureText(text, selected ? ctx.FontSelected : ctx.Font);
                size.Height += (int)(Selected ? DisplayHelper.ScaleX(7) : DisplayHelper.ScaleY(5));
                size.Width += (int)DisplayHelper.ScaleX(5) + size.Height / 2;
                return size;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            base.OnPaint(e);
            BidiGraphics g = new BidiGraphics(e.Graphics, VirtualClientRectangle);

            Rectangle tabRectangle = VirtualClientRectangle;

            if (selected)
                ColorizedResources.Instance.ViewSwitchingTabSelected.DrawBorder(e.Graphics, tabRectangle);
            else
                ColorizedResources.Instance.ViewSwitchingTabUnselected.DrawBorder(e.Graphics, tabRectangle);

            if (ColorizedResources.UseSystemColors)
            {
                if (BorderColor.HasValue)
                {
                    using (Pen pen = new Pen(BorderColor.Value))
                    {
                        if (!selected)
                            g.DrawLine(pen, tabRectangle.Left, tabRectangle.Top, tabRectangle.Right,
                                       tabRectangle.Top);
                        g.DrawLine(pen, tabRectangle.Left, tabRectangle.Top, tabRectangle.Left,
                                   tabRectangle.Bottom);
                        g.DrawLine(pen, tabRectangle.Right - 1, tabRectangle.Top, tabRectangle.Right - 1,
                                   tabRectangle.Bottom);
                        g.DrawLine(pen, tabRectangle.Left, tabRectangle.Bottom - 1,
                                   tabRectangle.Right, tabRectangle.Bottom - 1);
                    }
                }
            }

            /*
            if (!selected && !SystemInformation.HighContrast)
            {

                using (Pen p = new Pen(borderColor, 1.0f))
                    g.DrawLine(p, 0, 0, VirtualWidth, 0);
                using (Pen p = new Pen(Color.FromArgb(192, borderColor), 1.0f))
                    g.DrawLine(p, 0, 1, VirtualWidth - 1, 1);
                using (Pen p = new Pen(Color.FromArgb(128, borderColor), 1.0f))
                    g.DrawLine(p, 0, 2, VirtualWidth - 2, 2);
                using (Pen p = new Pen(Color.FromArgb(64, borderColor), 1.0f))
                    g.DrawLine(p, 0, 3, VirtualWidth - 2, 3);
            }
             * */

            Rectangle textBounds = tabRectangle;
            if (!selected)
                textBounds.Y += (int)DisplayHelper.ScaleX(3);
            else
                textBounds.Y += (int)DisplayHelper.ScaleX(3);

            Color textColor = ColorizedResources.Instance.MainMenuTextColor;
            if (selected)
                textColor = Parent.ForeColor;

            g.DrawText(Text, selected ? ctx.Font : ctx.Font, textBounds, SystemInformation.HighContrast ? SystemColors.ControlText : textColor,
                       TextFormatFlags.Top | TextFormatFlags.HorizontalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            SetToolTip(tooltip);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            SetToolTip(null);
        }
    }

    class MiniTabContext
    {
        private readonly Control parent;
        private readonly Bitmap leftOn;
        private readonly Bitmap centerOn;
        private readonly Bitmap rightOn;
        private readonly Bitmap leftOff;
        private readonly Bitmap centerOff;
        private readonly Bitmap rightOff;
        private Font font, fontSelected;

        public MiniTabContext(Control parent)
        {
            this.parent = parent;
            string suffix = SystemInformation.HighContrast ? "-hi.png" : ".png";
            leftOn = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabLeftSelected" + suffix);
            centerOn = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabCenterSelected" + suffix);
            rightOn = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabRightSelected" + suffix);
            leftOff = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabLeft" + suffix);
            centerOff = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabCenter" + suffix);
            rightOff = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabRight" + suffix);

            parent.FontChanged += parent_FontChanged;
            RefreshFonts();
        }

        void parent_FontChanged(object sender, EventArgs e)
        {
            RefreshFonts();
        }

        private void RefreshFonts()
        {
            font = parent.Font;
            fontSelected = new Font(font, FontStyle.Bold);
        }

        public Font Font
        {
            get { return font; }
        }

        public Font FontSelected
        {
            get { return fontSelected; }
        }

        public Bitmap LeftOn
        {
            get { return leftOn; }
        }

        public Bitmap CenterOn
        {
            get { return centerOn; }
        }

        public Bitmap RightOn
        {
            get { return rightOn; }
        }

        public Bitmap LeftOff
        {
            get { return leftOff; }
        }

        public Bitmap CenterOff
        {
            get { return centerOff; }
        }

        public Bitmap RightOff
        {
            get { return rightOff; }
        }
    }
}
