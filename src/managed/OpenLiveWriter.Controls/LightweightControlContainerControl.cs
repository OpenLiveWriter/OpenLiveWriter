// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    #region Public Delegates

    /// <summary>
    /// The delegate for ShowContextMenu events.
    /// </summary>
    public delegate void ShowContextMenuEventHandler(object sender, ShowContextMenuEventArgs e);

    #endregion Public Delegates

    #region Public Enumerations

    /// <summary>
    /// Visible direction enumeration.
    /// </summary>
    public enum VisibleDirection
    {
        Horizontal = 1,
        Vertical = 2,
        Both = Horizontal | Vertical
    }

    #endregion Public Enumerations

    /// <summary>
    /// LightweightControlContainerControl class.  Serves as the base class for heavyweight
    /// controls that can contain lightweight controls as well as other heavyweight controls.
    /// </summary>
    public class LightweightControlContainerControl : FocusWatchingUserControl, ISupportInitialize, ILightweightControlContainerControl, IToolTipDisplay
    {
        #region Private Enumerations

        /// <summary>
        /// Drag-and-drop auto-scroll modes.
        /// </summary>
        private enum DragDropAutoScrollMode
        {
            /// <summary>
            /// Drag-and-drop auto-scroll is off.
            /// </summary>
            Off,

            /// <summary>
            /// Drag-and-drop auto-scroll is occuring in the up direction.
            /// </summary>
            Up,

            /// <summary>
            /// Drag-and-drop auto-scroll is occuring in the down direction.
            /// </summary>
            Down
        }

        #endregion

        #region Static & Constant Declarations

        /// <summary>
        /// The height of the drag-and-drop auto-scroll rectangle at the top and bottom of the
        /// control's ClientRectangle.
        /// </summary>
        private static int DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT = 25;

        /// <summary>
        /// The initial drag-and-drop auto-scroll timer interval.
        /// </summary>
        private const int DRAG_AND_DROP_AUTO_SCROLL_INITIAL_INTERVAL = 500;

        /// <summary>
        /// The drag-and-drop auto-scroll timer interval.
        /// </summary>
        private const int DRAG_AND_DROP_AUTO_SCROLL_INTERVAL = 15;

        /// <summary>
        /// The drag-and-drop auto-scroll step (how much we move).
        /// </summary>
        private const int DRAG_AND_DROP_AUTO_SCROLL_STEP = 2;

        #endregion Static & Constant Declarations

        #region Private Member Variables & Declarations

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// Tooltip component.
        /// </summary>
        private ToolTip2 toolTip;

        /// <summary>
        /// The collection of lightweight controls contained within the control.
        /// </summary>
        private LightweightControlCollection lightweightControls;

        /// <summary>
        /// The active lightweight control.  All keyboard messages are sent to the active control.
        /// </summary>
        private LightweightControl activeLightweightControl;

        /// <summary>
        /// The mouse lightweight control.  The lightweight control that the mouse is in.
        /// </summary>
        private LightweightControl mouseLightweightControl;

        /// <summary>
        /// The drag-and-drop lightweight control.  The lightweight control that the mouse is in
        /// that allows drag-and-drop events.
        /// </summary>
        private LightweightControl dragDropLightweightControl;

        /// <summary>
        /// A value which indicates whether drag-and-drop auto-scroll is allowed.
        /// </summary>
        private bool allowDragDropAutoScroll;

        /// <summary>
        /// Gets or sets a value which indicates whether drag-and-drop auto-scroll is allowed.
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(true),
                Localizable(false),
                Description("Specifies whether drag-and-drop auto-scroll is initially allowed.")
        ]
        public bool AllowDragDropAutoScroll
        {
            get
            {
                return allowDragDropAutoScroll;
            }
            set
            {
                if (allowDragDropAutoScroll != value)
                {
                    allowDragDropAutoScroll = value;
                    if (!allowDragDropAutoScroll)
                        StopDragDropAutoScroll();
                    OnAllowDragDropAutoScrollChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The drag-and-drop auto-scroll timer.  This timer controls when drag-and-drop auto-scroll
        /// occurs.
        /// </summary>
        private Timer dragDropAutoScrollTimer;

        /// <summary>
        /// The drag-and-drop auto-scroll mode.
        /// </summary>
        private DragDropAutoScrollMode dragDropAutoScrollMode = DragDropAutoScrollMode.Off;

        /// <summary>
        /// The drag-and-drop auto-scroll rectangle.  This is the area where the mouse must stay
        /// during drag-and-drop auto-scroll.  If the mouse moves out of this rectangle, the
        /// drag-and-drop auto-scroll will stop.
        /// </summary>
        private Rectangle dragDropAutoScrollRectangle;

        /// <summary>
        /// The mouse position.
        /// </summary>
        private Point dragDropAutoScrollPoint;

        /// <summary>
        /// The initialization count of the component.
        /// </summary>
        private int initializationCount = 0;

        /// <summary>
        /// A value which indicates whether this is the first paint.
        /// </summary>
        private bool firstPaint = true;

        /// <summary>
        /// Controller for custom focus and accessibility
        /// </summary>
        private LCCCFocusAndAccessibilityController _focusAndAccessibilityController;

        #endregion Private Member Variables & Declarations

        #region Public Events

        /// <summary>
        /// The AllowDragDropAutoScrollChanged event key.
        /// </summary>
        private static readonly object AllowDragDropAutoScrollChangedEventKey = new object();

        /// <summary>
        /// Occurs when the VirtualSize property is changed.
        /// </summary>
        [
            Category("Property Changed"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the AllowDragDropAutoScroll property is changed.")
        ]
        public event EventHandler AllowDragDropAutoScrollChanged
        {
            add
            {
                Events.AddHandler(AllowDragDropAutoScrollChangedEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(AllowDragDropAutoScrollChangedEventKey, value);
            }
        }

        /// <summary>
        /// The DragDropAutoScrollBegin event key.
        /// </summary>
        private static readonly object DragDropAutoScrollBeginEventKey = new object();

        /// <summary>
        /// Occurs when drag-and-drop auto-scroll begins.
        /// </summary>
        [
            Category("Action"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when drag-and-drop auto-scroll begins.")
        ]
        public event EventHandler DragDropAutoScrollBegin
        {
            add
            {
                Events.AddHandler(DragDropAutoScrollBeginEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(DragDropAutoScrollBeginEventKey, value);
            }
        }

        /// <summary>
        /// The DragDropAutoScrollEnd event key.
        /// </summary>
        private static readonly object DragDropAutoScrollEndEventKey = new object();

        /// <summary>
        /// Occurs when drag-and-drop auto-scroll ends.
        /// </summary>
        [
            Category("Action"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when drag-and-drop auto-scroll ends.")
        ]
        public event EventHandler DragDropAutoScrollEnd
        {
            add
            {
                Events.AddHandler(DragDropAutoScrollEndEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(DragDropAutoScrollEndEventKey, value);
            }
        }

        /// <summary>
        /// The DragInside event key.
        /// </summary>
        private static readonly object DragInsideEventKey = new object();

        /// <summary>
        /// Occurs when an object is dragged into the control's bounds.
        /// </summary>
        [
            Category("Action"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when an object is dragged into the control's bounds.")
        ]
        public event DragEventHandler DragInside
        {
            add
            {
                Events.AddHandler(DragInsideEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(DragInsideEventKey, value);
            }
        }

        /// <summary>
        /// The DragOutside event key.
        /// </summary>
        private static readonly object DragOutsideEventKey = new object();

        /// <summary>
        /// Occurs when an object is dragged into the control's bounds.
        /// </summary>
        [
            Category("Action"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when an object is dragged into the control's bounds.")
        ]
        public event EventHandler DragOutside
        {
            add
            {
                Events.AddHandler(DragOutsideEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(DragOutsideEventKey, value);
            }
        }

        /// <summary>
        /// The ShowContextMenu event key.
        /// </summary>
        private static readonly object ShowContextMenuEventKey = new object();

        /// <summary>
        /// Occurs when a context menu is about to be shown.
        /// </summary>
        [
            Category("Action"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when a context menu is being displayed.")
        ]
        public event ShowContextMenuEventHandler ShowContextMenu
        {
            add
            {
                Events.AddHandler(ShowContextMenuEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(ShowContextMenuEventKey, value);
            }
        }

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the LightweightScrollableControl class.
        /// </summary>
        public LightweightControlContainerControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Instantiate the lightweight control collection.
            lightweightControls = new LightweightControlCollection(this);

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            //	Redraw the control on resize.
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lightweightControls.Clear();
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion Class Initialization & Termination

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new ToolTip2(this.components);
            //
            // toolTip
            //
            this.toolTip.ShowAlways = true;
            //
            // LightweightControlContainerControl
            //
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Name = "LightweightControlContainerControl";
            this.Size = new System.Drawing.Size(216, 224);

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the collection of lightweight controls contained within the control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public LightweightControlCollection LightweightControls
        {
            get
            {
                return lightweightControls;
            }
        }

        /// <summary>
        /// The active lightweight control.  All keyboard messages are sent to the active control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public LightweightControl ActiveLightweightControl
        {
            get
            {
                return activeLightweightControl;
            }
            set
            {
                LightweightControl oldActiveLightweightControl = activeLightweightControl;
                activeLightweightControl = value;
                if (oldActiveLightweightControl != null && oldActiveLightweightControl != value)
                {
                    //clear the existing active lightweight control.
                    oldActiveLightweightControl.Unfocus();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value which indicates whether all painting occurs during Paint event processing.
        /// </summary>
        public bool AllPaintingInWmPaint
        {
            get
            {
                return GetStyle(ControlStyles.AllPaintingInWmPaint);
            }
            set
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint, value);
            }
        }

#if false
        /// <summary>
        /// Gets or sets the shortcut menu associated with the control.
        /// </summary>
        public override ContextMenu ContextMenu
        {
            get
            {
                return base.ContextMenu;
            }
            set
            {
                base.ContextMenu = contextMenu = value;
            }
        }
#endif

        /// <summary>
        /// Gets a value indicating whether the the LightweightControl is scrolled to the top.
        /// </summary>
        public bool ScrolledToTop
        {
            get
            {
                return AutoScrollPosition.Y == 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the the LightweightControl is scrolled to the bottom.
        /// </summary>
        public bool ScrolledToBottom
        {
            get
            {
                return AutoScrollMinSize.Height < ClientRectangle.Height || Math.Abs(AutoScrollPosition.Y) == AutoScrollMinSize.Height - ClientRectangle.Height;
            }
        }

        /// <summary>
        /// Gets a value indicating whether drag-and-drop auto-scroll is in progress.
        /// </summary>
        public bool DragDropAutoScrollInProgress
        {
            get
            {
                if (dragDropAutoScrollTimer == null)
                    return false;
                else
                {
                    return dragDropAutoScrollMode != DragDropAutoScrollMode.Off && dragDropAutoScrollTimer.Interval == DRAG_AND_DROP_AUTO_SCROLL_INTERVAL;
                }
            }
        }

        /// <summary>
        /// Gets a value which indicates whether focus queues should be shown.
        /// </summary>
        public bool ShowFocus
        {
            get
            {
                return ShowFocusCues;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <interface>ISupportInitialize</interface>
        /// <summary>
        /// Begins initialization.
        /// </summary>
        public void BeginInit()
        {
            if (++initializationCount == 1)
                SuspendLayout();
        }

        /// <interface>ISupportInitialize</interface>
        /// <summary>
        /// Ends initialization.
        /// </summary>
        public void EndInit()
        {
            if (--initializationCount == 0)
                ResumeLayout(true);
        }

        /// <summary>
        /// Translates a virtual client rectangle to a parent rectangle.
        /// </summary>
        /// <param name="rectangle">The virtual client rectangle to translate.</param>
        /// <returns>Translated rectangle.</returns>
        public Rectangle VirtualClientRectangleToParent(Rectangle rectangle)
        {
            return VirtualClientRectangleToClientRectangle(rectangle);
        }

        /// <summary>
        /// Translates a point to be relative to the the virtual client rectangle.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        public Point PointToVirtualClient(Point point)
        {
            //	Map the client point to a virtual point.
            return ClientPointToVirtualClientPoint(point);
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Translates a virtual client point to be relative to a parent point.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        public Point VirtualClientPointToParent(Point point)
        {
            return VirtualClientPointToClientPoint(point);
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Translates a virtual client point to be relative to a parent point.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        public Point VirtualClientPointToScreen(Point point)
        {
            return PointToScreen(VirtualClientPointToClientPoint(point));
        }

        ///	<interface>IToolTipDisplay</interface>
        /// <summary>
        /// Sets the tooltip.
        /// </summary>
        /// <param name="toolTipText">The tooltip text, or null if no tooltip text should be displayed.</param>
        public void SetToolTip(string toolTipText)
        {
            toolTip.SetToolTip(this, toolTipText);
        }

        /// <summary>
        /// Makes the specified client rectangle visible in the specified direction by adjusting
        ///	the AutoScrollPosition as needed.
        /// </summary>
        /// <param name="rectangle">The client rectangle to make visible.</param>
        /// <param name="visibleDirection">The direction in which to make the client rectangle visible.</param>
        public void MakeClientRectangleVisible(Rectangle rectangle, VisibleDirection visibleDirection)
        {
            //	Obtain the client rectangle.
            Rectangle clientRectangle = ClientRectangle;

            //	Adjust rectangle height if it's larger than the client height.
            if (rectangle.Height > clientRectangle.Height)
                rectangle.Height = clientRectangle.Height;

            //	Adjust rectangle width if it's larger than the client width.
            if (rectangle.Width > clientRectangle.Width)
                rectangle.Width = clientRectangle.Width;

            //	Check vertical visibility, as specified.
            bool verticallyVisible = rectangle.Y >= clientRectangle.Y && rectangle.Bottom <= clientRectangle.Bottom;
            bool horizontallyVisible = rectangle.X >= clientRectangle.X && rectangle.Right <= clientRectangle.Right;

            //	Check visibility.
            if (visibleDirection == VisibleDirection.Vertical)
            {
                if (verticallyVisible)
                    return;
            }
            else if (visibleDirection == VisibleDirection.Horizontal)
            {
                if (horizontallyVisible)
                    return;
            }
            else if (visibleDirection == VisibleDirection.Both)
            {
                if (verticallyVisible && horizontallyVisible)
                    return;
            }

            //	Calculate the vertical delta, as specified and needed.
            int verticalDelta = 0;
            if ((visibleDirection & VisibleDirection.Vertical) != 0 && !verticallyVisible)
            {
                if (rectangle.Top < 0)
                    verticalDelta = rectangle.Top;
                else
                    verticalDelta = rectangle.Bottom - clientRectangle.Height;
            }

            //	Calculate the horizontal delta, as specified and needed.
            int horizontalDelta = 0;
            if ((visibleDirection & VisibleDirection.Horizontal) != 0 && !horizontallyVisible)
            {
                if (rectangle.Left < 0)
                    horizontalDelta = rectangle.Left;
                else
                    horizontalDelta = rectangle.Right - clientRectangle.Right;
            }

            //	Adjust the auto-scroll position.
            AutoScrollPosition = new Point(Math.Abs(AutoScrollPosition.X) + horizontalDelta,
                                            Math.Abs(AutoScrollPosition.Y) + verticalDelta);
        }

        /// <summary>
        /// Scrolls the control home.
        /// </summary>
        /// <returns></returns>
        public void ScrollHome()
        {
            AutoScrollPosition = new Point(0, 0);
        }

        /// <summary>
        /// Scrolls the control to the end.
        /// </summary>
        /// <returns></returns>
        public void ScrollEnd()
        {
            AutoScrollPosition = new Point(AutoScrollMinSize);
        }

        public void RtlLayoutFixupLightweightControls(bool recursive)
        {
            if (BidiHelper.IsRightToLeft && RightToLeft == RightToLeft.Yes)
            {
                foreach (LightweightControl lc in LightweightControls)
                {
                    lc.VirtualLocation = new Point(
                        ClientSize.Width - lc.VirtualBounds.Right,
                        lc.VirtualLocation.Y);
                    if (recursive)
                        lc.RtlLayoutFixup(true);
                }
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified client rectangle is visible in the
        /// specified direction.
        /// </summary>
        /// <param name="rectangle">The client rectangle check the visibility of.</param>
        /// <param name="visibleDirection">The direction to check visibility.</param>
        public bool IsClientRectangleVisible(Rectangle rectangle, VisibleDirection visibleDirection)
        {
            //	Obtain the client rectangle.
            Rectangle clientRectangle = ClientRectangle;

            //	Adjust rectangle height if it's larger than the client height.
            if (rectangle.Height > clientRectangle.Height)
                rectangle.Height = clientRectangle.Height;

            //	Adjust rectangle width if it's larger than the client width.
            if (rectangle.Width > clientRectangle.Width)
                rectangle.Width = clientRectangle.Width;

            //	Check vertical visibility, as specified.
            bool verticallyVisible = rectangle.Y >= clientRectangle.Y && rectangle.Bottom <= clientRectangle.Bottom;
            bool horizontallyVisible = rectangle.X >= clientRectangle.X && rectangle.Right <= clientRectangle.Right;

            //	Check visibility.
            if (visibleDirection == VisibleDirection.Vertical)
                return verticallyVisible;
            else if (visibleDirection == VisibleDirection.Horizontal)
                return horizontallyVisible;
            else if (visibleDirection == VisibleDirection.Both)
                return verticallyVisible && horizontallyVisible;
            else
            {
                //	Can't happen unless a new type of VisibleDirection is added.
                Debug.Fail("Unexpected VisibleDirection.");
                return false;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified client rectangle is visible.
        /// </summary>
        /// <param name="rectangle">The client rectangle.</param>
        public bool IsClientRectangleVisible(Rectangle rectangle)
        {
            return false;
        }

        protected override void Select(bool directed, bool forward)
        {
            //If custom focus management is enabled, let it decide where focus goes
            if (!ProcessControlNavigation(forward))
                base.Select(directed, forward);
        }
        #endregion Public Methods

        #region Protected Events

        /// <summary>
        /// Raises the AllowDragDropAutoScrollChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnAllowDragDropAutoScrollChanged(EventArgs e)
        {
            RaiseEvent(AllowDragDropAutoScrollChangedEventKey, e);
        }

        /// <summary>
        /// Raises the DragDropAutoScrollBegin event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnDragDropAutoScrollBegin(EventArgs e)
        {
            RaiseEvent(DragDropAutoScrollBeginEventKey, e);
        }

        /// <summary>
        /// Raises the DragDropAutoScrollEnd event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnDragDropAutoScrollEnd(EventArgs e)
        {
            RaiseEvent(DragDropAutoScrollEndEventKey, e);
        }

        /// <summary>
        /// Raises the DragInside event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected virtual void OnDragInside(DragEventArgs e)
        {
            RaiseEvent(DragInsideEventKey, e);
        }

        /// <summary>
        /// Raises the DragOutside event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnDragOutside(EventArgs e)
        {
            RaiseEvent(DragOutsideEventKey, e);
        }

        /// <summary>
        /// Raises the ShowContextMenu event.
        /// </summary>
        /// <param name="e">A ShowContextMenuEventArgs that contains the event data.</param>
        protected virtual void OnShowContextMenu(ShowContextMenuEventArgs e)
        {
            RaiseEvent(ShowContextMenuEventKey, e);
        }

        #endregion Protected Events

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">A EventArgs that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            //	If we have a mouse lightweight control, raise its Click event.  Otherwise, call
            //	the base class's method to raise the Click event on this control.
            if (MouseLightweightControl != null)
                MouseLightweightControl.RaiseClick(e);
            else
                base.OnClick(e);
        }

        /// <summary>
        /// Raises the DoubleClick event.
        /// </summary>
        /// <param name="e">A EventArgs that contains the event data.</param>
        protected override void OnDoubleClick(EventArgs e)
        {
            //	If we have a mouse lightweight control, raise its DoubleClick event.  Otherwise, call
            //	the base class's method to raise the DoubleClick event on this control.
            if (MouseLightweightControl != null)
                MouseLightweightControl.RaiseDoubleClick(e);
            else
                base.OnDoubleClick(e);
        }

        /// <summary>
        /// Raises the DragDrop event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected override void OnDragDrop(DragEventArgs e)
        {
            //	Stop drag-and-drop auto-scroll.
            StopDragDropAutoScroll();

            //	If we have a drag-and-drop lightweight control, raise its DragDrop event.
            //	Otherwise, call the base class's method to raise the DragDrop event on this
            //	control.
            if (dragDropLightweightControl != null)
            {
                //	Raise the DragDrop event on the drag-and-drop lightweight control.
                dragDropLightweightControl.RaiseDragDrop(e);

                //	Clear the drag-and-drop lightweight control.
                dragDropLightweightControl = null;
            }
            else
                base.OnDragDrop(e);
        }

        /// <summary>
        /// Raises the DragEnter event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnDragEnter(e);

            //	Find the drag-and-drop lightweight control at the mouse position.
            LightweightControl lightweightControl = GetDragDropLightweightControlAtClientPoint(PointToClient(new Point(e.X, e.Y)));

            //	If there is a drag-and-drop lightweight control at the mouse position, the event
            //	is for it.  Otherwise, it's for this control.
            if (lightweightControl != null)
            {
                //	Set the drag-and-drop lightweight control and raise its DragEnter event.
                dragDropLightweightControl = lightweightControl;
                e.Effect = DragDropEffects.None;
                dragDropLightweightControl.RaiseDragInside(e);
            }
        }

        /// <summary>
        /// Raises the DragLeave event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected override void OnDragLeave(EventArgs e)
        {
            //	Stop drag-and-drop auto-scroll.
            StopDragDropAutoScroll();

            base.OnDragLeave(e);

            //	If we have a drag-and-drop lightweight control, raise its DragLeave event, and
            //	then clear it.  Otherwise, call the base class's method to raise the DragLeave
            //	event on this control.
            if (dragDropLightweightControl != null)
            {
                dragDropLightweightControl.RaiseDragOutside(e);
                dragDropLightweightControl = null;
            }
            else
                base.OnDragLeave(e);

            //	Raise the DragOutside event.
            OnDragOutside(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the DragOver event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            //	Manage drag-and-drop auto-scroll.
            ManageDragDropAutoScroll(e);

            //	Update the drag-and-drop lightweight control.
            UpdateDragDropLightweightControl(e);

            //	If we have a drag-and-drop lightweight control, raise its DragOver event.
            //	Otherwise, call the base class's method to raise the DragOver event on this
            //	control.
            if (dragDropLightweightControl != null)
                dragDropLightweightControl.RaiseDragOver(e);
            else
                base.OnDragOver(e);
        }

        /// <summary>
        /// Raises the GiveFeedback event.
        /// </summary>
        /// <param name="e">A GiveFeedbackEventArgs that contains the event data.</param>
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            //	If we have a drag-and-drop lightweight control, raise its GiveFeedback event.
            //	Otherwise, call the base class's method to raise the GiveFeedback event on this
            //	control.
            if (dragDropLightweightControl != null)
                dragDropLightweightControl.RaiseGiveFeedback(e);
            else
                base.OnGiveFeedback(e);
        }

        /// <summary>
        /// Raises the KeyDown event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            //	If we have a active lightweight control, raise its KeyDown event.  Otherwise,
            //	call the base class's method to raise the KeyDown event on this control.
            if (activeLightweightControl != null)
            {
                activeLightweightControl.RaiseKeyDown(e);
            }
            else
                base.OnKeyDown(e);
        }

        /// <summary>
        /// Raises the KeyPress event.
        /// </summary>
        /// <param name="e">A KeyPressEventArgs that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            //	If we have a active lightweight control, raise its KeyPress event.  Otherwise,
            //	call the base class's method to raise the KeyPress event on this control.
            if (activeLightweightControl != null)
                activeLightweightControl.RaiseKeyPress(e);
            else
                base.OnKeyPress(e);
        }

        /// <summary>
        /// Raises the KeyUp event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            //	If we have a active lightweight control, raise its KeyUp event.  Otherwise,
            //	call the base class's method to raise the KeyUp event on this control.
            if (activeLightweightControl != null)
                activeLightweightControl.RaiseKeyUp(e);
            else
                base.OnKeyUp(e);
        }

        /// <summary>
        /// Raises the GotFocus event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnGotFocus(e);

            if (_focusAndAccessibilityController != null)
                _focusAndAccessibilityController.OnGotFocus();
        }

        /// <summary>
        /// Raises the LostFocus event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            if (_focusAndAccessibilityController != null)
                _focusAndAccessibilityController.OnLostFocus();

            //	Call the base class's method so that registered delegates receive the event.
            base.OnLostFocus(e);
        }

        internal void NotifyControlFocused(LightweightControl control)
        {
            if (_focusAndAccessibilityController != null)
                _focusAndAccessibilityController.NotifyFocusedControlChanged();
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (_focusAndAccessibilityController != null)
                _focusAndAccessibilityController.OnEnter();
        }

        protected override void OnLeave(EventArgs e)
        {
            if (_focusAndAccessibilityController != null)
                _focusAndAccessibilityController.OnLeave();
            base.OnLeave(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
            {
                Keys keys1 = keyData & Keys.KeyCode;
                switch (keys1)
                {
                    case Keys.Left:
                    case Keys.Up:
                    case Keys.Right:
                    case Keys.Down:
                        Control focusedChild = GetInnerMostFocusedControl(this);
                        if (focusedChild == null || !AllowNavKeysForControl(focusedChild, keys1))
                        {
                            if (ProcessControlNavigation((keys1 == Keys.Right) || (keys1 == Keys.Down)))
                            {
                                return true;
                            }
                        }
                        break;
                    case Keys.Tab:
                        if (ProcessControlNavigation((keyData & Keys.Shift) == Keys.None))
                        {
                            return true;
                        }
                        break;
                    case Keys.Enter:
                        if (DoDefaultAction())
                            return true;
                        break;
                    case Keys.Space:
                        if (DoDefaultAction())
                            return true;
                        break;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Allows subclasses to override whether navigation keyboard events will be
        /// passed along to child controls.  Controls should only be allowed if they
        /// override keyboard handling to prevent focus from changing.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected virtual bool AllowNavKeysForControl(Control control, Keys keys)
        {
            //allow up/down navigation for combo boxes in lightweight containers.
            if (control is TextBoxBase || control is NumericUpDown || control is ListView)
                return true;
            if ((control is ListControl) && (keys == Keys.Down || keys == Keys.Up))
                return true;
            return false;
        }

        /// <summary>
        /// Returns the inner most focused control.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private Control GetInnerMostFocusedControl(Control control)
        {
            while (control is ContainerControl)
            {
                ContainerControl containerControl = (ContainerControl)control;
                if (containerControl.Focused)
                    return containerControl;
                control = containerControl.ActiveControl;
            }
            return control;
        }

        private bool ProcessControlNavigation(bool forward)
        {
            if (_focusAndAccessibilityController != null)
            {
                if (_focusAndAccessibilityController.ProcessControlFocusNavigation(forward))
                    return true;
                else
                    return Parent.SelectNextControl(this, forward, true, true, false);
            }
            return false;
        }

        private bool DoDefaultAction()
        {
            if (_focusAndAccessibilityController != null)
                return _focusAndAccessibilityController.DoDefaultAction();
            return false;
        }

        /// <summary>
        /// Forces control of the control focus to be managed by the focus manager (tabIndex will be ignored!).
        /// </summary>
        ///
        public void InitFocusManager(bool wrap)
        {
            _focusAndAccessibilityController = new LCCCFocusAndAccessibilityController(this, wrap);
        }

        public void InitFocusManager()
        {
            _focusAndAccessibilityController = new LCCCFocusAndAccessibilityController(this, false);
        }

        public void AddFocusableControls(LightweightControl[] controls)
        {
            foreach (LightweightControl control in controls)
            {
                AddFocusableControl(control);
            }
        }

        public void AddFocusableControls(List<Control> controls)
        {
            foreach (Control control in controls)
            {
                AddFocusableControl(control);
            }
        }

        public void AddFocusableControl(LightweightControl control)
        {
            _focusAndAccessibilityController.AddControl(control);
        }

        public void AddFocusableControl(Control control)
        {
            _focusAndAccessibilityController.AddControl(control);
        }

        public void ClearFocusableControls()
        {
            _focusAndAccessibilityController.ClearControls();
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseLightweightControl(new Point(e.X, e.Y));

            //	If we have a mouse lightweight control, raise its MouseDown event.  Otherwise,
            //	call the base class's method to raise the MouseDown event on this control.
            if (MouseLightweightControl != null)
                MouseLightweightControl.RaiseMouseDown(TranslateMouseEventArgsForLightweightControl(e, MouseLightweightControl));
            else
                base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	If we have a current mouse lightweight control, clear it.  This is a bug.
            if (MouseLightweightControl != null)
                MouseLightweightControl = null;

            //	Update the mouse lightweight control.
            UpdateMouseLightweightControl();
        }

        /// <summary>
        /// Raises the MouseHover event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseHover(EventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseLightweightControl();

            //	If we have a mouse lightweight control, raise its MouseHover event.  Otherwise,
            //	call the base class's method to raise the MouseHover event on this control.
            if (MouseLightweightControl != null)
                MouseLightweightControl.RaiseMouseHover(EventArgs.Empty);
            else
                base.OnMouseHover(e);
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseLightweightControl();

            //	If we have a mouse lightweight control, raise its MouseLeave event, then clear it.
            //	Otherwise, call the base class's method to raise the MouseLeave event on this
            //	control.
            if (MouseLightweightControl != null)
            {
                MouseLightweightControl.RaiseMouseLeave(EventArgs.Empty);
                MouseLightweightControl = null;
            }
            else
                base.OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //	If any mouse button is down, we will not update the mouse lightweight control.
            if (e.Button != MouseButtons.None)
            {
                //	If we have a mouse lightweight control, raise its MouseMove event.  Otherwise,
                //	call the base class's method to raise the MouseMove event on this control.
                if (MouseLightweightControl != null)
                    MouseLightweightControl.RaiseMouseMove(TranslateMouseEventArgsForLightweightControl(e, MouseLightweightControl));
                else
                    base.OnMouseMove(e);
            }
            else
            {
                //	Update the mouse lightweight control.
                UpdateMouseLightweightControl(new Point(e.X, e.Y));

                //	If we have a mouse lightweight control, raise its MouseMove event.  Otherwise,
                //	call the base class's method to raise the MouseMove event on this control.
                if (MouseLightweightControl != null)
                    MouseLightweightControl.RaiseMouseMove(TranslateMouseEventArgsForLightweightControl(e, MouseLightweightControl));
                else
                    base.OnMouseMove(e);
            }
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            //	Update the mouse lightweight control.
            UpdateMouseLightweightControl(new Point(e.X, e.Y));

            //	If we have a mouse wheel lightweight control, raise its MouseUp event.  Otherwise,
            //	call the base class's method to raise the MouseUp event on this control.
            if (MouseLightweightControl != null)
                MouseLightweightControl.RaiseMouseUp(TranslateMouseEventArgsForLightweightControl(e, MouseLightweightControl));
            else
                base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the MouseWheel event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //	Find the lightweight control at the mouse position.
            LightweightControl mouseWheelLightweightControl = GetMouseWheelLightweightControlAtClientPoint(new Point(e.X, e.Y));

            //	If we have a mouse wheel lightweight control, raise its MouseWheel event.  Otherwise,
            //	call the base class's method to raise the MouseWheel event on this control.
            if (mouseWheelLightweightControl != null)
                mouseWheelLightweightControl.RaiseMouseWheel(TranslateMouseEventArgsForLightweightControl(e, mouseWheelLightweightControl));
            else
                base.OnMouseWheel(e);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            //	A very strange bug occurs where the application containing this control will be activated
            //	on tool-tip changes when it is not the active application.  This problem only occurs however
            //	when a tool tip has not been previously set.  So, this little hack here sets and then clears
            //	the tooltip on the first paint, and this seems to solve the problem.
            if (firstPaint)
            {
                firstPaint = false;
                SetToolTip(" ");
                SetToolTip(null);
            }

            //	Obtain the virtual clip rectangle.
            Rectangle virtualClipRectangle = ClientRectangleToVirtualClientRectangle(e.ClipRectangle);

            //	Enumerate the child lightweight controls and paint controls that need painting.
            //	This enumeration occurs in reverse Z order.  Child controls lower in the Z order
            //	are painted before child controls higher in the Z order.
            foreach (LightweightControl lightweightControl in lightweightControls)
            {
                //	If the control is visible and inside the virtual clip rectangle, paint it.
                if (lightweightControl.Visible && lightweightControl.VirtualBounds.IntersectsWith(virtualClipRectangle))
                {
                    //	Obtain the lightweight control virtual client rectangle.
                    Rectangle lightweightControlVirtualClientRectangle = lightweightControl.VirtualClientRectangle;

                    //	Create the graphics container for the lightweight control paint event that
                    //	causes anything applied to the lightweight control's virtual client
                    //	rectangle to be mapped to the lightweight control's on-screen bounds.
                    using (GraphicsHelper.Offset(e.Graphics,
                        VirtualClientRectangleToClientRectangle(lightweightControl.VirtualBounds),
                        lightweightControlVirtualClientRectangle))
                    {
                        //	Clip the graphics context to prevent the lightweight control from drawing
                        //	outside its client rectangle.
                        e.Graphics.SetClip(lightweightControlVirtualClientRectangle);

                        //	Set a new default compositing mode and quality.
                        e.Graphics.CompositingMode = CompositingMode.SourceOver;
                        e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                        //	Raise the Paint event for the lightweight control.
                        lightweightControl.RaisePaint(new PaintEventArgs(e.Graphics, lightweightControlVirtualClientRectangle));
                    }
                }
            }
        }

        /// <summary>
        /// Raises the QueryContinueDrag event.
        /// </summary>
        /// <param name="e">A QueryContinueDragEventArgs that contains the event data.</param>
        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            //	If we have a drag-and-drop lightweight control, raise its QueryContinueDrag event.
            //	Otherwise, call the base class's method to raise the QueryContinueDrag event on
            //	this control.
            if (dragDropLightweightControl != null)
                dragDropLightweightControl.RaiseQueryContinueDrag(e);
            else
                base.OnQueryContinueDrag(e);
        }

        #endregion Protected Event Overrides

        #region Protected Method Overrides

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows Message to process.</param>
        protected override void WndProc(ref Message m)
        {
            //	Dispatch the message.
            switch (m.Msg)
            {
                //	The WM_CONTEXTMENU message notifies a window that the user clicked the right
                //	mouse button (right clicked) in the window (or performed some other action that
                //	will display the context menu).
                case (int)WM.CONTEXTMENU:
                    {
                        //	Crack out x and y position.  This is in screen coordinates.
                        int x = MessageHelper.LOWORDToInt32(m.LParam);
                        int y = MessageHelper.HIWORDToInt32(m.LParam);

                        //	Remember the old ContextMenu so we can restore it later on should we change
                        //	it below.
                        ContextMenu oldContextMenu = ContextMenu;

                        //	Raise the ShowContextMenu event.
                        ShowContextMenuEventArgs showContextMenuEventArgs;
                        if (x == -1 && y == -1)
                        {
                            //	The position is -1, -1.  This indicates that the context menu is being
                            //	shown without a mouse position.  Raise the ShowContextMenu event on
                            //	this control.
                            showContextMenuEventArgs = new ShowContextMenuEventArgs(x, y, ContextMenu);
                            OnShowContextMenu(showContextMenuEventArgs);
                        }
                        else
                        {
                            //	The position is not -1, -1.  This indicates that the context menu is
                            //	being shown via a mouse gesture.  Attempt to locate the lightweight
                            //	control at the position.
                            Point point = PointToClient(new Point(x, y));
                            LightweightControl lightweightControl = GetMouseLightweightControlAtClientPoint(point);

                            //	If the lightweight control is non-null, and has a ContextMenu, raise
                            //	the ShowContextMenu event on it.
                            if (lightweightControl != null)
                            {
                                //	Instantiate the ShowContextMenuEventArgs.
                                showContextMenuEventArgs = new ShowContextMenuEventArgs(x, y, lightweightControl.ContextMenu);

                                //	Raise the ShowContextMenu event.
                                lightweightControl.RaiseShowContextMenu(showContextMenuEventArgs);
                            }
                            else
                            {
                                //	Raise the ShowContextMenu event.
                                showContextMenuEventArgs = new ShowContextMenuEventArgs(x, y, ContextMenu);
                                OnShowContextMenu(showContextMenuEventArgs);
                            }
                        }

                        //	If the event was not handled, handle it.
                        if (!showContextMenuEventArgs.Handled)
                        {
                            //	If we have a context menu to show, show it.  Otherwise, pass the message
                            //	on to the base class.
                            if (showContextMenuEventArgs.ContextMenu != null)
                            {
                                //	Make the context menu we're showing the context menu of this
                                //	control.  Doing so allows mnemonic processing to work.
                                ContextMenu = showContextMenuEventArgs.ContextMenu;

                                //	Show the context menu.
                                showContextMenuEventArgs.ContextMenu.Show(this, new Point(showContextMenuEventArgs.X, showContextMenuEventArgs.Y));
                            }
                            else
                                base.WndProc(ref m);
                        }

                        //	If we changed the context menu of this control, restore it.
                        if (ContextMenu != oldContextMenu)
                            ContextMenu = oldContextMenu;

                        //	Done!
                        return;
                    }
                case (int)WM.SETFOCUS:
                    {
                        if (_focusAndAccessibilityController != null && ActiveControl == null)
                        {
                            //eat focus messages so we can override all focus handling for this control!
                            OnGotFocus(EventArgs.Empty);
                            return;
                        }
                        break;
                    }
            }

            //	Call the base class's method.

            try
            {
                base.WndProc(ref m);
            }
            catch (Exception e)
            {
                Trace.Fail("LightweightControlContainerControl WndProc Exception", e.ToString());
            }
        }

        #endregion Protected Method Overrides

        #region Protected Methods
        /// <summary>
        /// Fires a click event so that subscribers to the LightweightContainerControl.Click event will receive it
        /// (reguardless of whether there is a mouseLightweightControl active)
        /// </summary>
        /// <param name="evt"></param>
        protected void OnLightweightContainerControlClick(EventArgs evt)
        {
            base.OnClick(evt);
        }
        #endregion

        #region Private Properties

        /// <summary>
        /// Gets or sets the mouse lightweight control.  The lightweight control that the mouse is in.
        /// </summary>
        private LightweightControl MouseLightweightControl
        {
            get
            {
                return mouseLightweightControl;
            }
            set
            {
                //	Set the mouse lightweight control.
                mouseLightweightControl = value;
#if false
                //	The mouse lightweight control's context menu becomes this control's context menu.
                if (mouseLightweightControl != null && mouseLightweightControl.ContextMenu != null)
                    base.ContextMenu = mouseLightweightControl.ContextMenu;
                else
                    base.ContextMenu = contextMenu;
#endif
            }
        }

        /// <summary>
        /// Gets the top drag-and-drop auto-scroll rectangle.  This is the rectangle at the top
        /// of the ClientRectangle where a drag-and-drop operation will initiate drag-and-drop
        /// auto-scroll in the up direction
        /// </summary>
        private Rectangle TopDragDropAutoScrollRectangle
        {
            get
            {
                //	Obtain the ClientRectangle.
                Rectangle rectangle = ClientRectangle;

                //	If the height of the control will not support drag-and-drop auto-scroll, return
                //	an empty rectangle.  Otherwise, calculate the top drag-and-drop auto-scroll
                //	rectangle.
                if (rectangle.Height < DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT * 2)
                    return Rectangle.Empty;
                else
                {
                    rectangle.Height = DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT;
                    return rectangle;
                }
            }
        }

        /// <summary>
        /// Gets the bottom drag-and-drop auto-scroll rectangle.  This is the rectangle at the
        /// bottom  of the ClientRectangle where a drag-and-drop operation will initiate
        /// drag-and-drop auto-scroll in the down direction
        /// </summary>
        private Rectangle BottomDragDropAutoScrollRectangle
        {
            get
            {
                //	Obtain the ClientRectangle.
                Rectangle rectangle = ClientRectangle;

                //	If the height of the control will not support drag-and-drop auto-scroll, return
                //	an empty rectangle.  Otherwise, calculate the bottom drag-and-drop auto-scroll
                //	rectangle.
                if (rectangle.Height < DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT * 2)
                    return Rectangle.Empty;
                else
                {
                    rectangle.Y = rectangle.Bottom - DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT;
                    rectangle.Height = DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT;
                    return rectangle;
                }
            }
        }

        #endregion Private Properties

        #region Private Methods

        /// <summary>
        /// Recursively lays out all lightweight controls in this lightweight control.
        /// </summary>
        /// <param name="lightweightControlContainerControl"></param>
        private void aLayoutLightweightControls(ILightweightControlContainerControl lightweightControlContainerControl)
        {
            //	If there are lightweight controls to layout, enumerate them.
            if (lightweightControlContainerControl.LightweightControls != null)
            {
                //	Enumerate the child lightweight controls and layout each one.
                foreach (LightweightControl lightweightControl in lightweightControlContainerControl.LightweightControls)
                {
                    //	Recursively layout all the lightweight controls in the lightweight control,
                    //	then layout the lightweight control.
                    aLayoutLightweightControls(lightweightControl);
                    lightweightControl.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Update the mouse lightweight control.  Detects whether the mouse lightweight control
        /// has changed, and raises the appropriate events if it has.
        /// </summary>
        private void UpdateMouseLightweightControl()
        {
            UpdateMouseLightweightControl(PointToClient(Control.MousePosition));
        }

        /// <summary>
        /// Update the mouse lightweight control.  Detects whether the mouse lightweight control
        /// has changed, and raises the appropriate events if it has.
        /// </summary>
        private void UpdateMouseLightweightControl(Point point)
        {
            //	Find the lightweight control at the mouse position.
            LightweightControl lightweightControl = GetMouseLightweightControlAtClientPoint(point);

            //	If the mouse lightweight control is changing, make the change.
            if (lightweightControl != MouseLightweightControl)
            {
                //	If we have a mouse lightweight control, raise its MouseLeave event.
                //	Otherwise, call the base class's method to raise the MouseLeave event on
                //	this control.
                if (MouseLightweightControl != null)
                {
                    if (MouseLightweightControl.LightweightControlContainerControl != null)
                        MouseLightweightControl.RaiseMouseLeave(EventArgs.Empty);
                }
                else
                    base.OnMouseLeave(EventArgs.Empty);

                //	Set the mouse lightweight control.
                MouseLightweightControl = lightweightControl;

                //	If we have a mouse lightweight control, raise its MouseEnter event.
                //	Otherwise, call the base class's method to raise the MouseEnter event on
                //	this control.
                if (MouseLightweightControl != null)
                    MouseLightweightControl.RaiseMouseEnter(EventArgs.Empty);
                else
                    base.OnMouseEnter(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Update the drag-and-drop lightweight control.  Detects whether the drag-and-drop
        /// lightweight control has changed, and raises the appropriate events, if it has.
        /// </summary>
        /// <param name="e">A MouseEventArgs containing the event data.</param>
        private void UpdateDragDropLightweightControl(DragEventArgs e)
        {
            //	Find the drag-and-drop lightweight control at the mouse position.
            LightweightControl lightweightControl = GetDragDropLightweightControlAtClientPoint(PointToClient(new Point(e.X, e.Y)));

            //	If the drag-and-drop lightweight control is changing, make the change.
            if (lightweightControl != dragDropLightweightControl)
            {
                //	If we have a drag-and-drop lightweight control, raise its OnDragLeave event.
                //	Otherwise, call the base class's method to raise the OnDragLeave event on
                //	this control.
                if (dragDropLightweightControl != null)
                    dragDropLightweightControl.RaiseDragOutside(EventArgs.Empty);
                else
                    OnDragOutside(EventArgs.Empty);

                //	Set the drag-and-drop lightweight control.
                dragDropLightweightControl = lightweightControl;
                e.Effect = DragDropEffects.None;

                //	If we have a drag-and-drop lightweight control, raise its DragEnter event.
                //	Otherwise, call the base class's method to raise the DragEnter event on this
                //	control.
                if (dragDropLightweightControl != null)
                    dragDropLightweightControl.RaiseDragInside(e);
                else
                    OnDragInside(e);
            }
        }

        /// <summary>
        /// Gets the mouse lightweight control that is located at the specified client point.
        /// </summary>
        /// <param name="point">A point that contains the coordinates where you want to look for a
        /// control. Coordinates are expressed relative to the upper-left corner of the control's
        /// client area.</param>
        /// <returns>The LightweightControl at the specified client point.  If there is no
        /// LightweightControl at the specified client point, the GetMouseLightweightControlAtClientPoint
        /// method returns a null reference.</returns>
        private LightweightControl GetMouseLightweightControlAtClientPoint(Point point)
        {
            //	Map the client point to a virtual client point.
            point = ClientPointToVirtualClientPoint(point);

            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = lightweightControls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                LightweightControl lightweightControl = lightweightControls[index];

                //	If the lightweight control is visible, and the virtual client point is inside
                //	it, translate the virtual client point to be relative to the location of the
                //	lightweight control and recursively ask the lightweight control to return the
                //	lightweight control at the virtual client point.  This will find the innermost
                //	lightweight control, at the top of the Z order, that contains the virual client
                //	point.
                if (lightweightControl.Visible && lightweightControl.VirtualBounds.Contains(point))
                {
                    //	Translate the virtual point to be relative to the location of the lightweight control.
                    point.Offset(lightweightControl.VirtualLocation.X * -1, lightweightControl.VirtualLocation.Y * -1);

                    //	Recursively get the mouse lightweight control at the translated point.
                    return lightweightControl.GetLightweightControlAtPoint(point);
                }
            }

            //	The client point is not inside any lightweight control.
            return null;
        }

        /// <summary>
        /// Gets the lightweight control that is located at the specified client point and allows
        /// drag-and-drop events.
        /// </summary>
        /// <param name="point">A point that contains the coordinates where you want to look for a
        /// control. Coordinates are expressed relative to the upper-left corner of the control's
        /// client area.</param>
        /// <returns>The LightweightControl at the specified client point that allows drag-and-drop
        /// events.  If there is no LightweightControl at the specified client point that allows
        /// drag-and-drop events, the GetLightweightControlAtClientPoint method returns a null
        /// reference.</returns>
        private LightweightControl GetDragDropLightweightControlAtClientPoint(Point point)
        {
            //	Map the client point to a virtual client point.
            point = ClientPointToVirtualClientPoint(point);

            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = lightweightControls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                LightweightControl lightweightControl = lightweightControls[index];

                //	If the lightweight control is visible, and the virtual client point is inside
                //	it, translate the virtual client point to be relative to the location of the
                //	lightweight control and recursively ask the lightweight control to return the
                //	lightweight control at the virtual client point that supports drag-and-drop
                //	events.  This will find the innermost lightweight control, at the top of the
                //	Z order, that contains the virual client point and supports drag-and-drop events.
                if (lightweightControl.Visible && lightweightControl.AllowDrop && lightweightControl.VirtualBounds.Contains(point))
                {
                    //	Translate the virtual point to be relative to the location of the lightweight control.
                    point.Offset(lightweightControl.VirtualLocation.X * -1, lightweightControl.VirtualLocation.Y * -1);

                    //	Recursively get the lightweight control at the translated point that
                    //	supports drag-and-drop events.
                    return lightweightControl.GetDragDropLightweightControlAtPoint(point);
                }
            }

            //	The client point is not inside any lightweight control that allows drag-and-drop events.
            return null;
        }

        /// <summary>
        /// Gets the lightweight control that is located at the specified client point and allows
        /// mouse wheel events.
        /// </summary>
        /// <param name="point">A point that contains the coordinates where you want to look for a
        /// control. Coordinates are expressed relative to the upper-left corner of the control's
        /// client area.</param>
        /// <returns>The LightweightControl at the specified client point that allows mouse wheel
        /// events.  If there is no LightweightControl at the specified client point that allows
        /// mouse wheel events, the GetLightweightControlAtClientPoint method returns a null
        /// reference.</returns>
        private LightweightControl GetMouseWheelLightweightControlAtClientPoint(Point point)
        {
            //	Map the client point to a virtual client point.
            point = ClientPointToVirtualClientPoint(point);

            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = lightweightControls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                LightweightControl lightweightControl = lightweightControls[index];

                //	If the lightweight control is visible, and the virtual client point is inside
                //	it, translate the virtual client point to be relative to the location of the
                //	lightweight control and recursively ask the lightweight control to return the
                //	lightweight control at the virtual client point that supports mouse wheel
                //	events.  This will find the innermost lightweight control, at the top of the
                //	Z order that contains the virual client point and supports mouse wheel events.
                if (lightweightControl.Visible && lightweightControl.VirtualBounds.Contains(point))
                {
                    //	Translate the virtual point to be relative to the location of the lightweight control.
                    point.Offset(lightweightControl.VirtualLocation.X * -1, lightweightControl.VirtualLocation.Y * -1);

                    //	Recursively get the lightweight control at the translated point that
                    //	supports mouse wheel events.
                    return lightweightControl.GetMouseWheelLightweightControlAtPoint(point);
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
        private MouseEventArgs TranslateMouseEventArgsForLightweightControl(MouseEventArgs e, LightweightControl lightweightControl)
        {
            //	Map the client point to a virtual point.
            Point virtualPoint = lightweightControl.PointToVirtualClient(new Point(e.X, e.Y));

            //	Return a new MouseEventArgs with the translated point.
            return new MouseEventArgs(e.Button, e.Clicks, virtualPoint.X, virtualPoint.Y, e.Delta);
        }

        /// <summary>
        /// Converts a client point to a virtual client point based on the scroll position.
        /// </summary>
        /// <param name="clientPoint">Client point.</param>
        /// <returns>Virtual point.</returns>
        private Point ClientPointToVirtualClientPoint(Point clientPoint)
        {
            clientPoint.Offset(AutoScrollPosition.X * -1, AutoScrollPosition.Y * -1);
            return clientPoint;
        }

        /// <summary>
        /// Converts a virtual client point to a client point based on the scroll position.
        /// </summary>
        /// <param name="virtualClientPoint">The virtual client point to convert to a client point.</param>
        /// <returns>The client point.</returns>
        private Point VirtualClientPointToClientPoint(Point virtualClientPoint)
        {
            virtualClientPoint.Offset(AutoScrollPosition.X, AutoScrollPosition.Y);
            return virtualClientPoint;
        }

        /// <summary>
        ///	Converts a client rectangle to a virtual client rectangle based on the scroll position.
        /// </summary>
        /// <param name="clientRectangle"></param>
        /// <returns></returns>
        private Rectangle ClientRectangleToVirtualClientRectangle(Rectangle clientRectangle)
        {
            clientRectangle.Offset(AutoScrollPosition.X * -1, AutoScrollPosition.Y * -1);
            return clientRectangle;
        }

        /// <summary>
        /// Converts a virtual client rectangle to a client rectangle based on the scroll position.
        /// </summary>
        /// <param name="virtualClientRectangle">virtual client rectangle to convert.</param>
        /// <returns>Converted virtual client rectangle.</returns>
        private Rectangle VirtualClientRectangleToClientRectangle(Rectangle virtualClientRectangle)
        {
            virtualClientRectangle.Offset(AutoScrollPosition.X, AutoScrollPosition.Y);
            return virtualClientRectangle;
        }

        /// <summary>
        /// Manages drag-and-drop auto-scroll.
        /// </summary>
        /// <param name="e">The DragEventArgs that this method processes.</param>
        private void ManageDragDropAutoScroll(DragEventArgs e)
        {
            //	If drag-and-drop auto-scroll is disabled, do nothing.
            if (!AllowDragDropAutoScroll)
                return;

            //	Determine the drag-and-drop point in client coordinates.
            Point dragDropPoint = PointToClient(new Point(e.X, e.Y));

            //	If drag-and-drop auto-scroll mode is Up or Down, and the mouse has left the
            //	drag-and-drop auto-scroll rectangle established when drag-and-drop auto-scroll
            //	was started, or auto-scroll has reached it's end, stop drag-and-drop auto-scroll.
            if (dragDropAutoScrollMode != DragDropAutoScrollMode.Off)
            {
                if (!dragDropAutoScrollRectangle.Contains(dragDropPoint) ||
                    (dragDropAutoScrollMode == DragDropAutoScrollMode.Up && ScrolledToTop) ||
                    (dragDropAutoScrollMode == DragDropAutoScrollMode.Down && ScrolledToBottom))
                    StopDragDropAutoScroll();
            }

            //	Start drag-and-drop auto-scroll, if we should.
            if (dragDropAutoScrollMode == DragDropAutoScrollMode.Off)
            {
                //	If the mouse is positioned in the top or bottom drag-and-drop auto-scroll
                //	rectangle, start to drag-and-drop auto-scroll up or down.
                if (TopDragDropAutoScrollRectangle.Contains(dragDropPoint) && !ScrolledToTop)
                    StartDragDropAutoScrollUp();
                else if (BottomDragDropAutoScrollRectangle.Contains(dragDropPoint) && !ScrolledToBottom)
                    StartDragDropAutoScrollDown();
            }
        }

        /// <summary>
        /// Starts drag-and-drop auto-scroll in the Up direction.
        /// </summary>
        private void StartDragDropAutoScrollUp()
        {
            //	Initialize drag-and-drop auto-scroll.
            InitializeDragDropAutoScroll();

            //	Set the drag-and-drop auto-scroll mode.
            dragDropAutoScrollMode = DragDropAutoScrollMode.Up;

            //	Set the drag-and-drop auto-scroll rectangle.
            dragDropAutoScrollRectangle = TopDragDropAutoScrollRectangle;

            //	Start the drag-and-drop auto-scroll timer.
            dragDropAutoScrollTimer.Start();
        }

        /// <summary>
        /// Starts drag-and-drop auto-scroll in the Down direction.
        /// </summary>
        private void StartDragDropAutoScrollDown()
        {
            //	Initialize drag-and-drop auto-scroll.
            InitializeDragDropAutoScroll();

            //	Set the drag-and-drop auto-scroll mode.
            dragDropAutoScrollMode = DragDropAutoScrollMode.Down;

            //	Set the drag-and-drop auto-scroll rectangle.
            dragDropAutoScrollRectangle = BottomDragDropAutoScrollRectangle;

            //	Raise the DragDropAutoScrollBegin event.
            OnDragDropAutoScrollBegin(EventArgs.Empty);

            //	Start the drag-and-drop auto-scroll timer.
            dragDropAutoScrollTimer.Start();
        }

        /// <summary>
        /// Helper to perform command drag-and-drop auto-scroll initialization.  This method is to
        /// be called only by StartDragDropAutoScrollUp or StartDragDropAutoScrollDown.
        /// </summary>
        private void InitializeDragDropAutoScroll()
        {
            //	Stop or instantiate the drag-and-drop auto-scroll timer.  The initial interval
            //	is large.  The user signals that they would like auto-scroll by not moving the
            //	mouse outside the auto-scroll rectangle this time period.
            if (dragDropAutoScrollTimer != null)
                StopDragDropAutoScroll();
            else
            {
                //	Instantiate the drag-and-drop auto-scroll timer.
                dragDropAutoScrollTimer = new Timer(this.components);
                dragDropAutoScrollTimer.Tick += new EventHandler(dragDropAutoScrollTimer_Tick);
            }

            //	Set the drag-and-drop auto-scroll timer interval.
            dragDropAutoScrollTimer.Interval = DRAG_AND_DROP_AUTO_SCROLL_INITIAL_INTERVAL;
        }

        /// <summary>
        /// Stops drag-and-drop auto-scroll.
        /// </summary>
        private void StopDragDropAutoScroll()
        {
            if (dragDropAutoScrollMode != DragDropAutoScrollMode.Off)
            {
                //	Stop the drag-and-drop auto-scroll timer.
                dragDropAutoScrollTimer.Stop();

                //	Set the drag-and-drop auto-scroll mode to Off.
                dragDropAutoScrollMode = DragDropAutoScrollMode.Off;

                //	Raise the DragDropAutoScrollEnd event.
                OnDragDropAutoScrollEnd(EventArgs.Empty);
            }
        }

        /// <summary>
        /// dragDropAutoScrollTimer_Tick event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void dragDropAutoScrollTimer_Tick(object sender, EventArgs e)
        {
            //	If this is the initial timer tick, switch the interval and note where the mouse
            //	was positioned so that we can use horizontal mouse movement as gestures that
            //	control auto-scroll speed and direction.
            if (dragDropAutoScrollTimer.Interval == DRAG_AND_DROP_AUTO_SCROLL_INITIAL_INTERVAL)
            {
                //	Switch the interval and note the mouse position.
                dragDropAutoScrollTimer.Stop();
                dragDropAutoScrollTimer.Interval = DRAG_AND_DROP_AUTO_SCROLL_INTERVAL;
                dragDropAutoScrollTimer.Start();
                dragDropAutoScrollPoint = PointToClient(MousePosition);

                //	Raise the DragDropAutoScrollBegin event.
                OnDragDropAutoScrollBegin(EventArgs.Empty);
            }

            //	Obtain the mouse position for this tick.
            Point mousePosition = PointToClient(MousePosition);

            //	Calculate the drag-and-drop auto-scroll vertical mouse position delta (how far
            //	the mouse has moved up or down since the auto-scroll started).
            int pixels;
            if (dragDropAutoScrollMode == DragDropAutoScrollMode.Up)
            {
                pixels = -Math.Max(1, (DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT - mousePosition.Y) / 5);
            }
            else if (dragDropAutoScrollMode == DragDropAutoScrollMode.Down)
            {
                pixels = Math.Max(1, (mousePosition.Y - (Height - DRAG_AND_DROP_AUTO_SCROLL_RECTANGLE_HEIGHT)) / 5);
            }
            else
            {
                Debug.Fail("Unrecognized DragDropAutoScrollMode");
                return;
            }

            //	Adjust the AutoScrollPosition.
            AutoScrollPosition = new Point(Math.Abs(AutoScrollPosition.X), Math.Abs(AutoScrollPosition.Y) + pixels);
        }

        /// <summary>
        /// Private helper to raise an EventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, EventArgs e)
        {
            EventHandler eventHandler = (EventHandler)Events[eventKey];
            if (eventHandler != null)
                eventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise an EventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A ShowContextMenuEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, ShowContextMenuEventArgs e)
        {
            ShowContextMenuEventHandler showContextMenuEventHandler = (ShowContextMenuEventHandler)Events[eventKey];
            if (showContextMenuEventHandler != null)
                showContextMenuEventHandler(this, e);
        }

        #endregion Private Methods

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new LightweightControlContainerAccessibility(this);
        }

        public class LightweightControlContainerAccessibility : Control.ControlAccessibleObject
        {
            private LightweightControlContainerControl _containerControl;

            public LightweightControlContainerAccessibility(LightweightControlContainerControl containerControl) : base(containerControl)
            {
                _containerControl = containerControl;
            }

            public override AccessibleObject GetChild(int index)
            {
                if (_containerControl._focusAndAccessibilityController != null)
                    return _containerControl._focusAndAccessibilityController.GetAccessibleObject(index);
                else
                    return base.GetChild(index);
            }

            public override int GetChildCount()
            {
                if (_containerControl._focusAndAccessibilityController != null)
                    return _containerControl._focusAndAccessibilityController.Count;
                else
                    return base.GetChildCount();
            }

        }
    }
}
