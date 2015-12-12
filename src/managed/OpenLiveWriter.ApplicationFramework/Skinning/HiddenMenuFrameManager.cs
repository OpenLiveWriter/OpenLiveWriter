// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using Timer=System.Windows.Forms.Timer;

namespace OpenLiveWriter.ApplicationFramework.Skinning
{
    public class HiddenMenuFrameManager : IFrameManager
    {
        private static event EventHandler AlwaysShowMenuChanged;

        private SatelliteApplicationForm _form;
        private ColorizedResources res = ColorizedResources.Instance;
        private Timer _mouseFrameTimer;

        private bool _inMenuLoop;
        private bool _forceMenu = false;
        private bool _alwaysShowMenu;
        private Command _commandShowMenu;

        const int SIZE_RESTORED       =0;
        const int SIZE_MINIMIZED      =1;
        const int SIZE_MAXIMIZED      =2;
        const int SIZE_MAXSHOW        =3;
        const int SIZE_MAXHIDE        =4;

        public HiddenMenuFrameManager(SatelliteApplicationForm form)
        {
            _alwaysShowMenu = AlwaysShowMenu;

            _form = form;
            _form.Load +=new EventHandler(_form_Load);

            _mouseFrameTimer = new Timer();
            _mouseFrameTimer.Interval = 100;
            _mouseFrameTimer.Tick += new EventHandler(_mouseFrameTimer_Tick);

            _commandShowMenu = new Command(CommandId.ShowMenu);
            _commandShowMenu.Latched = AlwaysShowMenu;
            _commandShowMenu.Execute += new EventHandler(commandShowMenu_Execute);
            ApplicationManager.CommandManager.Add(_commandShowMenu);

            ColorizedResources.GlobalColorizationChanged += new EventHandler(ColorizedResources_GlobalColorizationChanged);
            _form.Disposed += new EventHandler(_form_Disposed);

            AlwaysShowMenuChanged += new EventHandler(HiddenMenuFrameManager_AlwaysShowMenuChanged);
        }

        private void commandShowMenu_Execute(object sender, EventArgs e)
        {
            AlwaysShowMenu = !_commandShowMenu.Latched;
        }

        public bool WndProc(ref Message m)
        {
            switch ((uint)m.Msg)
            {
                case WM.NCHITTEST:
                {
                    Point p = _form.PointToClient(new Point(m.LParam.ToInt32()));

                    if (_form.ClientRectangle.Contains(p))
                    {
                        // gripper
                        Size gripperSize = new Size(15,15);
                        if (new Rectangle(
                            _form.ClientSize.Width - gripperSize.Width,
                            _form.ClientSize.Height - gripperSize.Height,
                            gripperSize.Width,
                            gripperSize.Height).Contains(p))
                        {
                            m.Result = new IntPtr(HT.BOTTOMRIGHT) ;
                            return true ;
                        }
                    }

                    break;
                }

                case WM.ENTERMENULOOP:
                    _inMenuLoop = true;
                    ForceMenu = true;
                    break;
                case WM.EXITMENULOOP:
                    _inMenuLoop = false;
                    if (!IsMouseInFrame())
                        ForceMenu = false;
                    else
                        _mouseFrameTimer.Start();
                    break;
            }
            return false;
        }

        private bool ForceMenu
        {
            get { return _forceMenu; }
            set
            {
                if (_forceMenu != value)
                {
                    // set value
                    _forceMenu = value;

                    // update appearance
                    UpdateMenuVisibility();

                    // attempt to force accelerators (this isn't working!)
                    /*
                    int stateChange = _forceFrame ? UIS.CLEAR : UIS.SET ;
                    User32.SendMessage(_form.Handle, WM.CHANGEUISTATE, new UIntPtr(Convert.ToUInt32(
                        MessageHelper.MAKELONG(stateChange, UISF.HIDEACCEL).ToInt32())), IntPtr.Zero);
                    //_form.Update();
                    */
                }
            }
        }

