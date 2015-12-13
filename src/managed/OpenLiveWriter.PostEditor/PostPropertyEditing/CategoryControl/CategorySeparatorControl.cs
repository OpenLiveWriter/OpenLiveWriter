// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    /// <summary>
    /// Summary description for CategorySeparatorControl.
    /// </summary>
    internal class CategorySeparatorControl : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public CategorySeparatorControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
            e.Graphics.DrawLine(new Pen(SystemBrushes.ControlDarkDark, 1), 0, 0, Width - 8, 0 );
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
            //
            // CategorySeparatorControl
            //
            this.Name = "CategorySeparatorControl";
            this.Size = new System.Drawing.Size(100, 5);

        }
        #endregion
    }
}
