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

        /// <summary>
        /// Event raised when a command button within this group is clicked.
        /// </summary>
        public event EventHandler<CommandId> CommandExecuted;

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
                    var comboBox = new global::Avalonia.Controls.ComboBox
                    {
                        Width = combo.PreferredWidth,
                        Height = 24,
                        PlaceholderText = CommandLabelHelper.GetLabel(combo.CommandId),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    comboRow.Children.Add(comboBox);
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
                    var colorBtn = new RibbonButtonControl(
                        color.CommandId,
                        CommandLabelHelper.GetLabel(color.CommandId),
                        RibbonGroupSize.Small);
                    colorBtn.CommandExecuted += (s, cmd) => CommandExecuted?.Invoke(this, cmd);
                    control = colorBtn;
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

            return control;
        }

        private Control CreateGalleryPlaceholder(GalleryConfig gallery, RibbonGroupSize sizeOverride)
        {
            var label = CommandLabelHelper.GetLabel(gallery.CommandId);

            if (gallery.GalleryType == RibbonGalleryType.InRibbon)
            {
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
                // Compact dropdown (like blog selector)
                return new global::Avalonia.Controls.ComboBox
                {
                    Width = 140,
                    Height = 24,
                    PlaceholderText = label,
                    VerticalAlignment = VerticalAlignment.Center
                };
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
}
