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
        private int _minColumnsLarge = 0;
        private RibbonGalleryLayout _layout = RibbonGalleryLayout.Flow;

        private readonly List<RibbonGalleryItem> _items = new List<RibbonGalleryItem>();
        private int _selectedIndex = -1;
        private int _hoveredIndex = -1;
        private int _scrollOffset = 0;

        private Rectangle _contentBounds;
        private Rectangle _upScrollBounds;
        private Rectangle _downScrollBounds;
        private Rectangle _expandBounds;

        private ToolStripDropDown _dropDown;
        private RibbonGalleryDropDownPanel _dropDownPanel;
        private DropDownMouseHook _mouseHook;

        /// <summary>
        /// Gets the dropdown control for external access.
        /// </summary>
        internal ToolStripDropDown DropDown => _dropDown;

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
        /// Gets or sets the minimum columns for large ribbon mode.
        /// When set to a positive value, this controls the gallery width.
        /// Use 0 for auto-calculation based on Columns property.
        /// </summary>
        public int MinColumnsLarge
        {
            get => _minColumnsLarge;
            set
            {
                _minColumnsLarge = Math.Max(0, value);
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
                    UpdateAccessibility();  // Update accessible name when selection changes
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

        // Accessible button overlay for UI Automation support
        // Using a regular Button with minimal opacity for better click handling
        private Button _accessibleButton;

        public RibbonGallery()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            
            // Create accessible button overlay for UI Automation click support
            // This button is only shown for CompactDropDown galleries
            _accessibleButton = new Button
            {
                Name = "GalleryAccessibleButton",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(1, 255, 255, 255), // Nearly transparent but still receives clicks
                ForeColor = Color.Transparent,
                FlatAppearance = { 
                    BorderSize = 0,
                    MouseOverBackColor = Color.Transparent,
                    MouseDownBackColor = Color.Transparent
                },
                TabStop = false,
                Enabled = true,
                Text = "",
                Visible = false  // Hidden by default, shown only for CompactDropDown
            };
            
            // Wire up click event
            _accessibleButton.Click += AccessibleButton_Click;
            
            // Wire up mouse events to propagate hover state to gallery
            _accessibleButton.MouseEnter += (s, ev) => 
            {
                if (_galleryType != RibbonGalleryType.InRibbon)
                {
                    _hoveredIndex = 0;
                    Invalidate();
                }
            };
            _accessibleButton.MouseLeave += (s, ev) => 
            {
                if (_galleryType != RibbonGalleryType.InRibbon)
                {
                    _hoveredIndex = -1;
                    Invalidate();
                }
            };
            
            Controls.Add(_accessibleButton);
            
            UpdateSize();
        }

        private void AccessibleButton_Click(object sender, EventArgs e)
        {
            // For non-InRibbon galleries, show dropdown
            if (_galleryType != RibbonGalleryType.InRibbon)
            {
                ShowExpandedDropDown();
            }
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
            UpdateAccessibility();
        }

        /// <summary>
        /// Updates accessibility properties for UI Automation discovery.
        /// </summary>
        private void UpdateAccessibility()
        {
            // Set accessible name based on selected item or command label
            // This allows UI Automation to find the control (e.g., by blog name)
            // Strip accelerator characters (&) for clean accessible names
            string accessibleText = RibbonRenderer.StripAccelerator(CommandLabel);
            
            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
            {
                var selectedItem = _items[_selectedIndex];
                if (!string.IsNullOrEmpty(selectedItem.Label))
                {
                    accessibleText = RibbonRenderer.StripAccelerator(selectedItem.Label);
                }
            }
            
            // Set accessible name ONLY on the button for CompactDropDown galleries
            // This ensures UI Automation finds and clicks the button, not the parent
            if (_galleryType == RibbonGalleryType.CompactDropDown && _accessibleButton != null)
            {
                // Clear parent's accessible name so button is found instead
                AccessibleName = "";
                AccessibleRole = AccessibleRole.Client;
                
                _accessibleButton.AccessibleName = accessibleText;
                _accessibleButton.AccessibleRole = AccessibleRole.PushButton;
                _accessibleButton.AccessibleDescription = $"Click to show {RibbonRenderer.StripAccelerator(CommandLabel)} dropdown";
                _accessibleButton.Text = ""; // No visible text
                
                // Position button to cover the control and make visible
                _accessibleButton.Location = Point.Empty;
                _accessibleButton.Size = Size;
                _accessibleButton.Visible = true;  // Show only for CompactDropDown
                _accessibleButton.BringToFront();
            }
            else
            {
                AccessibleName = accessibleText;
                AccessibleRole = AccessibleRole.List;
                AccessibleDescription = $"Gallery: {RibbonRenderer.StripAccelerator(CommandLabel)}";
                
                // Hide accessible button for InRibbon galleries
                if (_accessibleButton != null)
                {
                    _accessibleButton.Visible = false;
                }
            }
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

        /// <summary>
        /// Forces loading of gallery items from the source command.
        /// Call this before showing the dropdown if items might not be loaded.
        /// </summary>
        private void ForceLoadItemsFromCommand()
        {
            var command = CommandManager?.GetCommand(CommandId);
            if (command is BridgedCommand bridged)
            {
                bridged.ForceLoadGalleryItems();
                // Items will be updated via ItemsChanged event, but also reload directly
                LoadItemsFromCommand();
            }
        }

        private void OnGalleryItemsChanged(object sender, EventArgs e)
        {
            LoadItemsFromCommand();
        }

        // Track last time we tried to load items to avoid excessive retries
        private DateTime _lastItemLoadAttempt = DateTime.MinValue;
        private static readonly TimeSpan ItemLoadRetryInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Tries to load items if the gallery is empty.
        /// Rate-limited to avoid excessive retries.
        /// </summary>
        private void TryLoadItemsIfEmpty()
        {
            if (_items.Count > 0)
                return;

            // Rate limit retries to avoid performance issues
            var now = DateTime.Now;
            if (now - _lastItemLoadAttempt < ItemLoadRetryInterval)
                return;

            _lastItemLoadAttempt = now;
            
            // Try forcing a load from the source command
            ForceLoadItemsFromCommand();
        }

        /// <summary>
        /// Gets the preferred width for this gallery.
        /// </summary>
        public int GetPreferredWidth()
        {
            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Use MinColumnsLarge for explicit width control if set
                var effectiveColumns = _minColumnsLarge > 0 ? _minColumnsLarge : _columns;

                // For TextPosition.Right, size for icon + text
                var effectiveItemWidth = _itemWidth;
                if (_textPosition == RibbonTextPosition.Right)
                {
                    // Icon (16px) + padding (4px) + text space. Measure the
                    // longest item label so text is not cut off, with a floor
                    // of 110px (compact) and a cap so long names do not
                    // balloon the group.
                    effectiveItemWidth = 110;
                    using (var g = CreateGraphics())
                    {
                        foreach (var item in _items)
                        {
                            if (!string.IsNullOrEmpty(item?.Label))
                            {
                                var textWidth = TextRenderer.MeasureText(g, item.Label, SystemFonts.MenuFont).Width;
                                effectiveItemWidth = Math.Min(Math.Max(effectiveItemWidth, 16 + 4 + textWidth + 8), 260);
                            }
                        }
                    }
                }

                return effectiveColumns * effectiveItemWidth + SCROLL_BUTTON_WIDTH + BORDER_WIDTH * 2;
            }
            else if (_galleryType == RibbonGalleryType.CompactDropDown)
            {
                // Calculate width to fit selected item text, capped to avoid bloating the group
                // Layout: 4px + 16px icon + 4px gap + text + 18px arrow
                const int maxDropDownWidth = 170;
                var minWidth = 120;
                var textToMeasure = CommandLabel;
                if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                    textToMeasure = _items[_selectedIndex].Label ?? CommandLabel;
                if (!string.IsNullOrEmpty(textToMeasure))
                {
                    using (var g = CreateGraphics())
                    {
                        var textWidth = TextRenderer.MeasureText(g, textToMeasure, SystemFonts.MenuFont).Width;
                        minWidth = Math.Max(minWidth, Math.Min(4 + 20 + textWidth + 18, maxDropDownWidth));
                    }
                }
                return minWidth;
            }
            else
            {
                // Standard dropdown galleries use button sizing
                return CurrentSize == RibbonGroupSize.Large ? 56 : 
                       CurrentSize == RibbonGroupSize.Medium ? 80 : 24;
            }
        }

        /// <summary>
        /// Gets the maximum width this gallery would use if given unlimited space.
        /// For InRibbon galleries, this is the width needed to show MaxColumns items.
        /// </summary>
        public int GetMaxPreferredWidth()
        {
            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Allow expansion up to MaxColumns when surplus width is available.
                // Proportional surplus distribution in RibbonTab.LayoutGroups prevents
                // over-expansion at moderate widths while allowing full use at fullscreen.
                var effectiveItemWidth = _itemWidth;
                if (_textPosition == RibbonTextPosition.Right)
                {
                    effectiveItemWidth = 110;
                }
                return _maxColumns * effectiveItemWidth + SCROLL_BUTTON_WIDTH + BORDER_WIDTH * 2;
            }
            return GetPreferredWidth();
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Calculate preferred size for in-ribbon gallery
                var width = GetPreferredWidth();
                
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
                // Compact dropdown - use dynamic width from GetPreferredWidth
                var dropdownWidth = GetPreferredWidth();
                Size = new Size(dropdownWidth, 24);
            }
            else
            {
                // Standard dropdown gallery uses button size
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
            
            // Update accessible button size to match (only for CompactDropDown)
            if (_accessibleButton != null && _galleryType == RibbonGalleryType.CompactDropDown)
            {
                _accessibleButton.Size = Size;
                _accessibleButton.BringToFront();
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
            if (_items.Count == 0)
            {
                if (CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
                {
                    EnsureSemanticHtmlItems();
                }
                else
                {
                    // For other galleries (like BlogProviderButtonsGallery), try to load items
                    // This handles the case where the source command wasn't available at initialization
                    TryLoadItemsIfEmpty();
                }
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

            // If no items and this is the SemanticHtmlGallery, draw default style preview
            // Other galleries (like BlogProviderButtonsGallery) should not show this placeholder
            if (_items.Count == 0 && CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
            {
                DrawDefaultStylePreview(g, _contentBounds);
            }
            else if (_items.Count > 0)
            {
                // For TextPosition.Right, use a fixed width for icon + text (not full width)
                var effectiveItemWidth = _itemWidth;
                if (_textPosition == RibbonTextPosition.Right)
                {
                    // Icon (16px) + padding (4px) + text space (~100px) = ~120px
                    // Use the content width but cap it to reasonable size
                    effectiveItemWidth = Math.Min(_contentBounds.Width, 140);
                }

                // Draw visible items - allow up to MaxColumns when space is available
                var visibleColumns = Math.Min(_maxColumns, Math.Max(1, _contentBounds.Width / effectiveItemWidth));
                var visibleRows = Math.Max(1, _contentBounds.Height / _itemHeight);

                // Calculate effective item height to fill the available space
                // This ensures items fill the full gallery height like the native ribbon
                var effectiveItemHeight = _contentBounds.Height / visibleRows;

                for (int row = 0; row < visibleRows; row++)
                {
                    for (int col = 0; col < visibleColumns; col++)
                    {
                        var index = (_scrollOffset + row) * visibleColumns + col;
                        if (index >= _items.Count) break;

                        var itemBounds = new Rectangle(
                            _contentBounds.X + col * effectiveItemWidth,
                            _contentBounds.Y + row * effectiveItemHeight,
                            effectiveItemWidth, effectiveItemHeight);

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
            
            var visibleCols = Math.Min(_maxColumns, Math.Max(1, contentWidth / _itemWidth));
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

            // Draw white background with subtle border (matches item styling)
            using (var brush = new SolidBrush(Color.White))
                g.FillRectangle(brush, bounds);
            using (var pen = new Pen(Color.FromArgb(212, 212, 212), 1))
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

            // Calculate layout: preview text takes ~68% of height, label takes ~32%
            var previewHeight = (int)(bounds.Height * 0.68f);
            var labelHeight = bounds.Height - previewHeight;

            // Preview text (styled font - matches Paragraph style)
            // Preview text with high-quality rendering for crisp text at any DPI
            using (var previewFont = new Font("Calibri", 11f))
            {
                var previewBounds = new Rectangle(bounds.X + 3, bounds.Y + 2, 
                    bounds.Width - 6, previewHeight - 2);
                RibbonRenderer.DrawHighQualityText(g, previewText, previewFont, 
                    Color.FromArgb(51, 51, 51), previewBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | 
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }

            // Label text (smaller, gray) with high-quality rendering
            using (var labelFont = new Font(SystemFonts.MenuFont.FontFamily, 7.5f))
            {
                var labelBounds = new Rectangle(bounds.X + 3, bounds.Y + previewHeight, 
                    bounds.Width - 6, labelHeight - 3);
                RibbonRenderer.DrawHighQualityText(g, labelText, labelFont, 
                    Color.FromArgb(102, 102, 102), labelBounds,
                    TextFormatFlags.Left | TextFormatFlags.Top | 
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
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

            // Select image based on size: large buttons use 32x32, medium/small use 16x16
            // Fall back from large to small (or vice versa) if the preferred size is not available
            var image = CurrentSize == RibbonGroupSize.Large 
                ? (CommandLargeImage ?? CommandSmallImage) 
                : (CommandSmallImage ?? CommandLargeImage);

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

            // Draw text with high-quality rendering
            if (!string.IsNullOrEmpty(itemText))
            {
                var textBounds = new Rectangle(x, 0, Width - x - 18, Height);
                RibbonRenderer.DrawHighQualityText(g, RibbonRenderer.StripAccelerator(itemText), 
                    SystemFonts.MenuFont, textColor, textBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | 
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
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
                GalleryItemRenderer.DrawSemanticHtmlItem(g, bounds, item, isSelected, isHovered,
                    TextFormatFlags.Left);
                return;
            }

            // Special rendering for TextPosition.Right (icon on left, text on right)
            if (_textPosition == RibbonTextPosition.Right)
            {
                GalleryItemRenderer.DrawListStyleItem(g, bounds, item, isSelected, isHovered);
                return;
            }

            var text = _textPosition == RibbonTextPosition.Hide ? null : item.Label;
            RibbonRenderer.Instance.DrawGalleryItem(g, bounds, text, item.Image, isSelected, isHovered);
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

            // For TextPosition.Right, use a fixed width for icon + text
            var effectiveItemWidth = _itemWidth;
            if (_textPosition == RibbonTextPosition.Right)
            {
                effectiveItemWidth = Math.Min(_contentBounds.Width, 140);
            }

            // Use max columns and max rows
            var visibleColumns = Math.Min(_maxColumns, Math.Max(1, _contentBounds.Width / effectiveItemWidth));
            var visibleRows = Math.Min(_maxRows, Math.Max(1, _contentBounds.Height / _itemHeight));
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
                    UpdateAccessibility();  // Update accessible name when selection changes
                    
                    // Fire item click event
                    ItemClick?.Invoke(this, new GalleryItemClickEventArgs(item, index));
                    
                    // Execute the clicked item
                    ExecuteGalleryItem(item, index);
                    
                    Invalidate();
                }
            }
            else
            {
                ShowExpandedDropDown();
            }
        }

        /// <summary>
        /// Executes a gallery item click action.
        /// </summary>
        private void ExecuteGalleryItem(RibbonGalleryItem item, int index)
        {
            // If the item has a command ID in its Tag (e.g., SemanticHtmlGallery), execute that command
            if (item.Tag is OpenLiveWriter.Localization.CommandId itemCommandId)
            {
                CommandManager?.Execute(itemCommandId);
                return;
            }
            
            // For gallery commands like BlogProviderButtonsGallery, the Tag contains a Command object
            // We need to set the SelectedIndex on the bridged command and then execute the gallery command
            var command = CommandManager?.GetCommand(CommandId);
            if (command is IGalleryCommand galleryCommand)
            {
                // Set selected index first
                galleryCommand.SelectedIndex = index;
            }
            
            // Execute the gallery command itself (triggers ExecuteWithArgs on the source command)
            // Execute the gallery command itself (triggers ExecuteWithArgs on the source command)
            command?.PerformExecute();
        }

        private int GetItemIndexAtPoint(Point pt)
        {
            if (_contentBounds.IsEmpty || !_contentBounds.Contains(pt)) return -1;

            // For TextPosition.Right, use a fixed width for icon + text
            var effectiveItemWidth = _itemWidth;
            if (_textPosition == RibbonTextPosition.Right)
            {
                effectiveItemWidth = Math.Min(_contentBounds.Width, 140);
            }

            // Allow up to MaxColumns when space is available
            var visibleColumns = Math.Min(_maxColumns, Math.Max(1, _contentBounds.Width / effectiveItemWidth));
            
            // Calculate effective item height (same as in DrawInRibbonGallery for consistent hit testing)
            var visibleRows = Math.Max(1, _contentBounds.Height / _itemHeight);
            var effectiveItemHeight = _contentBounds.Height / visibleRows;
            
            var col = (pt.X - _contentBounds.X) / effectiveItemWidth;
            var row = (pt.Y - _contentBounds.Y) / effectiveItemHeight;

            if (col < 0 || col >= visibleColumns || row < 0) return -1;

            var index = (_scrollOffset + row) * visibleColumns + col;
            return (index >= 0 && index < _items.Count) ? index : -1;
        }

        private void ShowExpandedDropDown()
        {
            // Ensure items are populated before showing dropdown
            if (_items.Count == 0)
            {
                if (CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery)
                {
                    EnsureSemanticHtmlItems();
                }
                else
                {
                    // Force load items from source command (e.g., for blog selector)
                    ForceLoadItemsFromCommand();
                }
            }

            if (_dropDown == null)
            {
                _dropDownPanel = new RibbonGalleryDropDownPanel(this);
                _dropDown = new ToolStripDropDown
                {
                    AutoSize = false,
                    AutoClose = true,
                    Padding = Padding.Empty,
                    Margin = Padding.Empty
                };
                _dropDown.Items.Add(new ToolStripControlHost(_dropDownPanel)
                {
                    AutoSize = false,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                });
                
                // Handle closing to clean up message filter and update state
                _dropDown.Closing += (s, e) => RemoveMessageFilter();
            }

            _dropDownPanel.UpdateLayout();
            
            // Use the size calculated by UpdateLayout which properly accounts for grid layout
            var dropDownSize = _dropDownPanel.Size;
            
            // Ensure minimum width for readability (e.g., blog selector needs wider dropdown)
            if (dropDownSize.Width < Width)
                dropDownSize.Width = Width;
            
            _dropDown.Size = dropDownSize;
            _dropDown.Items[0].Size = dropDownSize;

            // Add message filter to detect clicks outside the dropdown
            // This handles clicks on native controls (WebView2/MSHTML) that don't trigger auto-close
            AddMessageFilter();
            
            _dropDown.Show(this, new Point(0, Height));
            DropDownMouseHook.RegisterVisibleDropDown(_dropDown);
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

        /// <summary>
        /// Closes the expanded dropdown if open.
        /// </summary>
        internal void CloseDropDown()
        {
            _dropDown?.Close();
        }

        /// <summary>
        /// Adds the mouse hook to detect clicks outside the dropdown.
        /// </summary>
        private void AddMessageFilter()
        {
            if (_mouseHook == null)
            {
                _mouseHook = new DropDownMouseHook(
                    this,
                    () => _dropDown,
                    () => CloseDropDown()
                );
            }
            _mouseHook.Install();
        }

        /// <summary>
        /// Removes the mouse hook.
        /// </summary>
        private void RemoveMessageFilter()
        {
            _mouseHook?.Remove();
            DropDownMouseHook.UnregisterVisibleDropDown(_dropDown);
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
}
