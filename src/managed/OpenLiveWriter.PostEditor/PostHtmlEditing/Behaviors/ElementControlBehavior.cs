// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    public abstract class ElementControlBehavior : HtmlEditorElementBehavior, IBehaviorControlContainerControl
    {
        private Rectangle elementRect;

        private BehaviorControlCollection controls;
        private int leftPadding;
        private int topPadding;
        private int rightPadding;
        private int bottomPadding;

        /// <summary>
        /// The zero-based bounds of this control
        /// </summary>
        protected Rectangle bounds;

        /// <summary>
        /// The mouse lightweight control.  The lightweight control that the mouse is in.
        /// </summary>
        private BehaviorControl mouseBehaviorControl;

        /// <summary>
        /// The suspend layout state of the lightweight control.
        /// </summary>
        private int suspendLayoutCount = 0;

        public event EventHandler ElementSizeChanged;
        public event EventHandler SelectedChanged;

        public ElementControlBehavior(IHtmlEditorComponentContext editorContext)
            : base(editorContext)
        {
            controls = new BehaviorControlCollection(this);
            controls.ControlAdded += new BehaviorControlCollection.ControlEvent(controls_ControlAdded);
            controls.ControlRemoved += new BehaviorControlCollection.ControlEvent(controls_ControlRemoved);
        }

        protected override void OnElementAttached()
        {
            base.OnElementAttached();
            EditorContext.PreHandleEvent += new HtmlEditDesignerEventHandler(HandlePreHandleEvent);
            EditorContext.KeyDown += new HtmlEventHandler(editorContext_KeyDown);

            SynchronizeElementRectangle();
        }

        protected override void OnSelectedChanged()
        {
            if (SelectedChanged != null)
                SelectedChanged(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    controls.Clear();
                    controls.ControlAdded -= new BehaviorControlCollection.ControlEvent(controls_ControlAdded);
                    controls.ControlRemoved -= new BehaviorControlCollection.ControlEvent(controls_ControlRemoved);

                    Debug.Assert(EditorContext != null);
                    EditorContext.PreHandleEvent -= new HtmlEditDesignerEventHandler(HandlePreHandleEvent);
                    EditorContext.KeyDown -= new HtmlEventHandler(editorContext_KeyDown);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;

        public override void Draw(RECT rcBounds, RECT rcUpdate, int lDrawFlags, IntPtr hdc, IntPtr pvDrawObject)
        {
            using (Graphics g = Graphics.FromHdc(hdc))
            {
                Rectangle rcBoundsRect = new Rectangle(
                    new Point(rcBounds.left, rcBounds.top),
                    new Size(rcBounds.right - rcBounds.left, rcBounds.bottom - rcBounds.top)
                    );

                //reset the paint bounds so that 0,0 to the top-left element corner
                //Note: this allows us to have a consistent coordinate system for paint and mouse events
                bounds = new Rectangle(0 - LeftPadding, 0 - TopPadding, rcBoundsRect.Width, rcBoundsRect.Height);

                //	Create the graphics container for the control paint event maps to the zero-based location
                GraphicsContainer graphicsContainer = g.BeginContainer(rcBoundsRect,
                    bounds,
                    GraphicsUnit.Pixel);

                //	Clip the graphics context to prevent the lightweight control from drawing
                //	outside its client rectangle.
                g.SetClip(bounds);

                //	Set a new default compositing mode and quality.
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;

                //	Raise the Paint event
                Paint(new PaintEventArgs(g, bounds));

                //	End the graphics container.
                g.EndContainer(graphicsContainer);
            }
        }

        private void Paint(PaintEventArgs e)
        {
            OnPaint(e);

            //	Obtain the virtual clip rectangle.
            Rectangle virtualClipRectangle = ClientRectangleToVirtualClientRectangle(e.ClipRectangle);

            //	Enumerate the child lightweight controls and paint controls that need painting.
            //	This enumeration occurs in reverse Z order.  Child controls lower in the Z order
            //	are painted before child controls higher in the Z order.
            foreach (BehaviorControl behaviorControl in controls)
            {
                //	If the control is visible and inside the virtual clip rectangle, paint it.
                if (behaviorControl.Visible && behaviorControl.VirtualBounds.IntersectsWith(virtualClipRectangle))
                {
                    //	Obtain the lightweight control virtual client rectangle.
                    Rectangle BehaviorControlVirtualClientRectangle = behaviorControl.VirtualClientRectangle;

                    //	Create the graphics container for the lightweight control paint event that
                    //	causes anything applied to the lightweight control's virtual client
                    //	rectangle to be mapped to the lightweight control's on-screen bounds.
                    GraphicsContainer graphicsContainer = e.Graphics.BeginContainer(VirtualClientRectangleToClientRectangle(behaviorControl.VirtualBounds),
                        BehaviorControlVirtualClientRectangle,
                        GraphicsUnit.Pixel);

                    //	Clip the graphics context to prevent the lightweight control from drawing
                    //	outside its client rectangle.
                    e.Graphics.SetClip(BehaviorControlVirtualClientRectangle);

                    //	Set a new default compositing mode and quality.
                    e.Graphics.CompositingMode = CompositingMode.SourceOver;
                    e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                    //	Raise the Paint event for the lightweight control.
                    behaviorControl.RaisePaint(new PaintEventArgs(e.Graphics, BehaviorControlVirtualClientRectangle));

                    //	End the graphics container.
                    e.Graphics.EndContainer(graphicsContainer);
                }
            }
        }

        protected virtual void OnPaint(PaintEventArgs e)
        {

        }

        protected virtual void OnKeyDown(HtmlEventArgs e)
        {
            bool shiftKey = (Control.ModifierKeys & Keys.Shift) > 0;
            bool altKey = (Control.ModifierKeys & Keys.Alt) > 0;

            if (!altKey) //alt+nav key has special handling for control selections
            {
                //adjust the selection as appropriate for making the cursor keys work as expected
                switch (e.htmlEvt.keyCode)
                {
                    case (int)Keys.Left:
                        PrepareForKeyboardNavigation(MarkupDirection.Left, shiftKey);
                        break;
                    case (int)Keys.Right:
                        PrepareForKeyboardNavigation(MarkupDirection.Right, shiftKey);
                        break;
                    case (int)Keys.Up:
                        PrepareForKeyboardNavigation(MarkupDirection.Left, shiftKey);
                        break;
                    case (int)Keys.Down:
                        PrepareForKeyboardNavigation(MarkupDirection.Right, shiftKey);
                        break;
                }
            }
        }

        /// <summary>
        /// Prepare the HTML selection to deal with keyboard navigation events.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="selectBlock"></param>
        private void PrepareForKeyboardNavigation(MarkupDirection direction, bool selectBlock)
        {
            if (direction == MarkupDirection.Left && selectBlock)
            {
                //HACK: make shift+Left keep the div selected by putting the selection at the
                //right-side of the div before the key event is handled.
                MarkupPointer caretPointer = EditorContext.MarkupServices.CreateMarkupPointer(HTMLElement, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
                EditorContext.MarkupServices.CreateMarkupRange(caretPointer, caretPointer).ToTextRange().select();
            }
            else
            {
                ElementRange.ToTextRange().select();
            }
        }
        private enum MarkupDirection { Left, Right };

        public virtual bool CaptureAllEvents
        {
            get { return false; }
        }

        /// <summary>
        /// If you backspace over a smart content element, it gets removed from
        /// the document before the behavior gets attached.  Some more events from
        /// MSHTML might slip in that time, but they should be ignored
        /// </summary>
        public bool ShouldProcessEvents(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            try
            {
                return (Attached && (HTMLElement != null && HTMLElement.document != null && ((IHTMLDocument2)HTMLElement.document).body != null) && (IsPointInControls(new Point(pIEventObj.clientX, pIEventObj.clientY)) || CaptureAllEvents));

            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                return false;
            }
        }

        protected virtual int HandlePreHandleEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            if (ShouldProcessEvents(inEvtDispId, pIEventObj))
            {
                if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEMOVE)
                {
                    this.OnMouseMove(CreateMouseEvent(pIEventObj));
                    fireClickOnMouseUp = false;
                }
                else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEDOWN)
                {
                    fireClickOnMouseUp = true;
                    this.OnMouseDown(CreateMouseEvent(pIEventObj));
                }
                else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSELEAVE)
                {
                    fireClickOnMouseUp = false;
                    this.OnMouseLeave(CreateMouseEvent(pIEventObj));
                }
                else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONCLICK)
                {
                    //Note: this never really seems to be fired, so we hack it using the fireClickOnMouseUp flag
                    this.OnClick(EventArgs.Empty);
                    fireClickOnMouseUp = false;
                }
                else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEENTER)
                {
                    fireClickOnMouseUp = false;
                    this.OnMouseEnter(CreateMouseEvent(pIEventObj));
                }
                else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEUP)
                {
                    this.OnMouseUp(CreateMouseEvent(pIEventObj));
                    if (fireClickOnMouseUp)
                    {
                        this.OnClick(EventArgs.Empty);
                        fireClickOnMouseUp = false;
                    }
                }
                else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONMOUSEWHEEL)
                {
                    this.OnMouseWheel(CreateMouseEvent(pIEventObj));
                }
            }
            return HRESULT.S_FALSE;
        }
        private bool fireClickOnMouseUp;

        private MouseEventArgs CreateMouseEvent(IHTMLEventObj pIEventObj)
        {
            Point p = TranslateClientPointToBounds(new Point(pIEventObj.x, pIEventObj.y));
            return new MouseEventArgs(Control.MouseButtons, 0, p.X, p.Y, 1);
        }

        public bool IsPointInControls(Point p)
        {
            try
            {
                p = TranslateClientPointToBounds(p);
                bool b = bounds.IntersectsWith(new Rectangle(p, new Size(2, 1)));
                return b;
            }
            catch (Exception)
            {
                //eat error that occurs occasionally in drag/drop so it doesn't scare users
                //   (System.Runtime.InteropServices.COMException (0x8000FFFF): Catastrophic failure)
                //Debug.Fail("Unexpected error in point translation", e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseEnter(EventArgs e)
        {
            //	If we have a current mouse lightweight control, clear it.  This is a bug.
            if (MouseBehaviorControl != null)
                MouseBehaviorControl = null;

            //	Update the mouse lightweight control.
            UpdateMouseBehaviorControl();
        }

        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseBehaviorControl(new Point(e.X, e.Y));

            //	If we have a mouse lightweight control, raise its MouseDown event.  Otherwise,
            //	call the base class's method to raise the MouseDown event on this control.
            if (MouseBehaviorControl != null)
                MouseBehaviorControl.RaiseMouseDown(TranslateMouseEventArgsForBehaviorControl(e, MouseBehaviorControl));
        }

        /// <summary>
        /// Raises the MouseHover event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseHover(EventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseBehaviorControl();

            //	If we have a mouse lightweight control, raise its MouseHover event.  Otherwise,
            //	call the base class's method to raise the MouseHover event on this control.
            if (MouseBehaviorControl != null)
                MouseBehaviorControl.RaiseMouseHover(EventArgs.Empty);

        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseLeave(EventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseBehaviorControl();

            //	If we have a mouse lightweight control, raise its MouseLeave event, then clear it.
            //	Otherwise, call the base class's method to raise the MouseLeave event on this
            //	control.
            if (MouseBehaviorControl != null)
            {
                MouseBehaviorControl.RaiseMouseLeave(EventArgs.Empty);
                MouseBehaviorControl = null;
            }
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            //	If any mouse button is down, we will not update the mouse lightweight control.
            if (e.Button != MouseButtons.None)
            {
                //	If we have a mouse lightweight control, raise its MouseMove event.  Otherwise,
                //	call the base class's method to raise the MouseMove event on this control.
                if (MouseBehaviorControl != null)
                    MouseBehaviorControl.RaiseMouseMove(TranslateMouseEventArgsForBehaviorControl(e, MouseBehaviorControl));
            }
            else
            {
                //	Update the mouse lightweight control.
                UpdateMouseBehaviorControl(new Point(e.X, e.Y));

                //	If we have a mouse lightweight control, raise its MouseMove event.  Otherwise,
                //	call the base class's method to raise the MouseMove event on this control.
                if (MouseBehaviorControl != null)
                    MouseBehaviorControl.RaiseMouseMove(TranslateMouseEventArgsForBehaviorControl(e, MouseBehaviorControl));
            }
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseBehaviorControl(new Point(e.X, e.Y));

            //	If we have a mouse wheel lightweight control, raise its MouseUp event.  Otherwise,
            //	call the base class's method to raise the MouseUp event on this control.
            if (MouseBehaviorControl != null)
                MouseBehaviorControl.RaiseMouseUp(TranslateMouseEventArgsForBehaviorControl(e, MouseBehaviorControl));
        }

        /// <summary>
        /// Raises the MouseWheel event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            //	Find the lightweight control at the mouse position.
            BehaviorControl mouseWheelLightweightControl = GetMouseWheelBehaviorControlAtClientPoint(new Point(e.X, e.Y));

            //	If we have a mouse wheel lightweight control, raise its MouseWheel event.  Otherwise,
            //	call the base class's method to raise the MouseWheel event on this control.
            if (mouseWheelLightweightControl != null)
                mouseWheelLightweightControl.RaiseMouseWheel(TranslateMouseEventArgsForBehaviorControl(e, mouseWheelLightweightControl));
        }

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">A EventArgs that contains the event data.</param>
        protected virtual void OnClick(EventArgs e)
        {
            //	If we have a mouse lightweight control, raise its Click event.  Otherwise, call
            //	the base class's method to raise the Click event on this control.
            if (MouseBehaviorControl != null)
                MouseBehaviorControl.RaiseClick(e);
        }

        #region IBehaviorControlContainerControl Members

        /// <summary>
        ///	Converts a client rectangle to a virtual client rectangle.
        /// </summary>
        /// <param name="clientRectangle"></param>
        /// <returns></returns>
        private Rectangle ClientRectangleToVirtualClientRectangle(Rectangle clientRectangle)
        {
            return new Rectangle(-LeftPadding, -TopPadding, clientRectangle.Width, clientRectangle.Height);
        }

        /// <summary>
        /// Converts a virtual client rectangle to a client rectangle.
        /// </summary>
        /// <param name="virtualClientRectangle">virtual client rectangle to convert.</param>
        /// <returns>Converted virtual client rectangle.</returns>
        private Rectangle VirtualClientRectangleToClientRectangle(Rectangle virtualClientRectangle)
        {
            return virtualClientRectangle;
        }

        protected virtual void OnLayout()
        {

        }

        public void PerformLayout()
        {
            OnLayout();
            aLayoutBehaviorControls(this);
        }

        /// <summary>
        /// Recursively lays out all lightweight controls in this lightweight control.
        /// </summary>
        /// <param name="BehaviorControlContainerControl"></param>
        private void aLayoutBehaviorControls(IBehaviorControlContainerControl BehaviorControlContainerControl)
        {
            //	If there are lightweight controls to layout, enumerate them.
            if (BehaviorControlContainerControl.Controls != null)
            {
                //	Enumerate the child lightweight controls and layout each one.
                foreach (BehaviorControl BehaviorControl in BehaviorControlContainerControl.Controls)
                {
                    //	Recursively layout all the lightweight controls in the lightweight control,
                    //	then layout the lightweight control.
                    aLayoutBehaviorControls(BehaviorControl);
                    BehaviorControl.PerformLayout();
                }
            }
        }

        public BehaviorControlCollection Controls
        {
            get
            {
                return controls;
            }
        }

        public ElementControlBehavior Parent
        {
            get
            {
                return this;
            }
        }

        public void SuspendLayout()
        {
            Interlocked.Increment(ref suspendLayoutCount);
        }

        public void ResumeLayout()
        {
            Interlocked.Decrement(ref suspendLayoutCount);
        }

        protected bool IsLayoutSuspended
        {
            get
            {
                return Interlocked.CompareExchange(ref suspendLayoutCount, 0, 0) != 0;
            }
        }

        public Rectangle VirtualClientRectangleToParent(Rectangle rectangle)
        {
            return rectangle;
        }

        public Point PointToVirtualClient(Point point)
        {
            return point;
        }

        public Point VirtualClientPointToParent(Point point)
        {
            return point;
        }

        /// <summary>
        /// Converts a client point to a virtual client point based on the scroll position.
        /// </summary>
        /// <param name="clientPoint">Client point.</param>
        /// <returns>Virtual point.</returns>
        private Point ClientPointToVirtualClientPoint(Point clientPoint)
        {
            return clientPoint;
        }

        #endregion

        #region Mouse Handling

        /// <summary>
        /// Gets or sets the mouse lightweight control.  The lightweight control that the mouse is in.
        /// </summary>
        private BehaviorControl MouseBehaviorControl
        {
            get
            {
                return mouseBehaviorControl;
            }
            set
            {
                //	Set the mouse lightweight control.
                mouseBehaviorControl = value;
            }
        }

        /// <summary>
        /// Update the mouse lightweight control.  Detects whether the mouse lightweight control
        /// has changed, and raises the appropriate events if it has.
        /// </summary>
        private void UpdateMouseBehaviorControl()
        {
            //UpdateMouseBehaviorControl(PointToClient(Control.MousePosition));
        }

        /// <summary>
        /// Update the mouse lightweight control.  Detects whether the mouse lightweight control
        /// has changed, and raises the appropriate events if it has.
        /// </summary>
        private void UpdateMouseBehaviorControl(Point point)
        {
            //	Find the lightweight control at the mouse position.
            BehaviorControl BehaviorControl = GetMouseBehaviorControlAtClientPoint(point);

            //	If the mouse lightweight control is changing, make the change.
            if (BehaviorControl != MouseBehaviorControl)
            {
                //	If we have a mouse lightweight control, raise its MouseLeave event.
                //	Otherwise, call the base class's method to raise the MouseLeave event on
                //	this control.
                if (MouseBehaviorControl != null)
                {
                    if (MouseBehaviorControl.ContainerControl != null)
                        MouseBehaviorControl.RaiseMouseLeave(EventArgs.Empty);
                }
                else
                    OnMouseLeave(EventArgs.Empty);

                //	Set the mouse lightweight control.
                MouseBehaviorControl = BehaviorControl;

                //	If we have a mouse lightweight control, raise its MouseEnter event.
                //	Otherwise, call the base class's method to raise the MouseEnter event on
                //	this control.
                if (MouseBehaviorControl != null)
                    MouseBehaviorControl.RaiseMouseEnter(EventArgs.Empty);
                else
                    OnMouseEnter(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the mouse lightweight control that is located at the specified client point.
        /// </summary>
        /// <param name="point">A point that contains the coordinates where you want to look for a
        /// control. Coordinates are expressed relative to the upper-left corner of the control's
        /// client area.</param>
        /// <returns>The BehaviorControl at the specified client point.  If there is no
        /// BehaviorControl at the specified client point, the GetMouseBehaviorControlAtClientPoint
        /// method returns a null reference.</returns>
        private BehaviorControl GetMouseBehaviorControlAtClientPoint(Point point)
        {
            //	Map the client point to a virtual client point.
            point = ClientPointToVirtualClientPoint(point);

            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = Controls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                BehaviorControl BehaviorControl = Controls[index];

                //	If the lightweight control is visible, and the virtual client point is inside
                //	it, translate the virtual client point to be relative to the location of the
                //	lightweight control and recursively ask the lightweight control to return the
                //	lightweight control at the virtual client point.  This will find the innermost
                //	lightweight control, at the top of the Z order, that contains the virtual client
                //	point.
                if (BehaviorControl.Visible && BehaviorControl.VirtualBounds.Contains(point))
                {
                    //	Translate the virtual point to be relative to the location of the lightweight control.
                    point.Offset(BehaviorControl.VirtualLocation.X * -1, BehaviorControl.VirtualLocation.Y * -1);

                    //	Recursively get the mouse lightweight control at the translated point.
                    return BehaviorControl.GetBehaviorControlAtPoint(point);
                }
            }

            //	The client point is not inside any lightweight control.
            return null;
        }

        /// <summary>
        /// Gets the lightweight control that is located at the specified client point and allows
        /// mouse wheel events.
        /// </summary>
        /// <param name="point">A point that contains the coordinates where you want to look for a
        /// control. Coordinates are expressed relative to the upper-left corner of the control's
        /// client area.</param>
        /// <returns>The BehaviorControl at the specified client point that allows mouse wheel
        /// events.  If there is no BehaviorControl at the specified client point that allows
        /// mouse wheel events, the GetBehaviorControlAtClientPoint method returns a null
        /// reference.</returns>
        private BehaviorControl GetMouseWheelBehaviorControlAtClientPoint(Point point)
        {
            //	Map the client point to a virtual client point.
            point = ClientPointToVirtualClientPoint(point);

            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = Controls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                BehaviorControl BehaviorControl = Controls[index];

                //	If the lightweight control is visible, and the virtual client point is inside
                //	it, translate the virtual client point to be relative to the location of the
                //	lightweight control and recursively ask the lightweight control to return the
                //	lightweight control at the virtual client point that supports mouse wheel
                //	events.  This will find the innermost lightweight control, at the top of the
                //	Z order that contains the virtual client point and supports mouse wheel events.
                if (BehaviorControl.Visible && BehaviorControl.VirtualBounds.Contains(point))
                {
                    //	Translate the virtual point to be relative to the location of the lightweight control.
                    point.Offset(BehaviorControl.VirtualLocation.X * -1, BehaviorControl.VirtualLocation.Y * -1);

                    //	Recursively get the lightweight control at the translated point that
                    //	supports mouse wheel events.
                    return BehaviorControl.GetMouseWheelBehaviorControlAtPoint(point);
                }
            }

            //	The client point is not inside any lightweight control that allows mouse wheel
            //	events.
            return null;
        }

        /// <summary>
        /// Translates the specified MouseEventArgs for the specified lightweight control.
        /// </summary>
        /// <param name="e">MouseEventArgs to translate.</param>
        /// <returns>Translated MouseEventArgs.</returns>
        private MouseEventArgs TranslateMouseEventArgsForBehaviorControl(MouseEventArgs e, BehaviorControl BehaviorControl)
        {
            //	Map the client point to a virtual point.
            Point virtualPoint = BehaviorControl.PointToVirtualClient(new Point(e.X, e.Y));

            //	Return a new MouseEventArgs with the translated point.
            return new MouseEventArgs(e.Button, e.Clicks, virtualPoint.X, virtualPoint.Y, e.Delta);
        }
        #endregion

        #region Behavior Logic

        public override void GetPainterInfo(ref _HTML_PAINTER_INFO pInfo)
        {
            CalculatePadding();

            // ensure we paint above everything (including selection handles)
            pInfo.lFlags = (int)(HTML_PAINTER.OPAQUE | HTML_PAINTER.HITTEST);
            pInfo.lZOrder = (int)_HTML_PAINT_ZORDER.HTMLPAINT_ZORDER_WINDOW_TOP;

            // expand to the right to accomodate our widget
            pInfo.rcExpand.top = TopPadding;
            pInfo.rcExpand.bottom = BottomPadding;
            pInfo.rcExpand.left = LeftPadding;
            pInfo.rcExpand.right = RightPadding;
        }

        public override int HitTestPoint(POINT pt, ref bool pbHit, ref int plPartID)
        {
            // The given point's origin includes padding set by rcExpand. We translate this to a point whose origin
            // does not include padding.
            Point testPoint = new Point(pt.x - LeftPadding, pt.y - TopPadding);

            foreach (BehaviorControl behaviorControl in controls)
            {
                Point virtualTestPoint = behaviorControl.PointToVirtualClient(testPoint);
                pbHit |= behaviorControl.HitTestPoint(virtualTestPoint);
            }

            return HRESULT.S_OK;
        }

        public override void OnResize(SIZE size)
        {
            if (Attached)
                SynchronizeElementRectangle();
        }

        public Point TransformLocalToGlobal(Point p)
        {
            POINT pntLocal = new POINT();
            pntLocal.x = p.X;
            pntLocal.y = p.Y;
            POINT pntGlobal = new POINT();
            this.HTMLPaintSite.TransformLocalToGlobal(pntLocal, ref pntGlobal);
            return new Point(pntGlobal.x, pntGlobal.y);
        }

        public Point TransformGlobalToLocal(Point p)
        {
            POINT pntGlobal = new POINT();
            pntGlobal.x = p.X;
            pntGlobal.y = p.Y;
            POINT pntLocal = new POINT();
            this.HTMLPaintSite.TransformGlobalToLocal(pntGlobal, ref pntLocal);
            return new Point(pntLocal.x, pntLocal.y);
        }

        private void SynchronizeElementRectangle()
        {
            IHTMLElement2 el = ((IHTMLElement2)HTMLElement);
            SetElementRectangle(new Rectangle(
                new Point(0, 0),
                new Size(el.clientWidth, el.clientHeight)
            ));
        }

        private void SetElementRectangle(Rectangle rect)
        {
            if (!rect.Equals(elementRect))
            {
                elementRect = rect;
                if (ElementSizeChanged != null)
                    ElementSizeChanged(this, EventArgs.Empty);
            }
        }

        protected Point TranslateClientPointToBounds(Point p)
        {
            // calculate mouse position in local coordinates
            POINT clientMouseLocation = new POINT();
            clientMouseLocation.x = p.X;
            clientMouseLocation.y = p.Y;
            POINT localMouseLocation = new POINT();
            HTMLPaintSite.TransformGlobalToLocal(clientMouseLocation, ref localMouseLocation);

            Point pBounds = new Point(localMouseLocation.x - LeftPadding, localMouseLocation.y - TopPadding);
            return pBounds;
        }

        #endregion Behavior Logic

        public int LeftPadding
        {
            get { return leftPadding; }
        }

        public int TopPadding
        {
            get { return topPadding; }
        }

        public int RightPadding
        {
            get { return rightPadding; }
        }

        public int BottomPadding
        {
            get { return bottomPadding; }
        }

        public Rectangle ElementRectangle
        {
            get { return elementRect; }
        }

        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
        }

        protected void SetPadding(int padding)
        {
            SetPadding(padding, padding, padding, padding);
        }

        protected void SetPadding(int top, int right, int bottom, int left)
        {
            if (topPadding == top && rightPadding == right && bottomPadding == bottom && leftPadding == left)
                return;

            topPadding = top;
            rightPadding = right;
            bottomPadding = bottom;
            leftPadding = left;
            SynchronizeElementRectangle();
            HTMLPaintSite.InvalidatePainterInfo();
            Invalidate();
        }

        protected virtual void CalculatePadding()
        {
            int lPadding = 0;
            int tPadding = 0;
            int rPadding = 0;
            int bPadding = 0;
            Rectangle rect = ElementRectangle;
            if (rect != Rectangle.Empty)
            {
                foreach (BehaviorControl control in Controls)
                {
                    if (control.Visible)
                    {
                        lPadding = Math.Max(lPadding, -(control.VirtualBounds.Left));
                        tPadding = Math.Max(tPadding, -(control.VirtualBounds.Top));

                        //rPadding = Math.Max(rPadding, control.VirtualBounds.Left + control.VirtualBounds.Width  - rect.Right);
                        //bPadding = Math.Max(bPadding, control.VirtualBounds.Top + control.VirtualBounds.Height  - rect.Bottom);
                        rPadding = Math.Max(rPadding, control.VirtualBounds.Right - rect.Right);
                        bPadding = Math.Max(bPadding, control.VirtualBounds.Bottom - rect.Bottom);
                    }
                }

                SetPadding(tPadding, rPadding, bPadding, lPadding);
            }
        }

        public void SetCursor(Cursor cursor)
        {
            if (cursor != null)
            {
                EditorContext.OverrideCursor = true;
                Cursor.Current = cursor;
            }
            else
            {
                EditorContext.OverrideCursor = false;
            }
        }

        private void controls_ControlAdded(BehaviorControl c)
        {
            c.VirtualLocationChanged += new EventHandler(c_VirtualLocationChanged);
            c.VirtualSizeChanged += new EventHandler(c_VirtualSizeChanged);
            c.VisibleChanged += new EventHandler(c_VisibleChanged);
        }

        private void controls_ControlRemoved(BehaviorControl c)
        {
            c.VirtualLocationChanged -= new EventHandler(c_VirtualLocationChanged);
            c.VirtualSizeChanged -= new EventHandler(c_VirtualSizeChanged);
            c.VisibleChanged -= new EventHandler(c_VisibleChanged);
        }

        private void c_VirtualLocationChanged(object sender, EventArgs e)
        {
            InvalidateControlPadding();
        }

        private void c_VirtualSizeChanged(object sender, EventArgs e)
        {
            InvalidateControlPadding();
        }

        private void c_VisibleChanged(object sender, EventArgs e)
        {
            InvalidateControlPadding();
        }

        private void InvalidateControlPadding()
        {
            if (Attached)
            {
                //Warning: force a full invalidation of the current paint area before invalidating
                //the painter info so that we don't accidentally shrink the paintable region before
                //we've had a chance clear everything that was previously painted.
                Invalidate();
                HTMLPaintSite.InvalidatePainterInfo();
            }
        }

        private void editorContext_KeyDown(object o, HtmlEventArgs e)
        {
            if (Selected)
                OnKeyDown(e);
        }
    }
}
