// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

namespace Project31.ApplicationFramework
{
    /// <summary>
    /// ApplicationCommandBar lightweight control.  Provides the CommandBarLightweightControl for
    /// the ApplicationWorkspace.
    /// </summary>
    public class ApplicationWorkspaceCommandBarLightweightControl : CommandBarLightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Initializes a new instance of the ApplicationCommandBarLightweightControl class.
        /// </summary>
        public ApplicationWorkspaceCommandBarLightweightControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationCommandBarLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public ApplicationWorkspaceCommandBarLightweightControl(System.ComponentModel.IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
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
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            //	Fill the background.
            if (ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarTopColor == ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarBottomColor)
                using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarTopColor))
                    e.Graphics.FillRectangle(solidBrush, VirtualClientRectangle);
            else
                using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(VirtualClientRectangle, ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarTopColor, ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarBottomColor, LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(linearGradientBrush, VirtualClientRectangle);

            //	Draw the first line of the top bevel.
            using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarTopBevelFirstLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, 0, VirtualWidth, 1);

            //	Draw the second line of the top bevel.
            using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarTopBevelSecondLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, 1, VirtualWidth, 1);

            //	Draw the first line of the bottom bevel.
            using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarBottomBevelFirstLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, VirtualHeight-2, VirtualWidth, 1);

            //	Draw the first line of the bottom bevel.
            using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.ApplicationWorkspaceCommandBarBottomBevelSecondLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, VirtualHeight-1, VirtualWidth, 1);

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }

    }
}
