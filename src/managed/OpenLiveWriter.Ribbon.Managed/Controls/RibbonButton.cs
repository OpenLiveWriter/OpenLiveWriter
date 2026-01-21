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
    /// Ribbon button control supporting multiple button types.
    /// </summary>
    public class RibbonButton : RibbonControlBase
    {
        private RibbonButtonType _buttonType = RibbonButtonType.Button;
        private bool _isHovered;
        private bool _isPressed;
        private bool _isDropDownHovered;
        private bool _isDropDownPressed;

        private readonly List<RibbonMenuItem> _menuItems = new List<RibbonMenuItem>();
        private ContextMenuStrip _dropDownMenu;

        private Rectangle _buttonBounds;
        private Rectangle _dropDownBounds;

        /// <summary>
        /// Gets or sets the button type.
        /// </summary>
        public RibbonButtonType ButtonType
        {
            get => _buttonType;
            set
            {
                _buttonType = value;
                UpdateSize();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the label for this button (overrides command label).
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the large image (overrides command image).
        /// </summary>
        public Image LargeImage { get; set; }

        /// <summary>
        /// Gets or sets the small image (overrides command image).
        /// </summary>
        public Image SmallImage { get; set; }

        /// <summary>
        /// Gets the label to display (uses override or command label).
        /// </summary>
        private string DisplayLabel => Label ?? CommandLabel;

        /// <summary>
        /// Gets the large image to display.
        /// </summary>
        internal new Image CommandLargeImage => LargeImage ?? base.CommandLargeImage;

        /// <summary>
        /// Gets the small image to display.
        /// </summary>
        private Image DisplaySmallImage => SmallImage ?? base.CommandSmallImage;

        /// <summary>
        /// Gets the menu items for split/dropdown buttons.
        /// </summary>
        public List<RibbonMenuItem> MenuItems => _menuItems;

        /// <summary>
        /// Occurs when the button is clicked (not dropdown).
        /// </summary>
        public event EventHandler ButtonClick;

        /// <summary>
        /// Occurs when a menu item is clicked.
        /// </summary>
        public event EventHandler<MenuItemClickEventArgs> MenuItemClick;

        public RibbonButton()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();

            // Size will be set by the parent group's layout
            // These are just default/minimum sizes
            switch (CurrentSize)
            {
                case RibbonGroupSize.Large:
                    MinimumSize = new Size(40, 60);
                    break;
                case RibbonGroupSize.Medium:
                    MinimumSize = new Size(22, 20);
                    break;
                case RibbonGroupSize.Small:
                    MinimumSize = new Size(22, 22);
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var image = CurrentSize == RibbonGroupSize.Large ? CommandLargeImage : DisplaySmallImage;

            // Calculate bounds
            CalculateBounds();

            // Draw main button part
            var mainHovered = _isHovered && !_isDropDownHovered;
            var mainPressed = _isPressed && !_isDropDownPressed;

            RibbonRenderer.Instance.DrawButton(g, _buttonBounds, DisplayLabel, image,
                Enabled && CommandEnabled, mainHovered, mainPressed, CommandChecked,
                _buttonType == RibbonButtonType.DropDownButton ? RibbonButtonType.DropDownButton : RibbonButtonType.Button,
                CurrentSize);

            // Draw dropdown part for split buttons
            if (_buttonType == RibbonButtonType.SplitButton && CurrentSize != RibbonGroupSize.Small)
            {
                DrawSplitButtonSeparator(g);
                DrawDropDownPart(g);
            }
        }

        private void CalculateBounds()
        {
            if (_buttonType == RibbonButtonType.SplitButton)
            {
                if (CurrentSize == RibbonGroupSize.Large)
                {
                    _buttonBounds = new Rectangle(0, 0, Width, Height - 18);
                    _dropDownBounds = new Rectangle(0, Height - 18, Width, 18);
                }
                else if (CurrentSize == RibbonGroupSize.Medium)
                {
                    _buttonBounds = new Rectangle(0, 0, Width - 16, Height);
                    _dropDownBounds = new Rectangle(Width - 16, 0, 16, Height);
                }
                else
                {
                    // Small - no split
                    _buttonBounds = ClientRectangle;
                    _dropDownBounds = Rectangle.Empty;
                }
            }
            else
            {
                _buttonBounds = ClientRectangle;
                _dropDownBounds = Rectangle.Empty;
            }
        }

        private void DrawSplitButtonSeparator(Graphics g)
        {
            if (_dropDownBounds.IsEmpty) return;

            using (var pen = new Pen(RibbonColors.Current.GroupSeparator))
            {
                if (CurrentSize == RibbonGroupSize.Large)
                {
                    // Horizontal separator
                    g.DrawLine(pen, _dropDownBounds.Left + 4, _dropDownBounds.Top,
                        _dropDownBounds.Right - 4, _dropDownBounds.Top);
                }
                else
                {
                    // Vertical separator
                    g.DrawLine(pen, _dropDownBounds.Left, _dropDownBounds.Top + 4,
                        _dropDownBounds.Left, _dropDownBounds.Bottom - 4);
                }
            }
        }

        private void DrawDropDownPart(Graphics g)
        {
            if (_dropDownBounds.IsEmpty) return;

            // Background
            Color backColor = Color.Transparent;
            Color borderColor = Color.Transparent;

            if (!Enabled || !CommandEnabled)
            {
                // Disabled
            }
            else if (_isDropDownPressed)
            {
                backColor = RibbonColors.Current.ButtonBackgroundPressed;
                borderColor = RibbonColors.Current.ButtonBorderPressed;
            }
            else if (_isDropDownHovered)
            {
                backColor = RibbonColors.Current.ButtonBackgroundHover;
                borderColor = RibbonColors.Current.ButtonBorderHover;
            }

            if (backColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, _dropDownBounds);
                }
            }

            // Arrow
            var arrowColor = Enabled && CommandEnabled ?
                RibbonColors.Current.ButtonText : RibbonColors.Current.ButtonTextDisabled;

            var arrowSize = 5;
            var arrowX = _dropDownBounds.X + (_dropDownBounds.Width - arrowSize) / 2;
            var arrowY = _dropDownBounds.Y + (_dropDownBounds.Height - arrowSize / 2) / 2;

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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            CalculateBounds();

            var wasHovered = _isHovered;
            var wasDropDownHovered = _isDropDownHovered;

            _isHovered = _buttonBounds.Contains(e.Location);
            _isDropDownHovered = _dropDownBounds.Contains(e.Location);

            if (wasHovered != _isHovered || wasDropDownHovered != _isDropDownHovered)
            {
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isHovered || _isDropDownHovered)
            {
                _isHovered = false;
                _isDropDownHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) return;

            CalculateBounds();

            if (_buttonBounds.Contains(e.Location))
            {
                _isPressed = true;
                Invalidate();
            }
            else if (_dropDownBounds.Contains(e.Location))
            {
                _isDropDownPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left) return;

            CalculateBounds();

            var wasPressed = _isPressed;
            var wasDropDownPressed = _isDropDownPressed;

            _isPressed = false;
            _isDropDownPressed = false;

            if (wasPressed && _buttonBounds.Contains(e.Location))
            {
                OnButtonClick();
            }
            else if (wasDropDownPressed && _dropDownBounds.Contains(e.Location))
            {
                ShowDropDown();
            }

            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            // Don't call base.OnClick - we handle everything in OnMouseUp for proper split button support
            // base.OnClick(e);

            // Only handle non-split button types here (for keyboard/accessibility support)
            if (_buttonType == RibbonButtonType.DropDownButton)
            {
                ShowDropDown();
            }
            else if (_buttonType == RibbonButtonType.Button || _buttonType == RibbonButtonType.ToggleButton)
            {
                // This is only called for keyboard activation (Enter/Space)
                // Mouse clicks are handled in OnMouseUp
            }
        }

        private void OnButtonClick()
        {
            if (!Enabled || !CommandEnabled) return;

            // Toggle state for toggle buttons
            if (_buttonType == RibbonButtonType.ToggleButton)
            {
                var command = CommandManager?.GetCommand(CommandId);
                if (command != null)
                {
                    command.Checked = !command.Checked;
                }
            }

            ButtonClick?.Invoke(this, EventArgs.Empty);
            ExecuteCommand();
        }

        private void ShowDropDown()
        {
            if (_menuItems.Count == 0) return;

            if (_dropDownMenu == null)
            {
                _dropDownMenu = new ContextMenuStrip();
                _dropDownMenu.Opening += (s, e) => { _isDropDownPressed = true; Invalidate(); };
                _dropDownMenu.Closed += (s, e) => { _isDropDownPressed = false; Invalidate(); };

                foreach (var item in _menuItems)
                {
                    if (item.IsSeparator)
                    {
                        _dropDownMenu.Items.Add(new ToolStripSeparator());
                    }
                    else
                    {
                        var menuItem = new ToolStripMenuItem(item.Label, item.Image);
                        menuItem.Tag = item;
                        menuItem.Click += (s, e) =>
                        {
                            var mi = (ToolStripMenuItem)s;
                            var ribbonItem = (RibbonMenuItem)mi.Tag;
                            MenuItemClick?.Invoke(this, new MenuItemClickEventArgs(ribbonItem));

                            if (ribbonItem.CommandId != Localization.CommandId.None)
                            {
                                CommandManager?.Execute(ribbonItem.CommandId);
                            }
                        };
                        _dropDownMenu.Items.Add(menuItem);
                    }
                }
            }

            _dropDownMenu.Show(this, new Point(0, Height));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                if (_buttonType == RibbonButtonType.DropDownButton)
                {
                    ShowDropDown();
                }
                else
                {
                    OnButtonClick();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Simulates a click on the button.
        /// </summary>
        public override void PerformClick()
        {
            if (_buttonType == RibbonButtonType.DropDownButton)
            {
                ShowDropDown();
            }
            else
            {
                OnButtonClick();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dropDownMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Represents a menu item in a dropdown menu.
    /// </summary>
    public class RibbonMenuItem
    {
        public OpenLiveWriter.Localization.CommandId CommandId { get; set; }
        public string Label { get; set; }
        public Image Image { get; set; }
        public bool IsSeparator { get; set; }
    }

    /// <summary>
    /// Event args for menu item clicks.
    /// </summary>
    public class MenuItemClickEventArgs : EventArgs
    {
        public RibbonMenuItem MenuItem { get; }

        public MenuItemClickEventArgs(RibbonMenuItem menuItem)
        {
            MenuItem = menuItem;
        }
    }
}
