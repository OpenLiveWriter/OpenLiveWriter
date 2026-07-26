// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.App.Avalonia.ImageEditing
{
    /// <summary>
    /// Base64 <c>data:</c> URI decode/re-embed helpers for the Picture Tools
    /// pixel-baking pipeline: the selected image's bytes come out of its
    /// <c>src</c> attribute, and baked results go back in as a new data URI.
    /// Pure/deterministic so the round-trip is unit-testable.
    /// </summary>
    public static class ImageDataUri
    {
        /// <summary>
        /// Extracts the bytes from a base64 <c>data:</c> URI. Returns false for
        /// non-data URIs (http/https — fetch those instead), non-base64
        /// payloads, and malformed base64.
        /// </summary>
        public static bool TryDecode(string dataUri, out byte[] bytes)
        {
            bytes = null;
            if (string.IsNullOrEmpty(dataUri) ||
                !dataUri.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return false;

            int comma = dataUri.IndexOf(',');
            if (comma < 0)
                return false;

            string header = dataUri.Substring(0, comma);
            if (header.IndexOf(";base64", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            try
            {
                bytes = Convert.FromBase64String(dataUri.Substring(comma + 1));
                return true;
            }
            catch (FormatException)
            {
                bytes = null;
                return false;
            }
        }

        /// <summary>Wraps PNG bytes as an inline base64 <c>data:</c> URI for an img src.</summary>
        public static string BuildPng(byte[] pngBytes) =>
            "data:image/png;base64," + Convert.ToBase64String(pngBytes ?? Array.Empty<byte>());
    }
}
