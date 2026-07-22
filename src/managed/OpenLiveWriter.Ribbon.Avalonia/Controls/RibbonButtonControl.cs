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
        // Pure dropdown buttons only open their flyout — the menu items carry the
        // commands, so the parent's own CommandId is not raised on click.
        private readonly bool _suppressOwnCommand;

        public RibbonButtonControl(ButtonConfig config, Func<CommandId, bool> commandFilter = null)
        {
            _commandId = config.CommandId;
            _preferredSize = config.PreferredSize;
            _buttonType = config.ButtonType;
            _label = config.Label ?? CommandLabelHelper.GetLabel(config.CommandId);
            Focusable = false; // Prevent stealing focus from WebView editor

            if (config.ButtonType == RibbonButtonType.DropDownButton && config.MenuItems.Count > 0)
            {
                Flyout = BuildMenuFlyout(config.MenuItems, commandFilter);
                _suppressOwnCommand = true;
            }

            BuildContent();
        }

        // Builds the dropdown menu for a DropDownButton. Items whose command the
        // host does not handle render disabled so the menu never offers dead commands.
        private MenuFlyout BuildMenuFlyout(List<MenuItemConfig> items, Func<CommandId, bool> commandFilter)
        {
            var flyout = new MenuFlyout();
            foreach (MenuItemConfig item in items)
            {
                if (item.IsSeparator)
                {
                    flyout.Items.Add(new Separator());
                    continue;
                }

                CommandId itemCommand = item.CommandId;
                var menuItem = new MenuItem { Header = CommandLabelHelper.GetLabel(itemCommand) };
                menuItem.IsEnabled = commandFilter?.Invoke(itemCommand) ?? true;
                if (!menuItem.IsEnabled)
                    ToolTip.SetTip(menuItem, "Not yet available");
                menuItem.Click += (s, e) => CommandExecuted?.Invoke(this, itemCommand);
                flyout.Items.Add(menuItem);
            }
            return flyout;
        }

        public RibbonButtonControl(ToggleButtonConfig config)
        {
            _commandId = config.CommandId;
            _preferredSize = config.PreferredSize;
            _buttonType = RibbonButtonType.ToggleButton;
            _label = config.Label ?? CommandLabelHelper.GetLabel(config.CommandId);
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

            Click += (s, e) =>
            {
                if (!_suppressOwnCommand)
                    CommandExecuted?.Invoke(this, _commandId);
            };
        }

        private void BuildLargeButton(bool hasDropdown)
        {
            MinWidth = 48;
            MinHeight = 58;

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
            // Formatting toggles are icon-only — give them a square hit target.
            if (IsFormattingGlyphCommand(_commandId))
            {
                MinWidth = 30;
                Padding = new Thickness(4, 2);
            }
            else
            {
                Padding = new Thickness(4, 1);
            }

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
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.None
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
            bool multiChar = glyph != null && glyph.Length > 1;
            double size = large ? 18 : (multiChar ? 11 : 14);
            double boxW = large ? 32 : (multiChar ? 28 : 22);
            double boxH = large ? 32 : 22;

            var text = new TextBlock
            {
                Text = glyph,
                FontSize = size,
                FontWeight = fontWeight,
                FontStyle = fontStyle,
                TextDecorations = decorations,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Soft outline so the glyph reads as a control without looking like a
            // missing-image placeholder.
            return new Border
            {
                Width = boxW,
                Height = boxH,
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xD4, 0xD4, 0xD4)),
                BorderThickness = new Thickness(large ? 1 : 0),
                CornerRadius = new CornerRadius(3),
                HorizontalAlignment = HorizontalAlignment.Center,
                ClipToBounds = true,
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
                case CommandId.AlignLeft:
                case CommandId.AlignCenter:
                case CommandId.AlignRight:
                case CommandId.Justify:
                    return (glyph, FontWeight.Bold, FontStyle.Normal, null);
                case CommandId.Numbers:
                case CommandId.SavePost:
                case CommandId.PostAsDraft:
                case CommandId.ClearFormatting:
                case CommandId.CheckSpelling:
                case CommandId.WordCount:
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
                or CommandId.AlignRight or CommandId.Justify
                or CommandId.ClearFormatting;

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
                case CommandId.AlignLeft: return "\u25E7";   // ◧ left-filled
                case CommandId.AlignCenter: return "\u25A3"; // ▣ centered square
                case CommandId.AlignRight: return "\u25E8";  // ◨ right-filled
                case CommandId.Justify: return "\u2630";     // ☰ justified lines
                case CommandId.Paste:
                    return "\u2398"; // ⎘
                case CommandId.CopyCommand:
                    return "\u2750"; // ❐ copy pages
                case CommandId.Cut:
                    return "\u2702"; // ✂
                case CommandId.Undo:
                    return "\u21B6";
                case CommandId.Redo:
                    return "\u21B7";
                case CommandId.InsertLink:
                    return "\u29C9"; // ⧉
                case CommandId.InsertImageSplit:
                case CommandId.InsertPictureFromFile:
                case CommandId.WebImage:
                    return "\u25EB"; // ◫ framed picture-like
                case CommandId.InsertVideoSplit:
                case CommandId.InsertVideoFromFile:
                case CommandId.InsertVideoFromWeb:
                case CommandId.InsertVideoFromService:
                    return "\u25B6"; // ▶ play
                case CommandId.ClearFormatting:
                    return "Tx";
                case CommandId.CheckSpelling:
                    return "Abc";
                case CommandId.WordCount:
                    return "#";
                case CommandId.SelectAll:
                    return "\u25A3";
                case CommandId.FindButton:
                case CommandId.FindAndReplace:
                    return "\u2315"; // ⌕
                case CommandId.PostAndPublish:
                    return "\u2191";
                case CommandId.SavePost:
                case CommandId.PostAsDraft:
                    return "\u2399"; // ⎙ save-ish
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
