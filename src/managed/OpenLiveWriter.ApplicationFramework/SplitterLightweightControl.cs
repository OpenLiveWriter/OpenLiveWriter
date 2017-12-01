// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    #region Public Delegates

    /// <summary>
    /// Represents the method that will handle the SplitterMoved event.
    /// </summary>
    public delegate void LightweightSplitterEventHandler(object sender, LightweightSplitterEventArgs e);

    #endregion Public Delegates

    /// <summary>
    /// Splitter lightweight control.  Provides a horizontal or vertical splitter for use in a
    /// multipane window with resizable panes.
    /// </summary>
    public class SplitterLightweightControl : LightweightControl
    {
        #region Public Enumeration Declarations

        /// <summary>
        /// The splitter orientation: Horizontal or Vertical.
        /// </summary>
        public enum SplitterOrientation
        {
            Horizontal,
            Vertical
        }

        #endregion Public Enumeration Declarations

        #region Private Member Variables

        /// <summary>
        /// The splitter orientation.
        /// </summary>
        private SplitterOrientation orientation = SplitterOrientation.Horizontal;

        /// <summary>
        /// The starting position of a move.
        /// </summary>
        private int startingPosition;

        /// <summary>
        /// A value indicating whether the left mouse button is down.
        /// </summary>
        private bool leftMouseButtonDown = false;

        /// <summary>
        /// A value indicating whether the SplitterLightweightControl is enabled.
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// The layout rectangle for the attached control.
        /// </summary>
        private Rectangle attachedControlRectangle = new Rectangle();

        /// <summary>
        /// The attached control.
        /// </summary>
        private LightweightControl _attachedControl;

        #endregion Private Member Variables

        #region Public Events

        /// <summary>
        /// Occurs when the splitter control begins a move operation.
        /// </summary>
        public event EventHandler SplitterBeginMove;

        /// <summary>
        /// Occurs when the splitter control ends a move operation.
        /// </summary>
        public event LightweightSplitterEventHandler SplitterEndMove;

        /// <summary>
        /// Occurs when the splitter control is moving.
        /// </summary>
        public event LightweightSplitterEventHandler SplitterMoving;

        #endregion Public Events

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the SplitterLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public SplitterLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the SplitterLightweightControl class.
        /// </summary>
        public SplitterLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        #endregion Class Initialization & Termination

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the splitter orientation.
        /// </summary>
        [
            Category("Design"),
                Localizable(false),
                DefaultValue(SplitterOrientation.Horizontal),
                Description("Specifies the the splitter orientation.")
        ]
        public SplitterOrientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
            }
        }

        /// <summary>
        /// Gets or sets the splitter orientation.
        /// </summary>
        [
            Category("Behavior"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the splitter is initially enabled.")
        ]
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        /// <summary>
        /// Gets or set the LightweightControl attached to the center of the splitter bar.
        /// </summary>
        public LightweightControl AttachedControl
        {
            get
            {
                return _attachedControl;
            }
            set
            {
                if (_attachedControl != value)
                {
                    if (_attachedControl != null)
                    {
                        LightweightControls.Remove(_attachedControl);
                        _attachedControl.MouseDown -= new MouseEventHandler(_attachedControl_MouseDown);
                        _attachedControl.MouseEnter -= new EventHandler(_attachedControl_MouseEnter);
                        _attachedControl.MouseLeave -= new EventHandler(_attachedControl_MouseLeave);
                        _attachedControl.MouseMove -= new MouseEventHandler(_attachedControl_MouseMove);
                        _attachedControl.MouseUp -= new MouseEventHandler(_attachedControl_MouseUp);
                    }

                    _attachedControl = value;
                    if (_attachedControl != null)
                    {
                        LightweightControls.Add(_attachedControl);
                        _attachedControl.MouseDown += new MouseEventHandler(_attachedControl_MouseDown);
                        _attachedControl.MouseEnter += new EventHandler(_attachedControl_MouseEnter);
                        _attachedControl.MouseLeave += new EventHandler(_attachedControl_MouseLeave);
                        _attachedControl.MouseMove += new MouseEventHandler(_attachedControl_MouseMove);
                        _attachedControl.MouseUp += new MouseEventHandler(_attachedControl_MouseUp);
                    }
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        #endregion Public Properties

        #region Protected Events

        /// <summary>
        /// Raises the SplitterBeginMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSplitterBeginMove(EventArgs e)
        {
            if (SplitterBeginMove != null)
                SplitterBeginMove(this, e);
        }

        /// <summary>
        /// Raises the SplitterEndMove event.
        /// </summary>
        /// <param name="e">A LightweightSplitterEventArgs that contains the event data.</param>
        protected virtual void OnSplitterEndMove(LightweightSplitterEventArgs e)
        {
            if (SplitterEndMove != null)
                SplitterEndMove(this, e);
        }

        /// <summary>
        /// Raises the SplitterMoving event.
        /// </summary>
        /// <param name="e">A LightweightSplitterEventArgs that contains the event data.</param>
        protected virtual void OnSplitterMoving(LightweightSplitterEventArgs e)
        {
            if (SplitterMoving != null)
                SplitterMoving(this, e);
        }

        #endregion Protected Events

        #region Protected Event Overrides

        /// <summary>
        /// Handles the layout event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLayout(EventArgs e)
        {
            base.OnLayout(e);

            //If a control is attached to the splitter, lay it out.
            if (AttachedControl != null)
            {
                //	No layout required if this control is not visible.
                if (Parent == null || Parent.Parent == null)
                    return;

                //	Layout the expand control.
                attachedControlRectangle = new Rectangle(Utility.CenterMinZero(AttachedControl.DefaultVirtualSize.Width, VirtualWidth),
                    Utility.CenterMinZero(AttachedControl.DefaultVirtualSize.Height, VirtualHeight),
                    AttachedControl.DefaultVirtualSize.Width,
                    VirtualHeight);

                AttachedControl.VirtualBounds = attachedControlRectangle;
                AttachedControl.PerformLayout();
            }
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseDown(e);

            //	Ignore the event if the splitter is disabled.
            if (!enabled)
                return;

            //	If the mouse button is the left button, begin a splitter resize.
            if (e.Button == MouseButtons.Left)
            {
                //	Note that the left mouse button is down.
                leftMouseButtonDown = true;

                //	Note the starting position.
                startingPosition = (orientation == SplitterOrientation.Vertical) ? e.X : e.Y;

                //	Raise the SplitterBeginMove event.
                OnSplitterBeginMove(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            //	Ignore the event if the splitter is disabled.
            if (!enabled)
                return;

            //	Ensure that the left mouse button isn't down.
            Debug.Assert(!leftMouseButtonDown, "What?", "How can the left mouse button be down on mouse enter?");

            //	Turn on the splitter cursor.
            Parent.Cursor = (orientation == SplitterOrientation.Vertical) ? Cursors.VSplit : Cursors.HSplit;
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseLeave(e);

            //	Ignore the event if the splitter is disabled.
            if (!enabled)
                return;

            //	If the left mouse button was down, end the resize operation.
            if (leftMouseButtonDown)
            {
                //	Raise the event.
                OnSplitterEndMove(new LightweightSplitterEventArgs(0));

                //	The left mouse button is not down.
                leftMouseButtonDown = false;
            }

            //	Turn off the splitter cursor.
            Parent.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseMove(e);

            //	Ignore the event if the splitter is disabled.
            if (!enabled)
                return;

            //	If the left mouse button is down, continue the resize operation.
            if (leftMouseButtonDown)
            {
                //	If we have one or more registered delegates for the SplitterMoving event, raise
                //	the event.
                if (SplitterMoving != null)
                {
                    //	Calculate the new position.
                    int newPosition = ((orientation == SplitterOrientation.Vertical) ? e.X : e.Y) - startingPosition;

                    //	Raise the SplitterMoving event.
                    OnSplitterMoving(new LightweightSplitterEventArgs(newPosition));
                }
            }
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseUp(e);

            //	Ignore the event if the splitter is disabled.
            if (!enabled)
                return;

            //	If the left mouse button is down, end the resize operation.
            if (e.Button == MouseButtons.Left)
            {
                //	Ensure that the left mouse button is down.
                Debug.Assert(leftMouseButtonDown, "What?", "Got a MouseUp that was unexpected.");
                if (leftMouseButtonDown)
                {
                    //	Obtain the new position.
                    int newPosition = ((orientation == SplitterOrientation.Vertical) ? e.X : e.Y) - startingPosition;

                    //	Raise the event.
                    OnSplitterEndMove(new LightweightSplitterEventArgs(newPosition));

                    //	The left mouse button is not down.
                    leftMouseButtonDown = false;
                }
            }
        }

        #endregion Protected Event Overrides

        #region Private Event Handlers
        /// <summary>
        /// Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        /// <summary>
        /// Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        /// <summary>
        /// Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        /// <summary>
        /// Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        /// <summary>
        /// Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }
        #endregion
    }
}
