// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// IHtmlEditorSelection wrapper for a basic MSHTML Selection object.
    /// </summary>
    internal class HtmlEditorSelection : IHtmlEditorSelection
    {
        IHTMLDocument2 _document;
        MshtmlEditor _editor;
        MshtmlMarkupServices MarkupServices;
        public HtmlEditorSelection(MshtmlEditor editor, IHTMLDocument2 document)
        {
            _document = document;
            _editor = editor;
            MarkupServices = _editor.MshtmlControl.MarkupServices;
        }
        #region IHtmlEditorSelection Members

        public IHTMLSelectionObject HTMLSelectionObject
        {
            get
            {
                return _document.selection;
            }
        }

        public bool IsValid
        {
            get
            {
                return HTMLSelectionObject.type != "None";
            }
        }

        public bool HasContiguousSelection
        {
            get
            {
                return _editor.HasContiguousSelection;
            }
        }

        public void ExecuteSelectionOperation(HtmlEditorSelectionOperation op)
        {
            //this isn't a valid selection to execute an operation on
            if (!IsEditableSelection())
            {
                Debug.Fail("Selection is not positioned, operation will no-op");
                return;
            }
            //adjust the selection to include for any adjacent HTML that is invisibly part of
            //the selection.  This prevent bugs (like 256686) that are related to leaving behind
            //HTML elements that are invisibly part of a selection.
            _tempAdjustedSelection = AdjustSelection();
            try
            {
                //Note: we saved the adjusted selection temporarily because the editor's IHTMLTxtRange.select()
                //call isn't guaranteed to position the selection at exactly the adjusted location.  This was
                //causing issues with operations not actually executing on the adjusted selection, so now these
                //operations will have access to the exact adjusted selection when they use SelectedMarkupRange.

                //execute the selection operation
                op(this);
            }
            finally
            {
                _tempAdjustedSelection = null; //unset the saved adjusted selection
            }
        }

        public MarkupRange SelectedMarkupRange
        {
            get
            {
                if (_tempAdjustedSelection == null)
                {

                    MarkupRange range = MarkupServices.CreateMarkupRange(SelectedTextRange);

                    range.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
                    range.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
                    range.Start.Cling = false;
                    range.End.Cling = false;

                    return range;
                }
                else
                    return _tempAdjustedSelection;
            }
        }
        private MarkupRange _tempAdjustedSelection; //hack for temporarily nailing selection down to an exact adjusted location

        public IHTMLImgElement SelectedImage
        {
            get
            {
                return SelectedControl as IHTMLImgElement;
            }
        }

        public IHTMLElement SelectedControl
        {
            get
            {
                // get selection
                IHTMLSelectionObject selection = HTMLSelectionObject;

                // see if we have selected an image
                if (selection != null && selection.type == "Control")
                {
                    object range = selection.createRange();
                    if (range is IHTMLControlRange)
                    {
                        IHTMLControlRange controlRange = (IHTMLControlRange)range;
                        if (controlRange.length > 0)
                        {
                            IHTMLElement selectedElement = controlRange.item(0);
                            return selectedElement;
                        }
                    }
                }

                // selection was not an image
                return null;
            }
        }

        public IHTMLTable SelectedTable
        {
            get
            {
                // get selection
                IHTMLSelectionObject selection = HTMLSelectionObject;

                // see if we have selected an image
                if (selection != null && selection.type == "Control")
                {
                    object range = selection.createRange();
                    if (range is IHTMLControlRange)
                    {
                        IHTMLControlRange controlRange = (IHTMLControlRange)range;
                        if (controlRange.length > 0)
                        {
                            IHTMLElement selectedElement = controlRange.item(0);
                            if (selectedElement is IHTMLTable)
                                return selectedElement as IHTMLTable;
                        }
                    }
                }

                // selection was not an image
                return null;
            }
        }

        /// <summary>
        /// Currently selected range (null if there is no selection)
        /// </summary>
        private IHTMLTxtRange SelectedTextRange
        {
            get
            {
                if (_document == null)
                {
                    Trace.Fail("Document should not be null!");
                    return null;
                }

                if (_document.body == null)
                {
                    Trace.Fail("Document body should not be null!");
                    return null;
                }

                // get the selection
                IHTMLSelectionObject selection = _document.selection;
                if (selection == null)
                {
                    return null;
                }

                object range = null;
                try
                {
                    // see what type of range is selected
                    range = selection.createRange();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Trace.WriteLine("Exception while trying to read selection: " + ex);

                    // This could be an iframe, in which case we will focus the body and keep going
                    // this might cause a no-op upstream
                    if (selection.type == "None")
                    {
                        ((IHTMLElement2)_document.body).focus();
                        range = selection.createRange();
                    }
                    else
                        throw;
                }

                if (range is IHTMLTxtRange)
                {
                    return range as IHTMLTxtRange;
                }
                else if (range is IHTMLControlRange)
                {
                    // we only support single-selection so a "control-range" can always
                    // be converted into a single-element text range
                    IHTMLControlRange controlRange = range as IHTMLControlRange;
                    if (controlRange.length != 1)
                    {
                        Debug.Fail("Length of control range not equal to 1 (value was " + controlRange.length.ToString(CultureInfo.InvariantCulture));
                        return null;
                    }

                    //bug fix 1793: use markup services to select the range of markup because the
                    //IHTMLTxtRange.moveToElementText() operation doesn't create a reasonable
                    //selection range for <img> selections within an anchor (thumbnails, etc)
                    IHTMLElement selectedElement = controlRange.item(0);
                    if (selectedElement == null)
                    {
                        Trace.Fail("Control range had a length of one but selectedElement is null!");
                        return null;
                    }

                    MarkupRange markupRange = MarkupServices.CreateMarkupRange(selectedElement);

                    IHTMLElement parentElement = selectedElement.parentElement;
                    Trace.Assert(parentElement != null, "Parent element shouldn't equal null! Selected element was: " + selectedElement.tagName);
                    if (parentElement != null && parentElement is IHTMLAnchorElement)
                    {
                        //expand the selection to include the anchor if there is no content between
                        //the selected element and its anchor.
                        markupRange.MoveOutwardIfNoContent();
                    }

                    //return the precisely positioned text range
                    return markupRange.ToTextRange();
                }

                return null;
            }
        }

        #endregion

        private MarkupRange AdjustSelection()
        {
            MarkupRange selection = CreateMaxSafeRange(SelectedMarkupRange);

            if (selection.IsEmpty())
            {
                MarkupRange editableRange = MarkupHelpers.GetEditableRange(selection.Start.CurrentScope, MarkupServices);
                MarkupPointerMoveHelper.MoveUnitBounded(selection.Start,
                                                        MarkupPointerMoveHelper.MoveDirection.LEFT,
                                                        MarkupPointerAdjacency.BeforeVisible | MarkupPointerAdjacency.BeforeEnterScope,
                                                        editableRange.Start);
                selection.Collapse(true);
            }
            selection.ToTextRange().select();
            return selection;
        }

        private bool IsEditableSelection()
        {
            MarkupRange selectedRange = SelectedMarkupRange;
            if (MarkupHelpers.GetEditableRange(selectedRange.Start.CurrentScope, MarkupServices) != null)
            {
                if (selectedRange.IsEmpty() || MarkupHelpers.GetEditableRange(selectedRange.Start.CurrentScope, MarkupServices) != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the maximum range that can be safely considered equivalent to this range (without bringing new text into the range).
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private MarkupRange CreateMaxSafeRange(MarkupRange range)
        {
            MarkupRange maxRange = range.Clone();
            SelectOuter(maxRange);
            return maxRange;
        }

        /// <summary>
        /// Expands a range to the safest outter tags that can be contained without encompassing
        /// text that is not currently in this range.
        /// </summary>
        /// <param name="range"></param>
        private void SelectOuter(MarkupRange range)
        {
            IHTMLElement parent = range.ParentElement();
            MarkupRange editableRange = MarkupHelpers.GetEditableRange(parent, MarkupServices);
            if (editableRange == null)
                return;

            while (parent != null && range.MoveOutwardIfNoContent())
            {
                parent = range.Start.CurrentScope;
            }

            if (range.Start.IsLeftOf(editableRange.Start))
                range.Start.MoveToPointer(editableRange.Start);
            if (range.End.IsRightOf(editableRange.End))
                range.End.MoveToPointer(editableRange.End);
            return;
        }
    }
}
