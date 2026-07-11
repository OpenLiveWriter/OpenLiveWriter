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

        /// <summary>Post title (shown in the shell's title field).</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Full editor body HTML, including the <c>&lt;!--more--&gt;</c> extended-entry
        /// break if present. Stored faithfully so a load round-trips the editor content.
        /// </summary>
        public string BodyHtml { get; set; } = string.Empty;

        /// <summary>Assigned categories (server category names).</summary>
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>True when this document represents a page rather than a post.</summary>
        public bool IsPage { get; set; }

        /// <summary>Whether a publish should mark the post published (vs. server draft).</summary>
        public bool IsPublished { get; set; } = true;

        /// <summary>UTC creation time; set when the document is first saved.</summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>UTC last-modified time; refreshed on every save.</summary>
        public DateTime DateModifiedUtc { get; set; }

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
                Contents = BodyHtml ?? string.Empty
            };

            if (Categories != null)
            {
                foreach (string c in Categories)
                {
                    if (!string.IsNullOrEmpty(c))
                        post.Categories.Add(c);
                }
            }

            return post;
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
                IsPublished = post.IsPublished
            };

            foreach (string c in post.Categories)
                doc.Categories.Add(c);

            return doc;
        }
    }
}
