// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.ApplicationFramework.ApplicationStyles;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Localization;
using Timer = System.Windows.Forms.Timer;

namespace OpenLiveWriter.ApplicationFramework
{
    public class SatelliteApplicationForm : ApplicationForm, IVirtualTransparencyHost
    {
        //protected IFrameManager _framelessManager;

        /// <summary>
        /// Pixel inset for workspace (allow derived classes to do custom layout within
        /// their workspace with knowledge of the border width)
        /// </summary>
        public static readonly int WorkspaceInset = 0;

        /// <summary>
        /// Create and open a satellite application form
        /// </summary>
        /// <param name="applicationFormType"></param>
        /// <param name="parameters"></param>
        public static void Open(Type applicationFormType, params object[] parameters)
        {
            Launcher launcher = new Launcher(applicationFormType, parameters);
            launcher.OpenForm();
        }

        public SatelliteApplicationForm()
        {
            // allow windows to determine the location for new windows
            StartPosition = FormStartPosition.WindowsDefaultLocation;

            // use standard product icon
            Icon = ApplicationEnvironment.ProductIcon;

            // subscribe to resize event
            Resize += new EventHandler(SatelliteApplicationForm_Resize);

            // subscribe to closing event
            Closing += new CancelEventHandler(SatelliteApplicationForm_Closing);

            //	Redraw if resized (mainly for the designer).
            SetStyle(ControlStyles.ResizeRedraw, true);

            DockPadding.Bottom = 0;
        }

        private bool ribbonLoaded = false;

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                SuspendLayout();

                // initialize the workspace
                OnInitializeWorkspace();
                ribbonLoaded = true;

                // restore window state
                RestoreWindowState();

                ResumeLayout();
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(Win32WindowImpl.DesktopWin32Window, ex);
                Close();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
            //_framelessManager.PaintBackground(pevent);
        }

        void IVirtualTransparencyHost.Paint(PaintEventArgs args)
        {
            //_framelessManager.PaintBackground(args);
            //OnPaint(args);
        }

        private bool restoring = false;
        private FormWindowState initialWindowState = FormWindowState.Normal;

        void SatelliteApplicationForm_Resize(object sender, EventArgs e)
        {
            restoring = (WindowState == FormWindowState.Normal && initialWindowState != FormWindowState.Normal);
            initialWindowState = WindowState;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // WinLive 40828: Writer window's height keeps growing when each time Writer window is restored.
            // This is a hack.
            // For some unknown reason after we have the ribbon hooked up, the height parameter passed to this
            // method when the form is restored from a minimized/maximized state is ~30px too large (depending
            // on DPI). However, the this.Height property is correct, so we can just use it instead.
            int newHeight = (ribbonLoaded && restoring) ? this.Height : height;
            base.SetBoundsCore(x, y, width, newHeight, specified);
        }

        /// <summary>
        /// Initialize the state of the workspace -- only override this for advanced
        /// customization of the workspace. The default implementation queries the
        /// the subclass for the UI to initialize with via the FirstCommandBarDefinition,
        /// SecondCommandBarDefinition, and PrimaryControl properties
        /// </summary>
        protected virtual void OnInitializeWorkspace()
        {
            // Hmm.  How to do this?
            //
            _mainControl = CreateMainControl();

            //CommandBarLightweightControl commandBar = new ApplicationCommandBarLightweightControl();
            //commandBar.CommandManager = ApplicationManager.CommandManager;
            //commandBar.LeftContainerOffSetSpacing = 0;
            //_commandBarControl = new TransparentCommandBarControl(commandBar, FirstCommandBarDefinition);
            //_commandBarControl.Height = (int)Ribbon.GetHeight() + 34;
            //_commandBarControl.Height = 34;

            //_commandBarControl.Dock = DockStyle.Top;

            PositionMainControl();

            //Controls.Add(_commandBarControl);
            Controls.Add(_mainControl);
        }

        protected void PositionMainControl()
        {
            int TOP_PADDING = ScaleY(0); //26);
            int SIDE_PADDING = ScaleX(0);
            int BOTTOM_PADDING = ScaleY(0);//25) ;

            if (NEW_FRAME_UI_ENABLED)
            {
                TOP_PADDING = 0;
                SIDE_PADDING = 0;
                BOTTOM_PADDING = ScaleY(0);//25) ;
            }

            DockPadding.Top = TOP_PADDING;
            DockPadding.Left = SIDE_PADDING;
            DockPadding.Right = SIDE_PADDING;
            DockPadding.Bottom = BOTTOM_PADDING;

            _mainControl.Dock = DockStyle.Fill;
        }

        #region High DPI Scaling
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        private void SaveScale(float dx, float dy)
        {
            scale = new PointF(scale.X * dx, scale.Y * dy);
        }
        private PointF scale = new PointF(1f, 1f);

        protected int ScaleX(int x)
        {
            return (int)(x * scale.X);
        }

