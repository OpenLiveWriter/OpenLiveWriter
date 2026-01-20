// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Abstraction for the current selection in an HTML editor.
    /// This interface works with both MSHTML and WebView2 editors.
    /// </summary>
    public interface IEditorSelection
    {
        /// <summary>
        /// The type of the current selection.
        /// </summary>
        SelectionType SelectionType { get; }

        /// <summary>
        /// Returns true if there is a valid selection.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Returns true if text content is selected (as opposed to a control).
        /// </summary>
        bool HasTextSelection { get; }

        /// <summary>
        /// Returns true if a control element (image, table, etc.) is selected.
        /// </summary>
        bool HasControlSelection { get; }

        /// <summary>
        /// Gets the currently selected element, or null if no element is selected.
        /// </summary>
        ISelectedElement SelectedElement { get; }

        /// <summary>
        /// Gets the selected element as an image, or null if the selection is not an image.
        /// </summary>
        ISelectedImage SelectedImage { get; }

        /// <summary>
        /// Gets the selected text content, or null if no text is selected.
        /// </summary>
        string SelectedText { get; }

        /// <summary>
        /// Gets the HTML content of the selection.
        /// </summary>
        string SelectedHtml { get; }
    }

    /// <summary>
    /// Event args for selection changed events.
    /// </summary>
    public class EditorSelectionChangedEventArgs : EventArgs
    {
        public EditorSelectionChangedEventArgs(IEditorSelection selection)
        {
            Selection = selection;
        }

        public IEditorSelection Selection { get; }
    }
}
