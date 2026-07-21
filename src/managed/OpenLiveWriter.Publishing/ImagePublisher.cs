// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Upload-on-publish for inline images. The Avalonia editor embeds inserted images
    /// as base64 <c>data:</c> URIs (<c>&lt;img src="data:image/png;base64,..."&gt;</c>).
    /// Before a post is transmitted, this scans the body for those data-URI images,
    /// uploads each unique one via <see cref="IBlogClient.NewMediaObjectAsync"/>, and rewrites
    /// every <c>src</c> to the hosted URL the server returns — so the published HTML
    /// references real URLs instead of carrying multi-megabyte embedded payloads.
    ///
    /// Behavior:
    ///  - No inline images → the HTML is returned unchanged (no upload calls).
    ///  - Identical images (same data URI) are uploaded once and share the hosted URL.
    ///  - An upload failure surfaces as <see cref="BlogClientPublishException"/> so the
    ///    caller aborts rather than publishing broken/half-rewritten HTML.
    ///
    /// Pure/offline: scanning (<see cref="FindInlineImages"/>) needs no client, and the
    /// rewrite uses whatever <see cref="IBlogClient"/> is injected (a fake in tests).
    /// </summary>
    public static class ImagePublisher
    {
        // Matches a base64 image data URI. The base64 payload charset excludes the quote
        // characters that delimit an attribute, so the match stops at the closing quote.
        private static readonly Regex DataUriRegex = new Regex(
            @"data:image/(?<subtype>[A-Za-z0-9.+-]+);base64,(?<data>[A-Za-z0-9+/=\s]+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>A single inline base64 image discovered in editor HTML.</summary>
        public sealed class InlineImage
        {
            /// <summary>The full <c>data:</c> URI exactly as it appears in the HTML.</summary>
            public string DataUri { get; internal set; }

            /// <summary>The MIME type, e.g. <c>image/png</c>.</summary>
            public string MimeType { get; internal set; }

            /// <summary>The base64 payload text (whitespace removed).</summary>
            public string Base64Payload { get; internal set; }

            /// <summary>The decoded image bytes.</summary>
            public byte[] DecodedBytes { get; internal set; }

            /// <summary>A file extension derived from the MIME subtype (e.g. <c>png</c>).</summary>
            public string FileExtension { get; internal set; }
        }

        /// <summary>
        /// Finds every distinct base64 image data URI in <paramref name="html"/>, in order
        /// of first appearance. Duplicates (identical data URIs) collapse to one entry.
        /// Entries whose base64 fails to decode are skipped. No network access.
        /// </summary>
        public static IReadOnlyList<InlineImage> FindInlineImages(string html)
        {
            var results = new List<InlineImage>();
            if (string.IsNullOrEmpty(html))
                return results;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match m in DataUriRegex.Matches(html))
            {
                string dataUri = m.Value;
                if (!seen.Add(dataUri))
                    continue;

                string subtype = m.Groups["subtype"].Value;
                string payload = StripWhitespace(m.Groups["data"].Value);

                byte[] bytes;
                try { bytes = Convert.FromBase64String(payload); }
                catch (FormatException) { continue; }

                results.Add(new InlineImage
                {
                    DataUri = dataUri,
                    MimeType = "image/" + subtype,
                    Base64Payload = payload,
                    DecodedBytes = bytes,
                    FileExtension = ExtensionForSubtype(subtype)
                });
            }

            return results;
        }

        /// <summary>
        /// Uploads every distinct inline image via <paramref name="client"/> and returns
        /// <paramref name="html"/> with each data URI replaced by its hosted URL. When
        /// there are no inline images the input is returned unchanged (no upload calls).
        /// </summary>
        /// <exception cref="BlogClientPublishException">An image upload failed.</exception>
        public static async Task<string> RewriteInlineImagesAsync(IBlogClient client, string blogId, string html)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(html))
                return html;

            IReadOnlyList<InlineImage> images = FindInlineImages(html);
            if (images.Count == 0)
                return html;

            var urlByDataUri = new Dictionary<string, string>(StringComparer.Ordinal);
            int index = 0;
            foreach (InlineImage image in images)
            {
                index++;
                string fileName = "image" + index.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "." + image.FileExtension;

                string hostedUrl;
                try
                {
                    hostedUrl = await client.NewMediaObjectAsync(
                        blogId, fileName, image.MimeType, image.DecodedBytes).ConfigureAwait(false);
                }
                catch (BlogClientPublishException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new BlogClientPublishException(
                        $"Failed to upload image '{fileName}' before publishing: {ex.Message}");
                }

                if (string.IsNullOrEmpty(hostedUrl))
                {
                    throw new BlogClientPublishException(
                        $"The blog returned no hosted URL for uploaded image '{fileName}'.");
                }

                urlByDataUri[image.DataUri] = hostedUrl;
            }

            // Replace via the same regex so only genuine data URIs are rewritten, and every
            // occurrence (including duplicates) is swapped for the deduplicated hosted URL.
            return DataUriRegex.Replace(html, m =>
                urlByDataUri.TryGetValue(m.Value, out string url) ? url : m.Value);
        }

        private static string StripWhitespace(string value)
        {
            if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
            var sb = new System.Text.StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (!char.IsWhiteSpace(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private static string ExtensionForSubtype(string subtype)
        {
            switch ((subtype ?? string.Empty).ToLowerInvariant())
            {
                case "jpeg":
                case "jpg":
                    return "jpg";
                case "svg+xml":
                case "svg":
                    return "svg";
                case "x-icon":
                case "vnd.microsoft.icon":
                    return "ico";
                case "tiff":
                    return "tif";
                default:
                    // png, gif, webp, bmp, etc. use the subtype directly when it is a
                    // simple token; otherwise fall back to a generic binary extension.
                    return Regex.IsMatch(subtype ?? string.Empty, "^[A-Za-z0-9]+$")
                        ? subtype.ToLowerInvariant()
                        : "bin";
            }
        }
    }
}
