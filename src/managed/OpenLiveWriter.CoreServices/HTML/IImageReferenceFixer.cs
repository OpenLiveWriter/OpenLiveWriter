// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.HTML
{
    /// <summary>
    /// Fixes up image URLs.
    /// </summary>
    public interface IImageReferenceFixer
    {
        /// <summary>
        /// Iterates through the provided HTML and fixes up image URLs.
        /// </summary>
        /// <param name="html">The HTML to iterate through.</param>
        /// <param name="sourceUrl">The source URL that the HTML originated from.</param>
        /// <returns>The fixed up HTML.</returns>
        string FixImageReferences(string html, string sourceUrl);
    }
}
