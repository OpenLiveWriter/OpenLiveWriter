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
        private DropDownMouseHook _mouseHook;

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
        /// Gets the large image to display (32x32).
        /// Uses button-specific override, then command's large image, then scaled small image.
        /// </summary>
        public Image DisplayLargeImage
        {
            get
            {
                // Priority: 1) Button-specific override, 2) Command's large image, 3) Command's small image (scaled)
                return LargeImage ?? base.CommandLargeImage ?? base.CommandSmallImage;
            }
        }

        /// <summary>
        /// Gets the small image to display (16x16).
        /// Uses button-specific override, then command's small image, then scaled large image.
        /// </summary>
        public Image DisplaySmallImage
        {
            get
            {
                // Priority: 1) Button-specific override, 2) Command's small image, 3) Command's large image (scaled)
                return SmallImage ?? base.CommandSmallImage ?? base.CommandLargeImage;
            }
        }

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
            
            // Enable accessibility for UI Automation
            AccessibleRole = AccessibleRole.PushButton;
            AccessibleDefaultActionDescription = "Click";
        }

        /// <summary>
        /// Updates accessibility properties when command changes.
        /// </summary>
        protected override void UpdateFromCommand()
        {
            base.UpdateFromCommand();
            
            // Update AccessibleName to match the button label for UI Automation
            // Strip accelerator characters (&) for clean accessible names
            var label = DisplayLabel;
            if (!string.IsNullOrEmpty(label))
            {
                AccessibleName = RibbonRenderer.StripAccelerator(label);
            }
            else if (CommandId != Localization.CommandId.None)
            {
                AccessibleName = CommandId.ToString();
            }
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();

            // Size will be set by the parent group's layout
            // These are minimum sizes based on Windows Ribbon specifications:
            // - Large: 32x32 icon + padding + 2 lines of text (~24px) = ~66px height minimum
            // - Medium: 16x16 icon + text on right, typically 22-24px height
            // - Small: 16x16 icon only, 22x22 minimum
            switch (CurrentSize)
            {
                case RibbonGroupSize.Large:
                    // Width needs to fit icon (32) + margins, and 2-line text
                    // Height: 3px top padding + 32px icon + 2px gap + ~26px for 2 lines of 8pt text + 3px bottom
                    MinimumSize = new Size(LayoutConstants.LargeButtonMinWidth, LayoutConstants.LargeButtonMinHeight);
                    break;
                case RibbonGroupSize.Medium:
                    // Height matches LayoutConstants.MediumButtonHeight (24)
                    MinimumSize = new Size(LayoutConstants.MediumButtonMinWidth, LayoutConstants.MediumButtonHeight);
                    break;
                case RibbonGroupSize.Small:
                    // 16x16 icon + 3px padding each side
                    MinimumSize = new Size(LayoutConstants.SmallButtonSize, LayoutConstants.SmallButtonSize);
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var image = CurrentSize == RibbonGroupSize.Large ? DisplayLargeImage : DisplaySmallImage;

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
                    // For large split buttons, the dropdown area is below the icon+text
                    // Split at roughly where the text ends: icon (32) + gap + text area
                    // The split line should be below the 2-line text area
                    // Top portion: icon + text (~55px), bottom: dropdown arrow (~11px)
                    var splitY = Height - 14;
                    _buttonBounds = new Rectangle(0, 0, Width, splitY);
                    _dropDownBounds = new Rectangle(0, splitY, Width, Height - splitY);
                }
                else if (CurrentSize == RibbonGroupSize.Medium)
                {
                    // For medium split buttons, dropdown is on the right side
                    var dropdownWidth = 14;
                    _buttonBounds = new Rectangle(0, 0, Width - dropdownWidth, Height);
                    _dropDownBounds = new Rectangle(Width - dropdownWidth, 0, dropdownWidth, Height);
                }
                else
                {
                    // Small - no split capability
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

            // Only show separator on hover/pressed — native ribbon hides it in normal state
            if (!_isHovered && !_isDropDownHovered && !_isPressed && !_isDropDownPressed)
                return;

            using (var pen = new Pen(RibbonColors.Current.ButtonBorderHover))
            {
                if (CurrentSize == RibbonGroupSize.Large)
                {
                    // Horizontal separator line above dropdown arrow area
                    var margin = 6;
                    g.DrawLine(pen, _dropDownBounds.Left + margin, _dropDownBounds.Top,
                        _dropDownBounds.Right - margin, _dropDownBounds.Top);
                }
                else
                {
                    // Vertical separator line between button and dropdown
                    var margin = 4;
                    g.DrawLine(pen, _dropDownBounds.Left, _dropDownBounds.Top + margin,
                        _dropDownBounds.Left, _dropDownBounds.Bottom - margin);
                }
            }
        }

        private void DrawDropDownPart(Graphics g)
        {
            if (_dropDownBounds.IsEmpty) return;

            // Background - draw highlight when hovered/pressed
            Color backColor = Color.Transparent;
            Color borderColor = Color.Transparent;

            if (!Enabled || !CommandEnabled)
            {
                // Disabled - no highlight
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

            // ALWAYS fill the dropdown area to prevent black from showing
            // When backColor is Transparent (normal state), use the group background
            var fillColor = backColor.A < 255
                ? RibbonColors.Current.GetOpaqueGroupBackground()
                : backColor;
            using (var brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, _dropDownBounds);
            }

            // Draw dropdown arrow centered in the dropdown bounds
            var arrowColor = Enabled && CommandEnabled ?
                RibbonColors.Current.ButtonText : RibbonColors.Current.ButtonTextDisabled;

            var arrowWidth = 5;
            var arrowHeight = 3;
            var arrowX = _dropDownBounds.X + (_dropDownBounds.Width - arrowWidth) / 2;
            var arrowY = _dropDownBounds.Y + (_dropDownBounds.Height - arrowHeight) / 2;

            using (var brush = new SolidBrush(arrowColor))
            {
                var points = new Point[]
                {
                    new Point(arrowX, arrowY),
                    new Point(arrowX + arrowWidth, arrowY),
                    new Point(arrowX + arrowWidth / 2, arrowY + arrowHeight)
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

            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] RibbonButton.OnMouseDown: {CommandId} type={_buttonType} enabled={Enabled} cmdEnabled={CommandEnabled} menuItems={_menuItems.Count}");

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

            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] RibbonButton.OnClick: {CommandId} type={_buttonType} menuItems={_menuItems.Count}");

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
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] RibbonButton.OnButtonClick: {CommandId} type={_buttonType} enabled={Enabled} cmdEnabled={CommandEnabled}");

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
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] RibbonButton.ShowDropDown: {CommandId} menuItems={_menuItems.Count}");

            if (_menuItems.Count == 0) return;

            if (_dropDownMenu == null)
            {
                _dropDownMenu = new ContextMenuStrip();
                _dropDownMenu.Opening += (s, e) => { _isDropDownPressed = true; Invalidate(); };
                _dropDownMenu.Closed += (s, e) => 
                { 
                    _isDropDownPressed = false; 
                    Invalidate();
                    // Remove mouse hook when dropdown closes
                    _mouseHook?.Remove();
                };

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

                            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] RibbonButton.MenuItem.Click: {ribbonItem.CommandId}");

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

            // Install mouse hook to detect clicks on native controls (WebView2/MSHTML)
            if (_mouseHook == null)
            {
                _mouseHook = new DropDownMouseHook(
                    this,
                    () => _dropDownMenu,
                    () => _dropDownMenu?.Close()
                );
            }
            _mouseHook.Install();

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
                _mouseHook?.Dispose();
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
