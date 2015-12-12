// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Provides read-only access to a post.
    /// </summary>
    public interface IPostInfo
    {
        /// <summary>
        /// Gets the ID of the post, as assigned by the server. For
        /// new posts that have never been successfully posted to a
        /// server, the value will be null.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets whether the post is a page. Pages generally do not
        /// appear in reverse-chronological lists and tend to be less
        /// time-sensitive.
        /// </summary>
        bool IsPage { get; }

        /// <summary>
        /// Gets the permanent URL for this post. May be empty or null
        /// if the post has never been successfully published.
        /// </summary>
        string Permalink { get; }

        /// <summary>
        /// Gets the title of the post, in plain-text format.
        /// </summary>
        /// <remarks>
        /// Needs to be HTML-encoded before embedding in HTML, or
        /// URL-encoded before using as part of a URL.
        /// </remarks>
        string Title { get; }

        /// <summary>
        /// Gets the contents of the post in HTML format.
        /// </summary>
        string Contents { get; }

        /// <summary>
        /// Gets the keywords that were set by the author. Delimiting
        /// behavior is determined by the server (comma-delimited is
        /// the most common convention).
        /// </summary>
        string Keywords { get; }

        /// <summary>
        /// Gets the categories that were set by the author. This list
        /// may include categories that already exist on the server as
        /// well as categories that are newly created and exist only
        /// on the client.
        /// </summary>
        ICategoryInfo[] Categories { get; }

        // DateTime? PublishDate { get; }
        // Comment policy? Trackback policy?
    }

    /// <summary>
    /// Provides read-only information about a category.
    /// </summary>
    public interface ICategoryInfo
    {
        /// <summary>
        /// The ID of the category.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The name of the category.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True if the category is newly created and does not exist
        /// on the server yet.
        /// </summary>
        bool IsNew { get; }
    }
}
