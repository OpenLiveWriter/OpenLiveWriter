// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.ApplicationFramework.Skinning;

// @RIBBON TODO: Need to cleanly remove the UI code is made obsolete by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Application form.
    /// </summary>
    public class ApplicationForm : BaseForm, IMainMenuBackgroundPainter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// Initializes a new instance of the ApplicationForm class.
        /// </summary>
        public ApplicationForm()
        {
            //	Shut up!
            if (components == null)
                components = null;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            Text = ApplicationEnvironment.ProductNameQualified;

            //	Turn on double buffered painting.
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            if (!BidiHelper.IsRightToLeft)
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            //	Attach our command manager event handler.
            ColorizedResources.Instance.ColorizationChanged += new EventHandler(Instance_ColorizationChanged);
            SizeChanged += new EventHandler(ApplicationForm_SizeChanged);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                /* Explicit removal of the menu causes weird close-time jerkiness on Vista. There's a finalizer on the menu, it will get disposed eventually.
                if (Menu != null)
                {
                    MainMenu oldMainMenu = Menu;
                    Menu = null;
                    oldMainMenu.Dispose();
                }
                */
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
            this.SuspendLayout();
            //
            // ApplicationForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(442, 422);
            this.Name = "ApplicationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Raises the Closed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnClosed(e);

            //	Detach our command management event handler.
            ColorizedResources.Instance.ColorizationChanged -= new EventHandler(Instance_ColorizationChanged);
            SizeChanged -= new EventHandler(ApplicationForm_SizeChanged);
        }

        /// <summary>
        /// Disposed a menu and all of its submenu items.
        /// </summary>
        /// <param name="menu"></param>
        private void DisposeMenu(Menu menu)
        {
            if (menu != null)
            {
                foreach (MenuItem subMenu in menu.MenuItems)
                {
                    DisposeMenu(subMenu);
                }
                menu.Dispose();
            }
        }

        #region Custom Main Menu Background Painting

        protected virtual bool MainMenuVisible
        {
            get { return false; } // true; }
        }

        private void Instance_ColorizationChanged(object sender, EventArgs e)
        {
            UpdateMainMenuBrush();
        }

        private void ApplicationForm_SizeChanged(object sender, EventArgs e)
        {
            //UpdateMainMenuBrush() ;
        }

        private void UpdateMainMenuBrush()
        {
            // screen states where there is no menu yet
            if (Menu == null)
                return;

            if (WindowState == FormWindowState.Minimized)
                return;

            // alias colorized resources
            ColorizedResources cres = ColorizedResources.Instance;

            // dispose any existing brush and/or bitmaps
            if (_hMainMenuBrush != IntPtr.Zero)
                DisposeGDIObject(ref _hMainMenuBrush);
            if (_hMainMenuBitmap != IntPtr.Zero)
                DisposeGDIObject(ref _hMainMenuBitmap);
            if (_hMainMenuBrushBitmap != IntPtr.Zero)
                DisposeGDIObject(ref _hMainMenuBrushBitmap);
            if (_mainMenuBitmap != null)
            {
                Bitmap tmp = _mainMenuBitmap;
                _mainMenuBitmap = null;
                tmp.Dispose();
            }

            // create a brush which contains the menu background
            _mainMenuBitmap = new Bitmap(Width, -RelativeWindowBounds.Y);
            _hMainMenuBitmap = _mainMenuBitmap.GetHbitmap();
            using (Graphics g = Graphics.FromImage(_mainMenuBitmap))
            {
                Rectangle bounds = MenuBounds;
                Debug.WriteLine("MenuBounds: " + bounds);
                if (cres.CustomMainMenuPainting)
                {
                    // paint custom menu background
                    CustomPaintMenuBackground(g, bounds);
                }
                else
                {
                    using (Brush brush = new SolidBrush(SystemMainMenuColor))
                        g.FillRectangle(brush, bounds);
                }
            }

            _hMainMenuBrushBitmap = _mainMenuBitmap.GetHbitmap();
            _hMainMenuBrush = Gdi32.CreatePatternBrush(_hMainMenuBrushBitmap);

            // set the brush
            MENUINFO mi = new MENUINFO();
            mi.cbSize = Marshal.SizeOf(typeof(MENUINFO));
            mi.fMask = MIM.BACKGROUND;
            mi.hbrBack = _hMainMenuBrush;
            User32.SetMenuInfo(Menu.Handle, ref mi);
            User32.DrawMenuBar(Handle);
        }

        private void DisposeGDIObject(ref IntPtr hObject)
        {
            IntPtr tmp = hObject;
            hObject = IntPtr.Zero;
            Gdi32.DeleteObject(tmp);
        }

        private void CustomPaintMenuBackground(Graphics g, Rectangle menuBounds)
        {
            ColorizedResources cres = ColorizedResources.Instance;

            // Fill in the background--important for narrow window sizes when the main menu items are stacked
            using (Brush brush = new SolidBrush(cres.MainMenuGradientTopColor))
                g.FillRectangle(brush, new Rectangle(0, 0, Width, -RelativeWindowBounds.Y));

            // Fill in the gradient
            using (Brush brush = new LinearGradientBrush(menuBounds, cres.MainMenuGradientTopColor, cres.MainMenuGradientBottomColor, LinearGradientMode.Vertical))
                g.FillRectangle(brush, menuBounds);
        }

        /// <summary>
        /// The bounds of the menu, relative to the outer bounds of the window.
        /// </summary>
        private Rectangle MenuBounds
        {
            get
            {
                return new Rectangle(new Point(-RelativeWindowBounds.X, -RelativeWindowBounds.Y - SystemInformation.MenuHeight),
                                     new Size(ClientSize.Width, SystemInformation.MenuHeight));
            }
        }

        /// <summary>
        /// The outer bounds of the window, expressed in client coordinates.
        /// </summary>
        private Rectangle RelativeWindowBounds
        {
            get
            {
                RECT rect = new RECT();
                User32.GetWindowRect(Handle, ref rect);
                return RectangleToClient(RectangleHelper.Convert(rect));
            }
        }

        private IntPtr _hMainMenuBrush = IntPtr.Zero;
        private Bitmap _mainMenuBitmap = null;
        private IntPtr _hMainMenuBitmap = IntPtr.Zero;
        private IntPtr _hMainMenuBrushBitmap = IntPtr.Zero;

        void IMainMenuBackgroundPainter.PaintBackground(Graphics g, Rectangle menuItemBounds)
        {
            if (ColorizedResources.Instance.CustomMainMenuPainting)
            {
                if (_mainMenuBitmap != null)
                {
                    try
                    {
                        // This has to be GDI+, not GDI, in order for the menu text
                        // anti-aliasing to look good
                        g.DrawImage(
                            _mainMenuBitmap,
                            menuItemBounds,
                            menuItemBounds,
                            GraphicsUnit.Pixel);
                        return;
                    }
                    catch
                    {
                        Debug.Fail("Buffered paint of menu background failed");
                    }
                }

                try
                {
                    GraphicsState graphicsState = g.Save();
                    try
                    {
                        g.SetClip(menuItemBounds);
                        CustomPaintMenuBackground(g, menuItemBounds);
                    }
                    finally
                    {
                        g.Restore(graphicsState);
                    }
                }
                catch
                {
                    Debug.Fail("Unbuffered paint of menu background also failed");
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(SystemMainMenuColor))
                    g.FillRectangle(brush, 0, 0, menuItemBounds.Width, menuItemBounds.Height);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch ((uint)m.Msg)
            {
                case WM.NCPAINT:
                case WM.NCACTIVATE:
                case WM.ERASEBKGND:
                    {
                        if (MainMenuVisible)
                            NonClientPaint(ref m);
                        break;
                    }
            }
        }

        private void NonClientPaint(ref Message m)
        {
            try
            {
                // screen states where we can't do the nc paint
                if (!IsHandleCreated)
                    return;
                if (_mainMenuBitmap == null)
                    return;

                IntPtr hDC = User32.GetWindowDC(Handle);
                try
                {
                    using (Graphics g = Graphics.FromHdc(hDC))
                    {
                        // destination rect
                        int frameWidth = User32.GetSystemMetrics(SM.CXSIZEFRAME);
                        Debug.Assert(frameWidth != 0);
                        int frameHeight = User32.GetSystemMetrics(SM.CYSIZEFRAME);
                        Debug.Assert(frameHeight != 0);

                        /*
                        Rectangle destinationRect = new Rectangle(
                            frameWidth,
                            frameHeight + SystemInformation.CaptionHeight + SystemInformation.MenuHeight - 1,
                            ClientSize.Width, 1) ;
                        */
                        // takes into account narrow window sizes where the main menu items start stacking vertically
                        Rectangle destinationRect = new Rectangle(-RelativeWindowBounds.X, -RelativeWindowBounds.Y - 1, ClientSize.Width, 1);

                        if (ColorizedResources.Instance.CustomMainMenuPainting)
                        {
                            // source rect (from menu bitmap)
                            Rectangle sourceRect = new Rectangle(
                                destinationRect.X,
                                _mainMenuBitmap.Height - 1,
                                ClientSize.Width,
                                1);

                            //GdiPaint.BitBlt(g, _hMainMenuBitmap, sourceRect, destinationRect.Location);
                            g.DrawImage(_mainMenuBitmap, destinationRect, sourceRect, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            using (Brush b = new SolidBrush(SystemMainMenuColor))
                                g.FillRectangle(b, destinationRect);
                        }
                    }
                }
                finally
                {
                    User32.ReleaseDC(Handle, hDC);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception during non-client painting: " + ex.ToString());
            }
        }

        private Color SystemMainMenuColor
        {
            get { return ColorizedResources.Instance.WorkspaceBackgroundColor; }
        }

        #endregion
    }
}
