// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
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
            
            // For TextPosition.Right, use single column with wider items
            var effectiveItemWidth = _gallery.ItemWidth;
            if (_gallery.TextPosition == RibbonTextPosition.Right)
            {
                columns = 1;
                effectiveItemWidth = Math.Max(150, _gallery.ItemWidth); // At least 150px for text
            }
            
            var rows = (itemCount + columns - 1) / columns;
            var width = columns * effectiveItemWidth + 4;
            var height = Math.Min(rows * _gallery.ItemHeight + 4, 
                Math.Max(_gallery.MaxRows, rows) * _gallery.ItemHeight + 4);

            Size = new Size(width, height);
        }

        /// <summary>
        /// Gets the effective item width, accounting for TextPosition.Right layout.
        /// </summary>
        private int GetEffectiveItemWidth()
        {
            if (_gallery.TextPosition == RibbonTextPosition.Right)
            {
                return Math.Max(150, _gallery.ItemWidth);
            }
            return _gallery.ItemWidth;
        }

        /// <summary>
        /// Gets the effective column count, accounting for TextPosition.Right layout.
        /// </summary>
        private int GetEffectiveColumns()
        {
            if (_gallery.TextPosition == RibbonTextPosition.Right)
            {
                return 1; // Single column for list-style layout
            }
            return _gallery.MaxColumns;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var columns = GetEffectiveColumns();
            var effectiveItemWidth = GetEffectiveItemWidth();
            var isSemanticHtmlGallery = _gallery.CommandId == OpenLiveWriter.Localization.CommandId.SemanticHtmlGallery;
            var isListStyle = _gallery.TextPosition == RibbonTextPosition.Right;

            for (int i = 0; i < _gallery.Items.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;

                var itemBounds = new Rectangle(
                    2 + col * effectiveItemWidth,
                    2 + row * _gallery.ItemHeight,
                    effectiveItemWidth, _gallery.ItemHeight);

                var isSelected = i == _gallery.SelectedIndex;
                var isHovered = i == _hoveredIndex;
                var item = _gallery.Items[i];

                if (isSemanticHtmlGallery)
                {
                    GalleryItemRenderer.DrawSemanticHtmlItem(g, itemBounds, item, isSelected, isHovered);
                }
                else if (isListStyle)
                {
                    GalleryItemRenderer.DrawListStyleItem(g, itemBounds, item, isSelected, isHovered);
                }
                else
                {
                    var text = _gallery.TextPosition == RibbonTextPosition.Hide ? null : item.Label;
                    RibbonRenderer.Instance.DrawGalleryItem(g, itemBounds, text, item.Image, isSelected, isHovered);
                }
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

                // If the item has a command ID in its Tag (e.g., SemanticHtmlGallery), execute that command
                if (item.Tag is OpenLiveWriter.Localization.CommandId itemCommandId)
                {
                    _gallery.CommandManager?.Execute(itemCommandId);
                }
                else
                {
                    // For gallery commands like BlogProviderButtonsGallery, 
                    // set the selected index on the command and execute it
                    var command = _gallery.CommandManager?.GetCommand(_gallery.CommandId);
                    if (command is IGalleryCommand galleryCommand)
                    {
                        galleryCommand.SelectedIndex = index;
                    }
                    
                    command?.PerformExecute();
                }

                // Close dropdown
                _gallery.CloseDropDown();
            }
        }

        private int GetIndexAtPoint(Point pt)
        {
            var columns = GetEffectiveColumns();
            var effectiveItemWidth = GetEffectiveItemWidth();
            var col = (pt.X - 2) / effectiveItemWidth;
            var row = (pt.Y - 2) / _gallery.ItemHeight;

            if (col < 0 || col >= columns || row < 0) return -1;

            var index = row * columns + col;
            return index < _gallery.Items.Count ? index : -1;
        }
    }
}
