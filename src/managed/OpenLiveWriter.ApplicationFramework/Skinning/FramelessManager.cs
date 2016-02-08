// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Interop.Windows;
using Timer=System.Windows.Forms.Timer;

namespace OpenLiveWriter.ApplicationFramework.Skinning
{
    [Obsolete("not needed", true)]
    public class FramelessManager : IFrameManager
    {

        private static event EventHandler AlwaysShowFrameChanged;

        private SatelliteApplicationForm _form;
        private UITheme _uiTheme;
        private MinMaxClose minMaxClose;
        private SizeBorderHitTester _sizeBorderHitTester = new SizeBorderHitTester(new Size(15, 15), new Size(16,16));
        private ColorizedResources res = ColorizedResources.Instance;
        private Timer _mouseFrameTimer;

        private int _activationCount = 0;

        private bool _inMenuLoop;
        private bool _forceFrame = false;
        private bool _alwaysShowFrame;
        private Size _overrideSize = Size.Empty;
        private Command _commandShowMenu;

        private const int SYSICON_TOP = 6;
        private const int SYSICON_LEFT = 5;

        const int SIZE_RESTORED       =0;
        const int SIZE_MINIMIZED      =1;
        const int SIZE_MAXIMIZED      =2;
        const int SIZE_MAXSHOW        =3;
        const int SIZE_MAXHIDE        =4;

        public FramelessManager(SatelliteApplicationForm form)
        {
            _alwaysShowFrame = AlwaysShowFrame;

            _form = form;

            minMaxClose = new MinMaxClose();
            minMaxClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _form.Controls.Add(minMaxClose);

            _mouseFrameTimer = new Timer();
            _mouseFrameTimer.Interval = 100;
            _mouseFrameTimer.Tick += new EventHandler(_mouseFrameTimer_Tick);

            _commandShowMenu = new Command(CommandId.ShowMenu);
            _commandShowMenu.Latched = AlwaysShowFrame;
            _commandShowMenu.Execute += new EventHandler(commandShowMenu_Execute);
            // JJA: no longer provide a frameless option
            //ApplicationManager.CommandManager.Add(_commandShowMenu);

            ColorizedResources.GlobalColorizationChanged += new EventHandler(ColorizedResources_GlobalColorizationChanged);

            _form.Layout += new LayoutEventHandler(_form_Layout);
            _form.Activated += new EventHandler(_form_Activated);
            _form.Deactivate += new EventHandler(_form_Deactivate);
            _form.SizeChanged += new EventHandler(_form_SizeChanged);
            _form.MouseDown +=new MouseEventHandler(_form_MouseDown);
            _form.DoubleClick +=new EventHandler(_form_DoubleClick);
            _form.Disposed += new EventHandler(_form_Disposed);

            AlwaysShowFrameChanged += new EventHandler(FramelessManager_AlwaysShowFrameChanged);

            _uiTheme = new UITheme(form);
        }

        private void commandShowMenu_Execute(object sender, EventArgs e)
        {
            AlwaysShowFrame = !_commandShowMenu.Latched;
        }

        public bool WndProc(ref Message m)
        {
            switch ((uint)m.Msg)
            {
                case 0x031E: // WM_DWMCOMPOSITIONCHANGED
                    {
                        // Aero was enabled or disabled. Force the UI to switch to
                        // the glass or non-glass style.
                        DisplayHelper.IsCompositionEnabled(true);
                        ColorizedResources.FireColorChanged();
                        break;
                    }
                case WM.SIZE:
                    {
                        uint size = (uint) m.LParam.ToInt32();
                        switch (m.WParam.ToInt32())
                        {
                            case SIZE_RESTORED:
                                _overrideSize = new Size((int) (size & 0xFFFF), (int) (size >> 16));
                                UpdateFrame();
                                break;
                            case SIZE_MAXIMIZED:
                                UpdateFrame();
                                break;
                            case SIZE_MAXHIDE:
                            case SIZE_MINIMIZED:
                            case SIZE_MAXSHOW:
                                break;
                        }
                        break;
                    }
                case WM.NCHITTEST:
                    {
                        // Handling this message gives us an easy way to make
                        // areas of the client region behave as elements of the
                        // non-client region. For example, the edges of the form
                        // can act as sizers.

                        Point p = _form.PointToClient(new Point(m.LParam.ToInt32()));

                        if (_form.ClientRectangle.Contains(p))
                        {
                            // only override the behavior if the mouse is within the client area

                            if (Frameless)
                            {
                                int result = _sizeBorderHitTester.Test(_form.ClientSize, p);
                                if (result != -1)
                                {
                                    m.Result = new IntPtr(result);
                                    return true;
                                }
                            }

                            if ( !PointInSystemIcon(p) )
                            {
                                // The rest of the visible areas of the form act like
                                // the caption (click and drag to move, double-click to
                                // maximize/restore).
                                m.Result = new IntPtr(HT.CAPTION);
                                return true;
                            }
                        }
                        break;
                    }

                case WM.ENTERMENULOOP:
                    if ( Control.MouseButtons == MouseButtons.None )
                    {
                        _inMenuLoop = true;
                        ForceFrame = true;
                    }
                    break;
                case WM.EXITMENULOOP:
                    if (_inMenuLoop)
                    {
                        _inMenuLoop = false;
                        if (!IsMouseInFrame())
                            ForceFrame = false;
                        else
                            _mouseFrameTimer.Start();
                    }
                    break;

            }
            return false;
        }

