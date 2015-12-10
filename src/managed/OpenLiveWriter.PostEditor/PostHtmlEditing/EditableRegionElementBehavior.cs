// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal delegate void IHTMLElementCallback(IHTMLElement fromElement);
    internal class EditableRegionElementBehavior : HtmlEditorElementBehavior
    {
        IHTMLElement _nextEditableRegion;
        IHTMLElement _previousEditableRegion;

        #region Initialization and Disposal
        public EditableRegionElementBehavior(IHtmlEditorComponentContext editorContext, IHTMLElement previousEditableRegion, IHTMLElement nextEditableRegion)
            : base(editorContext)
        {
            _nextEditableRegion = nextEditableRegion;
            _previousEditableRegion = previousEditableRegion;
        }

        protected override void OnElementAttached()
        {
            if (EditorContext.EditMode)
            {
                IHTMLElement3 e3 = HTMLElement as IHTMLElement3;
                if (!e3.isContentEditable)
                    e3.contentEditable = "true";
            }

            base.OnElementAttached();

            SetPaintColors(HTMLElement);

            EditorContext.PreHandleEvent += new HtmlEditDesignerEventHandler(EditorContext_PreHandleEvent);
            EditorContext.CommandKey += new KeyEventHandler(EditorContext_CommandKey);
            EditorContext.KeyDown += new HtmlEventHandler(EditorContext_KeyDown);
            EditorContext.KeyUp += new HtmlEventHandler(EditorContext_KeyUp);

            _elementBehaviorAttached = true;
        }

        #endregion

        /// <summary>
        /// Returns true if the element behavior has been completely attached to element.
        /// </summary>
        public bool ElementBehaviorAttached
        {
            get
            {
                return _elementBehaviorAttached;
            }
        }
        private bool _elementBehaviorAttached;

        protected override bool QueryElementSelected()
        {
            return ElementRange.InRange(EditorContext.Selection.SelectedMarkupRange, false);
        }

        protected override void OnSelectedChanged()
        {
            Invalidate();
        }

        public virtual string GetEditedHtml(bool useXhtml, bool doCleanup)
        {
            try
            {
                if (doCleanup || useXhtml)
                {
                    MshtmlMarkupServices markupServices = new MshtmlMarkupServices(HTMLElement.document as IMarkupServicesRaw);
                    if (doCleanup)
                    {
                        MarkupRange bodyRange = markupServices.CreateMarkupRange(((IHTMLDocument2)HTMLElement.document).body, false);
                        bodyRange.RemoveElementsByTagId(_ELEMENT_TAG_ID.TAGID_FONT, true);
                    }

                    if (useXhtml)
                    {
                        MarkupRange bounds = markupServices.CreateMarkupRange(HTMLElement, false);
                        string xhtml = FormattedHtmlPrinter.ToFormattedHtml(markupServices, bounds);
                        return doCleanup ? CleanupHtml(xhtml, true) : xhtml;
                    }
                }
            }
            catch (Exception e)
            {
                // E.g. this failure case: <pre><b></pre></b>
                Trace.Fail("Exception generating XHTML: " + e.ToString());
            }

            string html = HTMLElement.innerHTML ?? String.Empty;
            if (doCleanup)
                return CleanupHtml(html, false);
            else
                return html;
        }

        /// <summary>
        /// Converts tag names, attribute names, and style text to lowercase.
        /// </summary>
        private string CleanupHtml(string html, bool xml)
        {
            bool needsCleanup;
            do
            {
                needsCleanup = false;
                StringBuilder output = new StringBuilder(html.Length);
                SimpleHtmlParser htmlParser = new SimpleHtmlParser(html);
                for (Element el; null != (el = htmlParser.Next());)
                {
                    if (el is BeginTag)
                    {
                        BeginTag bt = (BeginTag)el;

                        if (RemoveMeaninglessTags(htmlParser, bt))
                        {
                            // Since we are removing a tag, we will want to clean up again, since that might mean
                            // there will be another tag to remove
                            needsCleanup = true;
                            continue;
                        }

                        output.Append("<");
                        output.Append(bt.Name.ToLower(CultureInfo.InvariantCulture));
                        foreach (Attr attr in bt.Attributes)
                        {
                            if (attr.NameEquals("contenteditable") || attr.NameEquals("atomicselection") ||
                                attr.NameEquals("unselectable"))
                                continue;

                            output.Append(" ");
                            output.Append(attr.Name.ToLower(CultureInfo.InvariantCulture));
                            if (attr.Value != null)
                            {
                                string attrVal = attr.Value;
                                if (attr.NameEquals("style"))
                                    attrVal = LowerCaseCss(attrVal);
                                else if (attr.Name == attr.Value)
                                    attrVal = attrVal.ToLower(CultureInfo.InvariantCulture);
                                output.AppendFormat("=\"{0}\"",
                                                    xml
                                                        ? HtmlUtils.EscapeEntitiesForXml(attrVal, true)
                                                        : HtmlUtils.EscapeEntities(attrVal));
                            }
                        }
                        if (bt.HasResidue)
                        {
                            if (bt.Attributes.Length == 0)
                                output.Append(" ");
                            output.Append(bt.Residue);
                        }
                        if (bt.Complete)
                            output.Append(" /");
                        output.Append(">");
                    }
                    else if (el is EndTag)
                    {
                        output.AppendFormat("</{0}>", ((EndTag)el).Name.ToLower(CultureInfo.InvariantCulture));
                    }
                    else if (el is Text)
                    {
                        string textHtml = HtmlUtils.TidyNbsps(el.RawText);
                        if (xml)
                            textHtml =
                                HtmlUtils.EscapeEntitiesForXml(
                                    HtmlUtils.UnEscapeEntities(textHtml, HtmlUtils.UnEscapeMode.NonMarkupText), false);
                        output.Append(textHtml);
                    }
                    else if (el is StyleText)
                        output.Append(el.RawText.ToLower(CultureInfo.InvariantCulture));
                    else
                        output.Append(el.RawText);
                }
                html = output.ToString();
            } while (needsCleanup);
            return html;
        }

        /// <summary>
        /// Is the tag a meaningless tag such as <p></p> or <a href="..."></a> or <a href="...">&nbsp;</a>
        /// </summary>
        /// <param name="htmlParser"></param>
        /// <param name="bt"></param>
        /// <returns></returns>
        private static bool RemoveMeaninglessTags(SimpleHtmlParser htmlParser, BeginTag bt)
        {
            // Look to see if the tag is a <p> without any attributes
            if ((bt.NameEquals("p") && bt.Attributes.Length == 0 && !bt.HasResidue))
            {
                Element e = htmlParser.Peek(0);

                // Look to see if thereis a matching end tag to the element we are looking at
                if (e != null && e is EndTag && ((EndTag)e).NameEquals("p"))
                {
                    // eat up the end tag
                    htmlParser.Next();
                    return true;
                }
            }

            // Look to see if the tag is an <a> without a style/id/name attribute, but has an href... meaning the link is not useful
            if ((bt.NameEquals("a") && bt.GetAttribute("name") == null && bt.GetAttributeValue("style") == null && bt.GetAttributeValue("id") == null && bt.GetAttributeValue("href") != null))
            {
                bool hadWhiteSpaceText = false;
                Element e = htmlParser.Peek(0);

                // Look to see if the a just has whitespace inside of it
                if (e is Text && HtmlUtils.UnEscapeEntities(e.RawText, HtmlUtils.UnEscapeMode.NonMarkupText).Trim().Length == 0)
                {
                    e = htmlParser.Peek(1);
                    hadWhiteSpaceText = true;
                }

                // Look to see if thereis a matching end tag to the element we are looking at
                if (e != null && e is EndTag && ((EndTag)e).NameEquals("a"))
                {
                    // if this was an <a> with whitespace in the middle eat it up
                    if (hadWhiteSpaceText)
                        htmlParser.Next();
                    // eat up the end tag
                    htmlParser.Next();

                    return true;
                }
            }

            return false;
        }

        private string LowerCaseCss(string val)
        {
            StringBuilder output = new StringBuilder();
            CssParser parser = new CssParser(val);
            for (StyleElement el; null != (el = parser.Next());)
            {
                if (el is StyleText)
                    output.Append(el.RawText.ToLower(CultureInfo.InvariantCulture));
                else
                    output.Append(el.RawText);
            }
            return output.ToString();
        }

        #region Painting and Drawing
        public override void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo)
        {
            // ensure we paint above everything (including selection handles)
            pInfo.lFlags = (int)_HTML_PAINTER.HTMLPAINTER_OPAQUE;
            pInfo.lZOrder = (int)_HTML_PAINT_ZORDER.HTMLPAINT_ZORDER_WINDOW_TOP;

            // expand to the right to add padding for invalidates
            pInfo.rcExpand.top += invalidationPadding;
            pInfo.rcExpand.bottom += invalidationPadding;
            pInfo.rcExpand.left += invalidationPadding;
            pInfo.rcExpand.right += invalidationPadding;
        }
        protected int invalidationPadding = 3;

        public override void Draw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            drawDepth++;
            try
            {
                if (drawDepth > 1)
                    return;

                _HTML_PAINTER_INFO pInfo = new _HTML_PAINTER_INFO();
                GetPainterInfo(ref pInfo);

                //adjust the draw bounds to remove our padding for invalidates
                Rectangle drawBounds = RectangleHelper.Convert(rcBounds);
                drawBounds.X += invalidationPadding;
                drawBounds.Y += invalidationPadding;
                drawBounds.Height -= invalidationPadding * 2;
                drawBounds.Width -= invalidationPadding * 2;

                using (Graphics g = Graphics.FromHdc(hdc))
                {
                    OnDraw(g, drawBounds, rcBounds, rcUpdate, lDrawFlags, hdc, pvDrawObject);
                }
            }
            finally
            {
                drawDepth--;
            }
        }
        int drawDepth;

        //public virtual void OnDraw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        public virtual void OnDraw(Graphics g, Rectangle drawBounds, RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
        }

        protected Color PaintRegionBorder
        {
            get
            {
                return regionBorder;
            }
            set
            {
                regionBorder = value;
            }
        }
        private Color regionBorder = Color.Transparent;

        /// <summary>
        /// Translates a client rectangle into the rectangle defined by the drawing bounds.
        /// </summary>
        /// <param name="clientRectangle"></param>
        /// <param name="rcBounds"></param>
        /// <returns></returns>
        protected Rectangle GetPaintRectangle(Rectangle clientRectangle, RECT rcBounds)
        {
            POINT globalPoint = new POINT();
            globalPoint.x = clientRectangle.Left;
            globalPoint.y = clientRectangle.Top;
            POINT localPoint = new POINT();
            HTMLPaintSite.TransformGlobalToLocal(globalPoint, ref localPoint);

            Rectangle converted = new Rectangle(localPoint.x + rcBounds.left, localPoint.y + rcBounds.top,
                clientRectangle.Right - clientRectangle.Left, clientRectangle.Bottom - clientRectangle.Top);
            return converted;
        }
        #endregion

        #region Event Handling
        private int EditorContext_PreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (Attached)
            {
                IHTMLElement srcElement = pIEventObj.srcElement;
                if (srcElement != null && srcElement.sourceIndex == HTMLElement.sourceIndex)
                {
                    return OnPreHandleEvent(inEvtDispId, pIEventObj);
                }
            }

            return HRESULT.S_FALSE;
        }

        protected virtual int OnPreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            return HRESULT.S_FALSE;
        }

        private void EditorContext_CommandKey(object sender, KeyEventArgs e)
        {
            if (Selected)
            {
                OnCommandKey(sender, e);
            }
        }

        protected virtual void OnCommandKey(object sender, KeyEventArgs e)
        {
            if (!e.Shift && !e.Alt && !EditorContext.IsEditFieldSelected) //don't override keyboard navigation when shift or alt are pressed
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (IsCaretWithin(GetFirstLineClientRectangle()))
                    {
                        e.Handled = MoveCaretToNextRegion(MOVE_DIRECTION.UP);
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (IsCaretWithin(GetLastLineClientRectangle()))
                    {
                        e.Handled = MoveCaretToNextRegion(MOVE_DIRECTION.DOWN);
                    }
                }
                else if (e.KeyCode == Keys.Left)
                {
                    if (IsCaretWithin(GetFirstLineClientRectangle()) && IsCaretAtLinePosition(LINE_POSITION.START))
                    {
                        e.Handled = MoveCaretToNextRegion(MOVE_DIRECTION.LEFT);
                    }
                }
                else if (e.KeyCode == Keys.Right)
                {
                    if (IsCaretWithin(GetLastLineClientRectangle()) && IsCaretAtLinePosition(LINE_POSITION.END))
                    {
                        e.Handled = MoveCaretToNextRegion(MOVE_DIRECTION.RIGHT);
                    }
                }
            }
        }

        private void EditorContext_KeyDown(object o, HtmlEventArgs e)
        {
            if (Selected)
            {
                OnKeyDown(o, e);
            }
        }
        protected virtual void OnKeyDown(object o, HtmlEventArgs e)
        {

        }

        private void EditorContext_KeyUp(object o, HtmlEventArgs e)
        {
            if (Selected)
            {
                OnKeyUp(o, e);
            }
        }
        protected virtual void OnKeyUp(object o, HtmlEventArgs e)
        {

        }

        public event EventHandler EditableRegionFocusChanged;
        protected virtual void OnEditableRegionFocusChanged(object o, EditableRegionFocusChangedEventArgs e)
        {
            if (EditableRegionFocusChanged != null)
            {
                EditableRegionFocusChanged(null, e);
            }
        }

        #endregion

        /// <summary>
        /// Returns true if there is text to the right of the current position.
        /// </summary>
        /// <returns></returns>
        protected bool hasTextRight(MarkupPointer pointer)
        {
            MarkupPointer start = pointer;
            MarkupPointer end = ElementRange.End;

            IHTMLTxtRange textRange = EditorContext.MarkupServices.CreateTextRange(start, end);
            string text = textRange.text;
            if (text == null || text.Trim().Equals(String.Empty))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Switches focus to the previous editable region.
        /// </summary>
        protected void SelectPreviousRegion()
        {
            if (_previousEditableRegion != null)
            {
                MarkupRange range = EditorContext.MarkupServices.CreateMarkupRange(_previousEditableRegion, false);
                range.End.MoveToPointer(range.Start);
                range.ToTextRange().select();
            }
        }

        /// <summary>
        /// Switches focus to the next editable region.
        /// </summary>
        protected void SelectNextRegion()
        {
            if (_nextEditableRegion != null)
            {
                MarkupRange range = EditorContext.MarkupServices.CreateMarkupRange(_nextEditableRegion, false);
                range.End.MoveToPointer(range.Start);
                range.ToTextRange().select();
            }
            //Note: this could work if we have to use Focus (which causes scroll) but there is a flicker
            //HTMLElement.scrollIntoView(false);
        }

        private IHTMLElement NextEditableRegion
        {
            get { return _nextEditableRegion; }
        }

        private IHTMLElement PreviousEditableRegion
        {
            get { return _previousEditableRegion; }
        }

        protected IHTMLElement2 HTMLElement2
        {
            get
            {
                return (IHTMLElement2)HTMLElement;
            }
        }

        #region Caret Helpers
        /// <summary>
        /// Navigates the editor's caret to the next editable region.
        /// </summary>
        /// <param name="direction"></param>
        private bool MoveCaretToNextRegion(MOVE_DIRECTION direction)
        {
            IHTMLElement nextRegion;
            _ELEMENT_ADJACENCY nextRegionAdjacency;
            bool preserveXLocation;
            if (direction == MOVE_DIRECTION.UP || direction == MOVE_DIRECTION.LEFT)
            {
                nextRegion = PreviousEditableRegion;
                nextRegionAdjacency = _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd;
                preserveXLocation = direction == MOVE_DIRECTION.UP;
            }
            else if (direction == MOVE_DIRECTION.DOWN || direction == MOVE_DIRECTION.RIGHT)
            {
                nextRegion = NextEditableRegion;
                nextRegionAdjacency = _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin;
                preserveXLocation = direction == MOVE_DIRECTION.DOWN;

                if (nextRegion == null)
                    return false;

                MarkupPointer selectRegion = EditorContext.MarkupServices.CreateMarkupPointer(nextRegion, nextRegionAdjacency);
                MarkupContext mc = selectRegion.Right(false);
                if (mc.Context == _MARKUP_CONTEXT_TYPE.CONTEXT_TYPE_EnterScope && mc.Element is IHTMLElement3 && SmartContentSelection.SelectIfSmartContentElement(EditorContext, mc.Element) != null)
                {
                    return true;
                }
            }
            else
                throw new ArgumentException("Unsupported move direction detected: " + direction);

            IDisplayServicesRaw displayServices = (IDisplayServicesRaw)HTMLElement.document;
            IDisplayPointerRaw displayPointer;
            displayServices.CreateDisplayPointer(out displayPointer);
            IHTMLCaretRaw caret = GetCaret();
            caret.MoveDisplayPointerToCaret(displayPointer);

            ILineInfo lineInfo;
            displayPointer.GetLineInfo(out lineInfo);

            if (nextRegion != null)
            {
                MarkupPointer mp = EditorContext.MarkupServices.CreateMarkupPointer(nextRegion, nextRegionAdjacency);

                DisplayServices.TraceMoveToMarkupPointer(displayPointer, mp);
                try
                {
                    caret.MoveCaretToPointer(displayPointer, true, _CARET_DIRECTION.CARET_DIRECTION_SAME);
                    if (preserveXLocation)
                    {
                        POINT caretLocation;
                        caret.GetLocation(out caretLocation, true);
                        caretLocation.x = lineInfo.x;
                        uint hitTestResults;
                        displayPointer.MoveToPoint(caretLocation, _COORD_SYSTEM.COORD_SYSTEM_GLOBAL, nextRegion, 0, out hitTestResults);
                        caret.MoveCaretToPointer(displayPointer, true, _CARET_DIRECTION.CARET_DIRECTION_SAME);
                    }
                    //BEP: using this line causes scrolling	(nextRegion as IHTMLElement2).focus();
                    (nextRegion as IHTMLElement3).setActive();
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Fail("Unexpected exception in MoveCaretToNextRegion: " + e.ToString());
                }

                caret.MoveCaretToPointer(displayPointer, true, _CARET_DIRECTION.CARET_DIRECTION_SAME);
            }
            return false;
        }
        private enum MOVE_DIRECTION { UP, DOWN, LEFT, RIGHT };

        /// <summary>
        /// Returns true if the caret is currently positioned at the specified line position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected bool IsCaretAtLinePosition(LINE_POSITION position)
        {
            _DISPLAY_MOVEUNIT moveUnit;
            if (position == LINE_POSITION.START)
                moveUnit = _DISPLAY_MOVEUNIT.DISPLAY_MOVEUNIT_CurrentLineStart;
            else
                moveUnit = _DISPLAY_MOVEUNIT.DISPLAY_MOVEUNIT_CurrentLineEnd;
            IDisplayServicesRaw displayServices = (IDisplayServicesRaw)HTMLElement.document;
            IDisplayPointerRaw displayPointer, displayPointer2;
            displayServices.CreateDisplayPointer(out displayPointer);
            displayServices.CreateDisplayPointer(out displayPointer2);
            IHTMLCaretRaw caret = GetCaret();
            caret.MoveDisplayPointerToCaret(displayPointer);
            displayPointer2.MoveToPointer(displayPointer);
            displayPointer2.MoveUnit(moveUnit, -1);
            bool areEqual;
            displayPointer2.IsEqualTo(displayPointer, out areEqual);
            return areEqual;
        }
        protected enum LINE_POSITION { START, END };
        #endregion

        #region Rectangle Helpers
        /// <summary>
        /// Returns the bounds of the first line of text for the element in client-based coordinates.
        /// </summary>
        /// <returns></returns>
        protected Rectangle GetFirstLineClientRectangle()
        {
            MarkupPointer pointer = EditorContext.MarkupServices.CreateMarkupPointer(HTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
            return GetLineClientRectangle(pointer);
        }

        /// <summary>
        /// Returns the bounds of the last line of text for the element in client-based coordinates.
        /// </summary>
        /// <returns></returns>
        protected Rectangle GetLastLineClientRectangle()
        {
            MarkupPointer pointer = EditorContext.MarkupServices.CreateMarkupPointer(HTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
            return GetLineClientRectangle(pointer);
        }

        /// <summary>
        /// Returns the bounds of line that the pointer is positioned within in client-based coordinates.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        protected Rectangle GetLineClientRectangle(MarkupPointer pointer)
        {
            //getting the line associated with a pointer is a little complicated because the
            //ILineInfo for the pointer position only returns information based on the font
            //exactly at that position.  It does not take the max font height of the line into
            //account, so we need to that manually.  To do this, we get the LineInfo at each
            //point in the line where the line height may change by moving a markup pointer
            //in to each element declared on the line.

            IDisplayServicesRaw displayServices = (IDisplayServicesRaw)HTMLElement.document;

            //position a display pointer on the same line as the markup pointer
            IDisplayPointerRaw displayPointer;
            displayServices.CreateDisplayPointer(out displayPointer);
            DisplayServices.TraceMoveToMarkupPointer(displayPointer, pointer);

            //position a markup pointer at the end of the line
            MarkupPointer pLineEnd = pointer.Clone();
            displayPointer.MoveUnit(_DISPLAY_MOVEUNIT.DISPLAY_MOVEUNIT_CurrentLineEnd, 0);
            displayPointer.PositionMarkupPointer(pLineEnd.PointerRaw);

            //position a markup pointer at the start of the line
            MarkupPointer pLineStart = pointer.Clone();
            displayPointer.MoveUnit(_DISPLAY_MOVEUNIT.DISPLAY_MOVEUNIT_CurrentLineStart, 0);
            displayPointer.PositionMarkupPointer(pLineStart.PointerRaw);

            //calculate the maximum rectangle taken up by any text on this line by walking
            //the lineStart pointer to the lineEnd pointer and calculating a max rectangle
            //at each step.
            Rectangle lineRect = GetLineRect(HTMLElement, displayPointer);
            pLineStart.Right(true);
            while (pLineStart.IsLeftOfOrEqualTo(pLineEnd))
            {
                Rectangle dpLineRect;
                try
                {
                    displayPointer.MoveToMarkupPointer(pLineStart.PointerRaw, null);
                    dpLineRect = GetLineRect(HTMLElement, displayPointer);
                }
                catch (COMException e)
                {
                    if (e.ErrorCode == IE_CTL_E.INVALIDLINE)
                    {
                        // http://msdn.microsoft.com/en-us/library/aa752674(VS.85).aspx
                        // IDisplayPointer::MoveToMarkupPointer will return an error (CTL_E_INVALIDLINE),
                        // if the markup pointer is in a line whose nearest layout element *is not a flow layout element*.
                        dpLineRect = GetLineRect(pLineStart.CurrentScope);

                        // We also want to skip past the entire current scope...
                        pLineStart.MoveAdjacentToElement(pLineStart.CurrentScope, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);
                    }
                    else
                    {
                        Trace.Fail("Exception thrown in GetLineClientRectangle: " + e.ToString());
                        throw;
                    }
                }

                lineRect.Y = Math.Min(dpLineRect.Y, lineRect.Y);
                if (lineRect.Bottom < dpLineRect.Bottom)
                {
                    lineRect.Height += dpLineRect.Bottom - lineRect.Bottom;
                }

                pLineStart.Right(true);
            }

            //return the line rectangle
            return lineRect;
        }

        // But we need a rectangle based on an element
        private Rectangle GetLineRect(IHTMLElement nonFlowElement)
        {
            Rectangle elementRect = GetClientRectangle();
            return new Rectangle(elementRect.X, elementRect.Y - nonFlowElement.offsetHeight, elementRect.Width, nonFlowElement.offsetHeight);
        }

        /// <summary>
        /// Returns the bounds of the element in client-based coordinatates.
        /// Note: These bounds seem to map to the outer edge of the element's border region.
        /// </summary>
        /// <returns></returns>
        protected Rectangle GetClientRectangle()
        {
            return HTMLElementHelper.GetClientRectangle(HTMLElement);
        }

        protected enum ELEMENT_REGION { CONTENT, PADDING, BORDER, MARGIN };
        protected Rectangle GetClientRectangle(ELEMENT_REGION outerBoundary)
        {
            Rectangle elementBounds = GetClientRectangle();
            try
            {
                if (outerBoundary == ELEMENT_REGION.BORDER)
                    return new Rectangle(elementBounds.Location, elementBounds.Size);

                int marginTop = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginTop, HTMLElement, null, true);
                int marginBottom = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginBottom, HTMLElement, null, true);
                int marginLeft = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginLeft, HTMLElement, null, false);
                int marginRight = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringMarginRight, HTMLElement, null, false);

                int paddingTop = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingTop, HTMLElement, null, true);
                int paddingBottom = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingBottom, HTMLElement, null, true);
                int paddingLeft = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingLeft, HTMLElement, null, false);
                int paddingRight = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringPaddingRight, HTMLElement, null, false);

                int borderTop = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringBorderTop, HTMLElement, null, true);
                int borderBottom = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringBorderBottom, HTMLElement, null, true);
                int borderLeft = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringBorderLeft, HTMLElement, null, false);
                int borderRight = (int)HTMLElementHelper.CSSUnitStringToPointSize(HTMLElementHelper.CSSUnitStringBorderRight, HTMLElement, null, false);

                int offsetX;
                int offsetY;
                int offsetRight;
                int offsetBottom;
                if (outerBoundary == ELEMENT_REGION.PADDING)
                {
                    offsetX = borderLeft;
                    offsetY = borderTop;
                    offsetRight = -borderRight;
                    offsetBottom = -borderBottom;
                }
                else if (outerBoundary == ELEMENT_REGION.CONTENT)
                {
                    offsetX = paddingLeft + borderLeft;
                    offsetY = paddingTop + borderTop;
                    offsetRight = -paddingRight - borderRight;
                    offsetBottom = -paddingBottom - borderBottom;
                }
                else if (outerBoundary == ELEMENT_REGION.MARGIN)
                {
                    offsetX = -marginLeft;
                    offsetY = -marginTop;
                    offsetRight = marginRight;
                    offsetBottom = marginBottom;
                }
                else
                    throw new ArgumentException("Unnsupported ELEMENT_REGION: " + outerBoundary.ToString());

                Rectangle rect = new Rectangle(elementBounds.Location, elementBounds.Size);
                rect.X += offsetX;
                rect.Y += offsetY;
                rect.Width += offsetRight - offsetX;
                rect.Height += offsetBottom - offsetY;
                return rect;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error calculating element paint boundary: " + e.ToString());
                return new Rectangle(elementBounds.Location, elementBounds.Size);
            }
        }

        #endregion

        #region Color Management
        protected virtual void SetPaintColors(IHTMLElement element)
        {
            IHTMLElement2 element2 = (element as IHTMLElement2);
            string textColor = HTMLColorHelper.GetTextColorString(element2);
            _textColorHex = HTMLColorHelper.ParseColorToHex(textColor);
            _textColor = HTMLColorHelper.GetColorFromHexColor(_textColorHex, Color.Black);

            string backgroundColor = HTMLColorHelper.GetBackgroundColorString(element2);
            _backgroundColorHex = HTMLColorHelper.ParseColorToHex(backgroundColor);
            _backgroundColor = HTMLColorHelper.GetColorFromHexColor(_backgroundColorHex, Color.White);
        }
        private Color _backgroundColor;
        private string _backgroundColorHex;
        private Color _textColor;
        private string _textColorHex;

        protected Color BackgroundColor
        {
            get { return _backgroundColor; }
        }

        protected string BackgroundColorHex
        {
            get { return _backgroundColorHex; }
        }

        protected Color TextColor
        {
            get { return _textColor; }
        }

        protected string TextColorHex
        {
            get { return _textColorHex; }
        }

        #endregion

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    Debug.Assert(EditorContext != null);
                    EditorContext.PreHandleEvent -= new HtmlEditDesignerEventHandler(EditorContext_PreHandleEvent);
                    EditorContext.CommandKey -= new KeyEventHandler(EditorContext_CommandKey);
                    EditorContext.KeyDown -= new HtmlEventHandler(EditorContext_KeyDown);
                    EditorContext.KeyUp -= new HtmlEventHandler(EditorContext_KeyUp);
                }

                _elementBehaviorAttached = false;
                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;
    }

    public class EditableRegionFocusChangedEventArgs : EventArgs
    {
        public EditableRegionFocusChangedEventArgs(bool Editable)
        {
            this.IsFullyEditable = Editable;
        }
        public bool IsFullyEditable;
    }
}
