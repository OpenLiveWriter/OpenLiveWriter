// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.App.Avalonia.Theming
{
    /// <summary>
    /// The style assets of a blog's theme, harvested from the blog homepage:
    /// the absolute URLs of its <c>&lt;link rel="stylesheet"&gt;</c> stylesheets plus
    /// the contents of its inline <c>&lt;style&gt;</c> blocks. This is the honest
    /// cross-platform slice of the Windows template detection (which locates the exact
    /// post region and reuses the real theme HTML): here the Preview view keeps its
    /// neutral article wrapper and simply layers these stylesheets over it, so the
    /// blog's typography and colors show through without the MSHTML-heavy region
    /// detection. Layout rules scoped to theme-specific container classes therefore
    /// do not apply — a documented limitation versus the Windows Web Layout view.
    /// </summary>
    public sealed class BlogThemeStyle
    {
        /// <summary>Absolute stylesheet URLs, in document order (deduplicated).</summary>
        public IReadOnlyList<string> StylesheetUrls { get; set; } = Array.Empty<string>();

        /// <summary>Raw CSS text of each inline <c>&lt;style&gt;</c> block, in document order.</summary>
        public IReadOnlyList<string> InlineStyles { get; set; } = Array.Empty<string>();

        /// <summary>The homepage URL the styles were harvested from.</summary>
        public string SourceUrl { get; set; } = string.Empty;

        /// <summary>When the theme was fetched (UTC) — recorded by the cache.</summary>
        public DateTime FetchedUtc { get; set; }

        /// <summary>True when no stylesheets and no inline styles were found.</summary>
        public bool IsEmpty => StylesheetUrls.Count == 0 && InlineStyles.Count == 0;
    }
}
