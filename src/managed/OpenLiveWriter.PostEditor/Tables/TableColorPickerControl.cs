// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.Tables
{
    /// <summary>
    /// Summary description for TableColorPickerControl.
    /// </summary>
    public class TableColorPickerControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Button buttonBrowseColor;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public TableColorPickerControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

        }

        public CellColor CellColor
        {
            get
            {
                return _cellColor ;
            }
            set
            {
                // null == default
                CellColor cellColor = value ;
                if ( cellColor == null )
                    cellColor = new CellColor();

                _cellColor = cellColor ;

                Invalidate() ;
            }
        }
        private CellColor _cellColor = new CellColor();


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics ;

            // calculate rectangle for color display
            Rectangle rectColorDisplay = new Rectangle(0,0, Width-buttonBrowseColor.Width-5, Height-1 ) ;

            // draw color
            if ( CellColor.IsMixed )
            {
                using ( Brush brush = new SolidBrush(Color.Yellow) )
                    g.FillRectangle(brush, rectColorDisplay);
            }
            else if (CellColor.Color == Color.Empty )
            {
                using ( Brush brush = new SolidBrush(Color.Red) )
                    g.FillRectangle(brush, rectColorDisplay);
            }
            else
            {
                using ( Brush brush = new SolidBrush(CellColor.Color) )
                    g.FillRectangle(brush, rectColorDisplay);
            }

            // draw border
            using ( Pen pen = new Pen(ColorHelper.GetThemeBorderColor(SystemColors.ControlDark)) )
                g.DrawRectangle(pen, rectColorDisplay) ;
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
            this.buttonBrowseColor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // buttonBrowseColor
            //
            this.buttonBrowseColor.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonBrowseColor.Location = new System.Drawing.Point(50, 0);
            this.buttonBrowseColor.Name = "buttonBrowseColor";
            this.buttonBrowseColor.Size = new System.Drawing.Size(23, 21);
            this.buttonBrowseColor.TabIndex = 1;
            this.buttonBrowseColor.Text = "...";
            this.buttonBrowseColor.Click += new System.EventHandler(this.buttonBrowseColor_Click);
            //
            // TableColorPickerControl
            //
            this.Controls.Add(this.buttonBrowseColor);
            this.Name = "TableColorPickerControl";
            this.Size = new System.Drawing.Size(73, 21);
            this.ResumeLayout(false);

        }
        #endregion

        private void buttonBrowseColor_Click(object sender, System.EventArgs e)
        {
            // create the color picker dialog
            using (ColorDialog colorDialog = new ColorDialog() )
            {
                colorDialog.AllowFullOpen = false;
                colorDialog.FullOpen = false;
                colorDialog.Color = CellColor.Color ;

                // show the color picker dialog
                using ( new WaitCursor() )
                {
                    if ( colorDialog.ShowDialog(FindForm()) == DialogResult.OK )
                    {
                        // update color
                        CellColor = new CellColor(colorDialog.Color) ;
                    }
                }
            }
        }
    }
}
