// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{

    public abstract class HtmlEditorElementBehavior : MshtmlElementBehavior
    {
        public HtmlEditorElementBehavior(IHtmlEditorComponentContext editorContext)
        {
            // save reference
            _editorContext = editorContext;
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();

            // create and save a markup range for this element
            _markupRange = _editorContext.MarkupServices.CreateMarkupRange();
            _markupRange.MoveToElement(HTMLElement, true);
            _markupRange.Start.Cling = true;
            _markupRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            _markupRange.End.Cling = true;
            _markupRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;

            // subscribe to selection changed event
            _editorContext.SelectionChanged += new EventHandler(_editorContext_SelectionChanged);

            // force update to selection state
            UpdateSelectionState();
        }

        public bool Selected
        {
            get
            {
                return Attached && _selected;
            }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnSelectedChanged();
                }
            }
        }
        private bool _selected;

        protected abstract bool QueryElementSelected();

        protected abstract void OnSelectedChanged();

        internal protected bool IsInRange(MarkupRange range)
        {
            return range.Start.IsRightOfOrEqualTo(ElementRange.Start) && range.End.IsLeftOfOrEqualTo(ElementRange.End);
        }

        protected bool IsCaretWithin(Rectangle clientRect)
        {
            IHTMLCaretRaw caret = GetCaret();
            POINT p;
            caret.GetLocation(out p, true);

            Point pt = new Point(p.x, p.y);
            return pt.X >= clientRect.X && pt.X <= clientRect.Right && pt.Y >= clientRect.Y && pt.Y <= clientRect.Bottom;
        }

        protected IHTMLCaretRaw GetCaret()
        {
            IHTMLCaretRaw caret;
            IDisplayServicesRaw displayServices = (IDisplayServicesRaw)HTMLElement.document;
            displayServices.GetCaret(out caret);
            return caret;
        }

        /// <summary>
        /// Returns a rectangle based on the given IDisplayPointer
        /// </summary>
        /// <param name="element">The element that contains the display pointer</param>
        /// <param name="displayPointer"></param>
        /// <returns></returns>
        protected Rectangle GetLineRect(IHTMLElement element, IDisplayPointerRaw displayPointer)
        {
            ILineInfo lineInfo;
            displayPointer.GetLineInfo(out lineInfo);

            Rectangle elementRect = HTMLElementHelper.GetClientRectangle(element);

            //determine the rectangle based on the baseline and text height.
            //Note: baseline is relative to the parent element, and the text
            //height includes the textDescent, which is below the baseline
            int lineBottom = elementRect.Y + lineInfo.baseLine + lineInfo.textDescent;
            int lineTop = lineBottom - lineInfo.textHeight;
            int lineLeft = elementRect.X;
            int lineWidth = elementRect.Width;

            //Calculate the height of line.  Since the bottom pixel line is shared by the next line,
            //we subtract 1 from the height. This height exactly matches height of the caret.
            //Note: this height adjustment fixes a bug that can occur with some text styles where a
            //caret placed on the next line is detected as being within this rectangle.
            int lineHeight = lineInfo.textHeight - 1;

            //create a rectangle that exactly fits the caret for this line
            //Note: this can be verified by painting a rectangle over the line using this rectangle
            return new Rectangle(lineLeft, lineTop, lineWidth, lineHeight);
        }

        protected MarkupRange ElementRange
        {
            get
            {
                EnsureRangeIsPositioned();
                return _markupRange;
            }
        }

        protected bool ElementExists
        {
            get
            {
                EnsureRangeIsPositioned();
                return ElementRange.Positioned;
            }
        }

        /// <summary>
        /// Ensures that the element range is positioned around the element if it still exists in the
        /// document.
        /// </summary>
        private void EnsureRangeIsPositioned()
        {
            //there are occasions where the range gets unpositioned even though the HTML Element
            //is still in the document, and the cling is set to true (like maybe when backspacing over
            //the End pointer????).  In these situations, this method will reposition the range pointers
            //so that behaviors don't get hosed.

            //Note: if the source index of the HTMLElement is less than zero, then the HTMLElement is deleted.
            if (Attached && _markupRange != null && !_markupRange.Positioned && HTMLElement != null && HTMLElement.sourceIndex >= 0)
            {
                try
                {
                    _markupRange.MoveToElement(HTMLElement, true);
                    UpdateSelectionState();
                }
                catch (Exception)
                {
                    Debug.Fail("Failed to move to element, is it deleted?");
                }
            }
        }

        protected IHtmlEditorComponentContext EditorContext
        {
            get
            {
                return _editorContext;
            }
        }
        private IHtmlEditorComponentContext _editorContext;

        private void _editorContext_SelectionChanged(object sender, EventArgs e)
        {
            if (!Attached)
                return;

            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            if (ElementExists && EditorContext.Selection != null)
            {
                Selected = QueryElementSelected();
            }
            else
            {
                Selected = false;
            }
        }

        private MarkupRange _markupRange;

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    _editorContext.SelectionChanged -= new EventHandler(_editorContext_SelectionChanged);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;
    }

}
