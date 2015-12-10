// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{
    /// <summary>
    /// Summary description for MapLogoControl.
    /// </summary>
    public class MapLogoControl : System.Windows.Forms.UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Bitmap _bingLogoBitmap;

        private const int HORIZONTAL_INSET = 8;
        private const int VERTICAL_INSET = 12;

        public MapLogoControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            _bingLogoBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.BingLogo.png");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, ClientRectangle);

            Point logoLocation = new Point(HORIZONTAL_INSET, VERTICAL_INSET);
            g.DrawImage(false, _bingLogoBitmap, new Rectangle(logoLocation, _bingLogoBitmap.Size));
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
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
            //
            // MapLogoControl
            //
            this.Name = "MapLogoControl";
            this.Size = new System.Drawing.Size(200, 47);

        }
        #endregion
    }
}
