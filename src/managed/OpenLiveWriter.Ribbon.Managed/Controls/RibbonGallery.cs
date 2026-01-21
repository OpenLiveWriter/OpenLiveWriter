// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
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

        protected override void UpdateSize()
        {
            base.UpdateSize();

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
                // Calculate size for in-ribbon gallery
                var itemsWidth = _columns * _itemWidth;
                var width = itemsWidth + SCROLL_BUTTON_WIDTH + BORDER_WIDTH * 2;
                var height = _maxRows * _itemHeight + BORDER_WIDTH * 2;
                Size = new Size(width, Math.Min(height, 66));
            }
            else
            {
                // Dropdown gallery uses button size
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
            // Background and border
            using (var brush = new SolidBrush(RibbonColors.Current.GalleryBackground))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            using (var pen = new Pen(RibbonColors.Current.GalleryBorder))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            // Calculate bounds
            _contentBounds = new Rectangle(BORDER_WIDTH, BORDER_WIDTH,
                Width - SCROLL_BUTTON_WIDTH - BORDER_WIDTH * 2, Height - BORDER_WIDTH * 2);

            var scrollX = Width - SCROLL_BUTTON_WIDTH - BORDER_WIDTH;
            var scrollButtonHeight = (Height - BORDER_WIDTH * 2) / 3;
            _upScrollBounds = new Rectangle(scrollX, BORDER_WIDTH, SCROLL_BUTTON_WIDTH, scrollButtonHeight);
            _downScrollBounds = new Rectangle(scrollX, BORDER_WIDTH + scrollButtonHeight, SCROLL_BUTTON_WIDTH, scrollButtonHeight);
            _expandBounds = new Rectangle(scrollX, BORDER_WIDTH + scrollButtonHeight * 2, SCROLL_BUTTON_WIDTH, scrollButtonHeight);

            // Clip to content area
            g.SetClip(_contentBounds);

            // Draw visible items
            var visibleColumns = _contentBounds.Width / _itemWidth;
            var visibleRows = _contentBounds.Height / _itemHeight;

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

            g.ResetClip();

            // Draw scroll buttons
            DrawScrollButton(g, _upScrollBounds, true, _scrollOffset > 0);
            DrawScrollButton(g, _downScrollBounds, false, CanScrollDown());
            DrawExpandButton(g, _expandBounds);
        }

        private void DrawDropDownButton(Graphics g)
        {
            var image = CommandLargeImage ?? (CurrentSize == RibbonGroupSize.Large ? CommandLargeImage : CommandSmallImage);

            RibbonRenderer.Instance.DrawButton(g, ClientRectangle, CommandLabel, image,
                Enabled && CommandEnabled, _hoveredIndex >= 0, false, false,
                RibbonButtonType.DropDownButton, CurrentSize);
        }

        private void DrawGalleryItem(Graphics g, Rectangle bounds, RibbonGalleryItem item,
            bool isSelected, bool isHovered)
        {
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

            var visibleColumns = _contentBounds.Width / _itemWidth;
            var visibleRows = _contentBounds.Height / _itemHeight;
            var totalRows = (_items.Count + visibleColumns - 1) / visibleColumns;

            return _scrollOffset + visibleRows < totalRows;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_galleryType == RibbonGalleryType.InRibbon)
            {
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
                var wasHovered = _hoveredIndex >= 0;
                _hoveredIndex = ClientRectangle.Contains(e.Location) ? 0 : -1;
                if ((wasHovered && _hoveredIndex < 0) || (!wasHovered && _hoveredIndex >= 0))
                {
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
                    SelectedIndex = index;
                    ItemClick?.Invoke(this, new GalleryItemClickEventArgs(_items[index], index));
                    ExecuteCommand();
                }
            }
            else
            {
                ShowExpandedDropDown();
            }
        }

        private int GetItemIndexAtPoint(Point pt)
        {
            if (!_contentBounds.Contains(pt)) return -1;

            var col = (pt.X - _contentBounds.X) / _itemWidth;
            var row = (pt.Y - _contentBounds.Y) / _itemHeight;
            var visibleColumns = _contentBounds.Width / _itemWidth;

            var index = (_scrollOffset + row) * visibleColumns + col;
            return index < _items.Count ? index : -1;
        }

        private void ShowExpandedDropDown()
        {
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
            var rows = (_gallery.Items.Count + columns - 1) / columns;
            var width = columns * _gallery.ItemWidth + 4;
            var height = Math.Min(rows * _gallery.ItemHeight + 4, _gallery.MaxRows * _gallery.ItemHeight + 4);

            Size = new Size(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var columns = _gallery.MaxColumns;

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

                var text = _gallery.TextPosition == RibbonTextPosition.Hide ? null : _gallery.Items[i].Label;
                RibbonRenderer.Instance.DrawGalleryItem(g, itemBounds, text,
                    _gallery.Items[i].Image, isSelected, isHovered);
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
                _gallery.SelectedIndex = index;

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
