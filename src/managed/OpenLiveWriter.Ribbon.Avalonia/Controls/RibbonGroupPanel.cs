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
    /// Renders a ribbon group: a bordered panel containing controls laid out
    /// horizontally, with a group label at the bottom.
    /// </summary>
    public class RibbonGroupPanel : Border
    {
        private readonly GroupConfig _config;
        private readonly List<RibbonButtonControl> _buttons = new();
        private readonly List<(CommandId CommandId, ComboBox ComboBox)> _dropDowns = new();

        /// <summary>
        /// Event raised when a command button within this group is clicked.
        /// </summary>
        public event EventHandler<CommandId> CommandExecuted;

        /// <summary>
        /// Event raised when a combo box selection changes within this group.
        /// </summary>
        public event EventHandler<RibbonComboSelectionEventArgs> ComboSelectionChanged;

        /// <summary>
        /// All ribbon buttons created within this group, in creation order.
        /// Used by the ribbon control to sync toggle state from the editor.
        /// </summary>
        public IReadOnlyList<RibbonButtonControl> Buttons => _buttons;

        /// <summary>
        /// Host-populated compact dropdowns (e.g. the blog selector) created in this
        /// group, keyed by command. The ribbon control fills these from application data.
        /// </summary>
        public IReadOnlyList<(CommandId CommandId, ComboBox ComboBox)> DropDowns => _dropDowns;

        public RibbonGroupPanel(GroupConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            BuildGroup();
        }

        private void BuildGroup()
        {
            Classes.Add("ribbon-group");

            // Group border styling - subtle right separator
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0));
            BorderThickness = new Thickness(0, 0, 1, 0);
            Padding = new Thickness(4, 2, 4, 0);
            Margin = new Thickness(0, 0, 2, 0);

            var outerStack = new DockPanel();

            // Group label at bottom
            var label = new TextBlock
            {
                Text = _config.Label,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 2)
            };
            DockPanel.SetDock(label, Dock.Bottom);
            outerStack.Children.Add(label);

            // Controls area
            var controlsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                Spacing = 2,
                MinHeight = 66
            };

            // Determine layout strategy based on SizeDefinition
            var controls = _config.Controls;
            var sizeDefinition = _config.SizeDefinition ?? "";

            bool hasLargeAndSmall = sizeDefinition.Contains("OneLarge") &&
                                    (sizeDefinition.Contains("TwoSmall") || sizeDefinition == "OneLargeAndTwoSmall");
            bool hasFontGroup = sizeDefinition == "FontGroup";

            if (hasLargeAndSmall)
            {
                BuildOneLargeAndSmallLayout(controlsPanel, controls);
            }
            else if (sizeDefinition == "OneLargeComboSmall")
            {
                BuildOneLargeComboSmallLayout(controlsPanel, controls);
            }
            else if (hasFontGroup)
            {
                BuildFontGroupLayout(controlsPanel, controls);
            }
            else if (sizeDefinition.Contains("Gallery") && !sizeDefinition.Contains("Large"))
            {
                BuildGalleryLayout(controlsPanel, controls);
            }
            else
            {
                BuildStandardLayout(controlsPanel, controls, sizeDefinition);
            }

            outerStack.Children.Add(controlsPanel);

            Child = outerStack;
        }

        /// <summary>
        /// Layout: One large button + two small buttons stacked vertically.
        /// </summary>
        private void BuildOneLargeAndSmallLayout(StackPanel panel, List<ControlConfig> controls)
        {
            if (controls.Count == 0) return;

            // First control is the large one
            var largeControl = CreateControl(controls[0], RibbonGroupSize.Large);
            panel.Children.Add(largeControl);

            // Remaining controls stacked vertically
            if (controls.Count > 1)
            {
                var smallStack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 2
                };
                for (int i = 1; i < controls.Count; i++)
                {
                    smallStack.Children.Add(CreateControl(controls[i], RibbonGroupSize.Small));
                }
                panel.Children.Add(smallStack);
            }
        }

        /// <summary>
        /// Layout: One large button + combo/gallery + small button stacked.
        /// Used for Publish group (Publish + blog selector + Save Draft).
        /// </summary>
        private void BuildOneLargeComboSmallLayout(StackPanel panel, List<ControlConfig> controls)
        {
            if (controls.Count == 0) return;

            // First control is the large button
            panel.Children.Add(CreateControl(controls[0], RibbonGroupSize.Large));

            // Remaining stacked vertically
            if (controls.Count > 1)
            {
                var rightStack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 2
                };
                for (int i = 1; i < controls.Count; i++)
                {
                    rightStack.Children.Add(CreateControl(controls[i], RibbonGroupSize.Small));
                }
                panel.Children.Add(rightStack);
            }
        }

        /// <summary>
        /// Layout for the Font group: two combo boxes on top row,
        /// formatting buttons on rows below.
        /// </summary>
        private void BuildFontGroupLayout(StackPanel panel, List<ControlConfig> controls)
        {
            var outerStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 2
            };

            // First row: combo boxes
            var comboRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4
            };

            // Second row: toggle buttons
            var buttonRow = new WrapPanel
            {
                Orientation = Orientation.Horizontal
            };

            foreach (var control in controls)
            {
                if (control is ComboBoxConfig combo)
                {
                    comboRow.Children.Add(CreateEditorComboBox(combo));
                }
                else
                {
                    buttonRow.Children.Add(CreateControl(control, RibbonGroupSize.Small));
                }
            }

            outerStack.Children.Add(comboRow);
            outerStack.Children.Add(buttonRow);
            panel.Children.Add(outerStack);
        }

        /// <summary>
        /// Layout for groups with galleries.
        /// </summary>
        private void BuildGalleryLayout(StackPanel panel, List<ControlConfig> controls)
        {
            foreach (var control in controls)
            {
                var size = GetControlPreferredSize(control);
                panel.Children.Add(CreateControl(control, size));
            }
        }

        /// <summary>
        /// Standard layout: buttons laid out based on their preferred sizes.
        /// Large buttons are shown individually; small/medium buttons are stacked.
        /// </summary>
        private void BuildStandardLayout(StackPanel panel, List<ControlConfig> controls, string sizeDefinition)
        {
            bool allLarge = sizeDefinition.Contains("Large");
            bool allSmallOrMedium = !allLarge && (sizeDefinition.Contains("Buttons") || sizeDefinition.Contains("Button"));

            if (allLarge)
            {
                // All buttons shown as large
                foreach (var control in controls)
                {
                    panel.Children.Add(CreateControl(control, RibbonGroupSize.Large));
                }
            }
            else if (allSmallOrMedium)
            {
                // Stack medium/small buttons vertically in columns of 3
                var currentStack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 1
                };

                int count = 0;
                foreach (var control in controls)
                {
                    currentStack.Children.Add(CreateControl(control, RibbonGroupSize.Small));
                    count++;

                    if (count >= 3)
                    {
                        panel.Children.Add(currentStack);
                        currentStack = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            VerticalAlignment = VerticalAlignment.Center,
                            Spacing = 1
                        };
                        count = 0;
                    }
                }

                if (count > 0)
                    panel.Children.Add(currentStack);
            }
            else
            {
                // Fallback: each control at its own preferred size
                foreach (var control in controls)
                {
                    var size = GetControlPreferredSize(control);
                    panel.Children.Add(CreateControl(control, size));
                }
            }
        }

        // Common font families offered by the Font Family combo.
        private static readonly string[] FontFamilies =
        {
            "Segoe UI", "Arial", "Calibri", "Cambria", "Comic Sans MS",
            "Courier New", "Georgia", "Helvetica", "Tahoma", "Times New Roman",
            "Trebuchet MS", "Verdana"
        };

        // HTML font sizes (execCommand fontSize uses the 1-7 scale). Labels show the
        // approximate point size for familiarity.
        private static readonly (string Label, string Value)[] FontSizes =
        {
            ("8", "1"), ("10", "2"), ("12", "3"), ("14", "4"),
            ("18", "5"), ("24", "6"), ("36", "7")
        };

        /// <summary>
        /// Creates a Font group combo box (font family or size) populated with
        /// choices and wired to raise <see cref="ComboSelectionChanged"/>.
        /// </summary>
        private Control CreateEditorComboBox(ComboBoxConfig combo)
        {
            var comboBox = new global::Avalonia.Controls.ComboBox
            {
                Width = combo.PreferredWidth,
                Height = 24,
                PlaceholderText = CommandLabelHelper.GetLabel(combo.CommandId),
                VerticalAlignment = VerticalAlignment.Center
            };

            var commandId = combo.CommandId;
            if (commandId == CommandId.FontFamily)
            {
                foreach (var family in FontFamilies)
                    comboBox.Items.Add(new ComboBoxItem { Content = family, Tag = family });

                comboBox.SelectionChanged += (s, e) =>
                {
                    if (comboBox.SelectedItem is ComboBoxItem item && item.Content is string family)
                        ComboSelectionChanged?.Invoke(this, new RibbonComboSelectionEventArgs(commandId, family));
                };
                // Registered so the host can reflect the caret's current font family.
                _dropDowns.Add((commandId, comboBox));
            }
            else if (commandId == CommandId.FontSize)
            {
                // Tag carries the HTML 1-7 value so the host can select by the value
                // reported from the editor's getState().
                foreach (var (label, value) in FontSizes)
                    comboBox.Items.Add(new ComboBoxItem { Content = label, Tag = value });

                comboBox.SelectionChanged += (s, e) =>
                {
                    if (comboBox.SelectedItem is ComboBoxItem item && item.Tag is string value)
                        ComboSelectionChanged?.Invoke(this, new RibbonComboSelectionEventArgs(commandId, value));
                };
                _dropDowns.Add((commandId, comboBox));
            }

            return comboBox;
        }

        private Control CreateControl(ControlConfig config, RibbonGroupSize sizeOverride)
        {
            Control control;

            switch (config)
            {
                case ButtonConfig button:
                    var btn = new RibbonButtonControl(new ButtonConfig
                    {
                        CommandId = button.CommandId,
                        ButtonType = button.ButtonType,
                        PreferredSize = sizeOverride,
                        Label = button.Label
                    });
                    btn.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                    control = btn;
                    break;

                case ToggleButtonConfig toggle:
                    var toggleBtn = new RibbonButtonControl(new ToggleButtonConfig
                    {
                        CommandId = toggle.CommandId,
                        PreferredSize = sizeOverride
                    });
                    toggleBtn.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                    control = toggleBtn;
                    break;

                case GalleryConfig gallery:
                    control = CreateGalleryPlaceholder(gallery, sizeOverride);
                    break;

                case ComboBoxConfig combo:
                    control = new global::Avalonia.Controls.ComboBox
                    {
                        Width = combo.PreferredWidth,
                        Height = 24,
                        PlaceholderText = CommandLabelHelper.GetLabel(combo.CommandId),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    break;

                case SpinnerConfig spinner:
                    control = new NumericUpDown
                    {
                        Minimum = spinner.MinValue,
                        Maximum = spinner.MaxValue,
                        Increment = spinner.Increment,
                        Width = 80,
                        Height = 24,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    break;

                case ColorPickerConfig color:
                    control = CreateColorPicker(color);
                    break;

                case SeparatorConfig _:
                    control = new Border
                    {
                        Width = 1,
                        Background = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
                        Margin = new Thickness(2, 4),
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    break;

                default:
                    control = new TextBlock
                    {
                        Text = config.CommandId.ToString(),
                        FontSize = 11,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    break;
            }

            if (control is RibbonButtonControl createdButton)
                _buttons.Add(createdButton);

            return control;
        }

        private Control CreateGalleryPlaceholder(GalleryConfig gallery, RibbonGroupSize sizeOverride)
        {
            var label = CommandLabelHelper.GetLabel(gallery.CommandId);

            if (gallery.GalleryType == RibbonGalleryType.InRibbon)
            {
                // The semantic HTML styles gallery is interactive: clicking it opens
                // a flyout of block styles (Normal/Heading 1-6/Preformatted); the
                // chosen tag is raised via ComboSelectionChanged so the host applies
                // it through the editor's formatBlock bridge.
                if (gallery.CommandId == CommandId.SemanticHtmlGallery)
                    return CreateSemanticHtmlGallery(gallery, label);

                // In-ribbon gallery: show a bordered area with the gallery name
                var columns = gallery.MinColumnsLarge > 0 ? gallery.MinColumnsLarge : gallery.Columns;
                var itemWidth = gallery.ItemWidth > 0 ? gallery.ItemWidth : 48;
                var width = Math.Max(columns * itemWidth + 16, 80);

                return new Border
                {
                    Width = width,
                    MinHeight = 58,
                    Background = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(4),
                    Child = new TextBlock
                    {
                        Text = label,
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    }
                };
            }
            else if (gallery.GalleryType == RibbonGalleryType.CompactDropDown)
            {
                // Compact dropdown (like the blog selector). Items are supplied by the
                // host (e.g. stored blog accounts); selecting one raises
                // ComboSelectionChanged with the item's id so the shell can act on it.
                var comboBox = new global::Avalonia.Controls.ComboBox
                {
                    Width = 140,
                    Height = 24,
                    PlaceholderText = label,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var commandId = gallery.CommandId;
                comboBox.SelectionChanged += (s, e) =>
                {
                    if (comboBox.SelectedItem is ComboBoxItem item && item.Tag is string id)
                        ComboSelectionChanged?.Invoke(this, new RibbonComboSelectionEventArgs(commandId, id));
                };

                _dropDowns.Add((commandId, comboBox));
                return comboBox;
            }
            else
            {
                // Dropdown gallery: render as a button with dropdown indicator
                var btn = new RibbonButtonControl(
                    gallery.CommandId,
                    label,
                    sizeOverride);
                btn.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                return btn;
            }
        }

        // The semantic block styles offered by the SemanticHtmlGallery flyout.
        // Values are the formatBlock tags applied by the editor bridge.
        private static readonly (string Label, string Tag)[] SemanticHtmlStyleItems =
        {
            ("Normal", "p"),
            ("Heading 1", "h1"),
            ("Heading 2", "h2"),
            ("Heading 3", "h3"),
            ("Heading 4", "h4"),
            ("Heading 5", "h5"),
            ("Heading 6", "h6"),
            ("Preformatted", "pre"),
        };

        /// <summary>
        /// Builds the interactive "HTML styles" gallery: a bordered button that
        /// opens a flyout of semantic block styles. Selecting a style raises
        /// <see cref="ComboSelectionChanged"/> with the formatBlock tag as the value.
        /// </summary>
        private Control CreateSemanticHtmlGallery(GalleryConfig gallery, string label)
        {
            var columns = gallery.MinColumnsLarge > 0 ? gallery.MinColumnsLarge : gallery.Columns;
            var itemWidth = gallery.ItemWidth > 0 ? gallery.ItemWidth : 48;
            var width = Math.Max(columns * itemWidth + 16, 80);

            var flyout = new MenuFlyout();
            foreach (var (styleLabel, tag) in SemanticHtmlStyleItems)
            {
                var item = new MenuItem { Header = styleLabel };
                var capturedTag = tag;
                item.Click += (s, e) => ComboSelectionChanged?.Invoke(
                    this, new RibbonComboSelectionEventArgs(CommandId.SemanticHtmlGallery, capturedTag));
                flyout.Items.Add(item);
            }

            var button = new Button
            {
                Width = width,
                MinHeight = 58,
                Focusable = false,
                Background = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Flyout = flyout,
                Content = new TextBlock
                {
                    Text = label + " \u25BE",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            return button;
        }

        // Standard text-color palette (Office-like). Values are #RRGGBB hex.
        private static readonly string[] StandardColorPalette =
        {
            "#000000", "#444444", "#666666", "#999999", "#CCCCCC", "#FFFFFF",
            "#FF0000", "#FF9900", "#FFFF00", "#00FF00", "#00FFFF", "#0000FF",
            "#9900FF", "#FF00FF", "#990000", "#B45F06", "#BF9000", "#38761D",
            "#134F5C", "#0B5394", "#351C75", "#741B47",
        };

        // Highlight palette — brighter background swatches.
        private static readonly string[] HighlightColorPalette =
        {
            "#FFFF00", "#00FF00", "#00FFFF", "#FF00FF", "#0000FF", "#FF0000",
            "#000080", "#008080", "#008000", "#800080", "#800000", "#808000",
            "#808080", "#C0C0C0", "#FFFFFF", "#000000",
        };

        /// <summary>
        /// Builds a color-picker button: a small labelled button that opens a
        /// flyout of color swatches. Selecting a swatch raises
        /// <see cref="ComboSelectionChanged"/> with the chosen <c>#RRGGBB</c> value.
        /// </summary>
        private Control CreateColorPicker(ColorPickerConfig config)
        {
            var palette = config.ColorTemplate == RibbonColorTemplate.HighlightColors
                ? HighlightColorPalette
                : StandardColorPalette;

            var swatchPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 6 * 22 + 8,
                Margin = new Thickness(4)
            };

            var flyout = new Flyout { Content = swatchPanel };
            var commandId = config.CommandId;

            foreach (var hex in palette)
            {
                var color = Color.Parse(hex);
                var swatch = new Button
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(1),
                    Padding = new Thickness(0),
                    Focusable = false,
                    Background = new SolidColorBrush(color),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    BorderThickness = new Thickness(1),
                    [ToolTip.TipProperty] = hex
                };
                var capturedHex = hex;
                swatch.Click += (s, e) =>
                {
                    ComboSelectionChanged?.Invoke(this,
                        new RibbonComboSelectionEventArgs(commandId, capturedHex));
                    flyout.Hide();
                };
                swatchPanel.Children.Add(swatch);
            }

            var button = new Button
            {
                Focusable = false,
                MinHeight = 22,
                Padding = new Thickness(4, 1),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(3),
                Flyout = flyout,
                Content = new TextBlock
                {
                    Text = CommandLabelHelper.GetLabel(commandId) + " \u25BE",
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            return button;
        }

        private static RibbonGroupSize GetControlPreferredSize(ControlConfig config)
        {
            switch (config)
            {
                case ButtonConfig button: return button.PreferredSize;
                case ToggleButtonConfig toggle: return toggle.PreferredSize;
                default: return RibbonGroupSize.Small;
            }
        }
    }

    /// <summary>
    /// Carries a ribbon combo box selection (command + chosen value) to the host.
    /// </summary>
    public class RibbonComboSelectionEventArgs : EventArgs
    {
        public RibbonComboSelectionEventArgs(CommandId commandId, string value)
        {
            CommandId = commandId;
            Value = value;
        }

        public CommandId CommandId { get; }
        public string Value { get; }
    }

    /// <summary>
    /// A host-supplied item for a compact-dropdown gallery (e.g. a blog account in the
    /// blog selector). <see cref="Id"/> is the opaque value raised on selection;
    /// <see cref="Label"/> is the display text.
    /// </summary>
    public sealed class RibbonGalleryItem
    {
        public RibbonGalleryItem(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public string Id { get; }
        public string Label { get; }
    }
}
