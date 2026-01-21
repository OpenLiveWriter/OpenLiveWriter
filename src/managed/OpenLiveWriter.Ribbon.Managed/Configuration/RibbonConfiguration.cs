// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Ribbon.Managed.Configuration
{
    /// <summary>
    /// Configuration for the ribbon structure.
    /// Defines tabs, groups, and controls programmatically.
    /// </summary>
    public class RibbonConfiguration
    {
        /// <summary>
        /// Gets the application menu configuration.
        /// </summary>
        public ApplicationMenuConfig ApplicationMenu { get; set; } = new ApplicationMenuConfig();

        /// <summary>
        /// Gets the quick access toolbar configuration.
        /// </summary>
        public QuickAccessToolbarConfig QuickAccessToolbar { get; set; } = new QuickAccessToolbarConfig();

        /// <summary>
        /// Gets the list of tab configurations.
        /// </summary>
        public List<TabConfig> Tabs { get; } = new List<TabConfig>();

        /// <summary>
        /// Gets the list of contextual tab group configurations.
        /// </summary>
        public List<ContextualTabGroupConfig> ContextualTabGroups { get; } = new List<ContextualTabGroupConfig>();
    }

    /// <summary>
    /// Configuration for a ribbon tab.
    /// </summary>
    public class TabConfig
    {
        /// <summary>
        /// Gets or sets the command ID for this tab.
        /// </summary>
        public CommandId CommandId { get; set; }

        /// <summary>
        /// Gets or sets the display label for this tab.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the keytip for keyboard navigation.
        /// </summary>
        public string Keytip { get; set; }

        /// <summary>
        /// Gets or sets the application modes where this tab is visible.
        /// </summary>
        public RibbonApplicationMode VisibleModes { get; set; } = RibbonApplicationMode.All;

        /// <summary>
        /// Gets or sets the contextual tab group this tab belongs to (if any).
        /// </summary>
        public RibbonContextualTabGroup ContextualGroup { get; set; } = RibbonContextualTabGroup.None;

        /// <summary>
        /// Gets the list of groups in this tab.
        /// </summary>
        public List<GroupConfig> Groups { get; } = new List<GroupConfig>();

        /// <summary>
        /// Gets the scaling policy for this tab's groups.
        /// </summary>
        public ScalingPolicy ScalingPolicy { get; set; } = new ScalingPolicy();
    }

    /// <summary>
    /// Configuration for a ribbon group.
    /// </summary>
    public class GroupConfig
    {
        /// <summary>
        /// Gets or sets the command ID for this group.
        /// </summary>
        public CommandId CommandId { get; set; }

        /// <summary>
        /// Gets or sets the display label for this group.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the keytip for keyboard navigation.
        /// </summary>
        public string Keytip { get; set; }

        /// <summary>
        /// Gets or sets the application modes where this group is visible.
        /// </summary>
        public RibbonApplicationMode VisibleModes { get; set; } = RibbonApplicationMode.All;

        /// <summary>
        /// Gets or sets the preferred size definition for this group.
        /// </summary>
        public string SizeDefinition { get; set; }

        /// <summary>
        /// Gets the list of controls in this group.
        /// </summary>
        public List<ControlConfig> Controls { get; } = new List<ControlConfig>();
    }

    /// <summary>
    /// Base configuration for a ribbon control.
    /// </summary>
    public abstract class ControlConfig
    {
        /// <summary>
        /// Gets the type of control.
        /// </summary>
        public abstract string ControlType { get; }

        /// <summary>
        /// Gets or sets the command ID for this control.
        /// </summary>
        public CommandId CommandId { get; set; }

        /// <summary>
        /// Gets or sets the application modes where this control is visible.
        /// </summary>
        public RibbonApplicationMode VisibleModes { get; set; } = RibbonApplicationMode.All;
    }

    /// <summary>
    /// Configuration for a button control.
    /// </summary>
    public class ButtonConfig : ControlConfig
    {
        public override string ControlType => "Button";

        /// <summary>
        /// Gets or sets the button type.
        /// </summary>
        public RibbonButtonType ButtonType { get; set; } = RibbonButtonType.Button;

        /// <summary>
        /// Gets the list of menu items for dropdown/split buttons.
        /// </summary>
        public List<MenuItemConfig> MenuItems { get; } = new List<MenuItemConfig>();
    }

    /// <summary>
    /// Configuration for a toggle button control.
    /// </summary>
    public class ToggleButtonConfig : ControlConfig
    {
        public override string ControlType => "ToggleButton";
    }

    /// <summary>
    /// Configuration for a combobox control.
    /// </summary>
    public class ComboBoxConfig : ControlConfig
    {
        public override string ControlType => "ComboBox";

        /// <summary>
        /// Gets or sets whether auto-complete is enabled.
        /// </summary>
        public bool IsAutoCompleteEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the combobox is editable.
        /// </summary>
        public bool IsEditable { get; set; } = true;
    }

    /// <summary>
    /// Configuration for a gallery control.
    /// </summary>
    public class GalleryConfig : ControlConfig
    {
        public override string ControlType => "Gallery";

        /// <summary>
        /// Gets or sets the gallery type.
        /// </summary>
        public RibbonGalleryType GalleryType { get; set; } = RibbonGalleryType.DropDown;

        /// <summary>
        /// Gets or sets the text position.
        /// </summary>
        public RibbonTextPosition TextPosition { get; set; } = RibbonTextPosition.Bottom;

        /// <summary>
        /// Gets or sets the item height in pixels.
        /// </summary>
        public int ItemHeight { get; set; } = 32;

        /// <summary>
        /// Gets or sets the item width in pixels.
        /// </summary>
        public int ItemWidth { get; set; } = 32;

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public int Columns { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum number of columns.
        /// </summary>
        public int MaxColumns { get; set; } = 7;

        /// <summary>
        /// Gets or sets the maximum number of rows.
        /// </summary>
        public int MaxRows { get; set; } = 3;

        /// <summary>
        /// Gets or sets the gallery layout.
        /// </summary>
        public RibbonGalleryLayout Layout { get; set; } = RibbonGalleryLayout.Flow;

        /// <summary>
        /// Gets the list of menu groups below the gallery.
        /// </summary>
        public List<MenuGroupConfig> MenuGroups { get; } = new List<MenuGroupConfig>();
    }

    /// <summary>
    /// Configuration for a color picker control.
    /// </summary>
    public class ColorPickerConfig : ControlConfig
    {
        public override string ControlType => "ColorPicker";

        /// <summary>
        /// Gets or sets the color template.
        /// </summary>
        public RibbonColorTemplate ColorTemplate { get; set; } = RibbonColorTemplate.StandardColors;

        /// <summary>
        /// Gets or sets whether the "No Color" button is visible.
        /// </summary>
        public bool IsNoColorButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets whether the "Automatic" color button is visible.
        /// </summary>
        public bool IsAutomaticColorButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets the number of rows in the standard color grid.
        /// </summary>
        public int StandardColorGridRows { get; set; } = 6;

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public int Columns { get; set; } = 5;
    }

    /// <summary>
    /// Configuration for a spinner control.
    /// </summary>
    public class SpinnerConfig : ControlConfig
    {
        public override string ControlType => "Spinner";

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public int MinValue { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public int MaxValue { get; set; } = 100;

        /// <summary>
        /// Gets or sets the increment amount.
        /// </summary>
        public int Increment { get; set; } = 1;

        /// <summary>
        /// Gets or sets the format string.
        /// </summary>
        public string FormatString { get; set; } = "{0}";
    }

    /// <summary>
    /// Configuration for a separator control.
    /// </summary>
    public class SeparatorConfig : ControlConfig
    {
        public override string ControlType => "Separator";
    }

    /// <summary>
    /// Configuration for a menu group (within dropdown menus).
    /// </summary>
    public class MenuGroupConfig
    {
        /// <summary>
        /// Gets or sets the menu group class.
        /// </summary>
        public string Class { get; set; } = "StandardItems";

        /// <summary>
        /// Gets the list of menu items.
        /// </summary>
        public List<MenuItemConfig> Items { get; } = new List<MenuItemConfig>();
    }

    /// <summary>
    /// Configuration for a menu item.
    /// </summary>
    public class MenuItemConfig
    {
        /// <summary>
        /// Gets or sets the command ID for this menu item.
        /// </summary>
        public CommandId CommandId { get; set; }

        /// <summary>
        /// Gets or sets whether this is a separator.
        /// </summary>
        public bool IsSeparator { get; set; }
    }

    /// <summary>
    /// Configuration for a contextual tab group.
    /// </summary>
    public class ContextualTabGroupConfig
    {
        /// <summary>
        /// Gets or sets the command ID for this group.
        /// </summary>
        public CommandId CommandId { get; set; }

        /// <summary>
        /// Gets or sets the group type.
        /// </summary>
        public RibbonContextualTabGroup GroupType { get; set; }

        /// <summary>
        /// Gets or sets the display label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets the list of tabs in this contextual group.
        /// </summary>
        public List<TabConfig> Tabs { get; } = new List<TabConfig>();
    }

    /// <summary>
    /// Configuration for the application menu.
    /// </summary>
    public class ApplicationMenuConfig
    {
        /// <summary>
        /// Gets or sets the command ID for the menu.
        /// </summary>
        public CommandId CommandId { get; set; } = CommandId.FileMenu;

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string Label { get; set; } = "File";

        /// <summary>
        /// Gets the list of menu groups.
        /// </summary>
        public List<MenuGroupConfig> MenuGroups { get; } = new List<MenuGroupConfig>();

        /// <summary>
        /// Gets or sets the recent items command ID.
        /// </summary>
        public CommandId RecentItemsCommandId { get; set; } = CommandId.MRUList;

        /// <summary>
        /// Gets or sets the maximum number of recent items.
        /// </summary>
        public int MaxRecentItems { get; set; } = 10;
    }

    /// <summary>
    /// Configuration for the quick access toolbar.
    /// </summary>
    public class QuickAccessToolbarConfig
    {
        /// <summary>
        /// Gets or sets the command ID.
        /// </summary>
        public CommandId CommandId { get; set; } = CommandId.QAT;

        /// <summary>
        /// Gets the list of default commands on the QAT.
        /// </summary>
        public List<CommandId> DefaultCommands { get; } = new List<CommandId>();
    }

    /// <summary>
    /// Scaling policy for ribbon groups.
    /// </summary>
    public class ScalingPolicy
    {
        /// <summary>
        /// Gets the ideal sizes for groups.
        /// </summary>
        public Dictionary<CommandId, RibbonGroupSize> IdealSizes { get; } = new Dictionary<CommandId, RibbonGroupSize>();

        /// <summary>
        /// Gets the scaling steps (group -> next smaller size).
        /// </summary>
        public List<ScaleStep> ScaleSteps { get; } = new List<ScaleStep>();
    }

    /// <summary>
    /// A single scaling step.
    /// </summary>
    public class ScaleStep
    {
        /// <summary>
        /// Gets or sets the group command ID.
        /// </summary>
        public CommandId GroupId { get; set; }

        /// <summary>
        /// Gets or sets the size to scale to.
        /// </summary>
        public RibbonGroupSize Size { get; set; }
    }
}
