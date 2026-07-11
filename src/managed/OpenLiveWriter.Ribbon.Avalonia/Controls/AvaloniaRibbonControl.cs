// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
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
        private List<TabConfig> _visibleTabs;

        // Buttons currently rendered for the active tab, keyed by command.
        private readonly Dictionary<CommandId, List<RibbonButtonControl>> _buttonsByCommand = new();
        // Last-known toggle states, re-applied when the active tab changes.
        private readonly Dictionary<CommandId, bool> _toggleStates = new();

        /// <summary>
        /// The active application modes. Controls which tabs and groups are visible.
        /// Defaults to Normal + LTR + WithoutPlugins + Debug for development.
        /// </summary>
        public RibbonApplicationMode ActiveModes { get; set; } =
            RibbonApplicationMode.Normal |
            RibbonApplicationMode.LTR |
            RibbonApplicationMode.WithoutPlugins |
            RibbonApplicationMode.Debug;

        /// <summary>
        /// Event raised when a command button in the ribbon is clicked.
        /// </summary>
        public event EventHandler<CommandId> CommandExecuted;

        /// <summary>
        /// Event raised when a ribbon combo box selection changes (e.g. Font family/size).
        /// </summary>
        public event EventHandler<RibbonComboSelectionEventArgs> ComboSelectionChanged;

        public AvaloniaRibbonControl()
        {
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

            var rootPanel = new DockPanel();

            // Tab strip
            _tabStrip = new RibbonTabStrip(_visibleTabs);
            _tabStrip.TabChanged += OnTabChanged;
            DockPanel.SetDock(_tabStrip, Dock.Top);
            rootPanel.Children.Add(_tabStrip);

            // Content area (groups for the active tab)
            _groupsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                Spacing = 0
            };

            _contentScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                Content = _groupsPanel
            };

            _contentArea = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                MinHeight = 95,
                Padding = new Thickness(4, 4, 4, 0),
                Child = _contentScrollViewer
            };

            rootPanel.Children.Add(_contentArea);

            Content = rootPanel;

            // Select first tab
            if (_visibleTabs.Count > 0)
                _tabStrip.SelectedIndex = 0;
        }

        private void OnTabChanged(object sender, TabChangedEventArgs e)
        {
            ShowTab(e.Tab);
        }

        private void ShowTab(TabConfig tab)
        {
            _groupsPanel.Children.Clear();
            _buttonsByCommand.Clear();

            foreach (var group in tab.Groups)
            {
                // Filter groups by active modes
                if ((group.VisibleModes & ActiveModes) == 0)
                    continue;

                var groupPanel = new RibbonGroupPanel(group);
                groupPanel.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                groupPanel.ComboSelectionChanged += (s, args) => ComboSelectionChanged?.Invoke(this, args);
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
            }

            // Re-apply any known toggle states to the freshly built buttons.
            foreach (var kvp in _toggleStates)
                ApplyToggleState(kvp.Key, kvp.Value);
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
    }
}
