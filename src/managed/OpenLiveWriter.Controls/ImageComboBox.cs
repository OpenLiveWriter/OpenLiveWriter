// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    public class ImageComboBox : ComboBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImageComboBox(Size imageSize)
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = CalculateItemHeight(imageSize);
        }

        public bool AllowMirroring
        {
            get
            {
                return _allowMirroring;
            }
            set
            {
                _allowMirroring = value;
            }
        }

        private bool _allowMirroring = true;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // screen invalid drawing states
            if (DesignMode || e.Index == -1)
                return;

            // get item text and image
            string itemText = Items[e.Index].ToString();
            IComboItem comboItem = (IComboItem)Items[e.Index];
            Image itemImage = comboItem.Image;

            // determine state
            bool selected = (e.State & DrawItemState.Selected) > 0;

            // calculate colors
            Color backColor, textColor;
            if (selected && Focused)
            {
                textColor = SystemColors.ControlText;
                backColor = Color.FromArgb(50, SystemColors.Highlight);
            }
            else
            {
                backColor = SystemColors.Window;
                textColor = SystemColors.ControlText;
            }
            Rectangle area = e.Bounds;
            //overlap issue in RTL builds in the selected box area ONLY
            if (area.X == 21)
            {
                area = ClientRectangle;
            }
            BidiGraphics g = new BidiGraphics(e.Graphics, area);

            // draw background (always paint white over it first)
            g.FillRectangle(Brushes.White, area);
            using (SolidBrush solidBrush = new SolidBrush(backColor))
                g.FillRectangle(solidBrush, area);

            // draw icon
            g.DrawImage(AllowMirroring, itemImage, area.Left + IMAGE_INSET, area.Top + area.Height / 2 - itemImage.Height / 2);

            // text format and drawing metrics
            TextFormatFlags ellipsesStringFormat = TextFormatFlags.VerticalCenter |
                   TextFormatFlags.WordBreak | TextFormatFlags.ExpandTabs | TextFormatFlags.WordEllipsis;
            int leftMargin = IMAGE_INSET + itemImage.Width + TEXT_INSET;

            // draw title line
            // post title
            int titleWidth = area.Right - leftMargin;
            Rectangle titleRectangle = new Rectangle(area.Left + leftMargin, area.Top, titleWidth, area.Height);
            g.DrawText(
                itemText,
                e.Font, titleRectangle, textColor, ellipsesStringFormat);

            // separator line if necessary
            if (!selected && DroppedDown)
            {
                using (Pen pen = new Pen(SystemColors.ControlLight))
                    g.DrawLine(pen, area.Left, area.Bottom - 1, area.Right, area.Bottom - 1);
            }

            // focus rectangle if necessary
            e.DrawFocusRectangle();
        }

        private int CalculateItemHeight(Size imageSize)
        {
            float fontHeight = Font.GetHeight();
            return TOP_INSET +                                  // top-margin
                Math.Max(Convert.ToInt32(fontHeight), imageSize.Height) + // contentHeight
                BOTTOM_INSET;                                   // air at bottom
        }

        // item metrics
        private const int TOP_INSET = 2;
        private const int BOTTOM_INSET = 2;
        private const int IMAGE_INSET = 3;
        private const int TEXT_INSET = 5;

        public interface IComboItem
        {
            Image Image { get; }
        }
    }
}
