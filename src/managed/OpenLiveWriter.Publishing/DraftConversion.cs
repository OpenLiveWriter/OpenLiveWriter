// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Markdown;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// One-time bulk conversion of local HTML drafts to Markdown when a blog account
    /// switches its editing format from HTML to Markdown.
    /// </summary>
    public static class DraftConversion
    {
        /// <summary>
        /// Returns true when <paramref name="store"/> contains at least one draft
        /// assigned to <paramref name="blogId"/>.
        /// </summary>
        public static bool HasDraftsForBlog(IDraftStore store, string blogId)
        {
            if (store == null || string.IsNullOrEmpty(blogId))
                return false;

            foreach (DraftInfo info in store.List())
            {
                PostDocument doc = store.Load(info.Id);
                if (doc != null && string.Equals(doc.BlogId, blogId, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Converts HTML drafts for <paramref name="blogId"/> to Markdown and returns
        /// the number of drafts updated.
        /// </summary>
        public static int ConvertBlogDraftsToMarkdown(IDraftStore store, string blogId, IMarkdownService markdown)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (markdown == null) throw new ArgumentNullException(nameof(markdown));
            if (string.IsNullOrEmpty(blogId))
                return 0;

            int converted = 0;
            foreach (DraftInfo info in store.List())
            {
                PostDocument doc = store.Load(info.Id);
                if (doc == null)
                    continue;
                if (!string.Equals(doc.BlogId, blogId, StringComparison.Ordinal))
                    continue;
                if (!ShouldConvertToMarkdown(doc))
                    continue;

                doc.BodyMarkdown = markdown.ToMarkdown(doc.BodyHtml ?? string.Empty);
                doc.BodyFormat = ContentFormat.Markdown;
                store.Save(doc);
                converted++;
            }

            return converted;
        }

        internal static bool ShouldConvertToMarkdown(PostDocument doc)
        {
            if (doc == null)
                return false;
            if (doc.BodyFormat == ContentFormat.Markdown)
                return false;
            if (doc.BodyFormat == ContentFormat.Html)
                return true;
            return string.IsNullOrEmpty(doc.BodyMarkdown) && !string.IsNullOrEmpty(doc.BodyHtml);
        }
    }
}
