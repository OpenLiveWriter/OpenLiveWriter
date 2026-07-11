// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
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
    /// Renders a single ribbon button, supporting large and small modes.
    /// Large mode: icon placeholder on top, label below (tall button).
    /// Small mode: icon placeholder + label horizontally (compact button).
    /// </summary>
    public class RibbonButtonControl : Button
    {
        private readonly CommandId _commandId;
        private readonly RibbonGroupSize _preferredSize;
        private readonly RibbonButtonType _buttonType;
        private readonly string _label;

        public RibbonButtonControl(ButtonConfig config)
        {
            _commandId = config.CommandId;
            _preferredSize = config.PreferredSize;
            _buttonType = config.ButtonType;
            _label = config.Label ?? CommandLabelHelper.GetLabel(config.CommandId);
            Focusable = false; // Prevent stealing focus from WebView editor
            BuildContent();
        }

        public RibbonButtonControl(ToggleButtonConfig config)
        {
            _commandId = config.CommandId;
            _preferredSize = config.PreferredSize;
            _buttonType = RibbonButtonType.ToggleButton;
            _label = CommandLabelHelper.GetLabel(config.CommandId);
            Focusable = false;
            BuildContent();
        }

        /// <summary>
        /// Creates a button for a generic control config (used for gallery/combobox/spinner/colorpicker placeholders).
        /// </summary>
        public RibbonButtonControl(CommandId commandId, string label, RibbonGroupSize size)
        {
            _commandId = commandId;
            _preferredSize = size;
            _buttonType = RibbonButtonType.Button;
            _label = label;
            Focusable = false;
            BuildContent();
        }

        public CommandId CommandId => _commandId;

        /// <summary>
        /// True when this control represents a toggle button (e.g. Bold, Italic),
        /// which can reflect an on/off state from the editor's current selection.
        /// </summary>
        public bool IsToggleButton => _buttonType == RibbonButtonType.ToggleButton;

        private bool _isChecked;

        /// <summary>
        /// Sets the visual pressed/checked state for a toggle button. No-op for
        /// non-toggle buttons.
        /// </summary>
        public void SetChecked(bool isChecked)
        {
            if (!IsToggleButton || _isChecked == isChecked)
                return;

            _isChecked = isChecked;
            UpdateCheckedVisual();
        }

        private void UpdateCheckedVisual()
        {
            if (_isChecked)
            {
                Background = new SolidColorBrush(Color.FromArgb(0x66, 0x5B, 0x9B, 0xD5));
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x5B, 0x9B, 0xD5));
            }
            else
            {
                Background = Brushes.Transparent;
                BorderBrush = Brushes.Transparent;
            }
        }

        /// <summary>
        /// Event raised when this ribbon button is clicked.
        /// </summary>
        public event EventHandler<CommandId> CommandExecuted;

        private void BuildContent()
        {
            Classes.Add("ribbon-button");
            Background = Brushes.Transparent;
            BorderThickness = new Thickness(1);
            BorderBrush = Brushes.Transparent;
            Padding = new Thickness(4, 2);
            CornerRadius = new CornerRadius(3);

            var hasDropdown = _buttonType == RibbonButtonType.DropDownButton ||
                              _buttonType == RibbonButtonType.SplitButton;

            if (_preferredSize == RibbonGroupSize.Large)
            {
                BuildLargeButton(hasDropdown);
            }
            else
            {
                BuildSmallButton(hasDropdown);
            }

            Click += (s, e) => CommandExecuted?.Invoke(this, _commandId);
        }

        private void BuildLargeButton(bool hasDropdown)
        {
            MinWidth = 48;
            MinHeight = 66;

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 2
            };

            // Icon placeholder
            var iconBorder = new Border
            {
                Width = 32,
                Height = 32,
                Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = GetIconPlaceholder(),
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            stack.Children.Add(iconBorder);

            // Label (possibly with dropdown indicator)
            var labelText = _label;
            if (hasDropdown)
                labelText += " \u25BE"; // small down triangle

            var textBlock = new TextBlock
            {
                Text = labelText,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 72
            };
            stack.Children.Add(textBlock);

            Content = stack;
        }

        private void BuildSmallButton(bool hasDropdown)
        {
            MinHeight = 22;
            Padding = new Thickness(4, 1);

            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 4
            };

            // Small icon placeholder
            var iconBorder = new Border
            {
                Width = 16,
                Height = 16,
                Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                CornerRadius = new CornerRadius(2),
                Child = new TextBlock
                {
                    Text = GetIconPlaceholder(),
                    FontSize = 9,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            stack.Children.Add(iconBorder);

            // Label
            var labelText = _label;
            if (hasDropdown)
                labelText += " \u25BE";

            stack.Children.Add(new TextBlock
            {
                Text = labelText,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            });

            Content = stack;
        }

        private string GetIconPlaceholder()
        {
            // Return a single-char icon hint based on command type
            switch (_commandId)
            {
                case CommandId.Paste: return "\u2398";       // Clipboard
                case CommandId.Cut: return "\u2702";          // Scissors
                case CommandId.CopyCommand: return "\u2750";  // Copy
                case CommandId.Bold: return "B";
                case CommandId.Italic: return "I";
                case CommandId.Underline: return "U";
                case CommandId.Strikethrough: return "S";
                case CommandId.Subscript: return "\u2082";
                case CommandId.Superscript: return "\u00B2";
                case CommandId.Bullets: return "\u2022";
                case CommandId.Numbers: return "#";
                case CommandId.AlignLeft: return "\u2261";
                case CommandId.AlignCenter: return "\u2261";
                case CommandId.AlignRight: return "\u2261";
                case CommandId.Justify: return "\u2261";
                default: return "\u2756";  // Diamond
            }
        }
    }
}
