// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// CommandBar control lightweight control.  Allows any control to be placed on a CommandBar.
    /// </summary>
    internal class CommandBarControlLightweightControl : LightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The control for the command bar control lightweight control.
        /// </summary>
        private Control control;

        /// <summary>
        /// Gets or sets the control for the command bar control lightweight control.
        /// </summary>
        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                control = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarControlLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlLightweightControl class.
        /// </summary>
        public CommandBarControlLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlLightweightControl class.
        /// </summary>
        public CommandBarControlLightweightControl(Control control)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
            InitializeObject();
            this.control = control;
            control.TabStop = false;
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

        }
        #endregion

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject()
        {
            TabStop = true;
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);
            VirtualSize = new Size(control.Width + 4, control.Height);
        }

        private void SyncControlLocation()
        {
            Rectangle bounds = new Rectangle(
                VirtualClientPointToParent(new Point(2, 0)),
                control.Size);
            control.Location = bounds.Location;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Fix bug 722229: paragraph button doesn't show in the right place for RTL builds
            // We used to do SyncControlLocation() in OnLayout. The problem is that OnLayout
            // only occurs when this lightweight control is moved relative to its immediate
            // parent--it doesn't get called when its parent is moved relative to the grandparent.
            // Since layout happens from the bottom of the hierarchy up, and the heavyweight
            // control's coordinate system is relative to the parent heavyweight control, the
            // two coordinate systems would get out of sync.
            //
            // By moving the call to OnPaint we can be confident that the heavyweight control
            // will be moved after all layout has completed.
            SyncControlLocation();

            base.OnPaint(e);
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLightweightControlContainerControlChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLightweightControlContainerControlChanged(e);

            //
            if (control.Parent != Parent)
                control.Parent = Parent;

            if (Parent != null)
            {
                PerformLayout();
                Invalidate();
            }
        }

        public override bool Focus()
        {
            control.Focus();
            return base.Focus();
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return control.AccessibilityObject;
        }
    }
}
