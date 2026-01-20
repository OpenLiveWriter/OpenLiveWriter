// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// ApplicationWorkspaceColumnPaneLightweightControl.  Internal helper class used by
    /// ApplicationWorkspaceColumnLightweightControl.
    /// </summary>
    internal class ApplicationWorkspaceColumnPaneLightweightControl : LightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// The control.
        /// </summary>
        private Control control;

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
                if (control != value)
                {
                    if (control != null)
                        control.Parent = null;

                    control = value;

                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the DummyPaneLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public ApplicationWorkspaceColumnPaneLightweightControl(System.ComponentModel.IContainer container)
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
        public ApplicationWorkspaceColumnPaneLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // ApplicationWorkspaceColumnPaneLightweightControl
            //
            this.Visible = false;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Layout the control.
            if (control != null)
            {
                //
                if (control.Parent != Parent)
                    control.Parent = Parent;

                //
                Rectangle layoutRectangle = VirtualClientRectangle;
                layoutRectangle.Inflate(-1, -1);
                control.Bounds = VirtualClientRectangleToParent(layoutRectangle);
            }
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            //	Solid background color.
            using (SolidBrush backgroundBrush = new SolidBrush(ApplicationManager.ApplicationStyle.BorderColor))
                e.Graphics.FillRectangle(backgroundBrush, VirtualClientRectangle);
        }
    }
}
