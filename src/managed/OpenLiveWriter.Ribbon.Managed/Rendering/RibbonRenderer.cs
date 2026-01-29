// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Ribbon.Managed.Controls;

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

        #region High-Quality Text Rendering Helpers

        /// <summary>
        /// Configures graphics for high-quality text rendering at any DPI.
        /// Returns the previous TextRenderingHint so it can be restored.
        /// </summary>
        public static TextRenderingHint SetupHighQualityText(Graphics g)
        {
            var oldHint = g.TextRenderingHint;
            // ClearTypeGridFit provides the best quality for most text
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            return oldHint;
        }

        /// <summary>
        /// Restores the text rendering hint to a previous value.
        /// </summary>
        public static void RestoreTextRendering(Graphics g, TextRenderingHint previousHint)
        {
            g.TextRenderingHint = previousHint;
        }

        /// <summary>
        /// Draws text with high-quality rendering suitable for high-DPI displays.
        /// Uses TextRenderer for better DPI scaling than Graphics.DrawString.
        /// </summary>
        public static void DrawHighQualityText(Graphics g, string text, Font font, Color color, Rectangle bounds, 
            TextFormatFlags flags = TextFormatFlags.Default)
        {
            // TextRenderer provides better text rendering at high DPI than Graphics.DrawString
            TextRenderer.DrawText(g, text, font, bounds, color, flags);
        }

        /// <summary>
        /// Draws text with high-quality rendering using StringFormat for alignment control.
        /// When StringFormat is needed (e.g., for text wrapping), this method applies
        /// ClearTypeGridFit for best quality.
        /// </summary>
        public static void DrawHighQualityText(Graphics g, string text, Font font, Color color, Rectangle bounds,
            StringFormat format)
        {
            var oldHint = SetupHighQualityText(g);
            try
            {
                using (var brush = new SolidBrush(color))
                {
                    g.DrawString(text, font, brush, bounds, format);
                }
            }
            finally
            {
                g.TextRenderingHint = oldHint;
            }
        }

        #endregion

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

            // For selected tab, fill the entire area including bottom to blend with content
            if (isSelected)
            {
                // Fill selected tab background - extends to bottom edge to blend with content area
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, bounds);
                }

                // Draw borders - left, top, right only (no bottom border for seamless blending)
                // Note: Bottom border is drawn by RibbonPanel in segments that skip selected tab
                using (var pen = new Pen(Colors.TabBorder))
                {
                // Left border - DPI-scaled corner radius
                var cornerRadius = DisplayHelper.ScaleYCeil(2);
                g.DrawLine(pen, bounds.Left, bounds.Top + cornerRadius, bounds.Left, bounds.Bottom);
                // Top border with slight rounded corners - DPI-scaled
                g.DrawLine(pen, bounds.Left + cornerRadius, bounds.Top, bounds.Right - cornerRadius - 1, bounds.Top);
                // Small corner connectors
                g.DrawLine(pen, bounds.Left, bounds.Top + cornerRadius, bounds.Left + cornerRadius, bounds.Top);
                g.DrawLine(pen, bounds.Right - cornerRadius - 1, bounds.Top, bounds.Right - 1, bounds.Top + cornerRadius);
                // Right border
                g.DrawLine(pen, bounds.Right - 1, bounds.Top + cornerRadius, bounds.Right - 1, bounds.Bottom);
                }
            }
            else
            {
                // Non-selected tab - just fill background
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, bounds.X, bounds.Y, bounds.Width, bounds.Height - 1);
                }
            }

            // Note: Contextual tab indicator (colored bar at top) is drawn at the group level
            // by RibbonPanel.TabHeaderPanel_Paint to avoid double-rendering

            // Hover border for non-selected tabs - DPI-scaled
            if (isHovered && !isSelected)
            {
                var cornerRadius = DisplayHelper.ScaleYCeil(2);
                var hoverPadding = DisplayHelper.ScaleYCeil(2);
                using (var pen = new Pen(Color.FromArgb(180, Colors.TabBorder)))
                {
                    // Subtle border on hover
                    g.DrawLine(pen, bounds.Left, bounds.Top + cornerRadius, bounds.Left, bounds.Bottom - hoverPadding);
                    g.DrawLine(pen, bounds.Left + cornerRadius, bounds.Top, bounds.Right - cornerRadius - 1, bounds.Top);
                    g.DrawLine(pen, bounds.Right - 1, bounds.Top + cornerRadius, bounds.Right - 1, bounds.Bottom - hoverPadding);
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

            // Text bounds - vertically centered in tab area - DPI-scaled padding
            var textPadding = DisplayHelper.ScaleYCeil(1);
            var textBounds = new Rectangle(bounds.X, bounds.Y + textPadding, bounds.Width, bounds.Height - textPadding * 2);
            
            // Use high-quality text rendering for crisp text at any DPI
            DrawHighQualityText(g, StripAccelerator(text), SystemFonts.MenuFont, textColor, textBounds,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | 
                TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        }

        #endregion

        #region Group Rendering

        /// <summary>
        /// Draws a ribbon group background and border.
        /// </summary>
        public void DrawGroup(Graphics g, Rectangle bounds, string label)
        {
            // Use layout constant for label height (DPI-scaled)
            int labelHeight = LayoutConstants.GroupLabelHeight;

            // Group background - subtle gradient for Office-like appearance
            if (Colors.GroupBackground.A == 255)
            {
                using (var brush = new SolidBrush(Colors.GroupBackground))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
            else
            {
                // Draw subtle gradient background (lighter at top, slightly darker at bottom)
                var gradientTop = RibbonColors.DefaultOpaqueGroupBackground;
                var gradientBottom = Color.FromArgb(245, 246, 248);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    bounds, gradientTop, gradientBottom, 
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Calculate where the label area starts (bottom portion for label)
            var labelAreaTop = bounds.Bottom - labelHeight;
            var contentAreaBottom = labelAreaTop;

            // Draw vertical separator on right side (Office-style double-line effect)
            // The separator runs from top to just above the label area - DPI-scaled
            var separatorX = bounds.Right - 1;
            var separatorPadding = DisplayHelper.ScaleYCeil(2);
            var separatorTop = bounds.Top + separatorPadding;
            var separatorBottom = bounds.Bottom - separatorPadding;

            // Draw highlight line (lighter, left side of separator for 3D effect)
            using (var highlightPen = new Pen(Color.FromArgb(255, 255, 255)))
            {
                g.DrawLine(highlightPen, separatorX - 1, separatorTop, separatorX - 1, separatorBottom);
            }

            // Draw shadow line (darker, main separator line)
            using (var shadowPen = new Pen(Colors.GroupSeparator))
            {
                g.DrawLine(shadowPen, separatorX, separatorTop, separatorX, separatorBottom);
            }

            // Draw label background area (subtle but visible background) - DPI-scaled
            var labelBgMargin = DisplayHelper.ScaleXCeil(2);
            var labelBgBounds = new Rectangle(bounds.X, labelAreaTop, bounds.Width - labelBgMargin, bounds.Bottom - labelAreaTop);
            using (var labelBgBrush = new SolidBrush(Colors.GroupLabelBackground))
            {
                g.FillRectangle(labelBgBrush, labelBgBounds);
            }

            // Draw top border of label area (subtle separator between content and label) - DPI-scaled
            var labelBorderMargin = DisplayHelper.ScaleXCeil(3);
            using (var pen = new Pen(Colors.GroupLabelBorder))
            {
                g.DrawLine(pen, bounds.X, labelAreaTop, bounds.Right - labelBorderMargin, labelAreaTop);
            }

            // Group label at bottom - centered text
            if (!string.IsNullOrEmpty(label))
            {
                // Label bounds with proper padding to avoid clipping
                // Account for separator on right (2px) and padding on both sides - DPI-scaled
                var leftPadding = DisplayHelper.ScaleXCeil(4);
                var rightPadding = DisplayHelper.ScaleXCeil(4); // Space before separator
                var topPadding = DisplayHelper.ScaleYCeil(2);
                var bottomPadding = DisplayHelper.ScaleYCeil(2);
                
                var labelTextBounds = new Rectangle(
                    bounds.X + leftPadding, 
                    labelAreaTop + topPadding, 
                    bounds.Width - leftPadding - rightPadding, 
                    labelHeight - topPadding - bottomPadding);
                
                // Use smaller font size (7.5pt) to match native ribbon and prevent truncation
                // Native ribbon uses approximately 8pt but appears smaller due to rendering
                // Font size scales with DPI automatically via Graphics.DpiY
                var baseFontSize = 7.5f;
                var scaledFontSize = baseFontSize * (g.DpiY / 96f);
                using (var font = new Font(SystemFonts.MenuFont.FontFamily, scaledFontSize))
                {
                    DrawHighQualityText(g, StripAccelerator(label), font, Colors.GroupLabelText, labelTextBounds,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | 
                        TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
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

            // Background - ALWAYS fill to prevent ghost images from parent content
            // When backColor is Transparent, use the group background color instead
            var fillColor = backColor == Color.Transparent
                ? Colors.GetOpaqueGroupBackground()
                : backColor;
            using (var brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, bounds);
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
            // Large button: 32x32 icon centered at top, text centered below (may wrap to 2 lines)
            // Uses LayoutConstants for consistent sizing
            var imageSize = LayoutConstants.LargeImageSize;  // 32
            var topPadding = LayoutConstants.LargeButtonIconTopPadding;  // 3
            var iconTextGap = LayoutConstants.LargeButtonIconTextGap;  // 2
            
            var imageBounds = new Rectangle(
                bounds.X + (bounds.Width - imageSize) / 2,
                bounds.Y + topPadding,
                imageSize, imageSize);

            if (image != null)
            {
                if (isEnabled)
                    DrawScaledImage(g, image, imageBounds);
                else
                    DrawDisabledImage(g, image, imageBounds);
            }

            if (!string.IsNullOrEmpty(text))
            {
                // Text starts below icon with small gap, allowing for 2 lines of text
                var textTop = bounds.Y + topPadding + imageSize + iconTextGap;
                // Reserve space at bottom for potential dropdown arrow (8px) + small padding - DPI-scaled
                var bottomReserve = DisplayHelper.ScaleYCeil(4); // Small padding below text
                var textHeight = bounds.Height - (topPadding + imageSize + iconTextGap) - bottomReserve;
                
                // Ensure minimum text height for at least 1 line
                textHeight = Math.Max(textHeight, LayoutConstants.LargeButtonTextLineHeight);
                
                var textBounds = new Rectangle(bounds.X + DisplayHelper.ScaleXCeil(1), textTop, 
                    bounds.Width - DisplayHelper.ScaleXCeil(2), textHeight);

                // Allow text to wrap to 2 lines if needed, centered horizontally
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                    // No NoWrap flag - allow text to wrap
                };

                var textColor = isEnabled ? Colors.ButtonText : Colors.ButtonTextDisabled;
                // Font size scales with DPI automatically via Graphics.DpiY
                var baseFontSize = 8f;
                var scaledFontSize = baseFontSize * (g.DpiY / 96f);
                using (var font = new Font(SystemFonts.MenuFont.FontFamily, scaledFontSize))
                {
                    // Strip accelerator characters before drawing with high-quality rendering
                    var displayText = StripAccelerator(text);
                    DrawHighQualityText(g, displayText, font, textColor, textBounds, textFormat);
                }
            }
        }

        private void DrawMediumButtonContent(Graphics g, Rectangle bounds, string text, Image image, bool isEnabled)
        {
            // Medium button: 16x16 icon on left, text vertically centered on right - DPI-scaled
            var imageSize = LayoutConstants.SmallImageSize;
            var leftPadding = LayoutConstants.GroupPadding;
            var iconTextGap = LayoutConstants.GroupPadding;
            var rightPadding = LayoutConstants.GroupPadding;

            if (image != null)
            {
                var imageBounds = new Rectangle(
                    bounds.X + leftPadding,
                    bounds.Y + (bounds.Height - imageSize) / 2,
                    imageSize, imageSize);

                if (isEnabled)
                    DrawScaledImage(g, image, imageBounds);
                else
                    DrawDisabledImage(g, image, imageBounds);
            }

            if (!string.IsNullOrEmpty(text))
            {
                var textX = bounds.X + leftPadding + (image != null ? imageSize + iconTextGap : 0);
                var textWidth = bounds.Right - textX - rightPadding;
                var textBounds = new Rectangle(textX, bounds.Y, textWidth, bounds.Height);

                var textColor = isEnabled ? Colors.ButtonText : Colors.ButtonTextDisabled;
                // Use high-quality text rendering for crisp text at any DPI
                DrawHighQualityText(g, StripAccelerator(text), SystemFonts.MenuFont, textColor, textBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | 
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }
        }

        private void DrawSmallButtonContent(Graphics g, Rectangle bounds, Image image, bool isEnabled)
        {
            // Small button: 16x16 icon only, centered - DPI-scaled
            var imageSize = LayoutConstants.SmallImageSize;
            var imageBounds = new Rectangle(
                bounds.X + (bounds.Width - imageSize) / 2,
                bounds.Y + (bounds.Height - imageSize) / 2,
                imageSize, imageSize);

            if (image != null)
            {
                if (isEnabled)
                    DrawScaledImage(g, image, imageBounds);
                else
                    DrawDisabledImage(g, image, imageBounds);
            }
            // If no image is available, don't draw anything (button will show as blank)
            // This is preferable to drawing placeholder rectangles which create visual noise
        }

        /// <summary>
        /// Draws an image scaled to the specified bounds with high quality interpolation.
        /// For best results, the source image should match the target size (32x32 for large, 16x16 for small).
        /// </summary>
        private void DrawScaledImage(Graphics g, Image image, Rectangle bounds)
        {
            if (image == null) return;

            // If image is already the correct size, draw directly for best performance and sharpness
            if (image.Width == bounds.Width && image.Height == bounds.Height)
            {
                g.DrawImage(image, bounds.Location);
                return;
            }

            // Save original graphics settings
            var oldInterpolationMode = g.InterpolationMode;
            var oldPixelOffsetMode = g.PixelOffsetMode;
            var oldSmoothingMode = g.SmoothingMode;
            var oldCompositingQuality = g.CompositingQuality;
            var oldCompositingMode = g.CompositingMode;

            try
            {
                // Use highest quality settings for scaling icons
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                // Use ImageAttributes for best alpha handling
                using (var attributes = new System.Drawing.Imaging.ImageAttributes())
                {
                    // Prevent edge artifacts when scaling
                    attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    
                    g.DrawImage(image, bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            finally
            {
                // Restore original settings
                g.InterpolationMode = oldInterpolationMode;
                g.PixelOffsetMode = oldPixelOffsetMode;
                g.SmoothingMode = oldSmoothingMode;
                g.CompositingQuality = oldCompositingQuality;
                g.CompositingMode = oldCompositingMode;
            }
        }

        private void DrawDropDownArrow(Graphics g, Rectangle bounds, bool isEnabled, RibbonGroupSize size)
        {
            // Arrow size scales with DPI
            var arrowSize = DisplayHelper.ScaleXCeil(5);
            int arrowX, arrowY;

            if (size == RibbonGroupSize.Large)
            {
                // Arrow at bottom center for large buttons, positioned below the 2-line text area - DPI-scaled
                arrowX = bounds.X + (bounds.Width - arrowSize) / 2;
                arrowY = bounds.Bottom - DisplayHelper.ScaleYCeil(8);
            }
            else
            {
                // Arrow at right edge for medium/small buttons, vertically centered - DPI-scaled
                arrowX = bounds.Right - arrowSize - LayoutConstants.GroupPadding;
                arrowY = bounds.Y + (bounds.Height - arrowSize) / 2;
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

            // Image - DPI-scaled padding
            if (image != null)
            {
                var imagePadding = DisplayHelper.ScaleYCeil(2);
                var imageBounds = new Rectangle(
                    bounds.X + (bounds.Width - image.Width) / 2,
                    bounds.Y + imagePadding,
                    image.Width, image.Height);
                g.DrawImage(image, imageBounds);
            }

            // Text - DPI-scaled padding and font
            if (!string.IsNullOrEmpty(text))
            {
                var textPadding = DisplayHelper.ScaleYCeil(2);
                var textGap = DisplayHelper.ScaleYCeil(4);
                var textY = image != null ? bounds.Y + image.Height + textGap : bounds.Y + textPadding;
                var horizontalPadding = DisplayHelper.ScaleXCeil(2);
                var textBounds = new Rectangle(bounds.X + horizontalPadding, textY, 
                    bounds.Width - horizontalPadding * 2, bounds.Height - textY - textPadding);
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                // Font size scales with DPI automatically via Graphics.DpiY
                var baseFontSize = 7.5f;
                var scaledFontSize = baseFontSize * (g.DpiY / 96f);
                using (var font = new Font(SystemFonts.MenuFont.FontFamily, scaledFontSize))
                {
                    DrawHighQualityText(g, text, font, Colors.ButtonText, textBounds, textFormat);
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
            // Blue rectangular button (Office 2010/2013 style)
            // These colors match the authentic Office "File" button
            // Normal: Blue (#2672BF), Hover: Lighter blue (#2B78C5), Pressed: Darker blue (#12559A)
            Color fillColor;
            Color borderColor;

            if (isPressed)
            {
                fillColor = Color.FromArgb(18, 85, 154);      // Dark blue when pressed
                borderColor = Color.FromArgb(14, 68, 123);    // Darker border
            }
            else if (isHovered)
            {
                fillColor = Color.FromArgb(43, 120, 197);     // Lighter blue on hover
                borderColor = Color.FromArgb(38, 100, 165);   // Slightly darker border
            }
            else
            {
                fillColor = Color.FromArgb(38, 114, 191);     // Standard blue
                borderColor = Color.FromArgb(30, 92, 154);    // Darker border for depth
            }

            // Draw solid rectangle with slight border for depth
            using (var brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // Draw border
            using (var pen = new Pen(borderColor))
            {
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }

            // Optional: Draw subtle inner highlight at top for 3D effect - DPI-scaled
            using (var highlightPen = new Pen(Color.FromArgb(50, 255, 255, 255)))
            {
                var highlightPadding = DisplayHelper.ScaleXCeil(1);
                g.DrawLine(highlightPen, bounds.X + highlightPadding, bounds.Y + highlightPadding, 
                    bounds.Right - highlightPadding * 2, bounds.Y + highlightPadding);
            }

            // "File" text with high-quality rendering - DPI-scaled font
            var baseFontSize = 9.5f;
            var scaledFontSize = baseFontSize * (g.DpiY / 96f);
            using (var font = new Font(SystemFonts.MenuFont.FontFamily, scaledFontSize, FontStyle.Bold))
            {
                DrawHighQualityText(g, "File", font, Color.White, bounds,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>
        /// Draws an application menu item.
        /// </summary>
        public void DrawAppMenuItem(Graphics g, Rectangle bounds, string text, Image image,
            bool isHovered, bool isSeparatorBefore)
        {
            // Separator - light gray line for light theme
            if (isSeparatorBefore)
            {
                var separatorPadding = DisplayHelper.ScaleXCeil(10);
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawLine(pen, bounds.X + separatorPadding, bounds.Y, bounds.Right - separatorPadding, bounds.Y);
                }
            }

            // Background
            if (isHovered)
            {
                using (var brush = new SolidBrush(Colors.AppMenuItemBackgroundHover))
                {
                    g.FillRectangle(brush, bounds);
                }
                // Draw border around hovered item
                using (var pen = new Pen(Color.FromArgb(152, 193, 235)))
                {
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }

            // Icon - draw at native 32x32 size for crisp rendering
            var itemPadding = DisplayHelper.ScaleXCeil(10);
            var textX = bounds.X + itemPadding;
            if (image != null)
            {
                const int iconSize = 32;
                var iconX = bounds.X + itemPadding;
                var iconY = bounds.Y + (bounds.Height - iconSize) / 2;
                
                // Draw icon at native size without scaling
                g.DrawImageUnscaled(image, iconX, iconY);
                
                textX = bounds.X + itemPadding + iconSize + DisplayHelper.ScaleXCeil(10);
            }

            // Text with high-quality rendering - DPI-scaled font and padding
            var textBounds = new Rectangle(textX, bounds.Y, bounds.Width - textX - itemPadding, bounds.Height);
            var baseFontSize = 10f;
            var scaledFontSize = baseFontSize * (g.DpiY / 96f);
            using (var font = new Font(SystemFonts.MenuFont.FontFamily, scaledFontSize))
            {
                DrawHighQualityText(g, text, font, Colors.AppMenuItemText, textBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
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
                var separatorPadding = DisplayHelper.ScaleXCeil(4);
                if (isVertical)
                {
                    var x = bounds.X + bounds.Width / 2;
                    g.DrawLine(pen, x, bounds.Y + separatorPadding, x, bounds.Bottom - separatorPadding);
                }
                else
                {
                    var y = bounds.Y + bounds.Height / 2;
                    g.DrawLine(pen, bounds.X + separatorPadding, y, bounds.Right - separatorPadding, y);
                }
            }
        }

        #endregion

    }
}
