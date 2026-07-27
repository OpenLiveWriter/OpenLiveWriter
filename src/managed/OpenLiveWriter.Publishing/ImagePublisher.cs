// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Upload-on-publish for local images. The Avalonia editor references inserted
    /// images by <c>file://</c> path (<c>&lt;img src="file:///…/Media/{id}/name.png"&gt;</c>),
    /// and legacy drafts may still carry base64 <c>data:</c> URIs. Before a post is
    /// transmitted, this scans the body for both, uploads each unique image via
    /// <see cref="IBlogClient.NewMediaObjectAsync"/>, and rewrites every <c>src</c> to
    /// the hosted URL the server returns — so the published HTML references real URLs
    /// instead of local paths or multi-megabyte embedded payloads.
    ///
    /// Behavior:
    ///  - No local images → the HTML is returned unchanged (no upload calls).
    ///  - Identical images (same data URI or same file path) are uploaded once and
    ///    share the hosted URL.
    ///  - An upload failure — including a missing local file — surfaces as
    ///    <see cref="BlogClientPublishException"/> so the caller aborts rather than
    ///    publishing broken/half-rewritten HTML.
    ///
    /// Pure/offline: scanning (<see cref="FindInlineImages"/>, <see cref="FindLocalFileImages"/>)
    /// needs no client; the file read behind the file:// handling is injectable
    /// (<c>readLocalFile</c>), and the rewrite uses whatever
    /// <see cref="IBlogClient"/> is injected (a fake in tests).
    /// </summary>
    public static class ImagePublisher
    {
        // Matches a base64 image data URI. The base64 payload charset excludes the quote
        // characters that delimit an attribute, so the match stops at the closing quote.
        private static readonly Regex DataUriRegex = new Regex(
            @"data:image/(?<subtype>[A-Za-z0-9.+-]+);base64,(?<data>[A-Za-z0-9+/=\s]+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Matches an <img> whose src is a file:// URI. Scoped to img tags so a
        // file:// hyperlink in an anchor is never rewritten or uploaded.
        private static readonly Regex FileImageRegex = new Regex(
            @"<img\b[^>]*?\bsrc\s*=\s*""(?<uri>file://[^""]+)""",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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

        /// <summary>A single local (file://) image discovered in editor HTML.</summary>
        public sealed class LocalFileImage
        {
            /// <summary>The full <c>file://</c> URI exactly as it appears in the HTML.</summary>
            public string FileUri { get; internal set; }

            /// <summary>The local file-system path the URI resolves to.</summary>
            public string LocalPath { get; internal set; }

            /// <summary>The file name (used as the upload's <c>name</c> member).</summary>
            public string FileName { get; internal set; }
        }

        /// <summary>
        /// Finds every distinct file:// image referenced by an <c>&lt;img&gt;</c> in
        /// <paramref name="html"/>, in order of first appearance. Duplicates (identical
        /// URIs) collapse to one entry; URIs that do not resolve to a local path are
        /// skipped. Anchors linking to file:// URLs are not matched. No file I/O.
        /// </summary>
        public static IReadOnlyList<LocalFileImage> FindLocalFileImages(string html)
        {
            var results = new List<LocalFileImage>();
            if (string.IsNullOrEmpty(html))
                return results;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match m in FileImageRegex.Matches(html))
            {
                string fileUri = m.Groups["uri"].Value;
                if (!seen.Add(fileUri))
                    continue;

                if (!Uri.TryCreate(fileUri, UriKind.Absolute, out Uri uri) || !uri.IsFile)
                    continue;

                results.Add(new LocalFileImage
                {
                    FileUri = fileUri,
                    LocalPath = uri.LocalPath,
                    FileName = System.IO.Path.GetFileName(uri.LocalPath)
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
        public static Task<string> RewriteInlineImagesAsync(IBlogClient client, string blogId, string html) =>
            RewriteInlineImagesAsync(client, blogId, html, readLocalFile: null);

        /// <summary>
        /// Uploads every distinct local image (base64 data URIs and file:// <c>&lt;img&gt;</c>
        /// references) via <paramref name="client"/> and returns <paramref name="html"/>
        /// with each local src replaced by its hosted URL. When there are no local
        /// images the input is returned unchanged (no upload calls).
        /// </summary>
        /// <param name="readLocalFile">
        /// Reads the bytes of a local image path (test seam). Null uses
        /// <see cref="System.IO.File.ReadAllBytes(string)"/>. A null return or throw
        /// aborts the publish with <see cref="BlogClientPublishException"/> — the
        /// partially rewritten HTML is never returned.
        /// </param>
        /// <exception cref="BlogClientPublishException">An image upload or file read failed.</exception>
        public static async Task<string> RewriteInlineImagesAsync(
            IBlogClient client, string blogId, string html, Func<string, byte[]> readLocalFile)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(html))
                return html;

            readLocalFile ??= System.IO.File.ReadAllBytes;

            string rewritten = await RewriteDataUriImagesAsync(client, blogId, html).ConfigureAwait(false);
            return await RewriteFileImagesAsync(client, blogId, rewritten, readLocalFile).ConfigureAwait(false);
        }

        private static async Task<string> RewriteDataUriImagesAsync(IBlogClient client, string blogId, string html)
        {
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

                string hostedUrl = await UploadImageAsync(
                    client, blogId, fileName, image.MimeType, image.DecodedBytes).ConfigureAwait(false);
                urlByDataUri[image.DataUri] = hostedUrl;
            }

            // Replace via the same regex so only genuine data URIs are rewritten, and every
            // occurrence (including duplicates) is swapped for the deduplicated hosted URL.
            return DataUriRegex.Replace(html, m =>
                urlByDataUri.TryGetValue(m.Value, out string url) ? url : m.Value);
        }

        private static async Task<string> RewriteFileImagesAsync(
            IBlogClient client, string blogId, string html, Func<string, byte[]> readLocalFile)
        {
            IReadOnlyList<LocalFileImage> images = FindLocalFileImages(html);
            if (images.Count == 0)
                return html;

            var urlByFileUri = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (LocalFileImage image in images)
            {
                byte[] bytes;
                try
                {
                    bytes = readLocalFile(image.LocalPath);
                }
                catch (Exception ex) when (!(ex is BlogClientPublishException))
                {
                    throw new BlogClientPublishException(
                        $"Failed to read local image '{image.LocalPath}' before publishing: {ex.Message}");
                }

                if (bytes == null || bytes.Length == 0)
                {
                    throw new BlogClientPublishException(
                        $"Failed to read local image '{image.LocalPath}' before publishing: the file is missing or empty.");
                }

                string hostedUrl = await UploadImageAsync(
                    client, blogId, image.FileName, MimeTypeForFile(image.FileName), bytes).ConfigureAwait(false);
                urlByFileUri[image.FileUri] = hostedUrl;
            }

            return FileImageRegex.Replace(html, m =>
                urlByFileUri.TryGetValue(m.Groups["uri"].Value, out string url)
                    ? m.Value.Substring(0, m.Groups["uri"].Index - m.Index) + url + "\""
                    : m.Value);
        }

        private static async Task<string> UploadImageAsync(
            IBlogClient client, string blogId, string fileName, string mimeType, byte[] bytes)
        {
            string hostedUrl;
            try
            {
                hostedUrl = await client.NewMediaObjectAsync(blogId, fileName, mimeType, bytes).ConfigureAwait(false);
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

            return hostedUrl;
        }

        // Best-effort MIME type from the local file's extension (defaults to PNG,
        // matching the editor's GuessImageMimeType).
        private static string MimeTypeForFile(string fileName)
        {
            switch ((System.IO.Path.GetExtension(fileName) ?? string.Empty).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                case ".svg":
                    return "image/svg+xml";
                default:
                    return "image/png";
            }
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
