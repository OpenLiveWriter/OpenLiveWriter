// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.Ribbon.Managed.Rendering
{
    /// <summary>
    /// Color scheme for the ribbon control.
    /// Provides Office-style colors for rendering.
    /// </summary>
    public class RibbonColors
    {
        private static RibbonColors _current;

        /// <summary>
        /// Gets or sets the current color scheme.
        /// </summary>
        public static RibbonColors Current
        {
            get => _current ?? (_current = new RibbonColors());
            set => _current = value;
        }

        // Tab colors
        public Color TabBackground { get; set; } = Color.FromArgb(245, 246, 247);
        public Color TabBackgroundHover { get; set; } = Color.FromArgb(232, 239, 247);
        public Color TabBackgroundSelected { get; set; } = Color.White;
        public Color TabBorder { get; set; } = Color.FromArgb(198, 198, 198);
        public Color TabText { get; set; } = Color.FromArgb(68, 68, 68);
        public Color TabTextHover { get; set; } = Color.FromArgb(38, 38, 38);
        public Color TabTextSelected { get; set; } = Color.FromArgb(0, 102, 204);

        // Contextual tab colors
        public Color ContextualTabImageTools { get; set; } = Color.FromArgb(149, 89, 178);
        public Color ContextualTabVideoTools { get; set; } = Color.FromArgb(60, 150, 60);
        public Color ContextualTabTableTools { get; set; } = Color.FromArgb(200, 120, 60);
        public Color ContextualTabMapTools { get; set; } = Color.FromArgb(60, 130, 200);
        public Color ContextualTabTagTools { get; set; } = Color.FromArgb(180, 80, 80);

        // Ribbon panel colors
        public Color RibbonBackground { get; set; } = Color.FromArgb(245, 246, 247);
        public Color RibbonBorder { get; set; } = Color.FromArgb(198, 198, 198);

        // Group colors
        public Color GroupBackground { get; set; } = Color.Transparent;
        public Color GroupBorder { get; set; } = Color.FromArgb(225, 225, 225);
        public Color GroupLabelText { get; set; } = Color.FromArgb(102, 102, 102);
        public Color GroupSeparator { get; set; } = Color.FromArgb(225, 225, 225);

        // Button colors
        public Color ButtonBackground { get; set; } = Color.Transparent;
        public Color ButtonBackgroundHover { get; set; } = Color.FromArgb(232, 239, 247);
        public Color ButtonBackgroundPressed { get; set; } = Color.FromArgb(201, 224, 247);
        public Color ButtonBackgroundChecked { get; set; } = Color.FromArgb(201, 224, 247);
        public Color ButtonBorder { get; set; } = Color.Transparent;
        public Color ButtonBorderHover { get; set; } = Color.FromArgb(164, 206, 249);
        public Color ButtonBorderPressed { get; set; } = Color.FromArgb(98, 163, 229);
        public Color ButtonBorderChecked { get; set; } = Color.FromArgb(98, 163, 229);
        public Color ButtonText { get; set; } = Color.FromArgb(68, 68, 68);
        public Color ButtonTextDisabled { get; set; } = Color.FromArgb(166, 166, 166);

        // Application menu colors
        public Color AppMenuBackground { get; set; } = Color.FromArgb(53, 53, 53);
        public Color AppMenuItemBackground { get; set; } = Color.Transparent;
        public Color AppMenuItemBackgroundHover { get; set; } = Color.FromArgb(73, 73, 73);
        public Color AppMenuItemText { get; set; } = Color.White;
        public Color AppMenuRecentItemsBackground { get; set; } = Color.White;

        // Quick Access Toolbar colors
        public Color QatBackground { get; set; } = Color.FromArgb(77, 96, 130);
        public Color QatButtonBackground { get; set; } = Color.Transparent;
        public Color QatButtonBackgroundHover { get; set; } = Color.FromArgb(97, 116, 150);

        // Gallery colors
        public Color GalleryBackground { get; set; } = Color.White;
        public Color GalleryBorder { get; set; } = Color.FromArgb(198, 198, 198);
        public Color GalleryItemBackground { get; set; } = Color.Transparent;
        public Color GalleryItemBackgroundHover { get; set; } = Color.FromArgb(232, 239, 247);
        public Color GalleryItemBackgroundSelected { get; set; } = Color.FromArgb(201, 224, 247);
        public Color GalleryItemBorder { get; set; } = Color.Transparent;
        public Color GalleryItemBorderHover { get; set; } = Color.FromArgb(164, 206, 249);
        public Color GalleryItemBorderSelected { get; set; } = Color.FromArgb(98, 163, 229);

        // Dropdown/Popup colors
        public Color DropDownBackground { get; set; } = Color.FromArgb(250, 250, 250);
        public Color DropDownBorder { get; set; } = Color.FromArgb(198, 198, 198);
        public Color DropDownShadow { get; set; } = Color.FromArgb(40, 0, 0, 0);

        // Separator colors
        public Color Separator { get; set; } = Color.FromArgb(210, 210, 210);

        // Color picker colors
        public Color ColorPickerNoColor { get; set; } = Color.FromArgb(250, 250, 250);
        public Color ColorPickerAutomaticColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets the contextual tab color for a specific tab group.
        /// </summary>
        public Color GetContextualTabColor(RibbonContextualTabGroup group)
        {
            switch (group)
            {
                case RibbonContextualTabGroup.ImageTools:
                    return ContextualTabImageTools;
                case RibbonContextualTabGroup.VideoTools:
                    return ContextualTabVideoTools;
                case RibbonContextualTabGroup.TableTools:
                    return ContextualTabTableTools;
                case RibbonContextualTabGroup.MapTools:
                    return ContextualTabMapTools;
                case RibbonContextualTabGroup.TagTools:
                    return ContextualTabTagTools;
                default:
                    return TabBackground;
            }
        }

        /// <summary>
        /// Creates a dark theme color scheme.
        /// </summary>
        public static RibbonColors CreateDarkTheme()
        {
            return new RibbonColors
            {
                TabBackground = Color.FromArgb(45, 45, 48),
                TabBackgroundHover = Color.FromArgb(62, 62, 64),
                TabBackgroundSelected = Color.FromArgb(37, 37, 38),
                TabBorder = Color.FromArgb(63, 63, 70),
                TabText = Color.FromArgb(241, 241, 241),
                TabTextHover = Color.White,
                TabTextSelected = Color.FromArgb(0, 122, 204),

                RibbonBackground = Color.FromArgb(37, 37, 38),
                RibbonBorder = Color.FromArgb(63, 63, 70),

                GroupBorder = Color.FromArgb(63, 63, 70),
                GroupLabelText = Color.FromArgb(153, 153, 153),
                GroupSeparator = Color.FromArgb(63, 63, 70),

                ButtonBackgroundHover = Color.FromArgb(62, 62, 64),
                ButtonBackgroundPressed = Color.FromArgb(0, 122, 204),
                ButtonBackgroundChecked = Color.FromArgb(51, 51, 55),
                ButtonBorderHover = Color.FromArgb(63, 63, 70),
                ButtonBorderPressed = Color.FromArgb(0, 122, 204),
                ButtonBorderChecked = Color.FromArgb(0, 122, 204),
                ButtonText = Color.FromArgb(241, 241, 241),
                ButtonTextDisabled = Color.FromArgb(102, 102, 102),

                AppMenuBackground = Color.FromArgb(30, 30, 30),
                AppMenuItemBackgroundHover = Color.FromArgb(62, 62, 64),
                AppMenuItemText = Color.FromArgb(241, 241, 241),
                AppMenuRecentItemsBackground = Color.FromArgb(37, 37, 38),

                GalleryBackground = Color.FromArgb(37, 37, 38),
                GalleryBorder = Color.FromArgb(63, 63, 70),
                GalleryItemBackgroundHover = Color.FromArgb(62, 62, 64),
                GalleryItemBackgroundSelected = Color.FromArgb(51, 51, 55),
                GalleryItemBorderHover = Color.FromArgb(63, 63, 70),
                GalleryItemBorderSelected = Color.FromArgb(0, 122, 204),

                DropDownBackground = Color.FromArgb(37, 37, 38),
                DropDownBorder = Color.FromArgb(63, 63, 70),

                Separator = Color.FromArgb(63, 63, 70)
            };
        }
    }
}
