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
    /// Large mode: glyph/label on top, caption below (tall button).
    /// Small mode: glyph + label horizontally (compact button).
    /// Prefer readable text/geometry over flat gray placeholder squares.
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
            ToolTip.SetTip(this, _label);

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

            stack.Children.Add(CreateGlyphVisual(large: true));

            // Label (possibly with dropdown indicator)
            var labelText = ShortLabel(_label);
            if (hasDropdown)
                labelText += " \u25BE";

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
            MinHeight = 24;
            Padding = new Thickness(4, 1);

            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 4
            };

            stack.Children.Add(CreateGlyphVisual(large: false));

            var labelText = ShortLabel(_label);
            if (hasDropdown)
                labelText += " \u25BE";

            // Formatting toggles are recognizable from the glyph alone — skip the
            // redundant word label to keep small rows tight.
            if (!IsFormattingGlyphCommand(_commandId))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = labelText,
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            Content = stack;
        }

        /// <summary>
        /// Readable glyph (styled letter or unicode) instead of a flat gray square.
        /// </summary>
        private Control CreateGlyphVisual(bool large)
        {
            var (glyph, fontWeight, fontStyle, decorations) = GetGlyphStyle();
            double size = large ? 18 : 13;
            double box = large ? 32 : 18;

            var text = new TextBlock
            {
                Text = glyph,
                FontSize = size,
                FontWeight = fontWeight,
                FontStyle = fontStyle,
                TextDecorations = decorations,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Soft outline so the glyph reads as a control without looking like a
            // missing-image placeholder.
            return new Border
            {
                Width = box,
                Height = box,
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                BorderThickness = new Thickness(large ? 1 : 0),
                CornerRadius = new CornerRadius(3),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = text
            };
        }

        private (string Glyph, FontWeight Weight, FontStyle Style, TextDecorationCollection Decorations) GetGlyphStyle()
        {
            string glyph = GlyphForCommand(_commandId);
            switch (_commandId)
            {
                case CommandId.Bold:
                    return (glyph, FontWeight.Bold, FontStyle.Normal, null);
                case CommandId.Italic:
                    return (glyph, FontWeight.Normal, FontStyle.Italic, null);
                case CommandId.Underline:
                    return (glyph, FontWeight.Normal, FontStyle.Normal, TextDecorations.Underline);
                case CommandId.Strikethrough:
                    return (glyph, FontWeight.Normal, FontStyle.Normal, TextDecorations.Strikethrough);
                case CommandId.Bullets:
                case CommandId.Blockquote:
                    return (glyph, FontWeight.Bold, FontStyle.Normal, null);
                case CommandId.Numbers:
                case CommandId.SavePost:
                    return (glyph, FontWeight.SemiBold, FontStyle.Normal, null);
                case CommandId.PostAndPublish:
                    return (glyph, FontWeight.Bold, FontStyle.Normal, null);
                default:
                    return (glyph, FontWeight.SemiBold, FontStyle.Normal, null);
            }
        }

        private static bool IsFormattingGlyphCommand(CommandId id) =>
            id is CommandId.Bold or CommandId.Italic or CommandId.Underline
                or CommandId.Strikethrough or CommandId.Subscript or CommandId.Superscript
                or CommandId.Bullets or CommandId.Numbers or CommandId.Blockquote
                or CommandId.AlignLeft or CommandId.AlignCenter
                or CommandId.AlignRight or CommandId.Justify;

        /// <summary>
        /// Readable glyph text used for the given command (testable without a visual tree).
        /// </summary>
        public static string GlyphForCommand(CommandId commandId)
        {
            switch (commandId)
            {
                case CommandId.Bold: return "B";
                case CommandId.Italic: return "I";
                case CommandId.Underline: return "U";
                case CommandId.Strikethrough: return "S";
                case CommandId.Subscript: return "X\u2082";
                case CommandId.Superscript: return "X\u00B2";
                case CommandId.Bullets: return "\u2022";
                case CommandId.Numbers: return "1.";
                case CommandId.Blockquote: return "\u201C";
                case CommandId.AlignLeft:
                case CommandId.AlignCenter:
                case CommandId.AlignRight:
                case CommandId.Justify:
                    return "\u2630";
                case CommandId.Paste:
                case CommandId.CopyCommand:
                    return "\u2398";
                case CommandId.Cut:
                    return "\u2702";
                case CommandId.Undo:
                    return "\u21B6";
                case CommandId.Redo:
                    return "\u21B7";
                case CommandId.InsertLink:
                    return "\u29C9";
                case CommandId.FindButton:
                case CommandId.FindAndReplace:
                    return "\u2315";
                case CommandId.PostAndPublish:
                    return "\u2191";
                case CommandId.SavePost:
                    return "S";
                default:
                    var label = CommandLabelHelper.GetLabel(commandId);
                    return string.IsNullOrEmpty(label)
                        ? "?"
                        : label.Trim()[0].ToString().ToUpperInvariant();
            }
        }

        /// <summary>Shortens long ribbon captions so Large buttons stay readable.</summary>
        private static string ShortLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
                return label;
            // Keep first line / word group under ~14 chars for wrap friendliness.
            if (label.Length <= 14)
                return label;
            int space = label.IndexOf(' ');
            if (space > 0 && space <= 14)
                return label.Substring(0, space);
            return label.Substring(0, 12) + "\u2026";
        }
    }
}
