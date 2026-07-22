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
        private readonly bool _compact;
        private readonly Func<CommandId, bool> _commandFilter;
        private readonly List<RibbonButtonControl> _buttons = new();
        private readonly List<(CommandId CommandId, ComboBox ComboBox)> _dropDowns = new();
        private readonly List<(CommandId CommandId, NumericUpDown Spinner)> _spinners = new();

        /// <summary>
        /// Event raised when a command button within this group is clicked.
        /// </summary>
        public event EventHandler<CommandId> CommandExecuted;

        /// <summary>
        /// Event raised when a combo box selection changes within this group.
        /// </summary>
        public event EventHandler<RibbonComboSelectionEventArgs> ComboSelectionChanged;

        /// <summary>
        /// Event raised when a spinner (NumericUpDown) value changes within this
        /// group. The ribbon control re-raises this so the shell can apply values
        /// (e.g. Picture Tools width/height) to the editor.
        /// </summary>
        public event EventHandler<RibbonSpinnerValueEventArgs> SpinnerValueChanged;

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

        /// <summary>
        /// Spinners created in this group, keyed by command. The ribbon control
        /// reflects editor state into these (e.g. the selected image's size).
        /// </summary>
        public IReadOnlyList<(CommandId CommandId, NumericUpDown Spinner)> Spinners => _spinners;

        public RibbonGroupPanel(GroupConfig config, bool compact = false, Func<CommandId, bool> commandFilter = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _compact = compact;
            _commandFilter = commandFilter;
            BuildGroup();
        }

        private void BuildGroup()
        {
            Classes.Add("ribbon-group");

            // Group border styling - subtle right separator
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xC8));
            BorderThickness = new Thickness(0, 0, 1, 0);
            Padding = _compact ? new Thickness(6, 2, 6, 2) : new Thickness(8, 4, 8, 2);
            Margin = new Thickness(0);
            VerticalAlignment = VerticalAlignment.Stretch;
            Background = Brushes.Transparent;

            var outerStack = new DockPanel();

            // Group label band: reserved height so descenders never clip.
            var labelBand = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinHeight = _compact ? 18 : 22,
                Margin = _compact ? new Thickness(0, 2, 0, 2) : new Thickness(0, 2, 0, 2)
            };
            labelBand.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                Margin = new Thickness(2, 0, 2, 2),
                HorizontalAlignment = HorizontalAlignment.Stretch
            });
            labelBand.Children.Add(new TextBlock
            {
                Text = _config.Label,
                FontSize = _compact ? 10 : 11,
                FontWeight = FontWeight.SemiBold,
                LineHeight = _compact ? 14 : 15,
                Foreground = new SolidColorBrush(Color.FromRgb(0x5A, 0x5A, 0x5A)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.None,
                TextWrapping = TextWrapping.NoWrap
            });
            DockPanel.SetDock(labelBand, Dock.Bottom);
            outerStack.Children.Add(labelBand);

            // Controls area
            var controlsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 2,
                MinHeight = _compact ? 40 : 58
            };

            var controls = _config.Controls;
            var sizeDefinition = _config.SizeDefinition ?? "";

            if (_compact)
            {
                // Narrow window: force Small buttons in short columns so groups
                // wrap via horizontal scroll instead of leaving tall empty chrome.
                BuildCompactLayout(controlsPanel, controls);
            }
            else
            {
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
                else if (sizeDefinition == "SevenSmallButtons")
                {
                    BuildParagraphLayout(controlsPanel, controls);
                }
                else if (sizeDefinition == "FourButtons")
                {
                    BuildFourButtonGrid(controlsPanel, controls);
                }
                else if (sizeDefinition.Contains("Gallery") && !sizeDefinition.Contains("Large"))
                {
                    BuildGalleryLayout(controlsPanel, controls);
                }
                else
                {
                    BuildStandardLayout(controlsPanel, controls, sizeDefinition);
                }
            }

            outerStack.Children.Add(controlsPanel);

            Child = outerStack;
        }

        /// <summary>
        /// Compact layout: preserve known group structures (large+small, font, etc.)
        /// so narrow windows stay readable instead of reshuffling into broken columns.
        /// </summary>
        private void BuildCompactLayout(StackPanel panel, List<ControlConfig> controls)
        {
            var sizeDefinition = _config.SizeDefinition ?? "";

            if (sizeDefinition.Contains("OneLarge") &&
                (sizeDefinition.Contains("TwoSmall") || sizeDefinition == "OneLargeAndTwoSmall"))
            {
                BuildOneLargeAndSmallLayout(panel, controls);
                return;
            }

            if (sizeDefinition == "OneLargeComboSmall")
            {
                BuildOneLargeComboSmallLayout(panel, controls);
                return;
            }

            if (sizeDefinition == "FontGroup")
            {
                BuildFontGroupLayout(panel, controls);
                return;
            }

            if (sizeDefinition == "SevenSmallButtons")
            {
                BuildParagraphLayout(panel, controls);
                return;
            }

            if (sizeDefinition == "FourButtons")
            {
                BuildFourButtonGrid(panel, controls);
                return;
            }

            var currentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 1
            };

            int count = 0;
            foreach (var control in controls)
            {
                if (control is SeparatorConfig)
                {
                    if (count > 0)
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
                    panel.Children.Add(CreateControl(control, RibbonGroupSize.Small));
                    continue;
                }

                currentStack.Children.Add(CreateControl(control, RibbonGroupSize.Small));
                count++;

                if (count >= 2)
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
        /// Layout for the Font group: font/size combos on top, formatting toggles
        /// and compact color pickers on a single aligned row below.
        /// </summary>
        private void BuildFontGroupLayout(StackPanel panel, List<ControlConfig> controls)
        {
            var outerStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 3
            };

            var comboRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                VerticalAlignment = VerticalAlignment.Center
            };

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 1,
                VerticalAlignment = VerticalAlignment.Center
            };

            foreach (var control in controls)
            {
                if (control is ComboBoxConfig combo)
                    comboRow.Children.Add(CreateEditorComboBox(combo));
                else
                    buttonRow.Children.Add(CreateControl(control, RibbonGroupSize.Small));
            }

            outerStack.Children.Add(comboRow);
            outerStack.Children.Add(buttonRow);
            panel.Children.Add(outerStack);
        }

        /// <summary>
        /// Paragraph group: lists column + alignment column (Office-like 2-stack).
        /// </summary>
        private void BuildParagraphLayout(StackPanel panel, List<ControlConfig> controls)
        {
            var lists = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 2
            };
            var aligns = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 2
            };

            // First three controls are list/quote; remainder are alignment.
            for (int i = 0; i < controls.Count; i++)
            {
                var host = i < 3 ? lists : aligns;
                host.Children.Add(CreateControl(controls[i], RibbonGroupSize.Small));
            }

            panel.Children.Add(lists);
            panel.Children.Add(new Border
            {
                Width = 1,
                Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                Margin = new Thickness(3, 2),
                VerticalAlignment = VerticalAlignment.Stretch
            });
            panel.Children.Add(aligns);
        }

        /// <summary>
        /// Editing group: 2×2 grid of medium-style small buttons (avoids a lonely
        /// fourth button in a 3-high column).
        /// </summary>
        private void BuildFourButtonGrid(StackPanel panel, List<ControlConfig> controls)
        {
            var currentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 2
            };

            int count = 0;
            foreach (var control in controls)
            {
                currentStack.Children.Add(CreateControl(control, RibbonGroupSize.Small));
                count++;

                if (count >= 2)
                {
                    panel.Children.Add(currentStack);
                    currentStack = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Center,
                        Spacing = 2
                    };
                    count = 0;
                }
            }

            if (count > 0)
                panel.Children.Add(currentStack);
        }

        /// <summary>
        /// Layout for groups with galleries.
        /// </summary>
        private void BuildGalleryLayout(StackPanel panel, List<ControlConfig> controls)
        {
            foreach (var control in controls)
            {
                var size = GetControlPreferredSize(control);
                var created = CreateControl(control, size);
                // Single-control galleries (Style) look less sparse when centered.
                if (controls.Count == 1)
                    created.VerticalAlignment = VerticalAlignment.Center;
                panel.Children.Add(created);
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
                    Spacing = 2
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
                            Spacing = 2
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

        // Pixel font sizes offered by the Font Size combo (applied via the bridge's
        // setFontSizePx; the editor reports the caret's computed px size back).
        private static readonly string[] FontSizesPx =
        {
            "9", "10", "11", "12", "14", "16", "18", "24", "32", "36", "48"
        };

        /// <summary>
        /// Creates a Font group combo box (font family or size) populated with
        /// choices and wired to raise <see cref="ComboSelectionChanged"/>.
        /// </summary>
        private Control CreateEditorComboBox(ComboBoxConfig combo)
        {
            double width = combo.PreferredWidth > 0 ? combo.PreferredWidth : 120;
            // Font size needs room for the selected label ("12", "14", "36") plus
            // the Avalonia combo chrome/arrow; Prefer Width == MinWidth so layout
            // cannot shrink below the configured preferred size.
            if (combo.CommandId == CommandId.FontSize)
                width = Math.Max(width, 80);

            var comboBox = new global::Avalonia.Controls.ComboBox
            {
                Width = width,
                MinWidth = width,
                Height = 26,
                MinHeight = 24,
                PlaceholderText = CommandLabelHelper.GetLabel(combo.CommandId),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left
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
                // Tag carries the px value so the host can select by the computed
                // px size reported from the editor's getState().
                foreach (string size in FontSizesPx)
                    comboBox.Items.Add(new ComboBoxItem { Content = size, Tag = size });

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
                    var buttonCopy = new ButtonConfig
                    {
                        CommandId = button.CommandId,
                        ButtonType = button.ButtonType,
                        PreferredSize = sizeOverride,
                        Label = button.Label
                    };
                    foreach (MenuItemConfig menuItem in button.MenuItems)
                        buttonCopy.MenuItems.Add(menuItem);
                    var btn = new RibbonButtonControl(buttonCopy, _commandFilter);
                    btn.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                    control = btn;
                    break;

                case ToggleButtonConfig toggle:
                    var toggleBtn = new RibbonButtonControl(new ToggleButtonConfig
                    {
                        CommandId = toggle.CommandId,
                        PreferredSize = sizeOverride,
                        Label = toggle.Label
                    });
                    toggleBtn.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                    control = toggleBtn;
                    break;

                case GalleryConfig gallery:
                    control = CreateGalleryPlaceholder(gallery, sizeOverride);
                    break;

                case ComboBoxConfig combo:
                    // Font family/size must use the populated editor combo path even in
                    // compact layout — a bare ComboBox only shows a truncated placeholder.
                    if (combo.CommandId == CommandId.FontFamily || combo.CommandId == CommandId.FontSize)
                        control = CreateEditorComboBox(combo);
                    else
                    {
                        control = new global::Avalonia.Controls.ComboBox
                        {
                            Width = combo.PreferredWidth > 0 ? combo.PreferredWidth : 120,
                            MinWidth = combo.PreferredWidth > 0 ? combo.PreferredWidth : 80,
                            Height = 26,
                            MinHeight = 24,
                            PlaceholderText = CommandLabelHelper.GetLabel(combo.CommandId),
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                    break;

                case SpinnerConfig spinner:
                    var numeric = new NumericUpDown
                    {
                        Minimum = spinner.MinValue,
                        Maximum = spinner.MaxValue,
                        Increment = spinner.Increment,
                        // The Fluent template's spin buttons take a fixed ~68px on
                        // the right; 112 leaves room for a 4-digit value.
                        Width = 112,
                        Height = 26,
                        MinHeight = 24,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    CommandId spinnerCommand = spinner.CommandId;
                    numeric.ValueChanged += (s, e) =>
                        SpinnerValueChanged?.Invoke(this, new RibbonSpinnerValueEventArgs(spinnerCommand, numeric.Value));
                    _spinners.Add((spinner.CommandId, numeric));
                    control = numeric;
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
            {
                // Disable commands the host says are unhandled so the ribbon does
                // not advertise dead commands (P0: "dead buttons that look alive").
                if (_commandFilter != null && !_commandFilter(createdButton.CommandId))
                {
                    createdButton.IsEnabled = false;
                    string tip = ToolTip.GetTip(createdButton) as string;
                    ToolTip.SetTip(createdButton,
                        string.IsNullOrEmpty(tip) ? "Not yet available" : tip + " (not yet available)");
                }
                _buttons.Add(createdButton);
            }

            return control;
        }

        private Control CreateGalleryPlaceholder(GalleryConfig gallery, RibbonGroupSize sizeOverride)
        {
            var label = CommandLabelHelper.GetLabel(gallery.CommandId);

            if (gallery.GalleryType == RibbonGalleryType.InRibbon)
            {
                // The semantic HTML styles gallery is a Style ComboBox (current
                // selection + dropdown), not an always-visible in-ribbon list.
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
                    Width = 128,
                    MinWidth = 120,
                    Height = 26,
                    MinHeight = 24,
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
        /// Builds the interactive "HTML styles" selector as a ComboBox (Windows OLW
        /// Style dropdown UX): shows the current block style and opens to pick
        /// Normal / Heading 1-6 / Preformatted. Registered in <see cref="_dropDowns"/>
        /// so the host can reflect <c>FormatState.BlockTag</c> via SetComboSelection.
        /// </summary>
        private Control CreateSemanticHtmlGallery(GalleryConfig gallery, string label)
        {
            // Style selector: show current block style (Normal / Heading N / …).
            // Sized for "Preformatted" without leaving a huge empty combo.
            var comboBox = new global::Avalonia.Controls.ComboBox
            {
                Width = 118,
                MinWidth = 118,
                Height = 26,
                MinHeight = 24,
                PlaceholderText = label,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            foreach (var (styleLabel, tag) in SemanticHtmlStyleItems)
                comboBox.Items.Add(new ComboBoxItem { Content = styleLabel, Tag = tag });

            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                    ComboSelectionChanged?.Invoke(
                        this, new RibbonComboSelectionEventArgs(CommandId.SemanticHtmlGallery, tag));
            };

            _dropDowns.Add((CommandId.SemanticHtmlGallery, comboBox));
            return comboBox;
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
        /// Builds a compact color-picker: glyph + color bar + chevron (not a long
        /// text label), so the Font row stays within typical Home-tab widths.
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
            bool isHighlight = config.ColorTemplate == RibbonColorTemplate.HighlightColors;
            var accent = isHighlight
                ? Color.FromRgb(0xFF, 0xEB, 0x3B)
                : Color.FromRgb(0xC6, 0x28, 0x28);

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

            string tip = CommandLabelHelper.GetLabel(commandId);
            var glyphStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 1,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            glyphStack.Children.Add(new TextBlock
            {
                Text = isHighlight ? "ab" : "A",
                FontSize = isHighlight ? 11 : 12,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B))
            });
            glyphStack.Children.Add(new Border
            {
                Width = isHighlight ? 14 : 12,
                Height = 3,
                CornerRadius = new CornerRadius(1),
                Background = new SolidColorBrush(accent),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 2,
                VerticalAlignment = VerticalAlignment.Center
            };
            content.Children.Add(glyphStack);
            content.Children.Add(new TextBlock
            {
                Text = "\u25BE",
                FontSize = 9,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66))
            });

            var button = new Button
            {
                Focusable = false,
                MinHeight = 26,
                MinWidth = 34,
                Padding = new Thickness(4, 1),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(3),
                Flyout = flyout,
                Content = content,
                [ToolTip.TipProperty] = tip
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
    /// Carries a ribbon spinner (NumericUpDown) value change to the host. A null
    /// <see cref="Value"/> means the spinner was cleared.
    /// </summary>
    public class RibbonSpinnerValueEventArgs : EventArgs
    {
        public RibbonSpinnerValueEventArgs(CommandId commandId, decimal? value)
        {
            CommandId = commandId;
            Value = value;
        }

        public CommandId CommandId { get; }
        public decimal? Value { get; }
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
