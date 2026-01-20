// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Helper methods for working with editor selections.
    /// Replaces the MSHTML-dependent HTMLSelectionHelper.
    /// </summary>
    public static class SelectionHelper
    {
        /// <summary>
        /// Returns true if the selection contains an image.
        /// </summary>
        public static bool SelectionIsImage(IEditorSelection selection)
        {
            if (selection == null || !selection.IsValid)
                return false;

            return selection.SelectionType == SelectionType.Image;
        }

        /// <summary>
        /// Returns true if the selection contains a table.
        /// </summary>
        public static bool SelectionIsTable(IEditorSelection selection)
        {
            if (selection == null || !selection.IsValid)
                return false;

            return selection.SelectionType == SelectionType.Table;
        }

        /// <summary>
        /// Returns true if the selection contains a control element (image, table, etc.).
        /// </summary>
        public static bool SelectionIsControl(IEditorSelection selection)
        {
            if (selection == null || !selection.IsValid)
                return false;

            return selection.HasControlSelection;
        }

        /// <summary>
        /// Returns true if the selection contains text.
        /// </summary>
        public static bool SelectionIsText(IEditorSelection selection)
        {
            if (selection == null || !selection.IsValid)
                return false;

            return selection.HasTextSelection;
        }

        /// <summary>
        /// Returns true if the selection contains smart content.
        /// </summary>
        public static bool SelectionIsSmartContent(IEditorSelection selection)
        {
            if (selection == null || !selection.IsValid)
                return false;

            return selection.SelectionType == SelectionType.SmartContent;
        }

        /// <summary>
        /// Gets the selected image element, or null if no image is selected.
        /// </summary>
        public static ISelectedImage GetSelectedImage(IEditorSelection selection)
        {
            if (!SelectionIsImage(selection))
                return null;

            return selection.SelectedImage;
        }
    }
}
