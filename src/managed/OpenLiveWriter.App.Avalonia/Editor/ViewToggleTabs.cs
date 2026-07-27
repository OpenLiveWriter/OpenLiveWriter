// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Edit / Source / Preview view tabs, docked at the far right of the ribbon tab
    /// strip (inline with Home/Insert/Blog Account). The active view gets the accent
    /// color; inactive tabs stay neutral. The shell wires <see cref="ViewRequested"/>
    /// to the editor's view switch and sets <see cref="ActiveView"/> to reflect it.
    /// </summary>
    public class ViewToggleTabs : StackPanel
    {
        private static readonly IBrush ActiveBrush = new SolidColorBrush(Color.FromRgb(0x0A, 0x84, 0xFF));
        private static readonly IBrush ActiveBorder = new SolidColorBrush(Color.FromRgb(0x00, 0x66, 0xD6));
        private static readonly IBrush InactiveForeground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
        private static readonly IBrush HoverBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xEA));

        private readonly ToggleButton _edit;
        private readonly ToggleButton _source;
        private readonly ToggleButton _preview;
        private string _activeView = "edit";
        private bool _suppress;

        /// <summary>Raised when the user clicks a view tab ("edit" / "source" / "preview").</summary>
        public event EventHandler<string> ViewRequested;

        public ViewToggleTabs()
        {
            Orientation = Orientation.Horizontal;
            Spacing = 2;
            VerticalAlignment = VerticalAlignment.Bottom;

            _edit = CreateTab("Edit", "edit");
            _source = CreateTab("Source", "source");
            _preview = CreateTab("Preview", "preview");
            Children.Add(_edit);
            Children.Add(_source);
            Children.Add(_preview);
            ApplyState();
        }

        /// <summary>The currently active view ("edit" / "source" / "preview").</summary>
        public string ActiveView
        {
            get => _activeView;
            set
            {
                if (value != "edit" && value != "source" && value != "preview")
                    return;
                _activeView = value;
                ApplyState();
            }
        }

        private ToggleButton CreateTab(string label, string view)
        {
            var button = new ToggleButton
            {
                Name = view == "edit" ? "EditViewButton"
                    : view == "source" ? "SourceViewButton"
                    : "PreviewViewButton",
                Content = label,
                Padding = new Thickness(12, 6),
                MinHeight = 28,
                MinWidth = 68,
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(4, 4, 0, 0),
                BorderThickness = new Thickness(1),
            };
            button.Click += (s, e) =>
            {
                if (_suppress)
                    return;
                _activeView = view;
                ApplyState();
                ViewRequested?.Invoke(this, view);
            };
            return button;
        }

        private void ApplyState()
        {
            _suppress = true;
            try
            {
                ApplyTo(_edit, _activeView == "edit");
                ApplyTo(_source, _activeView == "source");
                ApplyTo(_preview, _activeView == "preview");
            }
            finally
            {
                _suppress = false;
            }
        }

        private static void ApplyTo(ToggleButton button, bool active)
        {
            button.IsChecked = active;
            if (active)
            {
                button.Background = ActiveBrush;
                button.BorderBrush = ActiveBorder;
                button.Foreground = Brushes.White;
                button.FontWeight = FontWeight.SemiBold;
            }
            else
            {
                button.Background = Brushes.Transparent;
                button.BorderBrush = Brushes.Transparent;
                button.Foreground = InactiveForeground;
                button.FontWeight = FontWeight.Normal;
            }
        }
    }
}
