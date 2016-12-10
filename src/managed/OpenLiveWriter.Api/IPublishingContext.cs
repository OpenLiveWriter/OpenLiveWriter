// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System.Drawing;

    using JetBrains.Annotations;

    /// <summary>
    /// Publishing context for HTML generation.
    /// </summary>
    public interface IPublishingContext
    {
        /// <summary>
        /// Gets the unique identifier for account configured for use with Open Live Writer.
        /// </summary>
        [CanBeNull]
        string AccountId { get; }

        /// <summary>
        /// Gets the name of current publishing service (e.g. "WordPress.com")
        /// </summary>
        [CanBeNull]
        string ServiceName { get; }

        /// <summary>
        /// Gets a value indicating whether image uploading is supported by the current publishing context.
        /// </summary>
        SupportsFeature SupportsImageUpload { get; }

        /// <summary>
        /// Gets a value indicating whether scripts are supported by the current publishing context.
        /// </summary>
        SupportsFeature SupportsScripts { get; }

        /// <summary>
        /// Gets a value indicating whether embeds are supported by the current publishing context.
        /// </summary>
        SupportsFeature SupportsEmbeds { get; }

        /// <summary>
        /// Gets the name of the current publishing account, as entered by the user.
        /// </summary>
        [CanBeNull]
        string BlogName { get; }

        /// <summary>
        /// Gets the homepage of the current publishing context.
        /// </summary>
        [CanBeNull]
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
        /// Gets the post that is to be published.
        /// </summary>
        [CanBeNull]
        IPostInfo PostInfo { get; }
    }

}