        protected int ScaleY(int y)
        {
            return (int)(y * scale.Y);
        }
        #endregion

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            // force command bar to re-layout
            //if ( _commandBarControl != null )
            //    _commandBarControl.ForceLayout();
        }

        private const int MESSAGE_PRIVATE_BASE = 0x7334;
        private const int MESSAGE_PERFLOG_FLUSH = MESSAGE_PRIVATE_BASE;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WM.QUERYENDSESSION:
                    m.Result = new IntPtr(1);
                    return;
                case (int)WM.ENDSESSION:
                    {
                        bool isSessionEnding = Convert.ToBoolean(m.WParam.ToInt32()) &&
                                               (((uint)m.LParam.ToInt32() | ENDSESSION.ENDSESSION_CLOSEAPP) != 0 ||
                                                ((uint)m.LParam.ToInt32() | ENDSESSION.ENDSESSION_CRITICAL) != 0 ||
                                                ((uint)m.LParam.ToInt32() | ENDSESSION.ENDSESSION_LOGOFF) != 0);

                        if (isSessionEnding)
                        {
                            ((ISessionHandler)_mainControl).OnEndSession();
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    break;
                case MESSAGE_PERFLOG_FLUSH:
                    if (ApplicationPerformance.IsEnabled)
                        ApplicationPerformance.FlushLogFile();
                    return;
            }

            //if (_framelessManager != null && !_framelessManager.WndProc(ref m))
            base.WndProc(ref m);
        }

        internal void UpdateFramelessState(bool frameless)
        {
            if (!NEW_FRAME_UI_ENABLED)
            {
                SuspendLayout();
                if (frameless)
                {
                    DockPadding.Top = 26;
                    DockPadding.Left = 7;
                    DockPadding.Right = 7;
                }
                else
                {
                    DockPadding.Top = 0;
                    DockPadding.Left = 0;
                    DockPadding.Right = 0;
                }
                ResumeLayout();
            }
        }

        // overrideable methods used to customize the UI of the satellite form
        //protected virtual CommandBarDefinition FirstCommandBarDefinition { get { return null; }	}

        protected virtual Control CreateMainControl() { return null; }

        /// <summary>
        /// The primary command bar control.
        /// </summary>
        //protected virtual Control CommandBarControl
        //{
        //    get{ return _commandBarControl; }
        //}

        // overrieable processing methods
        protected virtual void OnBackgroundTimerTick() { }

        // override to let base know if the main menu is visible
        protected override bool MainMenuVisible
        {
            get
            {
                //if ( _framelessManager != null )
                //	return !_framelessManager.Frameless ;
                //else
                return false; // true;
            }
        }

        private void SatelliteApplicationForm_Closing(object sender, CancelEventArgs e)
        {
            // give the main control first crack at cancelling the form close
            if (_mainControl is IFormClosingHandler)
            {
                ((IFormClosingHandler)_mainControl).OnClosing(e);
                if (e.Cancel)
                    return;

                ((IFormClosingHandler)_mainControl).OnClosed();
            }

            // save the current window state
            if (WindowState == FormWindowState.Normal)
                SaveNormalWindowState();
            else if (WindowState == FormWindowState.Maximized)
                SaveMaximizedWindowState();
        }

        protected virtual void SaveNormalWindowState()
        {
        }

        protected virtual void SaveMaximizedWindowState()
        {
        }

        protected virtual void RestoreWindowState()
        {
        }

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
        private IContainer components = new Container();

        /// <summary>
        /// Reference to main control hosted on the form
        /// </summary>
        private Control _mainControl;

        /// <summary>
        /// Reference to command bar control
        /// </summary>
        //private TransparentCommandBarControl _commandBarControl;

        /// <summary>
        /// Helper class which manages creating a new thread for the form and
        /// creating and running the form on that thread
        /// </summary>
        private class Launcher
        {
            public Launcher(Type formType, params object[] parameters)
            {
                _formType = formType;
                _parameters = parameters;
            }

            public void OpenForm()
            {
                // open a new form on a non-background, STA thread

                using (ProcessKeepalive.Open()) // throws exception if we are shutting down
                {
                    // this object will be signalled when the new thread has
                    // finished incrementing
                    object signalIncremented = new object();

                    Thread formThread = ThreadHelper.NewThread(
                        ThreadStartWithParams.Create(new ThreadStartWithParamsDelegate(ThreadMain), signalIncremented),
                        "FormThread",
                        true,
                        true,
                        false);

                    lock (signalIncremented)
                    {
                        formThread.Start();
                        // Don't continue until refcount has been incremented
                        Monitor.Wait(signalIncremented);
                    }
                }
            }

            [STAThread]
            private void ThreadMain(object[] parameters)
            {
                IDisposable splashScreen = null;
                if (parameters.Length > 0)
                    splashScreen = parameters[parameters.Length - 1] as IDisposable;

                ProcessKeepalive pk = null;
                try
                {
                    try
                    {
                        pk = ProcessKeepalive.Open();
                    }
                    finally
                    {
                        object signalIncremented = parameters[0];
                        lock (signalIncremented)
                            Monitor.Pulse(parameters[0]);
                    }

                    // housekeeping initialization
                    Application.OleRequired();
                    UnexpectedErrorDelegate.RegisterWindowsHandler();

                    // Create and run the form
                    SatelliteApplicationForm applicationForm = (SatelliteApplicationForm)Activator.CreateInstance(_formType, _parameters);
                    Application.Run(applicationForm);
                }
                catch (Exception ex)
                {
                    UnexpectedErrorMessage.Show(ex);
                }
                finally
                {
                    if (pk != null)
                        pk.Dispose();
                    if (splashScreen != null)
                    {
                        Debug.Assert(splashScreen is FormSplashScreen);
                        splashScreen.Dispose();
                    }
                }

            }

            private Type _formType;
            private object[] _parameters;
        }

        private static readonly bool NEW_FRAME_UI_ENABLED = false;
        internal const string SHOW_FRAME_KEY = "AlwaysShowMenu";
    }

    public interface IFrameManager
    {
        void PaintBackground(PaintEventArgs pevent);
        bool WndProc(ref Message m);
        void AddOwnedForm(Form f);
        void RemoveOwnedForm(Form f);
        bool Frameless { get; }
    }

    /// <summary>
    /// Interface for optionally intercepting the Form.Closing event
    /// </summary>
    public interface IFormClosingHandler
    {
        void OnClosing(CancelEventArgs e);
        void OnClosed();
    }

}
