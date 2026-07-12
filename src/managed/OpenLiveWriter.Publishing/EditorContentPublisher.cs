// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

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
        /// Images are hosted via <see cref="IBlogClient.NewMediaObject"/> and the body is
        /// rewritten to reference the returned URLs before the post is built, so the
        /// published HTML never carries embedded base64. No-op when there are no images.
        /// </summary>
        public static string Publish(IBlogClient client, string blogId, string title, string editorHtml,
            bool publish, params string[] categories)
        {
            string hosted = ImagePublisher.RewriteInlineImages(client, blogId, editorHtml ?? string.Empty);
            BlogPost post = BuildPost(title, hosted, publish, categories);
            return client.NewPost(blogId, post, publish);
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
