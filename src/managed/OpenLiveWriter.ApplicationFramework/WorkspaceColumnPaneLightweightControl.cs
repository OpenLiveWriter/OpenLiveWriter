// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// WorkspaceColumnPaneLightweightControl.
    /// </summary>
    public class WorkspaceColumnPaneLightweightControl : LightweightControl
    {
        #region Private Member Variables & Declarations

        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected Container components = null;

        /// <summary>
        /// The control.
        /// </summary>
        private Control control;

        /// <summary>
        /// The lightweight control.
        /// </summary>
        private LightweightControl lightweightControl;

        /// <summary>
        /// A value indicating whether a border will be drawn.
        /// </summary>
        private bool border;

        /// <summary>
        /// A value which indicates whether the pane should be layed out with a fixed height, when
        /// possible.
        /// </summary>
        private bool fixedHeightLayout;

        /// <summary>
        /// The fixed height to be used when the FixedHeightLayout property is true.
        /// </summary>
        private int fixedHeight;

        #endregion Private Member Variables & Declarations

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the DummyPaneLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public WorkspaceColumnPaneLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the DummyPaneLightweightControl class.
        /// </summary>
        public WorkspaceColumnPaneLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the control.
        /// </summary>
        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                //	If the control is changing, change it.
                if (control != value)
                {
                    //	Clear.
                    Clear();

                    //	Set the new control.
                    if ((control = value) != null)
                    {
                        control.Parent = Parent;
                        Visible = true;
                        PerformLayout();
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the control.
        /// </summary>
        public LightweightControl LightweightControl
        {
            get
            {
                return lightweightControl;
            }
            set
            {
                //	If the control is changing, change it.
                if (lightweightControl != value)
                {
                    //	Clear.
                    Clear();

                    //	Set the new control.
                    if ((lightweightControl = value) != null)
                    {
                        lightweightControl.LightweightControlContainerControl = this;
                        Visible = true;
                        PerformLayout();
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a border will be drawn.
        /// </summary>
        public bool Border
        {
            get
            {
                return border;
            }
            set
            {
                border = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which indicates whether the pane should be layed out with a fixed
        /// height, when possible.
        /// </summary>
        public bool FixedHeightLayout
        {
            get
            {
                return fixedHeightLayout;
            }
            set
            {
                if (fixedHeightLayout != value)
                {
                    fixedHeightLayout = value;
                    Parent.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the fixed height to be used when the FixedHeightLayout property is true.
        /// </summary>
        public int FixedHeight
        {
            get
            {
                return fixedHeight;
            }
            set
            {
                if (fixedHeight != value)
                {
                    fixedHeight = value;
                    Parent.PerformLayout();
                }
            }
        }

        #endregion Public Properties

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            if (Parent == null)
                return;

            //	Layout the control.
            if (control != null)
            {
                Rectangle layoutRectangle = VirtualClientRectangle;
                if (border)
                    layoutRectangle.Inflate(-1, -1);
                control.Bounds = VirtualClientRectangleToParent(layoutRectangle);
            }
            else if (lightweightControl != null)
            {
                Rectangle layoutRectangle = VirtualClientRectangle;
                if (border)
                    layoutRectangle.Inflate(-1, -1);
                lightweightControl.VirtualBounds = layoutRectangle;
            }
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            //	If we're drawing a border, draw it.
            if (border)
                using (Pen pen = new Pen(ApplicationManager.ApplicationStyle.BorderColor))
                    e.Graphics.DrawRectangle(pen,
                                                VirtualClientRectangle.X,
                                                VirtualClientRectangle.Y,
                                                VirtualClientRectangle.Width - 1,
                                                VirtualClientRectangle.Height - 1);
        }

        /// <summary>
        /// Raises the VisibleChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnVisibleChanged(e);

            //	Ensure that the Control/LightweightControl Visible property is matched.
            if (control != null)
                control.Visible = Visible;
            else if (lightweightControl != null)
                lightweightControl.Visible = Visible;
        }

        #endregion Protected Event Overrides

        #region Private Methods

        /// <summary>
        /// Clears the workspace column pane.
        /// </summary>
        private void Clear()
        {
            //	If there's a control or a lightweight control, remove it.
            if (control != null)
            {
                control.Parent = null;
                control.Dispose();
                control = null;
            }
            else if (lightweightControl != null)
            {
                lightweightControl.LightweightControlContainerControl = null;
                lightweightControl.Dispose();
                lightweightControl = null;
            }

            //	Poof!  We're invisible.
            Visible = false;
        }

        #endregion Private Methods
    }
}
