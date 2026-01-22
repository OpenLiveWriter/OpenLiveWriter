// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Represents a group of controls within a ribbon tab.
    /// </summary>
    public class RibbonGroup : UserControl
    {
        // Use shared layout constants
        private const int MIN_WIDTH = LayoutConstants.GroupMinWidth;
        private const int LABEL_HEIGHT = LayoutConstants.GroupLabelHeight;
        private const int PADDING = LayoutConstants.GroupPadding;

        private RibbonCommandManager _commandManager;
        private CommandId _commandId;
        private string _label;
        private string _keytip;
        private string _sizeDefinition;
        private RibbonApplicationMode _visibleModes = RibbonApplicationMode.All;
        private RibbonGroupSize _currentSize = RibbonGroupSize.Large;
        private RibbonGroupSize _idealSize = RibbonGroupSize.Large;

        private readonly List<RibbonControlBase> _controls = new List<RibbonControlBase>();
        private readonly Panel _contentPanel;
        private readonly Label _labelControl;

        private bool _isPopupMode;
        private ToolStripDropDown _popupDropDown;

        /// <summary>
        /// Gets or sets the command ID for this group.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandId CommandId
        {
            get => _commandId;
            set => _commandId = value;
        }

        /// <summary>
        /// Gets or sets the display label.
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                _labelControl.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the keytip for keyboard navigation.
        /// </summary>
        public string Keytip
        {
            get => _keytip;
            set => _keytip = value;
        }

        /// <summary>
        /// Gets or sets the application modes where this group is visible.
        /// </summary>
        public RibbonApplicationMode VisibleModes
        {
            get => _visibleModes;
            set => _visibleModes = value;
        }

        /// <summary>
        /// Gets or sets the ideal size for this group.
        /// </summary>
        public RibbonGroupSize IdealSize
        {
            get => _idealSize;
            set => _idealSize = value;
        }

        /// <summary>
        /// Gets or sets the size definition for this group.
        /// Controls the layout of controls within the group.
        /// </summary>
        public string SizeDefinition
        {
            get => _sizeDefinition;
            set
            {
                _sizeDefinition = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// Gets or sets the current size for this group.
        /// </summary>
        public RibbonGroupSize CurrentSize
        {
            get => _currentSize;
            set
            {
                if (_currentSize != value)
                {
                    _currentSize = value;
                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the command manager.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCommandManager CommandManager
        {
            get => _commandManager;
            set
            {
                _commandManager = value;
                foreach (var control in _controls)
                {
                    control.CommandManager = value;
                }
            }
        }

        /// <summary>
        /// Gets the controls in this group.
        /// </summary>
        public new IReadOnlyList<RibbonControlBase> Controls => _controls.AsReadOnly();

        public RibbonGroup()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            MinimumSize = new Size(MIN_WIDTH, 0);

            // Label at bottom
            _labelControl = new Label
            {
                Dock = DockStyle.Bottom,
                Height = LABEL_HEIGHT,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = RibbonColors.Current.GroupLabelText,
                Font = new Font(SystemFonts.MenuFont.FontFamily, 8f)
            };
            base.Controls.Add(_labelControl);

            // Content panel
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(PADDING)
            };
            base.Controls.Add(_contentPanel);
        }

        /// <summary>
        /// Adds a control to this group.
        /// </summary>
        public void AddControl(RibbonControlBase control)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));

            control.CommandManager = _commandManager;
            // Don't overwrite the control's CurrentSize if it was explicitly configured
            // (non-Large means it was explicitly set)
            if (control.CurrentSize == RibbonGroupSize.Large)
            {
                control.CurrentSize = _currentSize;
            }
            _controls.Add(control);

            if (!_isPopupMode)
            {
                _contentPanel.Controls.Add(control);
            }

            UpdateLayout();
        }

        /// <summary>
        /// Removes a control from this group.
        /// </summary>
        public void RemoveControl(RibbonControlBase control)
        {
            if (control == null) return;

            if (_controls.Remove(control))
            {
                _contentPanel.Controls.Remove(control);
                UpdateLayout();
            }
        }

        /// <summary>
        /// Clears all controls from this group.
        /// </summary>
        public void ClearControls()
        {
            foreach (var control in _controls)
            {
                _contentPanel.Controls.Remove(control);
                control.Dispose();
            }
            _controls.Clear();
            UpdateLayout();
        }

        /// <summary>
        /// Gets the preferred width for this group at its current size.
        /// </summary>
        public int GetPreferredWidth()
        {
            if (_currentSize == RibbonGroupSize.Popup)
            {
                return LayoutConstants.PopupWidth;
            }

            // Handle specific SizeDefinition layouts
            if (SizeDefinition == "OneLargeComboSmall" && _controls.Count >= 3)
            {
                return GetOneLargeComboSmallWidth();
            }

            if (SizeDefinition == "FourButtons" && _controls.Count >= 4)
            {
                return GetFourButtonsWidth();
            }

            // Calculate width by simulating the layout
            var x = PADDING;
            var smallButtonSize = LayoutConstants.SmallButtonSize;
            var smallColumnStart = -1;
            var smallRow = 0;
            var maxSmallRows = LayoutConstants.MaxSmallButtonRows;

            foreach (var control in _controls)
            {
                if (!control.Visible) continue;

                var controlSize = control.CurrentSize;

                if (control is RibbonSeparator)
                {
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    x += 8; // separator width + spacing
                }
                else if (control is RibbonButton && controlSize == RibbonGroupSize.Small)
                {
                    if (smallColumnStart < 0)
                    {
                        smallColumnStart = x;
                        smallRow = 0;
                    }
                    smallRow++;
                    if (smallRow >= maxSmallRows)
                    {
                        smallColumnStart += smallButtonSize + 1;
                        smallRow = 0;
                    }
                    x = Math.Max(x, smallColumnStart + smallButtonSize + 2);
                }
                else if (control is RibbonButton btn && controlSize == RibbonGroupSize.Medium)
                {
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    // Calculate width based on text content
                    var buttonWidth = 60;
                    var label = btn.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        using (var g = CreateGraphics())
                        {
                            var textWidth = (int)g.MeasureString(label, SystemFonts.MenuFont).Width;
                            var dropdownSpace = (btn.ButtonType == RibbonButtonType.DropDownButton || 
                                                 btn.ButtonType == RibbonButtonType.SplitButton) ? 16 : 0;
                            buttonWidth = Math.Max(buttonWidth, 24 + textWidth + 8 + dropdownSpace);
                        }
                    }
                    x += buttonWidth + 2;
                }
                else if (control is RibbonButton btn2)
                {
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    // Calculate width based on text content
                    var buttonWidth = 50;
                    var label = btn2.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        using (var g = CreateGraphics())
                        {
                            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                            {
                                var textWidth = (int)g.MeasureString(label, font).Width;
                                buttonWidth = Math.Max(buttonWidth, textWidth + 10);
                            }
                        }
                    }
                    x += buttonWidth + 2;
                }
                else if (control is RibbonComboBox || control is RibbonSpinner)
                {
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    x += control.Width + 4;
                }
                else if (control is RibbonColorPicker)
                {
                    if (smallColumnStart < 0)
                    {
                        smallColumnStart = x;
                        smallRow = 0;
                    }
                    smallRow++;
                    if (smallRow >= maxSmallRows)
                    {
                        smallColumnStart += smallButtonSize + 1;
                        smallRow = 0;
                    }
                    x = Math.Max(x, smallColumnStart + smallButtonSize + 2);
                }
                else if (control is RibbonGallery gallery)
                {
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    x += gallery.GetPreferredWidth() + 2;
                }
                else
                {
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    x += control.Width + 2;
                }
            }

            var width = x + PADDING;

            // Ensure label fits
            using (var g = CreateGraphics())
            {
                var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                width = Math.Max(width, labelWidth);
            }

            return Math.Max(width, MIN_WIDTH);
        }

        /// <summary>
        /// Calculate the preferred width for the "OneLargeComboSmall" SizeDefinition.
        /// </summary>
        private int GetOneLargeComboSmallWidth()
        {
            var x = PADDING;
            
            // Large button width
            if (_controls.Count > 0)
            {
                var largeButton = _controls[0];
                var buttonWidth = 56;
                var label = largeButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                        {
                            var textWidth = (int)g.MeasureString(label, font).Width;
                            buttonWidth = Math.Max(buttonWidth, textWidth + 10);
                        }
                    }
                }
                x += buttonWidth + 4;
            }
            
            // Right column width (max of dropdown and button)
            var rightColumnWidth = 130; // default minimum
            
            // Check dropdown's configured width
            if (_controls.Count > 1 && _controls[1] is RibbonGallery gallery)
            {
                rightColumnWidth = Math.Max(rightColumnWidth, gallery.Width);
            }
            
            if (_controls.Count > 2 && _controls[2] is RibbonButton mediumButton)
            {
                var label = mediumButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        var textWidth = (int)g.MeasureString(label, SystemFonts.MenuFont).Width;
                        var dropdownSpace = (mediumButton.ButtonType == RibbonButtonType.DropDownButton || 
                                             mediumButton.ButtonType == RibbonButtonType.SplitButton) ? 16 : 0;
                        // 16px icon + 6px gap + text + 8px padding + dropdown arrow
                        rightColumnWidth = Math.Max(rightColumnWidth, 16 + 6 + textWidth + 8 + dropdownSpace);
                    }
                }
            }
            x += rightColumnWidth + PADDING;
            
            // Ensure label fits
            using (var g = CreateGraphics())
            {
                var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                x = Math.Max(x, labelWidth);
            }
            
            return Math.Max(x, MIN_WIDTH);
        }

        /// <summary>
        /// Calculate the preferred width for the "FourButtons" SizeDefinition.
        /// </summary>
        private int GetFourButtonsWidth()
        {
            var buttonWidth = 70; // minimum width
            using (var g = CreateGraphics())
            {
                foreach (var control in _controls)
                {
                    var label = control.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        var textWidth = (int)g.MeasureString(label, SystemFonts.MenuFont).Width;
                        // icon (16) + padding (6) + text + padding (4)
                        buttonWidth = Math.Max(buttonWidth, 16 + 6 + textWidth + 4);
                    }
                }
            }
            
            var width = buttonWidth + PADDING * 2;
            
            // Ensure label fits
            using (var g = CreateGraphics())
            {
                var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                width = Math.Max(width, labelWidth);
            }
            
            return Math.Max(width, MIN_WIDTH);
        }

        private void UpdateLayout()
        {
            if (_currentSize == RibbonGroupSize.Popup)
            {
                // Hide controls, show popup button
                foreach (var control in _controls)
                {
                    control.Visible = false;
                }
                _contentPanel.Visible = false;
                _isPopupMode = true;
                Width = 48;
            }
            else
            {
                _contentPanel.Visible = true;
                _isPopupMode = false;

                // Update control sizes (preserve explicitly configured sizes)
                foreach (var control in _controls)
                {
                    // Only update size if control uses default (Large) size
                    if (control.CurrentSize == RibbonGroupSize.Large)
                    {
                        control.CurrentSize = _currentSize;
                    }
                    control.Visible = true;
                }

                // Layout controls
                LayoutControls();

                Width = GetPreferredWidth();
            }

            Invalidate();
        }

        private void LayoutControls()
        {
            if (_contentPanel == null) return;

            var contentHeight = _contentPanel.Height;
            var availableHeight = Math.Max(contentHeight - PADDING * 2, 60);

            // Handle specific SizeDefinition layouts
            if (SizeDefinition == "OneLargeComboSmall" && _controls.Count >= 3)
            {
                // Layout: Large button on left, dropdown and medium button stacked on right
                LayoutOneLargeComboSmall(availableHeight);
                return;
            }

            if (SizeDefinition == "FourButtons" && _controls.Count >= 4)
            {
                // Layout: 4 medium buttons stacked vertically (2 rows of 2 or 4 rows of 1)
                LayoutFourButtons(availableHeight);
                return;
            }

            // Use a smarter layout that respects individual control sizes
            // Layout controls left-to-right, stacking small controls in columns
            var x = PADDING;
            var y = PADDING;
            var smallButtonSize = LayoutConstants.SmallButtonSize;
            var mediumButtonHeight = LayoutConstants.MediumButtonHeight;
            var smallColumnStart = -1; // Track where small button column starts
            var smallRow = 0;
            var maxSmallRows = LayoutConstants.MaxSmallButtonRows;

            foreach (var control in _controls)
            {
                if (!control.Visible) continue;

                // Use the control's own CurrentSize for layout decisions
                var controlSize = control.CurrentSize;

                if (control is RibbonSeparator sep)
                {
                    // If we were in a small button column, close it
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    sep.IsVertical = true;
                    control.Size = new Size(6, availableHeight);
                    control.Location = new Point(x, y);
                    x += control.Width + 2;
                }
                else if (control is RibbonButton btn && controlSize == RibbonGroupSize.Small)
                {
                    // Small buttons stack vertically in columns
                    if (smallColumnStart < 0)
                    {
                        smallColumnStart = x;
                        smallRow = 0;
                    }

                    control.Size = new Size(smallButtonSize, smallButtonSize);
                    control.Location = new Point(smallColumnStart, y + smallRow * (smallButtonSize + 1));

                    smallRow++;
                    if (smallRow >= maxSmallRows)
                    {
                        smallColumnStart += smallButtonSize + 1;
                        smallRow = 0;
                    }
                    x = Math.Max(x, smallColumnStart + smallButtonSize + 2);
                }
                else if (control is RibbonButton btn2 && controlSize == RibbonGroupSize.Medium)
                {
                    // Medium buttons are horizontal with icon and text
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    // Calculate width based on text content
                    var buttonWidth = 60; // reasonable minimum
                    var label = btn2.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        using (var g = CreateGraphics())
                        {
                            var textWidth = (int)g.MeasureString(label, SystemFonts.MenuFont).Width;
                            // icon (16) + padding (8) + text + padding (8) + dropdown arrow space (16)
                            var dropdownSpace = (btn2.ButtonType == RibbonButtonType.DropDownButton || 
                                                 btn2.ButtonType == RibbonButtonType.SplitButton) ? 16 : 0;
                            buttonWidth = Math.Max(buttonWidth, 24 + textWidth + 8 + dropdownSpace);
                        }
                    }
                    control.Size = new Size(buttonWidth, mediumButtonHeight);
                    control.Location = new Point(x, y + (availableHeight - mediumButtonHeight) / 2);
                    x += control.Width + 2;
                }
                else if (control is RibbonButton btn3)
                {
                    // Large buttons - calculate width based on text content
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    // Calculate width based on text content
                    var buttonWidth = 50; // minimum width
                    var label = btn3.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        using (var g = CreateGraphics())
                        {
                            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                            {
                                var textWidth = (int)g.MeasureString(label, font).Width;
                                buttonWidth = Math.Max(buttonWidth, textWidth + 10); // text + padding
                            }
                        }
                    }
                    control.Size = new Size(buttonWidth, availableHeight);
                    control.Location = new Point(x, y);
                    x += control.Width + 2;
                }
                else if (control is RibbonComboBox || control is RibbonSpinner)
                {
                    // Comboboxes and spinners take their natural width
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    var comboHeight = 44;
                    control.Size = new Size(control.Width, Math.Min(comboHeight, availableHeight));
                    control.Location = new Point(x, y);
                    x += control.Width + 4;
                }
                else if (control is RibbonGallery gallery)
                {
                    // Galleries - set height to available space
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    
                    // Use gallery's preferred width (respects Columns property)
                    var galleryWidth = gallery.GetPreferredWidth();
                    System.Diagnostics.Debug.WriteLine($"RibbonGroup.LayoutControls: Gallery CommandId={gallery.CommandId}, Columns={gallery.Columns}, ItemWidth={gallery.ItemWidth}, PreferredWidth={galleryWidth}");
                    gallery.Size = new Size(galleryWidth, availableHeight);
                    control.Location = new Point(x, y);
                    x += galleryWidth + 2;  // Use the calculated width, not control.Width
                }
                else if (control is RibbonColorPicker)
                {
                    // Color pickers - small size
                    if (smallColumnStart < 0)
                    {
                        smallColumnStart = x;
                        smallRow = 0;
                    }

                    control.Size = new Size(smallButtonSize, smallButtonSize);
                    control.Location = new Point(smallColumnStart, y + smallRow * (smallButtonSize + 1));

                    smallRow++;
                    if (smallRow >= maxSmallRows)
                    {
                        smallColumnStart += smallButtonSize + 1;
                        smallRow = 0;
                    }
                    x = Math.Max(x, smallColumnStart + smallButtonSize + 2);
                }
                else
                {
                    // Other controls
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    control.Location = new Point(x, y);
                    x += control.Width + 2;
                }
            }
        }

        /// <summary>
        /// Layout for the "OneLargeComboSmall" SizeDefinition:
        /// One large button on the left, a compact dropdown and a medium button stacked on the right.
        /// </summary>
        private void LayoutOneLargeComboSmall(int availableHeight)
        {
            var x = PADDING;
            var y = PADDING;
            
            // Control 0: Large button (full height)
            if (_controls.Count > 0)
            {
                var largeButton = _controls[0];
                largeButton.CurrentSize = RibbonGroupSize.Large;
                
                // Calculate width based on text content
                var buttonWidth = 56; // default minimum width for large button
                var label = largeButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                        {
                            var textWidth = (int)g.MeasureString(label, font).Width;
                            buttonWidth = Math.Max(buttonWidth, textWidth + 10);
                        }
                    }
                }
                
                largeButton.Size = new Size(buttonWidth, availableHeight);
                largeButton.Location = new Point(x, y);
                x += largeButton.Width + 4;
            }
            
            // Controls 1 and 2: Stacked vertically on the right
            // Top: Blog selector dropdown, Bottom: Post draft button
            var rightColumnX = x;
            var topRowY = y;
            var rowHeight = (availableHeight - 4) / 2; // Split height evenly with gap
            var bottomRowY = y + rowHeight + 4;
            
            // Calculate right column width based on both controls
            var rightColumnWidth = 130; // Minimum for blog selector
            
            // Check dropdown's preferred width (from its configuration)
            if (_controls.Count > 1 && _controls[1] is RibbonGallery gallery)
            {
                rightColumnWidth = Math.Max(rightColumnWidth, gallery.Width);
            }
            
            // Also check the medium button's width requirement
            if (_controls.Count > 2 && _controls[2] is RibbonButton mediumButton)
            {
                var label = mediumButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        var textWidth = (int)g.MeasureString(label, SystemFonts.MenuFont).Width;
                        var dropdownSpace = (mediumButton.ButtonType == RibbonButtonType.DropDownButton || 
                                             mediumButton.ButtonType == RibbonButtonType.SplitButton) ? 16 : 0;
                        // 16px icon + 6px gap + text + 8px padding + dropdown arrow
                        rightColumnWidth = Math.Max(rightColumnWidth, 16 + 6 + textWidth + 8 + dropdownSpace);
                    }
                }
            }
            
            // Control 1: Compact dropdown (top of right column)
            if (_controls.Count > 1)
            {
                var dropdown = _controls[1];
                dropdown.Size = new Size(rightColumnWidth, rowHeight);
                dropdown.Location = new Point(rightColumnX, topRowY);
            }
            
            // Control 2: Medium button (bottom of right column)
            if (_controls.Count > 2)
            {
                var bottomButton = _controls[2];
                bottomButton.CurrentSize = RibbonGroupSize.Medium;
                bottomButton.Size = new Size(rightColumnWidth, rowHeight);
                bottomButton.Location = new Point(rightColumnX, bottomRowY);
            }
        }

        /// <summary>
        /// Layout for the "FourButtons" SizeDefinition:
        /// Four medium buttons arranged in a vertical stack (3 rows max height).
        /// </summary>
        private void LayoutFourButtons(int availableHeight)
        {
            var x = PADDING;
            var y = PADDING;
            
            // Calculate button width based on longest text
            var buttonWidth = 70; // minimum width
            using (var g = CreateGraphics())
            {
                foreach (var control in _controls)
                {
                    var label = control.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        var textWidth = (int)g.MeasureString(label, SystemFonts.MenuFont).Width;
                        // icon (16) + padding (6) + text + padding (4)
                        buttonWidth = Math.Max(buttonWidth, 16 + 6 + textWidth + 4);
                    }
                }
            }
            
            // Stack 3 buttons vertically, remaining space for 4th
            var rowHeight = (availableHeight - 4) / 3; // 3 rows with spacing
            var row = 0;
            
            foreach (var control in _controls)
            {
                if (!control.Visible) continue;
                
                control.CurrentSize = RibbonGroupSize.Medium;
                control.Size = new Size(buttonWidth, Math.Min(rowHeight - 2, 22));
                control.Location = new Point(x, y + row * rowHeight);
                row++;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw group background and separator
            RibbonRenderer.Instance.DrawGroup(e.Graphics, ClientRectangle, null);

            // If in popup mode, draw a dropdown button
            if (_isPopupMode)
            {
                var buttonBounds = new Rectangle(PADDING, PADDING,
                    Width - PADDING * 2, Height - LABEL_HEIGHT - PADDING * 2);

                // Get first control's image for the popup button
                Image image = null;
                foreach (var control in _controls)
                {
                    if (control is RibbonButton btn)
                    {
                        image = btn.CommandLargeImage;
                        break;
                    }
                }

                RibbonRenderer.Instance.DrawButton(e.Graphics, buttonBounds, _label, image,
                    Enabled, false, false, false, RibbonButtonType.DropDownButton, RibbonGroupSize.Large);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_isPopupMode && e.Button == MouseButtons.Left)
            {
                ShowPopup();
            }
        }

        private void ShowPopup()
        {
            if (_popupDropDown == null)
            {
                _popupDropDown = new ToolStripDropDown
                {
                    AutoSize = false,
                    LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow,
                    Padding = new Padding(4)
                };

                // Add controls as popup menu items
                foreach (var control in _controls)
                {
                    var host = new ToolStripControlHost(control)
                    {
                        AutoSize = false,
                        Size = new Size(120, 28)
                    };
                    _popupDropDown.Items.Add(host);
                }
            }

            _popupDropDown.Show(this, new Point(0, Height));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!_isPopupMode)
            {
                LayoutControls();
            }
        }
    }
}
