// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Small, pure markup snippets inserted by the Insert-tab "Breaks" commands.
    /// Kept in one place so the exact markup (especially the extended-entry marker
    /// that the publish split recognizes) is testable and stays in sync with the
    /// publish pipeline.
    /// </summary>
    public static class EditorMarkup
    {
        /// <summary>
        /// The extended-entry ("more") break marker. Must match the marker the
        /// publish pipeline splits on so "read more" works after publish. Sourced
        /// from the publishing layer to guarantee they never drift apart.
        /// </summary>
        public static string ExtendedEntryBreakHtml => Publishing.ExtendedEntry.BreakMarker;

        /// <summary>
        /// A clearing line break that pushes following content below any floated
        /// element (e.g. a left/right-aligned image), mirroring the Windows
        /// "Insert Clear Break" command.
        /// </summary>
        public const string ClearBreakHtml = "<br style=\"clear:both;\" />";
    }
}
