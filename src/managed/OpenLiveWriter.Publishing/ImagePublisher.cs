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

        // Matches a complete <img> tag (through the closing >) whose src is a
        // file:// URI. Used by the rewrite pass so display dimensions can be read
        // from the whole tag and the tag can be wrapped in a click-through anchor.
        private static readonly Regex FileImageTagRegex = new Regex(
            @"<img\b[^>]*?\bsrc\s*=\s*""(?<uri>file://[^""]+)""[^>]*>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        // Display-size probes: CSS px lengths (inline style overrides the width/height
        // attributes in the browser, so style is probed first) and plain attributes.
        private static readonly Regex StyleWidthRegex = new Regex(
            @"\bwidth\s*:\s*(?<v>\d+)px",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex StyleHeightRegex = new Regex(
            @"\bheight\s*:\s*(?<v>\d+)px",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex AttrWidthRegex = new Regex(
            @"\bwidth\s*=\s*[""'](?<v>\d+)[""']",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex AttrHeightRegex = new Regex(
            @"\bheight\s*=\s*[""'](?<v>\d+)[""']",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        // Anchor boundaries for the "already linked" check (no double-wrapping).
        private static readonly Regex AnchorOpenRegex = new Regex(
            @"<a\b[^>]*>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex AnchorCloseRegex = new Regex(
            @"</a\s*>",
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
        /// Reads the display size of an <c>&lt;img&gt;</c> tag from its inline style
        /// (<c>width:…px;height:…px</c>, which overrides the attributes in the browser)
        /// or its <c>width</c>/<c>height</c> attributes. Both dimensions must be
        /// present and positive; anything else (no sizing, one-sided sizing,
        /// non-px units) returns false. Pure; no I/O.
        /// </summary>
        public static bool TryGetDisplaySize(string imgTag, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (string.IsNullOrEmpty(imgTag))
                return false;

            int w = MatchPx(StyleWidthRegex, imgTag) ?? MatchPx(AttrWidthRegex, imgTag) ?? 0;
            int h = MatchPx(StyleHeightRegex, imgTag) ?? MatchPx(AttrHeightRegex, imgTag) ?? 0;
            if (w <= 0 || h <= 0)
                return false;

            width = w;
            height = h;
            return true;
        }

        /// <summary>
        /// The "should resize?" decision (pure): resize when an explicit display size
        /// is set AND it is smaller than the natural size in both dimensions. Upscaled
        /// or mixed images publish their original bytes (the browser scales those
        /// better than a re-encode would, and there is no full-size original to gain).
        /// </summary>
        public static bool ShouldResizeForDisplay(
            int displayWidth, int displayHeight, int naturalWidth, int naturalHeight) =>
            displayWidth > 0 && displayHeight > 0
            && displayWidth < naturalWidth && displayHeight < naturalHeight;

        /// <summary>
        /// True when the &lt;img&gt; starting at <paramref name="imgIndex"/> already
        /// sits inside an anchor — the nearest anchor boundary before it is an
        /// opening <c>&lt;a&gt;</c>. Such images keep whatever they link to; the
        /// two-stage upload must not double-wrap them.
        /// </summary>
        internal static bool IsWrappedInAnchor(string html, int imgIndex)
        {
            if (string.IsNullOrEmpty(html) || imgIndex <= 0)
                return false;

            string prefix = html.Substring(0, imgIndex);
            int open = -1;
            foreach (Match m in AnchorOpenRegex.Matches(prefix))
                open = m.Index;
            int close = -1;
            foreach (Match m in AnchorCloseRegex.Matches(prefix))
                close = m.Index;
            return open > close;
        }

        private static int? MatchPx(Regex regex, string imgTag)
        {
            Match m = regex.Match(imgTag);
            if (!m.Success)
                return null;
            return int.TryParse(m.Groups["v"].Value,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int v) ? v : (int?)null;
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
        /// <param name="resizer">
        /// Optional two-stage-upload seam (Windows "Link to: source picture"
        /// behavior). When supplied, a file:// <c>&lt;img&gt;</c> whose display size
        /// (width/height attributes or inline px style) is smaller than its natural
        /// size in both dimensions uploads twice: the original bytes under the
        /// original file name, and a resized display copy named
        /// <c>{name}_{width}x{height}.png</c> (e.g. <c>photo_320x240.png</c>). The
        /// rewritten <c>src</c> points at the resized URL and the <c>&lt;img&gt;</c>
        /// is wrapped in <c>&lt;a href="{original-url}"&gt;</c> — unless it already
        /// sits inside an anchor, whose target is respected (no double-wrap).
        /// Images without a qualifying display size keep the single-upload behavior.
        /// Null disables resizing entirely.
        /// </param>
        /// <exception cref="BlogClientPublishException">An image upload or file read failed.</exception>
        public static async Task<string> RewriteInlineImagesAsync(
            IBlogClient client, string blogId, string html, Func<string, byte[]> readLocalFile,
            PublishImageResizer resizer = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(html))
                return html;

            readLocalFile ??= System.IO.File.ReadAllBytes;

            string rewritten = await RewriteDataUriImagesAsync(client, blogId, html).ConfigureAwait(false);
            return await RewriteFileImagesAsync(client, blogId, rewritten, readLocalFile, resizer).ConfigureAwait(false);
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

        /// <summary>Per-unique-image upload plan for the file:// rewrite.</summary>
        private sealed class FileImagePlan
        {
            public LocalFileImage Image { get; set; }
            public byte[] Bytes { get; set; }
            public (int Width, int Height)? NaturalSize { get; set; }
            public string OriginalUrl { get; set; }

            /// <summary>Hosted URL per qualifying display size ("{w}x{h}" → URL).</summary>
            public Dictionary<string, string> ResizedUrls { get; } =
                new Dictionary<string, string>(StringComparer.Ordinal);
        }

        private static async Task<string> RewriteFileImagesAsync(
            IBlogClient client, string blogId, string html, Func<string, byte[]> readLocalFile,
            PublishImageResizer resizer)
        {
            IReadOnlyList<LocalFileImage> images = FindLocalFileImages(html);
            if (images.Count == 0)
                return html;

            // One plan per unique image: read the bytes once, probe the natural size
            // (when a resizer is wired), and upload the original — it is always
            // needed, either as the src (no resize) or as the click-through target.
            var plans = new Dictionary<string, FileImagePlan>(StringComparer.Ordinal);
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

                var plan = new FileImagePlan { Image = image, Bytes = bytes };
                if (resizer != null)
                {
                    // An undecodable (or probe-failing) image simply publishes
                    // without resizing; it must not abort the publish.
                    try { plan.NaturalSize = ToSize(resizer.ProbeNaturalSize(bytes)); }
                    catch (Exception) { plan.NaturalSize = null; }
                }

                plan.OriginalUrl = await UploadImageAsync(
                    client, blogId, image.FileName, MimeTypeForFile(image.FileName), bytes).ConfigureAwait(false);
                plans[image.FileUri] = plan;
            }

            // Pre-pass: for every distinct display size that qualifies, upload the
            // resized display copy once (deduped per image + size). The original was
            // already uploaded above, so ordering per image is original → resized.
            if (resizer != null)
            {
                foreach (Match m in FileImageTagRegex.Matches(html))
                {
                    FileImagePlan plan = plans[m.Groups["uri"].Value];
                    string dimsKey = QualifyingDisplaySize(plan, m.Value);
                    if (dimsKey == null || plan.ResizedUrls.ContainsKey(dimsKey))
                        continue;

                    if (!TryGetDisplaySize(m.Value, out int displayW, out int displayH))
                        continue;

                    byte[] resizedBytes;
                    try
                    {
                        resizedBytes = resizer.Resize(plan.Bytes, displayW, displayH);
                    }
                    catch (Exception ex) when (!(ex is BlogClientPublishException))
                    {
                        throw new BlogClientPublishException(
                            $"Failed to resize local image '{plan.Image.LocalPath}' before publishing: {ex.Message}");
                    }

                    if (resizedBytes == null || resizedBytes.Length == 0)
                    {
                        throw new BlogClientPublishException(
                            $"Failed to resize local image '{plan.Image.LocalPath}' before publishing: the resizer returned no bytes.");
                    }

                    plan.ResizedUrls[dimsKey] = await UploadImageAsync(
                        client, blogId, ResizedFileName(plan.Image.FileName, displayW, displayH),
                        "image/png", resizedBytes).ConfigureAwait(false);
                }
            }

            // Rewrite pass: swap each file:// src for its hosted URL and wrap the
            // qualifying tags in a click-through anchor to the original.
            return FileImageTagRegex.Replace(html, m =>
            {
                FileImagePlan plan = plans[m.Groups["uri"].Value];
                string dimsKey = QualifyingDisplaySize(plan, m.Value);

                string resizedUrl = null;
                bool resized = dimsKey != null && plan.ResizedUrls.TryGetValue(dimsKey, out resizedUrl);
                string url = resized ? resizedUrl : plan.OriginalUrl;

                string newTag = FileImageRegex.Replace(m.Value, srcMatch =>
                    srcMatch.Value.Substring(0, srcMatch.Groups["uri"].Index - srcMatch.Index) + url + "\"");

                return resized && !IsWrappedInAnchor(html, m.Index)
                    ? "<a href=\"" + plan.OriginalUrl + "\">" + newTag + "</a>"
                    : newTag;
            });
        }

        // The display-size key ("{w}x{h}") when this occurrence qualifies for the
        // two-stage upload, else null. Pure given the plan's probed natural size.
        private static string QualifyingDisplaySize(FileImagePlan plan, string imgTag)
        {
            if (!plan.NaturalSize.HasValue)
                return null;
            if (!TryGetDisplaySize(imgTag, out int displayW, out int displayH))
                return null;
            if (!ShouldResizeForDisplay(displayW, displayH,
                plan.NaturalSize.Value.Width, plan.NaturalSize.Value.Height))
                return null;
            return displayW.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + "x" + displayH.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        // Resized display copies are PNG (the resizer seam's contract) and named
        // after the original file plus the display size: photo.png → photo_320x240.png.
        private static string ResizedFileName(string fileName, int width, int height)
        {
            string baseName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(baseName))
                baseName = "image";
            return baseName + "_"
                + width.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x"
                + height.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".png";
        }

        private static (int Width, int Height)? ToSize(ValueTuple<int, int>? probed) =>
            probed.HasValue ? (probed.Value.Item1, probed.Value.Item2) : ((int Width, int Height)?)null;

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
