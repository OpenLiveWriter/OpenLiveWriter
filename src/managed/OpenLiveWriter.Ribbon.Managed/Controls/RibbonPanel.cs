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
    /// Main ribbon panel control that hosts tabs, groups, and the application menu.
    /// </summary>
    public class RibbonPanel : UserControl
    {
        private const int TAB_HEIGHT = 25;
        private const int CONTENT_HEIGHT = 94;
        private const int APP_BUTTON_WIDTH = 48;
        private const int TAB_PADDING = 10;

        private RibbonCommandManager _commandManager;
        private RibbonConfiguration _configuration;
        private RibbonApplicationMode _currentMode = RibbonApplicationMode.Normal | RibbonApplicationMode.LTR | RibbonApplicationMode.WithPlugins;

        private readonly List<RibbonTab> _tabs = new List<RibbonTab>();
        private readonly Dictionary<RibbonContextualTabGroup, List<RibbonTab>> _contextualTabs = new Dictionary<RibbonContextualTabGroup, List<RibbonTab>>();
        private readonly HashSet<RibbonContextualTabGroup> _visibleContextualGroups = new HashSet<RibbonContextualTabGroup>();

        private RibbonTab _selectedTab;
        private RibbonTab _hoveredTab;
        private Rectangle _appMenuButtonBounds;
        private bool _appMenuButtonHovered;
#pragma warning disable CS0649 // Field is never assigned
        private bool _appMenuButtonPressed;
#pragma warning restore CS0649

        private ApplicationMenu _applicationMenu;
        private QuickAccessToolbar _quickAccessToolbar;

        private Panel _tabHeaderPanel;
        private Panel _contentPanel;

        /// <summary>
        /// Occurs when the selected tab changes.
        /// </summary>
        public event EventHandler SelectedTabChanged;

        /// <summary>
        /// Occurs when the application menu button is clicked.
        /// </summary>
        public event EventHandler ApplicationMenuClicked;

        /// <summary>
        /// Gets or sets the command manager for this ribbon.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCommandManager CommandManager
        {
            get => _commandManager;
            set
            {
                _commandManager = value;
                foreach (var tab in _tabs)
                {
                    tab.CommandManager = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current application mode.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonApplicationMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    UpdateVisibility();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets the currently selected tab.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonTab SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value && value != null)
                {
                    // Check if tab is valid for current mode (don't check Visible - that's set by UpdateSelectedTab)
                    var isValidForMode = (value.VisibleModes & _currentMode) != 0;
                    var isContextualVisible = value.ContextualGroup == RibbonContextualTabGroup.None ||
                                              _visibleContextualGroups.Contains(value.ContextualGroup);

                    if (isValidForMode && isContextualVisible)
                    {
                        _selectedTab = value;
                        UpdateSelectedTab();
                        SelectedTabChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the height of the ribbon.
        /// </summary>
        public int RibbonHeight => TAB_HEIGHT + CONTENT_HEIGHT + 2;

        public RibbonPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            Height = RibbonHeight;
            Dock = DockStyle.Top;
            BackColor = RibbonColors.Current.RibbonBackground;

            // Content panel - add first so it's docked after tab header
            _contentPanel = new Panel
            {
                Location = new Point(0, TAB_HEIGHT),
                Height = CONTENT_HEIGHT,
                Dock = DockStyle.Fill,
                BackColor = RibbonColors.Current.TabBackgroundSelected
            };
            Controls.Add(_contentPanel);

            // Tab header panel - add second so it's docked first (top)
            _tabHeaderPanel = new Panel
            {
                Location = new Point(0, 0),
                Height = TAB_HEIGHT,
                Dock = DockStyle.Top,
                BackColor = RibbonColors.Current.TabBackground
            };
            _tabHeaderPanel.Paint += TabHeaderPanel_Paint;
            _tabHeaderPanel.MouseMove += TabHeaderPanel_MouseMove;
            _tabHeaderPanel.MouseLeave += TabHeaderPanel_MouseLeave;
            _tabHeaderPanel.MouseClick += TabHeaderPanel_MouseClick;
            Controls.Add(_tabHeaderPanel);

            // Quick Access Toolbar - positioned above tabs
            _quickAccessToolbar = new QuickAccessToolbar
            {
                Location = new Point(60, 2),
                BackColor = Color.Transparent
            };
            Controls.Add(_quickAccessToolbar);
            _quickAccessToolbar.BringToFront();

            ResumeLayout(false);
        }

        /// <summary>
        /// Builds the ribbon from a configuration.
        /// </summary>
        public void BuildFromConfiguration(RibbonConfiguration config)
        {
            _configuration = config ?? throw new ArgumentNullException(nameof(config));

            SuspendLayout();
            ClearTabs();

            // Create tabs
            foreach (var tabConfig in config.Tabs)
            {
                var tab = CreateTabFromConfig(tabConfig);
                AddTab(tab);
            }

            // Create contextual tabs
            foreach (var ctgConfig in config.ContextualTabGroups)
            {
                foreach (var tabConfig in ctgConfig.Tabs)
                {
                    var tab = CreateTabFromConfig(tabConfig);
                    tab.ContextualGroup = ctgConfig.GroupType;
                    AddContextualTab(ctgConfig.GroupType, tab);
                }
            }

            // Select first visible tab
            foreach (var tab in _tabs)
            {
                if (tab.Visible)
                {
                    SelectedTab = tab;
                    break;
                }
            }

            // Configure Quick Access Toolbar
            if (_quickAccessToolbar != null && config.QuickAccessToolbar != null)
            {
                _quickAccessToolbar.CommandManager = _commandManager;
                _quickAccessToolbar.SetCommands(config.QuickAccessToolbar.DefaultCommands);
            }

            ResumeLayout(true);
            Invalidate();
        }

        private RibbonTab CreateTabFromConfig(TabConfig config)
        {
            var tab = new RibbonTab
            {
                CommandId = config.CommandId,
                Label = config.Label,
                Keytip = config.Keytip,
                VisibleModes = config.VisibleModes,
                CommandManager = _commandManager
            };

            foreach (var groupConfig in config.Groups)
            {
                var group = CreateGroupFromConfig(groupConfig);
                tab.AddGroup(group);
            }

            return tab;
        }

        private RibbonGroup CreateGroupFromConfig(GroupConfig config)
        {
            var group = new RibbonGroup
            {
                CommandId = config.CommandId,
                Label = config.Label,
                Keytip = config.Keytip,
                VisibleModes = config.VisibleModes,
                CommandManager = _commandManager
            };

            foreach (var controlConfig in config.Controls)
            {
                var control = CreateControlFromConfig(controlConfig);
                if (control != null)
                {
                    group.AddControl(control);
                }
            }

            return group;
        }

        private RibbonControlBase CreateControlFromConfig(ControlConfig config)
        {
            RibbonControlBase control = null;

            switch (config)
            {
                case ButtonConfig buttonConfig:
                    control = new RibbonButton
                    {
                        CommandId = buttonConfig.CommandId,
                        ButtonType = buttonConfig.ButtonType,
                        CurrentSize = buttonConfig.PreferredSize,
                        CommandManager = _commandManager
                    };
                    break;

                case ToggleButtonConfig toggleConfig:
                    control = new RibbonButton
                    {
                        CommandId = toggleConfig.CommandId,
                        ButtonType = RibbonButtonType.ToggleButton,
                        CurrentSize = toggleConfig.PreferredSize,
                        CommandManager = _commandManager
                    };
                    break;

                case ComboBoxConfig comboConfig:
                    var comboBox = new RibbonComboBox
                    {
                        CommandId = comboConfig.CommandId,
                        IsAutoCompleteEnabled = comboConfig.IsAutoCompleteEnabled,
                        IsEditable = comboConfig.IsEditable,
                        CommandManager = _commandManager
                    };
                    if (comboConfig.PreferredWidth > 0)
                    {
                        comboBox.Width = comboConfig.PreferredWidth;
                    }
                    control = comboBox;
                    break;

                case GalleryConfig galleryConfig:
                    control = new RibbonGallery
                    {
                        CommandId = galleryConfig.CommandId,
                        GalleryType = galleryConfig.GalleryType,
                        ItemHeight = galleryConfig.ItemHeight,
                        ItemWidth = galleryConfig.ItemWidth,
                        Columns = galleryConfig.Columns,
                        CommandManager = _commandManager
                    };
                    break;

                case ColorPickerConfig colorConfig:
                    control = new RibbonColorPicker
                    {
                        CommandId = colorConfig.CommandId,
                        ColorTemplate = colorConfig.ColorTemplate,
                        IsNoColorButtonVisible = colorConfig.IsNoColorButtonVisible,
                        IsAutomaticColorButtonVisible = colorConfig.IsAutomaticColorButtonVisible,
                        CommandManager = _commandManager
                    };
                    break;

                case SpinnerConfig spinnerConfig:
                    control = new RibbonSpinner
                    {
                        CommandId = spinnerConfig.CommandId,
                        MinValue = spinnerConfig.MinValue,
                        MaxValue = spinnerConfig.MaxValue,
                        Increment = spinnerConfig.Increment,
                        CommandManager = _commandManager
                    };
                    break;

                case SeparatorConfig _:
                    control = new RibbonSeparator();
                    break;
            }

            if (control != null)
            {
                control.VisibleModes = config.VisibleModes;
            }

            return control;
        }

        /// <summary>
        /// Adds a tab to the ribbon.
        /// </summary>
        public void AddTab(RibbonTab tab)
        {
            if (tab == null) throw new ArgumentNullException(nameof(tab));

            tab.CommandManager = _commandManager;
            _tabs.Add(tab);
            _contentPanel.Controls.Add(tab);
            tab.Dock = DockStyle.Fill;
            tab.Visible = false;

            UpdateVisibility();
        }

        /// <summary>
        /// Adds a contextual tab to a group.
        /// </summary>
        public void AddContextualTab(RibbonContextualTabGroup group, RibbonTab tab)
        {
            if (!_contextualTabs.ContainsKey(group))
            {
                _contextualTabs[group] = new List<RibbonTab>();
            }

            tab.ContextualGroup = group;
            tab.CommandManager = _commandManager;
            _contextualTabs[group].Add(tab);
            _contentPanel.Controls.Add(tab);
            tab.Dock = DockStyle.Fill;
            tab.Visible = false;
        }

        /// <summary>
        /// Shows a contextual tab group.
        /// </summary>
        public void ShowContextualTabGroup(RibbonContextualTabGroup group)
        {
            if (!_visibleContextualGroups.Contains(group))
            {
                _visibleContextualGroups.Add(group);
                UpdateVisibility();
                _tabHeaderPanel.Invalidate();

                // Auto-select first tab in the group
                if (_contextualTabs.TryGetValue(group, out var tabs) && tabs.Count > 0)
                {
                    SelectedTab = tabs[0];
                }
            }
        }

        /// <summary>
        /// Hides a contextual tab group.
        /// </summary>
        public void HideContextualTabGroup(RibbonContextualTabGroup group)
        {
            if (_visibleContextualGroups.Remove(group))
            {
                // If selected tab is in this group, switch to first regular tab
                if (_selectedTab?.ContextualGroup == group)
                {
                    foreach (var tab in _tabs)
                    {
                        if (tab.Visible && tab.ContextualGroup == RibbonContextualTabGroup.None)
                        {
                            SelectedTab = tab;
                            break;
                        }
                    }
                }

                UpdateVisibility();
                _tabHeaderPanel.Invalidate();
            }
        }

        private void ClearTabs()
        {
            foreach (var tab in _tabs)
            {
                _contentPanel.Controls.Remove(tab);
                tab.Dispose();
            }
            _tabs.Clear();

            foreach (var group in _contextualTabs.Values)
            {
                foreach (var tab in group)
                {
                    _contentPanel.Controls.Remove(tab);
                    tab.Dispose();
                }
            }
            _contextualTabs.Clear();
            _visibleContextualGroups.Clear();

            _selectedTab = null;
        }

        private void UpdateVisibility()
        {
            // Update tab visibility based on current mode
            foreach (var tab in _tabs)
            {
                var isVisibleForMode = (tab.VisibleModes & _currentMode) != 0;
                tab.Visible = isVisibleForMode && tab == _selectedTab;
            }

            foreach (var group in _contextualTabs.Values)
            {
                foreach (var tab in group)
                {
                    var isContextVisible = _visibleContextualGroups.Contains(tab.ContextualGroup);
                    var isVisibleForMode = (tab.VisibleModes & _currentMode) != 0;
                    tab.Visible = isContextVisible && isVisibleForMode && tab == _selectedTab;
                }
            }

            // Ensure a tab is selected
            if (_selectedTab == null || !_selectedTab.Visible)
            {
                foreach (var tab in _tabs)
                {
                    if ((tab.VisibleModes & _currentMode) != 0)
                    {
                        _selectedTab = tab;
                        tab.Visible = true;
                        break;
                    }
                }
            }
        }

        private void UpdateSelectedTab()
        {
            foreach (var tab in _tabs)
            {
                tab.Visible = tab == _selectedTab;
            }

            foreach (var group in _contextualTabs.Values)
            {
                foreach (var tab in group)
                {
                    tab.Visible = tab == _selectedTab;
                }
            }

            _tabHeaderPanel.Invalidate();
        }

        #region Tab Header Painting

        private void TabHeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(RibbonColors.Current.TabBackground);

            // Draw app menu button
            _appMenuButtonBounds = new Rectangle(4, 2, APP_BUTTON_WIDTH, TAB_HEIGHT - 4);
            RibbonRenderer.Instance.DrawAppMenuButton(g, _appMenuButtonBounds,
                _appMenuButtonHovered, _appMenuButtonPressed);

            // Draw tab headers
            var x = _appMenuButtonBounds.Right + 8;

            // Regular tabs
            foreach (var tab in _tabs)
            {
                if ((tab.VisibleModes & _currentMode) == 0) continue;

                var tabWidth = MeasureTabWidth(g, tab.Label);
                var tabBounds = new Rectangle(x, 0, tabWidth, TAB_HEIGHT);
                tab.HeaderBounds = tabBounds;

                RibbonRenderer.Instance.DrawTabHeader(g, tabBounds, tab.Label,
                    tab == _selectedTab, tab == _hoveredTab, tab.ContextualGroup);

                x += tabWidth + 2;
            }

            // Contextual tabs
            foreach (var group in _visibleContextualGroups)
            {
                if (!_contextualTabs.TryGetValue(group, out var tabs)) continue;

                // Draw contextual group header background
                var groupStartX = x;
                var groupWidth = 0;

                foreach (var tab in tabs)
                {
                    if ((tab.VisibleModes & _currentMode) == 0) continue;
                    var tabWidth = MeasureTabWidth(g, tab.Label);
                    groupWidth += tabWidth + 2;
                }

                if (groupWidth > 0)
                {
                    // Draw colored header bar
                    var groupColor = RibbonColors.Current.GetContextualTabColor(group);
                    var headerBounds = new Rectangle(groupStartX, 0, groupWidth, 3);
                    using (var brush = new SolidBrush(groupColor))
                    {
                        g.FillRectangle(brush, headerBounds);
                    }
                }

                // Draw tabs
                foreach (var tab in tabs)
                {
                    if ((tab.VisibleModes & _currentMode) == 0) continue;

                    var tabWidth = MeasureTabWidth(g, tab.Label);
                    var tabBounds = new Rectangle(x, 0, tabWidth, TAB_HEIGHT);
                    tab.HeaderBounds = tabBounds;

                    RibbonRenderer.Instance.DrawTabHeader(g, tabBounds, tab.Label,
                        tab == _selectedTab, tab == _hoveredTab, tab.ContextualGroup);

                    x += tabWidth + 2;
                }
            }

            // Bottom border
            using (var pen = new Pen(RibbonColors.Current.TabBorder))
            {
                g.DrawLine(pen, 0, TAB_HEIGHT - 1, Width, TAB_HEIGHT - 1);
            }
        }

        private int MeasureTabWidth(Graphics g, string text)
        {
            var size = g.MeasureString(text, SystemFonts.MenuFont);
            return (int)size.Width + TAB_PADDING * 2;
        }

        private void TabHeaderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            // Check app button hover
            var wasHovered = _appMenuButtonHovered;
            _appMenuButtonHovered = _appMenuButtonBounds.Contains(e.Location);
            if (wasHovered != _appMenuButtonHovered)
            {
                _tabHeaderPanel.Invalidate(_appMenuButtonBounds);
            }

            // Check tab hover
            RibbonTab newHoveredTab = null;

            foreach (var tab in _tabs)
            {
                if ((tab.VisibleModes & _currentMode) == 0) continue;
                if (tab.HeaderBounds.Contains(e.Location))
                {
                    newHoveredTab = tab;
                    break;
                }
            }

            if (newHoveredTab == null)
            {
                foreach (var group in _visibleContextualGroups)
                {
                    if (!_contextualTabs.TryGetValue(group, out var tabs)) continue;
                    foreach (var tab in tabs)
                    {
                        if ((tab.VisibleModes & _currentMode) == 0) continue;
                        if (tab.HeaderBounds.Contains(e.Location))
                        {
                            newHoveredTab = tab;
                            break;
                        }
                    }
                    if (newHoveredTab != null) break;
                }
            }

            if (_hoveredTab != newHoveredTab)
            {
                if (_hoveredTab != null)
                    _tabHeaderPanel.Invalidate(_hoveredTab.HeaderBounds);
                _hoveredTab = newHoveredTab;
                if (_hoveredTab != null)
                    _tabHeaderPanel.Invalidate(_hoveredTab.HeaderBounds);
            }
        }

        private void TabHeaderPanel_MouseLeave(object sender, EventArgs e)
        {
            if (_appMenuButtonHovered)
            {
                _appMenuButtonHovered = false;
                _tabHeaderPanel.Invalidate(_appMenuButtonBounds);
            }

            if (_hoveredTab != null)
            {
                var bounds = _hoveredTab.HeaderBounds;
                _hoveredTab = null;
                _tabHeaderPanel.Invalidate(bounds);
            }
        }

        private void TabHeaderPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            // Check app button click
            if (_appMenuButtonBounds.Contains(e.Location))
            {
                ApplicationMenuClicked?.Invoke(this, EventArgs.Empty);
                ShowApplicationMenu();
                return;
            }

            // Check tab click
            foreach (var tab in _tabs)
            {
                if ((tab.VisibleModes & _currentMode) == 0) continue;
                if (tab.HeaderBounds.Contains(e.Location))
                {
                    SelectedTab = tab;
                    return;
                }
            }

            foreach (var group in _visibleContextualGroups)
            {
                if (!_contextualTabs.TryGetValue(group, out var tabs)) continue;
                foreach (var tab in tabs)
                {
                    if ((tab.VisibleModes & _currentMode) == 0) continue;
                    if (tab.HeaderBounds.Contains(e.Location))
                    {
                        SelectedTab = tab;
                        return;
                    }
                }
            }
        }

        #endregion

        #region Application Menu

        private void ShowApplicationMenu()
        {
            if (_applicationMenu == null)
            {
                _applicationMenu = new ApplicationMenu
                {
                    CommandManager = _commandManager
                };

                if (_configuration?.ApplicationMenu != null)
                {
                    _applicationMenu.BuildFromConfiguration(_configuration.ApplicationMenu);
                }
            }

            var screenLocation = _tabHeaderPanel.PointToScreen(new Point(_appMenuButtonBounds.Left, _appMenuButtonBounds.Bottom));
            _applicationMenu.Show(screenLocation);
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _tabHeaderPanel?.Invalidate();
        }
    }
}
