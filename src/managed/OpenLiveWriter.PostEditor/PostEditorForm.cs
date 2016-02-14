// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.BlogProviderButtons;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Controls;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.PostEditor.Updates;
using OpenLiveWriter.Localization.Bidi;

// @RIBBON TODO: Cleanly remove code made obsolete by the ribbon.

namespace OpenLiveWriter.PostEditor
{
    public class PostEditorForm : SatelliteApplicationForm, IMainFrameWindow, IBlogContext, ICommandManagerHost
    {

        public static void Launch(IDisposable splashScreen)
        {
            Launch(BlogSettings.DefaultBlogId, splashScreen);
        }

        public static void Launch(string blogId, IDisposable splashScreen)
        {
            if (blogId == null)
            {
                blogId = BlogSettings.DefaultBlogId;
            }

            Launch(new BlogPostEditingContext(blogId, new BlogPost()), splashScreen);
        }

        public static void Launch(IBlogPostEditingContext editingContext)
        {
            Launch(editingContext, null);
        }

        public static void Launch(IBlogPostEditingContext editingContext, IDisposable splashScreen)
        {
            Launch(editingContext, false, splashScreen);
        }

        public static void Launch(IBlogPostEditingContext editingContext, bool synchronizePost)
        {
            Launch(editingContext, synchronizePost, null);
        }

        public static void Launch(IBlogPostEditingContext editingContext, bool synchronizePost, IDisposable splashScreen)
        {
            SatelliteApplicationForm.Open(typeof(PostEditorForm), editingContext, synchronizePost, splashScreen);
        }

        public PostEditorForm(IBlogPostEditingContext editingContext, IDisposable splashScreen)
            : this(editingContext, false, splashScreen)
        {
        }

        public PostEditorForm(IBlogPostEditingContext editingContext, bool synchronizePost, IDisposable splashScreen)
        {
            CommonInit();

            _initialEditingContext = editingContext;
            _synchronizePost = synchronizePost;
            _splashScreen = splashScreen;
        }

        #region IBlogContext Members

        public string CurrentAccountId
        {
            get
            {
                return _postEditorMainControl.CurrentBlogId;
            }
        }

        #endregion

        private void CommonInit()
        {
            // Auto RTL fixup is way too expensive, since it happens
            // after many controls are already created and showing
            // onscreen. Do manual RTL mirroring instead.
            SuppressAutoRtlFixup = true;

            _blogClientUIContextScope = new BlogClientUIContextScope(this);

            //InitializeStatusBar();

            Icon = ApplicationEnvironment.ProductIcon;

            AutoScaleMode = AutoScaleMode.None;

            if (ApplicationPerformance.IsEnabled && !startupLogged)
            {
                Application.Idle += LogStartupPerf;
            }

            BackColor = Color.Red;
        }

        private static bool startupLogged = false;
        private static void LogStartupPerf(object sender, EventArgs args)
        {
            Application.Idle -= LogStartupPerf;
            if (!startupLogged)
            {
                startupLogged = true;
                DateTime endTime = DateTime.Now;
                DateTime startTime = Process.GetCurrentProcess().StartTime;
                long millis = (long)Math.Round((endTime - startTime).TotalMilliseconds);
                ApplicationPerformance.WriteEvent("StartupToIdle", millis);
            }
        }

        protected override void OnInitializeWorkspace()
        {
            base.OnInitializeWorkspace();

            //CommandBarControl.AccessibleName = "Primary Toolbar";
            //init the focusable panes
            //_paneFocusManager.AddControl(new FocusableControl(CommandBarControl));
            foreach (IFocusableControl focusControl in _postEditorMainControl.GetFocusPanes())
                _paneFocusManager.AddControl(focusControl);

            //ApplyAutoScaling();
            DisplayHelper.Scale(this);
            PositionMainControl();

            //InitializeStatusBarControl();
        }

        //protected override CommandBarDefinition FirstCommandBarDefinition
        //{
        //    get
        //    {
        //        return _postEditorMainControl.FirstCommandBarDefinition ;
        //    }
        //}

