// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Undo
{
    internal class SelectionOleUndoUnit : OleUndoUnit
    {
        public SelectionOleUndoUnit(MshtmlMarkupServices markupServices, MarkupRange selection)
            : base(markupServices, selection)
        {
        }

        protected override void HandleUndo()
        {
            SelectRange();
        }

        protected override void HandleRedo()
        {
            // Doesn't support redo.
        }

        protected void SelectRange()
        {
            MarkupRange range = GetMarkupRange();
            if (range != null && range.Positioned)
                range.ToTextRange().select();
        }
    }

    internal class SelectionOleRedoUnit : SelectionOleUndoUnit
    {
        public SelectionOleRedoUnit(MshtmlMarkupServices markupServices, MarkupRange selection)
            : base(markupServices, selection)
        {
        }

        protected override void HandleUndo()
        {
            // Doesn't support undo.
        }

        protected override void HandleRedo()
        {
            SelectRange();
        }
    }
}