        private bool PointInSystemIcon(Point clientPoint)
        {
            return false;
            //return new Rectangle(SYSICON_LEFT, SYSICON_TOP, res.LogoImage.Width, res.LogoImage.Height).Contains(clientPoint) ;
        }

        private bool ForceFrame
        {
            get { return _forceFrame; }
            set
            {
                if (_forceFrame != value)
                {
                    _forceFrame = value;
                    UpdateFrame();
                    _form.Update();
                }
            }
        }

        public static bool AlwaysShowFrame
        {
            // JJA: no longer provide a frameless option
            get { return true; }
            //get { return ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").GetBoolean(SatelliteApplicationForm.SHOW_FRAME_KEY, false); }
            set
            {
                ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").SetBoolean(SatelliteApplicationForm.SHOW_FRAME_KEY, value);
                if (AlwaysShowFrameChanged != null)
                    AlwaysShowFrameChanged(null, EventArgs.Empty);
            }
        }

        public bool Frameless
        {
            // JJA: no longer provide a frameless option
            get { return false; }
            //get { return !_alwaysShowFrame && !_forceFrame && _form.WindowState != FormWindowState.Maximized && !_uiTheme.ForceShowFrame; }
        }

        private void _form_Layout(object sender, LayoutEventArgs e)
        {

            // align the min/max/close button to the top-right of the form
            minMaxClose.Left = _form.ClientSize.Width - minMaxClose.Width - 6;
            minMaxClose.Top = 0;
        }

        private void UpdateFrame()
        {
            minMaxClose.Visible = Frameless;

            if (Frameless)
            {
                SetRoundedRegion(_form, _overrideSize);
                _wasFrameless = true ;
            }
            else
            {
                // don't set a null region if we were never frameless
                // to begin with (eliminates the black corners that
                // appear when you set the region ot null)
                if ( _wasFrameless )
                    _form.Region = null;
            }

            _form.UpdateFramelessState(Frameless) ;

            _form.PerformLayout();
            _form.Invalidate(false);
        }
        private bool _wasFrameless = false ;

        public static void SetRoundedRegion(Form form, Size overrideSize)
        {
            int width, height;
            if (overrideSize == Size.Empty)
            {
                width = form.ClientSize.Width;
                height = form.ClientSize.Height;
            }
            else
            {
                width = overrideSize.Width;
                height = overrideSize.Height;
            }

            Region r = new Region(new Rectangle(3, 0, width - 6, height));
            r.Union(new Rectangle(2, 1, width - 4, height - 2));
            r.Union(new Rectangle(1, 2, width - 2, height - 4));
            r.Union(new Rectangle(0, 3, width, height - 6));

            RECT rect = new RECT();
            User32.GetWindowRect(form.Handle, ref rect);
            Point windowScreenPos = RectangleHelper.Convert(rect).Location;
            Point clientScreenPos = form.PointToScreen(new Point(0, 0));

            r.Translate(clientScreenPos.X - windowScreenPos.X, clientScreenPos.Y - windowScreenPos.Y);

            form.Region = r;
        }

