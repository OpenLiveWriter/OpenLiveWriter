// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// This class is a convenience wrapper for the MSHTML IMarkupServices interface.
    /// </summary>
    public class MshtmlMarkupServices
    {
        private readonly IMarkupServicesRaw MarkupServices;

        public MshtmlMarkupServices(IMarkupServicesRaw markupServices)
        {
            MarkupServices = markupServices;
        }

        /// <summary>
        /// Marks the beginning of a reversible unit of work.
        /// </summary>
        /// <param name="title">the title of a reversible unit of work</param>
        public void BeginUndoUnit(string title)
        {
            MarkupServices.BeginUndoUnit(title);
        }

        /// <summary>
        /// Creates a duplicate of an element.
        /// </summary>
        /// <param name="e">the element to clone</param>
        /// <returns>a clone of the element</returns>
        public IHTMLElement CloneElement(IHTMLElement e)
        {
            IHTMLElement clone;
            MarkupServices.CloneElement(e, out clone);
            return clone;
        }

        /// <summary>
        /// Copies the content between markers to a target location.
        /// </summary>
        /// <param name="start">start point of the text to be copied</param>
        /// <param name="end">end point of the text to be copied</param>
        /// <param name="target">target point of insertion</param>
        public void Copy(MarkupPointer start, MarkupPointer end, MarkupPointer target)
        {
            MarkupServices.Copy(start.PointerRaw, end.PointerRaw, target.PointerRaw);
        }

        /// <summary>
        /// Creates an element with the specified tag.
        /// </summary>
        /// <param name="tagID">specifies the type of tag to create</param>
        /// <param name="attributes">specifies the attributes of the element</param>
        /// <returns>the newly created element</returns>
        public IHTMLElement CreateElement(_ELEMENT_TAG_ID tagID, string attributes)
        {
            IHTMLElement element;
            MarkupServices.CreateElement(tagID, attributes, out element);
            return element;
        }

        /// <summary>
        /// Creates an instance of a MarkupContainer object.
        /// </summary>
        /// <returns>An empty MarkupContainer</returns>
        public MarkupContainer CreateMarkupContainer()
        {
            IMarkupContainerRaw container;
            MarkupServices.CreateMarkupContainer(out container);
            return new MarkupContainer(this, container);
        }

        /// <summary>
        /// Creates an instance of a MarkupPointer object.
        /// </summary>
        /// <returns></returns>
        public MarkupPointer CreateMarkupPointer()
        {
            IMarkupPointerRaw pointer;
            MarkupServices.CreateMarkupPointer(out pointer);
            return new MarkupPointer(this, pointer);
        }

        /// <summary>
        /// Creates an instance of a MarkupPointer object from a pointer raw.
        /// </summary>
        /// <returns></returns>
        public MarkupPointer CreateMarkupPointer(IMarkupPointerRaw rawPtr)
        {
            IMarkupPointerRaw pointer;
            MarkupServices.CreateMarkupPointer(out pointer);
            pointer.MoveToPointer(rawPtr);
            return new MarkupPointer(this, pointer);
        }

        /// <summary>
        /// Creates an instance of the IMarkupPointer object with an initial position
        /// at the same location as another pointer.
        /// </summary>
        /// <param name="initialPosition"></param>
        /// <returns></returns>
        public MarkupPointer CreateMarkupPointer(MarkupPointer initialPosition)
        {
            MarkupPointer pointer = CreateMarkupPointer();
            pointer.MoveToPointer(initialPosition);
            return pointer;
        }

        /// <summary>
        /// Create an unpositioned MarkupRange.
        /// </summary>
        /// <returns></returns>
        public MarkupRange CreateMarkupRange()
        {
            MarkupPointer start = CreateMarkupPointer();
            MarkupPointer end = CreateMarkupPointer();
            end.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            return CreateMarkupRange(start, end);
        }

        /// <summary>
        /// Create a MarkupRange from a selection object.
        /// </summary>
        public MarkupRange CreateMarkupRange(IHTMLSelectionObject selection)
        {
            if (selection == null)
            {
                return null;
            }

            // see what type of range is selected
            object range = selection.createRange();
            if (range is IHTMLTxtRange)
            {
                return CreateMarkupRange(range as IHTMLTxtRange);
            }
            else if (range is IHTMLControlRange)
            {
                // we only support single-selection so a "control-range" can always
                // be converted into a single-element text range
                IHTMLControlRange controlRange = range as IHTMLControlRange;
                if (controlRange.length == 1)
                {
                    IHTMLElement selectedElement = controlRange.item(0);
                    MarkupRange markupRange = CreateMarkupRange(selectedElement);

                    //return the precisely positioned text range
                    return markupRange;
                }
                else
                {
                    Debug.Fail("Length of control range not equal to 1 (value was " + controlRange.length.ToString(CultureInfo.InvariantCulture));
                    return null;
                }
            }
            else // null or unexpected range type
            {
                return null;
            }

        }

        /// <summary>
        /// Create a MarkupRange from that surrounds an Element.
        /// </summary>
        /// <returns></returns>
        public MarkupRange CreateMarkupRange(IHTMLElement element)
        {
            return CreateMarkupRange(element, true);
        }

        /// <summary>
        /// Create a MarkupRange from that surrounds an Element.
        /// </summary>
        /// <returns></returns>
        public MarkupRange CreateMarkupRange(IHTMLElement element, bool outside)
        {
            _ELEMENT_ADJACENCY beginAdj = outside ? _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin : _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin;
            _ELEMENT_ADJACENCY endAdj = outside ? _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd : _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd;
            MarkupPointer Begin = CreateMarkupPointer(element, beginAdj);
            MarkupPointer End = CreateMarkupPointer(element, endAdj);
            End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            MarkupRange markupRange = new MarkupRange(Begin, End, this);
            return markupRange;
        }

        /// <summary>
        /// Create a MarkupRange from a TextRange.
        /// </summary>
        /// <param name="textRange"></param>
        /// <returns></returns>
        public MarkupRange CreateMarkupRange(IHTMLTxtRange textRange)
        {
            MarkupPointer Begin = CreateMarkupPointer();
            MarkupPointer End = CreateMarkupPointer();
            End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            MovePointersToRange(textRange, Begin, End);
            MarkupRange markupRange = new MarkupRange(Begin, End, this);
            return markupRange;
        }

        /// <summary>
        /// Create a MarkupRange from a set of MarkupPointers.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public MarkupRange CreateMarkupRange(MarkupPointer start, MarkupPointer end)
        {
            MarkupRange markupRange = new MarkupRange(start, end, this);
            return markupRange;
        }

        /// <summary>
        /// Create a TextRange that spans a set of pointers.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IHTMLTxtRange CreateTextRange(MarkupPointer start, MarkupPointer end)
        {
            Debug.Assert(start.Positioned && end.Positioned, "pointers are not positioned");
            IHTMLTxtRange range = start.Container.CreateTextRange(start, end);
            return range;
        }

        /// <summary>
        /// Marks the end of a reversible unit of work.
        /// </summary>
        public void EndUndoUnit()
        {
            MarkupServices.EndUndoUnit();
        }

        /// <summary>
        /// Creates an instance of the IMarkupPointer object with an initial position
        /// adjacent to the specified HTML element.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="eAdj"></param>
        /// <returns></returns>
        public MarkupPointer CreateMarkupPointer(IHTMLElement e, _ELEMENT_ADJACENCY eAdj)
        {
            MarkupPointer pointer = CreateMarkupPointer();
            pointer.MoveAdjacentToElement(e, eAdj);
            return pointer;
        }

        /// <summary>
        /// Retrieves an element's tag identifier (ID).
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public _ELEMENT_TAG_ID GetElementTagId(IHTMLElement e)
        {
            _ELEMENT_TAG_ID tagId;
            MarkupServices.GetElementTagId(e, out tagId);
            return tagId;
        }

        /// <summary>
        /// Returns the tag name given the identifier (ID).
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public string GetNameForTagId(_ELEMENT_TAG_ID tagId)
        {
            IntPtr p;
            MarkupServices.GetNameForTagID(tagId, out p);
            return Marshal.PtrToStringBSTR(p);
        }

        /// <summary>
        /// Retrieve the HTML content between two pointers.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string GetHtmlText(MarkupPointer start, MarkupPointer end)
        {
            string html = CreateTextRange(start, end).htmlText;
            return html;
        }

        /// <summary>
        /// Retrieve the text content between two pointers (html markup is stripped).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string GetText(MarkupPointer start, MarkupPointer end)
        {
            string text = CreateTextRange(start, end).text;
            return text;
        }

        /// <summary>
        /// Inserts an element between the target pointers.
        /// </summary>
        /// <param name="element">the element to insert</param>
        /// <param name="start">start point of insertion</param>
        /// <param name="end">end point of insertion</param>
        public void InsertElement(IHTMLElement element, MarkupPointer start, MarkupPointer end)
        {
            MarkupServices.InsertElement(element, start.PointerRaw, end.PointerRaw);
        }

        /// <summary>
        /// Insert text at the given pointer.
        /// </summary>
        /// <param name="text">the text to insert</param>
        /// <param name="start">the point of insertion</param>
        public void InsertText(string text, MarkupPointer insertionPoint)
        {
            MarkupServices.InsertText(text, text.Length, insertionPoint.PointerRaw);
        }

        /// <summary>
        /// Insert html at the given pointer.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="insertionPoint"></param>
        public void InsertHtml(string html, MarkupPointer insertionPoint)
        {
            MarkupRange content = CreateMarkupRange();
            ParseString(html, content.Start, content.End);

            Move(content.Start, content.End, insertionPoint);
        }

        public IMarkupServicesRaw MarkupServicesRaw
        {
            get
            {
                return MarkupServices;
            }
        }

        /// <summary>
        /// Move the content between markers to a target location.
        /// </summary>
        /// <param name="start">start point of the text to be moved</param>
        /// <param name="end">end point of the text to be moved</param>
        /// <param name="target">target point of insertion</param>
        public void Move(MarkupPointer start, MarkupPointer end, MarkupPointer target)
        {
            Trace.Assert(start.Positioned && end.Positioned && target.Positioned, string.Format(CultureInfo.InvariantCulture, "Invalid pointer being used for insert. start:({0}),end:({1}),target:({2})", start.Positioned, end.Positioned, target.Positioned));
            MarkupServices.Move(start.PointerRaw, end.PointerRaw, target.PointerRaw);
        }

        /// <summary>
        /// Positions a DisplayPointer at the specified MarkupPointer.
        /// </summary>
        /// <param name="displayPointer"></param>
        /// <param name="p"></param>
        public void MoveDisplayPointerToMarkupPointer(IDisplayPointerRaw displayPointer, MarkupPointer p)
        {
            DisplayServices.TraceMoveToMarkupPointer(displayPointer, p);
        }

        /// <summary>
        /// Positions a MarkupPointer at the specified caret.
        /// </summary>
        /// <param name="caret"></param>
        /// <param name="p"></param>
        public void MoveMarkupPointerToCaret(IHTMLCaretRaw caret, MarkupPointer p)
        {
            caret.MoveMarkupPointerToCaret(p.PointerRaw);
        }

        /// <summary>
        /// Positions pointers at the edges of an existing range.
        /// </summary>
        /// <param name="range">the text range to move to</param>
        /// <param name="start">the pointer to position at the start of the range</param>
        /// <param name="end">the pointer to position at the end of the range</param>
        public void MovePointersToRange(IHTMLTxtRange range, MarkupPointer start, MarkupPointer end)
        {
            MarkupServices.MovePointersToRange(range, start.PointerRaw, end.PointerRaw);
        }

        /// <summary>
        /// Positions pointers at the edges of an existing range.
        /// </summary>
        /// <param name="start">the pointer positioned at the start of the range</param>
        /// <param name="end">the pointer position at the end of the range</param>
        /// <param name="range">the text range to move</param>
        public void MoveRangeToPointers(MarkupPointer start, MarkupPointer end, IHTMLTxtRange range)
        {
            MarkupServices.MoveRangeToPointers(start.PointerRaw, end.PointerRaw, range);
        }

        /// <summary>
        /// Creates a MarkupContainer that contains the results of parsing the contents of a string.
        /// </summary>
        /// <param name="html">html content to parse</param>
        /// <param name="start">pointer to position at the beginning of the parsed content (null is allowed)</param>
        /// <param name="end">pointer to position at the end of the parsed content (null is allowed)</param>
        /// <returns></returns>
        public MarkupContainer ParseString(string html, MarkupPointer start, MarkupPointer end)
        {
            if (start == null)
                start = CreateMarkupPointer();
            if (end == null)
                end = CreateMarkupPointer();
            IMarkupContainerRaw container;
            MarkupServices.ParseString(html, 0, out container, start.PointerRaw, end.PointerRaw);
            return new MarkupContainer(this, container);
        }

        /// <summary>
        /// Returns a container that contains the results of parsing the contents of a string.
        /// </summary>
        /// <param name="html">the HTML to content parse into a container</param>
        /// <returns></returns>
        public MarkupContainer ParseString(string html)
        {
            MarkupPointer start = CreateMarkupPointer();
            MarkupPointer end = CreateMarkupPointer();
            MarkupContainer container = ParseString(html, start, end);
            return container;
        }

        /// <summary>
        /// Removes content between two pointers.
        /// </summary>
        /// <param name="start">start point of text to remove</param>
        /// <param name="end">end point of text to remove</param>
        public void Remove(MarkupPointer start, MarkupPointer end)
        {
            MarkupServices.Remove(start.PointerRaw, end.PointerRaw);
        }

        /// <summary>
        /// Removes the given element without removing the content contained within it.
        /// </summary>
        /// <param name="e"></param>
        public void RemoveElement(IHTMLElement e)
        {
            MarkupServices.RemoveElement(e);
        }

        /// <summary>
        /// Replaces an element with a new element while preserving the content inside the old element.
        /// </summary>
        /// <param name="oldElement"></param>
        /// <param name="newElement"></param>
        public void ReplaceElement(IHTMLElement oldElement, IHTMLElement newElement)
        {
            MarkupRange range = CreateMarkupRange();
            range.MoveToElement(oldElement, true);
            InsertElement(newElement, range.Start, range.End);
            RemoveElement(oldElement);
        }
    }
}
