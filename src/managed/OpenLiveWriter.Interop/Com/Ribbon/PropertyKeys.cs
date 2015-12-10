// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.Ribbon
{
    public static class PropertyKeys
    {
        // Core command properties
        public static PropertyKey Enabled; // bool
        public static PropertyKey LabelDescription; // string
        public static PropertyKey Keytip; // string
        public static PropertyKey Label; // string
        public static PropertyKey TooltipDescription; // string
        public static PropertyKey TooltipTitle; // string
        public static PropertyKey LargeImage; // IUIImage
        public static PropertyKey LargeHighContrastImage; // IUIImage
        public static PropertyKey SmallImage; // IUIImage
        public static PropertyKey SmallHighContrastImage; // IUIImage

        // Collections properties
        public static PropertyKey CommandId; // uint
        public static PropertyKey ItemsSource; // IEnumUnknown or IUICollection
        public static PropertyKey Categories; // IEnumUnknown or IUICollection
        public static PropertyKey CategoryId; // uint
        public static PropertyKey SelectedItem; // uint
        public static PropertyKey CommandType; // enum CommandTypeID
        public static PropertyKey ItemImage; // IUIImage

        // Control properties
        public static PropertyKey BooleanValue;
        public static PropertyKey DecimalValue;
        public static PropertyKey StringValue;
        public static PropertyKey MaxValue;
        public static PropertyKey MinValue;
        public static PropertyKey Increment;
        public static PropertyKey DecimalPlaces;
        public static PropertyKey FormatString;
        public static PropertyKey RepresentativeString;

        // Font control properties
        public static PropertyKey FontProperties; // IPropertyStore
        public static PropertyKey FontProperties_Family;
        public static PropertyKey FontProperties_Size;
        public static PropertyKey FontProperties_Bold; // FONT_CONTROL_PROPERTY_VALUE
        public static PropertyKey FontProperties_Italic; // FONT_CONTROL_PROPERTY_VALUE
        public static PropertyKey FontProperties_Underline; // FONT_CONTROL_PROPERTY_VALUE
        public static PropertyKey FontProperties_Strikethrough; // FONT_CONTROL_PROPERTY_VALUE
        public static PropertyKey FontProperties_VerticalPositioning; // FONT_CONTROL_PROPERTY_VALUE
        public static PropertyKey FontProperties_ForegroundColor;
        public static PropertyKey FontProperties_BackgroundColor;
        public static PropertyKey FontProperties_ForegroundColorType;
        public static PropertyKey FontProperties_BackgroundColorType;
        public static PropertyKey FontProperties_ChangedProperties;
        public static PropertyKey FontProperties_DeltaSize;

        // Application menu properties
        public static PropertyKey RecentItems; // SafeArray<IUISimplePropertySet>
        public static PropertyKey Pinned; // bool

        // Color picker properties
        public static PropertyKey Color; // COLORREF
        public static PropertyKey ColorType; // UI_COMMAND_SWATCHCOLORTYPE

        public static PropertyKey ThemeColorsCategoryLabel;
        public static PropertyKey StandardColorsCategoryLabel;
        public static PropertyKey RecentColorsCategoryLabel;
        public static PropertyKey AutomaticColorLabel;
        public static PropertyKey NoColorLabel;
        public static PropertyKey MoreColorsLabel;
        public static PropertyKey ThemeColors;
        public static PropertyKey StandardColors;
        public static PropertyKey ThemeColorsTooltips;
        public static PropertyKey StandardColorsTooltips;

        // Ribbon properties
        public static PropertyKey Viewable;
        public static PropertyKey Minimized;
        public static PropertyKey QuickAccessToolbarDock;

        // Contextual tabset properties
        public static PropertyKey ContextAvailable;

        // Global properties
        public static PropertyKey GlobalBackgroundColor;
        public static PropertyKey GlobalHighlightColor;
        public static PropertyKey GlobalTextColor;

        //
        // Initialization

        static PropertyKeys()
        {
            // Core command properties
            Enabled = new PropertyKey(1, VarEnum.VT_BOOL);
            LabelDescription = new PropertyKey(2, VarEnum.VT_LPWSTR);
            Keytip = new PropertyKey(3, VarEnum.VT_LPWSTR);
            Label = new PropertyKey(4, VarEnum.VT_LPWSTR);
            TooltipDescription = new PropertyKey(5, VarEnum.VT_LPWSTR);
            TooltipTitle = new PropertyKey(6, VarEnum.VT_LPWSTR);
            LargeImage = new PropertyKey(7, VarEnum.VT_UNKNOWN);
            LargeHighContrastImage = new PropertyKey(8, VarEnum.VT_UNKNOWN);
            SmallImage = new PropertyKey(9, VarEnum.VT_UNKNOWN);
            SmallHighContrastImage = new PropertyKey(10, VarEnum.VT_UNKNOWN);

            // Collections properties
            CommandId = new PropertyKey(100, VarEnum.VT_UI4);
            ItemsSource = new PropertyKey(101, VarEnum.VT_UNKNOWN);
            Categories = new PropertyKey(102, VarEnum.VT_UNKNOWN);
            CategoryId = new PropertyKey(103, VarEnum.VT_UI4);
            SelectedItem = new PropertyKey(104, VarEnum.VT_UI4);
            CommandType = new PropertyKey(105, VarEnum.VT_UI4);
            ItemImage = new PropertyKey(106, VarEnum.VT_UNKNOWN);

            // Control properties
            BooleanValue = new PropertyKey(200, VarEnum.VT_BOOL);
            DecimalValue = new PropertyKey(201, VarEnum.VT_DECIMAL);
            StringValue = new PropertyKey(202, VarEnum.VT_LPWSTR);
            MaxValue = new PropertyKey(203, VarEnum.VT_DECIMAL);
            MinValue = new PropertyKey(204, VarEnum.VT_DECIMAL);
            Increment = new PropertyKey(205, VarEnum.VT_DECIMAL);
            DecimalPlaces = new PropertyKey(206, VarEnum.VT_UI4);
            FormatString = new PropertyKey(207, VarEnum.VT_LPWSTR);
            RepresentativeString = new PropertyKey(208, VarEnum.VT_LPWSTR);

            // Font control properties
            FontProperties = new PropertyKey(300, VarEnum.VT_UNKNOWN);
            FontProperties_Family = new PropertyKey(301, VarEnum.VT_LPWSTR);
            FontProperties_Size = new PropertyKey(302, VarEnum.VT_DECIMAL);
            FontProperties_Bold = new PropertyKey(303, VarEnum.VT_UI4);
            FontProperties_Italic = new PropertyKey(304, VarEnum.VT_UI4);
            FontProperties_Underline = new PropertyKey(305, VarEnum.VT_UI4);
            FontProperties_Strikethrough = new PropertyKey(306, VarEnum.VT_UI4);
            FontProperties_VerticalPositioning = new PropertyKey(307, VarEnum.VT_UI4);
            FontProperties_ForegroundColor = new PropertyKey(308, VarEnum.VT_UI4);
            FontProperties_BackgroundColor = new PropertyKey(309, VarEnum.VT_UI4);
            FontProperties_ForegroundColorType = new PropertyKey(310, VarEnum.VT_UI4);
            FontProperties_BackgroundColorType = new PropertyKey(311, VarEnum.VT_UI4);
            FontProperties_ChangedProperties = new PropertyKey(312, VarEnum.VT_UNKNOWN);
            FontProperties_DeltaSize = new PropertyKey(313, VarEnum.VT_UI4);

            // Application menu properties
            RecentItems = new PropertyKey(350, VarEnum.VT_ARRAY | VarEnum.VT_UNKNOWN);
            Pinned = new PropertyKey(351, VarEnum.VT_BOOL);

            // Color picker properties
            Color = new PropertyKey(400, VarEnum.VT_UI4);
            ColorType = new PropertyKey(401, VarEnum.VT_UI4);

            ThemeColorsCategoryLabel = new PropertyKey(403, VarEnum.VT_LPWSTR);
            StandardColorsCategoryLabel = new PropertyKey(404, VarEnum.VT_LPWSTR);
            RecentColorsCategoryLabel = new PropertyKey(405, VarEnum.VT_LPWSTR);
            AutomaticColorLabel = new PropertyKey(406, VarEnum.VT_LPWSTR);
            NoColorLabel = new PropertyKey(407, VarEnum.VT_LPWSTR);
            MoreColorsLabel = new PropertyKey(408, VarEnum.VT_LPWSTR);
            ThemeColors = new PropertyKey(409, VarEnum.VT_VECTOR | VarEnum.VT_UI4);
            StandardColors = new PropertyKey(410, VarEnum.VT_VECTOR | VarEnum.VT_UI4);
            ThemeColorsTooltips = new PropertyKey(411, VarEnum.VT_VECTOR | VarEnum.VT_LPWSTR);
            StandardColorsTooltips = new PropertyKey(412, VarEnum.VT_VECTOR | VarEnum.VT_LPWSTR);

            // Ribbon properties
            Viewable = new PropertyKey(1000, VarEnum.VT_BOOL);
            Minimized = new PropertyKey(1001, VarEnum.VT_BOOL);
            QuickAccessToolbarDock = new PropertyKey(1002, VarEnum.VT_UI4);

            // Contextual tabset properties
            ContextAvailable = new PropertyKey(1100, VarEnum.VT_UI4);

            // Global properties
            GlobalBackgroundColor = new PropertyKey(2000, VarEnum.VT_UI4);
            GlobalHighlightColor = new PropertyKey(2001, VarEnum.VT_UI4);
            GlobalTextColor = new PropertyKey(2002, VarEnum.VT_UI4);
        }

        //
        // Interface

        public static string GetName(PropertyKey key)
        {
            // Core command properties
            if (key == Enabled)
                return "UI_PKEY_Enabled";
            else if (key == LabelDescription)
                return "UI_PKEY_LabelDescription";
            else if (key == Keytip)
                return "UI_PKEY_Keytip";
            else if (key == Label)
                return "UI_PKEY_Label";
            else if (key == TooltipDescription)
                return "UI_PKEY_ToolTipDescription";
            else if (key == TooltipTitle)
                return "UI_PKEY_ToolTipTitle";
            else if (key == LargeImage)
                return "UI_PKEY_LargeImageHighColor";
            else if (key == LargeHighContrastImage)
                return "UI_PKEY_LargeImageLowColor";
            else if (key == SmallImage)
                return "UI_PKEY_SmallImageHighColor";
            else if (key == SmallHighContrastImage)
                return "UI_PKEY_SmallImageLowColor";

            // Collections properties
            else if (key == CommandId)
                return "UI_PKEY_CommandId";
            else if (key == ItemsSource)
                return "UI_PKEY_ItemsSource";
            else if (key == Categories)
                return "UI_PKEY_Categories";
            else if (key == CategoryId)
                return "UI_PKEY_CategoryId";
            else if (key == SelectedItem)
                return "UI_PKEY_SelectedItem";
            else if (key == CommandType)
                return "UI_PKEY_CommandType";
            else if (key == ItemImage)
                return "UI_PKEY_ItemImage";

            // Control properties
            else if (key == BooleanValue)
                return "UI_PKEY_BooleanValue";
            else if (key == DecimalValue)
                return "UI_PKEY_DecimalValue";
            else if (key == StringValue)
                return "UI_PKEY_StringValue";
            else if (key == MaxValue)
                return "UI_PKEY_MaxValue";
            else if (key == MinValue)
                return "UI_PKEY_MinValue";
            else if (key == Increment)
                return "UI_PKEY_Increment";
            else if (key == DecimalPlaces)
                return "UI_PKEY_DecimalPlaces";
            else if (key == FormatString)
                return "UI_PKEY_FormatString";
            else if (key == RepresentativeString)
                return "UI_PKEY_RepresentativeString";

            // Font control properties
            else if (key == FontProperties)
                return "UI_PKEY_FontProperties";
            else if (key == FontProperties_Family)
                return "UI_PKEY_FontFamily";
            else if (key == FontProperties_Size)
                return "UI_PKEY_FontSize";
            else if (key == FontProperties_Bold)
                return "UI_PKEY_FontBold";
            else if (key == FontProperties_Italic)
                return "UI_PKEY_FontItalic";
            else if (key == FontProperties_Underline)
                return "UI_PKEY_FontUnderline";
            else if (key == FontProperties_Strikethrough)
                return "UI_PKEY_FontStrikethrough";
            else if (key == FontProperties_VerticalPositioning)
                return "UI_PKEY_FontVerticalPositioning";
            else if (key == FontProperties_ForegroundColor)
                return "UI_PKEY_FontForegroundColor";
            else if (key == FontProperties_BackgroundColor)
                return "UI_PKEY_FontBackgroundColor";
            else if (key == FontProperties_ChangedProperties)
                return "UI_PKEY_FontProperties_FontChangedProperties";
            else if (key == FontProperties_DeltaSize)
                return "UI_PKEY_FontProperties_DeltaSize";

            // Application menu properties
            else if (key == RecentItems)
                return "UI_PKEY_RecentItems";
            else if (key == Pinned)
                return "UI_PKEY_Pinned";

            // Color picker properties
            else if (key == Color)
                return "UI_PKEY_Color";
            else if (key == ColorType)
                return "UI_PKEY_ColorType";
            else if (key == ThemeColorsCategoryLabel)
                return "UI_PKEY_ThemeColorsCategoryLabel";
            else if (key == StandardColorsCategoryLabel)
                return "UI_PKEY_StandardColorsCategoryLabel";
            else if (key == RecentColorsCategoryLabel)
                return "UI_PKEY_RecentColorsCategoryLabel";
            else if (key == AutomaticColorLabel)
                return "UI_PKEY_AutomaticColorLabel";
            else if (key == NoColorLabel)
                return "UI_PKEY_NoColorLabel";
            else if (key == MoreColorsLabel)
                return "UI_PKEY_MoreColorsLabel";
            else if (key == ThemeColors)
                return "UI_PKEY_ThemeColors";
            else if (key == StandardColors)
                return "UI_PKEY_StandardColors";
            else if (key == ThemeColorsTooltips)
                return "UI_PKEY_ThemeColorsTooltips";
            else if (key == StandardColorsTooltips)
                return "UI_PKEY_StandardColorsTooltips";

            // Ribbon properties
            else if (key == Viewable)
                return "UI_PKEY_Viewable";
            else if (key == Minimized)
                return "UI_PKEY_Minimized";
            else if (key == QuickAccessToolbarDock)
                return "UI_PKEY_QuickAccessToolbarDock";

            // Contextual tabset properties
            else if (key == ContextAvailable)
                return "UI_PKEY_ContextAvailable";

            // Global properties
            else if (key == GlobalBackgroundColor)
                return "UI_PKEY_GlobalBackgroundColor";
            else if (key == GlobalHighlightColor)
                return "UI_PKEY_GlobalHighlightColor";
            else if (key == GlobalTextColor)
                return "UI_PKEY_GlobalTextColor";

            return "Unknown Key";
        }
    }

    public static class PropertyKeyExtensions
    {
        private static int MAX_KEYS = 13;
        private static Dictionary<PropertyKey, CommandInvalidationFlags> commandInvalidationFlags = null;
        public static CommandInvalidationFlags GetCommandInvalidationFlags(PropertyKey key)
        {
            if (commandInvalidationFlags == null)
            {
                commandInvalidationFlags = new Dictionary<PropertyKey, CommandInvalidationFlags>(MAX_KEYS);

                // State
                commandInvalidationFlags.Add(PropertyKeys.DecimalPlaces, CommandInvalidationFlags.State);
                commandInvalidationFlags.Add(PropertyKeys.Enabled, CommandInvalidationFlags.State);
                commandInvalidationFlags.Add(PropertyKeys.FormatString, CommandInvalidationFlags.State);
                commandInvalidationFlags.Add(PropertyKeys.Increment, CommandInvalidationFlags.State);
                commandInvalidationFlags.Add(PropertyKeys.Pinned, CommandInvalidationFlags.State);
                commandInvalidationFlags.Add(PropertyKeys.RecentItems, CommandInvalidationFlags.State);
                commandInvalidationFlags.Add(PropertyKeys.RepresentativeString, CommandInvalidationFlags.State);

                // Value
                commandInvalidationFlags.Add(PropertyKeys.SelectedItem, CommandInvalidationFlags.Value);
                commandInvalidationFlags.Add(PropertyKeys.BooleanValue, CommandInvalidationFlags.Value);
                commandInvalidationFlags.Add(PropertyKeys.MaxValue, CommandInvalidationFlags.Value);
                commandInvalidationFlags.Add(PropertyKeys.MinValue, CommandInvalidationFlags.Value);
                commandInvalidationFlags.Add(PropertyKeys.DecimalValue, CommandInvalidationFlags.Value);
                commandInvalidationFlags.Add(PropertyKeys.StringValue, CommandInvalidationFlags.Value);
                Debug.Assert(commandInvalidationFlags.Count == MAX_KEYS);
            }

            CommandInvalidationFlags flags;
            if (commandInvalidationFlags.TryGetValue(key, out flags))
                return flags;
            return CommandInvalidationFlags.Property;
        }
    }
}
