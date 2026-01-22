// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Ribbon gallery control for displaying selectable items.
    /// </summary>
    public class RibbonGallery : RibbonControlBase
    {
        private const int SCROLL_BUTTON_WIDTH = 16;
        private const int BORDER_WIDTH = 1;

        private RibbonGalleryType _galleryType = RibbonGalleryType.DropDown;
        private RibbonTextPosition _textPosition = RibbonTextPosition.Bottom;
        private int _itemHeight = 32;
        private int _itemWidth = 32;
        private int _columns = 5;
        private int _maxColumns = 7;
        private int _maxRows = 3;
        private RibbonGalleryLayout _layout = RibbonGalleryLayout.Flow;

        private readonly List<RibbonGalleryItem> _items = new List<RibbonGalleryItem>();
        private int _selectedIndex = -1;
        private int _hoveredIndex = -1;
        private int _scrollOffset = 0;
#pragma warning disable CS0414 // Field is assigned but never used
        private bool _isExpanded;
#pragma warning restore CS0414

        private Rectangle _contentBounds;
        private Rectangle _upScrollBounds;
        private Rectangle _downScrollBounds;
        private Rectangle _expandBounds;

        private ToolStripDropDown _dropDown;
        private RibbonGalleryDropDownPanel _dropDownPanel;

        /// <summary>
        /// Gets or sets the gallery type.
        /// </summary>
        public RibbonGalleryType GalleryType
        {
            get => _galleryType;
            set
            {
                _galleryType = value;
                UpdateSize();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text position for items.
        /// </summary>
        public RibbonTextPosition TextPosition
        {
            get => _textPosition;
            set => _textPosition = value;
        }

        /// <summary>
        /// Gets or sets the item height in pixels.
        /// </summary>
        public int ItemHeight
        {
            get => _itemHeight;
            set
            {
                _itemHeight = value;
                UpdateSize();
            }
        }

        /// <summary>
        /// Gets or sets the item width in pixels.
        /// </summary>
        public int ItemWidth
        {
            get => _itemWidth;
            set
            {
                _itemWidth = value;
                UpdateSize();
            }
        }

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public int Columns
        {
            get => _columns;
            set
            {
                _columns = Math.Max(1, value);
                UpdateSize();
            }
        }

        /// <summary>
        /// Gets or sets the maximum columns for expanded view.
        /// </summary>
        public int MaxColumns
        {
            get => _maxColumns;
            set => _maxColumns = Math.Max(1, value);
        }

        /// <summary>
        /// Gets or sets the maximum rows.
        /// </summary>
        public int MaxRows
        {
            get => _maxRows;
            set
            {
                _maxRows = Math.Max(1, value);
                UpdateSize();
            }
        }

        /// <summary>
        /// Gets or sets the gallery layout.
        /// </summary>
        public new RibbonGalleryLayout Layout
        {
            get => _layout;
            set => _layout = value;
        }

        /// <summary>
        /// Gets the gallery items.
        /// </summary>
        public List<RibbonGalleryItem> Items => _items;

        /// <summary>
        /// Gets or sets the selected index.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value && value >= -1 && value < _items.Count)
                {
                    _selectedIndex = value;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets the selected item.
        /// </summary>
        public RibbonGalleryItem SelectedItem =>
            _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

        /// <summary>
        /// Occurs when the selected index changes.
        /// </summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>
        /// Occurs when an item is clicked.
        /// </summary>
        public event EventHandler<GalleryItemClickEventArgs> ItemClick;

        public RibbonGallery()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            UpdateSize();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Ensure items are populated when the control is ready
            EnsureItemsPopulated();
        }

        /// <summary>
        /// Called when the command is updated.
        /// </summary>
        protected override void UpdateFromCommand()
        {
            base.UpdateFromCommand();
            LoadItemsFromCommand();
        }

        private void EnsureItemsPopulated()
        {
            // Only for SemanticHtmlGallery - ensure items are populated before first paint
            if (_items.Count == 0 && CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
            {
                EnsureSemanticHtmlItems();
                Invalidate();
            }
        }

        private void LoadItemsFromCommand()
        {
            var command = CommandManager?.GetCommand(CommandId);
            if (command is IGalleryCommand galleryCommand)
            {
                // Subscribe to items changed if not already
                galleryCommand.ItemsChanged -= OnGalleryItemsChanged;
                galleryCommand.ItemsChanged += OnGalleryItemsChanged;

                // Load items
                _items.Clear();
                foreach (var item in galleryCommand.GalleryItems)
                {
                    _items.Add(new RibbonGalleryItem(item.Label, item.Image) { Tag = item.Tag });
                }
                _selectedIndex = galleryCommand.SelectedIndex;
                Invalidate();
            }
            else
            {
                // If no gallery command, check for semantic HTML gallery
                EnsureItemsPopulated();
            }
        }

        private void OnGalleryItemsChanged(object sender, EventArgs e)
        {
            LoadItemsFromCommand();
        }

        /// <summary>
        /// Gets the preferred width for this gallery.
        /// </summary>
        public int GetPreferredWidth()
        {
            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Use configured columns, not maxColumns
                return _columns * _itemWidth + SCROLL_BUTTON_WIDTH + BORDER_WIDTH * 2;
            }
            else
            {
                // Dropdown galleries use button sizing
                return CurrentSize == RibbonGroupSize.Large ? 56 : 
                       CurrentSize == RibbonGroupSize.Medium ? 80 : 24;
            }
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Calculate preferred size for in-ribbon gallery
                var width = GetPreferredWidth();
                System.Diagnostics.Debug.WriteLine($"RibbonGallery.UpdateSize: CommandId={CommandId}, Columns={_columns}, Width={Width}, PreferredWidth={width}");
                
                // Don't clamp height - let parent layout control it
                // Only set minimum height to show at least 1 row
                var minHeight = _itemHeight + BORDER_WIDTH * 2;
                if (Height < minHeight)
                {
                    Height = minHeight;
                }
                // Always respect the configured width for in-ribbon galleries
                Width = width;
            }
            else if (_galleryType == RibbonGalleryType.CompactDropDown)
            {
                // Compact dropdown - used for blog selector
                // Use configured ItemWidth or default minimum
                var dropdownWidth = Math.Max(140, _itemWidth);
                Size = new Size(dropdownWidth, 24);
            }
            else
            {
                // Standard dropdown gallery uses button size
                switch (CurrentSize)
                {
                    case RibbonGroupSize.Large:
                        Size = new Size(56, 66);
                        break;
                    case RibbonGroupSize.Medium:
                        Size = new Size(80, 22);
                        break;
                    case RibbonGroupSize.Small:
                        Size = new Size(24, 24);
                        break;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                DrawInRibbonGallery(g);
            }
            else
            {
                DrawDropDownButton(g);
            }
        }

        private void DrawInRibbonGallery(Graphics g)
        {
            // Ensure items are populated before drawing
            if (_items.Count == 0 && CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
            {
                EnsureSemanticHtmlItems();
            }

            // Background
            using (var brush = new SolidBrush(RibbonColors.Current.GalleryBackground))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Border
            using (var pen = new Pen(RibbonColors.Current.GalleryBorder))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // Calculate bounds - always calculate these for mouse hit testing
            CalculateBounds();

            // Clip to content area
            g.SetClip(_contentBounds);

            // If no items, draw default style preview
            if (_items.Count == 0)
            {
                DrawDefaultStylePreview(g, _contentBounds);
            }
            else
            {
                // Draw visible items - limit to configured columns
                var visibleColumns = Math.Min(_columns, Math.Max(1, _contentBounds.Width / _itemWidth));
                var visibleRows = Math.Max(1, _contentBounds.Height / _itemHeight);

                for (int row = 0; row < visibleRows; row++)
                {
                    for (int col = 0; col < visibleColumns; col++)
                    {
                        var index = (_scrollOffset + row) * visibleColumns + col;
                        if (index >= _items.Count) break;

                        var itemBounds = new Rectangle(
                            _contentBounds.X + col * _itemWidth,
                            _contentBounds.Y + row * _itemHeight,
                            _itemWidth, _itemHeight);

                        DrawGalleryItem(g, itemBounds, _items[index], index == _selectedIndex, index == _hoveredIndex);
                    }
                }
            }

            g.ResetClip();

            // Draw scroll buttons
            DrawScrollButton(g, _upScrollBounds, true, _scrollOffset > 0);
            DrawScrollButton(g, _downScrollBounds, false, CanScrollDown());
            DrawExpandButton(g, _expandBounds);
        }

        private void CalculateBounds()
        {
            var contentWidth = Math.Max(1, Width - SCROLL_BUTTON_WIDTH - BORDER_WIDTH * 2);
            var contentHeight = Math.Max(1, Height - BORDER_WIDTH * 2);
            _contentBounds = new Rectangle(BORDER_WIDTH, BORDER_WIDTH, contentWidth, contentHeight);

            var scrollX = Width - SCROLL_BUTTON_WIDTH - BORDER_WIDTH;
            var scrollButtonHeight = Math.Max(1, contentHeight / 3);
            _upScrollBounds = new Rectangle(scrollX, BORDER_WIDTH, SCROLL_BUTTON_WIDTH, scrollButtonHeight);
            _downScrollBounds = new Rectangle(scrollX, BORDER_WIDTH + scrollButtonHeight, SCROLL_BUTTON_WIDTH, scrollButtonHeight);
            _expandBounds = new Rectangle(scrollX, BORDER_WIDTH + scrollButtonHeight * 2, SCROLL_BUTTON_WIDTH, scrollButtonHeight);
            
            var visibleCols = Math.Min(_columns, Math.Max(1, contentWidth / _itemWidth));
            System.Diagnostics.Debug.WriteLine($"RibbonGallery.CalculateBounds: CommandId={CommandId}, Width={Width}, Columns={_columns}, ItemWidth={_itemWidth}, ContentWidth={contentWidth}, VisibleCols={visibleCols}, Items={_items.Count}");
        }

        private void DrawDefaultStylePreview(Graphics g, Rectangle bounds)
        {
            // Check if this is the SemanticHtmlGallery - if so, ensure items are populated
            if (CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
            {
                EnsureSemanticHtmlItems();
            }

            // If we have items now, don't draw default preview - items will be drawn normally
            if (_items.Count > 0) return;

            // Draw a default style preview like the original ribbon
            var previewText = "AaBbCcDdI";
            var labelText = "Paragraph";

            // Preview text (larger font)
            using (var previewFont = new Font("Calibri", 12f))
            using (var textBrush = new SolidBrush(Color.FromArgb(68, 68, 68)))
            {
                var previewBounds = new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height / 2);
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                g.DrawString(previewText, previewFont, textBrush, previewBounds, format);
            }

            // Label text (smaller)
            using (var labelFont = new Font(SystemFonts.MenuFont.FontFamily, 7.5f))
            using (var labelBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                var labelBounds = new Rectangle(bounds.X + 4, bounds.Y + bounds.Height / 2 - 2, bounds.Width - 8, bounds.Height / 2);
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                g.DrawString(labelText, labelFont, labelBrush, labelBounds, format);
            }
        }

        private void EnsureSemanticHtmlItems()
        {
            if (_items.Count > 0) return;

            // Add the semantic HTML style items with their corresponding command IDs
            _items.Add(new RibbonGalleryItem("Paragraph") { Tag = OpenLiveWriter.Localization.CommandId.ApplySemanticParagraph });
            _items.Add(new RibbonGalleryItem("Heading 1") { Tag = OpenLiveWriter.Localization.CommandId.ApplySemanticHeader1 });
            _items.Add(new RibbonGalleryItem("Heading 2") { Tag = OpenLiveWriter.Localization.CommandId.ApplySemanticHeader2 });
            _items.Add(new RibbonGalleryItem("Heading 3") { Tag = OpenLiveWriter.Localization.CommandId.ApplySemanticHeader3 });
            _items.Add(new RibbonGalleryItem("Heading 4") { Tag = OpenLiveWriter.Localization.CommandId.ApplySemanticHeader4 });
            _items.Add(new RibbonGalleryItem("Heading 5") { Tag = OpenLiveWriter.Localization.CommandId.ApplySemanticHeader5 });
            _items.Add(new RibbonGalleryItem("Heading 6") { Tag = OpenLiveWriter.Localization.CommandId.ApplySemanticHeader6 });
        }

        private void DrawDropDownButton(Graphics g)
        {
            // For compact dropdown galleries (like blog selector), draw as a combobox-style control
            if (_galleryType == RibbonGalleryType.CompactDropDown)
            {
                DrawCompactDropDown(g);
                return;
            }

            var image = CommandLargeImage ?? (CurrentSize == RibbonGroupSize.Large ? CommandLargeImage : CommandSmallImage);

            RibbonRenderer.Instance.DrawButton(g, ClientRectangle, CommandLabel, image,
                Enabled && CommandEnabled, _hoveredIndex >= 0, false, false,
                RibbonButtonType.DropDownButton, CurrentSize);
        }

        private void DrawCompactDropDown(Graphics g)
        {
            // Draw background
            var bgColor = _hoveredIndex >= 0 ? RibbonColors.Current.ButtonBackgroundHover : Color.White;
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Draw border
            using (var pen = new Pen(Color.FromArgb(171, 171, 171)))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // Get the selected item info (for blog selector, show current blog)
            Image itemImage = null;
            string itemText = CommandLabel;

            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
            {
                var selectedItem = _items[_selectedIndex];
                itemImage = selectedItem.Image;
                itemText = selectedItem.Label ?? CommandLabel;
            }
            else
            {
                // Try to get image from command
                itemImage = CommandSmallImage;
            }

            var x = 4;
            var textColor = Enabled && CommandEnabled ? Color.FromArgb(68, 68, 68) : Color.Gray;

            // Draw icon
            if (itemImage != null)
            {
                var iconBounds = new Rectangle(x, (Height - 16) / 2, 16, 16);
                if (Enabled && CommandEnabled)
                {
                    g.DrawImage(itemImage, iconBounds);
                }
                else
                {
                    using (var attributes = new System.Drawing.Imaging.ImageAttributes())
                    {
                        var matrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.5f };
                        attributes.SetColorMatrix(matrix);
                        g.DrawImage(itemImage, iconBounds, 0, 0, itemImage.Width, itemImage.Height,
                            GraphicsUnit.Pixel, attributes);
                    }
                }
                x += 20;
            }

            // Draw text
            if (!string.IsNullOrEmpty(itemText))
            {
                var textBounds = new Rectangle(x, 0, Width - x - 18, Height);
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                using (var brush = new SolidBrush(textColor))
                {
                    g.DrawString(RibbonRenderer.StripAccelerator(itemText), SystemFonts.MenuFont, brush, textBounds, format);
                }
            }

            // Draw dropdown arrow
            var arrowX = Width - 14;
            var arrowY = (Height - 4) / 2;
            using (var brush = new SolidBrush(textColor))
            {
                var points = new Point[]
                {
                    new Point(arrowX, arrowY),
                    new Point(arrowX + 8, arrowY),
                    new Point(arrowX + 4, arrowY + 4)
                };
                g.FillPolygon(brush, points);
            }
        }

        private void DrawGalleryItem(Graphics g, Rectangle bounds, RibbonGalleryItem item,
            bool isSelected, bool isHovered)
        {
            // Special rendering for semantic HTML styles
            if (CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
            {
                DrawSemanticHtmlItem(g, bounds, item, isSelected, isHovered);
                return;
            }

            var text = _textPosition == RibbonTextPosition.Hide ? null : item.Label;
            RibbonRenderer.Instance.DrawGalleryItem(g, bounds, text, item.Image, isSelected, isHovered);
        }

        private void DrawSemanticHtmlItem(Graphics g, Rectangle bounds, RibbonGalleryItem item,
            bool isSelected, bool isHovered)
        {
            // Determine styling based on the label - larger font sizes for better visibility
            float fontSize = 11f;
            bool isBold = false;
            string displayLabel = item.Label;

            switch (item.Label)
            {
                case "Heading 1":
                    fontSize = 14f;
                    isBold = true;
                    displayLabel = "Heading 1";
                    break;
                case "Heading 2":
                    fontSize = 13f;
                    isBold = true;
                    displayLabel = "Heading 2";
                    break;
                case "Heading 3":
                    fontSize = 12f;
                    isBold = true;
                    displayLabel = "Heading 3";
                    break;
                case "Heading 4":
                    fontSize = 11f;
                    isBold = true;
                    displayLabel = "Heading 4";
                    break;
                case "Heading 5":
                    fontSize = 10.5f;
                    isBold = true;
                    displayLabel = "Heading 5";
                    break;
                case "Heading 6":
                    fontSize = 10f;
                    isBold = true;
                    displayLabel = "Heading 6";
                    break;
                case "Paragraph":
                default:
                    fontSize = 11f;
                    isBold = false;
                    displayLabel = "Paragraph";
                    break;
            }

            // Draw background with clear hover/selection states
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
                // Light background for unselected items
                using (var brush = new SolidBrush(Color.White))
                    g.FillRectangle(brush, bounds);
            }

            // Draw display label with appropriate font style
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
            using (var font = new Font("Calibri", fontSize, fontStyle))
            using (var textBrush = new SolidBrush(Color.FromArgb(51, 51, 51)))
            {
                var textBounds = new Rectangle(bounds.X + 4, bounds.Y + 2,
                    bounds.Width - 8, bounds.Height - 4);
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(displayLabel, font, textBrush, textBounds, format);
            }
        }

        private void DrawScrollButton(Graphics g, Rectangle bounds, bool isUp, bool isEnabled)
        {
            // Background
            using (var brush = new SolidBrush(RibbonColors.Current.ButtonBackground))
            {
                g.FillRectangle(brush, bounds);
            }

            // Arrow
            var arrowColor = isEnabled ? RibbonColors.Current.ButtonText : RibbonColors.Current.ButtonTextDisabled;
            var arrowSize = 5;
            var arrowX = bounds.X + (bounds.Width - arrowSize) / 2;
            var arrowY = bounds.Y + (bounds.Height - arrowSize / 2) / 2;

            using (var brush = new SolidBrush(arrowColor))
            {
                Point[] points;
                if (isUp)
                {
                    points = new Point[]
                    {
                        new Point(arrowX + arrowSize / 2, arrowY),
                        new Point(arrowX, arrowY + arrowSize / 2 + 1),
                        new Point(arrowX + arrowSize, arrowY + arrowSize / 2 + 1)
                    };
                }
                else
                {
                    points = new Point[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX + arrowSize, arrowY),
                        new Point(arrowX + arrowSize / 2, arrowY + arrowSize / 2 + 1)
                    };
                }
                g.FillPolygon(brush, points);
            }

            // Border
            using (var pen = new Pen(RibbonColors.Current.GroupSeparator))
            {
                g.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
            }
        }

        private void DrawExpandButton(Graphics g, Rectangle bounds)
        {
            // Background
            using (var brush = new SolidBrush(RibbonColors.Current.ButtonBackground))
            {
                g.FillRectangle(brush, bounds);
            }

            // Dropdown arrow (larger)
            var arrowColor = RibbonColors.Current.ButtonText;
            var arrowSize = 6;
            var arrowX = bounds.X + (bounds.Width - arrowSize) / 2;
            var arrowY = bounds.Y + (bounds.Height - arrowSize / 2) / 2;

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

            // Underline
            using (var pen = new Pen(arrowColor))
            {
                g.DrawLine(pen, arrowX, arrowY + arrowSize / 2 + 4,
                    arrowX + arrowSize, arrowY + arrowSize / 2 + 4);
            }
        }

        private bool CanScrollDown()
        {
            if (_galleryType != RibbonGalleryType.InRibbon) return false;

            // Use configured columns
            var visibleColumns = Math.Min(_columns, Math.Max(1, _contentBounds.Width / _itemWidth));
            var visibleRows = Math.Max(1, _contentBounds.Height / _itemHeight);
            var totalRows = (_items.Count + visibleColumns - 1) / visibleColumns;

            return _scrollOffset + visibleRows < totalRows;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Ensure bounds are calculated
                if (_contentBounds.IsEmpty)
                {
                    CalculateBounds();
                }

                // Find hovered item
                var newHovered = GetItemIndexAtPoint(e.Location);
                if (newHovered != _hoveredIndex)
                {
                    _hoveredIndex = newHovered;
                    Invalidate();
                }
            }
            else
            {
                // Dropdown mode - hover the whole control
                var newHovered = ClientRectangle.Contains(e.Location) ? 0 : -1;
                if (newHovered != _hoveredIndex)
                {
                    _hoveredIndex = newHovered;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_hoveredIndex >= 0)
            {
                _hoveredIndex = -1;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left) return;

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Ensure bounds are calculated
                if (_contentBounds.IsEmpty)
                {
                    CalculateBounds();
                }

                // Check scroll buttons
                if (_upScrollBounds.Contains(e.Location) && _scrollOffset > 0)
                {
                    _scrollOffset--;
                    Invalidate();
                    return;
                }

                if (_downScrollBounds.Contains(e.Location) && CanScrollDown())
                {
                    _scrollOffset++;
                    Invalidate();
                    return;
                }

                if (_expandBounds.Contains(e.Location))
                {
                    ShowExpandedDropDown();
                    return;
                }

                // Check item click
                var index = GetItemIndexAtPoint(e.Location);
                if (index >= 0 && index < _items.Count)
                {
                    var item = _items[index];
                    
                    // Update selected index
                    _selectedIndex = index;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    
                    // Fire item click event
                    ItemClick?.Invoke(this, new GalleryItemClickEventArgs(item, index));
                    
                    // If the item has a command ID in its Tag, execute that command
                    if (item.Tag is OpenLiveWriter.Localization.CommandId itemCommandId)
                    {
                        System.Diagnostics.Debug.WriteLine($"Executing SemanticHtml command: {itemCommandId}");
                        CommandManager?.Execute(itemCommandId);
                    }
                    
                    Invalidate();
                }
            }
            else
            {
                ShowExpandedDropDown();
            }
        }

        private int GetItemIndexAtPoint(Point pt)
        {
            if (_contentBounds.IsEmpty || !_contentBounds.Contains(pt)) return -1;

            // Limit to configured columns
            var visibleColumns = Math.Min(_columns, Math.Max(1, _contentBounds.Width / _itemWidth));
            var col = (pt.X - _contentBounds.X) / _itemWidth;
            var row = (pt.Y - _contentBounds.Y) / _itemHeight;

            if (col < 0 || col >= visibleColumns || row < 0) return -1;

            var index = (_scrollOffset + row) * visibleColumns + col;
            return (index >= 0 && index < _items.Count) ? index : -1;
        }

        private void ShowExpandedDropDown()
        {
            // Ensure items are populated before showing dropdown
            if (_items.Count == 0 && CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
            {
                EnsureSemanticHtmlItems();
            }

            if (_dropDown == null)
            {
                _dropDownPanel = new RibbonGalleryDropDownPanel(this);
                _dropDown = new ToolStripDropDown
                {
                    AutoSize = false,
                    Padding = Padding.Empty,
                    Margin = Padding.Empty
                };
                _dropDown.Items.Add(new ToolStripControlHost(_dropDownPanel)
                {
                    AutoSize = false,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                });
            }

            _dropDownPanel.UpdateLayout();
            _dropDown.Size = _dropDownPanel.Size;
            _dropDown.Items[0].Size = _dropDownPanel.Size;

            System.Diagnostics.Debug.WriteLine($"ShowExpandedDropDown: Items.Count={_items.Count}, DropDownSize={_dropDown.Size}");
            _dropDown.Show(this, new Point(0, Height));
            _isExpanded = true;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                if (e.Delta > 0 && _scrollOffset > 0)
                {
                    _scrollOffset--;
                    Invalidate();
                }
                else if (e.Delta < 0 && CanScrollDown())
                {
                    _scrollOffset++;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Adds an item to the gallery.
        /// </summary>
        public void AddItem(RibbonGalleryItem item)
        {
            _items.Add(item);
            Invalidate();
        }

        /// <summary>
        /// Clears all items from the gallery.
        /// </summary>
        public void ClearItems()
        {
            _items.Clear();
            _selectedIndex = -1;
            _scrollOffset = 0;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dropDown?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Represents an item in a gallery.
    /// </summary>
    public class RibbonGalleryItem
    {
        public string Label { get; set; }
        public Image Image { get; set; }
        public string Tooltip { get; set; }
        public object Tag { get; set; }

        public RibbonGalleryItem() { }

        public RibbonGalleryItem(string label, Image image = null)
        {
            Label = label;
            Image = image;
        }
    }

    /// <summary>
    /// Event args for gallery item clicks.
    /// </summary>
    public class GalleryItemClickEventArgs : EventArgs
    {
        public RibbonGalleryItem Item { get; }
        public int Index { get; }

        public GalleryItemClickEventArgs(RibbonGalleryItem item, int index)
        {
            Item = item;
            Index = index;
        }
    }

    /// <summary>
    /// Dropdown panel for expanded gallery view.
    /// </summary>
    internal class RibbonGalleryDropDownPanel : UserControl
    {
        private readonly RibbonGallery _gallery;
        private int _hoveredIndex = -1;

        public RibbonGalleryDropDownPanel(RibbonGallery gallery)
        {
            _gallery = gallery;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = RibbonColors.Current.GalleryBackground;
        }

        public void UpdateLayout()
        {
            var columns = _gallery.MaxColumns;
            var itemCount = Math.Max(1, _gallery.Items.Count);
            var rows = (itemCount + columns - 1) / columns;
            var width = columns * _gallery.ItemWidth + 4;
            var height = Math.Min(rows * _gallery.ItemHeight + 4, 
                Math.Max(_gallery.MaxRows, rows) * _gallery.ItemHeight + 4);

            Size = new Size(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var columns = _gallery.MaxColumns;
            var isSemanticHtmlGallery = _gallery.CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery;

            for (int i = 0; i < _gallery.Items.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;

                var itemBounds = new Rectangle(
                    2 + col * _gallery.ItemWidth,
                    2 + row * _gallery.ItemHeight,
                    _gallery.ItemWidth, _gallery.ItemHeight);

                var isSelected = i == _gallery.SelectedIndex;
                var isHovered = i == _hoveredIndex;
                var item = _gallery.Items[i];

                if (isSemanticHtmlGallery)
                {
                    // Use special rendering for semantic HTML items
                    DrawSemanticHtmlDropdownItem(g, itemBounds, item, isSelected, isHovered);
                }
                else
                {
                    var text = _gallery.TextPosition == RibbonTextPosition.Hide ? null : item.Label;
                    RibbonRenderer.Instance.DrawGalleryItem(g, itemBounds, text, item.Image, isSelected, isHovered);
                }
            }
        }

        private void DrawSemanticHtmlDropdownItem(Graphics g, Rectangle bounds, RibbonGalleryItem item, bool isSelected, bool isHovered)
        {
            // Determine styling based on the label
            float fontSize = 10f;
            bool isBold = false;
            string previewText = "AaBbCcDdI";

            switch (item.Label)
            {
                case "Heading 1":
                    fontSize = 16f;
                    isBold = true;
                    previewText = "AaBb";
                    break;
                case "Heading 2":
                    fontSize = 14f;
                    isBold = true;
                    previewText = "AaBbCc";
                    break;
                case "Heading 3":
                    fontSize = 12f;
                    isBold = true;
                    previewText = "AaBbCcDd";
                    break;
                case "Heading 4":
                    fontSize = 11f;
                    isBold = true;
                    previewText = "AaBbCcDdI";
                    break;
                case "Heading 5":
                    fontSize = 10f;
                    isBold = true;
                    previewText = "AaBbCcDdE";
                    break;
                case "Heading 6":
                    fontSize = 9f;
                    isBold = true;
                    previewText = "AaBbCcDdEe";
                    break;
                case "Paragraph":
                default:
                    fontSize = 10f;
                    isBold = false;
                    previewText = "AaBbCcDdI";
                    break;
            }

            // Draw background
            if (isSelected)
            {
                using (var brush = new SolidBrush(Color.FromArgb(201, 222, 245)))
                    g.FillRectangle(brush, bounds);
                using (var pen = new Pen(Color.FromArgb(168, 198, 230)))
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
            else if (isHovered)
            {
                using (var brush = new SolidBrush(Color.FromArgb(232, 239, 247)))
                    g.FillRectangle(brush, bounds);
                using (var pen = new Pen(Color.FromArgb(168, 198, 230)))
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }

            // Draw preview text
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
            var actualFontSize = Math.Min(fontSize, bounds.Height * 0.35f);
            using (var previewFont = new Font("Calibri", actualFontSize, fontStyle))
            using (var textBrush = new SolidBrush(Color.FromArgb(68, 68, 68)))
            {
                var previewBounds = new Rectangle(bounds.X + 2, bounds.Y + 2,
                    bounds.Width - 4, (int)(bounds.Height * 0.55f));
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(previewText, previewFont, textBrush, previewBounds, format);
            }

            // Draw label
            using (var labelFont = new Font(SystemFonts.MenuFont.FontFamily, 7f))
            using (var labelBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                var labelBounds = new Rectangle(bounds.X + 2, bounds.Y + (int)(bounds.Height * 0.55f),
                    bounds.Width - 4, (int)(bounds.Height * 0.4f));
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(item.Label, labelFont, labelBrush, labelBounds, format);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var newHovered = GetIndexAtPoint(e.Location);
            if (newHovered != _hoveredIndex)
            {
                _hoveredIndex = newHovered;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredIndex = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left) return;

            var index = GetIndexAtPoint(e.Location);
            if (index >= 0 && index < _gallery.Items.Count)
            {
                var item = _gallery.Items[index];
                
                // Update selection
                _gallery.SelectedIndex = index;

                // Execute the item's command if it has one
                if (item.Tag is OpenLiveWriter.Localization.CommandId itemCommandId)
                {
                    System.Diagnostics.Debug.WriteLine($"Dropdown executing SemanticHtml command: {itemCommandId}");
                    _gallery.CommandManager?.Execute(itemCommandId);
                }

                // Close dropdown
                var dropDown = Parent?.Parent as ToolStripDropDown;
                dropDown?.Close();
            }
        }

        private int GetIndexAtPoint(Point pt)
        {
            var columns = _gallery.MaxColumns;
            var col = (pt.X - 2) / _gallery.ItemWidth;
            var row = (pt.Y - 2) / _gallery.ItemHeight;

            if (col < 0 || col >= columns || row < 0) return -1;

            var index = row * columns + col;
            return index < _gallery.Items.Count ? index : -1;
        }
    }
}
