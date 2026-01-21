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
        private const int MIN_WIDTH = 52;
        private const int LABEL_HEIGHT = 18;
        private const int PADDING = 3;
        private const int SEPARATOR_WIDTH = 1;

        private RibbonCommandManager _commandManager;
        private CommandId _commandId;
        private string _label;
        private string _keytip;
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
                return 48; // Collapsed popup button width
            }

            // Calculate width by simulating the layout
            var x = PADDING;
            var smallButtonSize = 22;
            var smallColumnStart = -1;
            var smallRow = 0;
            var maxSmallRows = 3;

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

        private int GetControlWidth(RibbonControlBase control)
        {
            switch (_currentSize)
            {
                case RibbonGroupSize.Large:
                    return control is RibbonButton ? 50 : (control is RibbonSeparator ? 6 : control.Width);
                case RibbonGroupSize.Medium:
                    return 22;
                case RibbonGroupSize.Small:
                    return 22;
                default:
                    return control.Width;
            }
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

            // Use a smarter layout that respects individual control sizes
            // Layout controls left-to-right, stacking small controls in columns
            var x = PADDING;
            var y = PADDING;
            var smallButtonSize = 22;
            var mediumButtonHeight = 24;
            var smallColumnStart = -1; // Track where small button column starts
            var smallRow = 0;
            var maxSmallRows = 3;

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
                    // Galleries
                    if (smallColumnStart >= 0)
                    {
                        x = smallColumnStart + smallButtonSize + 2;
                        smallColumnStart = -1;
                        smallRow = 0;
                    }
                    control.Location = new Point(x, y);
                    x += control.Width + 2;
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
