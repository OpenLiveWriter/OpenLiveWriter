// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.HtmlEditor;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Summary description for BehaviorControl.
    /// </summary>
    public class BehaviorControl : IBehaviorControlContainerControl, IDisposable
    {
        /// <summary>
        /// The behavior control container for this behavior control.  This will be either
        /// another behavior control or a heavyweight control that is designed to contain
        /// behavior controls through an implementation of the IBehaviorControlContainerControl
        /// interface.
        /// </summary>
        private IBehaviorControlContainerControl containerControl;

        /// <summary>
        /// The the virtual location of the lightweight control relative to the upper-left corner
        /// of its lightweight control container.
        /// </summary>
        private Point virtualLocation;

        /// <summary>
        /// The virtual size of the lightweight control.
        /// </summary>
        private Size virtualSize = new Size(0, 0);

        /// <summary>
        /// The collection of lightweight controls contained within the lightweight control.
        /// </summary>
        private BehaviorControlCollection controls;

        /// <summary>
        /// A value indicating whether the control is displayed.
        /// </summary>
        private bool visible = true;

        /// <summary>
        /// The suspend layout state of the lightweight control.
        /// </summary>
        private int suspendLayoutCount = 0;

        public bool AllowDrop = false;

        public bool AllowMouseWheel = false;

        public BehaviorControl()
        {
            //	Instantiate the lightweight control collection.
            controls = new BehaviorControlCollection(this);
        }

        public IBehaviorControlContainerControl ContainerControl
        {
            get
            {
                return containerControl;
            }
            set
            {
                containerControl = value;
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
                if (ContainerControl != null)
                    return ContainerControl.Parent;
                return null;
            }
        }

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    Invalidate();
                    OnVisibleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the virtual location of the lightweight control relative to the upper-left
        /// corner of its lightweight control container.
        /// </summary>
        public Point VirtualLocation
        {
            get
            {
                return virtualLocation;
            }
            set
            {
                if (virtualLocation != value)
                {
                    //	Set the virtual location.
                    virtualLocation = value;

                    //	If we have a lightweight control container, have it perform a layout.
                    if (containerControl != null)
                        containerControl.PerformLayout();

                    //	Finally, raise the VirtualLocationChanged event.
                    OnVirtualLocationChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the virtual size of the lightweight control.
        /// </summary>
        public Size VirtualSize
        {
            get
            {
                return virtualSize;
            }
            set
            {
                Debug.Assert(value.Width >= 0 && value.Height >= 0, "virtual size must be a positive value");
                //	If the virtual size has changed, change it.
                if (virtualSize != value)
                {
                    //	Set the virtual size.
                    virtualSize = value;

                    //	Layout the control.
                    PerformLayout();

                    //	If we have a lightweight control container, have it perform a layout.
                    if (containerControl != null)
                        containerControl.PerformLayout();

                    //	Finally, raise the VirtualSizeChanged event.
                    OnVirtualSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        public virtual Size DefaultVirtualSize
        {
            get
            {
                return new Size(0, 0);
            }
        }

        /// <summary>
        /// Gets the minimum virtual size of the lightweight control.
        /// </summary>
        public virtual Size MinimumVirtualSize
        {
            get
            {
                return new Size(0, 0);
            }
        }

        /// <summary>
        /// Gets or sets the virtual width of the lightweight control.
        /// </summary>
        public int VirtualWidth
        {
            get
            {
                return virtualSize.Width;
            }
            set
            {
                if (virtualSize.Width != value)
                    VirtualSize = new Size(value, VirtualHeight);
            }
        }

        /// <summary>
        /// Gets or sets the virtual height of the lightweight control.
        /// </summary>
        public int VirtualHeight
        {
            get
            {
                return virtualSize.Height;
            }
            set
            {
                if (virtualSize.Height != value)
                    VirtualSize = new Size(VirtualWidth, value);
            }
        }

        /// <summary>
        /// Gets or sets the virtual bounds, or size and location of the lightweight control.
        /// </summary>
        public Rectangle VirtualBounds
        {
            get
            {
                return new Rectangle(VirtualLocation, VirtualSize);
            }
            set
            {
                if (VirtualBounds != value)
                {
                    SuspendLayout();
                    VirtualLocation = value.Location;
                    VirtualSize = value.Size;
                    ResumeLayout();
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets the rectangle that represents the client area of the lightweight control.
        /// </summary>
        public Rectangle VirtualClientRectangle
        {
            get
            {
                return new Rectangle(0, 0, VirtualSize.Width, VirtualSize.Height);
            }
        }

        /// <summary>
        /// Used by MSHTML to determine if the specified point should be considered as part of the element this BehaviorControl is attached to.
        /// </summary>
        /// <returns>true if the point is located in an area that should be considered part of the element this BehaviorControl is attached to, false otherwise</returns>
        public virtual bool HitTestPoint(Point testPoint)
        {
            bool hit = false;

            foreach (BehaviorControl lightweightControl in controls)
            {
                Point virtualTestPoint = lightweightControl.PointToVirtualClient(testPoint);
                hit |= lightweightControl.HitTestPoint(virtualTestPoint);
            }

            return hit;
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

        /// <summary>
        /// Invalidates the lightweight control.
        /// </summary>
        public void Invalidate()
        {
            if (Parent != null)
                Parent.Invalidate(VirtualClientRectangleToParent());
        }

        public void PerformLayout()
        {
            //	If layout is not suspended, invoke the OnLayout method.
            if (!IsLayoutSuspended)
            {
                //	During layout we suspend layout so that the control can change it's size as
                //	needed. This is the key to the lightweight control architecture working
                //	properly.  As each lightweight control processes its layout event, it makes
                //	whatever changes are necessary to its size and layout at the same time.
                SuspendLayout();
                OnLayout(EventArgs.Empty);
                ResumeLayout();
            }
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Translates the virtual client rectangle to a parent rectangle.
        /// </summary>
        /// <returns>Translated VirtualClientRectangle rectangle.</returns>
        public Rectangle VirtualClientRectangleToParent()
        {
            return VirtualClientRectangleToParent(VirtualClientRectangle);
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Translates a virtual client rectangle to a parent rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to translate.</param>
        /// <returns>Translated rectangle.</returns>
        public Rectangle VirtualClientRectangleToParent(Rectangle rectangle)
        {
            //	Translate the rectangle to be relative to the virtual location of this lightweight
            //	control.
            rectangle.Offset(virtualLocation);

            //	Continue up the chain of lightweight control containers.
            if (containerControl == null)
                return rectangle;
            else
                return containerControl.VirtualClientRectangleToParent(rectangle);
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Translates a point to be relative to the the virtual client rectangle.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        public Point PointToVirtualClient(Point point)
        {
            //	Translate the point to be relative to the virtual location of this lightweight
            //	control.
            point.Offset(VirtualLocation.X * -1, VirtualLocation.Y * -1);

            //	Continue up the chain of lightweight control container controls.
            if (containerControl == null)
                return point;
            else
                return containerControl.PointToVirtualClient(point);
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Translates a virtual client point to be relative to a parent point.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        public Point VirtualClientPointToParent(Point point)
        {
            //	Translate the point to be relative to the virtual location of this lightweight
            //	control.
            point.Offset(VirtualLocation.X, VirtualLocation.Y);

            //	Continue up the chain of lightweight control container controls.
            if (containerControl == null)
                return point;
            else
                return containerControl.VirtualClientPointToParent(point);
        }

        #region Events

        /// <summary>
        /// Raises the VirtualSizeChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVirtualSizeChanged(EventArgs e)
        {
            if (VirtualSizeChanged != null)
                VirtualSizeChanged(this, e);
        }

        public event EventHandler VirtualSizeChanged;

        /// <summary>
        /// Raises the VirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVirtualLocationChanged(EventArgs e)
        {
            if (VirtualLocationChanged != null)
                VirtualLocationChanged(this, e);
        }

        public event EventHandler VirtualLocationChanged;

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnLayout(EventArgs e)
        {
            if (Layout != null)
                Layout(this, e);
        }
        public event EventHandler Layout;

        /// <summary>
        /// Raises the VisibleChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVisibleChanged(EventArgs e)
        {
            if (VisibleChanged != null)
                VisibleChanged(this, e);
        }
        public event EventHandler VisibleChanged;

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        internal void RaiseMouseDown(MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseMouseEnter(EventArgs e)
        {
            OnMouseEnter(e);
        }

        /// <summary>
        /// Raises the MouseHover event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseMouseHover(EventArgs e)
        {
            OnMouseHover(e);
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseMouseLeave(EventArgs e)
        {
            OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        internal void RaiseMouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        internal void RaiseMouseUp(MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        /// <summary>
        /// Raises the MouseWheel event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        internal void RaiseMouseWheel(MouseEventArgs e)
        {
            OnMouseWheel(e);
        }

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseClick(EventArgs e)
        {
            OnClick(e);
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null)
                MouseDown(this, e);
        }
        public event MouseEventHandler MouseDown;

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseEnter(EventArgs e)
        {
            if (MouseEnter != null)
                MouseEnter(this, e);
        }
        public event EventHandler MouseEnter;

        /// <summary>
        /// Raises the MouseHover event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseHover(EventArgs e)
        {
            if (MouseHover != null)
                MouseHover(this, e);
        }
        public event EventHandler MouseHover;

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseLeave(EventArgs e)
        {
            if (MouseLeave != null)
                MouseLeave(this, e);
        }
        public event EventHandler MouseLeave;

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null)
                MouseMove(this, e);
        }
        public event MouseEventHandler MouseMove;

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null)
                MouseUp(this, e);
        }
        public event MouseEventHandler MouseUp;

        /// <summary>
        /// Raises the MouseWheel event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            if (MouseWheel != null)
                MouseWheel(this, e);
        }
        public event MouseEventHandler MouseWheel;

        protected virtual void OnClick(EventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }
        public event EventHandler Click;

        #endregion

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        internal void RaisePaint(PaintEventArgs e)
        {
            OnPaint(e);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected virtual void OnPaint(PaintEventArgs e)
        {
            //	Raise the paint event to get this lightweight control painted first.
            //RaiseEvent(PaintEventKey, e);

            //	Enumerate the child lightweight controls and paint controls that need painting.
            //	This enumeration occurs in reverse Z order.  Child controls lower in the Z order
            //	are painted before child controls higher in the Z order.
            foreach (BehaviorControl lightweightControl in controls)
            {
                //	If the control is visible and inside the clip rectangle, paint it.
                if (lightweightControl.Visible &&
                    lightweightControl.VirtualWidth != 0 &&
                    lightweightControl.VirtualHeight != 0 &&
                    lightweightControl.VirtualBounds.IntersectsWith(e.ClipRectangle))
                {
                    //	Obtain the lightweight control virtual client rectangle.
                    Rectangle lightweightControlVirtualClientRectangle = lightweightControl.VirtualClientRectangle;

                    //	Create the graphics container for the lightweight control paint event that
                    //	causes anything applied to the lightweight control's virtual client
                    //	rectangle to be mapped to the lightweight control's on-screen bounds.
                    GraphicsContainer graphicsContainer = e.Graphics.BeginContainer(lightweightControl.VirtualBounds,
                        lightweightControlVirtualClientRectangle,
                        GraphicsUnit.Pixel);

                    //	Clip the graphics context to prevent the lightweight control from drawing
                    //	outside its client rectangle.
                    e.Graphics.SetClip(lightweightControlVirtualClientRectangle);

                    //	Set a new default compositing mode and quality.
                    e.Graphics.CompositingMode = CompositingMode.SourceOver;
                    e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                    //	Raise the Paint event for the lightweight control.
                    lightweightControl.OnPaint(new PaintEventArgs(e.Graphics, lightweightControlVirtualClientRectangle));

                    //	End the graphics container for the lightweight control.
                    e.Graphics.EndContainer(graphicsContainer);
                }
            }
        }

        /// <summary>
        /// Gets the Behavior control that is located at the specified point.
        /// </summary>
        /// <param name="point">A Point that contains the coordinates where you want to look for a control.
        ///	Coordinates are expressed relative to the upper-left corner of the control's client area.</param>
        /// <returns>Behavior control at the specified point.</returns>
        internal BehaviorControl GetBehaviorControlAtPoint(Point point)
        {
            //	Enumerate the Behavior controls, in Z order, to locate one at the virtual point.
            for (int index = Controls.Count - 1; index >= 0; index--)
            {
                //	Access the Behavior control.
                BehaviorControl BehaviorControl = Controls[index];

                //	If the Behavior control is visible, and the point is within its virtual
                //	bounds, then recursively search for the right control.
                if (BehaviorControl.Visible && BehaviorControl.VirtualBounds.Contains(point))
                {
                    //	Translate the point to be relative to the location of the Behavior control.
                    point.Offset(BehaviorControl.VirtualLocation.X * -1, BehaviorControl.VirtualLocation.Y * -1);

                    //	Recursively search for the right Behavior control.
                    return BehaviorControl.GetBehaviorControlAtPoint(point);
                }
            }

            //	The point is not inside a child Behavior control of this Behavior control,
            //	so it's inside this Behavior control.
            return this;
        }

        /// <summary>
        /// Gets the Behavior control that is located at the specified point and allows drag and drop events.
        /// </summary>
        /// <param name="point">A Point that contains the coordinates where you want to look for a control.
        ///	Coordinates are expressed relative to the upper-left corner of the control's client area.</param>
        /// <returns>Behavior control at the specified point.</returns>
        internal BehaviorControl GetDragDropBehaviorControlAtPoint(Point point)
        {
            //	Enumerate the Behavior controls, in Z order, to locate one at the virtual point.
            for (int index = Controls.Count - 1; index >= 0; index--)
            {
                //	Access the Behavior control.
                BehaviorControl BehaviorControl = Controls[index];

                //	If the Behavior control is visible, and the point is within its virtual
                //	bounds, then recursively call it sit is under the point.
                if (BehaviorControl.Visible && BehaviorControl.VirtualBounds.Contains(point))
                {
                    //	Translate the point to be relative to the location of the Behavior control.
                    point.Offset(BehaviorControl.VirtualLocation.X * -1, BehaviorControl.VirtualLocation.Y * -1);

                    //	Recursively search for the right drag and drop Behavior control.
                    return BehaviorControl.GetDragDropBehaviorControlAtPoint(point);
                }
            }

            //	The point is not inside a child Behavior control of this Behavior control
            //	that allows mouse wheel events.  So it's inside this Behavior control.  If this
            //	Behavior control allows mouse wheel events, return it.  Otherwise, return null.
            return AllowDrop ? this : null;
        }

        /// <summary>
        /// Gets the Behavior control that is located at the specified point and allows mouse wheel events.
        /// </summary>
        /// <param name="point">A Point that contains the coordinates where you want to look for a control.
        ///	Coordinates are expressed relative to the upper-left corner of the control's client area.</param>
        /// <returns>Behavior control at the specified point.</returns>
        internal BehaviorControl GetMouseWheelBehaviorControlAtPoint(Point point)
        {
            //	Enumerate the Behavior controls, in Z order, to locate one at the virtual point.
            for (int index = Controls.Count - 1; index >= 0; index--)
            {
                //	Access the Behavior control.
                BehaviorControl BehaviorControl = Controls[index];

                //	If the Behavior control is visible, and the point is within its virtual
                //	bounds, then recursively call it sit is under the point.
                if (BehaviorControl.Visible && BehaviorControl.VirtualBounds.Contains(point))
                {
                    //	Translate the point to be relative to the location of the Behavior control.
                    point.Offset(BehaviorControl.VirtualLocation.X * -1, BehaviorControl.VirtualLocation.Y * -1);

                    //	Recursively search for the right mouse wheel Behavior control.
                    BehaviorControl childMouseWheelBehaviorControl = BehaviorControl.GetMouseWheelBehaviorControlAtPoint(point);
                    if (childMouseWheelBehaviorControl != null)
                        return childMouseWheelBehaviorControl;
                }
            }

            //	The point is not inside a child Behavior control of this Behavior control
            //	that allows mouse wheel events.  So it's inside this Behavior control.  If this
            //	Behavior control allows mouse wheel events, return it.  Otherwise, return null.
            return AllowMouseWheel ? this : null;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
        }

        #endregion
    }
}
