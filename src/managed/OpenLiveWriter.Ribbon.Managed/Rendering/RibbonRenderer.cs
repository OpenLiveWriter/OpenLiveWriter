// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OpenLiveWriter.Ribbon.Managed.Rendering
{
    /// <summary>
    /// Renders ribbon controls with Office-style appearance.
    /// </summary>
    public class RibbonRenderer
    {
        private static RibbonRenderer _instance;

        /// <summary>
        /// Gets the singleton instance of the renderer.
        /// </summary>
        public static RibbonRenderer Instance => _instance ?? (_instance = new RibbonRenderer());

        /// <summary>
        /// Gets or sets the color scheme used for rendering.
        /// </summary>
        public RibbonColors Colors { get; set; } = RibbonColors.Current;

        /// <summary>
        /// Strips ampersand accelerator characters from text for display.
        /// </summary>
        public static string StripAccelerator(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Replace && with a placeholder, remove single &, restore &&
            return text.Replace("&&", "\x00").Replace("&", "").Replace("\x00", "&");
        }

        #region Tab Rendering

        /// <summary>
        /// Draws a ribbon tab header.
        /// </summary>
        public void DrawTabHeader(Graphics g, Rectangle bounds, string text, bool isSelected, bool isHovered,
            RibbonContextualTabGroup contextualGroup = RibbonContextualTabGroup.None)
        {
            // Background
            Color backColor;
            if (isSelected)
                backColor = Colors.TabBackgroundSelected;
            else if (isHovered)
                backColor = Colors.TabBackgroundHover;
            else
                backColor = Colors.TabBackground;

            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // Contextual tab indicator
            if (contextualGroup != RibbonContextualTabGroup.None)
            {
                var indicatorColor = Colors.GetContextualTabColor(contextualGroup);
                var indicatorBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, 3);
                using (var brush = new SolidBrush(indicatorColor))
                {
                    g.FillRectangle(brush, indicatorBounds);
                }
            }

            // Border
            if (isSelected)
            {
                using (var pen = new Pen(Colors.TabBorder))
                {
                    // Draw left, top, right borders only (bottom is open)
                    g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 1);
                    g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
                    g.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
                }
            }

            // Text
            Color textColor;
            if (isSelected)
                textColor = Colors.TabTextSelected;
            else if (isHovered)
                textColor = Colors.TabTextHover;
            else
                textColor = Colors.TabText;

            var textFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };

            using (var brush = new SolidBrush(textColor))
            {
                g.DrawString(StripAccelerator(text), SystemFonts.MenuFont, brush, bounds, textFormat);
            }
        }

        #endregion

        #region Group Rendering

        /// <summary>
        /// Draws a ribbon group background and border.
        /// </summary>
        public void DrawGroup(Graphics g, Rectangle bounds, string label)
        {
            // Group background
            using (var brush = new SolidBrush(Colors.GroupBackground))
            {
                g.FillRectangle(brush, bounds);
            }

            // Group border (right side separator)
            using (var pen = new Pen(Colors.GroupSeparator))
            {
                g.DrawLine(pen, bounds.Right - 1, bounds.Top + 4, bounds.Right - 1, bounds.Bottom - 20);
            }

            // Group label at bottom
            if (!string.IsNullOrEmpty(label))
            {
                var labelBounds = new Rectangle(bounds.X, bounds.Bottom - 18, bounds.Width, 16);
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                using (var brush = new SolidBrush(Colors.GroupLabelText))
                {
                    g.DrawString(StripAccelerator(label), font, brush, labelBounds, textFormat);
                }
            }
        }

        #endregion

        #region Button Rendering

        /// <summary>
        /// Draws a ribbon button.
        /// </summary>
        public void DrawButton(Graphics g, Rectangle bounds, string text, Image image,
            bool isEnabled, bool isHovered, bool isPressed, bool isChecked,
            RibbonButtonType buttonType, RibbonGroupSize size)
        {
            // Determine state colors
            Color backColor = Colors.ButtonBackground;
            Color borderColor = Colors.ButtonBorder;

            if (!isEnabled)
            {
                // Disabled - no special background
            }
            else if (isPressed)
            {
                backColor = Colors.ButtonBackgroundPressed;
                borderColor = Colors.ButtonBorderPressed;
            }
            else if (isChecked)
            {
                backColor = Colors.ButtonBackgroundChecked;
                borderColor = Colors.ButtonBorderChecked;
            }
            else if (isHovered)
            {
                backColor = Colors.ButtonBackgroundHover;
                borderColor = Colors.ButtonBorderHover;
            }

            // Background
            if (backColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Border
            if (borderColor != Color.Transparent)
            {
                using (var pen = new Pen(borderColor))
                {
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }

            // Content rendering depends on size
            switch (size)
            {
                case RibbonGroupSize.Large:
                    DrawLargeButtonContent(g, bounds, text, image, isEnabled);
                    break;
                case RibbonGroupSize.Medium:
                    DrawMediumButtonContent(g, bounds, text, image, isEnabled);
                    break;
                case RibbonGroupSize.Small:
                    DrawSmallButtonContent(g, bounds, image, isEnabled);
                    break;
                default:
                    DrawMediumButtonContent(g, bounds, text, image, isEnabled);
                    break;
            }

            // Draw dropdown arrow for split/dropdown buttons
            if (buttonType == RibbonButtonType.SplitButton || buttonType == RibbonButtonType.DropDownButton)
            {
                DrawDropDownArrow(g, bounds, isEnabled, size);
            }
        }

        private void DrawLargeButtonContent(Graphics g, Rectangle bounds, string text, Image image, bool isEnabled)
        {
            // Large button: 32x32 icon on top, text below
            var imageSize = 32;
            var imageBounds = new Rectangle(
                bounds.X + (bounds.Width - imageSize) / 2,
                bounds.Y + 4,
                imageSize, imageSize);

            if (image != null)
            {
                if (isEnabled)
                    g.DrawImage(image, imageBounds);
                else
                    DrawDisabledImage(g, image, imageBounds);
            }

            if (!string.IsNullOrEmpty(text))
            {
                var textBounds = new Rectangle(bounds.X + 2, bounds.Y + 38, bounds.Width - 4, bounds.Height - 40);
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                var textColor = isEnabled ? Colors.ButtonText : Colors.ButtonTextDisabled;
                using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                using (var brush = new SolidBrush(textColor))
                {
                    g.DrawString(StripAccelerator(text), font, brush, textBounds, textFormat);
                }
            }
        }

        private void DrawMediumButtonContent(Graphics g, Rectangle bounds, string text, Image image, bool isEnabled)
        {
            // Medium button: 16x16 icon on left, text on right
            var imageSize = 16;
            var padding = 4;

            if (image != null)
            {
                var imageBounds = new Rectangle(
                    bounds.X + padding,
                    bounds.Y + (bounds.Height - imageSize) / 2,
                    imageSize, imageSize);

                if (isEnabled)
                    g.DrawImage(image, imageBounds);
                else
                    DrawDisabledImage(g, image, imageBounds);
            }

            if (!string.IsNullOrEmpty(text))
            {
                var textX = bounds.X + padding + (image != null ? imageSize + padding : 0);
                var textBounds = new Rectangle(textX, bounds.Y, bounds.Width - textX - padding, bounds.Height);
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                var textColor = isEnabled ? Colors.ButtonText : Colors.ButtonTextDisabled;
                using (var brush = new SolidBrush(textColor))
                {
                    g.DrawString(StripAccelerator(text), SystemFonts.MenuFont, brush, textBounds, textFormat);
                }
            }
        }

        private void DrawSmallButtonContent(Graphics g, Rectangle bounds, Image image, bool isEnabled)
        {
            // Small button: 16x16 icon only, centered
            var imageSize = 16;

            if (image != null)
            {
                var imageBounds = new Rectangle(
                    bounds.X + (bounds.Width - imageSize) / 2,
                    bounds.Y + (bounds.Height - imageSize) / 2,
                    imageSize, imageSize);

                if (isEnabled)
                    g.DrawImage(image, imageBounds);
                else
                    DrawDisabledImage(g, image, imageBounds);
            }
        }

        private void DrawDropDownArrow(Graphics g, Rectangle bounds, bool isEnabled, RibbonGroupSize size)
        {
            var arrowSize = 5;
            int arrowX, arrowY;

            if (size == RibbonGroupSize.Large)
            {
                // Arrow at bottom center for large buttons
                arrowX = bounds.X + (bounds.Width - arrowSize) / 2;
                arrowY = bounds.Bottom - 10;
            }
            else
            {
                // Arrow at right edge for medium/small buttons
                arrowX = bounds.Right - arrowSize - 6;
                arrowY = bounds.Y + (bounds.Height - arrowSize / 2) / 2;
            }

            var arrowColor = isEnabled ? Colors.ButtonText : Colors.ButtonTextDisabled;
            using (var brush = new SolidBrush(arrowColor))
            {
                var points = new Point[]
                {
                    new Point(arrowX, arrowY),
                    new Point(arrowX + arrowSize, arrowY),
                    new Point(arrowX + arrowSize / 2, arrowY + arrowSize / 2 + 1)
                };
                g.FillPolygon(brush, points);
            }
        }

        private void DrawDisabledImage(Graphics g, Image image, Rectangle bounds)
        {
            // Draw image with reduced opacity for disabled state
            using (var attributes = new System.Drawing.Imaging.ImageAttributes())
            {
                var matrix = new System.Drawing.Imaging.ColorMatrix();
                matrix.Matrix33 = 0.4f; // 40% opacity
                matrix.Matrix00 = matrix.Matrix11 = matrix.Matrix22 = 1.0f;
                // Desaturate
                matrix.Matrix00 = matrix.Matrix01 = matrix.Matrix02 = 0.33f;
                matrix.Matrix10 = matrix.Matrix11 = matrix.Matrix12 = 0.33f;
                matrix.Matrix20 = matrix.Matrix21 = matrix.Matrix22 = 0.33f;

                attributes.SetColorMatrix(matrix);
                g.DrawImage(image, bounds, 0, 0, image.Width, image.Height,
                    GraphicsUnit.Pixel, attributes);
            }
        }

        #endregion

        #region Gallery Rendering

        /// <summary>
        /// Draws a gallery item.
        /// </summary>
        public void DrawGalleryItem(Graphics g, Rectangle bounds, string text, Image image,
            bool isSelected, bool isHovered)
        {
            // Background
            Color backColor = Colors.GalleryItemBackground;
            Color borderColor = Colors.GalleryItemBorder;

            if (isSelected)
            {
                backColor = Colors.GalleryItemBackgroundSelected;
                borderColor = Colors.GalleryItemBorderSelected;
            }
            else if (isHovered)
            {
                backColor = Colors.GalleryItemBackgroundHover;
                borderColor = Colors.GalleryItemBorderHover;
            }

            if (backColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            if (borderColor != Color.Transparent)
            {
                using (var pen = new Pen(borderColor))
                {
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }

            // Image
            if (image != null)
            {
                var imageBounds = new Rectangle(
                    bounds.X + (bounds.Width - image.Width) / 2,
                    bounds.Y + 2,
                    image.Width, image.Height);
                g.DrawImage(image, imageBounds);
            }

            // Text
            if (!string.IsNullOrEmpty(text))
            {
                var textY = image != null ? bounds.Y + image.Height + 4 : bounds.Y + 2;
                var textBounds = new Rectangle(bounds.X + 2, textY, bounds.Width - 4, bounds.Height - textY - 2);
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                using (var font = new Font(SystemFonts.MenuFont.FontFamily, 7.5f))
                using (var brush = new SolidBrush(Colors.ButtonText))
                {
                    g.DrawString(text, font, brush, textBounds, textFormat);
                }
            }
        }

        #endregion

        #region Application Menu Rendering

        /// <summary>
        /// Draws the application menu button.
        /// </summary>
        public void DrawAppMenuButton(Graphics g, Rectangle bounds, bool isHovered, bool isPressed)
        {
            // Blue rectangular button (Office 2010 style)
            var fillColor = isPressed ? Color.FromArgb(0, 82, 164) :
                           isHovered ? Color.FromArgb(41, 122, 204) :
                           Color.FromArgb(0, 102, 204);

            // Draw solid rectangle (not rounded)
            using (var brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // "File" text
            var textFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 9f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString("File", font, brush, bounds, textFormat);
            }
        }

        /// <summary>
        /// Draws an application menu item.
        /// </summary>
        public void DrawAppMenuItem(Graphics g, Rectangle bounds, string text, Image image,
            bool isHovered, bool isSeparatorBefore)
        {
            // Separator
            if (isSeparatorBefore)
            {
                using (var pen = new Pen(Color.FromArgb(73, 73, 73)))
                {
                    g.DrawLine(pen, bounds.X + 10, bounds.Y, bounds.Right - 10, bounds.Y);
                }
            }

            // Background
            if (isHovered)
            {
                using (var brush = new SolidBrush(Colors.AppMenuItemBackgroundHover))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Icon
            var textX = bounds.X + 10;
            if (image != null)
            {
                var imageBounds = new Rectangle(bounds.X + 10, bounds.Y + (bounds.Height - 32) / 2, 32, 32);
                g.DrawImage(image, imageBounds);
                textX = bounds.X + 52;
            }

            // Text
            var textBounds = new Rectangle(textX, bounds.Y, bounds.Width - textX - 10, bounds.Height);
            var textFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 10f))
            using (var brush = new SolidBrush(Colors.AppMenuItemText))
            {
                g.DrawString(text, font, brush, textBounds, textFormat);
            }
        }

        #endregion

        #region Separator Rendering

        /// <summary>
        /// Draws a vertical separator line.
        /// </summary>
        public void DrawSeparator(Graphics g, Rectangle bounds, bool isVertical)
        {
            using (var pen = new Pen(Colors.Separator))
            {
                if (isVertical)
                {
                    var x = bounds.X + bounds.Width / 2;
                    g.DrawLine(pen, x, bounds.Y + 4, x, bounds.Bottom - 4);
                }
                else
                {
                    var y = bounds.Y + bounds.Height / 2;
                    g.DrawLine(pen, bounds.X + 4, y, bounds.Right - 4, y);
                }
            }
        }

        #endregion

        #region Helper Methods

        private GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion
    }
}
