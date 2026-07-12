// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Ribbon.Managed;

namespace OpenLiveWriter.App.Avalonia.Editor
{
    /// <summary>
    /// Maps the editor's current selection context (reported by <c>getState()</c> and
    /// parsed into a <see cref="FormatState"/>) to the ribbon contextual-tab group that
    /// should be active. This is the pure, deterministic mapping that drives
    /// contextual-tab activation, so it is unit-testable without a live WebView or a
    /// rendered ribbon.
    ///
    /// A selected rich element (image/video/map/tag anchor) takes precedence over a
    /// plain table-cell caret, matching the Windows behavior where selecting an image
    /// inside a table shows Picture Tools rather than Table Tools.
    /// </summary>
    public static class ContextualTabResolver
    {
        /// <summary>
        /// Resolves the contextual tab group for the given format state. Returns
        /// <see cref="RibbonContextualTabGroup.None"/> when the caret is in ordinary
        /// body text (no contextual tab should be shown).
        /// </summary>
        public static RibbonContextualTabGroup Resolve(FormatState state)
        {
            if (state == null)
                return RibbonContextualTabGroup.None;

            switch (state.SelectedElementType)
            {
                case "image": return RibbonContextualTabGroup.ImageTools;
                case "video": return RibbonContextualTabGroup.VideoTools;
                case "map": return RibbonContextualTabGroup.MapTools;
                case "tag": return RibbonContextualTabGroup.TagTools;
            }

            if (state.InTable)
                return RibbonContextualTabGroup.TableTools;

            return RibbonContextualTabGroup.None;
        }
    }
}
