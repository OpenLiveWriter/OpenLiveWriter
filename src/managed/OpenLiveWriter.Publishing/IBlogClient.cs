// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Minimal cross-platform publish transport contract. Mirrors the core
    /// <c>NewPost</c>/<c>EditPost</c> surface of the Windows
    /// <c>OpenLiveWriter.Extensibility.BlogClient.IBlogClient</c>, scoped to what
    /// the first publish slice needs. The Windows implementation is XML-RPC
    /// MetaWeblog; see <see cref="MetaWeblogXmlRpcClient"/>.
    ///
    /// All operations are genuinely async end-to-end (no sync-over-async) so the
    /// publish path never blocks the UI thread while the network round-trip runs.
    /// </summary>
    public interface IBlogClient
    {
        /// <summary>Provider options that shape the generated payload.</summary>
        IBlogClientOptions Options { get; }

        /// <summary>Creates a new post and returns the server-assigned post id.</summary>
        Task<string> NewPostAsync(string blogId, BlogPost post, bool publish);

        /// <summary>Edits an existing post (identified by <see cref="BlogPost.Id"/>).</summary>
        Task EditPostAsync(string blogId, BlogPost post, bool publish);

        /// <summary>
        /// Uploads a binary media object (image/attachment) to the blog and returns the
        /// hosted URL the server assigns. Mirrors <c>metaWeblog.newMediaObject</c>: the
        /// <paramref name="bits"/> are the raw file bytes (base64-encoded on the wire).
        /// Used by the publish path to host inline (data-URI) images before the post is
        /// sent so the body references real URLs rather than embedded base64.
        /// </summary>
        Task<string> NewMediaObjectAsync(string blogId, string fileName, string mimeType, byte[] bits);

        /// <summary>
        /// Fetches the categories available on the blog (<c>metaWeblog.getCategories</c>).
        /// The publish UI presents these for selection; the chosen category names are then
        /// included inline in the post struct. Returns an empty list when the provider
        /// exposes no categories.
        /// </summary>
        Task<IReadOnlyList<BlogPostCategory>> GetCategoriesAsync(string blogId);

        /// <summary>
        /// Lists the most recent posts on the blog (<c>metaWeblog.getRecentPosts</c>),
        /// newest first, up to <paramref name="count"/>. The returned structs carry the
        /// full body (description + mt_text_more) like the Windows client relies on, so
        /// opening a listed post needs no second round-trip.
        /// </summary>
        Task<IReadOnlyList<ServerPost>> GetRecentPostsAsync(string blogId, int count);

        /// <summary>Fetches a single post in full (<c>metaWeblog.getPost</c>).</summary>
        Task<ServerPost> GetPostAsync(string postId);

        /// <summary>
        /// Lists the blog's pages (<c>wp.getPages</c>). Pages are returned as
        /// <see cref="ServerPost"/> entries with <see cref="ServerPostInfo.IsPage"/> set;
        /// WordPress page structs carry the same body members as posts.
        /// </summary>
        Task<IReadOnlyList<ServerPost>> GetPagesAsync(string blogId);

        /// <summary>Creates a new page (<c>wp.newPage</c>) and returns the server page id.</summary>
        Task<string> NewPageAsync(string blogId, BlogPost post, bool publish);

        /// <summary>Edits an existing page (<c>wp.editPage</c>, identified by <see cref="BlogPost.Id"/>).</summary>
        Task EditPageAsync(string blogId, BlogPost post, bool publish);
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

        /// <summary>When true, post keywords are sent as the <c>mt_keywords</c> member.</summary>
        bool SupportsKeywords { get; }
    }

    /// <summary>Default options: MetaWeblog with extended entries, inline categories, keywords.</summary>
    public sealed class BlogClientOptions : IBlogClientOptions
    {
        public bool SupportsExtendedEntries { get; set; } = true;

        public bool SupportsCategoriesInline { get; set; } = true;

        public bool SupportsKeywords { get; set; } = true;

        public static BlogClientOptions Default => new BlogClientOptions();
    }
}