        void IMiniFormOwner.AddOwnedForm(Form f)
        {
            // Prevents the vapor from appearing faded while the mini form is visible.
            //_framelessManager.AddOwnedForm(f);
            this.AddOwnedForm(f);
        }

        void IMiniFormOwner.RemoveOwnedForm(Form f)
        {
            this.RemoveOwnedForm(f);
            //_framelessManager.RemoveOwnedForm(f);
        }

        protected override void OnLoad(EventArgs e)
        {
            Hide();
            SuspendLayout();

            // We defer post-synchronization until OnLoad because showing a dialog (which
            // occurs if the sync takes more than a predefined interval) too early in
            // the life of the thread prevents the main post editor form from being able
            // to subsequently come to the foreground (no idea why). Our approach is
            // therefore to allow callers to request this service in the constructor
            // and then have it be executed here,
            if (_synchronizePost && _initialEditingContext != null)
            {
                // synchronize recent post if necessary
                using (new WaitCursor())
                    _initialEditingContext = RecentPostSynchronizer.Synchronize(Win32WindowImpl.DesktopWin32Window, _initialEditingContext);
            }

            // allow standard OnLoad processing to proceed
            base.OnLoad(e);

            // only save our size if the form successfully loaded
            _formLoaded = true;

            // for task-manager optics
            ProcessHelper.TrimWorkingSet(1000);

            // check for updates in the background so startup not slowed
            //UpdateChecker.CheckForUpdates();

            //UpdateChecker.UpdateFound += new EventHandler(_postEditorMainControl.ShowUpdatesAvailable);

            // close splash screen
            CloseSplashScreen();

            ResumeLayout();
            Show();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // Make sure the main control has been created.
            // In the case of starting Writer with a maximized window
            // the WM_ACTIVATE call comes earlier then normal,
            // and our control isn't created yet.
            if (_postEditorMainControl != null)
                _postEditorMainControl.LoadRibbonSettings();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            // Protected against the window being deactivated before
            // the main control has been created.  This will happen everytime
            // OnActivated is called to early(because it is maximized) when
            // we create the mshtml editor and set the client site, the editor
            // will become activated.
            if (_postEditorMainControl != null)
                _postEditorMainControl.SaveRibbonSettings();
            base.OnDeactivate(e);
        }

        private void CloseSplashScreen()
        {
            // hide the splash screen
            if (_splashScreen != null)
                _splashScreen.Dispose();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_postEditorMainControl.CommandManager.ProcessCmdKeyShortcut(keyData))
                return true;

            // base.ProcessCmdKey() will look for shortcuts that are present on main menu
            // items. If we are ignoring the shortcut, we should not look on the main menu
            // either.
            if (_postEditorMainControl.CommandManager.ShouldIgnore(keyData))
                return false;

            //	Check for a Command with an AcceleratorMnemonic matching the CmdKey.  This is a
            //	special shortcut that allows a Command to be registered that responds to an
            //	AcceleratorMnemonic without being part of a menu.
            if (_postEditorMainControl.CommandManager.ProcessCmdKeyAcceleratorMnemonic(keyData))
                return true;

            if (_postEditorMainControl.PreviewMode && keyData == Keys.Escape)
                _postEditorMainControl.CommandManager.Execute(CommandId.ClosePreview);

            //	Call the base class's method.
            return base.ProcessCmdKey(ref msg, keyData); ;
        }

        protected override Control CreateMainControl()
        {
            // create post editor main control as appropriate
            if (_initialEditingContext != null)
            {
                _postEditorMainControl = new PostEditorMainControl(this, _initialEditingContext);
            }
            else
            {
                throw new ArgumentException("PostEditorForm was not properly initialized with either a blog post or blog this item");
            }

            InitializeCommands();

            // connect our provider command manager to the BlogPostEditingManager
            _providerButtonManager.Initialize(this, _postEditorMainControl.BlogPostEditingManager);

            // update status bar then subscribe to weblog changed events for future updates
            //UpdateStatusBarBlogInfo();
            //_postEditorMainControl.BlogPostEditingManager.BlogChanged +=new EventHandler(BlogPostEditingManager_BlogChanged);
            //_postEditorMainControl.BlogPostEditingManager.BlogSettingsChanged +=new WeblogSettingsChangedHandler(BlogPostEditingManager_BlogSettingsChanged);

            // return the post editor main control
            return _postEditorMainControl;
        }
        private PostEditorMainControl _postEditorMainControl;

