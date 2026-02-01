// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
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
        // Use shared layout constants where applicable (these are now DPI-aware properties)
        private int TAB_HEIGHT => LayoutConstants.TabHeight;
        private int CONTENT_HEIGHT => LayoutConstants.ContentHeight;
        private int APP_BUTTON_WIDTH => LayoutConstants.PopupWidth;
        private int TAB_PADDING => DisplayHelper.ScaleXCeil(12);  // Horizontal padding on each side of tab text - DPI-scaled
        private int TAB_SPACING => LayoutConstants.TabSpacing;

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
        private bool _qatInTitleBar = false; // QAT is in tab header panel by default
        private Label _fileButton; // Accessible File button overlay for UI Automation (Label supports transparency better)
        
        // Help button state
        private Rectangle _helpButtonBounds;
        private bool _helpButtonHovered;
        private CommandId _helpButtonCommandId;
        private string _helpButtonTooltip;
        private ToolTip _helpButtonToolTip;

        private Panel _tabHeaderPanel;
        private Panel _contentPanel;
        
        // Tab accessibility overlays - transparent Labels for UI Automation
        private readonly Dictionary<RibbonTab, Label> _tabAccessibilityOverlays = new Dictionary<RibbonTab, Label>();

        /// <summary>
        /// Occurs when the selected tab changes.
        /// </summary>
        public event EventHandler SelectedTabChanged;

        /// <summary>
        /// Occurs when the application menu button is clicked.
        /// </summary>
        public event EventHandler ApplicationMenuClicked;

        /// <summary>
        /// Occurs when the help button is clicked.
        /// </summary>
        public event EventHandler HelpButtonClicked;

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
                    // Invalidate both the main panel and the tab header panel
                    // The tab header panel needs to be invalidated to redraw tabs
                    // when mode changes (e.g., showing/hiding Debug tab)
                    _tabHeaderPanel?.Invalidate();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets the Quick Access Toolbar control.
        /// This can be repositioned by the parent form to place it in the title bar.
        /// </summary>
        /// <remarks>
        /// On Windows, the QAT should ideally be placed in the window's title bar area.
        /// The parent form can detach this control and add it to its own Controls collection
        /// at the appropriate position in the title bar.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public QuickAccessToolbar QuickAccessToolbar => _quickAccessToolbar;

        /// <summary>
        /// Gets or sets whether the QAT should be displayed in the title bar area.
        /// When true (default), the QAT is positioned at the top of the ribbon next to the app button.
        /// When false, the QAT is hidden from the ribbon panel (parent form should handle it).
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool QatInTitleBar
        {
            get => _qatInTitleBar;
            set
            {
                if (_qatInTitleBar != value)
                {
                    _qatInTitleBar = value;
                    if (_quickAccessToolbar != null)
                    {
                        _quickAccessToolbar.Visible = !value; // Hide if parent will handle title bar placement
                    }
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
        public int RibbonHeight => TAB_HEIGHT + CONTENT_HEIGHT + DisplayHelper.ScaleYCeil(2);

        public RibbonPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            Height = RibbonHeight;
            Dock = DockStyle.Top;
            BackColor = RibbonColors.Current.RibbonBackground;

            // Content panel - uses Dock=Fill to take remaining space after tab header
            // Tab header panel (added second) will dock to Top first, leaving remaining space for content
            _contentPanel = new Panel
            {
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

            // File button - transparent label overlay for accessibility and click handling
            // Label supports true transparency better than Button in WinForms - DPI-scaled
            var fileButtonMarginX = DisplayHelper.ScaleXCeil(2);
            var fileButtonMarginY = DisplayHelper.ScaleYCeil(1);
            _fileButton = new Label
            {
                Name = "FileButton",
                Text = "",
                Location = new Point(fileButtonMarginX, fileButtonMarginY),
                Size = new Size(APP_BUTTON_WIDTH, TAB_HEIGHT - fileButtonMarginY * 2),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                // Accessibility properties for UI Automation
                AccessibleName = "File",
                AccessibleRole = AccessibleRole.PushButton,
                AccessibleDescription = "Opens the File menu"
            };
            _fileButton.Click += (s, e) =>
            {
                ApplicationMenuClicked?.Invoke(this, EventArgs.Empty);
                ShowApplicationMenu();
            };
            _fileButton.MouseEnter += (s, e) =>
            {
                _appMenuButtonHovered = true;
                // Update bounds when hovering (they're recalculated in paint)
                _tabHeaderPanel.Invalidate();
            };
            _fileButton.MouseLeave += (s, e) =>
            {
                _appMenuButtonHovered = false;
                // Update bounds when leaving (they're recalculated in paint)
                _tabHeaderPanel.Invalidate();
            };
            _tabHeaderPanel.Controls.Add(_fileButton);

            // Quick Access Toolbar - positioned at RibbonPanel level (above tab header panel)
            // This avoids the child control clipping issue
            // QAT is positioned right after the File button, vertically centered in tab header - DPI-scaled
            var qatGap = DisplayHelper.ScaleXCeil(4);
            _quickAccessToolbar = new QuickAccessToolbar
            {
                Location = new Point(APP_BUTTON_WIDTH + qatGap, 0), // Right after File button with scaled gap, aligned to top
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

            // Select first tab that is valid for current mode
            // (tabs start with Visible=false until one is selected)
            foreach (var tab in _tabs)
            {
                if ((tab.VisibleModes & _currentMode) != 0)
                {
                    SelectedTab = tab;
                    break;
                }
            }

            // Configure Quick Access Toolbar BEFORE creating tabs so positioning is correct
            if (_quickAccessToolbar != null && config.QuickAccessToolbar != null)
            {
                _quickAccessToolbar.CommandManager = _commandManager;
                _quickAccessToolbar.SetCommands(config.QuickAccessToolbar.DefaultCommands);
                // Force layout update
                _quickAccessToolbar.Refresh();
            }

            // Configure Help Button
            if (config.HelpButton != null)
            {
                _helpButtonCommandId = config.HelpButton.CommandId;
                _helpButtonTooltip = config.HelpButton.TooltipTitle;
                
                // Create tooltip for help button
                if (_helpButtonToolTip == null)
                {
                    _helpButtonToolTip = new ToolTip();
                }
            }
            else
            {
                _helpButtonCommandId = CommandId.None;
            }

            ResumeLayout(true);
            PerformLayout();
            
            // Force repaint of tab header after QAT is configured
            _tabHeaderPanel?.Refresh();
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
                SizeDefinition = config.SizeDefinition,
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
                    var button = new RibbonButton
                    {
                        CommandId = buttonConfig.CommandId,
                        ButtonType = buttonConfig.ButtonType,
                        CurrentSize = buttonConfig.PreferredSize,
                        Label = buttonConfig.Label,  // Use label override if specified
                        CommandManager = _commandManager
                    };
                    // Populate menu items for dropdown/split buttons
                    if (buttonConfig.MenuItems != null && buttonConfig.MenuItems.Count > 0)
                    {
                        foreach (var menuItemConfig in buttonConfig.MenuItems)
                        {
                            // Get label and image from the command
                            var menuCommand = _commandManager?.GetCommand(menuItemConfig.CommandId);
                            button.MenuItems.Add(new RibbonMenuItem
                            {
                                CommandId = menuItemConfig.CommandId,
                                IsSeparator = menuItemConfig.IsSeparator,
                                Label = menuCommand?.Label ?? menuItemConfig.CommandId.ToString(),
                                Image = menuCommand?.SmallImage
                            });
                        }
                    }
                    control = button;
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
                        TextPosition = galleryConfig.TextPosition,
                        ItemHeight = galleryConfig.ItemHeight,
                        ItemWidth = galleryConfig.ItemWidth,
                        Columns = galleryConfig.Columns,
                        MaxColumns = galleryConfig.MaxColumns,
                        MaxRows = galleryConfig.MaxRows,
                        MinColumnsLarge = galleryConfig.MinColumnsLarge,
                        Layout = galleryConfig.Layout,
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

            // Create accessibility overlay for UI Automation
            CreateTabAccessibilityOverlay(tab);

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

            // Create accessibility overlay for UI Automation
            CreateTabAccessibilityOverlay(tab);
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
            // Remove accessibility overlays
            foreach (var overlay in _tabAccessibilityOverlays.Values)
            {
                if (overlay != null && !overlay.IsDisposed)
                {
                    _tabHeaderPanel.Controls.Remove(overlay);
                    overlay.Dispose();
                }
            }
            _tabAccessibilityOverlays.Clear();

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
                
                // Update group visibility within the tab based on current mode
                tab.UpdateGroupVisibility(_currentMode);
            }

            foreach (var group in _contextualTabs.Values)
            {
                foreach (var tab in group)
                {
                    var isContextVisible = _visibleContextualGroups.Contains(tab.ContextualGroup);
                    var isVisibleForMode = (tab.VisibleModes & _currentMode) != 0;
                    tab.Visible = isContextVisible && isVisibleForMode && tab == _selectedTab;
                    
                    // Update group visibility within the tab based on current mode
                    tab.UpdateGroupVisibility(_currentMode);
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

            // Draw app menu button (File button) - full height minus small top margin - DPI-scaled
            var appButtonMarginX = DisplayHelper.ScaleXCeil(2);
            var appButtonMarginY = DisplayHelper.ScaleYCeil(1);
            _appMenuButtonBounds = new Rectangle(appButtonMarginX, appButtonMarginY, APP_BUTTON_WIDTH, TAB_HEIGHT - appButtonMarginY * 2);
            
            // Sync file button overlay bounds with painted bounds
            if (_fileButton != null && _fileButton.Bounds != _appMenuButtonBounds)
            {
                _fileButton.Bounds = _appMenuButtonBounds;
            }
            
            RibbonRenderer.Instance.DrawAppMenuButton(g, _appMenuButtonBounds,
                _appMenuButtonHovered, _appMenuButtonPressed);

            // Draw tab headers - start after QAT - DPI-scaled spacing
            // QAT is positioned after File button (APP_BUTTON_WIDTH + 4px gap)
            var qatGap = DisplayHelper.ScaleXCeil(4);
            var qatEndX = APP_BUTTON_WIDTH + qatGap; // Start after File button by default
            if (_quickAccessToolbar != null && _quickAccessToolbar.Visible)
            {
                // QAT.Right gives Location.X + Width, add spacing before tabs
                qatEndX = _quickAccessToolbar.Right + qatGap;
            }
            // Minimum start position to avoid any overlap with File button
            var tabStartX = Math.Max(APP_BUTTON_WIDTH + qatGap, qatEndX);
            var x = tabStartX;

            // Track selected tab bounds for border drawing
            Rectangle? selectedTabBounds = null;

            // Regular tabs
            foreach (var tab in _tabs)
            {
                if ((tab.VisibleModes & _currentMode) == 0) continue;

                var tabWidth = MeasureTabWidth(g, tab.Label);
                // Selected tab extends to full height to blend with content area
                var tabBounds = new Rectangle(x, 0, tabWidth, TAB_HEIGHT);
                tab.HeaderBounds = tabBounds;

                // Update accessibility overlay position
                UpdateTabAccessibilityOverlay(tab, tabBounds);

                if (tab == _selectedTab)
                {
                    selectedTabBounds = tabBounds;
                }

                RibbonRenderer.Instance.DrawTabHeader(g, tabBounds, tab.Label,
                    tab == _selectedTab, tab == _hoveredTab, tab.ContextualGroup);

                x += tabWidth + TAB_SPACING;
            }

            // Contextual tabs
            foreach (var group in _visibleContextualGroups)
            {
                if (!_contextualTabs.TryGetValue(group, out var tabs)) continue;

                // Calculate contextual group header bounds
                var groupStartX = x;
                var groupWidth = 0;

                foreach (var tab in tabs)
                {
                    if ((tab.VisibleModes & _currentMode) == 0) continue;
                    var tabWidth = MeasureTabWidth(g, tab.Label);
                    groupWidth += tabWidth + TAB_SPACING;
                }

                if (groupWidth > 0)
                {
                    // Draw colored header bar for contextual group
                    var groupColor = RibbonColors.Current.GetContextualTabColor(group);
                    var headerBounds = new Rectangle(groupStartX, 0, groupWidth, 3);
                    using (var brush = new SolidBrush(groupColor))
                    {
                        g.FillRectangle(brush, headerBounds);
                    }
                }

                // Draw contextual tabs
                foreach (var tab in tabs)
                {
                    if ((tab.VisibleModes & _currentMode) == 0) continue;

                    var tabWidth = MeasureTabWidth(g, tab.Label);
                    var tabBounds = new Rectangle(x, 0, tabWidth, TAB_HEIGHT);
                    tab.HeaderBounds = tabBounds;

                    // Update accessibility overlay position
                    UpdateTabAccessibilityOverlay(tab, tabBounds);

                    if (tab == _selectedTab)
                    {
                        selectedTabBounds = tabBounds;
                    }

                    RibbonRenderer.Instance.DrawTabHeader(g, tabBounds, tab.Label,
                        tab == _selectedTab, tab == _hoveredTab, tab.ContextualGroup);

                    x += tabWidth + TAB_SPACING;
                }
            }

            // Draw help button on the right side (if configured)
            if (_helpButtonCommandId != CommandId.None)
            {
                DrawHelpButton(g);
            }

            // Draw bottom border AFTER tabs, excluding the selected tab's area
            // This avoids z-order issues with drawing-then-erasing
            using (var pen = new Pen(RibbonColors.Current.TabBorder))
            {
                var borderY = TAB_HEIGHT - 1;
                if (selectedTabBounds.HasValue)
                {
                    var sel = selectedTabBounds.Value;
                    // Draw left segment (from 0 to selected tab start)
                    if (sel.Left > 0)
                    {
                        g.DrawLine(pen, 0, borderY, sel.Left, borderY);
                    }
                    // Draw right segment (from selected tab end to panel width)
                    if (sel.Right < Width)
                    {
                        g.DrawLine(pen, sel.Right - 1, borderY, Width, borderY);
                    }
                }
                else
                {
                    // No selected tab - draw full border
                    g.DrawLine(pen, 0, borderY, Width, borderY);
                }
            }
        }

        private void DrawHelpButton(Graphics g)
        {
            // Help button size and margin - DPI-scaled
            var helpButtonSize = DisplayHelper.ScaleXCeil(20);
            var helpButtonMargin = DisplayHelper.ScaleXCeil(8);
            
            // Position help button on the right side of the tab header
            var helpX = _tabHeaderPanel.Width - helpButtonSize - helpButtonMargin;
            var helpY = (TAB_HEIGHT - helpButtonSize) / 2;
            _helpButtonBounds = new Rectangle(helpX, helpY, helpButtonSize, helpButtonSize);

            // Get command info for the help button image
            var helpCommand = _commandManager?.GetCommand(_helpButtonCommandId);
            var helpImage = helpCommand?.SmallImage;

            // Draw hover background
            if (_helpButtonHovered)
            {
                using (var brush = new SolidBrush(RibbonColors.Current.ButtonBackgroundHover))
                {
                    g.FillRectangle(brush, _helpButtonBounds);
                }
                using (var pen = new Pen(RibbonColors.Current.ButtonBorderHover))
                {
                    g.DrawRectangle(pen, _helpButtonBounds.X, _helpButtonBounds.Y, 
                        _helpButtonBounds.Width - 1, _helpButtonBounds.Height - 1);
                }
            }

            // Draw the help icon or fallback "?" text
            if (helpImage != null)
            {
                // Center the image in the button bounds
                var imageX = _helpButtonBounds.X + (_helpButtonBounds.Width - helpImage.Width) / 2;
                var imageY = _helpButtonBounds.Y + (_helpButtonBounds.Height - helpImage.Height) / 2;
                g.DrawImage(helpImage, imageX, imageY, helpImage.Width, helpImage.Height);
            }
            else
            {
                // Fallback: draw "?" text
                using (var font = new Font("Segoe UI", 11f, FontStyle.Bold))
                using (var brush = new SolidBrush(RibbonColors.Current.TabText))
                {
                    var textSize = g.MeasureString("?", font);
                    var textX = _helpButtonBounds.X + (_helpButtonBounds.Width - textSize.Width) / 2;
                    var textY = _helpButtonBounds.Y + (_helpButtonBounds.Height - textSize.Height) / 2;
                    g.DrawString("?", font, brush, textX, textY);
                }
            }
        }

        private int MeasureTabWidth(Graphics g, string text)
        {
            // Use TextRenderer for accurate DPI-aware measurement
            var textSize = TextRenderer.MeasureText(g, text, SystemFonts.MenuFont, 
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            return textSize.Width + TAB_PADDING * 2;
        }

        /// <summary>
        /// Creates a transparent Label overlay for a tab to enable UI Automation discovery.
        /// </summary>
        private void CreateTabAccessibilityOverlay(RibbonTab tab)
        {
            if (_tabHeaderPanel == null || tab == null) return;

            // Don't create duplicate overlays
            if (_tabAccessibilityOverlays.ContainsKey(tab)) return;

            var overlay = new Label
            {
                Name = $"TabOverlay_{tab.Label}",
                Text = "",
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                // Accessibility properties for UI Automation
                AccessibleName = tab.Label,
                AccessibleRole = AccessibleRole.PageTab,
                AccessibleDescription = $"Ribbon tab: {tab.Label}"
            };

            // Wire up click handler to select the tab
            overlay.Click += (s, e) =>
            {
                SelectedTab = tab;
            };

            // Wire up mouse enter/leave to sync hover state
            overlay.MouseEnter += (s, e) =>
            {
                if (_hoveredTab != tab)
                {
                    if (_hoveredTab != null)
                        _tabHeaderPanel.Invalidate(_hoveredTab.HeaderBounds);
                    _hoveredTab = tab;
                    _tabHeaderPanel.Invalidate(tab.HeaderBounds);
                }
            };

            overlay.MouseLeave += (s, e) =>
            {
                if (_hoveredTab == tab)
                {
                    _hoveredTab = null;
                    _tabHeaderPanel.Invalidate(tab.HeaderBounds);
                }
            };

            _tabHeaderPanel.Controls.Add(overlay);
            _tabAccessibilityOverlays[tab] = overlay;

            // Initial position will be set during paint
            overlay.Visible = false;
        }

        /// <summary>
        /// Updates the position and visibility of a tab's accessibility overlay.
        /// </summary>
        private void UpdateTabAccessibilityOverlay(RibbonTab tab, Rectangle tabBounds)
        {
            if (!_tabAccessibilityOverlays.TryGetValue(tab, out var overlay)) return;
            if (overlay == null || overlay.IsDisposed) return;

            // Update overlay bounds to match painted tab bounds
            overlay.Bounds = tabBounds;
            
            // Show overlay only if tab is visible for current mode
            var isVisibleForMode = (tab.VisibleModes & _currentMode) != 0;
            var isContextualVisible = tab.ContextualGroup == RibbonContextualTabGroup.None ||
                                      _visibleContextualGroups.Contains(tab.ContextualGroup);
            overlay.Visible = isVisibleForMode && isContextualVisible;
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

            // Check help button hover
            if (_helpButtonCommandId != CommandId.None)
            {
                var wasHelpHovered = _helpButtonHovered;
                _helpButtonHovered = _helpButtonBounds.Contains(e.Location);
                if (wasHelpHovered != _helpButtonHovered)
                {
                    _tabHeaderPanel.Invalidate(_helpButtonBounds);
                    
                    // Show/hide tooltip
                    if (_helpButtonHovered && _helpButtonToolTip != null && !string.IsNullOrEmpty(_helpButtonTooltip))
                    {
                        var screenPos = _tabHeaderPanel.PointToScreen(new Point(_helpButtonBounds.Left, _helpButtonBounds.Bottom));
                        _helpButtonToolTip.Show(_helpButtonTooltip, _tabHeaderPanel, 
                            _helpButtonBounds.Left, _helpButtonBounds.Bottom + 2, 3000);
                    }
                    else if (!_helpButtonHovered && _helpButtonToolTip != null)
                    {
                        _helpButtonToolTip.Hide(_tabHeaderPanel);
                    }
                }
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

            if (_helpButtonHovered)
            {
                _helpButtonHovered = false;
                _tabHeaderPanel.Invalidate(_helpButtonBounds);
                _helpButtonToolTip?.Hide(_tabHeaderPanel);
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

            // Check help button click
            if (_helpButtonCommandId != CommandId.None && _helpButtonBounds.Contains(e.Location))
            {
                HelpButtonClicked?.Invoke(this, EventArgs.Empty);
                ExecuteHelpCommand();
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

        #region Help Button

        private void ExecuteHelpCommand()
        {
            if (_helpButtonCommandId == CommandId.None) return;

            var command = _commandManager?.GetCommand(_helpButtonCommandId);
            if (command != null && command.Enabled)
            {
                command.PerformExecute();
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
