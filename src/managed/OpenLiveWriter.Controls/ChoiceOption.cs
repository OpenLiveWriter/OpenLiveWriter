// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.CoreServices.UI;

namespace OpenLiveWriter.Controls
{
    public partial class ChoiceOption : UserControl
    {
        public readonly string Heading;
        public readonly string SubHeading;
        public Image OptionImage;
        public readonly object Id;

        private readonly int VERTICAL_PADDING = 10;
        private readonly int HORIZONTAL_PADDING = 10;
        private readonly int INSIDE_VERTICAL_PADDING = 2;

        public ChoiceOption()
        {
            InitializeComponent();
        }

        public ChoiceOption(object id, string heading, string subHeading, Image optionImage, string name)
        {
            InitializeComponent();
            Heading = heading;
            SubHeading = subHeading;
            OptionImage = optionImage;
            DoubleBuffered = true;
            Id = id;
            TabStop = true;
            AccessibleName = heading;
            AccessibleDescription = subHeading;
            Name = name;
            AccessibleRole = AccessibleRole.PushButton;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);

            if (IsHovered || (SystemInformation.HighContrast && Focused))
            {
                BorderPaint borderPaint = new BorderPaint(ResourceHelper.LoadAssemblyResourceBitmap("Images.ChoiceOptionHover.png"), true, BorderPaintMode.StretchToFill | BorderPaintMode.PaintMiddleCenter, 3, 4, 3, 71);
                borderPaint.DrawBorder(e.Graphics, ClientRectangle);
            }
            else if (Focused)
            {
                BorderPaint borderPaint = new BorderPaint(ResourceHelper.LoadAssemblyResourceBitmap("Images.ChoiceOptionFocus.png"), true, BorderPaintMode.StretchToFill | BorderPaintMode.PaintMiddleCenter, 3, 4, 3, 71);
                borderPaint.DrawBorder(e.Graphics, ClientRectangle);
            }

            // Draw everything else
            Render(g, ClientRectangle.Width, ClientRectangle.Height);
        }

        private Size subHeadingSize;
        private Size headingSize;

        private Size Render(BidiGraphics g, int width, int height)
        {
            int maxWidth = 0;
            // Start at the top left after padding
            Point origin = new Point(HORIZONTAL_PADDING, VERTICAL_PADDING);

            // Draw the green arrow
            Image arrow;
            if (IsHovered)
                arrow = ResourceHelper.LoadAssemblyResourceBitmap("Images.arrow_hover.png");
            else
                arrow = ResourceHelper.LoadAssemblyResourceBitmap("Images.arrow.png");
            if (g != null)
                g.DrawImage(true, arrow, origin.X, origin.Y + 3);
            origin.Offset(arrow.Width + HORIZONTAL_PADDING, 0);

            TextFormatFlags textFormatFlags = TextFormatFlags.NoPadding | TextFormatFlags.WordBreak;

            // Draw the Heading
            Size headingMaxSize = new Size(width - origin.X - HORIZONTAL_PADDING - 1, height - origin.Y - VERTICAL_PADDING);
            if (g == null)
            {
                headingSize = TextRenderer.MeasureText(Heading, Res.GetFont(FontSize.XLarge, FontStyle.Regular),
                                         headingMaxSize, textFormatFlags);
            }
            else
            {
                g.DrawText(Heading, Res.GetFont(FontSize.XLarge, FontStyle.Regular), new Rectangle(new Point(origin.X - 1, origin.Y), headingMaxSize), ChoiceDialog.BlueText, textFormatFlags);
            }
            origin.Offset(0, headingSize.Height + INSIDE_VERTICAL_PADDING);
            maxWidth = headingSize.Width;

            // Draw the subheading
            if (!string.IsNullOrEmpty(SubHeading))
            {
                if (g == null)
                {
                    subHeadingSize = TextRenderer.MeasureText(SubHeading, Res.DefaultFont,
                         new Size(width - origin.X - HORIZONTAL_PADDING, height - origin.Y), textFormatFlags | TextFormatFlags.NoPrefix);

                }
                else
                {
                    g.DrawText(SubHeading, Res.DefaultFont, new Rectangle(origin.X, origin.Y, width - origin.X - HORIZONTAL_PADDING, height - origin.Y), ChoiceDialog.BlueText, textFormatFlags | TextFormatFlags.NoPrefix);
                }
                maxWidth = Math.Max(subHeadingSize.Width, maxWidth);
                origin.Offset(0, subHeadingSize.Height + INSIDE_VERTICAL_PADDING);

            }

            // Draw the image
            if (OptionImage != null)
            {
                if (g != null)
                {
                    g.DrawImage(false, OptionImage, origin);
                }
                origin.Offset(0, OptionImage.Height);
                maxWidth = Math.Max(OptionImage.Width, maxWidth);
            }

            origin.Offset(maxWidth + HORIZONTAL_PADDING, VERTICAL_PADDING);
            return new Size(origin);
        }

        public void AdjustSize()
        {
            Size = Render(null, int.MaxValue, int.MaxValue);
        }

        public void AdjustSize(int maxWidth)
        {
            Size = Render(null, maxWidth, int.MaxValue);
        }

        private bool IsHovered = false;

        private void ChoiceOption_MouseEnter(object sender, EventArgs e)
        {
            IsHovered = true;
            Invalidate();
        }

        private void ChoiceOption_MouseLeave(object sender, EventArgs e)
        {
            IsHovered = false;
            Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Enter:
                case Keys.Space:
                    OnClick(EventArgs.Empty);
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }
    }
}
