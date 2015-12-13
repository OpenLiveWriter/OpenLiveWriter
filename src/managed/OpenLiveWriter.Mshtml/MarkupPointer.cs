// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// A pointer positioned within an HTML Document object.
    /// This class is a convenience wrapper for the MSHTML IMarkupPointer interface.
    /// </summary>
    public class MarkupPointer
    {
        private readonly IMarkupPointerRaw _pointerRaw;
        private readonly MshtmlMarkupServices MarkupServices;
        private Stack clingStack = new Stack();
        private Stack gravityStack = new Stack();

        internal MarkupPointer(MshtmlMarkupServices markupServices, IMarkupPointerRaw pointer)
        {
            MarkupServices = markupServices;
            _pointerRaw = pointer;
        }

        public IMarkupPointerRaw PointerRaw
        {
            get
            {
                return _pointerRaw;
            }
        }

        /// <summary>
        /// Returns a copy of this markup pointer that is positioned at the same location.
        /// </summary>
        /// <returns></returns>
        public MarkupPointer Clone()
        {
            MarkupPointer p = MarkupServices.CreateMarkupPointer(this);
            p.Cling = Cling;
            p.Gravity = Gravity;
            p.clingStack = (Stack)clingStack.Clone();
            p.gravityStack = (Stack)gravityStack.Clone();
            return p;
        }

        /// <summary>
        /// Enable/Disable the cling attribute for this markup pointer.
        /// </summary>
        public bool Cling
        {
            get
            {
                bool b;
                PointerRaw.Cling(out b);
                return b;
            }
            set
            {
                PointerRaw.SetCling(value);
            }
        }

        /// <summary>
        /// Retrieves the block element that this pointer is positioned within.
        /// </summary>
        public IHTMLElement CurrentBlockScope()
        {
            IHTMLElement parent = CurrentScope;
            while (parent != null && !ElementFilters.IsBlockElement(parent))
            {
                parent = parent.parentElement;
            }
            return parent;
        }

        /// <summary>
        /// Retrieves the IHTMLElement positioned in this pointer.
        /// </summary>
        public IHTMLElement CurrentScope
        {
            get
            {
                IHTMLElement currentScope;
                PointerRaw.CurrentScope(out currentScope);
                return currentScope;
            }
        }

        /// <summary>
        /// Retrieves the container associated with this markup pointer.
        /// </summary>
        public MarkupContainer Container
        {
            get
            {
                IMarkupContainerRaw container;
                PointerRaw.GetContainer(out container);
                return new MarkupContainer(MarkupServices, container);
            }
        }

        /// <summary>
        /// Retrieves the IHTMLDocument that this pointer is positioned within.
        /// </summary>
        public IHTMLDocument2 GetDocument()
        {
            if (Positioned)
            {
                if (CurrentScope != null)
                {
                    return (IHTMLDocument2)CurrentScope.document;
                }
                else
                {
                    return Container.Document;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Returns the closest parent element that matches the specified filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IHTMLElement GetParentElement(IHTMLElementFilter filter)
        {
            IHTMLElement parent = CurrentScope;
            while (parent != null && !filter(parent))
            {
                parent = parent.parentElement;
            }
            return parent;
        }

        /// <summary>
        /// Returns an array of the parent elements that match the specified filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>an array of matching parents (closest parent at index zero)</returns>
        public IHTMLElement[] GetParentElements(IHTMLElementFilter filter)
        {
            ArrayList list = null;
            IHTMLElement parent = CurrentScope;
            while (parent != null)
            {
                if (filter(parent))
                {
                    if (list == null)
                        list = new ArrayList();
                    list.Add(parent);
                }
                parent = parent.parentElement;
            }
            if (list != null)
                return HTMLElementHelper.ToElementArray(list);
            else
                return new IHTMLElement[0];
        }

        /// <summary>
        /// Get/Set the gravity attribute of this pointer.
        /// Pointer gravity determines whether a markup pointer will stay with the markup
        /// to its right or left when markup is inserted at the pointer's location.
        /// By default, markup pointers have "left gravity": that is, they stay with the
        /// markup to their left when text is inserted at the pointer's location.
        /// </summary>
        public _POINTER_GRAVITY Gravity
        {
            get
            {
                _POINTER_GRAVITY gravity;
                PointerRaw.Gravity(out gravity);
                return gravity;
            }
            set
            {
                PointerRaw.SetGravity(value);
            }
        }

        /// <summary>
        /// Checks to see whether this pointer's position is equal to another pointer's position.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsEqualTo(MarkupPointer p)
        {
            bool b;
            PointerRaw.IsEqualTo(p.PointerRaw, out b);
            return b;
        }

        /// <summary>
        /// Checks to see whether this pointer's position is to the left of another pointer's position.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsLeftOf(MarkupPointer p)
        {
            bool b;
            PointerRaw.IsLeftOf(p.PointerRaw, out b);
            return b;
        }

        /// <summary>
        /// Checks to see whether this pointer's position is to the left of or is equal to another pointer's position.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsLeftOfOrEqualTo(MarkupPointer p)
        {
            bool b;
            PointerRaw.IsLeftOfOrEqualTo(p.PointerRaw, out b);
            return b;
        }

        /// <summary>
        /// Checks to see whether this pointer's position is to the right of another pointer's position.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsRightOf(MarkupPointer p)
        {
            bool b;
            PointerRaw.IsRightOf(p.PointerRaw, out b);
            return b;
        }

        /// <summary>
        /// Checks to see whether this pointer's position is to the right of or is equal to another pointer's position.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsRightOfOrEqualTo(MarkupPointer p)
        {
            bool b;
            PointerRaw.IsRightOfOrEqualTo(p.PointerRaw, out b);
            return b;
        }

        /// <summary>
        /// Inspects the content of the container to the left of the markup pointer and optionally moves
        /// the pointer one position to the left.
        /// </summary>
        /// <param name="move">TRUE if the pointer is to move past the content to the left, or FALSE otherwise.
        /// If TRUE, the pointer will move either to the other side of the tag or text to its left, depending on
        /// the CONTEXT_TYPE to the pointer's left.
        /// </param>
        /// <returns>A MarkupContext object describing the content positioned to the pointer's left</returns>
        public MarkupContext Left(bool move)
        {
            MarkupContext context = new MarkupContext();
            Left(move, context);
            return context;
        }

        /// <summary>
        /// Inspects the content of the container to the left of the markup pointer and optionally moves
        /// the pointer one position to the left.
        /// </summary>
        /// <param name="move">TRUE if the pointer is to move past the content to the left, or FALSE otherwise.
        /// If TRUE, the pointer will move either to the other side of the tag or text to its left, depending on
        /// the CONTEXT_TYPE to the pointer's left.
        /// </param>
        /// <param name="context">MarkupContext object to populate with the context information</param>
        public void Left(bool move, MarkupContext context)
        {
            PointerRaw.Left(move, out context.Context, out context.Element, IntPtr.Zero, IntPtr.Zero);
        }

        public IHTMLElement SeekElementLeft(IHTMLElementFilter filter)
        {
            return SeekElementLeft(filter, null);
        }

        public IHTMLElement SeekElementLeft(IHTMLElementFilter filter, MarkupPointer boundaryPointer)
        {
            // initialize markup context used to track seeking
            MarkupContext markupContext = new MarkupContext();
            markupContext.Context = _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope;

            while ((boundaryPointer == null || IsRightOf(boundaryPointer)) &&             // apply boundary if one exists
                    (markupContext.Context != _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None))   // otherwise use begin of document
            {
                Left(true, markupContext);

                IHTMLElement element = markupContext.Element;

                if (element != null && filter(element))
                    return element;
            }

            // none found
            return null;
        }

        public IHTMLElement SeekElementRight(IHTMLElementFilter filter)
        {
            return SeekElementRight(filter, null);
        }

        public IHTMLElement SeekElementRight(IHTMLElementFilter filter, MarkupPointer boundaryPointer)
        {
            // initialize markup context used to track seeking
            MarkupContext markupContext = new MarkupContext();
            markupContext.Context = _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope;

            while ((boundaryPointer == null || IsLeftOf(boundaryPointer)) &&          // apply boundary if one exists
                (markupContext.Context != _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_None))   // otherwise use end of document
            {
                Right(true, markupContext);

                IHTMLElement element = markupContext.Element;

                if (element != null && filter(element))
                    return element;
            }

            // none found
            return null;
        }


        /// <summary>
        /// Moves the pointer adjacent to an element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="eAdj"></param>
        public void MoveAdjacentToElement(IHTMLElement element, _ELEMENT_ADJACENCY eAdj)
        {
            PointerRaw.MoveAdjacentToElement(element, eAdj);
        }

        /// <summary>
        /// Moves the pointer to a markup container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="atStart">bool that specifies whether to position the pointer at the beginning of the container's content.</param>
        public void MoveToContainer(MarkupContainer container, bool atStart)
        {
            PointerRaw.MoveToContainer(container.Container, atStart);
        }

        /// <summary>
        /// Moves this pointer to another pointer's location.
        /// </summary>
        /// <param name="p"></param>
        public void MoveToPointer(MarkupPointer p)
        {
            PointerRaw.MoveToPointer(p.PointerRaw);
        }

        /// <summary>
        /// Moves the pointer one unit.
        /// </summary>
        /// <param name="muAction"></param>
        public void MoveUnit(_MOVEUNIT_ACTION muAction)
        {
            PointerRaw.MoveUnit(muAction);
        }

        /// <summary>
        /// Moves the pointer one unit, but not past a given markup pointer's position.
        /// </summary>
        /// <param name="muAction"></param>
        public void MoveUnitBounded(_MOVEUNIT_ACTION muAction, MarkupPointer boundary)
        {
            (PointerRaw as IMarkupPointer2Raw).MoveUnitBounded(muAction, boundary.PointerRaw);
        }

        /// <summary>
        /// Returns true if this pointer is currently positioned.
        /// </summary>
        /// <returns></returns>
        public bool Positioned
        {
            get
            {
                bool isPositioned;
                PointerRaw.IsPositioned(out isPositioned);
                return isPositioned;
            }
        }

        /// <summary>
        /// Retrieves the top-level document associated with this object.
        /// Note: use the Document property to retrieve the document this pointer is positioned within.
        /// </summary>
        /// <returns></returns>
        public IHTMLDocument2 OwningDoc()
        {
            IHTMLDocument2 doc;
            PointerRaw.OwningDoc(out doc);
            return doc;
        }

        /// <summary>
        /// Return the cling to its value prior to the last clingPush operation.
        /// </summary>
        public void PopCling()
        {
            Cling = (bool)clingStack.Pop();
        }

        /// <summary>
        /// Back-up the current cling setting and set the cling to the new value;
        /// </summary>
        /// <param name="newCling"></param>
        public void PushCling(bool newCling)
        {
            clingStack.Push(Cling);
            Cling = newCling;
        }

        /// <summary>
        /// Return the gravity to its value prior to the last gravityPush operation.
        /// </summary>
        public void PopGravity()
        {
            this.Gravity = (_POINTER_GRAVITY)gravityStack.Pop();
        }

        /// <summary>
        /// Back-up the current gravity setting and set the gravity to the new value;
        /// </summary>
        /// <param name="newGravity"></param>
        public void PushGravity(_POINTER_GRAVITY newGravity)
        {
            gravityStack.Push(this.Gravity);
            this.Gravity = newGravity;
        }

        /// <summary>
        /// Inspects the content of the container to the right of the markup pointer and optionally moves
        /// the pointer one position to the right.
        /// </summary>
        /// <param name="move">TRUE if the pointer is to move past the content to the right, or FALSE otherwise.
        /// If TRUE, the pointer will move either to the other side of the tag or text to its right, depending on
        /// the CONTEXT_TYPE to the pointer's right.
        /// </summary>
        /// <param name="move"></param>
        /// <returns>A MarkupContext object describing the content positioned to the pointer's left</returns>
        public MarkupContext Right(bool move)
        {
            MarkupContext context = new MarkupContext();
            Right(move, context);
            return context;
        }

        /// <summary>
        /// Inspects the content of the container to the right of the markup pointer and optionally moves
        /// the pointer one position to the right.
        /// </summary>
        /// <param name="move">TRUE if the pointer is to move past the content to the right, or FALSE otherwise.
        /// If TRUE, the pointer will move either to the other side of the tag or text to its right, depending on
        /// the CONTEXT_TYPE to the pointer's right.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="context">context object to populate with the context information</param>
        public void Right(bool move, MarkupContext context)
        {
            PointerRaw.Right(move, out context.Context, out context.Element, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Resets the position of the pointer.
        /// </summary>
        public void Unposition()
        {
            PointerRaw.Unposition();
        }

        /// <summary>
        /// Provides a description of what's around this pointer (useful for debugging)
        /// </summary>
        public string PositionDetail
        {
            get
            {
                if (Positioned)
                {
                    StringBuilder detail = new StringBuilder();
                    MarkupContext context = Left(false);
                    AppendContextDetail(context, detail, false);
                    detail.Append("|");
                    context = Right(false);
                    AppendContextDetail(context, detail, true);
                    return detail.ToString();
                }
                else
                {
                    return "not positioned";
                }
            }
        }

        public string PositionTextDetail
        {
            get
            {
                MarkupPointer p = MarkupServices.CreateMarkupPointer();
                p.MoveToPointer(this);
                p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                string leftText = MarkupServices.GetText(p, this);
                p.MoveToPointer(this);
                p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
                string rightText = MarkupServices.GetText(this, p);
                return String.Format(CultureInfo.InvariantCulture, "{0}|{1}", leftText, rightText);
            }
        }

        public int MarkupPosition
        {
            get
            {
                int pos;
                ((IMarkupPointer2Raw)PointerRaw).GetMarkupPosition(out pos);
                return pos;
            }
        }

        public void MoveToMarkupPosition(MarkupContainer container, int markupPosition)
        {
            ((IMarkupPointer2Raw)PointerRaw).MoveToMarkupPosition(container.Container, markupPosition);
        }

        /// <summary>
        /// Appends a representation of a start tag to a string buffer.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sb"></param>
        private void AppendStartTagString(IHTMLElement e, StringBuilder sb)
        {
            sb.Append("<");
            sb.Append(e.tagName);
            if (e.className != null)
            {
                sb.Append(" class=");
                sb.Append(e.className);
            }
            sb.Append(">");
        }

        /// <summary>
        /// Appends a representation of an end tag to a string buffer.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sb"></param>
        private void AppendEndTagString(IHTMLElement e, StringBuilder sb)
        {
            sb.Append("</");
            sb.Append(e.tagName);
            sb.Append(">");
        }

        /// <summary>
        /// Appends a description of a MarkupContext context.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sb"></param>
        private void AppendContextDetail(MarkupContext context, StringBuilder detail, bool isRightContext)
        {
            switch (context.Context)
            {
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope:
                    if (isRightContext)
                        AppendStartTagString(context.Element, detail);
                    else
                        AppendEndTagString(context.Element, detail);
                    break;
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_ExitScope:
                    if (isRightContext)
                        AppendEndTagString(context.Element, detail);
                    else
                        AppendStartTagString(context.Element, detail);
                    break;
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_NoScope:
                    AppendEndTagString(context.Element, detail);
                    break;
                case _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_Text:
                    //not supported for now
                    detail.Append("...text...");
                    break;
            }
        }
    }

    /// <summary>
    /// Describes the HTML content that a MarkupPointer is positioned next to.
    /// </summary>
    public class MarkupContext
    {
        /// <summary>
        /// Enumeration value that describes the content to the next to the markup pointer
        /// </summary>
        public _MARKUP_CONTEXT_TYPE Context;

        /// <summary>
        /// The element, if any, that is coming into scope, is exiting scope, or is a no-scope
        /// element (such as a br element), as specified by pContext
        /// </summary>
        public IHTMLElement Element;

        /// <summary>
        /// The text that is coming into scope or null if there is no text coming into scope.
        /// </summary>
        //public string Text;

        public MarkupContext()
        {
        }
    }
}
