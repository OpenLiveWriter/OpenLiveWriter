// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Shared rendering logic for gallery items, used by both the in-ribbon gallery
    /// and the dropdown panel to avoid code duplication.
    /// </summary>
    internal static class GalleryItemRenderer
    {
        /// <summary>
        /// Gets the semantic HTML style properties for a heading label.
        /// </summary>
        private static (float fontSize, bool isBold, string previewText) GetSemanticHtmlStyle(string label)
        {
            switch (label)
            {
                case "Heading 1": return (18f, true, "AaBb");
                case "Heading 2": return (15f, true, "AaBbCc");
                case "Heading 3": return (13f, true, "AaBbCcD");
                case "Heading 4": return (12f, true, "AaBbCcDd");
                case "Heading 5": return (11f, true, "AaBbCcDdE");
                case "Heading 6": return (10f, true, "AaBbCcDdEe");
                default:          return (9.5f, false, "AaBbCcDdI");
            }
        }

        /// <summary>
        /// Draws a semantic HTML style gallery item with preview text and label.
        /// Used for both in-ribbon and dropdown rendering.
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        /// <param name="bounds">The bounding rectangle for the item.</param>
        /// <param name="item">The gallery item to draw.</param>
        /// <param name="isSelected">Whether the item is currently selected.</param>
        /// <param name="isHovered">Whether the item is currently hovered.</param>
        /// <param name="horizontalAlignment">Text alignment: HorizontalCenter for in-ribbon, Left for dropdown.</param>
        public static void DrawSemanticHtmlItem(Graphics g, Rectangle bounds, RibbonGalleryItem item,
            bool isSelected, bool isHovered,
            TextFormatFlags horizontalAlignment = TextFormatFlags.Left)
        {
            var (previewFontSize, isBold, previewText) = GetSemanticHtmlStyle(item.Label);

            // Draw background with selection/hover state
            DrawItemBackground(g, bounds, isSelected, isHovered);

            // Calculate layout: preview text takes ~68% of height, label takes ~32%
            var previewHeight = (int)(bounds.Height * 0.68f);
            var labelHeight = bounds.Height - previewHeight;

            // Draw preview text with appropriate font style
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
            using (var previewFont = new Font("Calibri", previewFontSize, fontStyle))
            {
                var previewBounds = new Rectangle(bounds.X + 3, bounds.Y + 2,
                    bounds.Width - 6, previewHeight - 2);
                RibbonRenderer.DrawHighQualityText(g, previewText, previewFont,
                    Color.FromArgb(51, 51, 51), previewBounds,
                    horizontalAlignment | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine);
            }

            // Draw label below preview (smaller font, gray text)
            using (var labelFont = new Font(SystemFonts.MenuFont.FontFamily, 7.5f))
            {
                var labelBounds = new Rectangle(bounds.X + 3, bounds.Y + previewHeight,
                    bounds.Width - 6, labelHeight - 3);
                RibbonRenderer.DrawHighQualityText(g, item.Label, labelFont,
                    Color.FromArgb(102, 102, 102), labelBounds,
                    horizontalAlignment | TextFormatFlags.Top |
                    TextFormatFlags.SingleLine);
            }
        }

        /// <summary>
        /// Draws a gallery item in list style (icon on left, text on right).
        /// Used for TextPosition.Right galleries like BlogProviderButtonsGallery.
        /// </summary>
        public static void DrawListStyleItem(Graphics g, Rectangle bounds, RibbonGalleryItem item,
            bool isSelected, bool isHovered)
        {
            // Background
            Color backColor = RibbonColors.Current.GalleryItemBackground;
            Color borderColor = RibbonColors.Current.GalleryItemBorder;

            if (isSelected)
            {
                backColor = RibbonColors.Current.GalleryItemBackgroundSelected;
                borderColor = RibbonColors.Current.GalleryItemBorderSelected;
            }
            else if (isHovered)
            {
                backColor = RibbonColors.Current.GalleryItemBackgroundHover;
                borderColor = RibbonColors.Current.GalleryItemBorderHover;
            }

            if (backColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(backColor))
                    g.FillRectangle(brush, bounds);
            }

            if (borderColor != Color.Transparent)
            {
                using (var pen = new Pen(borderColor))
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }

            var x = bounds.X + 2;
            var textColor = RibbonColors.Current.ButtonText;

            // Draw icon on the left
            if (item.Image != null)
            {
                var iconY = bounds.Y + (bounds.Height - 16) / 2;
                g.DrawImage(item.Image, new Rectangle(x, iconY, 16, 16));
                x += 20; // icon width + padding
            }

            // Draw text to the right of the icon
            if (!string.IsNullOrEmpty(item.Label))
            {
                var textBounds = new Rectangle(x, bounds.Y, bounds.Width - (x - bounds.X) - 4, bounds.Height);
                RibbonRenderer.DrawHighQualityText(g, RibbonRenderer.StripAccelerator(item.Label),
                    SystemFonts.MenuFont, textColor, textBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>
        /// Draws a semantic HTML item background with selection/hover state.
        /// These use fixed Office-style colors rather than theme colors.
        /// </summary>
        private static void DrawItemBackground(Graphics g, Rectangle bounds, bool isSelected, bool isHovered)
        {
            if (isSelected)
            {
                using (var brush = new SolidBrush(Color.FromArgb(201, 222, 245)))
                    g.FillRectangle(brush, bounds);
                using (var pen = new Pen(Color.FromArgb(98, 163, 229), 1))
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
            else if (isHovered)
            {
                using (var brush = new SolidBrush(Color.FromArgb(229, 243, 255)))
                    g.FillRectangle(brush, bounds);
                using (var pen = new Pen(Color.FromArgb(168, 198, 230), 1))
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
            else
            {
                using (var brush = new SolidBrush(Color.White))
                    g.FillRectangle(brush, bounds);
                using (var pen = new Pen(Color.FromArgb(212, 212, 212), 1))
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
        }
    }
}
