// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Ribbon.Managed.Controls
{
    /// <summary>
    /// Shared layout constants for ribbon controls.
    /// Centralizes sizing and spacing values for consistency and easy maintenance.
    /// </summary>
    public static class LayoutConstants
    {
        // Button sizes
        public const int SmallButtonSize = 22;
        public const int MediumButtonHeight = 24;
        public const int LargeButtonMinWidth = 50;
        public const int MediumButtonMinWidth = 60;

        // Image sizes
        public const int SmallImageSize = 16;
        public const int LargeImageSize = 32;

        // Padding and spacing
        public const int GroupPadding = 3;
        public const int ControlSpacing = 2;
        public const int SeparatorWidth = 6;
        public const int DropdownArrowSpace = 16;

        // Small button stacking
        public const int MaxSmallButtonRows = 3;

        // Group dimensions
        public const int GroupMinWidth = 52;
        public const int GroupLabelHeight = 18;
        public const int PopupWidth = 48;

        // Tab dimensions
        public const int TabHeight = 25;
        public const int ContentHeight = 94;
    }
}
