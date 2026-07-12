// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Minimal cross-platform publish transport contract. Mirrors the core
    /// <c>NewPost</c>/<c>EditPost</c> surface of the Windows
    /// <c>OpenLiveWriter.Extensibility.BlogClient.IBlogClient</c>, scoped to what
    /// the first publish slice needs. The Windows implementation is XML-RPC
    /// MetaWeblog; see <see cref="MetaWeblogXmlRpcClient"/>.
    /// </summary>
    public interface IBlogClient
    {
        /// <summary>Provider options that shape the generated payload.</summary>
        IBlogClientOptions Options { get; }

        /// <summary>Creates a new post and returns the server-assigned post id.</summary>
        string NewPost(string blogId, BlogPost post, bool publish);

        /// <summary>Edits an existing post (identified by <see cref="BlogPost.Id"/>).</summary>
        void EditPost(string blogId, BlogPost post, bool publish);

        /// <summary>
        /// Uploads a binary media object (image/attachment) to the blog and returns the
        /// hosted URL the server assigns. Mirrors <c>metaWeblog.newMediaObject</c>: the
        /// <paramref name="bits"/> are the raw file bytes (base64-encoded on the wire).
        /// Used by the publish path to host inline (data-URI) images before the post is
        /// sent so the body references real URLs rather than embedded base64.
        /// </summary>
        string NewMediaObject(string blogId, string fileName, string mimeType, byte[] bits);

        /// <summary>
        /// Fetches the categories available on the blog (<c>metaWeblog.getCategories</c>).
        /// The publish UI presents these for selection; the chosen category names are then
        /// included inline in the post struct. Returns an empty list when the provider
        /// exposes no categories.
        /// </summary>
        IReadOnlyList<BlogPostCategory> GetCategories(string blogId);
    }

    /// <summary>
    /// Subset of the Windows <c>IBlogClientOptions</c> needed to build a MetaWeblog
    /// payload for the minimal publish path.
    /// </summary>
    public interface IBlogClientOptions
    {
        /// <summary>
        /// When true, main/extended contents are sent as separate
        /// <c>description</c> / <c>mt_text_more</c> members; otherwise they are merged.
        /// </summary>
        bool SupportsExtendedEntries { get; }

        /// <summary>When true, categories are included inline in the post struct.</summary>
        bool SupportsCategoriesInline { get; }
    }

    /// <summary>Default options: MetaWeblog with extended entries and inline categories.</summary>
    public sealed class BlogClientOptions : IBlogClientOptions
    {
        public bool SupportsExtendedEntries { get; set; } = true;

        public bool SupportsCategoriesInline { get; set; } = true;

        public static BlogClientOptions Default => new BlogClientOptions();
    }
}
