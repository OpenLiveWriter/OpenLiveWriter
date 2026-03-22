// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Configuration;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Application menu (backstage) control for the ribbon.
    /// </summary>
    public class ApplicationMenu : Form
    {
        private const int MENU_WIDTH = 300;
        private const int RECENT_PANEL_WIDTH = 350;
        private const int MENU_ITEM_HEIGHT = 48;
        private const int SMALL_MENU_ITEM_HEIGHT = 32;
        private const int PADDING = 8;

        private RibbonCommandManager _commandManager;
        private readonly List<AppMenuItem> _menuItems = new List<AppMenuItem>();
        private readonly List<RecentItem> _recentItems = new List<RecentItem>();
        private int _maxRecentItems = 10;
        private DateTime _showTime;  // Track when menu was shown to prevent immediate close

        private Panel _menuPanel;
        private Panel _recentPanel;
        private Label _recentLabel;
        private int _hoveredMenuIndex = -1;
        private int _hoveredRecentIndex = -1;

        /// <summary>
        /// Gets or sets the command manager.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCommandManager CommandManager
        {
            get => _commandManager;
            set => _commandManager = value;
        }

        /// <summary>
        /// Gets the menu items.
        /// </summary>
        public List<AppMenuItem> MenuItems => _menuItems;

        /// <summary>
        /// Gets the recent items.
        /// </summary>
        public List<RecentItem> RecentItems => _recentItems;

        /// <summary>
        /// Gets or sets the maximum number of recent items.
        /// </summary>
        public int MaxRecentItems
        {
            get => _maxRecentItems;
            set => _maxRecentItems = Math.Max(1, value);
        }

        /// <summary>
        /// Occurs when a menu item is clicked.
        /// </summary>
        public event EventHandler<AppMenuItemClickEventArgs> MenuItemClick;

        /// <summary>
        /// Occurs when a recent item is clicked.
        /// </summary>
        public event EventHandler<RecentItemClickEventArgs> RecentItemClick;

        public ApplicationMenu()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Size = new Size(MENU_WIDTH + RECENT_PANEL_WIDTH, 500);
            BackColor = RibbonColors.Current.AppMenuBackground;

            // Menu panel
            _menuPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(MENU_WIDTH, Height),
                BackColor = RibbonColors.Current.AppMenuBackground
            };
            _menuPanel.Paint += MenuPanel_Paint;
            _menuPanel.MouseMove += MenuPanel_MouseMove;
            _menuPanel.MouseLeave += MenuPanel_MouseLeave;
            _menuPanel.MouseClick += MenuPanel_MouseClick;
            Controls.Add(_menuPanel);

            // Recent items panel
            _recentPanel = new Panel
            {
                Location = new Point(MENU_WIDTH, 0),
                Size = new Size(RECENT_PANEL_WIDTH, Height),
                BackColor = RibbonColors.Current.AppMenuRecentItemsBackground
            };
            _recentPanel.Paint += RecentPanel_Paint;
            _recentPanel.MouseMove += RecentPanel_MouseMove;
            _recentPanel.MouseLeave += RecentPanel_MouseLeave;
            _recentPanel.MouseClick += RecentPanel_MouseClick;
            Controls.Add(_recentPanel);

            // Recent items label - match native "Recent posts and drafts" header
            _recentLabel = new Label
            {
                Location = new Point(PADDING, PADDING),
                Size = new Size(RECENT_PANEL_WIDTH - PADDING * 2, 24),
                Text = "Recent posts and drafts",
                Font = new Font(SystemFonts.MenuFont.FontFamily, 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(68, 68, 68),
                BackColor = Color.Transparent
            };
            _recentPanel.Controls.Add(_recentLabel);

            // Close menu when it loses focus (user clicks outside)
            Deactivate += (s, e) =>
            {
                // Small delay to avoid closing immediately on show
                if ((DateTime.Now - _showTime).TotalMilliseconds > 200)
                    Hide();
            };
        }

        /// <summary>
        /// Builds the menu from configuration.
        /// </summary>
        public void BuildFromConfiguration(ApplicationMenuConfig config)
        {
            _menuItems.Clear();

            // Add standard menu items from configuration
            foreach (var menuGroup in config.MenuGroups)
            {
                foreach (var item in menuGroup.Items)
                {
                    if (item.IsSeparator)
                    {
                        _menuItems.Add(new AppMenuItem { IsSeparator = true });
                    }
                    else
                    {
                        var command = _commandManager?.GetCommand(item.CommandId);
                        _menuItems.Add(new AppMenuItem
                        {
                            CommandId = item.CommandId,
                            Label = command?.Label ?? item.CommandId.ToString(),
                            Image = command?.LargeImage
                        });
                    }
                }
            }

            // Add default items if none configured
            if (_menuItems.Count == 0)
            {
                AddDefaultMenuItems();
            }

            _maxRecentItems = config.MaxRecentItems;
            UpdateHeight();
            _menuPanel.Invalidate();
        }

        private void AddDefaultMenuItems()
        {
            _menuItems.Add(new AppMenuItem { CommandId = CommandId.NewPost, Label = "New Post", IsLarge = true });
            _menuItems.Add(new AppMenuItem { CommandId = CommandId.OpenPost, Label = "Open Post", IsLarge = true });
            _menuItems.Add(new AppMenuItem { CommandId = CommandId.SavePost, Label = "Save", IsLarge = true });
            _menuItems.Add(new AppMenuItem { IsSeparator = true });
            _menuItems.Add(new AppMenuItem { CommandId = CommandId.PostAndPublish, Label = "Publish", IsLarge = true });
            _menuItems.Add(new AppMenuItem { IsSeparator = true });
            _menuItems.Add(new AppMenuItem { CommandId = CommandId.PrintPreview, Label = "Print Preview", IsLarge = false });
            _menuItems.Add(new AppMenuItem { CommandId = CommandId.Print, Label = "Print", IsLarge = false });
            _menuItems.Add(new AppMenuItem { IsSeparator = true });
            _menuItems.Add(new AppMenuItem { CommandId = CommandId.About, Label = "About", IsLarge = false });
        }

        private void UpdateHeight()
        {
            var menuHeight = PADDING;
            foreach (var item in _menuItems)
            {
                if (item.IsSeparator)
                    menuHeight += 8;
                else if (item.IsLarge)
                    menuHeight += MENU_ITEM_HEIGHT;
                else
                    menuHeight += SMALL_MENU_ITEM_HEIGHT;
            }
            menuHeight += PADDING;

            var recentHeight = PADDING + 30 + (_recentItems.Count * 40) + PADDING;

            Height = Math.Max(menuHeight, Math.Max(recentHeight, 400));
            _menuPanel.Height = Height;
            _recentPanel.Height = Height;
        }

        /// <summary>
        /// Adds a recent item.
        /// </summary>
        public void AddRecentItem(string title, string path, DateTime lastAccessed)
        {
            // Remove if already exists
            _recentItems.RemoveAll(r => r.Path == path);

            // Add at top
            _recentItems.Insert(0, new RecentItem
            {
                Title = title,
                Path = path,
                LastAccessed = lastAccessed
            });

            // Trim to max
            while (_recentItems.Count > _maxRecentItems)
            {
                _recentItems.RemoveAt(_recentItems.Count - 1);
            }

            UpdateHeight();
            _recentPanel.Invalidate();
        }

        /// <summary>
        /// Shows the menu at the specified location.
        /// </summary>
        public void Show(Point screenLocation)
        {
            _showTime = DateTime.Now;  // Track show time for deactivation handling
            Location = screenLocation;
            Show();
            Activate();
        }

        #region Menu Panel

        private void MenuPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var y = PADDING;

            for (int i = 0; i < _menuItems.Count; i++)
            {
                var item = _menuItems[i];

                if (item.IsSeparator)
                {
                    DrawMenuSeparator(g, y);
                    y += 8;
                }
                else
                {
                    var height = item.IsLarge ? MENU_ITEM_HEIGHT : SMALL_MENU_ITEM_HEIGHT;
                    var bounds = new Rectangle(PADDING, y, MENU_WIDTH - PADDING * 2, height);
                    item.Bounds = bounds;

                    var isHovered = i == _hoveredMenuIndex;
                    RibbonRenderer.Instance.DrawAppMenuItem(g, bounds, item.Label, item.Image, isHovered, false);

                    y += height;
                }
            }
        }

        private void DrawMenuSeparator(Graphics g, int y)
        {
            // Light gray separator to match light theme
            using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                g.DrawLine(pen, PADDING + 10, y + 4, MENU_WIDTH - PADDING - 10, y + 4);
            }
        }

        private void MenuPanel_MouseMove(object sender, MouseEventArgs e)
        {
            var newHovered = -1;

            for (int i = 0; i < _menuItems.Count; i++)
            {
                if (!_menuItems[i].IsSeparator && _menuItems[i].Bounds.Contains(e.Location))
                {
                    newHovered = i;
                    break;
                }
            }

            if (newHovered != _hoveredMenuIndex)
            {
                _hoveredMenuIndex = newHovered;
                _menuPanel.Invalidate();
            }
        }

        private void MenuPanel_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredMenuIndex >= 0)
            {
                _hoveredMenuIndex = -1;
                _menuPanel.Invalidate();
            }
        }

        private void MenuPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (_hoveredMenuIndex >= 0 && _hoveredMenuIndex < _menuItems.Count)
            {
                var item = _menuItems[_hoveredMenuIndex];
                if (!item.IsSeparator)
                {
                    Hide();
                    MenuItemClick?.Invoke(this, new AppMenuItemClickEventArgs(item));
                    _commandManager?.Execute(item.CommandId);
                }
            }
        }

        #endregion

        #region Recent Panel

        private void RecentPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var y = 40;

            for (int i = 0; i < _recentItems.Count; i++)
            {
                var item = _recentItems[i];
                var bounds = new Rectangle(PADDING, y, RECENT_PANEL_WIDTH - PADDING * 2, 36);
                item.Bounds = bounds;

                var isHovered = i == _hoveredRecentIndex;
                DrawRecentItem(g, bounds, item, isHovered);

                y += 40;
            }

            if (_recentItems.Count == 0)
            {
                var textBounds = new Rectangle(PADDING, y, RECENT_PANEL_WIDTH - PADDING * 2, 40);
                RibbonRenderer.DrawHighQualityText(g, "No recent items", SystemFonts.MenuFont,
                    Color.FromArgb(128, 128, 128), textBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        private void DrawRecentItem(Graphics g, Rectangle bounds, RecentItem item, bool isHovered)
        {
            // Background
            if (isHovered)
            {
                using (var brush = new SolidBrush(Color.FromArgb(232, 239, 247)))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // Pin icon area
            var pinBounds = new Rectangle(bounds.X, bounds.Y, 24, bounds.Height);

            if (item.IsPinned)
            {
                // Draw pin icon with high-quality rendering
                using (var font = new Font("Segoe UI Symbol", 10f))
                {
                    var pinTextBounds = new Rectangle(pinBounds.X + 2, bounds.Y + (bounds.Height - 16) / 2, 20, 16);
                    RibbonRenderer.DrawHighQualityText(g, "📌", font, 
                        Color.FromArgb(0, 102, 204), pinTextBounds, TextFormatFlags.Left);
                }
            }

            // Title with high-quality rendering
            var titleBounds = new Rectangle(bounds.X + 26, bounds.Y + 2, bounds.Width - 30, 18);
            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 9f))
            {
                RibbonRenderer.DrawHighQualityText(g, item.Title, font, 
                    Color.FromArgb(38, 38, 38), titleBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | 
                    TextFormatFlags.PathEllipsis | TextFormatFlags.SingleLine);
            }

            // Path with high-quality rendering
            var pathBounds = new Rectangle(bounds.X + 26, bounds.Y + 18, bounds.Width - 30, 16);
            using (var font = new Font(SystemFonts.MenuFont.FontFamily, 7.5f))
            {
                RibbonRenderer.DrawHighQualityText(g, item.Path, font, 
                    Color.FromArgb(128, 128, 128), pathBounds,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | 
                    TextFormatFlags.PathEllipsis | TextFormatFlags.SingleLine);
            }
        }

        private void RecentPanel_MouseMove(object sender, MouseEventArgs e)
        {
            var newHovered = -1;

            for (int i = 0; i < _recentItems.Count; i++)
            {
                if (_recentItems[i].Bounds.Contains(e.Location))
                {
                    newHovered = i;
                    break;
                }
            }

            if (newHovered != _hoveredRecentIndex)
            {
                _hoveredRecentIndex = newHovered;
                _recentPanel.Invalidate();
            }
        }

        private void RecentPanel_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredRecentIndex >= 0)
            {
                _hoveredRecentIndex = -1;
                _recentPanel.Invalidate();
            }
        }

        private void RecentPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (_hoveredRecentIndex >= 0 && _hoveredRecentIndex < _recentItems.Count)
            {
                var item = _recentItems[_hoveredRecentIndex];

                // Check if clicking on pin area
                var pinBounds = new Rectangle(item.Bounds.X, item.Bounds.Y, 24, item.Bounds.Height);
                if (pinBounds.Contains(e.Location))
                {
                    item.IsPinned = !item.IsPinned;
                    _recentPanel.Invalidate();
                }
                else
                {
                    Hide();
                    RecentItemClick?.Invoke(this, new RecentItemClickEventArgs(item));
                }
            }
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW - don't show in taskbar
                return cp;
            }
        }
    }

    /// <summary>
    /// Represents a menu item in the application menu.
    /// </summary>
    public class AppMenuItem
    {
        public CommandId CommandId { get; set; }
        public string Label { get; set; }
        public Image Image { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsLarge { get; set; } = true;
        internal Rectangle Bounds { get; set; }
    }

    /// <summary>
    /// Represents a recent item in the application menu.
    /// </summary>
    public class RecentItem
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public DateTime LastAccessed { get; set; }
        public bool IsPinned { get; set; }
        public object Tag { get; set; }
        internal Rectangle Bounds { get; set; }
    }

    /// <summary>
    /// Event args for application menu item clicks.
    /// </summary>
    public class AppMenuItemClickEventArgs : EventArgs
    {
        public AppMenuItem MenuItem { get; }

        public AppMenuItemClickEventArgs(AppMenuItem menuItem)
        {
            MenuItem = menuItem;
        }
    }

    /// <summary>
    /// Event args for recent item clicks.
    /// </summary>
    public class RecentItemClickEventArgs : EventArgs
    {
        public RecentItem Item { get; }

        public RecentItemClickEventArgs(RecentItem item)
        {
            Item = item;
        }
    }
}
