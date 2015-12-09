// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading ;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

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

		/// <summary>
		/// Background image
		/// </summary>
		private Bitmap _backgroundImage ;
		private Bitmap _logoImage ;

		public SplashScreen()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//	Turn off CS_CLIPCHILDREN.
			User32.SetWindowLong(Handle, GWL.STYLE, User32.GetWindowLong(Handle, GWL.STYLE) & ~WS.CLIPCHILDREN);

			//	Turn on double buffered painting.
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
            if (!BidiHelper.IsRightToLeft)
			    SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            _backgroundImage = new Bitmap(this.GetType(), "Images.SplashScreen.png");
            _logoImage = new Bitmap(this.GetType(), "Images.SplashScreenLogo.jpg");

            if (SystemInformation.HighContrast)
            {
                ImageHelper.ConvertToHighContrast(_backgroundImage);
                ImageHelper.ConvertToHighContrast(_logoImage);
            }
		}


		private const int WS_EX_TOOLWINDOW= 0x00000080; 
		private const int WS_EX_APPWINDOW=0x00040000; 
		private const int WS_EX_LAYERED=0x00080000; 
		protected override CreateParams CreateParams 
		{ 
			get 
			{ 
				CreateParams cp=base.CreateParams;
			    cp.ExStyle &= ~WS_EX_APPWINDOW; 
				cp.ExStyle |= WS_EX_TOOLWINDOW;
			    cp.ExStyle |= WS_EX_LAYERED;
				return cp; 
			} 
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateBitmap();
        }

        private void UpdateBitmap()
        {
            using (Bitmap bitmap = CreateBitmap())
            {
                IntPtr screenDC = User32.GetDC(IntPtr.Zero);
                try
                {
                    IntPtr memDC = Gdi32.CreateCompatibleDC(screenDC);
                    try
                    {
                        IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                        try
                        {
                            IntPtr hOrigBitmap = Gdi32.SelectObject(memDC, hBitmap);
                            try
                            {
                                POINT dst = new POINT();
                                dst.x = Left;
                                dst.y = Top;

                                SIZE size = new SIZE();
                                size.cx = bitmap.Width;
                                size.cy = bitmap.Height;

                                POINT src = new POINT();
                                src.x = 0;
                                src.y = 0;

                                User32.BLENDFUNCTION blendFunction = new User32.BLENDFUNCTION();
                                blendFunction.BlendOp = 0; // AC_SRC_OVER
                                blendFunction.BlendFlags = 0;
                                blendFunction.SourceConstantAlpha = 255;
                                blendFunction.AlphaFormat = 1; // AC_SRC_ALPHA

                                User32.UpdateLayeredWindow(Handle, screenDC, ref dst, ref size, memDC, ref src, 0, ref blendFunction, 2);
                            }
                            finally
                            {
                                Gdi32.SelectObject(memDC, hOrigBitmap);
                            }
                        }
                        finally
                        {
                            Gdi32.DeleteObject(hBitmap);
                        }
                    }
                    finally
                    {
                        Gdi32.DeleteDC(memDC);
                    }
                }
                finally
                {
                    User32.ReleaseDC(IntPtr.Zero, screenDC);
                }
            }
        }

		private Bitmap CreateBitmap()
		{
            Bitmap bitmap = new Bitmap(_backgroundImage.Width, _backgroundImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                BidiGraphics g = new BidiGraphics(graphics, bitmap.Size);

                // draw transparent background image
                g.DrawImage(false, _backgroundImage,
                            new Rectangle(0, 0, _backgroundImage.Width, _backgroundImage.Height));

                // draw logo image
                g.DrawImage(false, _logoImage, new Rectangle(
                    (ClientSize.Width - _logoImage.Width) / 2,
                    120 - _logoImage.Height,
                    _logoImage.Width,
                    _logoImage.Height));

                // draw copyright notice
                string splashText = Res.Get(StringId.SplashScreenCopyrightNotice);
                using (Font font = new Font(Font.FontFamily, 7.5f))
                {
                    const int TEXT_PADDING_H = 36;
                    const int TEXT_PADDING_V = 26;
                    int textWidth = Size.Width - 2*TEXT_PADDING_H;
                    int textHeight =
                        Convert.ToInt32(
                            g.MeasureText(splashText, font, new Size(textWidth, 0), TextFormatFlags.WordBreak).Height,
                            CultureInfo.InvariantCulture);

                    // GDI text can't be drawn on an alpha-blended surface. So we render a black-on-white
                    // bitmap, then use a ColorMatrix to effectively turn it into an alpha mask.

                    using (Bitmap textBitmap = new Bitmap(textWidth, textHeight, PixelFormat.Format32bppRgb))
                    {
                        using (Graphics tbG = Graphics.FromImage(textBitmap))
                        {
                            tbG.FillRectangle(Brushes.Black, 0, 0, textWidth, textHeight);
                            new BidiGraphics(tbG, textBitmap.Size).
                                DrawText(splashText, font, new Rectangle(0, 0, textWidth, textHeight), Color.White, Color.Black, TextFormatFlags.WordBreak);
                        }

                        Rectangle textRect = new Rectangle(TEXT_PADDING_H, ClientSize.Height - TEXT_PADDING_V - textHeight, textWidth, textHeight);
                        using (ImageAttributes ia = new ImageAttributes())
                        {
                            ColorMatrix cm = new ColorMatrix(new float[][]
                                                                 {
                                                                     new float[] {0, 0, 0, 1f/3f, 0},
                                                                     new float[] {0, 0, 0, 1f/3f, 0},
                                                                     new float[] {0, 0, 0, 1f/3f, 0},
                                                                     new float[] {0, 0, 0, 0, 0},
                                                                     new float[] {0.9372f, 0.9372f, 0.9372f, 0, 0},
                                                                 });
                            ia.SetColorMatrix(cm);
                            g.DrawImage(false, textBitmap, textRect, 0, 0, textWidth, textHeight, GraphicsUnit.Pixel, ia);
                        }
                    }
                }
            }
		    return bitmap;
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
			// 
			// SplashScreen
			// 
		    this.AutoScaleMode = AutoScaleMode.None;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(380, 235);
			this.Cursor = Cursors.AppStarting;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "SplashScreen";
		    //if this inherits Yes from the parent the screenshot of the background is reversed
            this.RightToLeftLayout = false;
            this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		}
		#endregion
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
		/// occuring during close/dispose)
		/// </summary>
		public void Dispose()
		{
			if ( form != null )
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
		private Form form ;
	}
}
