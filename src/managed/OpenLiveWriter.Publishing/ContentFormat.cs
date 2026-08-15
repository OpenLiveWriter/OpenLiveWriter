// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Publishing
{
    /// <summary>
    /// Content representation for blog editing, publishing, and draft bodies.
    /// </summary>
    public enum ContentFormat
    {
        /// <summary>HTML body content.</summary>
        Html = 0,

        /// <summary>GitHub Flavored Markdown body content.</summary>
        Markdown = 1
    }
}
