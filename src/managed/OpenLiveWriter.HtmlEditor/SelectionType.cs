// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Represents the type of selection in the HTML editor.
    /// </summary>
    public enum SelectionType
    {
        /// <summary>
        /// No selection.
        /// </summary>
        None,

        /// <summary>
        /// Text selection (caret or range of text).
        /// </summary>
        Text,

        /// <summary>
        /// An image element is selected.
        /// </summary>
        Image,

        /// <summary>
        /// A table element is selected.
        /// </summary>
        Table,

        /// <summary>
        /// A generic control element is selected (not image or table).
        /// </summary>
        Control,

        /// <summary>
        /// A smart content element (plugin content) is selected.
        /// </summary>
        SmartContent
    }
}
