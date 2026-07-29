// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
        // Use shared layout constants (accessed as properties for DPI scaling)
        private static int MIN_WIDTH => LayoutConstants.GroupMinWidth;
        private static int LABEL_HEIGHT => LayoutConstants.GroupLabelHeight;
        private static int PADDING => LayoutConstants.GroupPadding;

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
        private readonly Control _labelControl;  // TransparentSpacer for layout only

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
                Invalidate();  // Trigger repaint to update the label drawn by renderer
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
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            MinimumSize = new Size(MIN_WIDTH, 0);

            // Content panel with margin to keep controls away from the separator line
            // Use TransparentPanel for proper transparent background support
            // NOTE: Add content panel FIRST so that label control (added second) 
            // gets docked first and reserves its space at the bottom
            _contentPanel = new TransparentPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(PADDING, PADDING, PADDING + LayoutConstants.GroupSeparatorMargin, PADDING) // Extra right padding for separator
            };
            base.Controls.Add(_contentPanel);

            // Label spacer at bottom - reserves space for the label area
            // Uses a TransparentSpacer that doesn't paint, allowing the parent's
            // custom OnPaint (via RibbonRenderer.DrawGroup) to show through
            // NOTE: Add label SECOND so it's docked first (WinForms docks in reverse order)
            _labelControl = new TransparentSpacer
            {
                Dock = DockStyle.Bottom,
                Height = LABEL_HEIGHT
            };
            base.Controls.Add(_labelControl);
        }

        /// <summary>
        /// Adds a control to this group.
        /// </summary>
        public void AddControl(RibbonControlBase control)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));

            // Prevent duplicate controls from being added
            if (_controls.Contains(control))
                return;

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
                // Calculate popup width based on label text (like native ribbon)
                // Popup renders: 32x32 icon centered, small label text below, ▼ arrow at bottom
                // The width must fit the label text in a compact font
                var popupWidth = LayoutConstants.PopupWidth;
                if (!string.IsNullOrEmpty(_label))
                {
                    using (var g = CreateGraphics())
                    using (var font = new Font(SystemFonts.MenuFont.FontFamily, 7f))
                    {
                        var textWidth = (int)g.MeasureString(_label, font).Width;
                        // Popup width must fit the label text rendered by DrawGroup (3px padding each side)
                        var totalWidth = textWidth + 8;
                        popupWidth = Math.Max(popupWidth, totalWidth);
                    }
                }
                return popupWidth;
            }

            // Handle specific SizeDefinition layouts
            if (SizeDefinition == "OneLargeAndTwoSmall" && _controls.Count >= 3)
            {
                // At Medium, all 3 buttons become stacked medium buttons (icon + text)
                if (_currentSize == RibbonGroupSize.Medium || _currentSize == RibbonGroupSize.Small)
                    return GetStackedMediumButtonsWidth();
                return GetOneLargeAndTwoSmallWidth();
            }

            if (SizeDefinition == "OneLargeComboSmall" && _controls.Count >= 3)
            {
                return GetOneLargeComboSmallWidth();
            }

            if (SizeDefinition == "FourButtons" && _controls.Count >= 4)
            {
                return GetStackedMediumButtonsWidth();
            }

            if (SizeDefinition == "SevenSmallButtons" && _controls.Count >= 7)
            {
                return GetSevenSmallButtonsWidth();
            }

            if (SizeDefinition == "ThreeLargeButtons" && _controls.Count >= 3)
            {
                // At Medium, ThreeLargeButtons collapses to 3 stacked medium buttons
                if (_currentSize == RibbonGroupSize.Medium || _currentSize == RibbonGroupSize.Small)
                    return GetStackedMediumButtonsWidth();
                return GetNLargeButtonsWidth(3);
            }

            if (SizeDefinition == "TwoLargeButtons" && _controls.Count >= 2)
            {
                if (_currentSize == RibbonGroupSize.Medium || _currentSize == RibbonGroupSize.Small)
                    return GetStackedMediumButtonsWidth();
                return GetNLargeButtonsWidth(2);
            }

            if (SizeDefinition == "ThreeMediumButtons" && _controls.Count >= 3)
            {
                return GetStackedMediumButtonsWidth();
            }

            if (SizeDefinition == "FontGroup")
            {
                return GetFontGroupWidth();
            }

            // OneInRibbonGallery: at Medium, reduce gallery width to save space.
            // This matches native ribbon behavior where InRibbon galleries shrink at Medium.
            if (SizeDefinition == "OneInRibbonGallery" && _currentSize == RibbonGroupSize.Medium)
            {
                var galleryWidth = PADDING;
                foreach (var control in _controls)
                {
                    if (!control.Visible) continue;
                    if (control is RibbonGallery gallery)
                    {
                        // At medium, use 1 column (matching native MinColumnsMedium=1)
                        var cols = 1;
                        var itemWidth = gallery.ItemWidth;
                        galleryWidth += cols * itemWidth + 16 + 2 + 2; // scroll + border + spacing
                    }
                    else
                    {
                        galleryWidth += control.Width + 2;
                    }
                }
                galleryWidth += PADDING;
                using (var g = CreateGraphics())
                {
                    var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                    galleryWidth = Math.Max(galleryWidth, labelWidth);
                }
                return Math.Max(galleryWidth, MIN_WIDTH);
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
                    // Calculate width based on text content (large button)
                    var buttonWidth = LayoutConstants.LargeButtonMinWidth;
                    var label = btn2.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        using (var g = CreateGraphics())
                        {
                            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                            {
                                var textWidth = (int)g.MeasureString(label, font).Width;
                                var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                                if (textBasedWidth > 70)
                                {
                                    textBasedWidth = Math.Max(LayoutConstants.LargeButtonMinWidth, (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                                }
                                buttonWidth = Math.Max(buttonWidth, textBasedWidth);
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
        /// Gets whether this group can expand beyond its preferred width.
        /// True if the group contains an InRibbon gallery at Large size.
        /// </summary>
        public bool CanExpand
        {
            get
            {
                if (_currentSize != RibbonGroupSize.Large)
                    return false;

                foreach (var control in _controls)
                {
                    if (control is RibbonGallery gallery && gallery.GalleryType == RibbonGalleryType.InRibbon)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the maximum width this group could use, based on InRibbon galleries
        /// showing their maximum number of columns. For non-expandable groups, returns
        /// the same as GetPreferredWidth().
        /// </summary>
        public int GetMaxPreferredWidth()
        {
            if (!CanExpand)
                return GetPreferredWidth();

            // Recalculate width using the gallery's max preferred width
            var x = PADDING;
            foreach (var control in _controls)
            {
                if (!control.Visible) continue;

                if (control is RibbonGallery gallery)
                {
                    x += gallery.GetMaxPreferredWidth() + 2;
                }
                else
                {
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
                var buttonWidth = LayoutConstants.LargeButtonMinWidth;
                var label = largeButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                        {
                            var textWidth = (int)g.MeasureString(label, font).Width;
                            var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                            if (textBasedWidth > 70)
                            {
                                textBasedWidth = Math.Max(LayoutConstants.LargeButtonMinWidth, (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                            }
                            buttonWidth = Math.Max(buttonWidth, textBasedWidth);
                        }
                    }
                }
                x += buttonWidth + 4;
            }
            
            // Right column width (max of dropdown and button) - no cap, let scaling handle overflow
            var rightColumnWidth = 70; // default minimum

            // Check dropdown's preferred width (from its configuration)
            if (_controls.Count > 1 && _controls[1] is RibbonGallery gallery)
            {
                rightColumnWidth = Math.Max(rightColumnWidth, gallery.GetPreferredWidth());
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
                        var buttonWidth = Math.Min(130, 16 + 6 + textWidth + 8 + dropdownSpace);
                        rightColumnWidth = Math.Max(rightColumnWidth, buttonWidth);
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
        /// Calculate the preferred width for stacked medium buttons (FourButtons or ThreeMediumButtons).
        /// Layout: N medium buttons stacked vertically with 16x16 icon on left and text on right.
        /// </summary>
        private int GetStackedMediumButtonsWidth()
        {
            var buttonWidth = 48; // minimum width for medium buttons with text
            using (var g = CreateGraphics())
            {
                foreach (var control in _controls)
                {
                    var label = control.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        // Measure text width using TextRenderer for accurate GDI measurement
                        var textWidth = TextRenderer.MeasureText(g, label, SystemFonts.MenuFont).Width;
                        buttonWidth = Math.Max(buttonWidth, 2 + 16 + 2 + textWidth + 2);
                    }
                }
                
                var width = buttonWidth + PADDING * 2;
                var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                width = Math.Max(width, labelWidth);
                return Math.Max(width, MIN_WIDTH);
            }
        }

        /// <summary>
        /// Calculate the preferred width for N large buttons arranged horizontally.
        /// Used by ThreeLargeButtons (Insert group) and TwoLargeButtons (Plugins group).
        /// </summary>
        private int GetNLargeButtonsWidth(int maxButtons)
        {
            var x = PADDING;
            var buttonSpacing = 1;
            
            using (var g = CreateGraphics())
            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
            {
                var numButtons = Math.Min(maxButtons, _controls.Count);
                for (int i = 0; i < numButtons; i++)
                {
                    var control = _controls[i];
                    var buttonWidth = LayoutConstants.LargeButtonMinWidth;
                    var label = control.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        var textWidth = (int)g.MeasureString(label, font).Width;
                        var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                        if (textBasedWidth > 70)
                        {
                            textBasedWidth = Math.Max(LayoutConstants.LargeButtonMinWidth, 
                                (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                        }
                        buttonWidth = Math.Max(buttonWidth, textBasedWidth);
                    }
                    
                    if (control is RibbonButton btn && 
                        (btn.ButtonType == RibbonButtonType.DropDownButton || btn.ButtonType == RibbonButtonType.SplitButton))
                    {
                        buttonWidth = Math.Max(buttonWidth, LayoutConstants.LargeButtonMinWidth);
                    }
                    
                    x += buttonWidth;
                    if (i < numButtons - 1)
                        x += buttonSpacing;
                }
                
                x += PADDING;
                
                var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                x = Math.Max(x, labelWidth);
            }
            
            return Math.Max(x, MIN_WIDTH);
        }

        /// <summary>
        /// Calculate the preferred width for the "SevenSmallButtons" SizeDefinition.
        /// Layout: 7 small icon-only buttons in 2 rows (3 top, 4 bottom) matching native ribbon.
        /// Uses compact 20px buttons packed tight with no gaps.
        /// </summary>
        private int GetSevenSmallButtonsWidth()
        {
            var btnSize = (int)(LayoutConstants.SmallButtonSize * 16.0 / 22.0); // compact button size for paragraph toolbar
            var numColumns = 4;
            var width = PADDING + (btnSize * numColumns) + PADDING;

            // Ensure label fits
            using (var g = CreateGraphics())
            {
                var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                width = Math.Max(width, labelWidth);
            }

            return Math.Max(width, MIN_WIDTH);
        }

        /// <summary>
        /// Calculate the preferred width for the "OneLargeAndTwoSmall" SizeDefinition.
        /// Layout: One large button on left, two small buttons stacked on right.
        /// Compact layout for Clipboard group: reduced button width and gaps.
        /// </summary>
        private int GetOneLargeAndTwoSmallWidth()
        {
            var x = PADDING;

            // Large button width (first control) - use standard sizing to match native ribbon
            if (_controls.Count > 0)
            {
                var largeButton = _controls[0];
                var buttonWidth = LayoutConstants.LargeButtonMinWidth;
                var label = largeButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                        {
                            var textWidth = (int)g.MeasureString(label, font).Width;
                            var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                            if (textBasedWidth > 70)
                            {
                                textBasedWidth = Math.Max(LayoutConstants.LargeButtonMinWidth, (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                            }
                            buttonWidth = Math.Max(buttonWidth, textBasedWidth);
                        }
                    }
                }
                x += buttonWidth + LayoutConstants.ControlSpacing;
            }

            // Small buttons column width
            x += LayoutConstants.SmallButtonSize + PADDING;

            // Ensure label fits
            using (var g = CreateGraphics())
            {
                var labelWidth = (int)g.MeasureString(_label ?? "", _labelControl.Font).Width + PADDING * 2;
                x = Math.Max(x, labelWidth);
            }

            return Math.Max(x, MIN_WIDTH);
        }

        /// <summary>
        /// Calculate the preferred width for the "FontGroup" SizeDefinition.
        /// Layout: Font family dropdown (~95px) + Font size dropdown (~45px) on top row,
        /// formatting buttons (small icons) in columns below.
        /// </summary>
        private int GetFontGroupWidth()
        {
            // Count combo boxes and small buttons
            var comboBoxes = new List<RibbonComboBox>();
            var smallButtons = new List<RibbonControlBase>();

            foreach (var control in _controls)
            {
                if (control is RibbonComboBox combo)
                    comboBoxes.Add(combo);
                else
                    smallButtons.Add(control);
            }

            var smallButtonSize = LayoutConstants.SmallButtonSize;

            // Top row: comboboxes + first button (ClearFormatting/AA) side by side
            var topRowWidth = PADDING;
            for (int i = 0; i < comboBoxes.Count; i++)
            {
                topRowWidth += comboBoxes[i].Width + 1; // 1px gap between combos
            }
            // Add first button on combo row
            if (smallButtons.Count > 0)
                topRowWidth += smallButtonSize;
            topRowWidth += PADDING;

            // Bottom rows: remaining buttons arranged in 2 rows
            var gridButtonCount = Math.Max(0, smallButtons.Count - 1); // First button is on combo row
            var buttonRows = 2;
            var numButtonColumns = (gridButtonCount + buttonRows - 1) / buttonRows;
            var bottomRowWidth = PADDING + (numButtonColumns * smallButtonSize) + PADDING;

            var width = Math.Max(topRowWidth, bottomRowWidth);

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
                Width = GetPreferredWidth();  // Use dynamic popup width based on label
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
            // Account for content panel padding when calculating available height
            var availableHeight = Math.Max(contentHeight - PADDING * 2, 60);


            // Handle specific SizeDefinition layouts
            if (SizeDefinition == "OneLargeAndTwoSmall" && _controls.Count >= 3)
            {
                // At Medium, all 3 buttons become stacked medium buttons (icon + text)
                if (_currentSize == RibbonGroupSize.Medium || _currentSize == RibbonGroupSize.Small)
                {
                    LayoutStackedMediumButtons(availableHeight, 3);
                    return;
                }
                // Layout: Large button on left, two small buttons stacked vertically on right
                LayoutOneLargeAndTwoSmall(availableHeight);
                return;
            }

            if (SizeDefinition == "OneLargeComboSmall" && _controls.Count >= 3)
            {
                // Layout: Large button on left, dropdown and medium button stacked on right
                LayoutOneLargeComboSmall(availableHeight);
                return;
            }

            if (SizeDefinition == "FourButtons" && _controls.Count >= 4)
            {
                LayoutStackedMediumButtons(availableHeight, 4, maxButtonHeight: 20);
                return;
            }

            if (SizeDefinition == "SevenSmallButtons" && _controls.Count >= 7)
            {
                LayoutSevenSmallButtons(availableHeight);
                return;
            }

            if (SizeDefinition == "ThreeLargeButtons" && _controls.Count >= 3)
            {
                // At Medium/Small, ThreeLargeButtons collapses to 3 stacked medium buttons
                if (_currentSize == RibbonGroupSize.Medium || _currentSize == RibbonGroupSize.Small)
                {
                    LayoutStackedMediumButtons(availableHeight, 3);
                    return;
                }
                LayoutNLargeButtons(availableHeight, 3);
                return;
            }

            if (SizeDefinition == "TwoLargeButtons" && _controls.Count >= 2)
            {
                if (_currentSize == RibbonGroupSize.Medium || _currentSize == RibbonGroupSize.Small)
                {
                    LayoutStackedMediumButtons(availableHeight, 2);
                    return;
                }
                LayoutNLargeButtons(availableHeight, 2);
                return;
            }

            if (SizeDefinition == "ThreeMediumButtons" && _controls.Count >= 3)
            {
                LayoutStackedMediumButtons(availableHeight, 3);
                return;
            }

            if (SizeDefinition == "FontGroup")
            {
                // Layout: Font family and size dropdowns on top row, formatting buttons below
                LayoutFontGroup(availableHeight);
                return;
            }

            // Use a smarter layout that respects individual control sizes
            // Layout controls left-to-right, stacking small controls in columns
            // Account for content panel padding
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
                    // Ensure proper spacing: button height + 1px gap between rows
                    var buttonY = y + smallRow * (smallButtonSize + 1);
                    control.Location = new Point(smallColumnStart, buttonY);
                    control.BringToFront(); // Ensure proper z-order

                    smallRow++;
                    if (smallRow >= maxSmallRows)
                    {
                        // Move to next column: current column start + button width + gap
                        smallColumnStart += smallButtonSize + 1;
                        smallRow = 0;
                    }
                    // Update x to account for the rightmost column of small buttons
                    x = Math.Max(x, smallColumnStart + smallButtonSize + 1);
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
                    // Large buttons should be roughly square-shaped with 32x32 icon + 2-line text
                    var buttonWidth = LayoutConstants.LargeButtonMinWidth; // minimum width (fits 32px icon + margins)
                    var label = btn3.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        using (var g = CreateGraphics())
                        {
                            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                            {
                                var textWidth = (int)g.MeasureString(label, font).Width;
                                // Text can wrap to 2 lines, so calculate width assuming single line first
                                // then compare with icon-based minimum (32 + padding)
                                var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                                // Prefer wider buttons for better readability, but allow wrapping for long text
                                // If text would require > 70px width, assume it will wrap to 2 lines
                                if (textBasedWidth > 70)
                                {
                                    // Estimate 2-line width (roughly half the single-line width, plus padding)
                                    textBasedWidth = Math.Max(LayoutConstants.LargeButtonMinWidth, (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                                }
                                buttonWidth = Math.Max(buttonWidth, textBasedWidth);
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
                    // Galleries - expand to fill available space (allows more columns)
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    
                    // Use this.Width since _contentPanel.Width may not yet reflect
                    // the expanded group size from surplus distribution in LayoutGroups.
                    var effectiveContentWidth = this.Width - PADDING - LayoutConstants.GroupSeparatorMargin;
                    var availableGalleryWidth = effectiveContentWidth - x - PADDING;
                    var galleryWidth = Math.Max(gallery.GetPreferredWidth(), availableGalleryWidth);
                    galleryWidth = Math.Min(galleryWidth, Math.Max(1, effectiveContentWidth - x));
                    gallery.Size = new Size(galleryWidth, availableHeight);
                    control.Location = new Point(x, y);
                    x += galleryWidth + 2;
                }
                else if (control is RibbonColorPicker)
                {
                    // Color pickers - small size, stack like small buttons
                    if (smallColumnStart < 0)
                    {
                        smallColumnStart = x;
                        smallRow = 0;
                    }

                    control.Size = new Size(smallButtonSize, smallButtonSize);
                    // Ensure proper spacing: button height + 1px gap between rows
                    var pickerY = y + smallRow * (smallButtonSize + 1);
                    control.Location = new Point(smallColumnStart, pickerY);
                    control.BringToFront(); // Ensure proper z-order

                    smallRow++;
                    if (smallRow >= maxSmallRows)
                    {
                        // Move to next column: current column start + button width + gap
                        smallColumnStart += smallButtonSize + 1;
                        smallRow = 0;
                    }
                    // Update x to account for the rightmost column of small buttons
                    x = Math.Max(x, smallColumnStart + smallButtonSize + 1);
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

            // Ensure proper z-order: small and medium controls should be on top of large controls
            // This fixes visibility issues when controls might overlap due to layout calculations
            foreach (var control in _controls)
            {
                if (control is RibbonButton btn)
                {
                    if (btn.CurrentSize == RibbonGroupSize.Small || btn.CurrentSize == RibbonGroupSize.Medium)
                    {
                        btn.BringToFront();
                    }
                }
                else if (control is RibbonColorPicker || control is RibbonComboBox || control is RibbonSpinner)
                {
                    // These are typically smaller controls that should be visible on top
                    control.BringToFront();
                }
            }
        }

        /// <summary>
        /// Layout for the "OneLargeComboSmall" SizeDefinition:
        /// One large button on the left (full height, 32x32 icon centered with text below),
        /// a compact dropdown and a medium button stacked vertically on the right.
        /// 
        /// Expected layout:
        /// +------------------+------------------+
        /// |                  |  Blog Dropdown   |
        /// |  [32x32 Globe]   +------------------+
        /// |    Publish       | Post draft...    |
        /// +------------------+------------------+
        /// </summary>
        private void LayoutOneLargeComboSmall(int availableHeight)
        {
            // Account for content panel padding
            var x = PADDING;
            var y = PADDING;
            
            // Ensure minimum height for proper large button rendering (needs room for 32x32 icon + 2 lines of text)
            var contentHeight = Math.Max(availableHeight, LayoutConstants.LargeButtonMinHeight);
            
            // ===========================================
            // Control 0: Large button (full height on LEFT side)
            // Layout: 32x32 icon centered horizontally, positioned at top
            //         Text below icon, may wrap to 2 lines, centered horizontally
            // ===========================================
            if (_controls.Count > 0)
            {
                var largeButton = _controls[0];
                largeButton.CurrentSize = RibbonGroupSize.Large;
                
                // Calculate width: must fit 32x32 icon OR text (whichever is wider)
                var buttonWidth = LayoutConstants.LargeButtonMinWidth; // 54px min (32 icon + padding)
                var label = largeButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                        {
                            var textWidth = (int)g.MeasureString(label, font).Width;
                            var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                            // For long text, assume it wraps to 2 lines (half width each)
                            if (textBasedWidth > 70)
                            {
                                textBasedWidth = Math.Max(LayoutConstants.LargeButtonMinWidth, 
                                    (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                            }
                            buttonWidth = Math.Max(buttonWidth, textBasedWidth);
                        }
                    }
                }
                
                // Large button spans full available height
                largeButton.Size = new Size(buttonWidth, contentHeight);
                largeButton.Location = new Point(x, y);
                x += largeButton.Width + 4; // 4px gap before right column
            }
            
            // ===========================================
            // Controls 1 and 2: Stacked VERTICALLY on the RIGHT side
            // Layout: Two controls sharing the vertical space
            //   - Top: Blog selector dropdown (RibbonGallery in compact mode)
            //   - Bottom: "Post draft to blog" button (Medium size)
            // ===========================================
            var rightColumnX = x;
            var verticalGap = 4; // Gap between the two stacked controls
            
            // Calculate height for each control: split available height with gap
            // Both controls should have equal height for visual balance
            var controlHeight = (contentHeight - verticalGap) / 2;
            
            // Vertical positions for the stacked controls
            var topControlY = y;                              // Top control starts at same Y as large button
            var bottomControlY = y + controlHeight + verticalGap;  // Bottom control after top + gap
            
            // Calculate right column width (must fit both dropdown and button text)
            var rightColumnWidth = 100; // Minimum width for blog selector dropdown (compact to match native)
            
            // Check dropdown's preferred width (respects ItemWidth configuration)
            if (_controls.Count > 1 && _controls[1] is RibbonGallery gallery)
            {
                rightColumnWidth = Math.Max(rightColumnWidth, gallery.GetPreferredWidth());
            }
            
            // Check the medium button's width requirement
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
                        // Medium button: 16px icon + 6px gap + text + 8px padding + dropdown arrow
                        rightColumnWidth = Math.Max(rightColumnWidth, 16 + 6 + textWidth + 8 + dropdownSpace);
                    }
                }
            }
            
            // Expand right column to fill available group width (surplus from LayoutGroups)
            // Use this.Width since _contentPanel.Width may not yet reflect the new group size
            var effectiveContentWidth = this.Width - PADDING - LayoutConstants.GroupSeparatorMargin;
            var availableRightWidth = effectiveContentWidth - rightColumnX - PADDING;
            if (availableRightWidth > rightColumnWidth)
                rightColumnWidth = availableRightWidth;

            // Control 1: Compact dropdown (TOP of right column - blog selector)
            if (_controls.Count > 1)
            {
                var dropdown = _controls[1];
                dropdown.Size = new Size(rightColumnWidth, controlHeight);
                dropdown.Location = new Point(rightColumnX, topControlY);
                // Bring to front to ensure visibility
                dropdown.BringToFront();
            }
            
            // Control 2: Medium button (BOTTOM of right column - "Post draft to blog")
            if (_controls.Count > 2)
            {
                var bottomButton = _controls[2];
                bottomButton.CurrentSize = RibbonGroupSize.Medium;
                bottomButton.Size = new Size(rightColumnWidth, controlHeight);
                bottomButton.Location = new Point(rightColumnX, bottomControlY);
                // Bring to front to ensure visibility
                bottomButton.BringToFront();
            }
        }

        /// <summary>
        /// Layout for stacked medium buttons (FourButtons or ThreeMediumButtons).
        /// N medium buttons stacked vertically in a single column.
        /// Each button has a 16x16 icon on the left and text on the right.
        /// </summary>
        private void LayoutStackedMediumButtons(int availableHeight, int maxButtons, int maxButtonHeight = 22)
        {
            var x = PADDING;
            var y = PADDING;

            // Use available content panel width so buttons don't overflow the group
            var buttonWidth = _contentPanel.Width - PADDING * 2;
            
            var buttonCount = Math.Min(_controls.Count, maxButtons);
            var buttonGap = 1;
            var totalButtonHeight = availableHeight - ((buttonCount - 1) * buttonGap);
            var buttonHeight = Math.Max(14, Math.Min(totalButtonHeight / buttonCount, maxButtonHeight));
            
            var row = 0;
            foreach (var control in _controls)
            {
                if (!control.Visible) continue;
                if (row >= maxButtons) break;
                
                control.CurrentSize = RibbonGroupSize.Medium;
                control.Size = new Size(buttonWidth, buttonHeight);
                control.Location = new Point(x, y + row * (buttonHeight + buttonGap));
                row++;
            }
        }

        /// <summary>
        /// Layout for N large buttons arranged horizontally side by side.
        /// Each button has full height with a 32x32 icon at top and text below.
        /// Used by ThreeLargeButtons (Insert) and TwoLargeButtons (Plugins).
        /// </summary>
        private void LayoutNLargeButtons(int availableHeight, int maxButtons)
        {
            var x = PADDING;
            var y = PADDING;
            var buttonSpacing = 1;
            var contentHeight = Math.Max(availableHeight, LayoutConstants.LargeButtonMinHeight);
            
            using (var g = CreateGraphics())
            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
            {
                for (int i = 0; i < Math.Min(maxButtons, _controls.Count); i++)
                {
                    var control = _controls[i];
                    
                    if (!control.Visible)
                        control.Visible = true;
                    
                    control.CurrentSize = RibbonGroupSize.Large;
                    
                    var buttonWidth = LayoutConstants.LargeButtonMinWidth;
                    var label = control.CommandLabel;
                    if (!string.IsNullOrEmpty(label))
                    {
                        var textWidth = (int)g.MeasureString(label, font).Width;
                        var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                        if (textBasedWidth > 70)
                        {
                            textBasedWidth = Math.Max(LayoutConstants.LargeButtonMinWidth, 
                                (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                        }
                        buttonWidth = Math.Max(buttonWidth, textBasedWidth);
                    }
                    
                    if (control is RibbonButton btn && 
                        (btn.ButtonType == RibbonButtonType.DropDownButton || btn.ButtonType == RibbonButtonType.SplitButton))
                    {
                        buttonWidth = Math.Max(buttonWidth, LayoutConstants.LargeButtonMinWidth);
                    }
                    
                    control.Size = new Size(buttonWidth, contentHeight);
                    control.Location = new Point(x, y);
                    control.BringToFront();
                    
                    x += buttonWidth + buttonSpacing;
                }
            }
        }

        /// <summary>
        /// Layout for the "SevenSmallButtons" SizeDefinition:
        /// Seven small icon-only buttons (22x22) in 2 rows matching native ribbon.
        /// Row 0: 3 buttons (Bullets, Numbers, Blockquote)
        /// Row 1: 4 buttons (AlignLeft, AlignCenter, AlignRight, Justify)
        /// </summary>
        private void LayoutSevenSmallButtons(int availableHeight)
        {
            var x = PADDING;
            var y = PADDING;
            var smallButtonSize = (int)(LayoutConstants.SmallButtonSize * 16.0 / 22.0); // compact size for paragraph toolbar

            // 2 rows, center vertically
            var totalHeight = smallButtonSize * 2;
            var startY = y + Math.Max(0, (availableHeight - totalHeight) / 2);

            var visibleButtons = new List<RibbonControlBase>();
            for (var i = 0; i < Math.Min(_controls.Count, 7); i++)
            {
                if (_controls[i].Visible)
                    visibleButtons.Add(_controls[i]);
            }

            // Row 0: first 3 buttons, Row 1: remaining buttons (matches native
            // SevenSmallButtons: bullets/numbers/blockquote on top, alignment on bottom)
            for (var i = 0; i < visibleButtons.Count; i++)
            {
                var control = visibleButtons[i];
                control.CurrentSize = RibbonGroupSize.Small;
                control.Size = new Size(smallButtonSize, smallButtonSize);

                int column, row;
                if (i < 3)
                {
                    row = 0;
                    column = i;
                }
                else
                {
                    row = 1;
                    column = i - 3;
                }

                var columnX = x + column * smallButtonSize;
                var rowY = startY + row * smallButtonSize;

                control.Location = new Point(columnX, rowY);
                control.BringToFront();
            }
        }

        /// <summary>
        /// Layout for the "OneLargeAndTwoSmall" SizeDefinition:
        /// One large button on the left (full height, 32x32 icon, text below),
        /// two small buttons stacked vertically on the right (icon-only, 22x22 each).
        /// Compact layout for Clipboard group: reduced button width and gaps.
        /// </summary>
        private void LayoutOneLargeAndTwoSmall(int availableHeight)
        {
            // Account for content panel padding
            var x = PADDING;
            var y = PADDING;

            // Compact large button minimum width for Clipboard group (reduced from 54px to 42px)
            const int CompactLargeButtonMinWidth = 42;
            
            // Control 0: Large button (full height) on the left
            if (_controls.Count > 0)
            {
                var largeButton = _controls[0];
                largeButton.CurrentSize = RibbonGroupSize.Large;
                
                // Calculate width based on text content
                var buttonWidth = CompactLargeButtonMinWidth;
                var label = largeButton.CommandLabel;
                if (!string.IsNullOrEmpty(label))
                {
                    using (var g = CreateGraphics())
                    {
                        using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f))
                        {
                            var textWidth = (int)g.MeasureString(label, font).Width;
                            var textBasedWidth = textWidth + LayoutConstants.LargeButtonTextPadding * 2;
                            if (textBasedWidth > 70)
                            {
                                textBasedWidth = Math.Max(CompactLargeButtonMinWidth, (textWidth / 2) + LayoutConstants.LargeButtonTextPadding * 2);
                            }
                            buttonWidth = Math.Max(buttonWidth, textBasedWidth);
                        }
                    }
                }
                
                largeButton.Size = new Size(buttonWidth, availableHeight);
                largeButton.Location = new Point(x, y);
                // Send large button to back so small buttons render on top
                largeButton.SendToBack();
                // Reduced gap from 2px to 1px for compact layout
                x += largeButton.Width + 1;
            }
            
            // Controls 1 and 2: Small buttons stacked vertically on the right
            var smallButtonSize = LayoutConstants.SmallButtonSize;
            var totalSmallHeight = smallButtonSize * 2 + 1;
            var smallStartY = y + (availableHeight - totalSmallHeight) / 2;

            // Control 1: First small button (top)
            if (_controls.Count > 1)
            {
                var smallButton1 = _controls[1];
                smallButton1.CurrentSize = RibbonGroupSize.Small;
                smallButton1.Size = new Size(smallButtonSize, smallButtonSize);
                smallButton1.Location = new Point(x, smallStartY);
                smallButton1.BringToFront();
            }

            // Control 2: Second small button (bottom)
            if (_controls.Count > 2)
            {
                var smallButton2 = _controls[2];
                smallButton2.CurrentSize = RibbonGroupSize.Small;
                smallButton2.Size = new Size(smallButtonSize, smallButtonSize);
                smallButton2.Location = new Point(x, smallStartY + smallButtonSize + 1);
                smallButton2.BringToFront();
            }
            
        }

        /// <summary>
        /// Layout for the "FontGroup" SizeDefinition:
        /// Font formatting controls arranged similar to the original Windows Ribbon Framework.
        /// 
        /// Layout:
        /// +--------------------------+------+
        /// | Font Family (~95px)      | Size |  <- Top row: dropdowns (no labels)
        /// +---+---+---+---+---+---+--+--45px-+
        /// | X | B | I | U | S | x2|^2| Bg| Fg|  <- Bottom rows: formatting buttons
        /// +---+---+---+---+---+---+--+---+---+
        /// 
        /// Where: X=Clear, B=Bold, I=Italic, U=Underline, S=Strikethrough, 
        ///        x2=Subscript, ^2=Superscript, Bg=Background, Fg=Font Color
        /// </summary>
        private void LayoutFontGroup(int availableHeight)
        {
            // Account for content panel padding
            var x = PADDING;
            var y = PADDING;
            var smallButtonSize = LayoutConstants.SmallButtonSize;
            var comboHeight = 23; // Compact combo height (no label)
            var verticalGap = 1; // Compact gap between combo row and button rows
            
            // Separate controls into combo boxes and buttons
            var comboBoxes = new List<RibbonComboBox>();
            var buttons = new List<RibbonControlBase>();
            
            foreach (var control in _controls)
            {
                if (control is RibbonComboBox combo)
                    comboBoxes.Add(combo);
                else
                    buttons.Add(control);
            }
            
            // ===========================================
            // Top Row: Font dropdowns + ClearFormatting button (matches native ribbon)
            // Native layout: [FontFamily combo] [FontSize combo] [AA button]
            // ===========================================
            var comboX = x;
            for (int i = 0; i < comboBoxes.Count; i++)
            {
                var combo = comboBoxes[i];
                // Hide the label for compact font group layout
                combo.ShowLabel = false;
                combo.Size = new Size(combo.Width, comboHeight);
                combo.Location = new Point(comboX, y);
                comboX += combo.Width + 1;
            }

            // Place first button (ClearFormatting/AA) on combo row, right after combos
            var gridButtons = new List<RibbonControlBase>();
            bool firstButtonPlaced = false;
            foreach (var control in buttons)
            {
                if (!control.Visible) continue;
                if (!firstButtonPlaced)
                {
                    // Place first button on combo row
                    control.CurrentSize = RibbonGroupSize.Small;
                    control.Size = new Size(smallButtonSize, comboHeight);
                    control.Location = new Point(comboX, y);
                    control.BringToFront();
                    firstButtonPlaced = true;
                }
                else
                {
                    gridButtons.Add(control);
                }
            }

            // ===========================================
            // Bottom Rows: Remaining formatting buttons in grid
            // Arranged row-first, columns based on available width (matches native ribbon)
            // ===========================================
            var buttonStartY = y + comboHeight + verticalGap;
            var availWidth = _contentPanel.Width - PADDING * 2;
            var numButtonCols = Math.Max(1, availWidth / smallButtonSize);
            var buttonCol = 0;
            var buttonRow = 0;

            foreach (var control in gridButtons)
            {
                if (!control.Visible) continue;

                control.CurrentSize = RibbonGroupSize.Small;
                control.Size = new Size(smallButtonSize, smallButtonSize);

                // Calculate position ensuring no overlap (no gap between buttons)
                var buttonX = x + buttonCol * smallButtonSize;
                var buttonY = buttonStartY + buttonRow * smallButtonSize;

                control.Location = new Point(buttonX, buttonY);
                control.BringToFront(); // Ensure proper z-order

                // Row-first: fill left-to-right, then move to next row
                buttonCol++;
                if (buttonCol >= numButtonCols)
                {
                    buttonCol = 0;
                    buttonRow++;
                }
            }
        }

        /// <summary>
        /// Override to initialize the double buffer with a proper background color.
        /// With OptimizedDoubleBuffer, we need to fill the buffer to prevent black
        /// showing through gaps in OnPaint rendering.
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Fill with opaque group background to prevent black from double buffer
            e.Graphics.Clear(RibbonColors.Current.GetOpaqueGroupBackground());
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw group background, separator, and label
            // Use ClientRectangle to paint the entire control area including label area
            // Note: TransparentSpacer doesn't paint, so parent's paint shows through naturally
            RibbonRenderer.Instance.DrawGroup(e.Graphics, ClientRectangle, _label);

            // If in popup mode, draw a compact popup button matching native ribbon style
            if (_isPopupMode)
            {
                var buttonBounds = new Rectangle(PADDING, PADDING,
                    Width - PADDING * 2, Height - LABEL_HEIGHT - PADDING * 2);

                // Get the group's own command image for the popup button
                Image image = null;
                var groupCommand = _commandManager?.GetCommand(_commandId);
                if (groupCommand != null)
                {
                    image = groupCommand.LargeImage;
                }

                // Fall back to first button's image if group command has no icon
                if (image == null)
                {
                    foreach (var control in _controls)
                    {
                        if (control is RibbonButton btn)
                        {
                            image = btn.DisplayLargeImage;
                            break;
                        }
                    }
                }

                // Draw popup content: centered icon + small ▼ arrow
                // Label is already rendered in the group label area by DrawGroup
                // Use DPI-scaled icon size to match native ribbon popup appearance
                var imageSize = LayoutConstants.LargeImageSize; // DPI-scaled 32px
                var imageX = buttonBounds.X + (buttonBounds.Width - imageSize) / 2;
                var imageY = buttonBounds.Y + (buttonBounds.Height - imageSize - 8) / 2;

                if (image != null)
                {
                    var imageBounds = new Rectangle(imageX, imageY, imageSize, imageSize);
                    if (Enabled)
                        RibbonRenderer.Instance.DrawScaledImage(e.Graphics, image, imageBounds);
                    else
                        RibbonRenderer.Instance.DrawDisabledImage(e.Graphics, image, imageBounds);
                }

                // Draw small ▼ arrow below icon
                var arrowY = imageY + imageSize + 2;
                var arrowX = buttonBounds.X + buttonBounds.Width / 2;
                var arrowColor = Enabled ? RibbonColors.Current.ButtonText : RibbonColors.Current.ButtonTextDisabled;
                using (var brush = new SolidBrush(arrowColor))
                {
                    var arrowPoints = new Point[]
                    {
                        new Point(arrowX - 3, arrowY),
                        new Point(arrowX + 3, arrowY),
                        new Point(arrowX, arrowY + 3)
                    };
                    e.Graphics.FillPolygon(brush, arrowPoints);
                }
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
