// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.Ribbon.Avalonia.Controls
{
    /// <summary>
    /// Horizontal strip of tab buttons. Highlights the active tab and raises
    /// TabChanged when the user clicks a different tab.
    /// </summary>
    public class RibbonTabStrip : Border
    {
        private readonly List<TabConfig> _tabs;
        private readonly StackPanel _tabPanel;
        private readonly List<ToggleButton> _tabButtons = new List<ToggleButton>();
        private int _selectedIndex = -1;

        /// <summary>
        /// Event raised when the active tab changes.
        /// </summary>
        public event EventHandler<TabChangedEventArgs> TabChanged;

        public RibbonTabStrip(List<TabConfig> tabs)
        {
            _tabs = tabs ?? throw new ArgumentNullException(nameof(tabs));

            Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0));
            BorderThickness = new Thickness(0, 0, 0, 1);
            Padding = new Thickness(8, 4, 8, 0);

            _tabPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 2
            };

            BuildTabs();

            Child = _tabPanel;
        }

        /// <summary>
        /// Gets or sets the currently selected tab index.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < 0 || value >= _tabButtons.Count) return;
                if (value == _selectedIndex) return;

                // Uncheck previous
                if (_selectedIndex >= 0 && _selectedIndex < _tabButtons.Count)
                    _tabButtons[_selectedIndex].IsChecked = false;

                _selectedIndex = value;
                _tabButtons[_selectedIndex].IsChecked = true;

                TabChanged?.Invoke(this, new TabChangedEventArgs(_selectedIndex, _tabs[_selectedIndex]));
            }
        }

        private void BuildTabs()
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                var button = new ToggleButton
                {
                    Content = tab.Label,
                    Padding = new Thickness(12, 6),
                    FontSize = 12,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(1, 1, 1, 0),
                    BorderBrush = Brushes.Transparent,
                    CornerRadius = new CornerRadius(4, 4, 0, 0),
                    Tag = i
                };

                int index = i; // Capture for closure
                button.Click += (s, e) =>
                {
                    SelectedIndex = index;
                };

                _tabButtons.Add(button);
                _tabPanel.Children.Add(button);
            }
        }
    }

    /// <summary>
    /// Event arguments for tab change events.
    /// </summary>
    public class TabChangedEventArgs : EventArgs
    {
        public int TabIndex { get; }
        public TabConfig Tab { get; }

        public TabChangedEventArgs(int tabIndex, TabConfig tab)
        {
            TabIndex = tabIndex;
            Tab = tab;
        }
    }
}
