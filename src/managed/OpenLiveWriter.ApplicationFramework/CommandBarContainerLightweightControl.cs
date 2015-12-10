// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Provides a container control for CommandBarLightweightControl.
    /// </summary>
    public class CommandBarContainerLightweightControl : LightweightControl
    {
        #region Private Member Variables

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the CommandBarContainerLightweightControl class.
        /// </summary>
        public CommandBarContainerLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarContainerLightweightControl class.
        /// </summary>
        public CommandBarContainerLightweightControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            InitializeObject();
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

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject()
        {
            AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
        }

        #region Public Properties

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        public override Size DefaultVirtualSize
        {
            get
            {
                CommandBarLightweightControl commandBarLightweightControl = LightweightControlContainerControl as CommandBarLightweightControl;
                if (commandBarLightweightControl == null)
                    return Size.Empty;

                //	Calculate the maximum width and height.
                int maximumWidth = 0;
                int maximumHeight = 0;
                foreach (LightweightControl lightweightControl in LightweightControls)
                {
                    //	Have the lightweight control perform its layout logic.
                    lightweightControl.PerformLayout();

                    //	Account for the width of the control.
                    maximumWidth += lightweightControl.VirtualWidth;
                    if (lightweightControl is CommandBarButtonLightweightControl)
                    {
                        maximumWidth += ((CommandBarButtonLightweightControl)lightweightControl).MarginLeft;
                        maximumWidth += ((CommandBarButtonLightweightControl)lightweightControl).MarginRight;
                    }

                    //	Handle separators.
                    if (lightweightControl is CommandBarSeparatorLightweightControl)
                        maximumWidth += commandBarLightweightControl.SeparatorLayoutMargin * 2;

                    //	Note the tallest virtual control.
                    if (lightweightControl.VirtualHeight > maximumHeight)
                        maximumHeight = lightweightControl.VirtualHeight;
                }

                //	Return the default virtual size.
                return new Size(maximumWidth, maximumHeight);
            }
        }

        public int OffsetSpacing
        {
            get
            {
                return _offSetSpacing;
            }
            set
            {
                _offSetSpacing = value;
            }
        }

        private int _offSetSpacing = 0;

        #endregion Public Properties

        #region Public Methods

        #endregion Public Methods

        #region Protected Event Overrides

        private enum Previous
        {
            None,
            Button,
            Separator,
            Other
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Obtain our CommandBarLightweightControl.  Must have one.
            CommandBarLightweightControl commandBarLightweightControl = LightweightControlContainerControl as CommandBarLightweightControl;
            if (commandBarLightweightControl == null)
                return;

            //	Layout the lightweight controls on the command bar lightweight control.
            int xOffset = 0;
            Previous previous = Previous.None;

            foreach (LightweightControl lightweightControl in LightweightControls)
            {
                //	Have the lightweight control perform its layout logic.
                lightweightControl.PerformLayout();

                //	Skip the lightweight control if it's not visible.
                if (!lightweightControl.Visible)
                    continue;

                //	Handle separators.
                if (lightweightControl is CommandBarSeparatorLightweightControl)
                {
                    lightweightControl.VirtualHeight = VirtualHeight;
                    xOffset += commandBarLightweightControl.SeparatorLayoutMargin;
                }

                if (previous == Previous.None)
                    xOffset += 5;

                if ((previous == Previous.Button || previous == Previous.Separator) && lightweightControl is CommandBarButtonLightweightControl)
                    xOffset += OffsetSpacing;

                if (lightweightControl is CommandBarButtonLightweightControl)
                    xOffset += ((CommandBarButtonLightweightControl)lightweightControl).MarginLeft;

                //	Set the location of this control.
                lightweightControl.VirtualLocation = new Point(xOffset, Utility.CenterMinZero(lightweightControl.VirtualHeight, VirtualHeight));

                //	Have the lightweight control perform its layout logic.
                //lightweightControl.PerformLayout();

                //	Adjust the x offset for the next loop iteration.
                xOffset += lightweightControl.VirtualWidth;

                if (lightweightControl is CommandBarButtonLightweightControl)
                    xOffset += ((CommandBarButtonLightweightControl)lightweightControl).MarginRight;

                previous = Previous.Other;

                //	Handle separators.
                if (lightweightControl is CommandBarSeparatorLightweightControl)
                {
                    xOffset += commandBarLightweightControl.SeparatorLayoutMargin;
                    previous = Previous.Separator;
                }

                if (lightweightControl is CommandBarButtonLightweightControl)
                {
                    //if (((CommandBarButtonLightweightControl)lightweightControl).DropDownContextMenuUserInterface)
                    //    xOffset += 15;

                    previous = Previous.Button;
                }
            }

            VirtualWidth = xOffset;

            RtlLayoutFixup(false);
        }

        #endregion Protected Event Overrides
    }
}
