// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// Copied from OpenLiveWriter.Ribbon.Managed.RibbonEnums to avoid referencing
// the Windows-only Ribbon.Managed project. Keep in sync with the original.

using System;

namespace OpenLiveWriter.Ribbon.Managed
{
    [Flags]
    public enum RibbonApplicationMode
    {
        Normal = 1 << 0,
        Preview = 1 << 1,
        LTR = 1 << 2,
        RTL = 1 << 3,
        WithoutPlugins = 1 << 4,
        WithPlugins = 1 << 5,
        Debug = 1 << 31,
        All = Normal | Preview | LTR | RTL | WithoutPlugins | WithPlugins | Debug
    }

    public enum RibbonGroupSize
    {
        Large,
        Medium,
        Small,
        Popup
    }

    public enum RibbonButtonType
    {
        Button,
        ToggleButton,
        SplitButton,
        DropDownButton
    }

    public enum RibbonTextPosition
    {
        Hide,
        Bottom,
        Right
    }

    public enum RibbonGalleryType
    {
        InRibbon,
        DropDown,
        SplitButton,
        CompactDropDown
    }

    public enum RibbonGalleryLayout
    {
        Flow,
        VerticalMenu
    }

    public enum RibbonColorTemplate
    {
        StandardColors,
        HighlightColors,
        ThemeColors
    }

    public enum RibbonContextualTabGroup
    {
        None,
        ImageTools,
        VideoTools,
        TableTools,
        MapTools,
        TagTools
    }
}