        protected override void SaveNormalWindowState()
        {
            if (_formLoaded)
            {
                PostEditorSettings.PostEditorWindowMaximized = false;
                PostEditorSettings.PostEditorWindowBounds = Bounds;
                PostEditorSettings.PostEditorWindowLocation = Location;
                PostEditorSettings.PostEditorWindowScale = scale;
            }
        }

        protected override void SaveMaximizedWindowState()
        {
            if (_formLoaded)
            {
                PostEditorSettings.PostEditorWindowMaximized = true;
            }
        }

        protected override void RestoreWindowState()
        {
            Rectangle savedBounds = PostEditorSettings.PostEditorWindowBounds;
            SizeF lastWindowScale = PostEditorSettings.PostEditorWindowScale;
            if (savedBounds != Rectangle.Empty)
            {
                if (lastWindowScale != scale)
                {
                    float widthScale = lastWindowScale.Width / scale.Width;
                    float heightScale = lastWindowScale.Height / scale.Height;
                    Size = new Size((int)(savedBounds.Width / widthScale), (int)(savedBounds.Height / heightScale));
                }
                else
                    Size = new Size(savedBounds.Width, savedBounds.Height);
            }
            else
            {
                Size = PostEditorSettings.DefaultWindowBounds.Size;
            }

            int offset = SystemInformation.CaptionHeight;
            Point formLocation = WindowCascadeHelper.GetNewPostLocation(Size, offset);
            if (formLocation != Point.Empty)
            {
                Location = formLocation;
                WindowCascadeHelper.SetNextOpenedLocation(formLocation);
            }

            // finally, maximize the form if that was the last state
            if (PostEditorSettings.PostEditorWindowMaximized)
                WindowState = FormWindowState.Maximized;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_blogClientUIContextScope != null)
                {
                    _blogClientUIContextScope.Dispose();
                    _blogClientUIContextScope = null;
                }

                if (_providerButtonManager != null)
                {
                    _providerButtonManager.Dispose();
                    _providerButtonManager = null;
                }

                // unsubscribe from weblog changed events
                if (_postEditorMainControl != null)
                {
                    //_postEditorMainControl.BlogPostEditingManager.BlogChanged -=new EventHandler(BlogPostEditingManager_BlogChanged);
                    //_postEditorMainControl.BlogPostEditingManager.BlogSettingsChanged -=new WeblogSettingsChangedHandler(BlogPostEditingManager_BlogSettingsChanged);
                    //UpdateChecker.UpdateFound -= new EventHandler(_postEditorMainControl.ShowUpdatesAvailable);
                }