        public void PaintBackground(PaintEventArgs e)
        {
            //using (new QuickTimer("Paint frameless app window. Clip: " + e.ClipRectangle.ToString()))
            {
                Graphics g = e.Graphics;
                g.InterpolationMode = InterpolationMode.Low;
                g.CompositingMode = CompositingMode.SourceCopy;

                Color light = res.FrameGradientLight;

                int width = _form.ClientSize.Width;
                int height = _form.ClientSize.Height;

                using (Brush b = new SolidBrush(light))
                    g.FillRectangle(b, 0, 0, width, height);

                // border
                if ( Frameless )
                    res.AppOutlineBorder.DrawBorder(g, _form.ClientRectangle);

                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighSpeed;

                Rectangle footerRect = new Rectangle(
                    0,
                    _form.ClientSize.Height - _form.DockPadding.Bottom,
                    _form.ClientSize.Width,
                    _form.DockPadding.Bottom);
                if (e.ClipRectangle.IntersectsWith(footerRect))
                    res.AppFooterBackground.DrawBorder(g, footerRect);
                    //GraphicsHelper.TileFillUnscaledImageHorizontally(g, res.FooterBackground, footerRect);

                Rectangle toolbarRect = new Rectangle(
                    _form.DockPadding.Left,
                    _form.DockPadding.Top,
                    _form.ClientSize.Width - _form.DockPadding.Left - _form.DockPadding.Right,
                    res.ToolbarBorder.MinimumHeight
                    );

                if (e.ClipRectangle.IntersectsWith(toolbarRect))
                {
                    res.ToolbarBorder.DrawBorder(g, toolbarRect);
                }

                // draw vapor
#if VAPOR
                Rectangle appVapor = AppVaporRectangle;
                if (e.ClipRectangle.IntersectsWith(appVapor))
                {
                    if (appVapor.Width == res.VaporImage.Width)
                    {
                        // NOTE: Try to bring Gdi painting back for performance (check with Joe on this)
                        //GdiPaint.BitBlt(g, minMaxClose.Faded ? res.VaporImageFadedHbitmap : res.VaporImageHbitmap, new Rectangle(Point.Empty, appVapor.Size), appVapor.Location);
                        g.DrawImage(minMaxClose.Faded ? res.VaporImageFaded : res.VaporImage, appVapor);
                    }
                    else
                    {
                        g.DrawImage(minMaxClose.Faded ? res.VaporImageFaded : res.VaporImage, appVapor);
                    }
                }
#endif

                g.CompositingQuality = CompositingQuality.HighQuality;

                /*
                if (Frameless)
                {
                    // gripper
                    g.DrawImage(res.GripperImage, width - 15, height - 15, res.GripperImage.Width, res.GripperImage.Height);

                    // draw window caption
                    //g.DrawIcon(ApplicationEnvironment.ProductIconSmall, 5, 6);
                    g.DrawImage(res.LogoImage, SYSICON_LEFT, SYSICON_TOP, res.LogoImage.Width, res.LogoImage.Height);

                    // determine caption font
                    NONCLIENTMETRICS metrics = new NONCLIENTMETRICS();
                    metrics.cbSize = Marshal.SizeOf(metrics);
                    User32.SystemParametersInfo(SPI.GETNONCLIENTMETRICS, 0, ref metrics, 0);
                    using ( Font font = new Font(
                                metrics.lfCaptionFont.lfFaceName,
                                Math.Min(-metrics.lfCaptionFont.lfHeight, 13),
                                (metrics.lfCaptionFont.lfItalic > 0 ? FontStyle.Italic : FontStyle.Regular) |
                                (metrics.lfCaptionFont.lfWeight >= 700 ? FontStyle.Bold : FontStyle.Regular),
                                GraphicsUnit.World))
                    {
                        using ( Brush brush = new SolidBrush(!minMaxClose.Faded ? Color.White : Color.FromArgb(140, Color.White)) )
                        {
                            // draw caption
                            g.DrawString(
                                ApplicationEnvironment.ProductName,
                                font,
                                brush,
                                22, 5 );
                        }
                    }
                }
                */
            }
        }

        private void _form_Activated(object sender, EventArgs e)
        {
            if (++_activationCount == 1)
            {
                minMaxClose.Faded = false;
                _form.Invalidate(false);
            }
        }

        private void _form_Deactivate(object sender, EventArgs e)
        {
            if (--_activationCount == 0)
            {
                minMaxClose.Faded = true;
                _form.Invalidate(false);
            }
        }

        private void _mouseFrameTimer_Tick(object sender, EventArgs e)
        {
            if (_inMenuLoop)
            {
                _mouseFrameTimer.Stop();
            }
            else if (!IsMouseInFrame())
            {
                ForceFrame = false;
                _mouseFrameTimer.Stop();
            }
        }

        private bool IsMouseInFrame()
        {

            return false;
            /*
            RECT rect = new RECT();
            User32.GetWindowRect(_form.Handle, ref rect);
            Rectangle windowRect = RectangleHelper.Convert(rect);
            return
                windowRect.Contains(Control.MousePosition) &&
                !_form.ClientRectangle.Contains(_form.PointToClient(Control.MousePosition));
            */
        }

