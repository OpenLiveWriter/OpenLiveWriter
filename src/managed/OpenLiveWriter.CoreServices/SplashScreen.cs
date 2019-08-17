// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for SplashScreen.
    /// </summary>
    public class SplashScreen : BaseForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private Label labelStatus;
        private Bitmap _logoBitmap;
        private PictureBox pictureBoxLogo;
        private Bitmap _fdnLogoBitmap;
        private PictureBox pictureBoxFdnLogo;
        private System.Windows.Forms.Timer timerAnimation;

        private int _ticks = 0;

        public SplashScreen()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // TODO Use Strings resources. Waiting on LocUtil changes and LocEdit to be merged into master before this can be done.

            DisplayHelper.Scale(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadScaledImages();
            FixLayout();

            // Create the timer
            timerAnimation = new System.Windows.Forms.Timer();
            timerAnimation.Interval = 17; // 60 FPS rounded up
            timerAnimation.Tick += new EventHandler(AnimationTick);
            timerAnimation.Enabled = true;
            timerAnimation.Start();
        }

        private void LoadScaledImages()
        {
            const float scaleFactor = 2f; // Assume logos are already at 2x scaling
            var fdnLogoBmp = new Bitmap(this.GetType(), "Images.NetFoundationLogo2x.png");
            var logoBmp = new Bitmap(this.GetType(), "Images.SplashScreenLogo2x.png");

            var fdnLogoSize = new Size(
                (int)Math.Ceiling(fdnLogoBmp.Width * (DisplayHelper.ScalingFactorX / scaleFactor)),
                (int)Math.Ceiling(fdnLogoBmp.Height * (DisplayHelper.ScalingFactorY / scaleFactor)));
            _fdnLogoBitmap = new Bitmap(fdnLogoBmp, fdnLogoSize);
            pictureBoxFdnLogo.Image = _fdnLogoBitmap;
            pictureBoxFdnLogo.Size = _fdnLogoBitmap.Size;

            var logoBmpSize = new Size(
                (int)Math.Ceiling(logoBmp.Width * (DisplayHelper.ScalingFactorX / scaleFactor)),
                (int)Math.Ceiling(logoBmp.Height * (DisplayHelper.ScalingFactorY / scaleFactor)));
            _logoBitmap = new Bitmap(logoBmp, logoBmpSize);
            pictureBoxLogo.Image = _logoBitmap;
            pictureBoxLogo.Size = _logoBitmap.Size;
        }

        private void FixLayout()
        {
            pictureBoxLogo.Top = Height / 2 - pictureBoxLogo.Height / 2;
        }

        public void ShowSplashScreen()
        {
            Thread thread = new Thread(() =>
            {
                ShowDialog();
            });
            thread.Name = "Splash Screen Animation Thread";
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelStatus = new System.Windows.Forms.Label();
            this.pictureBoxFdnLogo = new System.Windows.Forms.PictureBox();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFdnLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.ForeColor = System.Drawing.Color.White;
            this.labelStatus.Location = new System.Drawing.Point(19, 214);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(10, 0, 0, 10);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(57, 15);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "Starting...";
            // 
            // pictureBoxFdnLogo
            // 
            this.pictureBoxFdnLogo.ErrorImage = null;
            this.pictureBoxFdnLogo.InitialImage = null;
            this.pictureBoxFdnLogo.Location = new System.Drawing.Point(20, 20);
            this.pictureBoxFdnLogo.Name = "pictureBoxFdnLogo";
            this.pictureBoxFdnLogo.Size = new System.Drawing.Size(20, 20);
            this.pictureBoxFdnLogo.TabIndex = 2;
            this.pictureBoxFdnLogo.TabStop = false;
            this.pictureBoxFdnLogo.Visible = false;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.ErrorImage = null;
            this.pictureBoxLogo.InitialImage = null;
            this.pictureBoxLogo.Location = new System.Drawing.Point(20, 92);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(20, 20);
            this.pictureBoxLogo.TabIndex = 3;
            this.pictureBoxLogo.TabStop = false;
            this.pictureBoxLogo.Visible = false;
            // 
            // SplashScreen
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(162)))), ((int)(((byte)(93)))), ((int)(((byte)(162)))));
            this.ClientSize = new System.Drawing.Size(439, 248);
            this.Controls.Add(this.pictureBoxLogo);
            this.Controls.Add(this.pictureBoxFdnLogo);
            this.Controls.Add(this.labelStatus);
            this.Cursor = System.Windows.Forms.Cursors.AppStarting;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SplashScreen";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFdnLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void AnimationTick(object sender, EventArgs e)
        {
            // .NET Foundation Logo linear slide animation
            const int fdnLogoAnimTicks = 10;
            const int fdnLogoAnimTarget = 20;
            pictureBoxFdnLogo.Left = (int)Math.Min(DisplayHelper.ScalingFactorX * _ticks * ((float)fdnLogoAnimTarget / fdnLogoAnimTicks), DisplayHelper.ScalingFactorX * fdnLogoAnimTarget);
            pictureBoxFdnLogo.Image = ChangeOpacity(_fdnLogoBitmap, (float)Math.Min((float)_ticks / fdnLogoAnimTicks, 1.0));
            pictureBoxFdnLogo.Visible = true;

            // Open Live Writer logo non-linear slide animation
            const int logoAnimStart = 4;
            const int logoAnimTicks = 16;
            int logoAnimTarget = Width / 2 - _logoBitmap.Width / 2;
            const int logoAnimSlideWidth = 30;

            if (_ticks >= logoAnimStart)
            {
                // Decimal animation curve from 0 to 1
                double x = 1 - Math.Pow(Math.Max(1 - ((double)(_ticks - logoAnimStart) / logoAnimTicks), 0), 2);
                pictureBoxLogo.Left = 
                    (logoAnimTarget - (int)(logoAnimSlideWidth * DisplayHelper.ScalingFactorX)) + (int)Math.Ceiling(x * (logoAnimSlideWidth * DisplayHelper.ScalingFactorX));
                pictureBoxLogo.Image = ChangeOpacity(_logoBitmap, (float)Math.Min((float)(_ticks - logoAnimStart) / logoAnimTicks, 1.0));
                pictureBoxLogo.Visible = true;
            }
            
            Update();
            _ticks++;
        }

        private static Bitmap ChangeOpacity(Image img, float opacityvalue)
        {
            // Example from https://www.codeproject.com/Tips/201129/Change-Opacity-of-Image-in-C

            Bitmap bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
            Graphics graphics = Graphics.FromImage(bmp);
            ColorMatrix colormatrix = new ColorMatrix();
            colormatrix.Matrix33 = opacityvalue;
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            graphics.Dispose();   // Releasing all resource used by graphics 
            return bmp;
        }
    }

    /// <summary>
    /// Implementation of a splash screen connected to a form
    /// </summary>
    public class FormSplashScreen : IDisposable
    {
        public FormSplashScreen(Form form)
        {
            this.form = form;
        }

        public Form Form
        {
            get { return form; }
        }

        /// <summary>
        /// Close the form (do it only once and defend against exceptions
        /// occurring during close/dispose)
        /// </summary>
        public void Dispose()
        {
            if (form != null)
            {
                if (form.InvokeRequired)
                {
                    form.BeginInvoke(new ThreadStart(Dispose));
                }
                else
                {
                    try
                    {
                        form.Close();
                        form.Dispose();
                    }
                    finally
                    {
                        form = null;
                    }
                }
            }
        }

        /// <summary>
        /// Form instance
        /// </summary>
        private Form form;
    }
}
