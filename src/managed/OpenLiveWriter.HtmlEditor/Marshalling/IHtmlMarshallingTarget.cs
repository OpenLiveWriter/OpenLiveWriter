// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.Mshtml;
using System.Threading;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public interface IDropFeedback
    {
        bool CanDrop(IHTMLElement scope, DataObjectMeister meister);
        bool ShouldMoveDropLocationRight(MarkupPointer dropLocation);
    }

    /// <summary>
    /// Interface methods required to support marshalling of clipboard data into a HTML Editor control.
    /// </summary>
    public interface IHtmlMarshallingTarget : IDropFeedback, IImageReferenceFixer
    {
        /// <summary>
        /// Control used as the parent for when showing modal dialogs.
        /// </summary>
        IWin32Window FrameWindow { get; }

        IHTMLDocument2 HtmlDocument { get; }
        bool IsEditable { get; }

        MshtmlMarkupServices MarkupServices { get; }
        IHTMLCaretRaw MoveCaretToScreenPoint(Point screenPoint);
        void InsertPlainText(MarkupPointer start, MarkupPointer end, string text);
        void InsertHtml(MarkupPointer start, MarkupPointer end, string html, string sourceUrl);
        IHtmlGenerationService HtmlGenerationService { get; }
        MarkupRange SelectedMarkupRange { get; }
        bool SelectionIsInvalid { get; }

        string EditorId { get; }

        IUndoUnit CreateUndoUnit();
        IUndoUnit CreateInvisibleUndoUnit();

        bool MarshalImagesSupported { get; }
        bool MarshalFilesSupported { get; }
        bool MarshalHtmlSupported { get; }
        bool MarshalTextSupported { get; }
        bool MarshalUrlSupported { get; }

        void Invoke(ThreadStart func);

        bool CleanHtmlOnPaste { get; }
    }
}
