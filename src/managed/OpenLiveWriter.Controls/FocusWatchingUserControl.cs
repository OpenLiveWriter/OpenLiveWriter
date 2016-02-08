// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// FocusWatchingUserControl.  A specialized UserControl that watches focus.
    /// </summary>
    public class FocusWatchingUserControl : UserControl
    {
        #region Private Member Variables

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the FocusWatchingUserControl class.
        /// </summary>
        public FocusWatchingUserControl()
        {
            // This call is required by the Windows.Forms Form Designer.
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
            components = new System.ComponentModel.Container();
        }
        #endregion

        #region Protected Event Overrides

        /// <summary>
        /// Raises the ControlAdded event.
        /// </summary>
        /// <param name="e">A EventArgs that contains the event data.</param>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnControlAdded(e);

            //	If the control is not a FocusWatchingUserControl, watch focus events on it.
            if (!(e.Control is FocusWatchingUserControl))
            {
                e.Control.GotFocus += new EventHandler(Control_GotFocus);
                e.Control.LostFocus += new EventHandler(Control_LostFocus);
            }
        }

        /// <summary>
        /// Raises the ControlRemoved event.
        /// </summary>
        /// <param name="e">A EventArgs that contains the event data.</param>
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnControlRemoved(e);

            //	If the control is not a FocusWatchingUserControl, stop watching focus events on it.
            if (!(e.Control is FocusWatchingUserControl))
            {
                e.Control.GotFocus -= new EventHandler(Control_GotFocus);
                e.Control.LostFocus -= new EventHandler(Control_LostFocus);
            }
        }

        /// <summary>
        /// Raises the GotFocus event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnGotFocus(e);

            //	Process the focus event.
            ProcessFocusEvent(this, true);
        }

        /// <summary>
        /// Raises the LostFocus event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLostFocus(e);

            //	Process the focus event.
            ProcessFocusEvent(this, false);
        }

        #endregion Protected Event Overrides

        #region Private Methods

        /// <summary>
        /// Control_GotFocus event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void Control_GotFocus(object sender, EventArgs e)
        {
            //	Process the focus event.
            ProcessFocusEvent(sender as Control, true);
        }

        /// <summary>
        /// Control_LostFocus event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void Control_LostFocus(object sender, EventArgs e)
        {
            //	Process the focus event.
            ProcessFocusEvent(sender as Control, false);
        }

        /// <summary>
        /// Processes a focus event.
        /// </summary>
        /// <param name="control">The control that the event relates to.</param>
        /// <param name="gotFocus">true if the control got focus; false otherwise.</param>
        private void ProcessFocusEvent(Control control, bool gotFocus)
        {
            if (control != null)
            {
                IChildFocusWatcher childFocusWatcher = IChildFocusWatcher;
                if (childFocusWatcher != null)
                {
                    if (gotFocus)
                        childFocusWatcher.ChildGotFocus(control);
                    else
                        childFocusWatcher.ChildLostFocus(control);
                }
            }
        }

        /// <summary>
        /// Gets the parent control that is a IChildFocusWatcher.
        /// </summary>
        private IChildFocusWatcher IChildFocusWatcher
        {
            get
            {
                //	See if we have a parent control this is a IChildFocusWatcher.  If so, return it.
                for (Control parent = Parent; parent != null; parent = parent.Parent)
                    if (parent is IChildFocusWatcher)
                        return (IChildFocusWatcher)parent;

                //	We do not have a parent that is a IChildFocusWatcher, so return null.
                return null;
            }
        }

        #endregion Private Methods

    }
}
