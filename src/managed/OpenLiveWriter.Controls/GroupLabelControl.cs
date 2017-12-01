// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Summary description for GroupLabelControl.
    /// </summary>
    public class GroupLabelControl : LabelControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public GroupLabelControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
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
            components = new System.ComponentModel.Container();
        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);

            //get the rectangle representing the text in the label
            Rectangle textRectangle = GetMeasuredTextRectangle();

            //paint the line bevels to the left and right of the text
            if (textRectangle.Left != 0)
            {
                Rectangle leftRectangle = new Rectangle(0,
                    (Utility.Center(bevel.Height, textRectangle.Height) + textRectangle.Top + BEVEL_TEXT_MIDDLE_OFFSET),
                    textRectangle.X,
                    bevel.Height
                    );
                GraphicsHelper.TileFillScaledImageHorizontally(g, bevel, leftRectangle);
            }
            if (textRectangle.Right != ClientRectangle.Right)
            {
                int startX = textRectangle.Right + 3;
                Rectangle rightRectangle = new Rectangle(startX,
                    Utility.Center(bevel.Height, textRectangle.Height) + textRectangle.Top + BEVEL_TEXT_MIDDLE_OFFSET,
                    ClientRectangle.Width - startX,
                    bevel.Height
                    );
                GraphicsHelper.TileFillScaledImageHorizontally(g, bevel, rightRectangle);
            }
        }
        private readonly Bitmap bevel = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.GroupLabelBevel.png");
        private const int BEVEL_TEXT_MIDDLE_OFFSET = 2;

    }
}
