// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Specifies whether a feature is supported by a publishing context.
    /// </summary>
    public enum SupportsFeature
    {
        /// <summary>
        /// Support for the feature is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The feature is supported.
        /// </summary>
        Yes,

        /// <summary>
        /// The feature is not supported.
        /// </summary>
        No
    }

    /// <summary>
    /// Publishing context for HTML generation.
    /// </summary>
    public interface IPublishingContext
    {
        /// <summary>
        /// Unique identifier for account configured for use with Open Live Writer.
        /// </summary>
        string AccountId { get; }

        /// <summary>
        /// Name of current publishing service (e.g. "WordPress.com")
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Value indicating whether image uploading is supported by the current publishing context.
        /// </summary>
        SupportsFeature SupportsImageUpload { get; }

        /// <summary>
        /// Value indicating whether scripts are supported by the current publishing context.
        /// </summary>
        SupportsFeature SupportsScripts { get; }

        /// <summary>
        /// Value indicating whether embeds are supported by the current publishing context.
        /// </summary>
        SupportsFeature SupportsEmbeds { get; }

        /// <summary>
        /// Name of the current publishing account, as entered by the user.
        /// </summary>
        string BlogName { get; }

        /// <summary>
        /// The homepage of the current publishing context.
        /// </summary>
        string HomepageUrl { get; }

        /// <summary>
        /// Gets the detected background color of the publishing
        /// context, or null if none is available.
        /// </summary>
        /// <remarks>
        /// Since the detection is based on heuristics, it
        /// is just a good guess and may not always be correct.
        /// </remarks>
        Color? BodyBackgroundColor { get; }

        /// <summary>
        /// The post that is to be published.
        /// </summary>
        IPostInfo PostInfo { get; }
    }

}

