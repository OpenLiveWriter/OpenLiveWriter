// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.ApplicationFramework;

namespace OpenLiveWriter
{


    /// <summary>
    /// Summary description for PaintingTestForm.
    /// </summary>
    public class PaintingTestForm : Form
    {

        class TransparentLinkLabel : LinkLabel
        {
            public TransparentLinkLabel()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent ;
                FlatStyle = FlatStyle.System ;
                Cursor = Cursors.Hand ;
                LinkBehavior = LinkBehavior.HoverUnderline ;
            }

        }

        class TransparentLabel : Label
        {
            public TransparentLabel()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent ;
            }

        }

        private TransparentLinkLabel linkLabel1;
        private OpenLiveWriter.PaintingTestForm.TransparentLinkLabel transparentLinkLabel1;
        private OpenLiveWriter.PaintingTestForm.TransparentLinkLabel transparentLinkLabel2;
        private OpenLiveWriter.PaintingTestForm.TransparentLinkLabel transparentLinkLabel3;
        private OpenLiveWriter.PaintingTestForm.TransparentLinkLabel transparentLinkLabel4;
        private OpenLiveWriter.PaintingTestForm.TransparentLinkLabel transparentLinkLabel5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public static void Main(string[] args)
        {
            Application.Run(new PaintingTestForm());
        }

        public PaintingTestForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();


            //	Turn off CS_CLIPCHILDREN.
            //User32.SetWindowLong(Handle, GWL.STYLE, User32.GetWindowLong(Handle, GWL.STYLE) & ~WS.CLIPCHILDREN);

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged (e);
            Invalidate();
        }

        /*
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {

        }
        */


        protected override void OnPaint(PaintEventArgs e)
        {
            // paint background
            using ( Brush brush = new LinearGradientBrush(ClientRectangle, ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopColor, ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor, LinearGradientMode.Vertical))
                e.Graphics.FillRectangle(brush, ClientRectangle);

            // paint headers
            using ( Brush brush = new SolidBrush(ForeColor) )
            {
                e.Graphics.DrawString("Header1", Font, brush, linkLabel1.Left, linkLabel1.Top - 20) ;

                e.Graphics.DrawString("Header2", Font, brush, transparentLinkLabel5.Left, transparentLinkLabel5.Top - 20) ;
            }

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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.linkLabel1 = new OpenLiveWriter.PaintingTestForm.TransparentLinkLabel();
            this.transparentLinkLabel1 = new OpenLiveWriter.PaintingTestForm.TransparentLinkLabel();
            this.transparentLinkLabel2 = new OpenLiveWriter.PaintingTestForm.TransparentLinkLabel();
            this.transparentLinkLabel3 = new OpenLiveWriter.PaintingTestForm.TransparentLinkLabel();
            this.transparentLinkLabel4 = new OpenLiveWriter.PaintingTestForm.TransparentLinkLabel();
            this.transparentLinkLabel5 = new OpenLiveWriter.PaintingTestForm.TransparentLinkLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // linkLabel1
            //
            this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabel1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.Location = new System.Drawing.Point(64, 56);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(120, 24);
            this.linkLabel1.TabIndex = 0;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "linkLabel1";
            //
            // transparentLinkLabel1
            //
            this.transparentLinkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.transparentLinkLabel1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.transparentLinkLabel1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.transparentLinkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.transparentLinkLabel1.Location = new System.Drawing.Point(64, 80);
            this.transparentLinkLabel1.Name = "transparentLinkLabel1";
            this.transparentLinkLabel1.Size = new System.Drawing.Size(120, 24);
            this.transparentLinkLabel1.TabIndex = 1;
            this.transparentLinkLabel1.TabStop = true;
            this.transparentLinkLabel1.Text = "transparentLinkLabel1";
            //
            // transparentLinkLabel2
            //
            this.transparentLinkLabel2.BackColor = System.Drawing.Color.Transparent;
            this.transparentLinkLabel2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.transparentLinkLabel2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.transparentLinkLabel2.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.transparentLinkLabel2.Location = new System.Drawing.Point(64, 104);
            this.transparentLinkLabel2.Name = "transparentLinkLabel2";
            this.transparentLinkLabel2.TabIndex = 0;
            this.transparentLinkLabel2.TabStop = true;
            this.transparentLinkLabel2.Text = "last label";
            //
            // transparentLinkLabel3
            //
            this.transparentLinkLabel3.BackColor = System.Drawing.Color.Transparent;
            this.transparentLinkLabel3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.transparentLinkLabel3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.transparentLinkLabel3.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.transparentLinkLabel3.Location = new System.Drawing.Point(64, 208);
            this.transparentLinkLabel3.Name = "transparentLinkLabel3";
            this.transparentLinkLabel3.TabIndex = 3;
            this.transparentLinkLabel3.TabStop = true;
            this.transparentLinkLabel3.Text = "last label";
            //
            // transparentLinkLabel4
            //
            this.transparentLinkLabel4.BackColor = System.Drawing.Color.Transparent;
            this.transparentLinkLabel4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.transparentLinkLabel4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.transparentLinkLabel4.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.transparentLinkLabel4.Location = new System.Drawing.Point(64, 184);
            this.transparentLinkLabel4.Name = "transparentLinkLabel4";
            this.transparentLinkLabel4.Size = new System.Drawing.Size(120, 24);
            this.transparentLinkLabel4.TabIndex = 4;
            this.transparentLinkLabel4.TabStop = true;
            this.transparentLinkLabel4.Text = "transparentLinkLabel4";
            //
            // transparentLinkLabel5
            //
            this.transparentLinkLabel5.BackColor = System.Drawing.Color.Transparent;
            this.transparentLinkLabel5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.transparentLinkLabel5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.transparentLinkLabel5.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.transparentLinkLabel5.Location = new System.Drawing.Point(64, 160);
            this.transparentLinkLabel5.Name = "transparentLinkLabel5";
            this.transparentLinkLabel5.TabIndex = 5;
            this.transparentLinkLabel5.TabStop = true;
            this.transparentLinkLabel5.Text = "label5";
            //
            // button1
            //
            this.button1.Location = new System.Drawing.Point(216, 96);
            this.button1.Name = "button1";
            this.button1.TabIndex = 6;
            this.button1.Text = "Show";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //
            // button2
            //
            this.button2.Location = new System.Drawing.Point(216, 128);
            this.button2.Name = "button2";
            this.button2.TabIndex = 7;
            this.button2.Text = "Hide";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            //
            // PaintingTestForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(304, 266);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.transparentLinkLabel3);
            this.Controls.Add(this.transparentLinkLabel4);
            this.Controls.Add(this.transparentLinkLabel5);
            this.Controls.Add(this.transparentLinkLabel2);
            this.Controls.Add(this.transparentLinkLabel1);
            this.Controls.Add(this.linkLabel1);
            this.Name = "PaintingTestForm";
            this.Text = "PaintingTestForm";
            this.ResumeLayout(false);

        }
        #endregion

        private void button1_Click(object sender, System.EventArgs e)
        {
            transparentLinkLabel1.Visible = true ;
            transparentLinkLabel2.Top += transparentLinkLabel1.Height ;
            transparentLinkLabel3.Top += transparentLinkLabel1.Height ;
            transparentLinkLabel4.Top += transparentLinkLabel1.Height ;
            transparentLinkLabel5.Top += transparentLinkLabel1.Height ;
            Invalidate(false);

        }

        private void button2_Click(object sender, System.EventArgs e)
        {

            transparentLinkLabel1.Visible = false ;
            transparentLinkLabel2.Top -= transparentLinkLabel1.Height ;
            transparentLinkLabel3.Top -= transparentLinkLabel1.Height ;
            transparentLinkLabel4.Top -= transparentLinkLabel1.Height ;
            transparentLinkLabel5.Top -= transparentLinkLabel1.Height ;
            Invalidate(false);
        }
    }
}
