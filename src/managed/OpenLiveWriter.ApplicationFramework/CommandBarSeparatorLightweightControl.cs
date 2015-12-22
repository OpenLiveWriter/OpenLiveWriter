// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// CommandBar separator lightweight control.
    /// </summary>
    public class CommandBarSeparatorLightweightControl : LightweightControl
    {
        /// <summary>
        /// The default width.
        /// </summary>
        private const int DEFAULT_WIDTH = 2;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarSeparatorLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            InitializeComponent();
            InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorLightweightControl class.
        /// </summary>
        public CommandBarSeparatorLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            InitializeComponent();
            InitializeObject();
        }

        private void InitializeObject()
        {
            VirtualSize = DefaultVirtualSize;
            AccessibleRole = System.Windows.Forms.AccessibleRole.Separator;
            AccessibleName = "Separator";
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
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size DefaultVirtualSize
        {
            get
            {
                return new Size(DEFAULT_WIDTH, 0);
            }
        }

        /// <summary>
        /// Gets the minimum virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size MinimumVirtualSize
        {
            get
            {
                return DefaultVirtualSize;
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

            //	Paint the left line.
            using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0)))
                e.Graphics.FillRectangle(solidBrush, VirtualClientRectangle.X, VirtualClientRectangle.Y + 1, VirtualClientRectangle.Width / 2, VirtualClientRectangle.Height);

            //	Paint the right line.
            using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(64, 255, 255, 255)))
                e.Graphics.FillRectangle(solidBrush, VirtualClientRectangle.X + VirtualClientRectangle.Width / 2, VirtualClientRectangle.Y + 1, VirtualClientRectangle.Width - (VirtualClientRectangle.Width / 2), VirtualClientRectangle.Height);
        }
    }
}
