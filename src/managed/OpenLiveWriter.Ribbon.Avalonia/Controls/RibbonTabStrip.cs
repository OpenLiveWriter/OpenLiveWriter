// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Configuration;

namespace OpenLiveWriter.Ribbon.Avalonia.Controls
{
    /// <summary>
    /// Horizontal strip of tab buttons. Highlights the active tab and raises
    /// TabChanged when the user clicks a different tab.
    /// </summary>
    public class RibbonTabStrip : Border
    {
        private List<TabConfig> _tabs;
        private readonly StackPanel _tabPanel;
        private readonly ScrollViewer _tabScrollViewer;
        private readonly List<ToggleButton> _tabButtons = new List<ToggleButton>();
        private int _selectedIndex = -1;
        private Control _rightContent;

        // Contextual tabs get a distinct accent (mimics the Windows contextual-tab
        // coloring) so it's obvious they appeared in response to a selection.
        private static readonly IBrush ContextualBrush = new SolidColorBrush(Color.FromRgb(0x6B, 0x3F, 0xA0));

        /// <summary>
        /// Optional content docked to the far right of the tab strip (e.g. the
        /// Edit/Source/Preview view tabs). Sits inline with the ribbon tabs.
        /// </summary>
        public Control RightContent
        {
            get => _rightContent;
            set
            {
                if (ReferenceEquals(_rightContent, value)) return;
                if (_rightContent != null)
                    _rightDock.Children.Remove(_rightContent);
                _rightContent = value;
                if (_rightContent != null)
                    _rightDock.Children.Add(_rightContent);
            }
        }

        private readonly StackPanel _rightDock = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        /// <summary>
        /// Event raised when the active tab changes.
        /// </summary>
        public event EventHandler<TabChangedEventArgs> TabChanged;

        /// <summary>Horizontal scroller wrapping the tab buttons (layout harness).</summary>
        public ScrollViewer TabScrollViewer => _tabScrollViewer;

        /// <summary>Rendered tab toggle buttons, in strip order.</summary>
        public IReadOnlyList<ToggleButton> TabButtons => _tabButtons;

        public RibbonTabStrip(List<TabConfig> tabs)
        {
            _tabs = tabs ?? throw new ArgumentNullException(nameof(tabs));

            Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0));
            BorderThickness = new Thickness(0, 0, 0, 1);
            Padding = new Thickness(8, 4, 8, 0);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            ClipToBounds = true;

            _tabPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 2
            };

            BuildTabs();

            // Horizontal scroll when many tabs (or contextual tabs) won't fit —
            // StackPanel alone would clip or overflow the window edge.
            _tabScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = _tabPanel
            };

            var dock = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(_rightDock, Dock.Right);
            dock.Children.Add(_rightDock);
            dock.Children.Add(_tabScrollViewer);
            Child = dock;
        }

        /// <summary>The tab configs currently rendered, in order.</summary>
        public IReadOnlyList<TabConfig> Tabs => _tabs;

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

        /// <summary>
        /// Replaces the rendered tabs (e.g. to add/remove contextual tabs). Preserves
        /// the current selection by reference when the previously selected tab is still
        /// present; otherwise selects the first tab. Always raises <see cref="TabChanged"/>
        /// for the resulting selection so the content area rebuilds.
        /// </summary>
        public void SetTabs(List<TabConfig> tabs)
        {
            TabConfig previouslySelected =
                (_selectedIndex >= 0 && _selectedIndex < _tabs.Count) ? _tabs[_selectedIndex] : null;

            _tabs = tabs ?? throw new ArgumentNullException(nameof(tabs));
            _tabButtons.Clear();
            _tabPanel.Children.Clear();
            _selectedIndex = -1;

            BuildTabs();

            int restore = previouslySelected != null ? _tabs.IndexOf(previouslySelected) : -1;
            SelectedIndex = restore >= 0 ? restore : (_tabs.Count > 0 ? 0 : -1);
        }

        /// <summary>Selects the given tab config (no-op when it isn't present).</summary>
        public void SelectTab(TabConfig tab)
        {
            int index = _tabs.IndexOf(tab);
            if (index >= 0)
                SelectedIndex = index;
        }

        private void BuildTabs()
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                bool contextual = tab.ContextualGroup != RibbonContextualTabGroup.None;
                var button = new ToggleButton
                {
                    Content = tab.Label,
                    Padding = new Thickness(12, 6),
                    MinHeight = 28,
                    MinWidth = 48,
                    FontSize = 12,
                    FontWeight = contextual ? FontWeight.SemiBold : FontWeight.Normal,
                    Foreground = contextual ? ContextualBrush : Brushes.Black,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(1, contextual ? 2 : 1, 1, 0),
                    BorderBrush = contextual ? ContextualBrush : Brushes.Transparent,
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
