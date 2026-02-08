// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Shared layout constants for ribbon controls.
    /// Centralizes sizing and spacing values for consistency and easy maintenance.
    /// Based on Windows Ribbon Framework specifications.
    /// All values are DPI-aware and scale automatically based on the current DPI.
    /// </summary>
    public static class LayoutConstants
    {
        // Base values at 96 DPI (100% scaling)
        private const int BASE_SmallButtonSize = 22;
        private const int BASE_MediumButtonHeight = 22;
        private const int BASE_MediumButtonMinWidth = 40;
        private const int BASE_LargeButtonMinWidth = 46;
        private const int BASE_LargeButtonMinHeight = 72;  // 3 (top) + 32 (icon) + 2 (gap) + 26 (2 lines text) + 9 (bottom padding)
        private const int BASE_LargeButtonTextPadding = 6;
        private const int BASE_SmallImageSize = 16;
        private const int BASE_LargeImageSize = 32;
        private const int BASE_LargeButtonIconTopPadding = 3;
        private const int BASE_LargeButtonIconTextGap = 2;
        private const int BASE_LargeButtonTextLineHeight = 13;
        private const int BASE_GroupPadding = 4;
        private const int BASE_ControlSpacing = 3;
        private const int BASE_SeparatorWidth = 8;
        private const int BASE_DropdownArrowWidth = 14;
        private const int BASE_GroupSeparatorMargin = 3;
        private const int BASE_GroupMinWidth = 52;
        private const int BASE_GroupLabelHeight = 18;
        private const int BASE_PopupWidth = 54;
        private const int BASE_TabHeight = 24;
        private const int BASE_ContentHeight = 90;   // Native ribbon content area is ~90px at 100% DPI
        private const int BASE_TabSpacing = 0;

        // Button sizes (following Windows Ribbon specifications) - DPI-scaled
        public static int SmallButtonSize => DisplayHelper.ScaleXCeil(BASE_SmallButtonSize);
        public static int MediumButtonHeight => DisplayHelper.ScaleYCeil(BASE_MediumButtonHeight);
        public static int MediumButtonMinWidth => DisplayHelper.ScaleXCeil(BASE_MediumButtonMinWidth);
        public static int LargeButtonMinWidth => DisplayHelper.ScaleXCeil(BASE_LargeButtonMinWidth);
        public static int LargeButtonMinHeight => DisplayHelper.ScaleYCeil(BASE_LargeButtonMinHeight);
        public static int LargeButtonTextPadding => DisplayHelper.ScaleXCeil(BASE_LargeButtonTextPadding);

        // Image sizes (Windows Ribbon standard sizes) - DPI-scaled for layout
        public static int SmallImageSize => DisplayHelper.ScaleXCeil(BASE_SmallImageSize);
        public static int LargeImageSize => DisplayHelper.ScaleXCeil(BASE_LargeImageSize);

        // Unscaled image sizes for rendering icons at native pixel size (prevents blurry scaling)
        public const int SmallImageSizeUnscaled = BASE_SmallImageSize;  // 16px
        public const int LargeImageSizeUnscaled = BASE_LargeImageSize;  // 32px

        // Large button layout - DPI-scaled
        public static int LargeButtonIconTopPadding => DisplayHelper.ScaleYCeil(BASE_LargeButtonIconTopPadding);
        public static int LargeButtonIconTextGap => DisplayHelper.ScaleYCeil(BASE_LargeButtonIconTextGap);
        public static int LargeButtonTextLineHeight => DisplayHelper.ScaleYCeil(BASE_LargeButtonTextLineHeight);

        // Padding and spacing - DPI-scaled
        public static int GroupPadding => DisplayHelper.ScaleXCeil(BASE_GroupPadding);
        public static int ControlSpacing => DisplayHelper.ScaleXCeil(BASE_ControlSpacing);
        public static int SeparatorWidth => DisplayHelper.ScaleXCeil(BASE_SeparatorWidth);
        public static int DropdownArrowWidth => DisplayHelper.ScaleXCeil(BASE_DropdownArrowWidth);
        public static int GroupSeparatorMargin => DisplayHelper.ScaleXCeil(BASE_GroupSeparatorMargin);

        // Small button stacking (count, not scaled)
        public const int MaxSmallButtonRows = 3;

        // Group dimensions - DPI-scaled
        public static int GroupMinWidth => DisplayHelper.ScaleXCeil(BASE_GroupMinWidth);
        public static int GroupLabelHeight => DisplayHelper.ScaleYCeil(BASE_GroupLabelHeight);
        public static int PopupWidth => DisplayHelper.ScaleXCeil(BASE_PopupWidth);

        // Tab dimensions - DPI-scaled
        public static int TabHeight => DisplayHelper.ScaleYCeil(BASE_TabHeight);
        public static int ContentHeight => DisplayHelper.ScaleYCeil(BASE_ContentHeight);
        public static int TabSpacing => DisplayHelper.ScaleXCeil(BASE_TabSpacing);
    }
}
