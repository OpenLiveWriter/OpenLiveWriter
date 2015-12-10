// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Provides a base class for lightweight button-type controls.  Painting and layout is left
    /// to derived classes.
    /// </summary>
    public class ButtonBaseLightweightControl : LightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Gets the tool tip text to display for this ButtonBaseLightweightControl.
        /// </summary>
        protected virtual string ToolTipText
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A value indicating whether the mouse is inside the control.
        /// </summary>
        private bool mouseInside = false;

        /// <summary>
        /// Gets or sets a value indicating whether the mouse is inside the control.
        /// </summary>
        protected bool MouseInside
        {
            get
            {
                return mouseInside;
            }
            set
            {
                //	Ensure that the property is actually changing.
                if (mouseInside != value)
                {
                    //	Update the value.
                    mouseInside = value;
                    Invalidate();

                    //	Update the tool tip text, if the parent implements IToolTipDisplay.  Note
                    //	that we set the tool tip text when the parent implements IToolTipDisplay so
                    //	an older tool tip will be erased if it was being displayed.
                    if (Parent is IToolTipDisplay)
                    {
                        IToolTipDisplay toolTipDisplay = (IToolTipDisplay)Parent;
                        if (mouseInside && !LeftMouseButtonDown)
                            toolTipDisplay.SetToolTip(ToolTipText);
                        else
                            toolTipDisplay.SetToolTip(null);
                    }
                }
            }
        }

        /// <summary>
        /// A value indicating whether the left mouse button is down.
        /// </summary>
        private bool leftMouseButtonDown = false;

        /// <summary>
        /// Gets or sets a value indicating whether the left mouse button is down.
        /// </summary>
        protected bool LeftMouseButtonDown
        {
            get
            {
                return leftMouseButtonDown;
            }
            set
            {
                //	Ensure that the property is actually changing.
                if (leftMouseButtonDown != value)
                {
                    leftMouseButtonDown = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Occurs when the button is pushed.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the button is pushed.")
        ]
        public event EventHandler Pushed;

        /// <summary>
        /// Initializes a new instance of the ButtonBaseLightweightControl class.
        /// </summary>
        public ButtonBaseLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the ButtonBaseLightweightControl class.
        /// </summary>
        public ButtonBaseLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();

            //	Initialize the object.
            InitializeObject();
        }

        /// <summary>
        /// Object initialization.
        /// </summary>
        private void InitializeObject()
        {
            //	Set the default size.
            VirtualSize = DefaultVirtualSize;
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseDown(e);

            //	If the button is the left button, set the LeftMouseButtonDown property.
            if (e.Button == MouseButtons.Left)
                LeftMouseButtonDown = true;
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">A EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            //	Set the MouseInside property.
            MouseInside = true;
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseLeave(e);

            //	Set the MouseInside property.
            MouseInside = false;
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseMove(e);

            //	Update the state of the MouseInside property if the LeftMouseButtonDown property
            //	is true.
            if (LeftMouseButtonDown)
                MouseInside = VirtualClientRectangle.Contains(e.X, e.Y);
            else
                MouseInside = true;
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseUp(e);

            //	No mouse inside until it moves again.
            MouseInside = false;

            //	If the button is the left button, set the LeftMouseButtonDown property.
            if (e.Button == MouseButtons.Left)
            {
                LeftMouseButtonDown = false;

                if (VirtualClientRectangle.Contains(e.X, e.Y))
                    OnPushed(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the Pushed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnPushed(EventArgs e)
        {
            if (Pushed != null)
                Pushed(this, e);
        }
    }
}
