// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public class MinMaxClose : Control
    {
        private bool _faded = false;
        private BitmapButton btnMinimize;
        private BitmapButton btnMaximize;
        private BitmapButton btnClose;

        private Bitmap minEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizeEnabled.png");
        private Bitmap maxEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizeEnabled.png");
        private Bitmap closeEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.CloseEnabled.png");
        private Bitmap minDisabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizeDisabled.png");
        private Bitmap maxDisabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizeDisabled.png");
        private Bitmap closeDisabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.CloseDisabled.png");

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public MinMaxClose()
        {
            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.btnMinimize.Text = Res.Get(StringId.MinimizeButtonTooltip);
            this.btnMaximize.Text = Res.Get(StringId.MaximizeButtonTooltip);
            this.btnClose.Text = Res.Get(StringId.CloseButtonTooltip);

            this.btnMinimize.UseVirtualTransparency = true;
            this.btnMaximize.UseVirtualTransparency = true;
            this.btnClose.UseVirtualTransparency = true;

            this.btnMinimize.BitmapEnabled = minEnabled;
            this.btnMaximize.BitmapEnabled = maxEnabled;
            this.btnClose.BitmapEnabled = closeEnabled;

            this.btnMinimize.TabStop = false;
            this.btnMaximize.TabStop = false;
            this.btnClose.TabStop = false;
            this.TabStop = false;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            VirtualTransparency.VirtualPaint(this, e);
        }

        public bool Faded
        {
            get { return _faded; }
            set
            {
                _faded = value;
                if (value)
                {
                    this.btnMinimize.BitmapEnabled = minDisabled;
                    this.btnMaximize.BitmapEnabled = maxDisabled;
                    this.btnClose.BitmapEnabled = closeDisabled;
                    Invalidate(true);
                }
                else
                {
                    this.btnMinimize.BitmapEnabled = minEnabled;
                    this.btnMaximize.BitmapEnabled = maxEnabled;
                    this.btnClose.BitmapEnabled = closeEnabled;
                    Invalidate(true);
                }
            }
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
            this.btnMinimize = new BitmapButton();
            this.btnMaximize = new BitmapButton();
            this.btnClose = new BitmapButton();
            this.SuspendLayout();
            //
            // btnMinimize
            //
            this.btnMinimize.BackColor = Color.Transparent;
            this.btnMinimize.Location = new System.Drawing.Point(0, 0);
            this.btnMinimize.Size = new Size(25, 17);
            this.btnMinimize.Text = "Minimize";
            this.btnMinimize.ButtonStyle = ButtonStyle.Bitmap;
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizeHover.png");
            this.btnMinimize.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizePushed.png");
            this.btnMinimize.TabIndex = 2;
            this.btnMinimize.Click += new EventHandler(btnMinimize_Click);
            //
            // btnMaximize
            //
            this.btnMaximize.BackColor = Color.Transparent;
            this.btnMaximize.Location = new System.Drawing.Point(25, 0);
            this.btnMaximize.Size = new Size(26, 17);
            this.btnMaximize.Text = "Maximize";
            this.btnMaximize.ButtonStyle = ButtonStyle.Bitmap;
            this.btnMaximize.Name = "btnMaximize";
            this.btnMaximize.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizeHover.png");
            this.btnMaximize.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizePushed.png");
            this.btnMaximize.TabIndex = 3;
            this.btnMaximize.Click += new EventHandler(btnMaximize_Click);
            //
            // btnClose
            //
            this.btnClose.BackColor = Color.Transparent;
            this.btnClose.Location = new System.Drawing.Point(51, 0);
            this.btnClose.Size = new Size(42, 17);
            this.btnClose.Text = "Close";
            this.btnClose.ButtonStyle = ButtonStyle.Bitmap;
            this.btnClose.Name = "btnClose";
            this.btnClose.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.CloseHover.png");
            this.btnClose.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ClosePushed.png");
            this.btnClose.TabIndex = 4;
            this.btnClose.Click += new EventHandler(btnClose_Click);
            //
            // MinMaxClose
            //
            this.BackColor = Color.Transparent;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnMaximize);
            this.Controls.Add(this.btnMinimize);
            this.Name = "MinMaxClose";
            this.Size = new System.Drawing.Size(93, 17);
            this.ResumeLayout(false);

        }

        #endregion

        private static Point Center(Rectangle bounds)
        {
            return new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        }

        private void btnMinimize_Click(object sender, System.EventArgs e)
        {
            FindForm().WindowState = FormWindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, System.EventArgs e)
        {
            Form f = FindForm();
            switch (f.WindowState)
            {
                case FormWindowState.Maximized:
                    f.WindowState = FormWindowState.Normal;
                    break;
                case FormWindowState.Normal:
                    f.WindowState = FormWindowState.Maximized;
                    break;
                default:
                    Debug.Fail("Maximize/Restore clicked while minimized!?");
                    break;
            }
        }

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            FindForm().Close();
        }

    }
}
