// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Manages keytip display and keyboard navigation for the ribbon.
    /// </summary>
    public class KeytipManager : IDisposable
    {
        private readonly RibbonPanel _ribbonPanel;
        private readonly List<KeytipInfo> _activeKeytips = new List<KeytipInfo>();
        private readonly KeytipOverlay _overlay;

        private bool _isActive;
        private string _typedKeys = string.Empty;
        private KeytipMode _mode = KeytipMode.None;

        /// <summary>
        /// Gets whether keytip mode is active.
        /// </summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// Occurs when a keytip is activated.
        /// </summary>
        public event EventHandler<KeytipActivatedEventArgs> KeytipActivated;

        public KeytipManager(RibbonPanel ribbonPanel)
        {
            _ribbonPanel = ribbonPanel ?? throw new ArgumentNullException(nameof(ribbonPanel));

            _overlay = new KeytipOverlay(this);
            _overlay.Visible = false;
        }

        /// <summary>
        /// Activates keytip mode.
        /// </summary>
        public void Activate()
        {
            if (_isActive)
                return;

            _isActive = true;
            _typedKeys = string.Empty;
            _mode = KeytipMode.Tabs;

            BuildKeytips();
            ShowOverlay();
        }

        /// <summary>
        /// Deactivates keytip mode.
        /// </summary>
        public void Deactivate()
        {
            if (!_isActive)
                return;

            _isActive = false;
            _typedKeys = string.Empty;
            _mode = KeytipMode.None;
            _activeKeytips.Clear();

            HideOverlay();
        }

        /// <summary>
        /// Processes a key press while in keytip mode.
        /// </summary>
        public bool ProcessKey(Keys key)
        {
            if (!_isActive)
                return false;

            // Escape cancels keytip mode
            if (key == Keys.Escape)
            {
                if (_mode != KeytipMode.Tabs)
                {
                    // Go back to tab level
                    _mode = KeytipMode.Tabs;
                    _typedKeys = string.Empty;
                    BuildKeytips();
                    _overlay.Invalidate();
                }
                else
                {
                    Deactivate();
                }
                return true;
            }

            // Convert key to character
            char c = KeyToChar(key);
            if (c == '\0')
                return false;

            _typedKeys += c;

            // Find matching keytips
            var matches = _activeKeytips.FindAll(k =>
                k.Keytip.StartsWith(_typedKeys, StringComparison.OrdinalIgnoreCase));

            if (matches.Count == 0)
            {
                // No matches - beep and reset
                System.Media.SystemSounds.Beep.Play();
                _typedKeys = string.Empty;
                _overlay.Invalidate();
            }
            else if (matches.Count == 1 && matches[0].Keytip.Equals(_typedKeys, StringComparison.OrdinalIgnoreCase))
            {
                // Exact match - activate
                var match = matches[0];
                ActivateKeytip(match);
            }
            else
            {
                // Multiple or partial matches - update display
                _overlay.Invalidate();
            }

            return true;
        }

        private void ActivateKeytip(KeytipInfo keytip)
        {
            switch (keytip.Type)
            {
                case KeytipType.Tab:
                    // Select the tab and show control keytips
                    _ribbonPanel.SelectedTab = keytip.Tab;
                    _mode = KeytipMode.Controls;
                    _typedKeys = string.Empty;
                    BuildKeytips();
                    _overlay.Invalidate();
                    break;

                case KeytipType.Control:
                    // Execute the control and deactivate
                    KeytipActivated?.Invoke(this, new KeytipActivatedEventArgs(keytip.CommandId, keytip.Control));
                    // Execute the control's command
                    keytip.Control?.ExecuteCommand();
                    Deactivate();
                    break;

                case KeytipType.Group:
                    // Show group keytips
                    _mode = KeytipMode.GroupControls;
                    _typedKeys = string.Empty;
                    BuildKeytips();
                    _overlay.Invalidate();
                    break;
            }
        }

        private void BuildKeytips()
        {
            _activeKeytips.Clear();

            switch (_mode)
            {
                case KeytipMode.Tabs:
                    BuildTabKeytips();
                    break;

                case KeytipMode.Controls:
                    BuildControlKeytips();
                    break;

                case KeytipMode.GroupControls:
                    BuildGroupControlKeytips();
                    break;
            }
        }

        private void BuildTabKeytips()
        {
            // Add keytips for visible tabs
            foreach (var tab in _ribbonPanel.SelectedTab?.Groups ?? new List<RibbonGroup>())
            {
                // Tab keytips would be added here based on tab's keytip property
            }

            // For now, generate default keytips based on tab labels
            var tabs = new[] { "H", "N", "A", "P" }; // Home, Insert, Account, Preview
            var index = 0;

            // This is a simplified implementation - real version would iterate actual tabs
            foreach (var keytip in tabs)
            {
                _activeKeytips.Add(new KeytipInfo
                {
                    Keytip = keytip,
                    Type = KeytipType.Tab,
                    Bounds = new Rectangle(70 + index * 60, 4, 20, 16)
                });
                index++;
            }
        }

        private void BuildControlKeytips()
        {
            var selectedTab = _ribbonPanel.SelectedTab;
            if (selectedTab == null)
                return;

            foreach (var group in selectedTab.Groups)
            {
                var groupBounds = group.Bounds;

                foreach (Control control in group.Controls)
                {
                    if (control is RibbonControlBase ribbonControl && !string.IsNullOrEmpty(group.Keytip))
                    {
                        var controlBounds = control.Bounds;
                        controlBounds.Offset(groupBounds.Location);

                        _activeKeytips.Add(new KeytipInfo
                        {
                            Keytip = GenerateKeytip(control.Name),
                            Type = KeytipType.Control,
                            Control = ribbonControl,
                            CommandId = ribbonControl.CommandId,
                            Bounds = new Rectangle(
                                controlBounds.X + controlBounds.Width / 2 - 10,
                                controlBounds.Y + controlBounds.Height - 12,
                                20, 16)
                        });
                    }
                }
            }
        }

        private void BuildGroupControlKeytips()
        {
            // Build keytips for controls in expanded group popup
            BuildControlKeytips();
        }

        private string GenerateKeytip(string name)
        {
            // Generate a keytip from control name
            if (string.IsNullOrEmpty(name))
                return "X";

            // Use first letter of each word
            var parts = name.Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0].Substring(0, 1).ToUpper();
            }
            return name.Substring(0, 1).ToUpper();
        }

        private char KeyToChar(Keys key)
        {
            if (key >= Keys.A && key <= Keys.Z)
            {
                return (char)('A' + (key - Keys.A));
            }
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                return (char)('0' + (key - Keys.D0));
            }
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                return (char)('0' + (key - Keys.NumPad0));
            }
            return '\0';
        }

        private void ShowOverlay()
        {
            _overlay.Parent = _ribbonPanel;
            _overlay.Location = Point.Empty;
            _overlay.Size = _ribbonPanel.Size;
            _overlay.BringToFront();
            _overlay.Visible = true;
        }

        private void HideOverlay()
        {
            _overlay.Visible = false;
            _overlay.Parent = null;
        }

        /// <summary>
        /// Gets the active keytips.
        /// </summary>
        internal IReadOnlyList<KeytipInfo> ActiveKeytips => _activeKeytips.AsReadOnly();

        /// <summary>
        /// Gets the typed keys.
        /// </summary>
        internal string TypedKeys => _typedKeys;

        public void Dispose()
        {
            _overlay?.Dispose();
        }
    }

    /// <summary>
    /// Keytip overlay control that displays keytip badges.
    /// </summary>
    internal class KeytipOverlay : Control
    {
        private readonly KeytipManager _manager;

        public KeytipOverlay(KeytipManager manager)
        {
            _manager = manager;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            foreach (var keytip in _manager.ActiveKeytips)
            {
                DrawKeytip(g, keytip);
            }
        }

        private void DrawKeytip(Graphics g, KeytipInfo keytip)
        {
            var bounds = keytip.Bounds;

            // Determine if this keytip matches current typed keys
            var isMatch = keytip.Keytip.StartsWith(_manager.TypedKeys, StringComparison.OrdinalIgnoreCase);
            var isExactMatch = keytip.Keytip.Equals(_manager.TypedKeys, StringComparison.OrdinalIgnoreCase);

            // Background
            var backColor = isExactMatch ? Color.FromArgb(0, 102, 204) :
                           isMatch ? Color.White :
                           Color.FromArgb(200, 200, 200);

            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // Border
            using (var pen = new Pen(Color.FromArgb(100, 100, 100)))
            {
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }

            // Text with high-quality rendering
            var textColor = isExactMatch ? Color.White : Color.Black;
            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 8f, FontStyle.Bold))
            {
                RibbonRenderer.DrawHighQualityText(g, keytip.Keytip, font, textColor, bounds,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _manager.Deactivate();
        }
    }

    /// <summary>
    /// Information about a keytip.
    /// </summary>
    internal class KeytipInfo
    {
        public string Keytip { get; set; }
        public KeytipType Type { get; set; }
        public RibbonTab Tab { get; set; }
        public RibbonGroup Group { get; set; }
        public RibbonControlBase Control { get; set; }
        public OpenLiveWriter.Localization.CommandId CommandId { get; set; }
        public Rectangle Bounds { get; set; }
    }

    /// <summary>
    /// Type of keytip.
    /// </summary>
    internal enum KeytipType
    {
        Tab,
        Group,
        Control
    }

    /// <summary>
    /// Keytip mode.
    /// </summary>
    internal enum KeytipMode
    {
        None,
        Tabs,
        Controls,
        GroupControls
    }

    /// <summary>
    /// Event args for keytip activation.
    /// </summary>
    public class KeytipActivatedEventArgs : EventArgs
    {
        public OpenLiveWriter.Localization.CommandId CommandId { get; }
        public Control Control { get; }

        public KeytipActivatedEventArgs(OpenLiveWriter.Localization.CommandId commandId, Control control)
        {
            CommandId = commandId;
            Control = control;
        }
    }
}
