// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Summary description for SearchBoxControl.
    /// </summary>
    public class SearchBoxControl : System.Windows.Forms.UserControl
    {
        private TextBoxWithEnter txtQuery;
        //private System.Windows.Forms.PictureBox picSearchBox;
        private BitmapButton picSearchBox;
//		private Bitmap bmpSearchInput = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.SearchInput.png");
        private bool highlighted;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public SearchBoxControl()
        {
            SetStyle(ControlStyles.UserPaint, true);
//			SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            HookEvents(txtQuery, picSearchBox, this);

            //picSearchBox.Image = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.Search.png");
            picSearchBox.BitmapEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.Search.png");
            picSearchBox.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.Search.png");
            picSearchBox.BitmapDisabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.Search.png");
            txtQuery.TabIndex = 0;
            picSearchBox.TabStop = true;
            picSearchBox.TabIndex = 1;

            txtQuery.EnterPressed += new EventHandler(txtQuery_EnterPressed);
            picSearchBox.Click += new EventHandler(picSearchBox_Click);
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate(false);
            base.OnResize (e);
        }

        private void HookEvents(params Control[] controls)
        {
            foreach (Control c in controls)
            {
                 c.GotFocus += new EventHandler(UpdateHighlightStateEventHandler);
                 c.LostFocus += new EventHandler(UpdateHighlightStateEventHandler);
                 c.MouseEnter += new EventHandler(UpdateHighlightStateEventHandler);
                 c.MouseLeave += new EventHandler(UpdateHighlightStateEventHandler);
            }
        }

        public event EventHandler HighlightedChanged;

        public bool Highlighted
        {
            get { return highlighted; }
            set
            {
                if (highlighted != value)
                {
                    highlighted = value;
                    if (HighlightedChanged != null)
                        HighlightedChanged(this, EventArgs.Empty);
                }
            }
        }

        private void UpdateHighlightState()
        {
            Highlighted =
                ContainsFocus || new Rectangle(0, 0, Width, Height).Contains(PointToClient(MousePosition));
        }

        private void UpdateHighlightStateEventHandler(object sender, EventArgs args)
        {
            UpdateHighlightState();
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
            this.txtQuery = new OpenLiveWriter.ApplicationFramework.SearchBoxControl.TextBoxWithEnter();
            this.picSearchBox = new OpenLiveWriter.Controls.BitmapButton();
            this.SuspendLayout();
            //
            // txtQuery
            //
            this.txtQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.txtQuery.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtQuery.Location = new System.Drawing.Point(5, 5);
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(113, 14);
            this.txtQuery.TabIndex = 0;
            this.txtQuery.Text = "";
            //
            // picSearchBox
            //
            this.picSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picSearchBox.Location = new System.Drawing.Point(126, 1);
            this.picSearchBox.Name = "picSearchBox";
            this.picSearchBox.Size = new System.Drawing.Size(23, 20);
            this.picSearchBox.TabIndex = 0;
            this.picSearchBox.TabStop = false;
            //
            // SearchBoxControl
            //
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.picSearchBox);
            this.Controls.Add(this.txtQuery);
            this.Name = "SearchBoxControl";
            this.Size = new System.Drawing.Size(150, 22);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);

            /*
            GraphicsHelper.DrawCompositedImageBorder(
                e.Graphics,
                ClientRectangle,
                bmpSearchInput,
                GraphicsHelper.SliceCompositedImageBorder(bmpSearchInput.Size, 6, 7, 7, 8));
            */

            Color borderColor = Color.FromArgb(74, 114, 140);

            using (Brush b = new SolidBrush(SystemColors.Window))
                e.Graphics.FillRectangle(b, 1, 1, Width-2, Height-2);
            using (Pen p = new Pen(borderColor, 1))
            {
                e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                e.Graphics.DrawLine(p, Width - picSearchBox.Width - 2, 0, Width - picSearchBox.Width - 2, Height);
            }

        }

        private class TextBoxWithEnter : TextBox
        {
            public event EventHandler EnterPressed;

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                if (keyData == Keys.Enter)
                {
                    if (EnterPressed != null)
                        EnterPressed(this, EventArgs.Empty);
                    return true;
                }
                else
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void txtQuery_EnterPressed(object sender, EventArgs e)
        {
            Search();
        }

        private void picSearchBox_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void Search()
        {
            string query = txtQuery.Text.Trim();
            if (query == "")
                Process.Start("http://search.live.com/");
            else
                Process.Start(
                    string.Format(CultureInfo.InvariantCulture, "http://search.live.com/results.aspx?q={0}&mkt={1}&FORM=LVSP",
                        HttpUtility.UrlEncode(query),
                        HttpUtility.UrlEncode(CultureInfo.CurrentCulture.Name)));
        }
    }
}
