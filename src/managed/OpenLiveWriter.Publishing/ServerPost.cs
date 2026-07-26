// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Lightweight description of a post (or page) that already lives on the blog,
    /// as listed by <c>metaWeblog.getRecentPosts</c> / <c>wp.getPages</c>. Carries the
    /// identity + display metadata the Open-from-Blog picker needs; the full body
    /// (<see cref="ServerPost.Description"/> / <see cref="ServerPost.TextMore"/>) rides
    /// along on <see cref="ServerPost"/> since the MetaWeblog list structs include it.
    /// </summary>
    public class ServerPostInfo
    {
        /// <summary>Server-side post id (<c>postid</c>, or <c>page_id</c> for pages).</summary>
        public string PostId { get; set; } = string.Empty;

        /// <summary>Post title (<c>title</c>, or <c>page_title</c> for pages).</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Server-reported creation date (<c>dateCreated</c>), when present.</summary>
        public DateTime? DateCreatedUtc { get; set; }

        /// <summary>Public permalink, when the server reports one.</summary>
        public string Permalink { get; set; } = string.Empty;

        /// <summary>Publication status (<c>post_status</c> / <c>page_status</c>: publish, draft, …).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>True when this entry is a page rather than a post.</summary>
        public bool IsPage { get; set; }

        /// <summary>Category names assigned on the server (posts only).</summary>
        public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// A full server post as returned by <c>metaWeblog.getPost</c> (and by the
    /// <c>metaWeblog.getRecentPosts</c> / <c>wp.getPages</c> structs, which carry the
    /// same body members). <see cref="BodyHtml"/> rejoins the main/extended split with
    /// the editor's <c>&lt;!--more--&gt;</c> break so a fetched post round-trips into
    /// the editor exactly as it was authored.
    /// </summary>
    public sealed class ServerPost : ServerPostInfo
    {
        /// <summary>Main body HTML (MetaWeblog <c>description</c>).</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Extended body HTML (MetaWeblog <c>mt_text_more</c>).</summary>
        public string TextMore { get; set; } = string.Empty;

        /// <summary>Comma-separated keywords/tags (<c>mt_keywords</c>).</summary>
        public string Keywords { get; set; } = string.Empty;

        /// <summary>URL slug (<c>wp_slug</c>), when the server reports one.</summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>Post excerpt (<c>mt_excerpt</c>), when the server reports one.</summary>
        public string Excerpt { get; set; } = string.Empty;

        /// <summary>Trackback/ping URLs (<c>mt_tb_ping_urls</c>), when present.</summary>
        public IReadOnlyList<string> PingUrls { get; set; } = Array.Empty<string>();

        /// <summary>True when the struct carried a body (so opening needs no getPost call).</summary>
        public bool HasBody =>
            !string.IsNullOrEmpty(Description) || !string.IsNullOrEmpty(TextMore);

        /// <summary>
        /// Full editor body: main + extended joined by the extended-entry break, the
        /// inverse of the publish-time split in <see cref="BlogPost.Contents"/>.
        /// </summary>
        public string BodyHtml =>
            string.IsNullOrEmpty(TextMore)
                ? Description ?? string.Empty
                : (Description ?? string.Empty) + BlogPost.ExtendedEntryBreak + TextMore;
    }
}
