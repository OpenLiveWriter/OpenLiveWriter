// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Cross-platform post/draft document — the editable unit the macOS editor
    /// works on. Holds the identity, title, body HTML, blog/category metadata and
    /// create/modify timestamps that a local draft persists, plus a transient
    /// dirty flag the UI uses to prompt on unsaved changes.
    ///
    /// This is the persisted counterpart to the transport-only <see cref="BlogPost"/>:
    /// <see cref="ToBlogPost"/> / <see cref="FromBlogPost"/> convert between the two so
    /// the same document can be saved as a draft and published without a second model.
    /// Kept free of any WinForms/MSHTML/WebView dependency so it is unit-testable with
    /// plain file I/O.
    /// </summary>
    public class PostDocument
    {
        /// <summary>
        /// Stable local draft identifier. Empty until the document is first saved
        /// (assigned by the draft store), analogous to <see cref="BlogPost.Id"/>.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Server-side blog identifier this post targets (empty = unassigned).</summary>
        public string BlogId { get; set; } = string.Empty;

        /// <summary>
        /// Server-side post identifier returned by the blog after a successful publish
        /// (empty until first published). Distinct from <see cref="Id"/>, which is the
        /// local draft id; recorded so a later edit could target the same server post.
        /// </summary>
        public string PublishedPostId { get; set; } = string.Empty;

        /// <summary>Post title (shown in the shell's title field).</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Full editor body HTML, including the <c>&lt;!--more--&gt;</c> extended-entry
        /// break if present. Stored faithfully so a load round-trips the editor content.
        /// </summary>
        public string BodyHtml { get; set; } = string.Empty;

        /// <summary>Assigned categories (server category names).</summary>
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// Post keywords/tags carried to the blog as <c>mt_keywords</c>. Managed by the
        /// Insert/Edit Tags dialog; persisted with the draft.
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();

        /// <summary>True when this document represents a page rather than a post.</summary>
        public bool IsPage { get; set; }

        /// <summary>Whether a publish should mark the post published (vs. server draft).</summary>
        public bool IsPublished { get; set; } = true;

        /// <summary>UTC creation time; set when the document is first saved.</summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>UTC last-modified time; refreshed on every save.</summary>
        public DateTime DateModifiedUtc { get; set; }

        /// <summary>
        /// Optional publish date set via Post Properties (F2). When set it is sent as
        /// the MetaWeblog <c>dateCreated</c> member on publish — a future date
        /// schedules the post on servers that honor it. Null means publish
        /// immediately (the server stamps its own time).
        /// </summary>
        public DateTime? PublishDateUtc { get; set; }

        /// <summary>
        /// URL slug set via Post Properties (F2); carried to the blog as
        /// <c>wp_slug</c>. Persisted with the draft.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Post excerpt set via Post Properties (F2); carried to the blog as
        /// <c>mt_excerpt</c>. Persisted with the draft.
        /// </summary>
        public string Excerpt { get; set; } = string.Empty;

        /// <summary>
        /// Trackback/ping URLs set via Post Properties (F2); carried to the blog as
        /// the <c>mt_tb_ping_urls</c> array (posts only). Persisted with the draft.
        /// </summary>
        public List<string> PingUrls { get; set; } = new List<string>();

        /// <summary>True once the document has been saved at least once (has an id).</summary>
        [JsonIgnore]
        public bool IsSaved => !string.IsNullOrEmpty(Id);

        /// <summary>
        /// Transient unsaved-changes flag. Not persisted; the shell sets it on title/
        /// body edits and clears it after a successful save.
        /// </summary>
        [JsonIgnore]
        public bool IsDirty { get; set; }

        /// <summary>
        /// Projects this document into a transport <see cref="BlogPost"/> for publishing.
        /// The body HTML is assigned via <see cref="BlogPost.Contents"/>, which scrubs
        /// invalid XML characters and splits at the extended-entry break.
        /// </summary>
        public BlogPost ToBlogPost()
        {
            var post = new BlogPost
            {
                Id = Id ?? string.Empty,
                Title = Title ?? string.Empty,
                IsPage = IsPage,
                IsPublished = IsPublished,
                DateCreatedUtc = PublishDateUtc,
                Contents = BodyHtml ?? string.Empty,
                Slug = Slug ?? string.Empty,
                Excerpt = Excerpt ?? string.Empty
            };

            if (Categories != null)
            {
                foreach (string c in Categories)
                {
                    if (!string.IsNullOrEmpty(c))
                        post.Categories.Add(c);
                }
            }

            post.Keywords = JoinKeywords(Keywords);
            AddPingUrls(post.PingUrls, PingUrls);

            return post;
        }

        /// <summary>Copies non-empty ping URLs into <paramref name="target"/>.</summary>
        private static void AddPingUrls(IList<string> target, IEnumerable<string> source)
        {
            if (source == null)
                return;
            foreach (string url in source)
            {
                string t = url?.Trim();
                if (!string.IsNullOrEmpty(t))
                    target.Add(t);
            }
        }

        /// <summary>
        /// Splits a multi-line ping-URL text (one URL per line, as edited in Post
        /// Properties) into trimmed tokens; blank lines are dropped.
        /// </summary>
        public static List<string> SplitPingUrls(string pingUrlsText)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(pingUrlsText))
                return result;
            foreach (string line in pingUrlsText.Replace("\r\n", "\n").Split('\n'))
            {
                string t = line.Trim();
                if (t.Length > 0)
                    result.Add(t);
            }
            return result;
        }

        /// <summary>Joins keyword tokens into the comma-separated <c>mt_keywords</c> string.</summary>
        public static string JoinKeywords(IEnumerable<string> keywords)
        {
            if (keywords == null)
                return string.Empty;
            var cleaned = new List<string>();
            foreach (string k in keywords)
            {
                string t = k?.Trim();
                if (!string.IsNullOrEmpty(t))
                    cleaned.Add(t);
            }
            return string.Join(", ", cleaned);
        }

        /// <summary>Splits a comma-separated <c>mt_keywords</c> string into trimmed tokens.</summary>
        public static List<string> SplitKeywords(string keywords)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(keywords))
                return result;
            foreach (string part in keywords.Split(','))
            {
                string t = part.Trim();
                if (t.Length > 0)
                    result.Add(t);
            }
            return result;
        }

        /// <summary>
        /// Creates a document from a transport <see cref="BlogPost"/> (e.g. when
        /// opening a server post for local editing). The local draft id is left empty
        /// so a subsequent save creates a new local draft.
        /// </summary>
        public static PostDocument FromBlogPost(BlogPost post)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));

            var doc = new PostDocument
            {
                BlogId = string.Empty,
                Title = post.Title,
                BodyHtml = post.Contents,
                IsPage = post.IsPage,
                IsPublished = post.IsPublished,
                Slug = post.Slug ?? string.Empty,
                Excerpt = post.Excerpt ?? string.Empty
            };

            foreach (string c in post.Categories)
                doc.Categories.Add(c);

            doc.Keywords = SplitKeywords(post.Keywords);
            foreach (string url in post.PingUrls)
            {
                if (!string.IsNullOrEmpty(url))
                    doc.PingUrls.Add(url);
            }

            return doc;
        }

        /// <summary>
        /// Creates a document from a post fetched from the blog (Open from Blog). The
        /// document is marked published-to-<paramref name="blogId"/> with the server post
        /// id recorded, so a subsequent publish routes through the edit path
        /// (<c>metaWeblog.editPost</c> / <c>wp.editPage</c>) instead of creating a
        /// duplicate. The local draft id stays empty so a save creates a new local draft.
        /// </summary>
        public static PostDocument FromServerPost(ServerPost post, string blogId)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));

            var doc = new PostDocument
            {
                BlogId = blogId ?? string.Empty,
                PublishedPostId = post.PostId ?? string.Empty,
                Title = post.Title ?? string.Empty,
                BodyHtml = post.BodyHtml,
                IsPage = post.IsPage,
                // Only a server-side draft should stay unpublished on republish;
                // publish/pending/private entries are treated as published content.
                IsPublished = !string.Equals(post.Status, "draft", StringComparison.OrdinalIgnoreCase),
                DateCreatedUtc = post.DateCreatedUtc ?? default,
                Slug = post.Slug ?? string.Empty,
                Excerpt = post.Excerpt ?? string.Empty
            };

            if (post.Categories != null)
            {
                foreach (string c in post.Categories)
                {
                    if (!string.IsNullOrEmpty(c))
                        doc.Categories.Add(c);
                }
            }

            doc.Keywords = SplitKeywords(post.Keywords);

            if (post.PingUrls != null)
            {
                foreach (string url in post.PingUrls)
                {
                    if (!string.IsNullOrEmpty(url))
                        doc.PingUrls.Add(url);
                }
            }

            return doc;
        }
    }
}