        private void _form_SizeChanged(object sender, EventArgs e)
        {
            _overrideSize = Size.Empty;
        }

        private void ColorizedResources_GlobalColorizationChanged(object sender, EventArgs e)
        {
            try
            {
                if ( ControlHelper.ControlCanHandleInvoke(_form) )
                {
                    _form.BeginInvoke(new ThreadStart(RefreshColors));
                }
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.ToString());
            }
        }

        private void RefreshColors()
        {
            ColorizedResources colRes = ColorizedResources.Instance;
            colRes.Refresh();
            colRes.FireColorizationChanged();
            User32.SendMessage(_form.Handle, WM.NCPAINT, new UIntPtr(1), IntPtr.Zero ) ;
            _form.Invalidate(true);
            _form.Update();
        }

        private void _form_Disposed(object sender, EventArgs e)
        {
            ColorizedResources.GlobalColorizationChanged -= new EventHandler(ColorizedResources_GlobalColorizationChanged);
            AlwaysShowFrameChanged -= new EventHandler(FramelessManager_AlwaysShowFrameChanged);
        }

        private void FramelessManager_AlwaysShowFrameChanged(object sender, EventArgs e)
        {
            if (_form.IsDisposed)
            {
                return;
            }

            if (_form.InvokeRequired)
            {
                _form.BeginInvoke(new EventHandler(FramelessManager_AlwaysShowFrameChanged), new object[] {sender, e});
                return;
            }
            _alwaysShowFrame =
                ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").GetBoolean(SatelliteApplicationForm.SHOW_FRAME_KEY, false);
            UpdateFrame();
            _form.Update();
            _commandShowMenu.Latched = _alwaysShowFrame;

            Command commandMenu = ApplicationManager.CommandManager.Get(CommandId.Menu);
            if (commandMenu != null)
                commandMenu.On = !_alwaysShowFrame;
        }

        private void _hideFrame_Click(object sender, EventArgs e)
        {
            AlwaysShowFrame = false;
        }

        /// <summary>
        /// Prevents the vapor from appearing faded while the mini form is visible.
        /// This should only be used if the mini form will disappear when it loses
        /// focus (deactivated), otherwise the main form will be painted as activated
        /// even when it may not be.
        /// </summary>
        public void AddOwnedForm(Form f)
        {
            _form_Activated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Restore normal painting.
        /// </summary>
        public void RemoveOwnedForm(Form f)
        {
            _form_Deactivate(this, EventArgs.Empty);
        }

        private void _form_MouseDown(object sender, MouseEventArgs e)
        {
            /* JJA: Couldn't get both single and double-click working (would almost never
             * get the second click, presumably because the menu was up). We have decided
             * for the time being that double-click to close is a more important gesture
             * (for what it is worth, Windows Media Player also supports ONLY double-click
             * and not single-click). If we want to support both it is probably possible
             * however it will take some WndProc hacking.
             *
            if ( PointInSystemIcon( _form.PointToClient(Control.MousePosition) ) )
            {
                // handle single-click
                Point menuPoint = _form.PointToScreen(new Point(0, _form.DockPadding.Top)) ;
                IntPtr hSystemMenu = User32.GetSystemMenu(_form.Handle, false) ;
                int iCmd = User32.TrackPopupMenu(hSystemMenu, TPM.RETURNCMD | TPM.LEFTBUTTON, menuPoint.X, menuPoint.Y, 0, _form.Handle, IntPtr.Zero ) ;
                if ( iCmd != 0 )
                {
                    User32.SendMessage(_form.Handle, WM.SYSCOMMAND, new IntPtr(iCmd), MessageHelper.MAKELONG(Control.MousePosition.X, Control.MousePosition.Y)) ;
                }
            }
            */
        }

        private void _form_DoubleClick(object sender, EventArgs e)
        {
            if ( PointInSystemIcon( _form.PointToClient(Control.MousePosition) ) )
                _form.Close() ;
        }

        private class UITheme : ControlUITheme
        {
            public bool ForceShowFrame;
            //private FramelessManager _framelessManager;
//			public UITheme(Control control, FramelessManager framelessManager) : base(control, false)
            public UITheme(Control control)
                : base(control, false)
            {
                //_framelessManager = framelessManager;
                ApplyTheme();
            }

            protected override void ApplyTheme(bool highContrast)
            {
                ForceShowFrame = highContrast;
                //if(_framelessManager._form.Created)
                //{
                //    _framelessManager.UpdateFrame();
                //    _framelessManager._form.Update();
                //}
            }
        }
    }

}

