// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Controls;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.HtmlParser.Parser.FormAgent;
using OpenLiveWriter.CoreServices;
using System.Runtime.InteropServices;

using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Interop.Com.StructuredStorage;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.PostEditor.Configuration.Wizard;
using OpenLiveWriter.PostEditor.Configuration.Settings;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.JumpList;
using OpenLiveWriter.PostEditor.OpenPost;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.PostEditor.SupportingFiles;
using OpenLiveWriter.PostEditor.Updates;
using OpenLiveWriter.SpellChecker;
using OpenLiveWriter.Ribbon.Managed;
using OpenLiveWriter.Ribbon.Managed.Controls;
using OpenLiveWriter.Ribbon.Managed.Commands;
using OpenLiveWriter.Ribbon.Managed.Configuration;
using Timer = System.Windows.Forms.Timer;

// @RIBBON TODO: Cleanly remove obsolete code

namespace OpenLiveWriter.PostEditor
{
    internal enum ApplicationMode
    {
        Normal = 0,
        Preview = 1,
        LTR = 2,
        RTL = 3,
        NoPlugins = 4,
        HasPlugins = 5,
        Test = 31
    }

    internal class PostEditorMainControl : UserControl, IFormClosingHandler, IBlogPostEditor, IBlogPostEditingSite, IUIApplication, ISessionHandler
    {
        #region Private Data Declarations

        private IMainFrameWindow _mainFrameWindow;
        private BlogPostEditingManager _editingManager;

        private Command commandNewPost;
        private Command commandNewPage;
        private Command commandSavePost;
        private Command commandDeleteDraft;
        private Command commandPostAsDraft;
        private Command commandPostAsDraftAndEditOnline;

        private Command commandColorize;
        private CommandContextMenuDefinition _newPostContextMenuDefinition;
        private CommandContextMenuDefinition _savePostContextMenuDefinition;

        private Panel _mainEditorPanel;
        private HtmlStylePicker _styleComboControl;
        private BlogPostHtmlEditor _htmlEditor;

        private PostEditorPreferencesEditor _optionsEditor;
        private WeblogCommandManager _weblogCommandManager;

        //        private Bitmap statusNewPostBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.StatusComposingPost.png");
        //        private Bitmap statusDraftPostBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.StatusPosted.png");
        //        private Bitmap statusPublishedPostBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.StatusPublished.png");

        private System.Windows.Forms.Timer _autoSaveTimer;
        private System.Windows.Forms.Timer _autoSaveMessageDismissTimer;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = new Container();
        #endregion

        #region Initialization/Disposal

        public PostEditorMainControl(IMainFrameWindow mainFrameWindow, IBlogPostEditingContext editingContext)
        {
            Init(mainFrameWindow, editingContext);
        }

        private void Init(IMainFrameWindow mainFrameWindow, IBlogPostEditingContext editingContext)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} PostEditorMainControl.Init starting");
            // save reference to the frame window and workspace border manager
            _mainFrameWindow = mainFrameWindow;

            // This call is required by the Windows.Forms Form Designer.
            Font = Res.DefaultFont;
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeComponent");
            InitializeComponent();

            // initialize UI
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeUI");
            InitializeUI();

            // Initialize the editing manager
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeEditingManager");
            InitializeEditingManager();

            // initialize our commands
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeCommands");
            InitializeCommands();

            //subscribe to global events
            BlogSettings.BlogSettingsDeleted += new BlogSettings.BlogSettingsListener(HandleBlogDeleted);