                if (components != null)
                {
                    components.Dispose();
                }

            }
            base.Dispose(disposing);

            if (ApplicationDiagnostics.TestMode)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(EnsureThreadExit), Thread.CurrentThread);
            }
        }

        private void EnsureThreadExit(object state)
        {
            Thread.Sleep(10000);
            if (((Thread)state).IsAlive)
            {
                DialogResult result = MessageBox.Show(
                    @"Warning: Window thread did not exit within 10 seconds of the editor window being closed.

Abort - Terminate process
Retry - Start debugger
Ignore - Do nothing",
                    ApplicationEnvironment.ProductNameQualified,
                    MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    (BidiHelper.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : 0)
                    );

                switch (result)
                {
                    case DialogResult.Abort:
                        Process.GetCurrentProcess().Kill();
                        break;
                    case DialogResult.Retry:
                        Debugger.Break();
                        break;
                }
            }
        }

        private IContainer components = new Container();

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
            scale = new SizeF(scale.Width * dx, scale.Height * dy);
        }
        private SizeF scale = new SizeF(1f, 1f); //currently applied scale

        string IMainFrameWindow.Caption
        {
            set { Text = value; }
        }

        #region Commands

        private Command commandAbout;
        private Command commandFocusNextPane;
        private Command commandFocusPreviousPane;

        private BlogProviderButtonManager _providerButtonManager;

        private void InitializeCommands()
        {
            _postEditorMainControl.CommandManager.BeginUpdate();

            commandFocusNextPane = new Command(CommandId.FocusNextPane);
            commandFocusNextPane.Execute += new EventHandler(commandFocusNextPane_Execute);
            _postEditorMainControl.CommandManager.Add(commandFocusNextPane);

            commandFocusPreviousPane = new Command(CommandId.FocusPreviousPane);
            commandFocusPreviousPane.Execute += new EventHandler(commandFocusPreviousPane_Execute);
            _postEditorMainControl.CommandManager.Add(commandFocusPreviousPane);

            _postEditorMainControl.CommandManager.Add(CommandId.Close, commandClose_Execute);

            Command commandHelp = _postEditorMainControl.CommandManager.Add(CommandId.Help, UrlHandler.Create(GLink.Instance.Help));
            commandHelp.Enabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.Help);
            commandHelp.MenuFormatArgs = new object[] { ApplicationEnvironment.ProductName };

            commandAbout = new AboutCommand();
            commandAbout.MenuFormatArgs = new object[] { ApplicationEnvironment.ProductName };
            commandAbout.Execute += new EventHandler(commandAbout_Execute);
            _postEditorMainControl.CommandManager.Add(commandAbout);

            _providerButtonManager = new BlogProviderButtonManager(_postEditorMainControl.CommandManager);
            _postEditorMainControl.CommandManager.Add(new GroupCommand(CommandId.BlogProviderShortcutsGroup, _postEditorMainControl.CommandManager.Get(CommandId.BlogProviderButtonsGallery)));

            _postEditorMainControl.CommandManager.EndUpdate();
        }

        private void commandClose_Execute(object sender, EventArgs e)
        {
            // WinLive 203843: This function is called from inside a Ribbon execute handler, so we don't want to close
            // the current window (and thereby destroy the Ribbon) while we're still inside a call from the Ribbon.
            // Using BeginInvoke is basically just a wrapper around using User32.PostMessage, which puts the WM.CLOSE
            // message at the end of the message queue.
            this.BeginInvoke(new InvokeInUIThreadDelegate(() => this.Close()), null);
        }

        private void commandFocusNextPane_Execute(object sender, EventArgs e)
        {
            _paneFocusManager.FocusNextControl();
        }

        private void commandFocusPreviousPane_Execute(object sender, EventArgs e)
        {
            _paneFocusManager.FocusPreviousControl();
        }

        private void commandAbout_Execute(object sender, EventArgs e)
        {
            AboutForm.DisplayDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {

            try
            {
                //	Suppress all events from the ApplicationManager's CommandManager.  The .NET event
                //	model is just not up to how we use it, and we will receive events on disposed
                //	controls which .NET will use to reload them.
                if (_postEditorMainControl != null)
                    _postEditorMainControl.CommandManager.SuppressEvents = true;
            }
            catch (Exception ex)
            {
                // An uncaught exception here stops the user from ever being able to
                // close the window (other than TaskMan), so we'll Watson but continue.
                UnexpectedErrorMessage.Show(this, ex);
            }

            base.OnFormClosing(e);
        }

        #endregion

        public void OnKeyboardLanguageChanged()
        {
            _postEditorMainControl.OnKeyboardLanguageChanged();
        }

        FocusControlManager _paneFocusManager = new FocusControlManager();
        private BlogClientUIContextScope _blogClientUIContextScope;
        private IBlogPostEditingContext _initialEditingContext;
        private IDisposable _splashScreen;
        private bool _synchronizePost = false;
        private bool _formLoaded = false;
        //private Control _statusBarControl;

        private void supportCommand_Execute(object sender, EventArgs e)
        {

        }

        private class UrlHandler
        {
            public static EventHandler Create(string url)
            {
                return new EventHandler(new UrlHandler(url).Execute);
            }

            private UrlHandler(string url)
            {
                _url = url;
            }
            private string _url;

            private void Execute(object sender, EventArgs e)
            {
                ShellHelper.LaunchUrl(_url);
            }
        }

        #region ICommandManager Members

        public CommandManager CommandManager
        {
            get { return _postEditorMainControl.CommandManager; }
        }

        #endregion
    }

}
