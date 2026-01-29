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

        // Tab colors - Neutral gray to match original Windows Ribbon
        // Tab strip background - light gray (Office 2013/2016 style)
        public Color TabBackground { get; set; } = Color.FromArgb(245, 246, 247);
        // Hover state - light blue tint
        public Color TabBackgroundHover { get; set; } = Color.FromArgb(250, 251, 252);
        // Selected tab - pure white, matches content area
        public Color TabBackgroundSelected { get; set; } = Color.FromArgb(255, 255, 255);
        // Tab borders - lighter gray for subtle appearance
        public Color TabBorder { get; set; } = Color.FromArgb(198, 198, 198);
        // Tab text - dark gray for readability (matches Windows Ribbon)
        public Color TabText { get; set; } = Color.FromArgb(83, 83, 83);
        // Tab text hover - darker for visual feedback
        public Color TabTextHover { get; set; } = Color.FromArgb(38, 38, 38);
        // Tab text selected - same dark gray as normal text (Windows Ribbon style)
        // Selection is indicated by tab background/border blending with content area, not text color
        public Color TabTextSelected { get; set; } = Color.FromArgb(83, 83, 83);

        // Contextual tab colors
        public Color ContextualTabImageTools { get; set; } = Color.FromArgb(149, 89, 178);
        public Color ContextualTabVideoTools { get; set; } = Color.FromArgb(60, 150, 60);
        public Color ContextualTabTableTools { get; set; } = Color.FromArgb(200, 120, 60);
        public Color ContextualTabMapTools { get; set; } = Color.FromArgb(60, 130, 200);
        public Color ContextualTabTagTools { get; set; } = Color.FromArgb(180, 80, 80);

        // Ribbon panel colors - Neutral gray to match original
        // Content area is white/very light gray
        public Color RibbonBackground { get; set; } = Color.FromArgb(252, 252, 252);
        // Border - subtle gray
        public Color RibbonBorder { get; set; } = Color.FromArgb(171, 171, 171);

        // Group colors - Neutral gray to match original Windows Ribbon
        public Color GroupBackground { get; set; } = Color.Transparent;
        // Group borders/separators - neutral gray
        public Color GroupBorder { get; set; } = Color.FromArgb(200, 200, 200);
        // Label text - darker gray for better visibility (matches Windows Ribbon)
        public Color GroupLabelText { get; set; } = Color.FromArgb(68, 68, 68);
        // Separator - visible vertical line between groups
        public Color GroupSeparator { get; set; } = Color.FromArgb(170, 170, 170);
        // Label area styling - subtle but visible background
        public Color GroupLabelBackground { get; set; } = Color.FromArgb(238, 239, 240);
        public Color GroupLabelBorder { get; set; } = Color.FromArgb(210, 210, 212);

        // Button colors - Standard Office style
        public Color ButtonBackground { get; set; } = Color.Transparent;
        public Color ButtonBackgroundHover { get; set; } = Color.FromArgb(232, 239, 247);
        public Color ButtonBackgroundPressed { get; set; } = Color.FromArgb(200, 220, 240);
        public Color ButtonBackgroundChecked { get; set; } = Color.FromArgb(200, 220, 240);
        public Color ButtonBorder { get; set; } = Color.Transparent;
        public Color ButtonBorderHover { get; set; } = Color.FromArgb(164, 206, 249);
        public Color ButtonBorderPressed { get; set; } = Color.FromArgb(98, 163, 229);
        public Color ButtonBorderChecked { get; set; } = Color.FromArgb(98, 163, 229);
        // Button text - dark gray
        public Color ButtonText { get; set; } = Color.FromArgb(21, 21, 21);
        public Color ButtonTextDisabled { get; set; } = Color.FromArgb(160, 160, 160);

        // Application menu colors - light theme to match native Windows Ribbon
        public Color AppMenuBackground { get; set; } = Color.FromArgb(245, 246, 247);  // Light gray like native ribbon
        public Color AppMenuItemBackground { get; set; } = Color.Transparent;
        public Color AppMenuItemBackgroundHover { get; set; } = Color.FromArgb(201, 222, 245);  // Light blue hover
        public Color AppMenuItemText { get; set; } = Color.FromArgb(38, 38, 38);  // Dark text
        public Color AppMenuRecentItemsBackground { get; set; } = Color.White;

        // Quick Access Toolbar colors
        // QAT sits in the tab strip area
        public Color QatBackground { get; set; } = Color.FromArgb(245, 246, 247);
        public Color QatButtonBackground { get; set; } = Color.Transparent;
        public Color QatButtonBackgroundHover { get; set; } = Color.FromArgb(232, 239, 247);
        public Color QatButtonBackgroundPressed { get; set; } = Color.FromArgb(200, 220, 240);
        public Color QatButtonBorderHover { get; set; } = Color.FromArgb(164, 206, 249);
        public Color QatButtonBorderPressed { get; set; } = Color.FromArgb(98, 163, 229);
        public Color QatDropdownArrow { get; set; } = Color.FromArgb(83, 83, 83);

        // Gallery colors
        public Color GalleryBackground { get; set; } = Color.White;
        public Color GalleryBorder { get; set; } = Color.FromArgb(171, 171, 171);
        public Color GalleryItemBackground { get; set; } = Color.Transparent;
        public Color GalleryItemBackgroundHover { get; set; } = Color.FromArgb(232, 239, 247);
        public Color GalleryItemBackgroundSelected { get; set; } = Color.FromArgb(200, 220, 240);
        public Color GalleryItemBorder { get; set; } = Color.Transparent;
        public Color GalleryItemBorderHover { get; set; } = Color.FromArgb(164, 206, 249);
        public Color GalleryItemBorderSelected { get; set; } = Color.FromArgb(98, 163, 229);

        // Dropdown/Popup colors
        public Color DropDownBackground { get; set; } = Color.FromArgb(255, 255, 255);
        public Color DropDownBorder { get; set; } = Color.FromArgb(171, 171, 171);
        public Color DropDownShadow { get; set; } = Color.FromArgb(40, 0, 0, 0);

        // Separator colors
        public Color Separator { get; set; } = Color.FromArgb(190, 190, 190);

        // Color picker colors
        public Color ColorPickerNoColor { get; set; } = Color.FromArgb(250, 250, 250);
        public Color ColorPickerAutomaticColor { get; set; } = Color.Black;

        /// <summary>
        /// Default opaque background color for groups when GroupBackground is transparent.
        /// Matches the light gradient top color of the Office-style ribbon.
        /// </summary>
        public static readonly Color DefaultOpaqueGroupBackground = Color.FromArgb(253, 253, 254);

        /// <summary>
        /// Gets the group background color, guaranteed to be opaque.
        /// If GroupBackground has any transparency, returns DefaultOpaqueGroupBackground.
        /// </summary>
        public Color GetOpaqueGroupBackground()
        {
            return GroupBackground.A == 255 ? GroupBackground : DefaultOpaqueGroupBackground;
        }

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
                // Selected text same as normal - selection indicated by background, not text color
                TabTextSelected = Color.FromArgb(241, 241, 241),

                RibbonBackground = Color.FromArgb(37, 37, 38),
                RibbonBorder = Color.FromArgb(63, 63, 70),

                GroupBackground = Color.FromArgb(37, 37, 38),
                GroupBorder = Color.FromArgb(63, 63, 70),
                GroupLabelText = Color.FromArgb(180, 180, 180),  // Brighter for dark theme visibility
                GroupSeparator = Color.FromArgb(70, 70, 75),     // More visible separator
                GroupLabelBackground = Color.FromArgb(30, 30, 32),
                GroupLabelBorder = Color.FromArgb(50, 50, 55),

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

                Separator = Color.FromArgb(63, 63, 70),

                // QAT colors for dark theme
                QatBackground = Color.FromArgb(45, 45, 48),
                QatButtonBackground = Color.Transparent,
                QatButtonBackgroundHover = Color.FromArgb(62, 62, 64),
                QatButtonBackgroundPressed = Color.FromArgb(0, 122, 204),
                QatButtonBorderHover = Color.FromArgb(63, 63, 70),
                QatButtonBorderPressed = Color.FromArgb(0, 122, 204),
                QatDropdownArrow = Color.FromArgb(200, 200, 200)
            };
        }
    }
}
