// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// The LightweightControl class provides a windowless control that behaves very much like a
    /// standard .NET UserControl, but is based on a .NET component.  LightweightControl serves
    /// as the base class for lightweight controls throughout the system.
    /// </summary>
    public class LightweightControl : Component, ISupportInitialize, ILightweightControlContainerControl
    {
        #region Private Member Variables

        /// <summary>
        /// The initialization count, used to implement ISupportInitialize.
        /// </summary>
        private int initializationCount = 0;

        /// <summary>
        /// The lightweight control container for this lightweight control.  This will be either
        /// another lightweight control or a heavyweight control that is designed to contain
        /// lightweight controls through an implementation of the ILightweightControlContainer
        /// interface.
        /// </summary>
        private ILightweightControlContainerControl lightweightControlContainerControl;

        /// <summary>
        /// The the virtual location of the lightweight control relative to the upper-left corner
        /// of its lightweight control container.
        /// </summary>
        private Point virtualLocation;

        /// <summary>
        /// The virtual size of the lightweight control.
        /// </summary>
        private Size virtualSize;

        /// <summary>
        /// The collection of lightweight controls contained within the lightweight control.
        /// </summary>
        private LightweightControlCollection lightweightControls;

        /// <summary>
        /// The child lightweight control that is the current active focus.
        /// </summary>
        private LightweightControl activeLightweightControl;

        /// <summary>
        /// A value indicating whether this control can be focused via the tab key.
        /// </summary>
        private bool tabStop = false;

        /// <summary>
        /// A value indicating whether the lightweight control is displayed.
        /// </summary>
        private bool visible = true;

        /// <summary>
        /// Gets or sets the shortcut menu associated with the control.
        /// </summary>
        private ContextMenu contextMenu;

        /// <summary>
        /// A value indicating whether the lightweight control can accept data that the user drags onto it.
        /// </summary>
        private bool allowDrop = false;

        /// <summary>
        /// A value indicating whether the lightweight control can accept MouseWheel events.
        /// </summary>
        private bool allowMouseWheel = false;

        /// <summary>
        /// The suspend layout state of the lightweight control.
        /// </summary>
        private int suspendLayoutCount = 0;

        #endregion Private Member Variables

        #region Public Events

        /// <summary>
        /// The Click event key.
        /// </summary>
        private static readonly object ClickEventKey = new object();

        /// <summary>
        /// Occurs when the control is clicked.
        /// </summary>
        [
            Category("Action"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the control is clicked.")
        ]
        public event EventHandler Click
        {
            add
            {
                Events.AddHandler(ClickEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(ClickEventKey, value);
            }
        }

        /// <summary>
        /// The DoubleClick event key.
        /// </summary>
        private static readonly object DoubleClickEventKey = new object();

        /// <summary>
        /// Occurs when the control is double-clicked.
        /// </summary>
        [
            Category("Action"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the control is double-clicked.")
        ]
        public event EventHandler DoubleClick
        {
            add
            {
                Events.AddHandler(DoubleClickEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(DoubleClickEventKey, value);
            }
        }

        /// <summary>
        /// The DragDrop event key.
        /// </summary>
        private static readonly object DragDropEventKey = new object();

        /// <summary>
        /// Occurs when a drag-and-drop operation is completed.
        /// </summary>
        [
            Category("Drag Drop"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when a drag-and-drop operation is completed.")
        ]
        public event DragEventHandler DragDrop
        {
            add
            {
                Events.AddHandler(DragDropEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(DragDropEventKey, value);
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
            Category("Drag Drop"),
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
        /// Occurs when an object is dragged out of the control's bounds.
        /// </summary>
        [
            Category("Drag Drop"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when an object is dragged out of the control's bounds.")
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
        /// The DragOver event key.
        /// </summary>
        private static readonly object DragOverEventKey = new object();

        /// <summary>
        /// Occurs when an object is dragged over the control's bounds.
        /// </summary>
        [
            Category("Drag Drop"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when an object is dragged over the control's bounds.")
        ]
        public event DragEventHandler DragOver
        {
            add
            {
                Events.AddHandler(DragOverEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(DragOverEventKey, value);
            }
        }

        /// <summary>
        /// The GiveFeedback event key.
        /// </summary>
        private static readonly object GiveFeedbackEventKey = new object();

        /// <summary>
        /// Occurs when the mouse drags an item.  The system requests that the lightweight control
        /// provide feedback to that effect.
        /// </summary>
        [
            Category("Drag Drop"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse drags an item.  The system requests that the lightweight control provide feedback to that effect.")
        ]
        public event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                Events.AddHandler(GiveFeedbackEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(GiveFeedbackEventKey, value);
            }
        }

        /// <summary>
        /// The KeyDown event key.
        /// </summary>
        private static readonly object KeyDownEventKey = new object();

        /// <summary>
        /// Occurs when a key is pressed while the control has focus.
        /// </summary>
        [
            Category("Key"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when a key is first pressed.")
        ]
        public event KeyEventHandler KeyDown
        {
            add
            {
                Events.AddHandler(KeyDownEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(KeyDownEventKey, value);
            }
        }

        /// <summary>
        /// The KeyPress event key.
        /// </summary>
        private static readonly object KeyPressEventKey = new object();

        /// <summary>
        /// Occurs when a key is pressed while the lightweight control has focus.
        /// </summary>
        [
            Category("Key"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs after the user is finished pressing a key.")
        ]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                Events.AddHandler(KeyPressEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(KeyPressEventKey, value);
            }
        }

        /// <summary>
        /// The KeyUp event key.
        /// </summary>
        private static readonly object KeyUpEventKey = new object();

        /// <summary>
        /// Occurs when a key is released while the control has focus.
        /// </summary>
        [
            Category("Key"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when a key is released.")
        ]
        public event KeyEventHandler KeyUp
        {
            add
            {
                Events.AddHandler(KeyUpEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(KeyUpEventKey, value);
            }
        }

        /// <summary>
        /// The Layout event key.
        /// </summary>
        private static readonly object LayoutEventKey = new object();

        /// <summary>
        /// Occurs when a control should reposition its child controls.
        /// </summary>
        [
            Category("Layout"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when a control should reposition its child controls.")
        ]
        public event EventHandler Layout
        {
            add
            {
                Events.AddHandler(LayoutEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(LayoutEventKey, value);
            }
        }

        /// <summary>
        /// The LightweightControlContainerControlChanged event key.
        /// </summary>
        private static readonly object LightweightControlContainerControlChangedEventKey = new object();

        /// <summary>
        /// Occurs when the LightweightControlContainerControl property changes.
        /// </summary>
        [
            Category("Property Changed"),
                DefaultValue(null),
                Localizable(false),
                Description("Event fired when the value of the LightweightControlContainerControl property is changed on the LightweightControl.")
        ]
        public event EventHandler LightweightControlContainerControlChanged
        {
            add
            {
                Events.AddHandler(LightweightControlContainerControlChangedEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(LightweightControlContainerControlChangedEventKey, value);
            }
        }

        /// <summary>
        /// The LightweightControlContainerControlVirtualLocationChanged event key.
        /// </summary>
        private static readonly object LightweightControlContainerControlVirtualLocationChangedEventKey = new object();

        /// <summary>
        /// Occurs when the VirtualLocation property of a LightweightControlContainerControl in the
        /// lightweight control containment hierarchy changes.
        /// </summary>
        [
            Category("Property Changed"),
                DefaultValue(null),
                Localizable(false),
                Description("Event fired when the value of the LightweightControlContainerControl property is changed on the LightweightControl.")
        ]
        public event EventHandler LightweightControlContainerControlVirtualLocationChanged
        {
            add
            {
                Events.AddHandler(LightweightControlContainerControlChangedEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(LightweightControlContainerControlChangedEventKey, value);
            }
        }

        /// <summary>
        /// The MouseDown event key.
        /// </summary>
        private static readonly object MouseDownEventKey = new object();

        /// <summary>
        /// Occurs when the mouse pointer is over the control and a mouse button is pressed.
        /// </summary>
        [
            Category("Mouse"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse pointer is over the control and a mouse button is pressed.")
        ]
        public event MouseEventHandler MouseDown
        {
            add
            {
                Events.AddHandler(MouseDownEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(MouseDownEventKey, value);
            }
        }

        /// <summary>
        /// The MouseEnter event key.
        /// </summary>
        private static readonly object MouseEnterEventKey = new object();

        /// <summary>
        /// Occurs when the mouse pointer enters the control.
        /// </summary>
        [
            Category("Mouse"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse pointer enters the control.")
        ]
        public event EventHandler MouseEnter
        {
            add
            {
                Events.AddHandler(MouseEnterEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(MouseEnterEventKey, value);
            }
        }

        /// <summary>
        /// The MouseEnter event key.
        /// </summary>
        private static readonly object MouseHoverEventKey = new object();

        /// <summary>
        /// Occurs when the mouse pointer hovers over the control.
        /// </summary>
        [
            Category("Mouse"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse pointer hovers over the control.")
        ]
        public event EventHandler MouseHover
        {
            add
            {
                Events.AddHandler(MouseHoverEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(MouseHoverEventKey, value);
            }
        }

        /// <summary>
        /// The MouseLeave event key.
        /// </summary>
        private static readonly object MouseLeaveEventKey = new object();

        /// <summary>
        /// Occurs when the mouse pointer leaves the control.
        /// </summary>
        [
            Category("Mouse"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse pointer leaves the control.")
        ]
        public event EventHandler MouseLeave
        {
            add
            {
                Events.AddHandler(MouseLeaveEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(MouseLeaveEventKey, value);
            }
        }

        /// <summary>
        /// The MouseMove event key.
        /// </summary>
        private static readonly object MouseMoveEventKey = new object();

        /// <summary>
        /// Occurs when the mouse pointer is moved over the control.
        /// </summary>
        [
            Category("Mouse"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse pointer is moved over the control.")
        ]
        public event MouseEventHandler MouseMove
        {
            add
            {
                Events.AddHandler(MouseMoveEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(MouseMoveEventKey, value);
            }
        }

        /// <summary>
        /// The MouseUp event key.
        /// </summary>
        private static readonly object MouseUpEventKey = new object();

        /// <summary>
        /// Occurs when the mouse pointer is over the control and a mouse button is released.
        /// </summary>
        [
            Category("Mouse"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse pointer is over the control and a mouse button is released.")
        ]
        public event MouseEventHandler MouseUp
        {
            add
            {
                Events.AddHandler(MouseUpEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(MouseUpEventKey, value);
            }
        }

        /// <summary>
        /// The MouseWheel event key.
        /// </summary>
        private static readonly object MouseWheelEventKey = new object();

        /// <summary>
        /// Occurs when the mouse wheel moves while the control has focus.
        /// </summary>
        [
            Category("Mouse"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the mouse wheel moves while the control has focus.")
        ]
        public event MouseEventHandler MouseWheel
        {
            add
            {
                Events.AddHandler(MouseWheelEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(MouseWheelEventKey, value);
            }
        }

        /// <summary>
        /// The Paint event key.
        /// </summary>
        private static readonly object PaintEventKey = new object();

        /// <summary>
        /// Occurs when the control is redrawn.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the control is redrawn.")
        ]
        public event PaintEventHandler Paint
        {
            add
            {
                Events.AddHandler(PaintEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(PaintEventKey, value);
            }
        }

        /// <summary>
        /// The QueryContinueDrag event key.
        /// </summary>
        private static readonly object QueryContinueDragEventKey = new object();

        /// <summary>
        /// Occurs during a drag-and-drop operation and allows the drag source to determine whether
        /// the drag-and-drop operation should be canceled.
        /// </summary>
        [
            Category("Drag Drop"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs during a drag-and-drop operation and allows the drag source to determine whether the drag-and-drop operation should be canceled.")
        ]
        public event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                Events.AddHandler(QueryContinueDragEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(QueryContinueDragEventKey, value);
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

        /// <summary>
        /// The VisibleChanged event key.
        /// </summary>
        private static readonly object VisibleChangedEventKey = new object();

        /// <summary>
        /// Occurs when the VirtualSize property is changed.
        /// </summary>
        [
            Category("Property Changed"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the Visible property is changed.")
        ]
        public event EventHandler VisibleChanged
        {
            add
            {
                Events.AddHandler(VisibleChangedEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(VisibleChangedEventKey, value);
            }
        }

        /// <summary>
        /// The VirtualLocation event key.
        /// </summary>
        private static readonly object VirtualLocationChangedEventKey = new object();

        /// <summary>
        /// Occurs when the VirtualLocation property is changed.
        /// </summary>
        [
            Category("Property Changed"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the VirtualLocation property is changed.")
        ]
        public event EventHandler VirtualLocationChanged
        {
            add
            {
                Events.AddHandler(VirtualLocationChangedEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(VirtualLocationChangedEventKey, value);
            }
        }

        /// <summary>
        /// The VirtualSizeChanged event key.
        /// </summary>
        private static readonly object VirtualSizeChangedEventKey = new object();

        /// <summary>
        /// Occurs when the VirtualSize property is changed.
        /// </summary>
        [
            Category("Property Changed"),
                DefaultValue(null),
                Localizable(false),
                Description("Occurs when the VirtualSize property is changed.")
        ]
        public event EventHandler VirtualSizeChanged
        {
            add
            {
                Events.AddHandler(VirtualSizeChangedEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(VirtualSizeChangedEventKey, value);
            }
        }

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new insatnce of the LightweightControl class.
        /// </summary>
        public LightweightControl(IContainer container)
        {
            //	Required for Windows.Forms Class Composition Designer support
            container.Add(this);
            InitializeComponent();

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new insatnce of the LightweightControl class.
        /// </summary>
        public LightweightControl()
        {
            //	Required for Windows.Forms Class Composition Designer support
            InitializeComponent();

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the TwistieLightweightControl class.
        /// </summary>
        private void InitializeObject()
        {
            //	Instantiate the lightweight control collection.
            lightweightControls = new LightweightControlCollection(this);

            //	Set the default virtual size.
            VirtualSize = DefaultVirtualSize;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            new Container();
        }

        #endregion Class Initialization & Termination

        #region Public Properties
        /// <summary>
        /// Gets or sets a value indicating whether the lightweight control is a tab stop.
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        Localizable(false),
        Description("Specifies whether the lightweight control can accept focus when tabbing.")
        ]
        public bool TabStop
        {
            get { return tabStop; }
            set { tabStop = value; }
        }

        /// <summary>
        /// Gets or sets ActiveLightweightControl
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
                if (value != null && LightweightControlContainerControl.ActiveLightweightControl != this)
                    LightweightControlContainerControl.ActiveLightweightControl = this;
            }
        }

        /// <summary>
        /// Gets or sets the the lightweight control container for this lightweight control.
        /// </summary>
        [
            Category("Application"),
                Localizable(false),
                Description("Specifies the lightweight control container for this lightweight control.")
        ]
        public ILightweightControlContainerControl LightweightControlContainerControl
        {
            get
            {
                return lightweightControlContainerControl;
            }
            set
            {
                //	If the lightweight control container is changing, change it.
                if (lightweightControlContainerControl != value)
                {
                    //	If this control has a current lightweight control container, remove it from
                    //	the lightweight control container's lightweight control collection.
                    if (lightweightControlContainerControl != null && lightweightControlContainerControl.LightweightControls.Contains(this))
                        lightweightControlContainerControl.LightweightControls.Remove(this);

                    //	Set the new lightweight control container.
                    lightweightControlContainerControl = value;

                    //	If we have a new lightweight control container (i.e. it is not being set to
                    //	null), add this control to the lightweight control container's lightweight
                    //	control collection (if it has not already been added, which happens when
                    //	this the control is added through the container's LightweightControls
                    //	property).
                    if (lightweightControlContainerControl != null && !lightweightControlContainerControl.LightweightControls.Contains(this))
                        lightweightControlContainerControl.LightweightControls.Add(this);

                    //	Raise the LightweightControlContainerControlChanged event.
                    OnLightweightControlContainerControlChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the virtual location of the lightweight control relative to the upper-left
        /// corner of its lightweight control container.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
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
                    if (lightweightControlContainerControl != null)
                        lightweightControlContainerControl.PerformLayout();

                    //	Finally, raise the VirtualLocationChanged event.
                    OnVirtualLocationChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Size VirtualSize
        {
            get
            {
                return virtualSize;
            }
            set
            {
                //	If the virtual size has changed, change it.
                if (virtualSize != value)
                {
                    //	Set the virtual size.
                    virtualSize = value;

                    //	Layout the control.
                    PerformLayout();

                    //	If we have a lightweight control container, have it perform a layout.
                    if (lightweightControlContainerControl != null)
                        lightweightControlContainerControl.PerformLayout();

                    //	Finally, raise the VirtualSizeChanged event.
                    OnVirtualSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
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
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
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
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
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
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
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
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
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
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Rectangle VirtualClientRectangle
        {
            get
            {
                return new Rectangle(0, 0, VirtualSize.Width, VirtualSize.Height);
            }
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Gets the collection of lightweight controls contained within the lightweight control.
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
        /// Adjusts the position of all lightweight child controls to
        /// be horizontally mirrored if Writer is running in right-to-left
        /// mode.
        /// </summary>
        /// <param name="recursive">Recursively call RtlLayoutFixup on the whole child lightweight control hierarchy.</param>
        public void RtlLayoutFixup(bool recursive)
        {
            if (BidiHelper.IsRightToLeft)
            {
                foreach (LightweightControl lc in LightweightControls)
                {
                    lc.VirtualLocation = new Point(
                        VirtualClientRectangle.Width - lc.VirtualBounds.Right,
                        lc.VirtualLocation.Y);
                    if (recursive)
                        lc.RtlLayoutFixup(true);
                }
            }
        }

        /// <summary>
        /// Gets the parent control for the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Control Parent
        {
            get
            {
                //	Work up the chain of lightweight control containers until we find the
                //	heavyweight control that ultimately contains this lightweight control.
                if (lightweightControlContainerControl == null)
                    return null;
                else if (lightweightControlContainerControl is LightweightControlContainerControl)
                    return (LightweightControlContainerControl)lightweightControlContainerControl;
                else
                    return lightweightControlContainerControl.Parent;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the lightweight control is displayed.
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(true),
                Localizable(false),
                Description("Specifies whether the lightweight control is displayed.")
        ]
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
        /// The shortcut menu associated with the control.
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(null),
                Localizable(false),
                Description("The shortcut menu to display when the user right-clicks the control.")
        ]
        public virtual ContextMenu ContextMenu
        {
            get
            {
                return contextMenu;
            }
            set
            {
                contextMenu = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the lightweight control can accept data that
        /// the user drags onto it.
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(false),
                Localizable(false),
                Description("Specifies whether the lightweight control can accept data that the user drags onto it.")
        ]
        public bool AllowDrop
        {
            get
            {
                return allowDrop;
            }
            set
            {
                if (allowDrop != value)
                    allowDrop = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the lightweight control can accept MouseWheel events.
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(false),
                Localizable(false),
                Description("Specifies whether the lightweight control can accept MouseWheel events.")
        ]
        public bool AllowMouseWheel
        {
            get
            {
                return allowMouseWheel;
            }
            set
            {
                if (allowMouseWheel != value)
                    allowMouseWheel = value;
            }
        }

        /// <summary>
        /// Gets the suspend layout state of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected bool IsLayoutSuspended
        {
            get
            {
                return Interlocked.CompareExchange(ref suspendLayoutCount, 0, 0) != 0;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Begins initialization.
        /// </summary>
        public void BeginInit()
        {
            if (++initializationCount == 1)
                SuspendLayout();
        }

        /// <summary>
        /// Ends initialization.
        /// </summary>
        public void EndInit()
        {
            if (--initializationCount == 0)
                ResumeLayout();
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Temporarily suspends the layout logic for the control.
        /// </summary>
        public void SuspendLayout()
        {
            Interlocked.Increment(ref suspendLayoutCount);
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Resumes normal layout logic.
        /// </summary>
        public void ResumeLayout()
        {
            Interlocked.Decrement(ref suspendLayoutCount);
        }

        /// <summary>
        /// Invalidates the lightweight control.
        /// </summary>
        public void Invalidate()
        {
            if (Parent != null)
                Parent.Invalidate(VirtualClientRectangleToParent());
        }

        /// <summary>
        /// Causes the lightweight control to redraw its client area.
        /// </summary>
        public void Update()
        {
            if (Parent != null)
                Parent.Update();
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Forces the control to apply layout logic to child controls.
        /// </summary>
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

        /// <summary>
        /// Makes the lightweight control visible in the specified direction.
        /// </summary>
        /// <param name="visibleDirection">The direction in which to make the lightweight control visible.</param>
        public void MakeVisible(VisibleDirection visibleDirection)
        {
            Control parent = Parent;
            if (parent != null && parent is LightweightControlContainerControl)
                ((LightweightControlContainerControl)Parent).MakeClientRectangleVisible(VirtualClientRectangleToParent(), visibleDirection);
        }

        /// <summary>
        /// Returns a value indicating whether the lightweight control is visible in the specified direction.
        /// </summary>
        /// <param name="visibleDirection">The direction in which to determine whether the lightweight control visible.</param>
        public bool IsVisible(VisibleDirection visibleDirection)
        {
            Control parent = Parent;
            if (parent != null && parent is LightweightControlContainerControl)
                return ((LightweightControlContainerControl)Parent).IsClientRectangleVisible(VirtualClientRectangleToParent(), visibleDirection);
            return false;
        }

        /// <summary>
        /// Brings the lightweight control to the front of the z-order.
        /// </summary>
        public void BringToFront()
        {
            if (LightweightControlContainerControl != null)
                LightweightControlContainerControl.LightweightControls.BringToFront(this);
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
            if (lightweightControlContainerControl == null)
                return rectangle;
            else
                return lightweightControlContainerControl.VirtualClientRectangleToParent(rectangle);
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
            if (lightweightControlContainerControl == null)
                return point;
            else
                return lightweightControlContainerControl.PointToVirtualClient(point);
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
            if (lightweightControlContainerControl == null)
                return point;
            else
                return lightweightControlContainerControl.VirtualClientPointToParent(point);
        }

        /// <interface>ILightweightControlContainer</interface>
        /// <summary>
        /// Translates a virtual client point to be relative to a parent point.
        /// </summary>
        /// <param name="point">The point to translate.</param>
        /// <returns>Translated point.</returns>
        public Point VirtualClientPointToScreen(Point point)
        {
            //	Translate the point to be relative to the virtual location of this lightweight
            //	control.
            point.Offset(VirtualLocation.X, VirtualLocation.Y);

            //	Continue up the chain of lightweight control container controls.
            if (lightweightControlContainerControl == null)
                return point;
            else
                return lightweightControlContainerControl.VirtualClientPointToScreen(point);
        }

        /// <summary>
        /// Sets the tooltip.
        /// </summary>
        /// <param name="toolTipText">The tooltip text to set.</param>
        public void SetToolTip(string toolTipText)
        {
            if (Parent != null && Parent is LightweightControlContainerControl)
                LightweightControlContainerControl.SetToolTip(toolTipText);
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Gets the lightweight control that is located at the specified point.
        /// </summary>
        /// <param name="point">A Point that contains the coordinates where you want to look for a control.
        ///	Coordinates are expressed relative to the upper-left corner of the control's client area.</param>
        /// <returns>Lightweight control at the specified point.</returns>
        internal LightweightControl GetLightweightControlAtPoint(Point point)
        {
            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = lightweightControls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                LightweightControl lightweightControl = lightweightControls[index];

                //	If the lightweight control is visible, and the point is within its virtual
                //	bounds, then recursively search for the right control.
                if (lightweightControl.Visible && lightweightControl.VirtualBounds.Contains(point))
                {
                    //	Translate the point to be relative to the location of the lightweight control.
                    point.Offset(lightweightControl.VirtualLocation.X * -1, lightweightControl.VirtualLocation.Y * -1);

                    //	Recursively search for the right lightweight control.
                    return lightweightControl.GetLightweightControlAtPoint(point);
                }
            }

            //	The point is not inside a child lightweight control of this lightweight control,
            //	so it's inside this lightweight control.
            return this;
        }

        /// <summary>
        /// Gets the lightweight control that is located at the specified point and allows drag and drop events.
        /// </summary>
        /// <param name="point">A Point that contains the coordinates where you want to look for a control.
        ///	Coordinates are expressed relative to the upper-left corner of the control's client area.</param>
        /// <returns>Lightweight control at the specified point.</returns>
        internal LightweightControl GetDragDropLightweightControlAtPoint(Point point)
        {
            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = lightweightControls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                LightweightControl lightweightControl = lightweightControls[index];

                //	If the lightweight control is visible, and the point is within its virtual
                //	bounds, then recursively call it sit is under the point.
                if (lightweightControl.Visible && lightweightControl.VirtualBounds.Contains(point))
                {
                    //	Translate the point to be relative to the location of the lightweight control.
                    point.Offset(lightweightControl.VirtualLocation.X * -1, lightweightControl.VirtualLocation.Y * -1);

                    //	Recursively search for the right drag and drop lightweight control.
                    return lightweightControl.GetDragDropLightweightControlAtPoint(point);
                }
            }

            //	The point is not inside a child lightweight control of this lightweight control
            //	that allows mouse wheel events.  So it's inside this lightweight control.  If this
            //	lightweight control allows mouse wheel events, return it.  Otherwise, return null.
            return AllowDrop ? this : null;
        }

        /// <summary>
        /// Gets the lightweight control that is located at the specified point and allows mouse wheel events.
        /// </summary>
        /// <param name="point">A Point that contains the coordinates where you want to look for a control.
        ///	Coordinates are expressed relative to the upper-left corner of the control's client area.</param>
        /// <returns>Lightweight control at the specified point.</returns>
        internal LightweightControl GetMouseWheelLightweightControlAtPoint(Point point)
        {
            //	Enumerate the lightweight controls, in Z order, to locate one at the virtual point.
            for (int index = lightweightControls.Count - 1; index >= 0; index--)
            {
                //	Access the lightweight control.
                LightweightControl lightweightControl = lightweightControls[index];

                //	If the lightweight control is visible, and the point is within its virtual
                //	bounds, then recursively call it sit is under the point.
                if (lightweightControl.Visible && lightweightControl.VirtualBounds.Contains(point))
                {
                    //	Translate the point to be relative to the location of the lightweight control.
                    point.Offset(lightweightControl.VirtualLocation.X * -1, lightweightControl.VirtualLocation.Y * -1);

                    //	Recursively search for the right mouse wheel lightweight control.
                    LightweightControl childMouseWheelLightweightControl = lightweightControl.GetMouseWheelLightweightControlAtPoint(point);
                    if (childMouseWheelLightweightControl != null)
                        return childMouseWheelLightweightControl;
                }
            }

            //	The point is not inside a child lightweight control of this lightweight control
            //	that allows mouse wheel events.  So it's inside this lightweight control.  If this
            //	lightweight control allows mouse wheel events, return it.  Otherwise, return null.
            return AllowMouseWheel ? this : null;
        }

        #endregion Internal Methods

        #region Internal Event Methods

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseClick(EventArgs e)
        {
            OnClick(e);
        }

        /// <summary>
        /// Raises the DoubleClick event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseDoubleClick(EventArgs e)
        {
            OnDoubleClick(e);
        }

        /// <summary>
        /// Raises the DragDrop event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        internal void RaiseDragDrop(DragEventArgs e)
        {
            OnDragDrop(e);
        }

#if false
        /// <summary>
        /// Raises the DragEnter event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        internal void RaiseDragEnter(DragEventArgs e)
        {
            OnDragEnter(e);
        }
#endif

        /// <summary>
        /// Raises the DragInside event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        internal void RaiseDragInside(DragEventArgs e)
        {
            OnDragInside(e);
        }

#if false
        /// <summary>
        /// Raises the DragLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseDragLeave(EventArgs e)
        {
            OnDragLeave(e);
        }
#endif

        /// <summary>
        /// Raises the DragOutside event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseDragOutside(EventArgs e)
        {
            OnDragOutside(e);
        }

        /// <summary>
        /// Raises the DragOver event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        internal void RaiseDragOver(DragEventArgs e)
        {
            OnDragOver(e);
        }

        /// <summary>
        /// Raises the GiveFeedback event.
        /// </summary>
        /// <param name="e">A GiveFeedbackEventArgs that contains the event data.</param>
        internal void RaiseGiveFeedback(GiveFeedbackEventArgs e)
        {
            OnGiveFeedback(e);
        }

        /// <summary>
        /// Raises the KeyDown event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        internal void RaiseKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        /// <summary>
        /// Raises the KeyPress event.
        /// </summary>
        /// <param name="e">A KeyPressEventArgs that contains the event data.</param>
        internal void RaiseKeyPress(KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        /// <summary>
        /// Raises the KeyUp event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        internal void RaiseKeyUp(KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseLayout(EventArgs e)
        {
            OnLayout(e);
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseLightweightControlContainerControlChanged(EventArgs e)
        {
            OnLightweightControlContainerControlChanged(e);
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlVirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseLightweightControlContainerControlVirtualLocationChanged(EventArgs e)
        {
            OnLightweightControlContainerControlVirtualLocationChanged(e);
        }

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
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        internal void RaisePaint(PaintEventArgs e)
        {
            OnPaint(e);
        }

        /// <summary>
        /// Raises the QueryContinueDrag event.
        /// </summary>
        /// <param name="e">A QueryContinueDragEventArgs that contains the event data.</param>
        internal void RaiseQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            OnQueryContinueDrag(e);
        }

        /// <summary>
        /// Raises the ShowContextMenu event.
        /// </summary>
        /// <param name="e">A ShowContextMenuEventArgs that contains the event data.</param>
        internal void RaiseShowContextMenu(ShowContextMenuEventArgs e)
        {
            OnShowContextMenu(e);
        }

        /// <summary>
        /// Raises the VisibleChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseVisibleChanged(EventArgs e)
        {
            OnVisibleChanged(e);
        }

        /// <summary>
        /// Raises the VirtualSizeChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseVirtualSizeChanged(EventArgs e)
        {
            OnVirtualSizeChanged(e);
        }

        /// <summary>
        /// Raises the VirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal void RaiseVirtualLocationChanged(EventArgs e)
        {
            //	Raise the event.
            OnVirtualLocationChanged(e);

            //	Raise the LightweightControlContainerControlVirtualLocationChanged event on each
            //	child control.
            foreach (LightweightControl lightweightControl in lightweightControls)
                lightweightControl.OnLightweightControlContainerControlVirtualLocationChanged(e);
        }

        #endregion

        #region Protected Events

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnClick(EventArgs e)
        {
            RaiseEvent(ClickEventKey, e);
        }

        /// <summary>
        /// Raises the DoubleClick event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnDoubleClick(EventArgs e)
        {
            RaiseEvent(DoubleClickEventKey, e);
        }

        /// <summary>
        /// Raises the DragDrop event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected virtual void OnDragDrop(DragEventArgs e)
        {
            RaiseEvent(DragDropEventKey, e);
        }

#if false
        /// <summary>
        /// Raises the DragEnter event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected virtual void OnDragEnter(DragEventArgs e)
        {
            RaiseEvent(DragEnterEventKey, e);
        }
#endif

        /// <summary>
        /// Raises the DragInside event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected virtual void OnDragInside(DragEventArgs e)
        {
            RaiseEvent(DragInsideEventKey, e);
        }

#if false
        /// <summary>
        /// Raises the DragLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnDragLeave(EventArgs e)
        {
            RaiseEvent(DragLeaveEventKey, e);
        }
#endif

        /// <summary>
        /// Raises the DragOutside event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnDragOutside(EventArgs e)
        {
            RaiseEvent(DragOutsideEventKey, e);
        }

        /// <summary>
        /// Raises the DragOver event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected virtual void OnDragOver(DragEventArgs e)
        {
            RaiseEvent(DragOverEventKey, e);
        }

        /// <summary>
        /// Raises the GiveFeedback event.
        /// </summary>
        /// <param name="e">A GiveFeedbackEventArgs that contains the event data.</param>
        protected virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            RaiseEvent(GiveFeedbackEventKey, e);
        }

        /// <summary>
        /// Raises the KeyDown event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            RaiseEvent(KeyDownEventKey, e);

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                DoDefaultAction();
            }
        }

        /// <summary>
        /// Raises the KeyPress event.
        /// </summary>
        /// <param name="e">A KeyPressEventArgs that contains the event data.</param>
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            RaiseEvent(KeyPressEventKey, e);
        }

        /// <summary>
        /// Raises the KeyUp event.
        /// </summary>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            RaiseEvent(KeyUpEventKey, e);
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnLayout(EventArgs e)
        {
            //Debug.WriteLine("OnLayout "+this.GetType().ToString());

            RaiseEvent(LayoutEventKey, e);
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnLightweightControlContainerControlChanged(EventArgs e)
        {
            RaiseEvent(LightweightControlContainerControlChangedEventKey, e);
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlVirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnLightweightControlContainerControlVirtualLocationChanged(EventArgs e)
        {
            RaiseEvent(LightweightControlContainerControlVirtualLocationChangedEventKey, e);

            //	Enumerate the child lightweight controls.  Raise the VirtualLocationChanged event
            //	on each one.
            foreach (LightweightControl lightweightControl in lightweightControls)
                lightweightControl.OnLightweightControlContainerControlVirtualLocationChanged(e);
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            RaiseEvent(MouseDownEventKey, e);
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseEnter(EventArgs e)
        {
            RaiseEvent(MouseEnterEventKey, e);
        }

        /// <summary>
        /// Raises the MouseHover event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseHover(EventArgs e)
        {
            RaiseEvent(MouseHoverEventKey, e);
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMouseLeave(EventArgs e)
        {
            RaiseEvent(MouseLeaveEventKey, e);
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            RaiseEvent(MouseMoveEventKey, e);
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            RaiseEvent(MouseUpEventKey, e);
        }

        /// <summary>
        /// Raises the MouseWheel event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            RaiseEvent(MouseWheelEventKey, e);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected virtual void OnPaint(PaintEventArgs e)
        {
            //	Raise the paint event to get this lightweight control painted first.
            RaiseEvent(PaintEventKey, e);

            //	Enumerate the child lightweight controls and paint controls that need painting.
            //	This enumeration occurs in reverse Z order.  Child controls lower in the Z order
            //	are painted before child controls higher in the Z order.
            foreach (LightweightControl lightweightControl in lightweightControls)
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
                    GraphicsContainer graphicsContainer = e.Graphics.BeginContainer(/*lightweightControl.VirtualBounds,
                                                                                    lightweightControlVirtualClientRectangle,
                                                                                    GraphicsUnit.Pixel*/);
                    e.Graphics.TranslateTransform(lightweightControl.VirtualBounds.Location.X, lightweightControl.VirtualBounds.Location.Y);

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
        /// Raises the ShowContextMenu event.
        /// </summary>
        /// <param name="e">A ShowContextMenuEventArgs that contains the event data.</param>
        protected virtual void OnShowContextMenu(ShowContextMenuEventArgs e)
        {
            RaiseEvent(ShowContextMenuEventKey, e);
        }

        /// <summary>
        /// Raises the QueryContinueDrag event.
        /// </summary>
        /// <param name="e">A QueryContinueDragEventArgs that contains the event data.</param>
        protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            RaiseEvent(QueryContinueDragEventKey, e);
        }

        /// <summary>
        /// Raises the VisibleChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVisibleChanged(EventArgs e)
        {
            RaiseEvent(VisibleChangedEventKey, e);
        }

        /// <summary>
        /// Raises the VirtualSizeChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVirtualSizeChanged(EventArgs e)
        {
            RaiseEvent(VirtualSizeChangedEventKey, e);
        }

        /// <summary>
        /// Raises the VirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVirtualLocationChanged(EventArgs e)
        {
            //	Raise the event.
            RaiseEvent(VirtualLocationChangedEventKey, e);

            //	Raise the LightweightControlContainerControlVirtualLocationChanged event on each
            //	child control.
            foreach (LightweightControl lightweightControl in lightweightControls)
                lightweightControl.OnLightweightControlContainerControlVirtualLocationChanged(e);
        }

        #endregion Protected Events

        #region Focus Support
        public LightweightControl GetFocusedControl()
        {
            LightweightControl focusedControl = ActiveLightweightControl;
            while (focusedControl != null && focusedControl.ActiveLightweightControl != null)
            {
                focusedControl = focusedControl.ActiveLightweightControl;
            }
            return focusedControl;
        }

        private bool _focused;
        public bool Focused
        {
            get { return _focused; }
        }

        public virtual bool Focus()
        {
            if (Visible)
            {
                LightweightControlContainerControl.ActiveLightweightControl = this;
                _focused = true;
                Control parent = Parent;
                if (parent is LightweightControlContainerControl)
                {
                    ((LightweightControlContainerControl)parent).NotifyControlFocused(this);
                }
                Invalidate();
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool Unfocus()
        {
            LightweightControl focusedControl = GetFocusedControl();
            if (focusedControl != null)
                focusedControl.Unfocus();
            _focused = false;
            if (LightweightControlContainerControl.ActiveLightweightControl == this)
                LightweightControlContainerControl.ActiveLightweightControl = null;

            Invalidate();
            return _focused;
        }
        #endregion

        #region Accessibility
        /// <summary>
        /// Returns a list of all the accessible controls contained in this control (includes decendents).
        /// </summary>
        /// <returns></returns>
        public LightweightControl[] GetAccessibleControls()
        {
            ArrayList list = new ArrayList();
            AddAccessibleControlsToList(list);
            return (LightweightControl[])list.ToArray(typeof(LightweightControl));
        }

        /// <summary>
        /// Updates the list of accessible controls.
        /// </summary>
        /// <param name="list"></param>
        protected virtual void AddAccessibleControlsToList(ArrayList list)
        {
            foreach (LightweightControl child in LightweightControls)
                child.AddAccessibleControlsToList(list);
        }

        public virtual bool DoDefaultAction()
        {
            return false;
        }

        protected string AccessibleDefaultAction
        {
            get { return accessibleDefaultAction; }
            set { accessibleDefaultAction = value; }
        }
        private string accessibleDefaultAction;

        public string AccessibleName
        {
            get { return _accessibleName; }
            set { _accessibleName = value; }
        }
        private string _accessibleName;

        protected AccessibleRole AccessibleRole
        {
            get { return _accessibleRole; }

            set { _accessibleRole = value; }
        }
        private AccessibleRole _accessibleRole = AccessibleRole.None;

        public string AccessibleValue
        {
            get { return _accessibleValue ?? null; }
            set { _accessibleValue = value; }
        }
        private string _accessibleValue;

        public string AccessibleKeyboardShortcut
        {
            get { return _accessibleKeyboardShortcut; }
            set { _accessibleKeyboardShortcut = value; }
        }
        private string _accessibleKeyboardShortcut;

        private AccessibleObject _accessibleObject;
        internal AccessibleObject AccessibilityObject
        {
            get
            {
                if (_accessibleObject == null)
                    _accessibleObject = CreateAccessibilityInstance();
                return _accessibleObject;
            }
        }
        protected virtual AccessibleObject CreateAccessibilityInstance()
        {
            return new LightweightControlAccessibleObject(this);
        }

        public class LightweightControlAccessibleObject : AccessibleObject
        {
            private readonly LightweightControl _control;
            public LightweightControlAccessibleObject(LightweightControl control)
            {
                _control = control;
            }

            public override string Name
            {
                get { return _control.AccessibleName; }
            }

            public override AccessibleRole Role
            {
                get { return _control.AccessibleRole; }
            }

            public override string KeyboardShortcut
            {
                get { return _control.AccessibleKeyboardShortcut; }
            }

            public override string Value
            {
                get { return _control.AccessibleValue; }
            }

            public override AccessibleObject GetFocused()
            {
                return _control.Focused ? this : null;
            }

            public override void DoDefaultAction()
            {
                _control.DoDefaultAction();
            }

            public override string DefaultAction
            {
                get
                {
                    return _control.AccessibleDefaultAction;
                }
            }

            public AccessibleStates? _accessibleStateOverride;
            public override AccessibleStates State
            {
                get
                {
                    if (_accessibleStateOverride != null)
                        return _accessibleStateOverride.Value;

                    AccessibleStates states = AccessibleStates.Focusable;
                    if (_control.Focused)
                        states = states | AccessibleStates.Focused;

                    if (!IsControlVisible(_control))
                        states = states | AccessibleStates.Invisible;

                    return states;
                }
            }

            public void SetAccessibleStateOverride(AccessibleStates state)
            {
                _accessibleStateOverride = state;
            }

            private bool IsControlVisible(LightweightControl control)
            {
                bool visible = _control.Visible;
                //Control
                ILightweightControlContainerControl parent = control.LightweightControlContainerControl;
                while (visible && parent != null)
                {
                    if (parent is LightweightControl)
                    {
                        LightweightControl lwcParent = (LightweightControl)parent;
                        visible = lwcParent.Visible;
                        parent = lwcParent.LightweightControlContainerControl;
                    }
                    else if (parent is Control)
                    {
                        visible = ((Control)parent).Visible;
                        parent = null;
                    }
                }
                return visible;
            }

            public override AccessibleObject Parent
            {
                get
                {
                    /*ILightweightControlContainerControl parent = _control.lightweightControlContainerControl;
                    if(parent is Control)
                        return ((Control) parent).AccessibilityObject;
                    else if(parent is LightweightControl)
                        return ((LightweightControl) parent).AccessibilityObject;
                    else
                        return null;*/

                    Control parent = _control.Parent;
                    if (parent != null)
                        return _control.Parent.AccessibilityObject;
                    else
                        return null;
                }
            }

            public override AccessibleObject GetChild(int index)
            {
                return _control.LightweightControls[index].AccessibilityObject;
            }

            public override int GetChildCount()
            {
                return _control.LightweightControls.Count;
            }

            public override Rectangle Bounds
            {
                get
                {
                    if (IsControlVisible(_control))
                    {
                        Control parent = _control.Parent;
                        if (parent != null)
                        {
                            Rectangle rect = parent.RectangleToScreen(_control.VirtualClientRectangleToParent());
                            return rect;
                        }
                        else
                            return Rectangle.Empty;
                    }
                    else
                        return Rectangle.Empty;
                }
            }
        }
        #endregion

        #region Private Methods

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
        /// Private helper to raise a DragEventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, DragEventArgs e)
        {
            DragEventHandler dragEventHandler = (DragEventHandler)Events[eventKey];
            if (dragEventHandler != null)
                dragEventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise a GiveFeedbackEventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A GiveFeedbackEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, GiveFeedbackEventArgs e)
        {
            GiveFeedbackEventHandler giveFeedbackEventHandler = (GiveFeedbackEventHandler)Events[eventKey];
            if (giveFeedbackEventHandler != null)
                giveFeedbackEventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise a KeyEventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, KeyEventArgs e)
        {
            KeyEventHandler keyEventHandler = (KeyEventHandler)Events[eventKey];
            if (keyEventHandler != null)
                keyEventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise a KeyPressEventArgs event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A KeyPressEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, KeyPressEventArgs e)
        {
            KeyPressEventHandler keyPressEventHandler = (KeyPressEventHandler)Events[eventKey];
            if (keyPressEventHandler != null)
                keyPressEventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise a MouseEventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, MouseEventArgs e)
        {
            MouseEventHandler mouseEventHandler = (MouseEventHandler)Events[eventKey];
            if (mouseEventHandler != null)
                mouseEventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise a PaintEventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, PaintEventArgs e)
        {
            PaintEventHandler paintEventHandler = (PaintEventHandler)Events[eventKey];
            if (paintEventHandler != null)
                paintEventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise a QueryContinueDrag event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A QueryContinueDragEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, QueryContinueDragEventArgs e)
        {
            QueryContinueDragEventHandler queryContinueDragEventHandler = (QueryContinueDragEventHandler)Events[eventKey];
            if (queryContinueDragEventHandler != null)
                queryContinueDragEventHandler(this, e);
        }

        /// <summary>
        /// Private helper to raise a ShowContextMenu event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">A ShowContextMenuEventArgs that contains the event data.</param>
        private void RaiseEvent(object eventKey, ShowContextMenuEventArgs e)
        {
            ShowContextMenuEventHandler showContextEventHandler = (ShowContextMenuEventHandler)Events[eventKey];
            if (showContextEventHandler != null)
                showContextEventHandler(this, e);
        }

        #endregion Private Methods
    }
}
