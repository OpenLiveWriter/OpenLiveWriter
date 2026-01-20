// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using System.Diagnostics;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    ///
    /// </summary>
    internal class TabWorkPaneLightweightControl : LightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        //	The control list.
        private ArrayList tabControls = new ArrayList();

        /// <summary>
        /// Initializes a new instance of the TabWorkPaneLightweightControl class.
        /// </summary>
        public TabWorkPaneLightweightControl(System.ComponentModel.IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the TabWorkPaneLightweightControl class.
        /// </summary>
        public TabWorkPaneLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tabNumber">The tab number.</param>
        /// <param name="control">The control for the tab number.</param>
        public void SetTab(int tabNumber, Control tabControl)
        {
#if false
            if (control != null && Parent != null && Parent.Controls.Contains(control))
                Parent.Controls.Remove(control);
            control = value;
            PerformLayout();
            Invalidate();
#endif

        }

        public void RemoveTabControl(int tab)
        {
#if false
            if (control != null && Parent != null && Parent.Controls.Contains(control))
                Parent.Controls.Remove(control);
            control = value;
            PerformLayout();
            Invalidate();
#endif

        }

        public void SetActiveTab(int tab)
        {
        }

        /// <summary>
        /// Gets the command bar rectangle.
        /// </summary>
        private Rectangle CommandBarRectangle
        {
            get
            {
                return new Rectangle(	0,
                                        0,
                                        VirtualWidth,
                                        20);
            }
        }

        /// <summary>
        /// Gets the control rectangle.
        /// </summary>
        private Rectangle ControlRectangle
        {
            get
            {
                return new Rectangle(	0,
                                        20,
                                        VirtualWidth,
                                        VirtualHeight-20);
            }
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(System.EventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnLayout(e);

            //	Set the bounds of the command bar lightweight control.
            //commandBarLightweightControl.VirtualBounds = CommandBarRectangle;

#if false
            //
            if (control != null)
            {
                if (Parent != null && !Parent.Controls.Contains(control))
                {
                    Parent.Controls.Add(control);
                    if (control is ICommandBarProvider)
                        commandBarLightweightControl.CommandBarDefinition = ((ICommandBarProvider)control).CommandBarDefinition;
                }
                control.Bounds = VirtualClientRectangleToParent(ControlRectangle);
            }
#endif
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }
    }
}
