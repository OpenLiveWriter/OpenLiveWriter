// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// The semantic block styles the editor can apply via <c>formatBlock</c>,
    /// mirroring the Windows "HTML styles" / SemanticHtmlGallery: Normal (paragraph),
    /// Heading 1-6, and Preformatted. This is the single source of truth shared by
    /// the toolbar <c>HeadingCombo</c> and the ribbon gallery so both stay in sync,
    /// and it is pure/deterministic so the command → tag mapping is unit-testable
    /// without a live WebView backend.
    /// </summary>
    public static class SemanticHtmlStyles
    {
        /// <summary>
        /// The ordered style list. Index 0 is Normal (paragraph); the remaining
        /// entries are the heading levels followed by preformatted text. The order
        /// matches the toolbar combo item order.
        /// </summary>
        public static readonly IReadOnlyList<(string Label, string Tag)> Styles = new[]
        {
            ("Normal", "p"),
            ("Heading 1", "h1"),
            ("Heading 2", "h2"),
            ("Heading 3", "h3"),
            ("Heading 4", "h4"),
            ("Heading 5", "h5"),
            ("Heading 6", "h6"),
            ("Preformatted", "pre"),
        };

        /// <summary>
        /// Maps a style index (matching <see cref="Styles"/> / the toolbar combo)
        /// to the <c>formatBlock</c> tag. Out-of-range indices fall back to a plain
        /// paragraph.
        /// </summary>
        public static string TagForIndex(int index) =>
            index >= 0 && index < Styles.Count ? Styles[index].Tag : "p";

        /// <summary>
        /// True when the given tag is a block style this editor can apply
        /// (case-insensitive). Guards the ribbon gallery / bridge wiring against
        /// unexpected values.
        /// </summary>
        public static bool IsKnownTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return false;
            foreach (var (_, t) in Styles)
            {
                if (string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