            // edit the post
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} EditPost");
            _editingManager.EditPost(editingContext, false);

            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeRibbon");
            InitializeRibbon();
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} PostEditorMainControl.Init done");
        }

        private void InitializeUI()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeUI starting");
            ColorizedResources.Instance.RegisterControlForBackColorUpdates(this);

            // initialize workspace
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeWorkspace");
            InitializeWorkspace();

            // initialize the post property editors
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializePostPropertyEditors");
            InitializePostPropertyEditors();

            // initialize the core editor
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeHtmlEditor");
            InitializeHtmlEditor();

            // initialize options editor
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeOptionsEditor");
            InitializeOptionsEditor();
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeUI done");
        }

        private GalleryCommand<string> commandPluginsGallery = null;
        private void InitializeEditingManager()
        {
            _editingManager = new BlogPostEditingManager(
                this,
                new IBlogPostEditor[] { _htmlEditor, this },
                _htmlEditor
                );

            commandPluginsGallery = (GalleryCommand<string>)CommandManager.Get(CommandId.PluginsGallery);
            commandPluginsGallery.StateChanged += new EventHandler(commandPluginsGallery_StateChanged);
            _editingManager.BlogChanged += new EventHandler(_editingManager_BlogChanged);
            _editingManager.BlogSettingsChanged += new WeblogSettingsChangedHandler(_editingManager_BlogSettingsChanged);
            _editingManager.EditingStatusChanged += new EventHandler(_editingManager_EditingStatusChanged);
            _editingManager.UserSavedPost += new EventHandler(_editingManager_UserSavedPost);
            _editingManager.UserPublishedPost += new EventHandler(_editingManager_UserPublishedPost);
            _editingManager.UserDeletedPost += new EventHandler(_editingManager_UserDeletedPost);

            // initialize auto-save timer
            _autoSaveTimer = new System.Windows.Forms.Timer(this.components);
            _autoSaveTimer.Interval = 5000;
            _autoSaveTimer.Tick += new EventHandler(_autoSaveTimer_Tick);

            _autoSaveMessageDismissTimer = new Timer(components);
            _autoSaveMessageDismissTimer.Interval = 450;
            _autoSaveMessageDismissTimer.Tick += _autoSaveMessageDismissTimer_Tick;
        }

        void commandPluginsGallery_StateChanged(object sender, EventArgs e)
        {
            UpdateRibbonMode();
        }

        private void InitializeCommands()
        {
            _htmlEditor.CommandManager.BeginUpdate();

            commandNewPost = _htmlEditor.CommandManager.Add(CommandId.NewPost, commandNewPost_Execute);
            commandNewPage = _htmlEditor.CommandManager.Add(CommandId.NewPage, commandNewPage_Execute);

            // new context menu definition
            _newPostContextMenuDefinition = new CommandContextMenuDefinition(this.components);
            _newPostContextMenuDefinition.Entries.Add(CommandId.NewPost, false, false);
            _newPostContextMenuDefinition.Entries.Add(CommandId.NewPage, false, false);
            commandNewPost.CommandBarButtonContextMenuDropDown = true;

            _htmlEditor.CommandManager.Add(CommandId.OpenDrafts, commandOpenDrafts_Execute);
            _htmlEditor.CommandManager.Add(CommandId.OpenRecentPosts, commandOpenRecentPosts_Execute);
            _htmlEditor.CommandManager.Add(CommandId.OpenPost, commandOpenPost_Execute);
            commandSavePost = _htmlEditor.CommandManager.Add(CommandId.SavePost, commandSavePost_Execute);
            commandDeleteDraft = _htmlEditor.CommandManager.Add(CommandId.DeleteDraft, commandDeleteDraft_Execute);
            _htmlEditor.CommandManager.Add(CommandId.PostAndPublish, commandPostAndPublish_Execute);
            commandPostAsDraft = _htmlEditor.CommandManager.Add(CommandId.PostAsDraft, commandPostAsDraft_Execute);
            commandPostAsDraftAndEditOnline = _htmlEditor.CommandManager.Add(CommandId.PostAsDraftAndEditOnline, commandPostAsDraftAndEditOnline_Execute);

            DraftPostItemsGalleryCommand draftGallery = new DraftPostItemsGalleryCommand(this as IBlogPostEditingSite,
                                                                                         CommandManager, false);
            draftGallery.Execute += commandOpenDrafts_Execute;
            DraftPostItemsGalleryCommand postGallery = new DraftPostItemsGalleryCommand(this as IBlogPostEditingSite,
                                                                                         CommandManager, true);
            postGallery.Execute += commandOpenRecentPosts_Execute;

            // publish command bar context menu
            _savePostContextMenuDefinition = new CommandContextMenuDefinition(this.components);
            _savePostContextMenuDefinition.CommandBar = true;
            _savePostContextMenuDefinition.Entries.Add(CommandId.SavePost, false, true);
            _savePostContextMenuDefinition.Entries.Add(CommandId.PostAsDraft, false, false);
            _savePostContextMenuDefinition.Entries.Add(CommandId.PostAsDraftAndEditOnline, false, false);
            commandSavePost.CommandBarButtonContextMenuDropDown = true;

            if (ApplicationDiagnostics.TestMode)
            {
                _htmlEditor.CommandManager.Add(CommandId.DiagnosticsConsole, new EventHandler(commandDiagnosticsConsole_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ShowBetaExpiredDialogs, new EventHandler(commandShowBetaExpiredDialogs_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ShowWebLayoutWarning, delegate { new WebLayoutViewWarningForm().ShowDialog(this); });
                _htmlEditor.CommandManager.Add(CommandId.ShowErrorDialog, new EventHandler(commandErrorDialog_Execute));
                _htmlEditor.CommandManager.Add(CommandId.BlogClientOptions, new EventHandler(commandBlogClientOptions_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ShowDisplayMessageTestForm, new EventHandler(commandShowDisplayMessageTestForm_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ShowUpdateMessage, new EventHandler(commandShowUpdateMessage_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ShowSupportingFilesForm, new EventHandler(commandShowSupportingFilesForm_Execute));
                _htmlEditor.CommandManager.Add(CommandId.InsertLoremIpsum, new EventHandler(commandInsertLoremIpsum_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ValidateHtml, new EventHandler(commandValidateHtml_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ValidateXhtml, new EventHandler(commandValidateXhtml_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ValidateLocalizedResources, new EventHandler(commandValidateLocalizedResources_Execute));
                _htmlEditor.CommandManager.Add(CommandId.ShowAtomImageEndpointSelector, new EventHandler(commandShowAtomImageEndpointSelector_Execute));
                _htmlEditor.CommandManager.Add(CommandId.RaiseAssertion, delegate { Trace.Fail("You asked for it"); });
                _htmlEditor.CommandManager.Add(CommandId.ShowGoogleCaptcha, delegate { new GDataCaptchaForm().ShowDialog(this); });
                _htmlEditor.CommandManager.Add(CommandId.TerminateProcess, delegate { Process.GetCurrentProcess().Kill(); });
            }

            commandColorize = new Command(CommandId.Colorize);
            commandColorize.CommandBarButtonContextMenuHandler = new CommandBarButtonContextMenuHandler(new ColorizationContextHelper().Handler);
            _htmlEditor.CommandManager.Add(commandColorize);

            _htmlEditor.CommandManager.EndUpdate();

            // initialize the weblog menu commands
            _weblogCommandManager = new WeblogCommandManager(_editingManager, this);
            _weblogCommandManager.WeblogSelected += new WeblogHandler(_weblogMenuManager_WeblogSelected);
        }

        private void commandShowAtomImageEndpointSelector_Execute(object sender, EventArgs e)
        {
            OpenLiveWriter.Controls.Wizard.WizardController controller = new OpenLiveWriter.Controls.Wizard.WizardController();
            WeblogConfigurationWizardPanelSelectBlog selectBlogControl = new WeblogConfigurationWizardPanelSelectBlog();
            selectBlogControl.HeaderText = Res.Get(StringId.ConfigWizardSelectImageEndpoint);
            selectBlogControl.LabelText = Res.Get(StringId.CWSelectImageEndpointText);
            selectBlogControl.PrepareForAdd();
            controller.addWizardStep(new OpenLiveWriter.Controls.Wizard.WizardStep(selectBlogControl, StringId.ConfigWizardSelectImageEndpoint, null, null, null, null, null));

            using (OpenLiveWriter.Controls.Wizard.WizardForm form = new OpenLiveWriter.Controls.Wizard.WizardForm(controller))
            {
                form.Size = new Size((int)Math.Ceiling(DisplayHelper.ScaleX(460)), (int)Math.Ceiling(DisplayHelper.ScaleY(400)));
                form.Text = Res.Get(StringId.CWTitle);
                form.ShowDialog(this);
            }
        }

        private void MenuContextHandler(Control parent, Point menuLocation, int alternativeLocation, IDisposable disposeWhenDone)
        {
            Command cmd;
            using (disposeWhenDone)
            {
                ArrayList mainMenuItems = new ArrayList(_htmlEditor.CommandManager.BuildMenu(MenuType.Main));
                mainMenuItems.Add(new OwnerDrawMenuItem(MenuType.Context, "-"));
                Command commandShowMenu = _htmlEditor.CommandManager.Get(CommandId.ShowMenu);
                mainMenuItems.Add(new CommandOwnerDrawMenuItem(
                    MenuType.Context,
                    commandShowMenu,
                    commandShowMenu.Text));

                cmd = CommandContextMenu.ShowModal(this, menuLocation, alternativeLocation, (MenuItem[])mainMenuItems.ToArray(typeof(MenuItem)));
            }
            if (cmd != null)
                cmd.PerformExecute();
        }

        private class ColorizationContextHelper
        {
            private IDisposable _disposeWhenDone;

            public void Handler(Control parent, Point menuLocation, int alternativeLocation, IDisposable disposeWhenDone)
            {
                try
                {
                    ColorPickerForm form = new ColorPickerForm();
                    form.Color = ColorizedResources.AppColor;
                    form.ColorSelected += new ColorSelectedEventHandler(form_ColorSelected);
                    form.Closed += new EventHandler(form_Closed);

                    form.StartPosition = FormStartPosition.Manual;
                    Point startLocation = CommandBarButtonLightweightControl.PositionMenu(menuLocation, alternativeLocation, form.Size);
                    form.Location = startLocation;
                    _disposeWhenDone = disposeWhenDone;
                    IMiniFormOwner miniFormOwner = parent.FindForm() as IMiniFormOwner;
                    if (miniFormOwner != null)
                        form.FloatAboveOwner(miniFormOwner);
                    form.Show();
                }
                catch
                {
                    disposeWhenDone.Dispose();
                    throw;
                }
            }

            private void form_Closed(object sender, EventArgs e)
            {
                _disposeWhenDone.Dispose();
            }

            private void form_ColorSelected(object sender, ColorSelectedEventArgs args)
            {
                ColorizedResources.AppColor = args.SelectedColor;
            }
        }

        private void InitializeWorkspace()
        {
            if (!BidiHelper.IsRightToLeft)
                DockPadding.Left = 0;
            else
                DockPadding.Right = 0;

            _mainEditorPanel = new Panel();
            _mainEditorPanel.Dock = DockStyle.Fill;

            //Controls.Add(_publishBar);
            Controls.Add(_mainEditorPanel);
        }

        private static int ToAppMode(ApplicationMode m)
        {
            return Convert.ToInt32(1 << Convert.ToInt32(m, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        private ApplicationMode TextDirection
        {
            get
            {
                return _htmlEditor.IsRTLTemplate || BidiHelper.IsRightToLeft ? ApplicationMode.RTL : ApplicationMode.LTR;
            }
        }

        private int mode = ToAppMode(ApplicationMode.Normal);
        public bool TestMode
        {
            get
            {
                return Convert.ToBoolean(mode & ToAppMode(ApplicationMode.Test));
            }

            set
            {
                if (TestMode != value)
                {
                    mode ^= ToAppMode(ApplicationMode.Test);
                    UpdateRibbonMode();
                }
            }
        }

        public bool PreviewMode
        {
            get
            {
                return Convert.ToBoolean(mode & ToAppMode(ApplicationMode.Preview));
            }

            set
            {
                if (PreviewMode != value)
                {
                    mode ^= ToAppMode(ApplicationMode.Preview);
                    mode ^= ToAppMode(ApplicationMode.Normal);
                    Debug.Assert(!(PreviewMode && Convert.ToBoolean(mode & ToAppMode(ApplicationMode.Normal))));
                    UpdateRibbonMode();
                }
            }
        }

        private void UpdateRibbonMode()
        {
            if (TextDirection == ApplicationMode.RTL)
            {
                mode |= ToAppMode(ApplicationMode.RTL);
                mode &= ~ToAppMode(ApplicationMode.LTR);
            }
            else
            {
                mode |= ToAppMode(ApplicationMode.LTR);
                mode &= ~ToAppMode(ApplicationMode.RTL);
            }

            if (commandPluginsGallery.Items.Count > 0)
            {
                mode |= ToAppMode(ApplicationMode.HasPlugins);
                mode &= ~ToAppMode(ApplicationMode.NoPlugins);
            }
            else
            {
                mode |= ToAppMode(ApplicationMode.NoPlugins);
                mode &= ~ToAppMode(ApplicationMode.HasPlugins);
            }

            // Update managed ribbon mode
            UpdateManagedRibbonMode();

            // Legacy framework support
            if (_framework != null)
            {
                _framework.SetModes(mode);
            }
        }

        private void InvalidateCommand(CommandId commandId)
        {
            Command command = CommandManager.Get(commandId);
            if (command != null)
            {
                command.Invalidate();
            }
        }

        public void OnTestModeChanged(object sender, EventArgs e)
        {
            TestMode = ApplicationDiagnostics.TestMode;
        }

        [ComImport]
        [Guid("926749fa-2615-4987-8845-c33e65f2b957")]
        public class Framework
        {
        }

        public IUIFramework RibbonFramework
        {
            get
            {
                return _framework;
            }
        }

        private RibbonControl ribbonControl;
        private RibbonPanel _managedRibbon;
        private CommandManagerBridge _commandManagerBridge;

        // Legacy IUIFramework support - kept for backwards compatibility but no longer used
#pragma warning disable CS0649 // Field is never assigned to
        private IUIFramework _framework;
        private IUIRibbon ribbon;
#pragma warning restore CS0649

        private void InitializeRibbon()
        {
            try
            {
                // Initialize the RibbonControl for command management (both ribbons need it)
                ribbonControl = new RibbonControl(_htmlEditor.IHtmlEditorComponentContext, _htmlEditor);
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeRibbon - RibbonControl created");

                // Labs setting: use the classic native Windows ribbon when requested.
                // Falls back to the managed ribbon if the native one cannot initialize.
                if (PostEditorSettings.UseNativeRibbon && TryInitializeNativeRibbon())
                {
                    System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeRibbon - Using native ribbon");
                }
                else
                {
                    InitializeManagedRibbon();
                }

                CommandManager.Invalidate(CommandId.MRUList);
                CommandManager.Invalidate(CommandId.OpenDraftSplit);
                CommandManager.Invalidate(CommandId.OpenPostSplit);

                ApplicationDiagnostics.TestModeChanged += OnTestModeChanged;
                TestMode = ApplicationDiagnostics.TestMode;
                System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeRibbon - Complete");
            }
            catch (COMException ex)
            {
                Trace.Fail("Ribbon initialization failed with COMException: " + ex.Message);
                ribbonControl = null;
                _managedRibbon = null;
            }
            catch (Exception ex)
            {
                Trace.Fail("Ribbon initialization failed: " + ex.Message);
                ribbonControl = null;
                _managedRibbon = null;
            }
        }

        /// <summary>
        /// Initializes the classic Windows Ribbon Framework ribbon (as in 0.6.2).
        /// Returns false (so the caller falls back to the managed ribbon) when the
        /// framework or the compiled markup DLL is unavailable.
        /// </summary>
        private bool TryInitializeNativeRibbon()
        {
            try
            {
                IUIFramework framework = (IUIFramework)Activator.CreateInstance<Framework>();
                if (framework == null)
                    return false;

                int initializeResult = framework.Initialize(_mainFrameWindow.Handle, this);
                if (initializeResult != HRESULT.S_OK)
                {
                    Trace.TraceWarning("Native ribbon framework failed to initialize: " + initializeResult);
                    return false;
                }

                string nativeResourceDLL = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\OpenLiveWriter.Ribbon.dll";
                IntPtr hMod = Kernel32.LoadLibrary(nativeResourceDLL);
                if (hMod == IntPtr.Zero)
                {
                    Trace.TraceWarning("OpenLiveWriter.Ribbon.dll not found; falling back to the managed ribbon.");
                    return false;
                }

                int loadResult = framework.LoadUI(hMod, "RIBBON_RIBBON");
                if (loadResult != HRESULT.S_OK)
                {
                    Trace.TraceWarning("Native ribbon failed to load: " + loadResult + "; falling back to the managed ribbon.");
                    return false;
                }

                _framework = framework;
                _framework.SetModes(mode);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Native ribbon initialization failed, falling back to the managed ribbon: " + ex.Message);
                return false;
            }
        }

        private void InitializeManagedRibbon()
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeManagedRibbon - Starting");

            // Create the managed ribbon panel
            _managedRibbon = new RibbonPanel();

            // Create the command manager bridge to connect with existing CommandManager
            _commandManagerBridge = new CommandManagerBridge(_htmlEditor.CommandManager);
            _managedRibbon.CommandManager = _commandManagerBridge.RibbonCommandManager;

            // Build the ribbon from the default configuration
            var config = DefaultRibbonConfiguration.Create();
            _managedRibbon.BuildFromConfiguration(config);

            // Add the ribbon to the form
            Controls.Add(_managedRibbon);
            _managedRibbon.BringToFront();

            // Force an immediate synchronous paint: the init sequence blocks the
            // message loop, so without this the just-created ribbon sits
            // unpainted and transparent until the loop unblocks.
            _managedRibbon.Update();

            // Update editor panel padding to account for ribbon height
            if (_mainEditorPanel != null)
            {
                _mainEditorPanel.DockPadding.Top = _managedRibbon.RibbonHeight;
            }

            // Set initial mode
            UpdateManagedRibbonMode();
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} InitializeManagedRibbon - Complete");
        }

        private void UpdateManagedRibbonMode()
        {
            if (_managedRibbon == null) return;

            var ribbonMode = RibbonApplicationMode.Normal;

            // Map existing mode flags to RibbonApplicationMode
            if ((mode & ToAppMode(ApplicationMode.Preview)) != 0)
                ribbonMode |= RibbonApplicationMode.Preview;
            if ((mode & ToAppMode(ApplicationMode.LTR)) != 0)
                ribbonMode |= RibbonApplicationMode.LTR;
            if ((mode & ToAppMode(ApplicationMode.RTL)) != 0)
                ribbonMode |= RibbonApplicationMode.RTL;
            if ((mode & ToAppMode(ApplicationMode.HasPlugins)) != 0)
                ribbonMode |= RibbonApplicationMode.WithPlugins;
            if ((mode & ToAppMode(ApplicationMode.NoPlugins)) != 0)
                ribbonMode |= RibbonApplicationMode.WithoutPlugins;
            // Map Test mode to Debug mode - enables the Debug tab
            if ((mode & ToAppMode(ApplicationMode.Test)) != 0)
                ribbonMode |= RibbonApplicationMode.Debug;

            _managedRibbon.CurrentMode = ribbonMode;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (_mainEditorPanel != null)
            {
                if (_managedRibbon != null)
                {
                    _mainEditorPanel.DockPadding.Top = _managedRibbon.RibbonHeight;
                }
                else if (ribbon != null && ComHelper.SUCCEEDED(ribbon.GetHeight(out uint ribbonHeight)))
                {
                    _mainEditorPanel.DockPadding.Top = (int)ribbonHeight;
                }
            }

            Invalidate(false);
            Update();
        }

        private void InitializeHtmlEditor()
        {
            // create the editor
            _htmlEditor = BlogPostHtmlEditor.Create(_mainFrameWindow, _mainEditorPanel, this as IBlogPostEditingSite);
            _htmlEditor.TitleFocusChanged += new EventHandler(htmlEditor_TitleFocusChanged);
            _htmlEditor.Dirty += new EventHandler(htmlEditor_Dirty);
            _htmlEditor.EditorLoaded += new EventHandler(_htmlEditor_EditingModeChanged);
            _htmlEditor.DocumentComplete += new EventHandler(_htmlEditor_DocumentComplete);
        }

        void _htmlEditor_DocumentComplete(object sender, EventArgs e)
        {
            _htmlEditor.FocusBody();
        }

        public void OnKeyboardLanguageChanged()
        {
            //// Sync dictionary language with keyboard language (if enabled)
            //ushort currentLangId = (ushort) (User32.GetKeyboardLayout(0) & 0xFFFF);
            //SpellingLanguageEntry[] langs = SpellingSettings.GetInstalledLanguages();
            //foreach (var v in langs)
            //{
            //    if (v.LCID == currentLangId)
            //    {
            //        SpellingSettings.Language = v.Language;
            //        SpellingSettings.FireChangedEvent();
            //        break;
            //    }
            //}
        }

        void _htmlEditor_EditingModeChanged(object sender, EventArgs e)
        {
            PreviewMode = _htmlEditor.CurrentEditingMode == EditingMode.Preview;
        }

        private void htmlEditor_Dirty(object sender, EventArgs e)
        {
            _autoSaveTimer.Stop();
            if (PostEditorSettings.AutoSaveDrafts)
                _autoSaveTimer.Start();
        }

        private void InitializeOptionsEditor()
        {

            _optionsEditor = new PostEditorPreferencesEditor(_mainFrameWindow, this);
        }

        public CommandManager CommandManager
        {
            get
            {
                return this._htmlEditor.CommandManager;
            }
        }

        private class PostContentEditor : IBlogPostContentEditor
        {
            public PostContentEditor(PostEditorMainControl parent) { _parent = parent; }
            public bool FullyEditableRegionActive { get { return _parent._htmlEditor.FullyEditableRegionActive; } }
            public string SelectedText { get { return _parent._htmlEditor.SelectedText; } }
            public string SelectedHtml { get { return _parent._htmlEditor.SelectedHtml; } }
            public void InsertHtml(string content, bool moveSelectionRight) { _parent._htmlEditor.InsertHtml(content, moveSelectionRight); }
            public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow) { _parent._htmlEditor.InsertLink(url, linkText, linkTitle, rel, newWindow); }
            private PostEditorMainControl _parent;
        }

        private void InitializePostPropertyEditors()
        {
            _styleComboControl = new HtmlStylePicker(this._htmlEditor);
            _styleComboControl.Enabled = false;
        }

        internal BlogPostEditingManager BlogPostEditingManager
        {
            get { return _editingManager; }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_htmlEditor != null)
                    _htmlEditor.Dispose();

                if (_editingManager != null)
                    _editingManager.Dispose();

                if (_weblogCommandManager != null)
                    _weblogCommandManager.Dispose();

                if (_optionsEditor != null)
                    _optionsEditor.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }

                // Dispose managed ribbon
                if (_managedRibbon != null)
                {
                    _managedRibbon.Dispose();
                    _managedRibbon = null;
                }

                // Legacy framework disposal - only if framework was created
                _framework?.Destroy();

                BlogSettings.BlogSettingsDeleted -= new BlogSettings.BlogSettingsListener(HandleBlogDeleted);
                commandPluginsGallery.StateChanged -= new EventHandler(commandPluginsGallery_StateChanged);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Command Handlers

        private void commandNewPost_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            _editingManager.NewPost();
        }

        private void commandNewPage_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            _editingManager.NewPage();
        }

        private void commandOpenDrafts_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            _editingManager.OpenPost(OpenPostForm.OpenMode.Drafts);
        }

        private void commandOpenRecentPosts_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            _editingManager.OpenPost(OpenPostForm.OpenMode.RecentPosts);
        }

        private void commandOpenPost_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            _editingManager.OpenPost(OpenPostForm.OpenMode.Auto);
        }

        private void commandSavePost_Execute(object sender, EventArgs e)
        {
            // save the draft
            using (new PaddedWaitCursor(250))
            {
                _editingManager.SaveDraft();
            }
        }

        private void commandDeleteDraft_Execute(object sender, EventArgs e)
        {
            _editingManager.DeleteCurrentDraft();
        }

        private void commandPostAsDraft_Execute(object sender, EventArgs e)
        {
            if (_editingManager.PublishAsDraft())
            {
                // respect close settings
                if (CloseWindowOnPublish)
                    CloseMainFrameWindow();
            }
        }

        private void commandPostAsDraftAndEditOnline_Execute(object sender, EventArgs e)
        {
            if (_editingManager.PublishAsDraft())
            {
                // edit post online
                _editingManager.EditPostOnline(true);

                // respect close settings
                if (CloseWindowOnPublish)
                    CloseMainFrameWindow();
            }
        }

        private void commandPostAndPublish_Execute(object sender, EventArgs e)
        {
            if (_editingManager.Publish())
            {
                // respect post after publish
                if (PostEditorSettings.ViewPostAfterPublish)
                    _editingManager.ViewPost();

                // respect close settings
                if (CloseWindowOnPublish)
                    CloseMainFrameWindow();
            }
        }

        private void CloseMainFrameWindow()
        {
            // WinLive 164570: This function is called from inside a Ribbon execute handler, so we don't want to close
            // the current window (and thereby destroy the Ribbon) while we're still inside a call from the Ribbon.
            // Using BeginInvoke is basically just a wrapper around using User32.PostMessage, which puts the WM.CLOSE
            // message at the end of the message queue.
            this.BeginInvoke(new InvokeInUIThreadDelegate(() => this._mainFrameWindow.Close()), null);
        }

        private void commandDiagnosticsConsole_Execute(object sender, EventArgs e)
        {
            ApplicationEnvironment.ApplicationDiagnostics.ShowDiagnosticsConsole("test");
        }

        private void commandShowBetaExpiredDialogs_Execute(object sender, EventArgs e)
        {
            ExpirationForm.ShowExpiredDialog(20);
            ExpirationForm.ShowExpiredDialog(1);
            ExpirationForm.ShowExpiredDialog(-1);
        }

        private void commandErrorDialog_Execute(object sender, EventArgs e)
        {
            UnexpectedErrorMessage.Show(new ApplicationException("Force Error Dialog"));
        }

        private void commandBlogClientOptions_Execute(object sender, EventArgs e)
        {
            _editingManager.DisplayBlogClientOptions();
        }

        private void commandShowDisplayMessageTestForm_Execute(object sender, EventArgs e)
        {
            using (DisplayMessageTestForm form = new DisplayMessageTestForm())
            {
                form.ShowDialog(FindForm());
            }
        }

        private void commandShowUpdateMessage_Execute(object sender, EventArgs e)
        {
            // Debug aid: kick off an update check and point at the diagnostics
            // log for the outcome (the check itself is silent by design).
            Updates.UpdateManager.CheckforUpdates(forceCheck: true);
            MessageBox.Show(this, "Update check started. See the diagnostics log for the result.",
                "Update Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void commandShowSupportingFilesForm_Execute(object sender, EventArgs e)
        {
            SupportingFilesForm.ShowForm(_mainFrameWindow as Form, BlogPostEditingManager);
        }

        private void commandValidateLocalizedResources_Execute(object sender, EventArgs e)
        {
            string[] errors = Res.Validate();
            if (errors.Length > 0)
            {
                Trace.WriteLine("Localized resource validation errors:\r\n" + StringHelper.Join(errors, "\r\n"));
                MessageBox.Show(StringHelper.Join(errors, "\r\n\r\n"), ApplicationEnvironment.ProductNameQualified, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, (BidiHelper.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : 0));
            }
            else
            {
                MessageBox.Show("No problems detected.", ApplicationEnvironment.ProductNameQualified, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, (BidiHelper.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : 0));
            }
        }

        private void commandInsertLoremIpsum_Execute(object sender, EventArgs e)
        {
            string loremIpsum = "<p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</p>";
            _htmlEditor.InsertHtml(loremIpsum, HtmlInsertionOptions.SuppressSpellCheck | HtmlInsertionOptions.MoveCursorAfter);
        }

        private void commandValidateXhtml_Execute(object sender, EventArgs e)
        {
            ValidateHtml(true);
        }
        private void commandValidateHtml_Execute(object sender, EventArgs e)
        {
            ValidateHtml(false);
        }

        private void ValidateHtml(bool xhtml)
        {
            const string HTML_TEMPLATE = "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>Untitled</title></head><body>{0}</body></html>";
            const string XHTML_TEMPLATE = "<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head><title>Untitled</title></head><body>{0}</body></html>";

            // The old flow GET'd validator.w3.org and scraped the third form on
            // the page for its textarea. That page is now a 301 to https and no
            // longer has that layout, so the scrape threw before validating
            // anything. The Nu checker takes the document as the request body,
            // which needs no scraping and no DTD-era doctypes.
            const string VALIDATOR_URL = "https://validator.w3.org/nu/?out=html";
            const string VALIDATOR_BASE = "https://validator.w3.org/nu/";

            string html = string.Format(CultureInfo.InvariantCulture, xhtml ? XHTML_TEMPLATE : HTML_TEMPLATE, _htmlEditor.Body);
            string contentType = xhtml ? "application/xhtml+xml; charset=utf-8" : "text/html; charset=utf-8";

            using (Stream resultsStream = HttpRequestHelper.PostFormStream(
                VALIDATOR_URL, Encoding.UTF8.GetBytes(html), out _, contentType))
            {
                string resultsHtml = StreamHelper.AsString(resultsStream, Encoding.UTF8);
                // The results page references its stylesheet and icon relatively.
                resultsHtml = resultsHtml.Replace("<head>",
                                                  string.Format(CultureInfo.InvariantCulture, "<head><base href=\"{0}\"/>",
                                                                HtmlUtils.EscapeEntities(VALIDATOR_BASE)));

                string tempFile = TempFileManager.Instance.CreateTempFile("results.htm");
                using (Stream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(resultsHtml);
                    fileStream.Write(bytes, 0, bytes.Length);
                }
                ShellHelper.LaunchUrl(tempFile);
            }
        }

        private bool CloseWindowOnPublish
        {
            get
            {
                return PostEditorSettings.CloseWindowOnPublish;
            }
        }

        private bool ValidateTitleSpecified()
        {

            if (_htmlEditor.Title == String.Empty)
            {
                if (_editingManager.BlogRequiresTitles)
                {
                    // show error
                    DisplayMessage.Show(MessageId.NoTitleSpecified, FindForm());

                    // focus the title and return false
                    _htmlEditor.FocusTitle();
                    return false;
                }
                else if (PostEditorSettings.TitleReminder)
                {
                    using (TitleReminderForm titleReminderForm = new TitleReminderForm(_editingManager.EditingPage))
                    {
                        if (titleReminderForm.ShowDialog(FindForm()) != DialogResult.Yes)
                        {
                            _htmlEditor.FocusTitle();
                            return false;
                        }
                    }
                }
            }

            // got this far so the title must be valid
            return true;
        }

        private bool CheckSpelling()
        {
            // do auto spell check
            if (SpellingSettings.CheckSpellingBeforePublish && _htmlEditor.CanSpellCheck)
            {
                if (!_htmlEditor.CheckSpelling())
                {
                    return (DialogResult.Yes == DisplayMessage.Show(MessageId.SpellCheckCancelledStillPost, _mainFrameWindow));
                }
            }
            return true;
        }

        #endregion

        #region Event handlers for UI state management

        /// <summary>
        /// Calling with null sender will cause a forced auto save if the post is dirty.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _autoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (!_htmlEditor.SuspendAutoSave)
            {
                _autoSaveTimer.Stop();
                bool forceSave = sender == null && _editingManager.PostIsDirty;
                if (forceSave || _editingManager.ShouldAutoSave)
                {
                    _htmlEditor.StatusBar.PushStatusMessage(Res.Get(StringId.StatusAutoSaving));
                    try
                    {
                        _editingManager.AutoSaveIfRequired(forceSave);
                    }
                    catch
                    {
                        _htmlEditor.StatusBar.PopStatusMessage();
                        throw;
                    }
                    _autoSaveMessageDismissTimer.Start();
                }
            }
        }

        private void _autoSaveMessageDismissTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _autoSaveMessageDismissTimer.Stop();
                _htmlEditor.StatusBar.PopStatusMessage();
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.ToString());
            }
        }

        private void _weblogMenuManager_WeblogSelected(string blogId)
        {
            string currentBlogId = CurrentBlogId;

            _editingManager.SwitchBlog(blogId);

            // Only set the editor dirty if we actually switched blogs.
            if (blogId != currentBlogId)
                _htmlEditor.SetCurrentEditorDirty();
        }

        private void _editingManager_EditingStatusChanged(object sender, EventArgs e)
        {
            ManageCommands();
            UpdateFrameUI();
        }

        public void NotifyWeblogStylePreviewChanged()
        {
            _htmlEditor.CommandManager.Invalidate(CommandId.SemanticHtmlGallery);
        }

        private void _editingManager_BlogChanged(object sender, EventArgs e)
        {
            if (WeblogChanged != null)
                WeblogChanged(_editingManager.BlogId);

            UpdateRibbonMode();

            UpdateFrameUI();
        }

        private void _editingManager_BlogSettingsChanged(string blogId, bool templateChanged)
        {
            if (WeblogSettingsChanged != null)
                WeblogSettingsChanged(blogId, templateChanged);

            UpdateRibbonMode();

            UpdateFrameUI();
        }

        private void htmlEditor_TitleFocusChanged(object sender, EventArgs e)
        {
            UpdateFrameUI();
            ribbonControl.ManageCommands();
        }

        void IFormClosingHandler.OnClosing(CancelEventArgs e)
        {
            _editingManager.Closing(e);

            // if the control IsDirty then see if the user wants to publish their edits
            if (!e.Cancel && _editingManager.PostIsDirty)
            {
                _mainFrameWindow.Activate();
                DialogResult result = DisplayMessage.Show(MessageId.QueryForUnsavedChanges, _mainFrameWindow);
                if (result == DialogResult.Yes)
                {
                    using (new WaitCursor())
                        _editingManager.SaveDraft();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        void IFormClosingHandler.OnClosed()
        {
            _editingManager.OnClosed();
        }

        // implement IBlogPostEditor so we can participating in showing/hiding
        // the property editor control
        void IBlogPostEditor.Initialize(IBlogPostEditingContext editingContext, IBlogClientOptions clientOptions) { }
        void IBlogPostEditor.SaveChanges(BlogPost post, BlogPostSaveOptions options) { }
        bool IBlogPostEditor.ValidatePublish()
        {
            if (!ValidateTitleSpecified())
                return false;

            if (!CheckSpelling())
                return false;

            else
                return true;
        }
        void IBlogPostEditor.OnPublishSucceeded(BlogPost blogPost, PostResult postResult) { }
        bool IBlogPostEditor.IsDirty { get { return false; } }
        void IBlogPostEditor.OnBlogChanged(Blog newBlog)
        {
            AdaptToBlog(newBlog);
            CheckForServiceUpdates(newBlog);
        }

        void IBlogPostEditor.OnBlogSettingsChanged(bool templateChanged)
        {
            using (Blog blog = new Blog(_editingManager.BlogId))
                AdaptToBlog(blog);
        }

        private void AdaptToBlog(Blog newBlog)
        {
            // if the blog supports posting to draft or not
            commandPostAsDraft.Enabled = newBlog.ClientOptions.SupportsPostAsDraft;
            if (newBlog.ClientOptions.SupportsPostAsDraft)
                commandSavePost.CommandBarButtonContextMenuDefinition = _savePostContextMenuDefinition;
            else
                commandSavePost.CommandBarButtonContextMenuDefinition = null;

            // if the blog supports post draft and edit online
            commandPostAsDraftAndEditOnline.Enabled = newBlog.ClientOptions.SupportsPostAsDraft && (newBlog.ClientOptions.PostEditingUrl != String.Empty);

            // if the blog supports pages or not
            bool enablePages = newBlog.ClientOptions.SupportsPages;
            commandNewPage.Enabled = enablePages;
            if (enablePages)
                commandNewPost.CommandBarButtonContextMenuDefinition = _newPostContextMenuDefinition;
            else
                commandNewPost.CommandBarButtonContextMenuDefinition = null;
        }

        private void CheckForServiceUpdates(Blog blog)
        {
            if (_editingManager.BlogIsAutoUpdatable && !ApplicationDiagnostics.SuppressBackgroundRequests)
            {
                ServiceUpdateChecker checker = new ServiceUpdateChecker(blog.Id, new WeblogSettingsChangedHandler(FireWeblogSettingsChangedEvent));
                checker.Start();
            }
        }

        /// <summary>
        /// Responds to global blog deletion events.
        /// </summary>
        /// <param name="blogId"></param>
        private void HandleBlogDeleted(string blogId)
        {
            if (InvokeRequired)
                BeginInvoke(new InvokeInUIThreadDelegate(HandleMaybeBlogDeleted));
            else
                HandleMaybeBlogDeleted();
        }

        /// <summary>
        /// Updates the editor to use a new blog if the current blog has been deleted.
        /// </summary>
        private void HandleMaybeBlogDeleted()
        {
            Debug.Assert(!InvokeRequired, "This method must be invoked on the UI thread!");

            // if the current weblog got deleted as part of this operation then reselect the
            // new default weblog
            if (!BlogSettings.BlogIdIsValid(_editingManager.BlogId))
            {
                _editingManager.SwitchBlog(BlogSettings.DefaultBlogId);
            }
        }

        #endregion

        #region Private Helper Methods

        private void ManageCommands()
        {
            // Temporary work around for WinLive 51425.
            if (ApplicationDiagnostics.AutomationMode)
                commandDeleteDraft.Enabled = true;
            else
                commandDeleteDraft.Enabled = _editingManager.PostIsDraft && _editingManager.PostIsSaved;
        }

        /// <summary>
        /// Update the title and status bars as appropriate
        /// </summary>
        private void UpdateFrameUI()
        {
            // calculate the text that describes the post
            string title = _htmlEditor.Title;
            string postDescription = (title != String.Empty) ? title : Res.Get(StringId.Untitled);

            // update frame window
            _mainFrameWindow.Caption = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WindowTitleFormat), postDescription, ApplicationEnvironment.ProductNameQualified);

            UpdatePostStatusUI();
        }

        private void UpdatePostStatusUI()
        {
            string statusText;
            if (_editingManager.PostIsDraft || string.IsNullOrEmpty(_editingManager.BlogPostId))
            {
                DateTime dateSaved = _editingManager.PostDateSaved ?? DateTime.MinValue;
                statusText = dateSaved != DateTime.MinValue
                                 ? String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.StatusDraftSaved),
                                                   FormatUtcDate(dateSaved))
                                 : Res.Get(StringId.StatusDraftUnsaved);
            }
            else
            {
                statusText = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.StatusPublished), FormatUtcDate(_editingManager.PostDatePublished));
            }

            _htmlEditor.StatusBar.SetStatusMessage(statusText);
        }

        private string FormatUtcDate(DateTime dateTime)
        {
            DateTime localDateTime = DateTimeHelper.UtcToLocal(dateTime);
            return CultureHelper.GetDateTimeCombinedPattern(localDateTime.ToShortDateString(), localDateTime.ToShortTimeString());
        }

        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // PostEditorMainControl
            //
            this.Name = "PostEditorMainControl";
            this.Size = new System.Drawing.Size(499, 276);

        }
        #endregion

        #region IBlogPostEditingSite Members

        IMainFrameWindow IBlogPostEditingSite.FrameWindow
        {
            get
            {
                return _mainFrameWindow;
            }
        }

        void IBlogPostEditingSite.ConfigureWeblog(string blogId, Type selectedPanel)
        {
            using (new WaitCursor())
            {
                // edit settings
                if (WeblogSettingsManager.EditSettings(FindForm(), blogId, selectedPanel))
                {
                    // broadcast event
                    FireWeblogSettingsChangedEvent(blogId, true);
                }
            }
        }

        void IBlogPostEditingSite.ConfigureWeblogFtpUpload(string blogId)
        {
            if (WeblogSettingsManager.EditFtpImageUpload(FindForm(), blogId))
            {
                // broadcast event
                FireWeblogSettingsChangedEvent(blogId, false);
            }
        }

        bool IBlogPostEditingSite.UpdateWeblogTemplate(string blogID)
        {
            if (_editingManager.VerifyBlogCredentials())
            {
                using (PostHtmlEditingSettings editSettings = new PostHtmlEditingSettings(blogID))
                {
                    using (BlogSettings settings = BlogSettings.ForBlogId(blogID))
                    {
                        Color? backgroundColor;
                        BlogEditingTemplateFile[] templates = BlogEditingTemplateDetector.DetectTemplate(
                            new BlogClientUIContextImpl(_mainFrameWindow, _mainFrameWindow),
                            this,
                            settings,
                            !_editingManager.BlogIsAutoUpdatable,
                            out backgroundColor); // only probe if we do not support auto-update

                        if (templates.Length != 0)
                        {
                            editSettings.EditorTemplateHtmlFiles = templates;
                            if (backgroundColor != null)
                            {
                                IDictionary hpo = settings.HomePageOverrides ?? new Hashtable();
                                hpo[BlogClientOptions.POST_BODY_BACKGROUND_COLOR] =
                                    backgroundColor.Value.ToArgb().ToString(CultureInfo.InvariantCulture);

                                settings.HomePageOverrides = hpo;
                            }
                            FireWeblogSettingsChangedEvent(blogID, true);
                            return true;
                        }

                    }
                }
            }

            return false;
        }

        void IBlogPostEditingSite.AddWeblog()
        {
            using (new WaitCursor())
            {
                bool switchToWeblog;
                string newBlogId = WeblogConfigurationWizardController.Add(this, true, out switchToWeblog);
                if (newBlogId != null)
                {
                    (this as IBlogPostEditingSite).NotifyWeblogAccountListEdited();
                    if (switchToWeblog)
                    {
                        _editingManager.SwitchBlog(newBlogId);
                    }
                }
            }
        }

        void IBlogPostEditingSite.NotifyWeblogSettingsChanged(bool templateChanged)
        {
            (this as IBlogPostEditingSite).NotifyWeblogSettingsChanged(_editingManager.BlogId, templateChanged);
        }

        void IBlogPostEditingSite.NotifyWeblogSettingsChanged(string blogId, bool templateChanged)
        {
            FireWeblogSettingsChangedEvent(blogId, templateChanged);
        }

        void IBlogPostEditingSite.NotifyWeblogAccountListEdited()
        {
            FireWeblogListChangedEvent();
        }

        private void weblogAccountManagementForm_WeblogSettingsEdited(string blogId, bool templateChanged)
        {
            FireWeblogSettingsChangedEvent(blogId, templateChanged);
        }

        void IBlogPostEditingSite.OpenLocalPost(PostInfo postInfo)
        {
            _editingManager.OpenLocalPost(postInfo);
        }

        void IBlogPostEditingSite.DeleteLocalPost(PostInfo postInfo)
        {
            try
            {
                _editingManager.DeleteLocalPost(postInfo);
            }
            catch (Exception ex)
            {
                DisplayableExceptionDisplayForm.Show(_mainFrameWindow, ex);
            }
        }

        string IBlogPostEditingSite.CurrentAccountId
        {
            get
            {
                if (_editingManager != null)
                    return _editingManager.BlogId;
                else
                    return null;
            }
        }

        public event WeblogHandler WeblogChanged;

        public event WeblogSettingsChangedHandler WeblogSettingsChanged;

        public event WeblogSettingsChangedHandler GlobalWeblogSettingsChanged
        {
            add
            {
                RegisterWeblogSettingsChangedListener(this, value);
            }
            remove
            {
                UnregisterWeblogSettingsChangedListener(this, value);
            }
        }

        public event EventHandler WeblogListChanged
        {
            add
            {
                RegisterWeblogListChangedListener(this, value);
            }
            remove
            {
                UnregisterWeblogListChangedListener(this, value);
            }
        }

        public event EventHandler PostListChanged
        {
            add
            {
                RegisterPostListChangedListener(this, value);
            }
            remove
            {
                UnregisterPostListChangedListener(this, value);
            }
        }

        #endregion

        #region Implementation of post list changed event

        private void _editingManager_UserSavedPost(object sender, EventArgs e)
        {
            FirePostListChangedEvent();
        }

        private void _editingManager_UserPublishedPost(object sender, EventArgs e)
        {
            FirePostListChangedEvent();
        }

        private void _editingManager_UserDeletedPost(object sender, EventArgs e)
        {
            FirePostListChangedEvent();
        }

        private static void RegisterPostListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_postListChangedListeners)
            {
                _postListChangedListeners[listener] = controlContext;
            }
        }
        private static void UnregisterPostListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_postListChangedListeners)
            {
                _postListChangedListeners.Remove(listener);
            }
        }

        public void FirePostListChangedEvent()
        {
            try
            {
                // notify listeners of post-list changed
                lock (_postListChangedListeners)
                {
                    // first refresh the post-list cache for high-performance refresh
                    PostListCache.Update();
                    WriterJumpList.Invalidate(Handle);

                    CommandManager.Invalidate(CommandId.MRUList);
                    CommandManager.Invalidate(CommandId.OpenDraftSplit);
                    CommandManager.Invalidate(CommandId.OpenPostSplit);

                    // now notify all of the listeners asynchronously
                    foreach (DictionaryEntry listener in _postListChangedListeners)
                    {
                        Control control = listener.Value as Control;
                        if (ControlHelper.ControlCanHandleInvoke(control))
                        {
                            control.BeginInvoke(listener.Key as EventHandler, new object[] { control, EventArgs.Empty });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                // Log without assertion dialog - post list changed events are non-critical
                Trace.WriteLine("FirePostListChangedEvent: Unexpected exception (non-fatal): " + ex.ToString());
            }
        }

        private static Hashtable _postListChangedListeners = new Hashtable();

        #endregion

        #region Implementation of weblog settings changed event

        private static void RegisterWeblogSettingsChangedListener(Control controlContext, WeblogSettingsChangedHandler listener)
        {
            lock (_weblogSettingsChangedListeners)
            {
                _weblogSettingsChangedListeners[listener] = controlContext;
            }
        }
        private static void UnregisterWeblogSettingsChangedListener(Control controlContext, WeblogSettingsChangedHandler listener)
        {
            lock (_weblogSettingsChangedListeners)
            {
                _weblogSettingsChangedListeners.Remove(listener);
            }
        }

        private static void RegisterWeblogListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_weblogListChangedListeners)
            {
                _weblogListChangedListeners[listener] = controlContext;
            }
        }
        private static void UnregisterWeblogListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_weblogListChangedListeners)
            {
                _weblogListChangedListeners.Remove(listener);
            }
        }

        private static void FireWeblogSettingsChangedEvent(string blogId, bool templateChanged)
        {
            try
            {
                // notify listeners of post-list changed
                lock (_weblogSettingsChangedListeners)
                {
                    // now notify all of the listeners asynchronously
                    foreach (DictionaryEntry listener in _weblogSettingsChangedListeners)
                    {
                        Control control = listener.Value as Control;

                        if (ControlHelper.ControlCanHandleInvoke(control))
                        {
                            try
                            {
                                control.BeginInvoke(listener.Key as WeblogSettingsChangedHandler, new object[] { blogId, templateChanged });
                            }
                            catch (Exception ex)
                            {
                                Trace.Fail("Unexpected error calling BeginInvoke: " + ex.ToString());
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception firing weblog settings changed event: " + ex.ToString());
            }
        }

        private static Hashtable _weblogSettingsChangedListeners = new Hashtable();

        private static Hashtable _weblogListChangedListeners = new Hashtable();
        private static void FireWeblogListChangedEvent()
        {
            try
            {
                // notify listeners of post-list changed
                lock (_weblogListChangedListeners)
                {
                    // now notify all of the listeners asynchronously
                    foreach (DictionaryEntry listener in _weblogListChangedListeners)
                    {
                        Control control = listener.Value as Control;

                        if (ControlHelper.ControlCanHandleInvoke(control))
                        {
                            try
                            {
                                control.BeginInvoke(listener.Key as EventHandler, EventArgs.Empty);
                            }
                            catch (Exception ex)
                            {
                                Trace.Fail("Unexpected error calling BeginInvoke: " + ex.ToString());
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception firing weblog settings changed event: " + ex.ToString());
            }
        }

        #endregion

        #region internal properties

        internal string CurrentBlogId
        {
            get
            {
                return this._editingManager.BlogId;
            }
        }

        #endregion

        #region Focus Methods
        internal IFocusableControl[] GetFocusPanes()
        {
            return _htmlEditor.GetFocusablePanes();
        }

        #endregion

        public void OnClosing(CancelEventArgs e)
        {
        }

        public void OnPostClosing(CancelEventArgs e)
        {

        }

        public void OnClosed() { }

        public void OnPostClosed() { }

        #region IBlogPostEditingSite Members

        public IHtmlStylePicker StyleControl
        {
            get { return _styleComboControl; }
        }

        #endregion

        public int OnViewChanged(uint viewId, CommandTypeID typeID, object view, ViewVerb verb, int uReasonCode)
        {
            System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} OnViewChanged called: viewId={viewId}, verb={verb}");
            if (ribbon == null)
            {
                ribbon = view as IUIRibbon;
            }

            if (ribbon != null)
            {
                switch (verb)
                {
                    case ViewVerb.Create:
                        System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} OnViewChanged - ViewVerb.Create, calling LoadRibbonSettings");
                        LoadRibbonSettings();
                        System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} OnViewChanged - LoadRibbonSettings done");
                        break;
                    case ViewVerb.Destroy:
                        break;
                    case ViewVerb.Error:
                        Trace.Fail("Ribbon error: " + uReasonCode);
                        break;
                    case ViewVerb.Size:
                        uint ribbonHeight;
                        if (ribbon != null && ComHelper.SUCCEEDED(ribbon.GetHeight(out ribbonHeight)))
                        {
                            Debug.Assert(ribbonHeight >= 0);
                            OnSizeChanged(EventArgs.Empty);
                        }
                        break;
                    default:
                        Debug.Assert(false, "Unexpected ViewVerb!");
                        break;
                }
            }
            return HRESULT.S_OK;
        }

        /// <summary>
        /// All instances of Writer use the same Ribbon.dat file, so we need to be careful of race conditions.
        /// </summary>
        private static object _ribbonSettingsLock = new object();
        private static bool _ribbonSettingsLoadSaveActive = false;

        public void LoadRibbonSettings()
        {
            if (ribbon == null)
                return;

            lock (_ribbonSettingsLock)
            {
                try
                {
                    WithRibbonSettingsIStream(false, false, true, ribbon.LoadSettingsFromStream);
                }
                catch (Exception e)
                {
                    Trace.Fail("LoadRibbonSettings failed: " + e);
                }
            }
        }

        public void SaveRibbonSettings()
        {
            if (ribbon == null)
                return;

            lock (_ribbonSettingsLock)
            {
                try
                {
                    WithRibbonSettingsIStream(true, true, false, ribbon.SaveSettingsToStream);
                }
                catch (Exception e)
                {
                    Trace.Fail("SaveRibbonSettings failed: " + e);
                }
            }
        }

        private DateTime? ribbonSettingsTimestamp;

        private delegate int WithIStreamAction(IStream stream);
        /// <summary>
        ///
        /// </summary>
        /// <param name="create">Create the stream if it doesn't exist, and make it writable.</param>
        /// <param name="onlyIfChanged"></param>
        /// <param name="action"></param>
        private void WithRibbonSettingsIStream(bool create, bool writable, bool onlyIfChanged, WithIStreamAction action)
        {
            // Reentrancy check
            // We could have reentrancy while loading/saving the settings because of CLR/COM pumping messages
            // while pinvoking and marshalling interface pointers.
            // If we are already in the process of saving/loading, then we can't do this action.
            // There is no user bad of skipping this in case of reentrancy for the following reasons:
            //  - If this is a Load and the one before is a Save, then we can skip this since we haven't finished saving to load anything.
            //  - And vice-versa.
            if (_ribbonSettingsLoadSaveActive)
            {
                return;
            }

            try
            {
                // Flag this to prevent us from shooting ourselves on the foot in case of reentrancy
                _ribbonSettingsLoadSaveActive = true;

                string ribbonFile = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "Ribbon.dat");
                FileInfo fileInfo = new FileInfo(ribbonFile);
                if (onlyIfChanged && ribbonSettingsTimestamp != null && fileInfo.LastWriteTimeUtc == ribbonSettingsTimestamp)
                {
                    // We're up-to-date, skip the action
                    return;
                }

                IStream stream = CreateRibbonIStream(ribbonFile, writable, create);
                if (stream == null)
                    return;
                try
                {
                    int hr = action(stream);
                    if (hr != HRESULT.S_OK)
                        Trace.Fail("Ribbon state load/save operation failed: 0x" + hr.ToString("X8", CultureInfo.InvariantCulture));
                }
                catch (Exception)
                {
                    Trace.Fail("WithIStreamAction failed.");
                    throw;
                }
                finally
                {
                    Marshal.ReleaseComObject(stream);
                }

                // on successful completion, save the time
                fileInfo.Refresh();
                ribbonSettingsTimestamp = fileInfo.LastWriteTimeUtc;
            }
            catch (Exception)
            {
                Trace.Fail("Ribbon settings failure.  Check the ribbon.dat file.");
                throw;
            }
            finally
            {
                _ribbonSettingsLoadSaveActive = false;
            }
        }

        private static IStream CreateRibbonIStream(string filename, bool writable, bool create)
        {
            try
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Creating a {0} ribbon istream for {1}", writable ? "writable" : "readable", filename));
                STGM mode = writable ? STGM.WRITE : STGM.READ;
                if (create)
                    mode |= STGM.CREATE;
                const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
                IStream stream;
                int hr = Shlwapi.SHCreateStreamOnFileEx(filename, (int)mode, FILE_ATTRIBUTE_NORMAL, create, IntPtr.Zero, out stream);
                if (hr != HRESULT.S_OK)
                {
                    Trace.WriteLine("Failed to create ribbon stream for " + filename + ": hr = " + hr.ToString("X8", CultureInfo.InvariantCulture));
                    return null;
                }

                return stream;
            }
            catch (Exception e)
            {
                Trace.Fail(e.ToString());
                return null;
            }
        }

        public int OnCreateUICommand(uint commandId, CommandTypeID typeID, out IUICommandHandler commandHandler)
        {
            // Verbose logging commented out - uncomment if debugging Ribbon issues:
            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} OnCreateUICommand: commandId={commandId}, typeID={typeID}");
            commandHandler = _htmlEditor.CommandManager;
            // System.Diagnostics.Debug.WriteLine($"[OLW-DEBUG] {DateTime.Now:HH:mm:ss.fff} OnCreateUICommand: returning handler");
            return HRESULT.S_OK;
        }

        public int OnDestroyUICommand(uint commandId, CommandTypeID typeID, IUICommandHandler commandHandler)
        {
            return HRESULT.E_NOTIMPL;
        }

        #region Implementation of ISessionHandler

        public void OnEndSession()
        {
            _autoSaveTimer_Tick(null, EventArgs.Empty);
        }

        #endregion
    }
}
