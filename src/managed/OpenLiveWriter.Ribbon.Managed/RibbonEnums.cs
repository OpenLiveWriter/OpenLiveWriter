// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Ribbon.Managed
{
    /// <summary>
    /// Application modes that control ribbon visibility.
    /// Based on the Windows Ribbon Framework ApplicationModes.
    /// </summary>
    [Flags]
    public enum RibbonApplicationMode
    {
        /// <summary>
        /// Normal editing mode.
        /// </summary>
        Normal = 1 << 0,

        /// <summary>
        /// Preview mode.
        /// </summary>
        Preview = 1 << 1,

        /// <summary>
        /// Left-to-Right text direction mode.
        /// </summary>
        LTR = 1 << 2,

        /// <summary>
        /// Right-to-Left text direction mode.
        /// </summary>
        RTL = 1 << 3,

        /// <summary>
        /// Mode without plugins gallery.
        /// </summary>
        WithoutPlugins = 1 << 4,

        /// <summary>
        /// Mode with plugins gallery.
        /// </summary>
        WithPlugins = 1 << 5,

        /// <summary>
        /// Debug mode - shows debug tab.
        /// </summary>
        Debug = 1 << 31,

        /// <summary>
        /// All modes combined.
        /// </summary>
        All = Normal | Preview | LTR | RTL | WithoutPlugins | WithPlugins | Debug
    }

    /// <summary>
    /// Size for ribbon group scaling.
    /// </summary>
    public enum RibbonGroupSize
    {
        /// <summary>
        /// Large size - full icons with labels.
        /// </summary>
        Large,

        /// <summary>
        /// Medium size - smaller icons with labels.
        /// </summary>
        Medium,

        /// <summary>
        /// Small size - small icons only.
        /// </summary>
        Small,

        /// <summary>
        /// Popup size - collapsed to button with popup.
        /// </summary>
        Popup
    }

    /// <summary>
    /// Type of ribbon button.
    /// </summary>
    public enum RibbonButtonType
    {
        /// <summary>
        /// Standard button.
        /// </summary>
        Button,

        /// <summary>
        /// Toggle button with checked state.
        /// </summary>
        ToggleButton,

        /// <summary>
        /// Split button - button with dropdown.
        /// </summary>
        SplitButton,

        /// <summary>
        /// Dropdown button - dropdown only.
        /// </summary>
        DropDownButton
    }

    /// <summary>
    /// Position of text relative to image in button.
    /// </summary>
    public enum RibbonTextPosition
    {
        /// <summary>
        /// No text displayed.
        /// </summary>
        Hide,

        /// <summary>
        /// Text below image.
        /// </summary>
        Bottom,

        /// <summary>
        /// Text to the right of image.
        /// </summary>
        Right
    }

    /// <summary>
    /// Type of gallery control.
    /// </summary>
    public enum RibbonGalleryType
    {
        /// <summary>
        /// In-ribbon gallery (expanded in ribbon).
        /// </summary>
        InRibbon,

        /// <summary>
        /// Dropdown gallery.
        /// </summary>
        DropDown,

        /// <summary>
        /// Split button gallery.
        /// </summary>
        SplitButton,

        /// <summary>
        /// Compact dropdown (icon + text + dropdown arrow, like blog selector).
        /// </summary>
        CompactDropDown
    }

    /// <summary>
    /// Layout type for gallery items.
    /// </summary>
    public enum RibbonGalleryLayout
    {
        /// <summary>
        /// Flow layout with wrapping.
        /// </summary>
        Flow,

        /// <summary>
        /// Vertical menu layout.
        /// </summary>
        VerticalMenu
    }

    /// <summary>
    /// Color template for color pickers.
    /// </summary>
    public enum RibbonColorTemplate
    {
        /// <summary>
        /// Standard colors palette.
        /// </summary>
        StandardColors,

        /// <summary>
        /// Highlight colors palette (for text highlight).
        /// </summary>
        HighlightColors,

        /// <summary>
        /// Theme colors palette.
        /// </summary>
        ThemeColors
    }

    /// <summary>
    /// Contextual tab group types.
    /// </summary>
    public enum RibbonContextualTabGroup
    {
        /// <summary>
        /// No contextual tab group.
        /// </summary>
        None,

        /// <summary>
        /// Picture/Image tools.
        /// </summary>
        ImageTools,

        /// <summary>
        /// Video tools.
        /// </summary>
        VideoTools,

        /// <summary>
        /// Table tools.
        /// </summary>
        TableTools,

        /// <summary>
        /// Map tools.
        /// </summary>
        MapTools,

        /// <summary>
        /// Tag tools.
        /// </summary>
        TagTools
    }
}