        public static bool AlwaysShowMenu
        {
            get { return ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").GetBoolean(SatelliteApplicationForm.SHOW_FRAME_KEY, false); }
            set
            {
                ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").SetBoolean(SatelliteApplicationForm.SHOW_FRAME_KEY, value);
                if (AlwaysShowMenuChanged != null)
                    AlwaysShowMenuChanged(null, EventArgs.Empty);
            }
        }


        private void UpdateMenuVisibility()
        {
            //_form.ShowMainMenu =  _alwaysShowMenu || _forceMenu ;
        }

        public void PaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.Low;
            g.CompositingMode = CompositingMode.SourceCopy;

            Color light = res.FrameGradientLight;

            int width = _form.ClientSize.Width;
            int height = _form.ClientSize.Height;

            using (Brush b = new SolidBrush(light))
                g.FillRectangle(b, 0, 0, width, height);

            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighSpeed;

            Rectangle bodyFrameRect = new Rectangle(
                _form.DockPadding.Left - 5,
                _form.DockPadding.Top,
                _form.ClientSize.Width - _form.DockPadding.Right - _form.DockPadding.Left + 5 + 5,
                _form.ClientSize.Height - _form.DockPadding.Top - _form.DockPadding.Bottom + 7);
            if (e.ClipRectangle.IntersectsWith(bodyFrameRect))
                res.AppBodyFrameBorder.DrawBorder(g, bodyFrameRect);

            Rectangle toolbarRect = new Rectangle(
                _form.DockPadding.Left - 1,
                _form.DockPadding.Top,
                _form.ClientSize.Width - _form.DockPadding.Left - _form.DockPadding.Right + 2,
                res.ToolbarBorder.MinimumHeight
                );
            if (e.ClipRectangle.IntersectsWith(toolbarRect))
                res.ToolbarBorder.DrawBorder(g, toolbarRect);

            g.CompositingQuality = CompositingQuality.HighQuality;

            // gripper
            g.DrawImage(res.GripperImage, width - 15, height - 15, res.GripperImage.Width, res.GripperImage.Height);

        }

        private void _mouseFrameTimer_Tick(object sender, EventArgs e)
        {
            if (_inMenuLoop)
            {
                _mouseFrameTimer.Stop();
            }
            else if (!IsMouseInFrame())
            {
                ForceMenu = false;
                _mouseFrameTimer.Stop();
            }
        }

        private bool IsMouseInFrame()
        {
            RECT rect = new RECT();
            User32.GetWindowRect(_form.Handle, ref rect);
            Rectangle windowRect = RectangleHelper.Convert(rect);
            return
                windowRect.Contains(Control.MousePosition) &&
                !_form.ClientRectangle.Contains(_form.PointToClient(Control.MousePosition));
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
            _form.Invalidate(true);
            _form.Update();
        }

        private void _form_Load(object sender, EventArgs e)
        {
            UpdateMenuVisibility() ;
        }

        private void _form_Disposed(object sender, EventArgs e)
        {
            ColorizedResources.GlobalColorizationChanged -= new EventHandler(ColorizedResources_GlobalColorizationChanged);
            AlwaysShowMenuChanged -= new EventHandler(HiddenMenuFrameManager_AlwaysShowMenuChanged);
        }

        private void HiddenMenuFrameManager_AlwaysShowMenuChanged(object sender, EventArgs e)
        {
            if (_form.IsDisposed)
            {
                return;
            }

            if (_form.InvokeRequired)
            {
                _form.BeginInvoke(new EventHandler(HiddenMenuFrameManager_AlwaysShowMenuChanged), new object[] {sender, e});
                return;
            }
            _alwaysShowMenu =
                ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").GetBoolean(SatelliteApplicationForm.SHOW_FRAME_KEY, false);
            UpdateMenuVisibility();
            _form.Update();
            _commandShowMenu.Latched = _alwaysShowMenu;

            Command commandMenu = ApplicationManager.CommandManager.Get(CommandId.Menu);
            if (commandMenu != null)
                commandMenu.On = !_alwaysShowMenu;
        }

        public void AddOwnedForm(Form f)
        {
            User32.SendMessage(_form.Handle, WM.ACTIVATE, new UIntPtr(1), IntPtr.Zero ) ;
        }

        /// <summary>
        /// Restore normal painting.
        /// </summary>
        public void RemoveOwnedForm(Form f)
        {
        }

    }
}
