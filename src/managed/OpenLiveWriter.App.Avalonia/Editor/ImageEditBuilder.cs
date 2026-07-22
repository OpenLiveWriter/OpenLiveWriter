// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// The set of changes to apply to the editor's selected image. Null members
    /// mean "leave unchanged"; the serialized payload simply omits them. This is
    /// the argument to <c>OLWBridge.applyImageAttrs</c>.
    /// </summary>
    public class ImageAttributes
    {
        /// <summary>New display width in px; null leaves it unchanged. Use
        /// <see cref="ClearSize"/> to reset to natural size.</summary>
        public int? Width { get; set; }

        /// <summary>New display height in px; null leaves it unchanged.</summary>
        public int? Height { get; set; }

        /// <summary>When true, width/height attributes and inline styles are
        /// removed (Original size preset). Overrides <see cref="Width"/>/<see cref="Height"/>.</summary>
        public bool ClearSize { get; set; }

        /// <summary>Replacement alt text (empty string removes the attribute).</summary>
        public string Alt { get; set; }

        /// <summary>Replacement title text (empty string removes the attribute).</summary>
        public string Title { get; set; }

        /// <summary>Layout: inline/left/right/center (float or block+auto margins).</summary>
        public string Alignment { get; set; }

        /// <summary>Uniform margin in px; 0 clears margins. Null leaves unchanged.</summary>
        public int? MarginPx { get; set; }

        /// <summary>Absolute rotation in degrees (CSS transform); 0 removes it.</summary>
        public int? RotationDeg { get; set; }

        /// <summary>Solid border width in px; 0 removes the border. Null leaves unchanged.</summary>
        public int? BorderWidthPx { get; set; }

        /// <summary>Border color as <c>#RRGGBB</c> (used when <see cref="BorderWidthPx"/> is set).</summary>
        public string BorderColor { get; set; }
    }

    /// <summary>
    /// Pure builders/calculations for Picture Tools: the JSON payload for
    /// <c>OLWBridge.applyImageAttrs</c>, aspect-ratio math, and the Small /
    /// Medium / Large preset widths. Kept free of WebView dependencies so the
    /// whole pipeline is unit-testable headlessly.
    /// </summary>
    public static class ImageEditBuilder
    {
        /// <summary>Preset display widths (px) for the Custom size dropdown.
        /// Windows Live Writer's presets are user-configurable; these are the
        /// fixed mac defaults until a defaults dialog is ported.</summary>
        public const int SmallWidth = 160;
        public const int MediumWidth = 320;
        public const int LargeWidth = 640;

        /// <summary>
        /// Serializes the non-null members of <paramref name="attrs"/> as the JSON
        /// object argument for <c>OLWBridge.applyImageAttrs</c>.
        /// </summary>
        public static string BuildAttrsJson(ImageAttributes attrs)
        {
            if (attrs == null)
                return "{}";

            var payload = new Dictionary<string, object>();
            if (attrs.ClearSize)
            {
                payload["width"] = null;
                payload["height"] = null;
            }
            else
            {
                if (attrs.Width.HasValue) payload["width"] = attrs.Width.Value;
                if (attrs.Height.HasValue) payload["height"] = attrs.Height.Value;
            }
            if (attrs.Alt != null) payload["alt"] = attrs.Alt;
            if (attrs.Title != null) payload["title"] = attrs.Title;
            if (attrs.Alignment != null) payload["alignment"] = attrs.Alignment;
            if (attrs.MarginPx.HasValue) payload["margin"] = attrs.MarginPx.Value;
            if (attrs.RotationDeg.HasValue) payload["rotation"] = attrs.RotationDeg.Value;
            if (attrs.BorderWidthPx.HasValue)
            {
                payload["borderWidth"] = attrs.BorderWidthPx.Value;
                if (attrs.BorderColor != null) payload["borderColor"] = attrs.BorderColor;
            }

            return JsonSerializer.Serialize(payload);
        }

        /// <summary>
        /// The height that preserves the natural aspect ratio for the given
        /// display width (rounded to the nearest pixel). Returns null when the
        /// natural dimensions are unknown (image not yet loaded) — callers then
        /// apply the width alone and let the browser scale proportionally.
        /// </summary>
        public static int? HeightForWidth(int naturalWidth, int naturalHeight, int width)
        {
            if (naturalWidth <= 0 || naturalHeight <= 0 || width <= 0)
                return null;
            return Math.Max(1, (int)Math.Round(width * (double)naturalHeight / naturalWidth));
        }

        /// <summary>
        /// The width that preserves the natural aspect ratio for the given
        /// display height (rounded). Returns null when natural dims are unknown.
        /// </summary>
        public static int? WidthForHeight(int naturalWidth, int naturalHeight, int height)
        {
            if (naturalWidth <= 0 || naturalHeight <= 0 || height <= 0)
                return null;
            return Math.Max(1, (int)Math.Round(height * (double)naturalWidth / naturalHeight));
        }

        /// <summary>
        /// Normalizes a free-text alignment token to inline/left/right/center;
        /// anything else maps to inline.
        /// </summary>
        public static string NormalizeAlignment(string alignment)
        {
            switch ((alignment ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "left": return "left";
                case "right": return "right";
                case "center": return "center";
                default: return "inline";
            }
        }
    }
}
