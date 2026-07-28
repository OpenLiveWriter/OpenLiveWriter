// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Ribbon color picker control.
    /// </summary>
    public class RibbonColorPicker : RibbonControlBase
    {
        private const int COLOR_CELL_SIZE = 18;
        private const int PADDING = 4;

        private RibbonColorTemplate _colorTemplate = RibbonColorTemplate.StandardColors;
        private bool _isNoColorButtonVisible;
        private bool _isAutomaticColorButtonVisible = true;
        private int _standardColorGridRows = 6;
        private int _columns = 10;

        private Color _selectedColor = Color.Black;
        private Color _automaticColor = Color.Black;
        private bool _isHovered;
        private bool _isPressed;

        private ToolStripDropDown _dropDown;
        private ColorPickerPanel _pickerPanel;
        private DropDownMouseHook _mouseHook;

        // Standard colors palette
        private static readonly Color[] StandardColors = new Color[]
        {
            // Row 1 - Theme colors (darks)
            Color.FromArgb(0, 0, 0),
            Color.FromArgb(68, 84, 106),
            Color.FromArgb(91, 155, 213),
            Color.FromArgb(237, 125, 49),
            Color.FromArgb(165, 165, 165),
            Color.FromArgb(255, 192, 0),
            Color.FromArgb(68, 114, 196),
            Color.FromArgb(112, 173, 71),
            Color.FromArgb(37, 94, 145),
            Color.FromArgb(158, 72, 14),

            // Row 2 - Theme colors (mediums)
            Color.FromArgb(242, 242, 242),
            Color.FromArgb(213, 220, 228),
            Color.FromArgb(222, 235, 247),
            Color.FromArgb(252, 228, 214),
            Color.FromArgb(237, 237, 237),
            Color.FromArgb(255, 242, 204),
            Color.FromArgb(217, 226, 243),
            Color.FromArgb(226, 240, 217),
            Color.FromArgb(189, 215, 238),
            Color.FromArgb(248, 203, 173),

            // Row 3 - Theme colors (lights)
            Color.FromArgb(217, 217, 217),
            Color.FromArgb(175, 191, 210),
            Color.FromArgb(157, 195, 230),
            Color.FromArgb(248, 161, 113),
            Color.FromArgb(192, 192, 192),
            Color.FromArgb(255, 230, 153),
            Color.FromArgb(180, 199, 231),
            Color.FromArgb(198, 224, 180),
            Color.FromArgb(142, 180, 227),
            Color.FromArgb(244, 177, 131),

            // Row 4 - Standard colors
            Color.FromArgb(192, 0, 0),
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(255, 192, 0),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(146, 208, 80),
            Color.FromArgb(0, 176, 80),
            Color.FromArgb(0, 176, 240),
            Color.FromArgb(0, 112, 192),
            Color.FromArgb(0, 32, 96),
            Color.FromArgb(112, 48, 160),

            // Row 5
            Color.FromArgb(255, 128, 128),
            Color.FromArgb(255, 128, 0),
            Color.FromArgb(255, 255, 128),
            Color.FromArgb(128, 255, 128),
            Color.FromArgb(0, 255, 128),
            Color.FromArgb(128, 255, 255),
            Color.FromArgb(0, 128, 255),
            Color.FromArgb(255, 128, 192),
            Color.FromArgb(255, 128, 255),
            Color.FromArgb(128, 0, 64),

            // Row 6
            Color.FromArgb(128, 64, 64),
            Color.FromArgb(255, 128, 64),
            Color.FromArgb(128, 128, 64),
            Color.FromArgb(0, 128, 128),
            Color.FromArgb(0, 64, 128),
            Color.FromArgb(128, 128, 192),
            Color.FromArgb(128, 0, 128),
            Color.FromArgb(128, 0, 64),
            Color.FromArgb(64, 0, 64),
            Color.FromArgb(64, 0, 128)
        };

        // Highlight colors palette
        private static readonly Color[] HighlightColors = new Color[]
        {
            Color.FromArgb(255, 255, 0),   // Yellow
            Color.FromArgb(0, 255, 0),     // Green
            Color.FromArgb(0, 255, 255),   // Cyan
            Color.FromArgb(255, 0, 255),   // Magenta
            Color.FromArgb(0, 0, 255),     // Blue
            Color.FromArgb(255, 0, 0),     // Red
            Color.FromArgb(0, 0, 128),     // Dark Blue
            Color.FromArgb(0, 128, 128),   // Teal
            Color.FromArgb(0, 128, 0),     // Dark Green
            Color.FromArgb(128, 0, 128),   // Purple
            Color.FromArgb(128, 0, 0),     // Maroon
            Color.FromArgb(128, 128, 0),   // Olive
            Color.FromArgb(128, 128, 128), // Gray
            Color.FromArgb(192, 192, 192), // Silver
            Color.FromArgb(0, 0, 0)        // Black
        };

        /// <summary>
        /// Gets or sets the color template.
        /// </summary>
        public RibbonColorTemplate ColorTemplate
        {
            get => _colorTemplate;
            set
            {
                _colorTemplate = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether the "No Color" button is visible.
        /// </summary>
        public bool IsNoColorButtonVisible
        {
            get => _isNoColorButtonVisible;
            set => _isNoColorButtonVisible = value;
        }

        /// <summary>
        /// Gets or sets whether the "Automatic" color button is visible.
        /// </summary>
        public bool IsAutomaticColorButtonVisible
        {
            get => _isAutomaticColorButtonVisible;
            set => _isAutomaticColorButtonVisible = value;
        }

        /// <summary>
        /// Gets or sets the number of rows in the standard color grid.
        /// </summary>
        public int StandardColorGridRows
        {
            get => _standardColorGridRows;
            set => _standardColorGridRows = Math.Max(1, value);
        }

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public int Columns
        {
            get => _columns;
            set => _columns = Math.Max(1, value);
        }

        /// <summary>
        /// Gets or sets the selected color.
        /// </summary>
        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    ColorChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the automatic color.
        /// </summary>
        public Color AutomaticColor
        {
            get => _automaticColor;
            set => _automaticColor = value;
        }

        /// <summary>
        /// Occurs when the selected color changes.
        /// </summary>
        public event EventHandler ColorChanged;

        public RibbonColorPicker()
        {
            Size = new Size(56, LayoutConstants.LargeButtonMinHeight);

            // A color pick from the dropdown (or color dialog) executes the command,
            // passing the picked color to the source command via the bridge.
            ColorChanged += (s, e) =>
            {
                if (CommandManager?.GetCommand(CommandId) is BridgedCommand bridgedCommand)
                {
                    bridgedCommand.SelectedColor = _selectedColor;
                }
                ExecuteCommand();
            };
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();

            switch (CurrentSize)
            {
                case RibbonGroupSize.Large:
                    Size = new Size(56, LayoutConstants.LargeButtonMinHeight);
                    break;
                case RibbonGroupSize.Medium:
                    Size = new Size(80, 22);
                    break;
                case RibbonGroupSize.Small:
                    Size = new Size(24, 24);
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var image = CurrentSize == RibbonGroupSize.Large ? CommandLargeImage : CommandSmallImage;

            // Draw as split button with color indicator
            RibbonRenderer.Instance.DrawButton(g, ClientRectangle, CommandLabel, image,
                Enabled && CommandEnabled, _isHovered, _isPressed, false,
                RibbonButtonType.SplitButton, CurrentSize);

            // Draw color indicator
            DrawColorIndicator(g);
        }

        private void DrawColorIndicator(Graphics g)
        {
            Rectangle indicatorBounds;

            if (CurrentSize == RibbonGroupSize.Large)
            {
                // At bottom of button
                indicatorBounds = new Rectangle(10, Height - 22, Width - 20, 6);
            }
            else if (CurrentSize == RibbonGroupSize.Medium)
            {
                // To the left of dropdown arrow
                indicatorBounds = new Rectangle(4, Height - 6, 16, 4);
            }
            else
            {
                // Small indicator at bottom
                indicatorBounds = new Rectangle(4, Height - 5, 16, 3);
            }

            using (var brush = new SolidBrush(_selectedColor))
            {
                g.FillRectangle(brush, indicatorBounds);
            }

            using (var pen = new Pen(Color.FromArgb(128, 0, 0, 0)))
            {
                g.DrawRectangle(pen, indicatorBounds.X, indicatorBounds.Y,
                    indicatorBounds.Width - 1, indicatorBounds.Height - 1);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ShowColorPicker();
        }

        private void ShowColorPicker()
        {
            if (_dropDown == null)
            {
                _pickerPanel = new ColorPickerPanel(this);
                _dropDown = new ToolStripDropDown
                {
                    AutoSize = false,
                    Padding = Padding.Empty
                };
                _dropDown.Items.Add(new ToolStripControlHost(_pickerPanel)
                {
                    AutoSize = false,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                });
                
                // Remove mouse hook when dropdown closes
                _dropDown.Closing += (s, e) => _mouseHook?.Remove();
            }

            _pickerPanel.UpdateLayout();
            _dropDown.Size = _pickerPanel.Size;
            _dropDown.Items[0].Size = _pickerPanel.Size;

            // Install mouse hook to detect clicks on native controls (WebView2/MSHTML)
            if (_mouseHook == null)
            {
                _mouseHook = new DropDownMouseHook(
                    this,
                    () => _dropDown,
                    () => _dropDown?.Close()
                );
            }
            _mouseHook.Install();

            _dropDown.Show(this, new Point(0, Height));
        }

        /// <summary>
        /// Gets the colors for the current template.
        /// </summary>
        internal Color[] GetColors()
        {
            switch (_colorTemplate)
            {
                case RibbonColorTemplate.HighlightColors:
                    return HighlightColors;
                default:
                    return StandardColors;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mouseHook?.Dispose();
                _dropDown?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Color picker dropdown panel.
    /// </summary>
    internal class ColorPickerPanel : UserControl
    {
        private const int CELL_SIZE = 18;
        private const int PADDING = 4;
        private const int BUTTON_HEIGHT = 24;

        private readonly RibbonColorPicker _picker;
        private int _hoveredIndex = -1;
        private bool _hoveredAutomatic;
        private bool _hoveredNoColor;
        private bool _hoveredMoreColors;

        public ColorPickerPanel(RibbonColorPicker picker)
        {
            _picker = picker;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = RibbonColors.Current.DropDownBackground;
        }

        public void UpdateLayout()
        {
            var colors = _picker.GetColors();
            var columns = _picker.Columns;
            var rows = (colors.Length + columns - 1) / columns;

            var width = columns * CELL_SIZE + PADDING * 2;
            var height = PADDING;

            if (_picker.IsAutomaticColorButtonVisible)
                height += BUTTON_HEIGHT;

            height += rows * CELL_SIZE;

            if (_picker.IsNoColorButtonVisible)
                height += BUTTON_HEIGHT;

            height += BUTTON_HEIGHT; // More Colors button
            height += PADDING;

            Size = new Size(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var colors = _picker.GetColors();
            var columns = _picker.Columns;
            var y = PADDING;

            // Automatic color button
            if (_picker.IsAutomaticColorButtonVisible)
            {
                var autoBounds = new Rectangle(PADDING, y, Width - PADDING * 2, BUTTON_HEIGHT);
                DrawColorButton(g, autoBounds, "Automatic", _picker.AutomaticColor, _hoveredAutomatic);
                y += BUTTON_HEIGHT;
            }

            // Color grid
            for (int i = 0; i < colors.Length; i++)
            {
                var col = i % columns;
                var row = i / columns;

                var cellBounds = new Rectangle(
                    PADDING + col * CELL_SIZE,
                    y + row * CELL_SIZE,
                    CELL_SIZE, CELL_SIZE);

                DrawColorCell(g, cellBounds, colors[i],
                    colors[i] == _picker.SelectedColor,
                    i == _hoveredIndex);
            }

            var rows = (colors.Length + columns - 1) / columns;
            y += rows * CELL_SIZE;

            // No Color button
            if (_picker.IsNoColorButtonVisible)
            {
                var noColorBounds = new Rectangle(PADDING, y, Width - PADDING * 2, BUTTON_HEIGHT);
                DrawColorButton(g, noColorBounds, "No Color", Color.Empty, _hoveredNoColor);
                y += BUTTON_HEIGHT;
            }

            // More Colors button
            var moreColorsBounds = new Rectangle(PADDING, y, Width - PADDING * 2, BUTTON_HEIGHT);
            DrawMoreColorsButton(g, moreColorsBounds, _hoveredMoreColors);
        }

        private void DrawColorCell(Graphics g, Rectangle bounds, Color color, bool isSelected, bool isHovered)
        {
            // Background
            if (isSelected)
            {
                using (var brush = new SolidBrush(RibbonColors.Current.GalleryItemBackgroundSelected))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
            else if (isHovered)
            {
                using (var brush = new SolidBrush(RibbonColors.Current.GalleryItemBackgroundHover))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Color square
            var colorBounds = new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
            using (var brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, colorBounds);
            }

            using (var pen = new Pen(Color.FromArgb(128, 0, 0, 0)))
            {
                g.DrawRectangle(pen, colorBounds.X, colorBounds.Y,
                    colorBounds.Width - 1, colorBounds.Height - 1);
            }

            // Border
            if (isSelected || isHovered)
            {
                var borderColor = isSelected ?
                    RibbonColors.Current.GalleryItemBorderSelected :
                    RibbonColors.Current.GalleryItemBorderHover;

                using (var pen = new Pen(borderColor))
                {
                    g.DrawRectangle(pen, bounds.X, bounds.Y,
                        bounds.Width - 1, bounds.Height - 1);
                }
            }
        }

        private void DrawColorButton(Graphics g, Rectangle bounds, string text, Color color, bool isHovered)
        {
            if (isHovered)
            {
                using (var brush = new SolidBrush(RibbonColors.Current.ButtonBackgroundHover))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Color indicator
            var colorBounds = new Rectangle(bounds.X + 4, bounds.Y + 4, 16, 16);
            if (color != Color.Empty)
            {
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, colorBounds);
                }
            }

            using (var pen = new Pen(Color.FromArgb(128, 0, 0, 0)))
            {
                g.DrawRectangle(pen, colorBounds.X, colorBounds.Y,
                    colorBounds.Width - 1, colorBounds.Height - 1);

                if (color == Color.Empty)
                {
                    // Draw X for no color
                    g.DrawLine(pen, colorBounds.Left, colorBounds.Top,
                        colorBounds.Right - 1, colorBounds.Bottom - 1);
                    g.DrawLine(pen, colorBounds.Right - 1, colorBounds.Top,
                        colorBounds.Left, colorBounds.Bottom - 1);
                }
            }

            // Text with high-quality rendering
            var textBounds = new Rectangle(bounds.X + 24, bounds.Y, bounds.Width - 28, bounds.Height);
            RibbonRenderer.DrawHighQualityText(g, text, SystemFonts.MenuFont, 
                RibbonColors.Current.ButtonText, textBounds,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private void DrawMoreColorsButton(Graphics g, Rectangle bounds, bool isHovered)
        {
            if (isHovered)
            {
                using (var brush = new SolidBrush(RibbonColors.Current.ButtonBackgroundHover))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // High-quality text rendering for "More Colors..."
            RibbonRenderer.DrawHighQualityText(g, "More Colors...", SystemFonts.MenuFont, 
                RibbonColors.Current.ButtonText, bounds,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var colors = _picker.GetColors();
            var columns = _picker.Columns;
            var y = PADDING;

            var newHoveredIndex = -1;
            var newHoveredAutomatic = false;
            var newHoveredNoColor = false;
            var newHoveredMoreColors = false;

            // Check automatic button
            if (_picker.IsAutomaticColorButtonVisible)
            {
                var autoBounds = new Rectangle(PADDING, y, Width - PADDING * 2, BUTTON_HEIGHT);
                if (autoBounds.Contains(e.Location))
                {
                    newHoveredAutomatic = true;
                }
                y += BUTTON_HEIGHT;
            }

            // Check color grid
            if (!newHoveredAutomatic)
            {
                var rows = (colors.Length + columns - 1) / columns;
                var gridBounds = new Rectangle(PADDING, y, columns * CELL_SIZE, rows * CELL_SIZE);

                if (gridBounds.Contains(e.Location))
                {
                    var col = (e.Location.X - PADDING) / CELL_SIZE;
                    var row = (e.Location.Y - y) / CELL_SIZE;
                    var index = row * columns + col;
                    if (index >= 0 && index < colors.Length)
                    {
                        newHoveredIndex = index;
                    }
                }

                y += rows * CELL_SIZE;
            }

            // Check no color button
            if (_picker.IsNoColorButtonVisible && !newHoveredAutomatic && newHoveredIndex < 0)
            {
                var noColorBounds = new Rectangle(PADDING, y, Width - PADDING * 2, BUTTON_HEIGHT);
                if (noColorBounds.Contains(e.Location))
                {
                    newHoveredNoColor = true;
                }
                y += BUTTON_HEIGHT;
            }

            // Check more colors button
            if (!newHoveredAutomatic && !newHoveredNoColor && newHoveredIndex < 0)
            {
                var moreColorsBounds = new Rectangle(PADDING, y, Width - PADDING * 2, BUTTON_HEIGHT);
                if (moreColorsBounds.Contains(e.Location))
                {
                    newHoveredMoreColors = true;
                }
            }

            if (newHoveredIndex != _hoveredIndex ||
                newHoveredAutomatic != _hoveredAutomatic ||
                newHoveredNoColor != _hoveredNoColor ||
                newHoveredMoreColors != _hoveredMoreColors)
            {
                _hoveredIndex = newHoveredIndex;
                _hoveredAutomatic = newHoveredAutomatic;
                _hoveredNoColor = newHoveredNoColor;
                _hoveredMoreColors = newHoveredMoreColors;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredIndex = -1;
            _hoveredAutomatic = false;
            _hoveredNoColor = false;
            _hoveredMoreColors = false;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left) return;

            var colors = _picker.GetColors();

            if (_hoveredAutomatic)
            {
                _picker.SelectedColor = _picker.AutomaticColor;
                CloseDropDown();
            }
            else if (_hoveredNoColor)
            {
                _picker.SelectedColor = Color.Empty;
                CloseDropDown();
            }
            else if (_hoveredMoreColors)
            {
                CloseDropDown();
                ShowColorDialog();
            }
            else if (_hoveredIndex >= 0 && _hoveredIndex < colors.Length)
            {
                _picker.SelectedColor = colors[_hoveredIndex];
                CloseDropDown();
            }
        }

        private void CloseDropDown()
        {
            var dropDown = Parent?.Parent as ToolStripDropDown;
            dropDown?.Close();
        }

        private void ShowColorDialog()
        {
            using (var dialog = new ColorDialog())
            {
                dialog.Color = _picker.SelectedColor;
                dialog.FullOpen = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _picker.SelectedColor = dialog.Color;
                }
            }
        }
    }
}
