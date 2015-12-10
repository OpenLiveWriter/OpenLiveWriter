// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Draws a beveled line.
    /// </summary>
    public class LineBevelControl : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public LineBevelControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
            TabStop = false;
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
            // LineBevelControl
            //
            this.Name = "LineBevelControl";
            this.Size = new System.Drawing.Size(150, 12);

        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle bevelRectangle = new Rectangle(0, Utility.CenterMinZero(bevel.Height, ClientRectangle.Height), ClientRectangle.Width, bevel.Height);
            GraphicsHelper.TileFillUnscaledImageHorizontally(e.Graphics, bevel, bevelRectangle);
        }
        private readonly Bitmap bevel = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.GroupLabelBevel.png");

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //paint the bevel background
            using(Brush fillBrush = new SolidBrush(BackColor))
                pevent.Graphics.FillRectangle(fillBrush, ClientRectangle);
        }

    }
}
