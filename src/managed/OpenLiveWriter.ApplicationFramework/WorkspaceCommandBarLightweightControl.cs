// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Base class of CommandBarLightweightControls for the WorkspaceControl.
    /// </summary>
    internal class WorkspaceCommandBarLightweightControl : CommandBarLightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the ApplicationCommandBarLightweightControl class.
        /// </summary>
        public WorkspaceCommandBarLightweightControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationCommandBarLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public WorkspaceCommandBarLightweightControl(IContainer container)
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

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Our LightweightControlContainerControl will be a WorkspaceControl.  Get it.
            WorkspaceControl workspaceControl = LightweightControlContainerControl as WorkspaceControl;
            if (workspaceControl == null)
                return;

            //	Based on the type of workspace control, select the colors to use when painting.
            Color topColor;
            Color bottomColor;
            Color topBevelFirstLineColor;
            Color topBevelSecondLineColor;
            Color bottomBevelFirstLineColor;
            Color bottomBevelSecondLineColor;
            topColor = SystemColors.Control;
            bottomColor = SystemColors.Control;
            topBevelFirstLineColor = SystemColors.Control;
            topBevelSecondLineColor = SystemColors.Control;
            bottomBevelFirstLineColor = SystemColors.Control;
            bottomBevelSecondLineColor = SystemColors.Control;

            //	Fill the background.
            if (topColor == bottomColor)
                using (SolidBrush solidBrush = new SolidBrush(topColor))
                    e.Graphics.FillRectangle(solidBrush, VirtualClientRectangle);
            else
                using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(VirtualClientRectangle, topColor, bottomColor, LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(linearGradientBrush, VirtualClientRectangle);

            //	Draw the first line of the top bevel.
            using (SolidBrush solidBrush = new SolidBrush(topBevelFirstLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, 0, VirtualWidth, 1);

            //	Draw the second line of the top bevel.
            using (SolidBrush solidBrush = new SolidBrush(topBevelSecondLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, 1, VirtualWidth, 1);

            //	Draw the first line of the bottom bevel.
            using (SolidBrush solidBrush = new SolidBrush(bottomBevelFirstLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, VirtualHeight - 2, VirtualWidth, 1);

            //	Draw the first line of the bottom bevel.
            using (SolidBrush solidBrush = new SolidBrush(bottomBevelSecondLineColor))
                e.Graphics.FillRectangle(solidBrush, 0, VirtualHeight - 1, VirtualWidth, 1);

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }
    }
}
