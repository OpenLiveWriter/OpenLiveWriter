// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    public class ResizableElementBehavior : ElementControlBehavior
    {
        private BehaviorControl _dragBufferControl;
        private ResizerControl _resizerControl;
        BehaviorDragAndDropSource _dragDropController;
        public ResizableElementBehavior(IHtmlEditorComponentContext editorContext)
            : base(editorContext)
        {
            _dragBufferControl = new BehaviorControl();
            _dragBufferControl.Visible = false;

            _resizerControl = new ResizerControl();
            _resizerControl.SizerModeChanged += new SizerModeEventHandler(resizerControl_SizerModeChanged);
            _resizerControl.Resized += new EventHandler(resizerControl_Resized);
            _resizerControl.Visible = false;

            Controls.Add(_dragBufferControl);
            Controls.Add(_resizerControl);

            _dragDropController = new SmartContentDragAndDropSource(editorContext);

        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();
            (HTMLElement as IHTMLElement3).contentEditable = "false";

            SynchronizeResizerWithElement();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    _resizerControl.SizerModeChanged += new SizerModeEventHandler(resizerControl_SizerModeChanged);
                    _resizerControl.Resized -= new EventHandler(resizerControl_Resized);
                    _dragDropController.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();
            if (Selected)
            {
                _resizerControl.Visible = true;
                SynchronizeResizerWithElement();
            }
            else
                _resizerControl.Visible = false;
            PerformLayout();
            Invalidate();
        }

        protected virtual void Select()
        {
            SmartContentSelection.SelectIfSmartContentElement(EditorContext, HTMLElement);

            // The element is now selected, move it to the top of the list of events to get messages
            EditorContext.PreHandleEvent -= HandlePreHandleEvent;
            EditorContext.PreHandleEvent += HandlePreHandleEvent;
        }

        protected virtual void Deselect()
        {
            MarkupRange range = EditorContext.MarkupServices.CreateMarkupRange(HTMLElement);
            range.Start.MoveAdjacentToElement(HTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeBegin);
            range.Collapse(true);
            range.ToTextRange().select();
        }

        protected override bool QueryElementSelected()
        {
            SmartContentSelection selection = EditorContext.Selection as SmartContentSelection;
            if (selection != null)
            {
                return selection.HTMLElement.sourceIndex == HTMLElement.sourceIndex;
            }
            else
            {
                return false;
            }
        }

        protected override void OnLayout()
        {
            base.OnLayout();
            Rectangle elementRect = ElementRectangle;
            _resizerControl.VirtualLocation = new Point(elementRect.X - ResizerControl.SIZERS_PADDING, elementRect.Y - ResizerControl.SIZERS_PADDING);
        }

        public override void Draw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            base.Draw(rcBounds, rcUpdate, lDrawFlags, hdc, pvDrawObject);

            // JJA: Made this change because I noticed that unqualified calling of UpdateCursor
            // during painting would mess with other components trying to manipulate the cursor
            // for sizing (e.g. resizable smart content, tables). This basically caused the cursor
            // to "flash" back to the default cursor constantly during sizing. To repro, just
            // take out the if statement below (call UpdateCursor always) and note that if you have
            // two SmartContent objects on the page then the cursor flashes when sizing.
            // I would have removed this call entirely b/c it seems string that UpdateCursor
            // needs to be call from a paint event but I didn't want to disrupt whatever original
            // purpose this had. If we believe this call is not necessary we should definitely
            // remove it so it doesn't cause any more mischief!
            if (_resizerControl != null && _resizerControl.ActiveSizerHandle != SizerHandle.None)
            {
                UpdateCursor(Selected);
            }
        }

        protected override int HandlePreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (ShouldProcessEvents(inEvtDispId, pIEventObj))
            {
                if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN)
                {
                    leftMouseDown = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;

                    if (!Selected && HTMLElementHelper.IsChildOrSameElement(HTMLElement, pIEventObj.srcElement))
                    {
                        return HandlePreHandleEventLeftMouseButtonDown(inEvtDispId, pIEventObj);
                    }

                    rightMouseDown = (Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right;
                    if (rightMouseDown)
                    {
                        //cancel the event so that the editor doesn't try to do a right drag drop
                        //we'll handle showing the context menu on our own.
                        return HRESULT.S_OK;
                    }
                }

                int controlResult = base.HandlePreHandleEvent(inEvtDispId, pIEventObj);

                UpdateCursor(Selected, inEvtDispId, pIEventObj);

                if (_resizerControl.Mode == SizerMode.Resizing)
                {
                    //if the control is resizing, kill all events so that the editor doesn't
                    //try to do a drag and drop.
                    return HRESULT.S_OK;
                }
                else
                {
                    if (_dragDropController.PreHandleEvent(inEvtDispId, pIEventObj) == HRESULT.S_OK)
                    {
                        return HRESULT.S_OK;
                    }

                    //eat the mouse events so that the editor doesn't try to
                    //do anything funny (like opening a browser URL).
                    //Note: Allow non-left clicks through for right-click context menus
                    switch (inEvtDispId)
                    {
                        case DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP:
                            if (rightMouseDown & (Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right)
                            {
                                rightMouseDown = false;
                                Point p = EditorContext.PointToScreen(new Point(pIEventObj.clientX, pIEventObj.clientY));
                                EditorContext.ShowContextMenu(p);
                                return HRESULT.S_OK;
                            }
                            return leftMouseDown ? HRESULT.S_OK : HRESULT.S_FALSE;
                    }
                }
                return controlResult;
            }
            return HRESULT.S_FALSE;
        }

        protected bool leftMouseDown;
        protected bool rightMouseDown;

        protected int HandlePreHandleEventLeftMouseButtonDown(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            // Look to see if we were previously selected on the object
            bool isSelected = QueryElementSelected();

            // The user has clicked in the object somewhere, so select it
            Select();

            // Look to see if we are in a resize knob
            Point p = TranslateClientPointToBounds(new Point(pIEventObj.clientX, pIEventObj.clientY));
            p = _resizerControl.PointToVirtualClient(p);
            bool closeToHandle = _resizerControl.GetHandleForPoint(p) != SizerHandle.None;

            //notify the drag drop controller about the mouse down so that drag can be initiated
            //on the first click that selects this element. (Fixes bug that required 2 clicks to
            //initial drag/drop).
            if (!isSelected && !closeToHandle)
            {
                _dragDropController.PreHandleEvent(inEvtDispId, pIEventObj);
                //cancel the event so that the editor doesn't try to do anything funny (like placing a caret at the click)
                return HRESULT.S_OK;
            }

            // The user clicked a knob, let the resize handles know.  This prevents bugs where
            // it takes 2 clicks to catch a knob to resize
            _resizerControl.RaiseMouseDown(new MouseEventArgs(MouseButtons.Left, 1, p.X, p.Y, 0));
            return HRESULT.S_OK;
        }

        public override bool CaptureAllEvents
        {
            get
            {
                //capture all events while resizing so that mouse movements outside the element's
                //bounds can still affect the element's size.
                return _resizerControl.Mode == SizerMode.Resizing;
            }
        }

        public override void OnResize(SIZE size)
        {
            base.OnResize(size);
            if (Attached && _resizerControl.Mode != SizerMode.Resizing)
                SynchronizeResizerWithElement();
        }

        protected bool PreserveAspectRatio
        {
            get
            {
                return !_resizerControl.AllowAspectRatioDistortion;
            }
            set
            {
                _resizerControl.AllowAspectRatioDistortion = !value;
            }
        }

        protected bool Resizable
        {
            get { return _resizerControl.Resizable; }
            set { _resizerControl.Resizable = value; }
        }

        private void resizerControl_SizerModeChanged(SizerHandle handle, SizerMode mode)
        {
            SuspendLayout();
            if (mode == SizerMode.Normal)
            {
                //trim the padding of the behavior for drawing the sizers
                OnResizeEnd(_resizerControl.SizerSize);
                _dragBufferControl.Visible = false;
                SynchronizeResizerWithElement();
            }
            else
            {
                //notify subclasses that a resize action has been initiated
                bool preserveAspectRatio = handle == SizerHandle.TopLeft ||
                                           handle == SizerHandle.TopRight ||
                                           handle == SizerHandle.BottomLeft ||
                                           handle == SizerHandle.BottomRight;
                OnResizeStart(ElementRectangle.Size, preserveAspectRatio);

                //expand the padding of the behavior region to the whole body so that the resizer
                //can have the freedom to paint anywhere in the document. This is important
                //since the resizer handles may be placed well outside of the element bounds depending
                //on which sizer handle was grabbed and how the element is anchored.
                Rectangle rect = CalculateElementRectangleRelativeToBody(HTMLElement);
                IHTMLElement body = (HTMLElement.document as IHTMLDocument2).body;

                _dragBufferControl.VirtualSize = new Size(body.offsetWidth, body.offsetHeight);
                _dragBufferControl.VirtualLocation = new Point(-rect.X, -rect.Y);
                _dragBufferControl.Visible = true;
            }
            ResumeLayout();
            PerformLayout();
            Invalidate();
        }

        protected virtual void OnResizeStart(Size currentSize, bool preserveAspectRatio)
        {
        }

        protected void UpdateResizerAspectRatioOffset(Size size)
        {
            _resizerControl.AspectRatioOffset = size;
        }

        protected virtual void OnResizing(Size currentSize)
        {

        }

        protected virtual void OnResizeEnd(Size newSize)
        {
        }

        private void resizerControl_Resized(object sender, EventArgs e)
        {
            if (_resizerControl.Mode == SizerMode.Resizing)
            {
                OnResizing(_resizerControl.SizerSize);
            }
        }

        protected virtual void UpdateCursor(bool selected, int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            UpdateCursor(selected);
        }

        protected void UpdateCursor(bool selected)
        {
            if (!selected)
            {
                EditorContext.OverrideCursor = true;
                Cursor.Current = Cursors.Arrow;
                return;
            }

            switch (_resizerControl.ActiveSizerHandle)
            {
                case SizerHandle.TopLeft:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeNWSE;
                    break;
                case SizerHandle.TopRight:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeNESW;
                    break;
                case SizerHandle.BottomLeft:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeNESW;
                    break;
                case SizerHandle.BottomRight:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeNWSE;
                    break;
                case SizerHandle.Top:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeNS;
                    break;
                case SizerHandle.Right:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeWE;
                    break;
                case SizerHandle.Bottom:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeNS;
                    break;
                case SizerHandle.Left:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.SizeWE;
                    break;
                default:
                    EditorContext.OverrideCursor = true;
                    Cursor.Current = Cursors.Arrow;
                    break;
            }
        }

        private void SynchronizeResizerWithElement()
        {
            if (Attached && _resizerControl.Mode != SizerMode.Resizing)
            {
                Rectangle rect = ElementRectangle;
                _resizerControl.VirtualLocation = new Point(
                    rect.X - ResizerControl.SIZERS_PADDING,
                    rect.Y - ResizerControl.SIZERS_PADDING);
                _resizerControl.VirtualSize = new Size(rect.Width + ResizerControl.SIZERS_PADDING * 2, rect.Height + ResizerControl.SIZERS_PADDING * 2);
            }
        }

        private Point CalculateElementLocationRelativeToBody(IHTMLElement element)
        {
            int offsetTop = 0;
            int offsetLeft = 0;
            while (element != null)
            {
                offsetTop += element.offsetTop;
                offsetLeft += element.offsetLeft;
                element = element.offsetParent;
            }
            return new Point(offsetLeft, offsetTop);
        }

        private Rectangle CalculateElementRectangleRelativeToBody(IHTMLElement element)
        {
            return new Rectangle(
                CalculateElementLocationRelativeToBody(element),
                new Size(element.offsetWidth, element.offsetHeight)
                );
        }
    }
}
