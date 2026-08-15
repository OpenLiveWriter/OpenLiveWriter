// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.Ribbon.Avalonia.Controls
{
    /// <summary>
    /// The main Avalonia ribbon control. Reads a RibbonConfiguration and renders
    /// a tab strip across the top, with a content area below showing the active
    /// tab's groups and controls.
    /// </summary>
    public class AvaloniaRibbonControl : UserControl
    {
        private RibbonConfiguration _configuration;
        private RibbonTabStrip _tabStrip;
        private Border _contentArea;
        private ScrollViewer _contentScrollViewer;
        private StackPanel _groupsPanel;
        private Button _overflowButton;
        private MenuFlyout _overflowFlyout;
        private List<TabConfig> _visibleTabs;
        // The base (non-contextual) tabs, filtered by active modes.
        private List<TabConfig> _baseTabs;
        // The contextual tab group currently shown (None when the caret is in body text).
        private RibbonContextualTabGroup _activeContextualGroup = RibbonContextualTabGroup.None;

        // Buttons currently rendered for the active tab, keyed by command.
        private readonly Dictionary<CommandId, List<RibbonButtonControl>> _buttonsByCommand = new();
        // Host-populated compact dropdowns rendered for the active tab, keyed by command.
        private readonly Dictionary<CommandId, List<ComboBox>> _dropDownsByCommand = new();
        // Spinners rendered for the active tab, keyed by command.
        private readonly Dictionary<CommandId, List<NumericUpDown>> _spinnersByCommand = new();
        // Last-known toggle states, re-applied when the active tab changes.
        private readonly Dictionary<CommandId, bool> _toggleStates = new();
        // Last-known editor combo selections (font/size/style), re-applied on rebuild.
        private readonly Dictionary<CommandId, string> _comboSelections = new();
        // Last-known spinner values (e.g. selected image width/height), re-applied on rebuild.
        private readonly Dictionary<CommandId, decimal?> _spinnerValues = new();
        // Last-known dropdown item data, re-applied when the active tab changes.
        private readonly Dictionary<CommandId, (IReadOnlyList<RibbonGalleryItem> Items, string SelectedId)> _dropDownData = new();
        // Per-combo enable state and optional disabled tooltip (e.g. font controls in Markdown mode).
        private readonly Dictionary<CommandId, (bool Enabled, string DisabledTooltip)> _comboEnabled = new();
        // Guards against re-entrant ComboSelectionChanged while we populate items.
        private bool _populatingDropDowns;
        // Guards against re-entrant SpinnerValueChanged while we reflect editor state.
        private bool _populatingSpinners;

        // When the ribbon host is narrow, groups rebuild with Small buttons and a
        // shorter content band so chrome does not dominate the editor.
        private bool _compactMode;
        private TabConfig _activeTab;
        private const double CompactWidthThreshold = 960;

        /// <summary>
        /// The active application modes. Controls which tabs and groups are visible.
        /// Defaults to Normal + LTR + WithoutPlugins; the Debug tab only appears
        /// when the OLW_DEBUG_RIBBON environment variable is set (1/true).
        /// </summary>
        public RibbonApplicationMode ActiveModes { get; set; } = DefaultActiveModes;

        /// <summary>
        /// The production default mode set. The Debug tab (Terminate Process, Raise
        /// Assertion, etc.) is developer chrome — it is excluded unless
        /// OLW_DEBUG_RIBBON is set to 1/true.
        /// </summary>
        public static RibbonApplicationMode DefaultActiveModes
        {
            get
            {
                var modes = RibbonApplicationMode.Normal |
                            RibbonApplicationMode.LTR |
                            RibbonApplicationMode.WithoutPlugins;
                if (IsDebugRibbonEnabled())
                    modes |= RibbonApplicationMode.Debug;
                return modes;
            }
        }

        private static bool IsDebugRibbonEnabled()
        {
            string value = Environment.GetEnvironmentVariable("OLW_DEBUG_RIBBON");
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Optional host-supplied predicate reporting whether a command is actually
        /// handled. Buttons whose command returns false render disabled with a
        /// "not yet available" tooltip so the ribbon never promises dead commands.
        /// Set before <see cref="LoadConfiguration"/>.
        /// </summary>
        public Func<CommandId, bool> CommandFilter { get; set; }

        /// <summary>True when the ribbon is in the narrow/compact layout.</summary>
        public bool IsCompactMode => _compactMode;

        /// <summary>Horizontal scroller for the active tab's groups (layout harness).</summary>
        public ScrollViewer ContentScrollViewer => _contentScrollViewer;

        /// <summary>Tab strip (includes its own horizontal scroller).</summary>
        public RibbonTabStrip TabStrip => _tabStrip;

        /// <summary>Content docked to the far right of the tab strip (e.g. view tabs).</summary>
        public Control RightContent
        {
            get => _tabStrip?.RightContent;
            set
            {
                if (_tabStrip != null)
                    _tabStrip.RightContent = value;
            }
        }

        /// <summary>Pinned overflow ("More") button listing active-tab commands.</summary>
        public Button OverflowButton => _overflowButton;

        /// <summary>Panel hosting <see cref="RibbonGroupPanel"/> children for the active tab.</summary>
        public Panel GroupsPanel => _groupsPanel;

        /// <summary>
        /// Event raised when a command button in the ribbon is clicked.
        /// </summary>
        public event EventHandler<CommandId> CommandExecuted;

        /// <summary>
        /// Event raised when a ribbon combo box selection changes (e.g. Font family/size).
        /// </summary>
        public event EventHandler<RibbonComboSelectionEventArgs> ComboSelectionChanged;

        /// <summary>
        /// Event raised when a ribbon spinner value changes (e.g. Picture Tools
        /// width/height). Programmatic <see cref="SetSpinnerValue"/> calls do not
        /// raise this event.
        /// </summary>
        public event EventHandler<RibbonSpinnerValueEventArgs> SpinnerValueChanged;

        public AvaloniaRibbonControl()
        {
        }

        /// <summary>
        /// Populates a host-driven compact dropdown (e.g. the blog selector) with items
        /// and selects the given id. Item data is remembered so it survives tab switches.
        /// Programmatic population does not raise <see cref="ComboSelectionChanged"/>.
        /// </summary>
        public void SetDropDownItems(CommandId commandId, IReadOnlyList<RibbonGalleryItem> items, string selectedId)
        {
            _dropDownData[commandId] = (items ?? new List<RibbonGalleryItem>(), selectedId);
            ApplyDropDownData(commandId);
        }

        /// <summary>
        /// Reflects the caret's current value in a ribbon editor combo (e.g. Font
        /// family/size) by selecting the item whose <c>Tag</c> (or, failing that,
        /// <c>Content</c>) matches <paramref name="value"/> case-insensitively. A null
        /// or unmatched value clears the selection. Programmatic selection does not
        /// raise <see cref="ComboSelectionChanged"/>.
        /// </summary>
        public void SetComboSelection(CommandId commandId, string value)
        {
            _comboSelections[commandId] = value;
            ApplyComboSelection(commandId, value);
        }

        /// <summary>
        /// Enables or disables ribbon editor combos (e.g. Font family/size). When
        /// disabled, <paramref name="disabledTooltip"/> is shown on hover. State is
        /// remembered across tab switches.
        /// </summary>
        public void SetComboEnabled(CommandId commandId, bool enabled, string disabledTooltip = null)
        {
            _comboEnabled[commandId] = (enabled, disabledTooltip);
            ApplyComboEnabled(commandId);
        }

        private void ApplyComboEnabled(CommandId commandId)
        {
            if (!_dropDownsByCommand.TryGetValue(commandId, out var combos))
                return;

            bool enabled = true;
            string tooltip = null;
            if (_comboEnabled.TryGetValue(commandId, out var state))
            {
                enabled = state.Enabled;
                tooltip = state.DisabledTooltip;
            }

            foreach (var combo in combos)
            {
                combo.IsEnabled = enabled;
                ToolTip.SetTip(combo, enabled ? null : tooltip);
            }
        }

        private void ApplyComboSelection(CommandId commandId, string value)
        {
            if (!_dropDownsByCommand.TryGetValue(commandId, out var combos))
                return;

            bool previous = _populatingDropDowns;
            _populatingDropDowns = true;
            try
            {
                foreach (var combo in combos)
                {
                    ComboBoxItem match = null;
                    if (!string.IsNullOrEmpty(value))
                    {
                        foreach (var obj in combo.Items)
                        {
                            if (obj is ComboBoxItem item &&
                                (Matches(item.Tag as string, value) || Matches(item.Content as string, value)))
                            {
                                match = item;
                                break;
                            }
                        }
                    }
                    combo.SelectedItem = match;
                }
            }
            finally
            {
                _populatingDropDowns = previous;
            }
        }

        private static bool Matches(string candidate, string value) =>
            candidate != null && string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Reflects editor state in a ribbon spinner (e.g. the selected image's
        /// width/height). The value is remembered so it survives tab switches.
        /// Programmatic sets do not raise <see cref="SpinnerValueChanged"/>.
        /// </summary>
        public void SetSpinnerValue(CommandId commandId, decimal? value)
        {
            _spinnerValues[commandId] = value;
            ApplySpinnerValue(commandId, value);
        }

        private void ApplySpinnerValue(CommandId commandId, decimal? value)
        {
            if (!_spinnersByCommand.TryGetValue(commandId, out var spinners))
                return;

            bool previous = _populatingSpinners;
            _populatingSpinners = true;
            try
            {
                foreach (var spinner in spinners)
                    spinner.Value = value;
            }
            finally
            {
                _populatingSpinners = previous;
            }
        }

        private void ApplyDropDownData(CommandId commandId)
        {
            if (!_dropDownsByCommand.TryGetValue(commandId, out var combos))
                return;
            if (!_dropDownData.TryGetValue(commandId, out var data))
                return;

            bool previous = _populatingDropDowns;
            _populatingDropDowns = true;
            try
            {
                foreach (var combo in combos)
                {
                    combo.Items.Clear();
                    ComboBoxItem selectedItem = null;
                    foreach (var item in data.Items)
                    {
                        var boxItem = new ComboBoxItem { Content = item.Label, Tag = item.Id };
                        combo.Items.Add(boxItem);
                        if (string.Equals(item.Id, data.SelectedId, StringComparison.Ordinal))
                            selectedItem = boxItem;
                    }
                    combo.SelectedItem = selectedItem;
                }
            }
            finally
            {
                _populatingDropDowns = previous;
            }
        }

        /// <summary>
        /// Loads the ribbon from the given configuration and builds all visual elements.
        /// </summary>
        public void LoadConfiguration(RibbonConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            BuildRibbon();
        }

        private void BuildRibbon()
        {
            // Filter tabs by active application modes
            _visibleTabs = new List<TabConfig>();
            foreach (var tab in _configuration.Tabs)
            {
                if ((tab.VisibleModes & ActiveModes) != 0)
                    _visibleTabs.Add(tab);
            }
            _baseTabs = new List<TabConfig>(_visibleTabs);

            var rootPanel = new DockPanel
            {
                LastChildFill = true,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Tab strip
            _tabStrip = new RibbonTabStrip(_visibleTabs);
            _tabStrip.TabChanged += OnTabChanged;
            DockPanel.SetDock(_tabStrip, Dock.Top);
            rootPanel.Children.Add(_tabStrip);

            // Content area (groups for the active tab) — horizontal scroll when
            // groups exceed the window width so they don't clip or force overflow.
            // Hidden bar: macOS apps scroll invisibly (trackpad/wheel); the More
            // overflow menu is the visible affordance.
            _groupsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                Spacing = 0
            };

            _contentScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                // Clip at the viewport so overflowing groups never paint over the
                // More button docked at the right edge.
                ClipToBounds = true,
                Content = _groupsPanel
            };

            _overflowFlyout = new MenuFlyout();
            // Full-height ribbon button with a hairline separator — a first-class
            // part of the band, not a floating pill.
            _overflowButton = new Button
            {
                Content = "More \u25BE",
                MinWidth = 52,
                MinHeight = 0,
                Padding = new Thickness(12, 0),
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xC8)),
                BorderThickness = new Thickness(1, 0, 0, 0),
                CornerRadius = new CornerRadius(0),
                Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                Flyout = _overflowFlyout,
                IsVisible = false
            };
            ToolTip.SetTip(_overflowButton,
                "Commands that may be off-screen — also reachable via scroll");

            var contentDock = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(_overflowButton, Dock.Right);
            contentDock.Children.Add(_overflowButton);
            contentDock.Children.Add(_contentScrollViewer);

            _contentArea = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xF7, 0xF7, 0xF7)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                // Fits large buttons (~58) + group label (~15) + padding, no dead
                // space at the bottom; no MaxHeight so labels are never clipped.
                MinHeight = 96,
                // No MaxHeight — group labels must remain fully visible. Even top/
                // bottom padding so the band reads tight like the tab strip.
                Padding = new Thickness(2, 4, 2, 4),
                ClipToBounds = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = contentDock
            };

            rootPanel.Children.Add(_contentArea);

            HorizontalAlignment = HorizontalAlignment.Stretch;
            Content = rootPanel;

            SizeChanged += OnRibbonSizeChanged;
            _contentScrollViewer.ScrollChanged += (s, e) => UpdateOverflowVisibility();

            // Select first tab
            if (_visibleTabs.Count > 0)
                _tabStrip.SelectedIndex = 0;
        }

        private void OnRibbonSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyCompactForWidth(e.NewSize.Width);
            UpdateOverflowVisibility();
        }

        /// <summary>
        /// Re-evaluates compact mode from the given width (also used after the first
        /// measure so 800px windows start compact without waiting for a user resize).
        /// </summary>
        private void ApplyCompactForWidth(double width)
        {
            if (width <= 0)
                return;

            bool compact = width < CompactWidthThreshold;
            if (compact == _compactMode)
                return;

            _compactMode = compact;
            ApplyContentAreaHeight();
            if (_activeTab != null)
                ShowTab(_activeTab);
        }

        private void ApplyContentAreaHeight()
        {
            if (_contentArea == null)
                return;

            if (_compactMode)
            {
                _contentArea.MinHeight = 72;
                _contentArea.MaxHeight = double.PositiveInfinity;
                _contentArea.Padding = new Thickness(2, 4, 2, 6);
                _contentArea.ClipToBounds = false;
            }
            else
            {
                _contentArea.MinHeight = 124;
                _contentArea.MaxHeight = double.PositiveInfinity;
                _contentArea.Padding = new Thickness(2, 6, 2, 8);
                _contentArea.ClipToBounds = false;
            }
        }

        private void OnTabChanged(object sender, TabChangedEventArgs e)
        {
            ShowTab(e.Tab);
        }

        private void ShowTab(TabConfig tab)
        {
            _activeTab = tab;
            _groupsPanel.Children.Clear();
            _buttonsByCommand.Clear();
            _dropDownsByCommand.Clear();
            _spinnersByCommand.Clear();

            foreach (var group in tab.Groups)
            {
                // Filter groups by active modes
                if ((group.VisibleModes & ActiveModes) == 0)
                    continue;

                var groupPanel = new RibbonGroupPanel(group, compact: _compactMode, commandFilter: CommandFilter);
                groupPanel.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                groupPanel.ComboSelectionChanged += (s, args) =>
                {
                    // Ignore selection events raised while we programmatically fill items.
                    if (_populatingDropDowns)
                        return;
                    ComboSelectionChanged?.Invoke(this, args);
                };
                groupPanel.SpinnerValueChanged += (s, args) =>
                {
                    // Ignore value changes raised while we programmatically reflect state.
                    if (_populatingSpinners)
                        return;
                    SpinnerValueChanged?.Invoke(this, args);
                };
                _groupsPanel.Children.Add(groupPanel);

                foreach (var button in groupPanel.Buttons)
                {
                    if (!_buttonsByCommand.TryGetValue(button.CommandId, out var list))
                    {
                        list = new List<RibbonButtonControl>();
                        _buttonsByCommand[button.CommandId] = list;
                    }
                    list.Add(button);
                }

                foreach (var (commandId, comboBox) in groupPanel.DropDowns)
                {
                    if (!_dropDownsByCommand.TryGetValue(commandId, out var comboList))
                    {
                        comboList = new List<ComboBox>();
                        _dropDownsByCommand[commandId] = comboList;
                    }
                    comboList.Add(comboBox);
                }

                foreach (var (commandId, spinner) in groupPanel.Spinners)
                {
                    if (!_spinnersByCommand.TryGetValue(commandId, out var spinnerList))
                    {
                        spinnerList = new List<NumericUpDown>();
                        _spinnersByCommand[commandId] = spinnerList;
                    }
                    spinnerList.Add(spinner);
                }
            }

            // Re-apply any known toggle states to the freshly built buttons.
            foreach (var kvp in _toggleStates)
                ApplyToggleState(kvp.Key, kvp.Value);

            // Re-apply remembered editor combo selections (font/size/style).
            foreach (var kvp in _comboSelections)
                ApplyComboSelection(kvp.Key, kvp.Value);

            // Re-apply remembered combo enable/tooltip state (font gating in Markdown mode).
            foreach (var kvp in _comboEnabled)
                ApplyComboEnabled(kvp.Key);

            // Re-apply remembered spinner values (e.g. the selected image's size).
            foreach (var kvp in _spinnerValues)
                ApplySpinnerValue(kvp.Key, kvp.Value);

            // Re-apply any known dropdown item data to the freshly built dropdowns.
            foreach (var commandId in _dropDownData.Keys)
                ApplyDropDownData(commandId);

            RebuildOverflowMenu();
            UpdateOverflowVisibility();
        }

        private void RebuildOverflowMenu()
        {
            if (_overflowFlyout == null)
                return;

            _overflowFlyout.Items.Clear();
            foreach (var kvp in _buttonsByCommand.OrderBy(k => k.Key.ToString(), StringComparer.Ordinal))
            {
                var commandId = kvp.Key;
                var sample = kvp.Value.FirstOrDefault();
                string label = sample != null
                    ? (ToolTip.GetTip(sample) as string) ?? commandId.ToString()
                    : commandId.ToString();

                var item = new MenuItem { Header = label };
                item.IsEnabled = CommandFilter?.Invoke(commandId) ?? true;
                item.Click += (s, e) => CommandExecuted?.Invoke(this, commandId);
                _overflowFlyout.Items.Add(item);
            }
        }

        private void UpdateOverflowVisibility()
        {
            if (_overflowButton == null || _contentScrollViewer == null || _groupsPanel == null)
                return;

            // Show More only when content is actually wider than the viewport so
            // clipped commands stay reachable (avoid a permanent More chrome blob).
            double extent = _contentScrollViewer.Extent.Width;
            double viewport = _contentScrollViewer.Viewport.Width;
            double desired = _groupsPanel.DesiredSize.Width;
            bool needsOverflow =
                (viewport > 0 && extent > viewport + 1) ||
                (viewport > 0 && desired > viewport + 1);

            _overflowButton.IsVisible = needsOverflow;
        }

        /// <summary>
        /// Sets the on/off state of a toggle command's button(s) (e.g. Bold,
        /// Italic, AlignCenter). State is remembered across tab switches.
        /// </summary>
        public void SetToggleState(CommandId commandId, bool isChecked)
        {
            _toggleStates[commandId] = isChecked;
            ApplyToggleState(commandId, isChecked);
        }

        private void ApplyToggleState(CommandId commandId, bool isChecked)
        {
            if (_buttonsByCommand.TryGetValue(commandId, out var buttons))
            {
                foreach (var button in buttons)
                    button.SetChecked(isChecked);
            }
        }

        /// <summary>
        /// The contextual tab group currently shown in the tab strip
        /// (<see cref="RibbonContextualTabGroup.None"/> when none is active).
        /// </summary>
        public RibbonContextualTabGroup ActiveContextualGroup => _activeContextualGroup;

        /// <summary>
        /// Shows (and auto-selects) the contextual tab group appropriate for the
        /// current editor selection, or hides all contextual tabs when
        /// <paramref name="group"/> is <see cref="RibbonContextualTabGroup.None"/>.
        /// Idempotent: re-requesting the already-active group does nothing, so the
        /// user isn't yanked back to the contextual tab on every caret move.
        /// </summary>
        public void ActivateContextualTabGroup(RibbonContextualTabGroup group)
        {
            if (group == _activeContextualGroup)
                return;
            if (_tabStrip == null)
                return;

            _activeContextualGroup = group;

            var tabs = new List<TabConfig>(_baseTabs);
            TabConfig toSelect = null;

            if (group != RibbonContextualTabGroup.None && _configuration != null)
            {
                var groupConfig = _configuration.ContextualTabGroups
                    .FirstOrDefault(g => g.GroupType == group);
                if (groupConfig != null)
                {
                    foreach (var tab in groupConfig.Tabs)
                    {
                        if ((tab.VisibleModes & ActiveModes) == 0)
                            continue;
                        tabs.Add(tab);
                        toSelect ??= tab;
                    }
                }
            }

            _visibleTabs = tabs;
            _tabStrip.SetTabs(tabs);

            if (toSelect != null)
                _tabStrip.SelectTab(toSelect);
        }
    }
}
