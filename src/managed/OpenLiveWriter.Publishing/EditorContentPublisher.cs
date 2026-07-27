// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Cross-platform port of the editor-HTML → publish transformation:
    /// raw editor HTML → linebreak trim (HTMLTrimmer-equivalent) → invalid-XML-char
    /// scrub → <see cref="BlogPost"/> with main/extended split → transport
    /// (<see cref="IBlogClient"/>). This is the entry point the Avalonia editor
    /// calls with <c>WebViewEditor.GetContentAsync()</c> output.
    /// </summary>
    public static class EditorContentPublisher
    {
        /// <summary>
        /// Builds a <see cref="BlogPost"/> from raw editor HTML: strips leading/
        /// trailing linebreak noise, scrubs invalid XML characters, and splits the
        /// body at the extended-entry break.
        /// </summary>
        public static BlogPost BuildPost(string title, string editorHtml, bool publish, params string[] categories)
        {
            string trimmed = TrimLinebreaks(editorHtml ?? string.Empty);
            string scrubbed = XmlCharacterHelper.RemoveInvalidXmlChars(trimmed);

            var post = new BlogPost
            {
                Title = title ?? string.Empty,
                IsPublished = publish
            };

            // Setting Contents scrubs and splits at the extended-entry break.
            post.Contents = scrubbed;

            if (categories != null)
            {
                foreach (string c in categories)
                {
                    if (!string.IsNullOrEmpty(c))
                        post.Categories.Add(c);
                }
            }

            return post;
        }

        /// <summary>
        /// Uploads inline (data-URI) images and submits a new post, returning the id.
        /// Images are hosted via <see cref="IBlogClient.NewMediaObjectAsync"/> and the body is
        /// rewritten to reference the returned URLs before the post is built, so the
        /// published HTML never carries embedded base64. No-op when there are no images.
        /// </summary>
        public static Task<string> PublishAsync(IBlogClient client, string blogId, string title, string editorHtml,
            bool publish, params string[] categories)
        {
            return PublishOrEditAsync(client, blogId, existingPostId: null, title, editorHtml, publish, categories);
        }

        /// <summary>
        /// Uploads inline images (rewriting the body to hosted URLs) and then either creates
        /// a new post or, when <paramref name="existingPostId"/> is supplied, edits the
        /// existing server post. Returns the server post id (the existing id on an edit).
        /// This is the single entry point the shell uses so a re-publish of an
        /// already-published document targets the same post via <c>metaWeblog.editPost</c>.
        /// When <paramref name="isPage"/> is true the page methods
        /// (<c>wp.newPage</c>/<c>wp.editPage</c>) are used instead, so pages stay pages.
        /// When <paramref name="publishDateUtc"/> is set it is sent as
        /// <c>dateCreated</c> (scheduled/backdated posts); null omits the member so the
        /// server stamps its own time. <paramref name="slug"/> and
        /// <paramref name="excerpt"/> are sent as <c>wp_slug</c>/<c>mt_excerpt</c>
        /// (posts and pages); <paramref name="pingUrls"/> is sent as
        /// <c>mt_tb_ping_urls</c> (posts only). Empty values omit the members.
        /// <paramref name="imageResizer"/> enables the Windows-style two-stage image
        /// upload (resized display copy + original click-through); null keeps the
        /// single-upload behavior. See <see cref="ImagePublisher"/>.
        /// </summary>
        public static async Task<string> PublishOrEditAsync(IBlogClient client, string blogId, string existingPostId,
            string title, string editorHtml, bool publish, IEnumerable<string> categories,
            string keywords = null, bool isPage = false, System.DateTime? publishDateUtc = null,
            string slug = null, string excerpt = null, IEnumerable<string> pingUrls = null,
            PublishImageResizer imageResizer = null)
        {
            string hosted = await ImagePublisher.RewriteInlineImagesAsync(
                client, blogId, editorHtml ?? string.Empty, readLocalFile: null, resizer: imageResizer).ConfigureAwait(false);
            string[] categoryArray = categories?.Where(c => !string.IsNullOrEmpty(c)).ToArray()
                ?? System.Array.Empty<string>();
            BlogPost post = BuildPost(title, hosted, publish, categoryArray);
            post.IsPage = isPage;
            post.DateCreatedUtc = publishDateUtc;
            post.Slug = slug ?? string.Empty;
            post.Excerpt = excerpt ?? string.Empty;

            if (!string.IsNullOrEmpty(keywords))
                post.Keywords = keywords;

            if (pingUrls != null)
            {
                foreach (string url in pingUrls)
                {
                    string t = url?.Trim();
                    if (!string.IsNullOrEmpty(t))
                        post.PingUrls.Add(t);
                }
            }

            if (!string.IsNullOrEmpty(existingPostId))
            {
                post.Id = existingPostId;
                if (post.IsPage)
                    await client.EditPageAsync(blogId, post, publish).ConfigureAwait(false);
                else
                    await client.EditPostAsync(blogId, post, publish).ConfigureAwait(false);
                return existingPostId;
            }

            return post.IsPage
                ? await client.NewPageAsync(blogId, post, publish).ConfigureAwait(false)
                : await client.NewPostAsync(blogId, post, publish).ConfigureAwait(false);
        }

        /// <summary>
        /// Mirrors the linebreak-strip step of the Windows publish pipeline
        /// (HTMLTrimmer): normalizes CRLF and trims surrounding whitespace/newlines.
        /// </summary>
        private static string TrimLinebreaks(string html)
        {
            return html
                .Replace("\r\n", "\n")
                .Trim('\n', '\r', ' ', '\t');
        }
    }
}
