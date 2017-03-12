// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImagePickerControl.
    /// </summary>
    public class ImagePickerControl : ComboBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ImagePickerControl()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            IntegralHeight = false;
            ItemHeight = CalculateItemHeight();
        }

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
            Rectangle area = e.Bounds;

            e.Graphics.ResetClip();

            BidiGraphics g = new BidiGraphics(e.Graphics, area);

            Image borderImage = null;
            string borderText = "";
            object item = Items[e.Index];
            if (item != null)
                borderText = item.ToString();
            if (item is IComboImageItem)
            {
                IComboImageItem comboItem = (IComboImageItem)item;
                borderImage = comboItem.Image;
            }

            // determine state
            bool selected = (e.State & DrawItemState.Selected) > 0;

            // calculate colors
            Color backColor = SystemColors.Window;
            Color textColor = SystemColors.ControlText;

            // setup standard string format
            TextFormatFlags ellipsesStringFormat = TextFormatFlags.VerticalCenter |
                TextFormatFlags.WordBreak | TextFormatFlags.ExpandTabs | TextFormatFlags.WordEllipsis;

            // draw background
            using (SolidBrush solidBrush = new SolidBrush(backColor))
                g.FillRectangle(solidBrush, area);
            if (selected && Focused && dropDownShowing)
            {
                //draw the focus highlight rectangle
                Rectangle focusRect = new Rectangle(area.X, area.Y, area.Width - 1, area.Height - 1);
                using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(50, SystemColors.Highlight)))
                    g.FillRectangle(solidBrush, focusRect);
                using (Pen focusPen = new Pen(SystemColors.Highlight, 1))
                    g.DrawRectangle(focusPen, focusRect);
            }

            // draw border icon
            if (borderImage != null)
            {
                Rectangle imageRect =
                    new Rectangle(area.Left + HORIZONTAL_INSET, area.Top + TOP_INSET, borderImage.Width,
                                  borderImage.Height);
                g.DrawImage(false, borderImage, imageRect);
                //g.DrawRectangle(Pens.Blue, e.Bounds);
            }
            // calculate standard text drawing metrics
            int leftMargin = HORIZONTAL_INSET + MAX_IMAGE_WIDTH + HORIZONTAL_INSET;

            // draw title line
            // post title
            int titleWidth = e.Bounds.Width - leftMargin - 1;
            Rectangle titleRectangle = new Rectangle(area.Left + leftMargin, area.Top, titleWidth, area.Height - 1);
            //g.DrawRectangle(Pens.Red,titleRectangle);
            g.DrawText(
                borderText,
                e.Font, titleRectangle,
                textColor, ellipsesStringFormat);
            // focus rectange if necessary
            e.DrawFocusRectangle();
        }

        private int CalculateItemHeight()
        {
            float fontHeight = Font.GetHeight();
            return TOP_INSET +                                  // top-margin
                Math.Max(Convert.ToInt32(fontHeight), MAX_IMAGE_HEIGHT) + // contentHeight
                BOTTOM_INSET;                                   // air at bottom
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            //Bug fix: clears out the focus highlight if the selection change is cancelled
            if (e.KeyCode == Keys.Escape)
                dropDownShowing = false;
        }

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            dropDownShowing = false;
            base.OnSelectionChangeCommitted(e);
        }

        protected override void OnDropDown(EventArgs e)
        {
            dropDownShowing = true;
            base.OnDropDown(e);
        }
        bool dropDownShowing;

        int MAX_IMAGE_HEIGHT = 25;
        int MAX_IMAGE_WIDTH = 40;

        // item metrics
        private const int TOP_INSET = 2;
        private const int BOTTOM_INSET = 2;
        private const int HORIZONTAL_INSET = 3;

        public interface IComboImageItem
        {
            Image Image { get; }
        }
    }
}
