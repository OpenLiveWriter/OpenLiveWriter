// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Builds a map embed block for the editor. The Windows "Insert Map" feature used
    /// Bing/Virtual Earth (`MapContentSource`/`MapForm`), whose embed/geocoding APIs and
    /// keys are long dead. This is the modern replacement built on
    /// <b>OpenStreetMap</b>: it needs no API key and exposes a standards-based embed
    /// endpoint (<c>openstreetmap.org/export/embed.html</c>).
    ///
    /// Two output shapes are produced, both wrapped in a <c>&lt;div class="olw-map"&gt;</c>
    /// so the ribbon's contextual Map Tools tab activates on selection:
    /// <list type="bullet">
    ///   <item>When coordinates are supplied, a responsive <c>&lt;iframe&gt;</c> centered
    ///   on the point with a marker, plus a "view larger map" link.</item>
    ///   <item>When only a place name is supplied (no offline geocoding), a static link
    ///   block to an OpenStreetMap search for that place.</item>
    /// </list>
    ///
    /// URL and HTML composition are pure/deterministic so they are unit-testable without
    /// a live WebView or any network access.
    /// </summary>
    public static class MapEmbedBuilder
    {
        /// <summary>Default zoom used when the dialog doesn't specify one.</summary>
        public const int DefaultZoom = 14;

        private const int MinZoom = 1;
        private const int MaxZoom = 19;

        /// <summary>
        /// Builds a map block from a human label and/or a coordinate string. When the
        /// coordinates parse, a marker-centered OpenStreetMap iframe is produced;
        /// otherwise a non-empty label yields a static OSM search link. Returns null
        /// when neither usable coordinates nor a label are supplied.
        /// </summary>
        public static string BuildMapHtml(string label, string coordinates, int zoom = DefaultZoom)
        {
            string trimmedLabel = label?.Trim();

            if (TryParseCoordinates(coordinates, out double lat, out double lon))
                return BuildEmbedHtml(lat, lon, ClampZoom(zoom), trimmedLabel);

            if (!string.IsNullOrEmpty(trimmedLabel))
                return BuildSearchLinkHtml(trimmedLabel);

            return null;
        }

        /// <summary>
        /// Parses a coordinate string of the form "lat, lon" (comma- or whitespace-
        /// separated) into decimal degrees. Latitude must be within ±90 and longitude
        /// within ±180. Returns false for anything unparseable or out of range.
        /// </summary>
        internal static bool TryParseCoordinates(string input, out double lat, out double lon)
        {
            lat = 0;
            lon = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string[] parts = input.Split(new[] { ',', ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return false;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat) ||
                !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
                return false;

            return lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180;
        }

        internal static int ClampZoom(int zoom) => Math.Clamp(zoom, MinZoom, MaxZoom);

        /// <summary>
        /// Builds the OpenStreetMap embed URL for a marker-centered map. The bounding
        /// box half-span shrinks as zoom increases (higher zoom ⇒ tighter box).
        /// </summary>
        internal static string BuildEmbedUrl(double lat, double lon, int zoom)
        {
            double half = 180.0 / Math.Pow(2, ClampZoom(zoom));
            double minLon = Clamp(lon - half, -180, 180);
            double maxLon = Clamp(lon + half, -180, 180);
            double minLat = Clamp(lat - half / 2, -90, 90);
            double maxLat = Clamp(lat + half / 2, -90, 90);

            return "https://www.openstreetmap.org/export/embed.html?bbox=" +
                   F(minLon) + "%2C" + F(minLat) + "%2C" + F(maxLon) + "%2C" + F(maxLat) +
                   "&layer=mapnik&marker=" + F(lat) + "%2C" + F(lon);
        }

        /// <summary>Builds the "view larger map" permalink for the given point.</summary>
        internal static string BuildPermalinkUrl(double lat, double lon, int zoom) =>
            "https://www.openstreetmap.org/?mlat=" + F(lat) + "&mlon=" + F(lon) +
            "#map=" + ClampZoom(zoom).ToString(CultureInfo.InvariantCulture) + "/" + F(lat) + "/" + F(lon);

        /// <summary>Builds the OpenStreetMap search URL for a place-name query.</summary>
        internal static string BuildSearchUrl(string query) =>
            "https://www.openstreetmap.org/search?query=" + Uri.EscapeDataString(query ?? string.Empty);

        private static string BuildEmbedHtml(double lat, double lon, int zoom, string label)
        {
            string embedUrl = EscapeAttr(BuildEmbedUrl(lat, lon, zoom));
            string permalink = EscapeAttr(BuildPermalinkUrl(lat, lon, zoom));
            string linkText = string.IsNullOrEmpty(label) ? "View larger map" : EscapeText(label);

            return
                "<div class=\"olw-map\" style=\"position:relative;padding-bottom:66%;height:0;overflow:hidden;max-width:100%;\">" +
                "<iframe src=\"" + embedUrl + "\" " +
                "style=\"position:absolute;top:0;left:0;width:100%;height:100%;border:0;\" " +
                "frameborder=\"0\" scrolling=\"no\" title=\"Map\"></iframe>" +
                "</div>" +
                "<small><a href=\"" + permalink + "\" target=\"_blank\" rel=\"noopener\">" +
                linkText + "</a></small>";
        }

        private static string BuildSearchLinkHtml(string query) =>
            "<div class=\"olw-map\">" +
            "<a href=\"" + EscapeAttr(BuildSearchUrl(query)) + "\" target=\"_blank\" rel=\"noopener\">" +
            "Map: " + EscapeText(query) + "</a></div>";

        private static string F(double value) =>
            Math.Round(value, 6).ToString("0.######", CultureInfo.InvariantCulture);

        private static double Clamp(double v, double min, double max) => Math.Clamp(v, min, max);

        private static string EscapeAttr(string s) =>
            s?.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";

        private static string EscapeText(string s) =>
            s?.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";
    }
}
