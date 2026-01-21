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
            control.CurrentSize = _currentSize;
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

            var width = PADDING * 2;

            if (_currentSize == RibbonGroupSize.Large)
            {
                // Large: controls side by side
                foreach (var control in _controls)
                {
                    if (!control.Visible) continue;
                    var controlWidth = GetControlWidth(control);
                    width += controlWidth + 2;
                }
            }
            else
            {
                // Medium/Small: controls stacked in columns of 3
                var controlCount = 0;
                foreach (var control in _controls)
                {
                    if (control.Visible) controlCount++;
                }
                var columns = (controlCount + 2) / 3; // ceiling division
                var controlWidth = _currentSize == RibbonGroupSize.Medium ? 22 : 22;
                width = columns * (controlWidth + 2) + PADDING * 2;
            }

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

                // Update control sizes
                foreach (var control in _controls)
                {
                    control.CurrentSize = _currentSize;
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

            var x = PADDING;
            var y = PADDING;
            var contentHeight = _contentPanel.Height;
            var availableHeight = Math.Max(contentHeight - PADDING * 2, 60);

            switch (_currentSize)
            {
                case RibbonGroupSize.Large:
                    // Horizontal layout, large buttons with icon above text
                    foreach (var control in _controls)
                    {
                        if (!control.Visible) continue;

                        if (control is RibbonButton)
                        {
                            control.Size = new Size(50, availableHeight);
                            control.Location = new Point(x, y);
                            x += control.Width + 2;
                        }
                        else if (control is RibbonSeparator)
                        {
                            control.Size = new Size(6, availableHeight);
                            control.Location = new Point(x, y);
                            x += control.Width;
                        }
                        else
                        {
                            control.Size = new Size(control.Width, availableHeight);
                            control.Location = new Point(x, y);
                            x += control.Width + 2;
                        }
                    }
                    break;

                case RibbonGroupSize.Medium:
                    // Vertical stack of medium buttons (3 rows)
                    var buttonHeight = Math.Max((availableHeight - 4) / 3, 20);
                    var row = 0;
                    foreach (var control in _controls)
                    {
                        if (!control.Visible) continue;

                        control.Location = new Point(x, y + row * (buttonHeight + 2));
                        control.Size = new Size(22, buttonHeight);

                        row++;
                        if (row >= 3)
                        {
                            row = 0;
                            x += 24;
                        }
                    }
                    break;

                case RibbonGroupSize.Small:
                    // Grid of small icon buttons (3 rows)
                    var smallSize = 22;
                    row = 0;
                    var col = 0;
                    foreach (var control in _controls)
                    {
                        if (!control.Visible) continue;

                        control.Location = new Point(x + col * (smallSize + 1), y + row * (smallSize + 1));
                        control.Size = new Size(smallSize, smallSize);

                        row++;
                        if (row >= 3)
                        {
                            row = 0;
                            col++;
                        }
                    }
                    break;
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
