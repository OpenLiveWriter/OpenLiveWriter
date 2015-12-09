// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    public interface IHtmlEditorSelection
    {
        /// <summary>
        /// Returns the raw MSHTML Selection object.
        /// </summary>
        IHTMLSelectionObject HTMLSelectionObject
        {
            get;
        }

        /// <summary>
        /// Returns true is the selection is valid.
        /// </summary>
        bool IsValid
        {
            get;
        }

        /// <summary>
        /// Returns true if this selection spans a section of content.
        /// </summary>
        bool HasContiguousSelection
        {
            get;
        }

        /// <summary>
        /// Allows a selection object to setup and tear down any context needed for a selection-based operation.
        /// </summary>
        /// <param name="op"></param>
        void ExecuteSelectionOperation(HtmlEditorSelectionOperation op);

        MarkupRange SelectedMarkupRange { get; }

        IHTMLImgElement SelectedImage { get; }

        IHTMLElement SelectedControl { get; }

        IHTMLTable SelectedTable { get; }
    }

    /// <summary>
    /// Delegate for executing an operation that manipulates content selected by an IHTMLSelectionObject.
    /// </summary>
    public delegate void HtmlEditorSelectionOperation(IHtmlEditorSelection selection);
}
