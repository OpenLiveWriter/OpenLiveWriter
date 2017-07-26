// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.PostEditor.ContentSources.Common;
using OpenLiveWriter.PostEditor.Emoticons;
using OpenLiveWriter.PostEditor.OpenPost;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.PostEditor.Tables;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.Tagging;
using OpenLiveWriter.SpellChecker;
using OpenLiveWriter.InternalWriterPlugin;
using Timer = System.Windows.Forms.Timer;
using OpenLiveWriter.PostEditor.WordCount;
using OpenLiveWriter.PostEditor.Video;
using System.Collections.Generic;
using OpenLiveWriter.PostEditor.ImageInsertion.WebImages;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.Mshtml.Mshtml_Interop;
using IDropTarget = OpenLiveWriter.Interop.Com.IDropTarget;
using OpenLiveWriter.PostEditor.Commands;

namespace OpenLiveWriter.PostEditor
{
    internal class ContentEditor : IBlogPostContentEditor, IContentSourceSite, IDisposable, IHtmlEditorCommandSource, IHtmlEditorHost, IBlogPostImageEditingContext, IBlogPostSidebarContext, IContentSourceSidebarContext, IBlogPostSpellCheckingContext, ICommandManagerHost, IEditingMode, IInternalSmartContentContextSource
    {
        public ContentEditor(IMainFrameWindow mainFrameWindow, Control editorContainer, IBlogPostEditingSite postEditingSite, IInternetSecurityManager internetSecurityManager, BlogPostHtmlEditorControl.TemplateStrategy templateStrategy, int dlControlFlags)
        {
            // create a docked panel to host the editor
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;

            if (!BidiHelper.IsRightToLeft)
                panel.DockPadding.Right = 0;
            else
                panel.DockPadding.Left = 0;

            editorContainer.Controls.Add(panel);
            panel.Resize += new EventHandler(panel_Resize);
            if (BidiHelper.IsRightToLeft)
                editorContainer.RightToLeft = RightToLeft.Yes;

            // save references
            _mainFrameWindow = mainFrameWindow;
            _editorContainer = panel;
            _postEditingSite = postEditingSite;

            _commandManager = new CommandManager();

            _userPreferencesMonitor = new UserPreferencesMonitor();

            // To be high-contrast-aware we need to respond to changes in the high contrast mode
            // by invalidating commands, forcing the ribbon to ask us for new high contrast images.
            _userPreferencesMonitor.AccessibilityUserPreferencesChanged +=
                new EventHandler(delegate (object sender, EventArgs args)
                                     {
                                         _commandManager.InvalidateAllImages();
                                     });

            _imageDecoratorsManager = new LazyLoader<ImageDecoratorsManager>(() => new ImageDecoratorsManager(components, CommandManager, GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ImageBorderInherit)));
            _emoticonsManager = new EmoticonsManager(this, this);

            // initialize commands
            InitializeCommands();

            // initialize normal editor
            InitializeNormalEditor(postEditingSite, internetSecurityManager, templateStrategy, dlControlFlags);

            // initialize source editor
            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SourceEditor))
                InitializeSourceEditor();

            InitializeViewCommands();

            // initialize custom content
            InitializeContentSources();

            // bring main editor panel to front (this must be here for the sidebar to work!!!!)
            panel.BringToFront();
        }

        private EditingMode _lastNonPreviewEditingMode = EditingMode.Wysiwyg;

        public EditingMode LastNonPreviewEditingMode
        {
            get { return _lastNonPreviewEditingMode; }
            set { _lastNonPreviewEditingMode = value; }
        }

        /// <summary>
        /// The default to be used in the editor.  The string should be formatted to be used with
        /// IDM_COMPOSESETTINGS http://msdn.microsoft.com/en-us/library/aa769901(VS.85).aspx
        ///
        /// </summary>
        /// <returns></returns>
        public void SetDefaultFont(string fontString)
        {
            _mshtmlOptions.EditingOptions[IDM.COMPOSESETTINGS] = fontString;
            _normalHtmlContentEditor.UpdateOptions(_mshtmlOptions, true);
        }

        private MshtmlOptions _mshtmlOptions;

        private void InitializeNormalEditor(IBlogPostEditingSite postEditingSite, IInternetSecurityManager internetSecurityManager, BlogPostHtmlEditorControl.TemplateStrategy templateStrategy, int dlControlFlags)
        {
            // configure editing options
            _mshtmlOptions = new MshtmlOptions();
            _mshtmlOptions.UseDivForCarriageReturn = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.DivNewLine);
            // Commented out for now to prevent first chance exception
            //_mshtmlOptions.EditingOptions.Add(IDM.RESPECTVISIBILITY_INDESIGN, GlobalEditorOptions.SupportsFeature(ContentEditorFeature.HideNonVisibleElements));
            _mshtmlOptions.EditingOptions.Add(IDM.AUTOURLDETECT_MODE, false);
            _mshtmlOptions.EditingOptions.Add(IDM.MULTIPLESELECTION, false);
            _mshtmlOptions.EditingOptions.Add(IDM.LIVERESIZE, true);
            _mshtmlOptions.EditingOptions.Add(IDM.KEEPSELECTION, true);
            _mshtmlOptions.EditingOptions.Add(IDM.DISABLE_EDITFOCUS_UI, true);
            _mshtmlOptions.DLCTLOptions = dlControlFlags;
            _mshtmlOptions.DocHostUIOptionKeyPath = GlobalEditorOptions.GetSetting<string>(ContentEditorSetting.MshtmlOptionKeyPath);

            // create the editor
            _normalHtmlContentEditor = new BlogPostHtmlEditorControl(_mainFrameWindow, StatusBar, _mshtmlOptions, this, this, this, new SmartContentResizedListener(ResizedListener), this, new SharedCanvasImageReferenceFixer(ReferenceFixer), internetSecurityManager, CommandManager, templateStrategy, this);
            _normalHtmlContentEditor.PostBodyInlineStyle = GetPostBodyInlineStyleOverrides();
            // hookup services and events
            _normalHtmlContentEditor.HtmlGenerationService = new HtmlGenerator(this);
            _normalHtmlContentEditor.DataFormatHandlerFactory = new ExtendedHtmlEditorMashallingHandler(_normalHtmlContentEditor, this, this, postEditingSite as IDropTarget);
            _normalHtmlContentEditor.DocumentComplete += new EventHandler(htmlEditor_DocumentComplete);
            _normalHtmlContentEditor.GotFocus += htmlEditor_GotFocus;
            _normalHtmlContentEditor.LostFocus += htmlEditor_LostFocus;

            _normalHtmlContentEditor.SelectedImageResized += new SelectedImageResizedHandler(_normalHtmlContentEditor_SelectedImageResized);
            _normalHtmlContentEditor.UpdateImageLink += new UpdateImageLinkHandler(_normalHtmlContentEditor_UpdateImageLink);
            _normalHtmlContentEditor.HtmlInserted += new EventHandler(_normalHtmlContentEditor_HtmlInserted);
            _normalHtmlContentEditor.HelpRequest += new EventHandler(_normalHtmlContentEditor_HelpRequest);
            _normalHtmlContentEditor.EditorControl.AccessibleName = "Editor";

            _normalHtmlContentEditor.KeyboardLanguageChanged += new EventHandler(_normalHtmlContentEditor_KeyboardLanguageChanged);
            _normalHtmlContentEditor.TrackKeyboardLanguageChanges = true;

            // initialize the sidebar
            _htmlEditorSidebarHost = new HtmlEditorSidebarHost(_normalHtmlContentEditor);
            _htmlEditorSidebarHost.Dock = !BidiHelper.IsRightToLeft ? DockStyle.Right : DockStyle.Left;
            _htmlEditorSidebarHost.Width = Res.SidebarWidth;
            if (BidiHelper.IsRightToLeft)
            {
                _htmlEditorSidebarHost.RightToLeft = RightToLeft.Yes;
                BidiHelper.RtlLayoutFixup(_htmlEditorSidebarHost);
            }
            _editorContainer.Controls.Add(_htmlEditorSidebarHost);
            _htmlEditorSidebarHost.BringToFront();
            _htmlEditorSidebarHost.VisibleChanged += _htmlEditorSidebarHost_VisibleChanged;

            // register sidebars
            _htmlEditorSidebarHost.RegisterSidebar(new DisabledContentSourceSidebar(this));
            _htmlEditorSidebarHost.RegisterSidebar(new BrokenContentSourceSidebar(this));
            _htmlEditorSidebarHost.RegisterSidebar(new ContentSourceSidebar(this));
            _htmlEditorSidebarHost.RegisterSidebar(new ImagePropertiesSidebar(_normalHtmlContentEditor, this, new CreateFileCallback(CreateFileHandler)));
            _htmlEditorSidebarHost.AccessibleName = "Sidebar";

            // register the normal editor
            RegisterEditor(_normalHtmlContentEditor);
        }

        void _normalHtmlContentEditor_KeyboardLanguageChanged(object sender, EventArgs e)
        {
            _mainFrameWindow.OnKeyboardLanguageChanged();
        }

        public virtual IStatusBar StatusBar
        {
            get
            {
                return new NullStatusBar();
            }
        }

        protected virtual string GetPostBodyInlineStyleOverrides()
        {
            return null;
        }

        private void InitializeSourceEditor()
        {
            // create the source code editor control
            _codeHtmlContentEditor = new BlogPostHtmlSourceEditorControl(this, CommandManager, this);

            _codeHtmlContentEditor.AccessibleName = "Source Editor";

            // register the control
            RegisterEditor(_codeHtmlContentEditor);
        }

        protected Command[] _views;

        private void InitializeViewCommands()
        {
            _views = new Command[]
                {
                    commandViewNormal,
                    commandViewWebPreview,
                    commandViewCode
                };
        }

        public IHtmlEditorComponentContext IHtmlEditorComponentContext
        {
            get { return this._normalHtmlContentEditor; }
        }

        public string ReferenceFixer(BeginTag tag, string reference)
        {
            return ImageInsertionManager.ReferenceFixer(tag, reference, _fileService, this);
        }

        private UserPreferencesMonitor _userPreferencesMonitor;

        private readonly CommandManager _commandManager;
        public CommandManager CommandManager
        {
            get
            {
                return _commandManager;
            }
        }

        protected virtual string GetPreviewHtml()
        {
            return _editingTemplatePreview.Template;
        }

        // Shared group commands need to be here.
        protected virtual void InitializeCommands()
        {
            CommandManager.BeginUpdate();

            CommandManager.Add(new Command(CommandId.FontGroup)); // Has it's own icon.
            CommandManager.Add(new Command(CommandId.SemanticHtmlGroup));

            MarginCommand marginCommand = new MarginCommand(CommandManager);
            marginCommand.Enabled = false;
            CommandManager.Add(marginCommand);

            AlignmentCommand alignmentCommand = new AlignmentCommand(CommandManager);
            CommandManager.Add(alignmentCommand);

            CommandManager.Add(new Command(CommandId.TextEditingGroup));

            Command commandActivateContextualTab = new Command(CommandId.ActivateContextualTab);
            commandActivateContextualTab.Execute +=
                new EventHandler(
                   delegate (object sender, EventArgs args)
                   {
                       ContentEditor_BeforeInitialInsertion(this, EventArgs.Empty);
                       UpdateContextAvailability();
                       ContentEditor_AfterInitialInsertion(this, EventArgs.Empty);
                   });
            CommandManager.Add(commandActivateContextualTab);

            // Video contextual tab
            commandVideoContextTabGroup = new ContextAvailabilityCommand(CommandId.VideoContextTabGroup);
            CommandManager.Add(commandVideoContextTabGroup);
            commandFormatVideoTab = new ContextAvailabilityCommand(CommandId.FormatVideoTab);
            CommandManager.Add(commandFormatVideoTab);

            // Image contextual tab
            commandImageContextTabGroup = new ContextAvailabilityCommand(CommandId.ImageContextTabGroup);
            CommandManager.Add(commandImageContextTabGroup);
            commandFormatImageTab = new ContextAvailabilityCommand(CommandId.FormatImageTab);
            CommandManager.Add(commandFormatImageTab);

            // Map contextual tab
            commandMapContextTabGroup = new ContextAvailabilityCommand(CommandId.MapContextTabGroup);
            CommandManager.Add(commandMapContextTabGroup);
            commandFormatMapTab = new ContextAvailabilityCommand(CommandId.FormatMapTab);
            CommandManager.Add(commandFormatMapTab);
            CommandManager.Add(new Command(CommandId.FormatMapGroup));
            CommandManager.Add(new Command(CommandId.FormatMapPropertiesGroup));

            // Tag contextual tab
            commandTagContextTabGroup = new ContextAvailabilityCommand(CommandId.TagContextTabGroup);
            CommandManager.Add(commandTagContextTabGroup);
            commandFormatTagTab = new ContextAvailabilityCommand(CommandId.FormatTagTab);
            CommandManager.Add(commandFormatTagTab);
            CommandManager.Add(new Command(CommandId.FormatTagPropertiesGroup));
            CommandManager.Add(new Command(CommandId.FormatTagProvidersGroup));
            CommandManager.Add(new Command(CommandId.AddTagProvider));
            CommandManager.Add(new Command(CommandId.ManageTagProviders));
            CommandManager.Add(new Command(CommandId.EditTags));

            // Table contextual tab
            commandTableContextTabGroup = new ContextAvailabilityCommand(CommandId.TableContextTabGroup);
            CommandManager.Add(commandTableContextTabGroup);
            commandFormatTableTab = new ContextAvailabilityCommand(CommandId.FormatTableTab);
            CommandManager.Add(commandFormatTableTab);
            CommandManager.Add(new Command(CommandId.FormatTablePropertiesGroup));
            CommandManager.Add(new Command(CommandId.FormatTableInsertGroup));
            CommandManager.Add(new Command(CommandId.FormatTableMoveGroup));
            CommandManager.Add(new Command(CommandId.FormatTableEditingGroup));

            commandViewNormal = new Command(CommandId.ViewNormal);
            commandViewNormal.Execute += new EventHandler(commandViewNormal_Execute);
            CommandManager.Add(commandViewNormal);
            if (!GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ViewNormalEditorShortcut))
                CommandManager.IgnoreShortcut(commandViewNormal.Shortcut);

            commandViewPlainText = new Command(CommandId.ViewPlainText);
            commandViewPlainText.Execute += new EventHandler(commandViewPlainText_Execute);
            commandViewPlainText.On = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.PlainTextEditor);
            CommandManager.Add(commandViewPlainText);

            commandViewWebPreview = new Command(CommandId.ViewPreview);
            commandViewWebPreview.Execute += new EventHandler(commandViewWebPreview_Execute);
            commandViewWebPreview.On = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.PreviewMode);
            CommandManager.Add(commandViewWebPreview);

            commandViewCode = new Command(CommandId.ViewCode);
            commandViewCode.Execute += new EventHandler(commandViewCode_Execute);
            commandViewCode.On = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SourceEditor);
            CommandManager.Add(commandViewCode);

            commandViewSidebar = new Command(CommandId.ViewSidebar);
            commandViewSidebar.BeforeShowInMenu += new EventHandler(commandViewSidebar_BeforeShowInMenu);
            commandViewSidebar.Execute += new EventHandler(commandViewSidebar_Execute);
            CommandManager.Add(commandViewSidebar);

            commandInsertPicture = CommandManager.Add(CommandId.InsertPictureFromFile, commandInsertPicture_Execute);

            _videoProvidersFeatureEnabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.VideoProviders);
            commandInsertVideoFromFile = new Command(CommandId.InsertVideoFromFile);
            commandInsertVideoFromFile.Enabled = _videoProvidersFeatureEnabled;
            commandInsertVideoFromFile.Execute += new EventHandler(commandInsertVideoFromFile_Execute);
            CommandManager.Add(commandInsertVideoFromFile);

            commandInsertVideoFromWeb = new Command(CommandId.InsertVideoFromWeb);
            commandInsertVideoFromWeb.Enabled = _videoProvidersFeatureEnabled;
            commandInsertVideoFromWeb.Execute += new EventHandler(commandInsertVideoFromWeb_Execute);
            CommandManager.Add(commandInsertVideoFromWeb);

            bool tagProvidersFeatureEnabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.TagProviders);
            CommandManager.Add(CommandId.InsertTags, commandInsertTags_Execute, tagProvidersFeatureEnabled);

            commandInsertEmoticon = new EmoticonsGalleryCommand(CommandId.InsertEmoticon, this);
            CommandManager.Add(commandInsertEmoticon, commandInsertEmoticon_Execute);

            commandInsertVideoFromService = new Command(CommandId.InsertVideoFromService);
            commandInsertVideoFromService.Enabled = _videoProvidersFeatureEnabled;
            commandInsertVideoFromService.Execute += new EventHandler(commandInsertVideoFromService_Execute);
            CommandManager.Add(commandInsertVideoFromService);

            _mapsFeatureEnabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.Maps);
            commandInsertMap = new Command(CommandId.InsertMap);
            commandInsertMap.Enabled = _mapsFeatureEnabled;
            commandInsertMap.Execute += commandInsertMap_Execute;
            CommandManager.Add(commandInsertMap);

            _tagProvidersFeatureEnabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.TagProviders);
            commandInsertTags = new Command(CommandId.InsertTags);
            commandInsertTags.Enabled = _tagProvidersFeatureEnabled;
            commandInsertTags.Execute += commandInsertTags_Execute;
            CommandManager.Add(commandInsertTags);

            commandInsertTable = new Command(CommandId.InsertTable);
            commandInsertTable.Execute += new EventHandler(commandInsertTable_Execute);
            commandInsertTable.CommandBarButtonContextMenuDefinition = new TableContextMenuDefinition();
            commandInsertTable.CommandBarButtonContextMenuDefinition.CommandBar = true;
            CommandManager.Add(commandInsertTable);

            commandInsertTable2 = new Command(CommandId.InsertTable2);
            commandInsertTable2.Execute += new EventHandler(commandInsertTable_Execute);
            CommandManager.Add(commandInsertTable2);

            commandAddPlugin = new Command(CommandId.AddPlugin);
            commandAddPlugin.Execute += new EventHandler(commandAddPlugin_Execute);
            CommandManager.Add(commandAddPlugin);

            commandInsertHorizontalLine = new OverridableCommand(CommandId.InsertHorizontalLine);
            commandInsertHorizontalLine.Execute += commandInsertHorizontalLine_Execute;
            CommandManager.Add(commandInsertHorizontalLine);

            commandInsertClearBreak = CommandManager.Add(CommandId.InsertClearBreak, commandInsertClearBreak_Execute);

            CommandManager.Add(new Command(CommandId.FontGroup));

#if SUPPORT_FILES
            commandInsertFile = new Command(CommandId.InsertFile);
            commandInsertFile.Execute += new EventHandler(commandInsertFile_Execute);
            CommandManager.Add(commandInsertFile);
#endif

            CommandManager.Add(new CommandRecentPost());
            CommandManager.Add(CommandId.WordCount, new EventHandler(WordCount_Execute));
            if (ApplicationDiagnostics.TestMode)
            {
                CommandManager.Add(CommandId.ShowVideoErrorMessage, new EventHandler(commandShowVideoErrorMessage_Execute));
                CommandManager.Add(CommandId.ShowImageUploadError, new EventHandler(commandShowImageUploadError_Execute));
            }

            // Debug functions command to Mail and Writer.
            if (ApplicationDiagnostics.TestMode)
            {
                CommandManager.Add(CommandId.ViewSource, new EventHandler(commandViewSource_Execute));
            }

            CommandManager.EndUpdate();

            CommandManager.CommandStateChanged += new EventHandler(CommandManager_CommandStateChanged);

            wordCountTimer = new Timer();
            wordCountTimer.Interval = 250;
            wordCountTimer.Tick += new EventHandler(wordCountUpdate);
            WordCountSettings.SettingsChanged += new EventHandler(wordCountUpdate);
        }

        private ContextAvailabilityCommand commandImageContextTabGroup;
        private ContextAvailabilityCommand commandTableContextTabGroup;
        private ContextAvailabilityCommand commandVideoContextTabGroup;
        private ContextAvailabilityCommand commandMapContextTabGroup;
        private ContextAvailabilityCommand commandTagContextTabGroup;
        private ContextAvailabilityCommand commandFormatVideoTab;
        private ContextAvailabilityCommand commandFormatImageTab;
        private ContextAvailabilityCommand commandFormatMapTab;
        private ContextAvailabilityCommand commandFormatTableTab;
        private ContextAvailabilityCommand commandFormatTagTab;
        public virtual void UpdateContextAvailability()
        {
            commandImageContextTabGroup.ContextAvailability = ContextAvailability.NotAvailable;
            commandFormatImageTab.ContextAvailability = ContextAvailability.NotAvailable;

            commandMapContextTabGroup.ContextAvailability = ContextAvailability.NotAvailable;
            commandFormatMapTab.ContextAvailability = ContextAvailability.NotAvailable;

            commandTagContextTabGroup.ContextAvailability = ContextAvailability.NotAvailable;
            commandFormatTagTab.ContextAvailability = ContextAvailability.NotAvailable;

            commandVideoContextTabGroup.ContextAvailability = ContextAvailability.NotAvailable;
            commandFormatVideoTab.ContextAvailability = ContextAvailability.NotAvailable;

            // No contextual tabs in plain text or preview mode
            if (CurrentEditingMode == EditingMode.PlainText || CurrentEditingMode == EditingMode.Preview)
                return;

            bool initialInsertion = _makingInitialInsertion > 0;

            // We want this to get the source ids for any "containing" smart content.
            IHtmlEditorComponentContext componentContext = _currentEditor as IHtmlEditorComponentContext;
            MarkupRange selection = componentContext.Selection.SelectedMarkupRange;
            IHTMLElement smartContentElement = ContentSourceManager.GetContainingSmartContentElement(selection);

            string contentSourceId = smartContentElement != null ? smartContentElement.id : null;

            if (contentSourceId != null)
            {
                string sourceId;
                string contentBlockId;
                ContentSourceManager.ParseContainingElementId(contentSourceId, out sourceId, out contentBlockId);

                if (sourceId == MapContentSource.ID)
                {
                    if (ApplicationDiagnostics.TestMode)
                    {
                        commandMapContextTabGroup.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
                        commandFormatMapTab.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
                    }
                }
                else if (sourceId == TagContentSource.ID)
                {
                    if (ApplicationDiagnostics.TestMode)
                    {
                        commandTagContextTabGroup.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
                        commandFormatTagTab.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
                    }
                }
                else if (sourceId == VideoContentSource.ID)
                {
                    commandVideoContextTabGroup.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
                    commandFormatVideoTab.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
                }
            }

            bool imageSelected = componentContext.Selection.SelectedImage != null &&
                                 EmoticonsManager.GetEmoticon(componentContext.Selection.SelectedControl) == null;
            if (imageSelected)
                commandImageContextTabGroup.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
            else
            {
                commandImageContextTabGroup.ContextAvailability = ContextAvailability.NotAvailable;
            }
            commandFormatImageTab.ContextAvailability = commandImageContextTabGroup.ContextAvailability;

            TableSelection tableSelection = new TableSelection(selection);
            bool tableSelected = ((tableSelection.Table != null) &&
                                  (tableSelection.Table as IHTMLElement3).isContentEditable);
            if (tableSelected)
                commandTableContextTabGroup.ContextAvailability = initialInsertion ? ContextAvailability.Active : ContextAvailability.Available;
            else
                commandTableContextTabGroup.ContextAvailability = ContextAvailability.NotAvailable;
            commandFormatTableTab.ContextAvailability = commandTableContextTabGroup.ContextAvailability;
        }

        void ContentSourceManager_GlobalContentSourceListChanged(object sender, EventArgs e)
        {
            if (ControlHelper.ControlCanHandleInvoke(_mainFrameWindow as Control))
                _mainFrameWindow.BeginInvoke(new InvokeInUIThreadDelegate(UpdateContentSourceCommands), new object[] { });
        }

        void CommandManager_CommandStateChanged(object sender, EventArgs e)
        {
            Command command = (Command)sender;
            command.InvalidationCount++;

            try
            {
                if (_postEditingSite != null && _postEditingSite.RibbonFramework != null && command != null)
                {
                    command.FlushPendingInvalidations(_postEditingSite.RibbonFramework);

                    // NOTICE: Ribbon has a bug in SplitButton (Windows 7 712524). When all items inside a SplitButton change their enabled/disabled state
                    // while the ribbon is changing mode, the splitbutton state won't be updated correctly. Since Paste command is hosted
                    // by PasteSplit command, we now force to update the splitbutton state manually to ask Ribbon refresh the splitbutton.
                    switch (command.CommandId)
                    {
                        case CommandId.Paste:
                            _postEditingSite.RibbonFramework.InvalidateUICommand((uint)CommandId.PasteSplit, CommandInvalidationFlags.AllProperties, IntPtr.Zero);
                            break;
                        case CommandId.InsertPictureFromFile:
                            _postEditingSite.RibbonFramework.InvalidateUICommand((uint)CommandId.InsertImageSplit, CommandInvalidationFlags.AllProperties, IntPtr.Zero);
                            break;
                        case CommandId.InsertVideoFromFile:
                            _postEditingSite.RibbonFramework.InvalidateUICommand((uint)CommandId.InsertVideoSplit, CommandInvalidationFlags.AllProperties, IntPtr.Zero);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception thrown in InvalidateCommand: " + ex);
            }
        }

        private void commandInsertVideoFromService_Execute(object sender, EventArgs e)
        {
            InsertSmartContentFromTabbedDialog(VideoContentSource.ID, Convert.ToInt32(VideoContentSource.Tab.Service, CultureInfo.InvariantCulture));
        }

        private void commandInsertVideoFromWeb_Execute(object sender, EventArgs e)
        {
            InsertSmartContentFromTabbedDialog(VideoContentSource.ID, Convert.ToInt32(VideoContentSource.Tab.Web, CultureInfo.InvariantCulture));
        }

        private void commandInsertVideoFromFile_Execute(object sender, EventArgs e)
        {
            InsertSmartContentFromTabbedDialog(VideoContentSource.ID, Convert.ToInt32(VideoContentSource.Tab.File, CultureInfo.InvariantCulture));
        }

        private void commandInsertMap_Execute(object sender, EventArgs e)
        {
            CommandManager.Get(MapContentSource.ID).PerformExecute();
        }

        private void commandInsertTags_Execute(object sender, EventArgs e)
        {
            CommandManager.Get(TagContentSource.ID).PerformExecute();
        }

        private void commandInsertEmoticon_Execute(object sender, ExecuteEventHandlerArgs args)
        {
            EmoticonsGalleryCommand galleryCommand = (EmoticonsGalleryCommand)sender;
            int newSelectedIndex = args.GetInt(galleryCommand.CommandId.ToString());
            Emoticon emoticon = galleryCommand.Items[newSelectedIndex].Cookie;

            EmoticonsManager.AddToRecent(emoticon);
            using (EditorUndoUnit undo = new EditorUndoUnit(_currentEditor))
            {
                FocusBody();
                InsertHtml(EmoticonsManager.GetHtml(emoticon), HtmlInsertionOptions.MoveCursorAfter);
                undo.Commit();
            }
        }

        private void InitializeContentSources()
        {
            // create commands for the content-sources (used by the dynamic command menu and sidebar)
            CommandManager.BeginUpdate();
            // first built in
            foreach (ContentSourceInfo contentSourceInfo in ContentSourceManager.BuiltInInsertableContentSources)
                CommandManager.Add(new ContentSourceCommand(this, contentSourceInfo, true));

            // then plug ins
            UpdateContentSourceCommands();

            CommandManager.EndUpdate();

            // subscribe to changes in the list of content sources
            ContentSourceManager.GlobalContentSourceListChanged += new EventHandler(ContentSourceManager_GlobalContentSourceListChanged);

            // create a dynamic command menu for the content sources
            string basePath = new Command(CommandId.InsertPictureFromFile).MainMenuPath.Split('/')[0];

            DynamicCommandMenuOptions options = new DynamicCommandMenuOptions(basePath, 500, Res.Get(StringId.DynamicCommandMenuMore), Res.Get(StringId.DynamicCommandMenuInsert));
            options.UseNumericMnemonics = false;
            options.MaxCommandsShownOnMenu = 20;
            options.SeparatorBegin = true;
            IDynamicCommandMenuContext context = ContentSourceManager.CreateDynamicCommandMenuContext(options, CommandManager, this);
            DynamicCommandMenu contentSourceList = new DynamicCommandMenu(context);

            CommandContextMenuDefinition insertMenuDefinition = new CommandContextMenuDefinition();
            insertMenuDefinition.CommandBar = true;
            insertMenuDefinition.Entries.Add(CommandId.InsertLink, false, false);
            insertMenuDefinition.Entries.Add(CommandId.InsertPictureFromFile, false, false);
            insertMenuDefinition.Entries.Add(WebImageContentSource.ID, false, false);
            insertMenuDefinition.Entries.Add(CommandId.InsertTable2, true, true);

            foreach (string commandId in contentSourceList.CommandIdentifiers)
                if (WebImageContentSource.ID != commandId)
                    insertMenuDefinition.Entries.Add(commandId, false, false);
        }

        private void UpdateContentSourceCommands()
        {
            // add the commands
            foreach (ContentSourceInfo contentSourceInfo in ContentSourceManager.PluginInsertableContentSources)
            {
                ContentSourceCommand command = new ContentSourceCommand(this, contentSourceInfo, false);
                CommandManager.Add(command);
            }

            GalleryCommand<string> commandPluginsGallery = CommandManager.Get(CommandId.PluginsGallery) as GalleryCommand<string>;
            if (commandPluginsGallery != null)
                commandPluginsGallery.Invalidate();
        }

        /*
        public void ShowUpdatesAvailablePanel()
        {
            SetTopStripControl(new UpdatesAvailableBand(new UpdatesAvailableBand.HitClose(HidePanel)), false, true);
        }
        */

        private void RegisterEditor(IBlogPostHtmlEditor editor)
        {
            // create the editor and subscribe to events
            editor.CommandSource.CommandStateChanged += new EventHandler(editor_CommandStateChanged);
            editor.CommandSource.AggressiveCommandStateChanged += new EventHandler(editor_AggressiveCommandStateChanged);
            editor.IsDirtyEvent += new EventHandler(editor_IsDirtyEvent);
            // configure control and add to container (initially invisible)
            editor.EditorControl.Visible = false;
            editor.EditorControl.TabStop = true;
            editor.EditorControl.TabIndex = 2;
            editor.EditorControl.Dock = DockStyle.Fill;
            _editorContainer.Controls.Add(editor.EditorControl);
        }

        private void UnregisterEditor(IBlogPostHtmlEditor editor)
        {
            editor.CommandSource.CommandStateChanged -= new EventHandler(editor_CommandStateChanged);
            editor.CommandSource.AggressiveCommandStateChanged -= new EventHandler(editor_AggressiveCommandStateChanged);
            editor.IsDirtyEvent -= new EventHandler(editor_IsDirtyEvent);
            _editorContainer.Controls.Remove(editor.EditorControl);
        }

        private void DisposeHtmlEditor(HtmlEditorControl htmlEditor)
        {
            htmlEditor.DocumentComplete -= new EventHandler(htmlEditor_DocumentComplete);
            htmlEditor.GotFocus -= new EventHandler(htmlEditor_GotFocus);
            htmlEditor.LostFocus -= new EventHandler(htmlEditor_LostFocus);
            htmlEditor.KeyboardLanguageChanged -= new EventHandler(_normalHtmlContentEditor_KeyboardLanguageChanged);
            htmlEditor.HelpRequest -= new EventHandler(_normalHtmlContentEditor_HelpRequest);

            htmlEditor.Dispose();
        }

        private bool _disposed;
        public virtual void Dispose()
        {
            _disposed = true;
            wordCountTimer.Dispose();

            _editorContainer.Controls.Remove(_htmlEditorSidebarHost);
            _htmlEditorSidebarHost.Dispose();

            UnregisterEditor(_normalHtmlContentEditor);
            DisposeHtmlEditor(_normalHtmlContentEditor);

            if (_codeHtmlContentEditor != null)
            {
                UnregisterEditor(_codeHtmlContentEditor);
                _codeHtmlContentEditor.Dispose();
            }

            if (_imageDecoratorsManager.IsInitialized)
                DecoratorsManager.Dispose();

            if (components != null)
                components.Dispose();

            _userPreferencesMonitor.Dispose();
            _userPreferencesMonitor = null;

            ContentSourceManager.GlobalContentSourceListChanged -= new EventHandler(ContentSourceManager_GlobalContentSourceListChanged);
            CommandManager.CommandStateChanged -= new EventHandler(CommandManager_CommandStateChanged);

            if (_spellingChecker != null)
            {
                _spellingChecker.Dispose();
                _spellingChecker = null;
            }

            if (_insertImageDialogWin7 != null)
            {
                _insertImageDialogWin7.Dispose();
                _insertImageDialogWin7 = null;
            }

            DisposeTextEditingCommandDispatcher();

            GC.SuppressFinalize(this);
        }

        ~ContentEditor()
        {
            Trace.Fail("Failed to dispose BlogPostHtmlEditor");
        }

        public void Initialize(IBlogPostEditingContext editingContext, IEditorOptions clientOptions, string wysiwygHTML, string previewHTML, bool containsTitle)
        {
            // We suppress he editor reload here while we add the inital template because a bit further down
            // this function we will reload the editor using the BlogPost from editingContext
            using (SuppressEditorLoad())
                SetTheme(wysiwygHTML, previewHTML, containsTitle);

            _editingContext = editingContext;

            // @SharedCanvas - Check to make sure we can get rid of this once we get rid of the sidebar
            if (_editingContext is BlogPostEditingManager)
            {
                // Get an event everytime the user tried to publish, so we refesh smart content that might have been updated during published
                BlogPostEditingManager editingManager = (BlogPostEditingManager)_editingContext;
                editingManager.UserPublishedPost += new EventHandler(editingManager_UserPublishedPost);
            }

            // save whethere we are editing a page
            _isPage = editingContext.BlogPost.IsPage;

            // save a reference to the supporting files
            _supportingFileStorage = editingContext.SupportingFileStorage;

            //save a reference to the image data list
            _imageDataList = editingContext.ImageDataList;

            //save a reference to the extension data list
            _extensionDataList = editingContext.ExtensionDataList;

            // if there is already a RefreshableContentManager, which means that the user
            // is switching blogs or posts, then we need to dispose of it
            if (_refreshSmartContentManager != null)
                _refreshSmartContentManager.Dispose();

            // Make a new manager for the extension data for this blog/post
            _refreshSmartContentManager = new RefreshableContentManager(_extensionDataList, this);

            _fileService = editingContext.SupportingFileService;

            // Reset the emoticons manager
            _emoticonsManager = new EmoticonsManager(this, this);

            // initialize text-editing command manager
            // this needs to be called before html is loaded into the editor and before an editor gets
            // focus because that will cause the html commands to be updated
            InitializeTextEditingCommands();
            bool indent = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.TabAsIndent);
            CommandManager.Get(CommandId.Indent).On = indent;
            CommandManager.Get(CommandId.Outdent).On = indent;

            string postContentsHtml = SmartContentWorker.PerformOperation(editingContext.BlogPost.Contents, GetStructuredEditorHtml, true, this, true);
            postContentsHtml = StripPluginHeadersAndFooters(postContentsHtml);
            LoadEditorHtml(editingContext.BlogPost.Title, postContentsHtml, _currentEditorAccount.HomepageBaseUrl, false);

            // There are a lot of reasons that the post might become dirty, that are not from the users, these usually fall in
            // a very short time period. This is a catch all case, because there is no way to win against find all the different way.
            // Often times, it can be a css background loading, or mshtml changing around the html.
            TimerHelper.CallbackOnDelay(new InvokeInUIThreadDelegate(delegate () { _currentEditor.IsDirty = false; }), 50);

            // make sure the word count is in sync with the new post that has just been loaded
            wordCountUpdate(this, EventArgs.Empty);

            //update editing context of the editors
            _normalHtmlContentEditor.UpdateEditingContext();
            if (_codeHtmlContentEditor != null)
                _codeHtmlContentEditor.UpdateEditingContext();
        }

        public void SetTheme(string wysiwygHTML, string previewHTML, bool containsTitle)
        {
            if (string.IsNullOrEmpty(wysiwygHTML))
                throw new ArgumentException("wysiwygHTML cannot be a null or empty");

            Debug.Assert(wysiwygHTML.ToLower(CultureInfo.InvariantCulture).Contains("<head"), "wysiwygHTML does not contain head element");
            Debug.Assert(wysiwygHTML.ToLower(CultureInfo.InvariantCulture).Contains("<body"), "wysiwygHTML does not contain body element");

            BeforeSetTheme(ref wysiwygHTML, ref previewHTML, containsTitle);

            //transform the template HTML into the form required by the HTML editor
            string wysiwygTemplateWithBehaviors = CreateTemplateWithBehaviors(wysiwygHTML, _normalHtmlContentEditor.BehaviorManager);
            _editingTemplateWysiwyg = new BlogEditingTemplate(wysiwygTemplateWithBehaviors, containsTitle);

            if (!string.IsNullOrEmpty(previewHTML))
            {
                Debug.Assert(previewHTML.ToLower(CultureInfo.InvariantCulture).Contains("<head"), "previewHTML does not contain head element");
                Debug.Assert(previewHTML.ToLower(CultureInfo.InvariantCulture).Contains("<body"), "previewHTML does not contain body element");
                string previewTemplateWithBehaviors = CreateTemplateWithBehaviors(previewHTML, new NullElementBehaviorManager());
                _editingTemplatePreview = new BlogEditingTemplate(previewTemplateWithBehaviors, containsTitle);
            }

            ReloadEditor();
        }

        protected virtual void BeforeSetTheme(ref string wysiwygHTML, ref string previewHTML, bool containsTitle)
        {

        }

        private static Dictionary<string, string> _templateCache = new Dictionary<string, string>();
        private static object _templateCacheLock = new object();
        private string CreateTemplateWithBehaviors(string html, IElementBehaviorManager behaviorManager)
        {
            int htmlHashCode = StringHelper.GetHashCodeStable(html);
            int behaviorManagerHashCode = behaviorManager.GetHashCode();
            string key = string.Format(CultureInfo.InvariantCulture, "Html{0}Behaviors{1}", htmlHashCode, behaviorManagerHashCode);
            string templateWithBehaviors;
            bool found = false;

            lock (_templateCacheLock)
            {
                found = _templateCache.TryGetValue(key, out templateWithBehaviors);
            }

            if (found)
                return templateWithBehaviors;

            templateWithBehaviors = EditingTemplateLoader.CreateTemplateWithBehaviors(html, behaviorManager);
            lock (_templateCacheLock)
            {
                if (!_templateCache.ContainsKey(key))
                    _templateCache.Add(key, templateWithBehaviors);
            }

            return templateWithBehaviors;
        }

        public void SetBodyContents(string body)
        {
            LoadEditorHtml("", body, _currentEditorAccount.HomepageBaseUrl, true);
        }

        public void ChangeSelection(SelectionPosition position)
        {
            _currentEditor.ChangeSelection(position);
        }

        public string Publish(IPublishOperation imageConverter)
        {
            // @SharedCanvas - do we have to run the full publish operation here or is this enough?
            // @SharedCanvas - do we need to save, or does the caller do that?
            if (imageConverter != null)
            {
                SimplePublishOperation publish = new SimplePublishOperation(Body, imageConverter);
                return publish.PublishHtml();
            }

            return Body;
        }

        internal class SimplePublishOperation
        {
            private IPublishOperation _imageConverter;
            private string _html;
            public SimplePublishOperation(string html, IPublishOperation converter)
            {
                _imageConverter = converter;
                _html = html;
            }

            public string PublishHtml()
            {
                TextWriter htmlWriter = new StringWriter(CultureInfo.InvariantCulture);
                HtmlReferenceFixer fixer = new HtmlReferenceFixer(_html);
                fixer.FixReferences(htmlWriter, ReferenceFixerForPublish, null);
                htmlWriter.Flush();
                return htmlWriter.ToString();
            }

            public string ReferenceFixerForPublish(BeginTag tag, string reference)
            {
                if (_imageConverter != null)
                    return _imageConverter.GetUriForImage(reference);

                return reference;
            }
        }

        public event EventHandler TitleChanged;

        public string Title
        {
            get
            {
                return _currentEditor != null ? _currentEditor.GetEditedTitleHtml() : "";
            }
        }
        private void _currentEditor_TitleChanged(object sender, EventArgs e)
        {
            if (TitleChanged != null)
                TitleChanged(this, e);
        }

        public event EventHandler TitleFocusChanged;

        private void _currentEditor_EditableRegionFocusChanged(object sender, EventArgs e)
        {
            FixCommands((e as EditableRegionFocusChangedEventArgs).IsFullyEditable);

            Debug.Assert(_textEditingCommandDispatcher != null, "ContentEditor was not setup correctly, an editor got focus before command were created.");

            _textEditingCommandDispatcher.TitleFocusChanged();
            if (TitleFocusChanged != null)
                TitleFocusChanged(this, e);
        }

        private IHtmlEditorComponentContext ComponentContext()
        {
            return _currentEditor as IHtmlEditorComponentContext;
        }

        private void InitializeTextEditingCommands()
        {
            DisposeTextEditingCommandDispatcher();
            _textEditingCommandDispatcher = new TextEditingCommandDispatcher(_mainFrameWindow, _postEditingSite.StyleControl, CommandManager);
            _textEditingCommandDispatcher.RegisterPostEditor(this, ComponentContext, new TextEditingFocusHandler(Focus));

            if (_mainFrameWindow is Control)
                _textEditingCommandDispatcher.RegisterSimpleTextEditor(new ContainedTextBoxCommandSource((Control)_mainFrameWindow));
        }

        private void DisposeTextEditingCommandDispatcher()
        {
            if (_textEditingCommandDispatcher != null)
                _textEditingCommandDispatcher.Dispose();
            _textEditingCommandDispatcher = null;
        }

        public string Body
        {
            get
            {
                return GetBodyCore(_currentEditorAccount.EditorOptions.RequiresXHTML);
            }
        }

        private string GetBodyCore(bool preferWellFormed)
        {
            string html = _currentEditor != null ? _currentEditor.GetEditedHtml(preferWellFormed) : "";

            html = StripPluginHeadersAndFooters(html);

            return html;
        }

        private static string StripPluginHeadersAndFooters(string html)
        {
            // REVIEW: Only strip headers and footers of plugins that are installed/active?
            return SmartContentInsertionHelper.StripDivsWithClass(html, ContentSourceManager.HEADERS_FOOTERS);
        }

        public string BodyNoFixup
        {
            get
            {
                return _currentEditor != null ? _currentEditor.GetEditedHtmlFast() : "";
            }
        }

        public virtual void SaveChanges(BlogPost post, BlogPostSaveOptions options)
        {
            // get the title (remove linebreaks to prevent auto-convertion to P or BR)
            string postTitle = _currentEditor.GetEditedTitleHtml();
            post.Title = HtmlLinebreakStripper.RemoveLinebreaks(postTitle);

            // get the editor html
            // performance optimization for AutoSave
            string postContents = options.AutoSave ? GetBodyCore(false) : Body;

            //replace the structured content with the publish HTML.
            postContents = SmartContentWorker.PerformOperation(postContents, GetStructuredPublishHtml, false, this, true);

            // remove linebreaks to prevent auto-convertion to P or BR
            postContents = HtmlLinebreakStripper.RemoveLinebreaks(postContents);

            post.Contents = postContents;
            _currentEditor.IsDirty = false;
        }

        private void DisposeItemsOnEditorChange()
        {
            foreach (IDisposable d in _itemsToDisposeOnEditorClose)
            {
                try
                {
                    d.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.Fail("Failed to dispose item on editor: " + ex);
                }
            }

            _itemsToDisposeOnEditorClose.Clear();
        }

        internal void DisposeOnEditorChange(IDisposable disposable)
        {
            _itemsToDisposeOnEditorClose.Add(disposable);
        }

        public void OnClosed() { }
        public void OnPostClosed()
        {
            this.DisposeItemsOnEditorChange();

            // WinLive 105879: During the process of creating a new post, all the SmartContent present in the current
            // post was being be enumerated and refreshed unnecessarily. But at that point, those SmartContent held
            // stale references to the old SupportingFileService. Clearing the extension data list removes all the
            // SmartContent data, so that we no longer enumerate and refresh all the SmartContent when its unnecessary.
            _extensionDataList.Clear();
        }

        public void OnClosing(CancelEventArgs e)
        {

        }

        public void OnPostClosing(CancelEventArgs e)
        {
            if (!e.Cancel && _currentEditor != null)
                e.Cancel = !WaitOnSmartContentForOperation(SmartContentOperationType.Close);
        }

        public virtual bool ValidatePublish()
        {
            // Be sure that the title is not too long
            if (HtmlLinebreakStripper.RemoveLinebreaks(_currentEditor.GetEditedTitleHtml()).Length > _currentEditorAccount.EditorOptions.MaxPostTitleLength)
            {
                DisplayMessage.Show(MessageId.PostTitleTooLong);
                return false;
            }

            // make sure that all smart content that might be
            // waiting for something to happen, like a call back,
            // have been taken care of before we publish
            else if (!WaitOnSmartContentForOperation(SmartContentOperationType.Publish))
            {
                return false;
            }

            // all good!
            else
            {
                return true;
            }
        }

        private bool WaitOnSmartContentForOperation(SmartContentOperationType checkType)
        {
            VideoSmartContentWaitOperation operation = new VideoSmartContentWaitOperation(new BlogClientUIContextImpl(_mainFrameWindow, _mainFrameWindow), checkType, _extensionDataList.CalculateReferencedExtensionData(BodyNoFixup));

            // We want to check before we spend the time to show the dialog.
            if (operation.CheckVideos())
            {
                return true;
            }

            using (VideoPublishProgressForm dialog = new VideoPublishProgressForm())
            {
                operation.Start();
                dialog.ShowDialogWithDelay(_mainFrameWindow, operation, 1000);

                // Show a message if the user didn't cancel the dialog.
                if (!operation.IsSuccessful && !operation.DialogCancelled)
                {
                    DisplayMessage.Show(MessageId.VideoFailedPublish, _mainFrameWindow);
                }

                return operation.IsSuccessful;
            }
        }

        public virtual void OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {
        }

        void editingManager_UserPublishedPost(object sender, EventArgs e)
        {
            _htmlEditorSidebarHost.ForceUpdateSidebarState();
        }

        public virtual bool IsDirty
        {
            get
            {
                bool isDirty = (_currentEditor != null && _currentEditor.IsDirty);
                return isDirty;
            }
            set
            {
                if (_currentEditor != null)
                {
                    _currentEditor.IsDirty = value;
                }
            }
        }

        public event EventHandler Dirty;

        public void SetCurrentEditorDirty()
        {
            if (_currentEditor != null)
                _currentEditor.IsDirty = true;
        }

        private string CreateFileHandler(string requestedFileName)
        {
            return _supportingFileStorage.CreateFile(requestedFileName);
        }

        public virtual void OnEditorAccountChanged(IEditorAccount newEditorAccount)
        {

            // set the current weblog
            _currentEditorAccount = newEditorAccount;
            _normalHtmlContentEditor.TidyWhitespace = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.TidyWhitespace);
        }

        protected void ReloadEditor()
        {
            switch (CurrentEditingMode)
            {
                case EditingMode.Wysiwyg:
                    ChangeToWysiwygMode();
                    break;
                case EditingMode.Preview:
                    ChangeToPreviewMode();
                    break;
                case EditingMode.Source:
                    ChangeToCodeMode();
                    break;
                case EditingMode.PlainText:
                    ChangeToPlainTextMode();
                    break;
                default:
                    Debug.Fail("CurrentEditingMode is has invalid value: " + CurrentEditingMode);
                    break;
            }
        }

        public bool IsRTLTemplate
        {
            get
            {
                return _currentEditorAccount.EditorOptions.IsRTLTemplate;
            }
        }

        public bool HasRTLFeatures
        {
            get
            {
                return _currentEditorAccount.EditorOptions.HasRTLFeatures;
            }
        }

        private IEditorAccount _currentEditorAccount;

        private void LoadEditorHtml(string title, string html, string url, bool clearSelection)
        {
            if (_currentEditor == null || IsEditorLoadSuppressed)
                return;

            using (new WaitCursor())
            {
                // Clearing the selection will not be important if the whole document is replaced
                // or if this is the first time the document is loaded, in which case there will be
                // no selection.
                if (clearSelection)
                    _currentEditor.EmptySelection();

                // save dirty state
                bool isDirty = _currentEditor.IsDirty;

                if (CurrentEditingMode == EditingMode.Preview)
                    UpdateHtmlForPreview(ref html);
                else if (CurrentEditingMode == EditingMode.PlainText)
                {
                    UpdateHtmlForPlainText(ref html, false);
                }

                _currentEditor.LoadHtmlFragment(title, html, url, GetSurroundingContent());

                // restore dirty state
                _currentEditor.IsDirty = isDirty;
            }
        }

        public DefaultBlockElement DefaultBlockElement
        {
            get
            {
                Debug.Assert(_normalHtmlContentEditor != null, "BlogPostHtmlEditorControl isn't initialized yet!");
                return _normalHtmlContentEditor.DefaultBlockElement;
            }
        }

        private void UpdateHtmlForPlainText(ref string html, bool allowLeadingNewline)
        {
            // WinLive 128121: We're turning <div>blah</div> into /r/nblah/r/n  when going into plain text mode.
            // We do NOT want the leading /r/n in this case.
            html = HtmlUtils.HTMLToPlainTextNoTrim(html, false);

            if (!allowLeadingNewline && html.StartsWith("\r\n"))
                html = html.Substring(2);

            html = TextHelper.GetHTMLFromText(html, false, this.DefaultBlockElement);
        }

        public IDisposable SuppressEditorLoad()
        {
            return new ContentEditorLoadSuppresser(this);
        }

        public class ContentEditorLoadSuppresser : IDisposable
        {
            private ContentEditor _editor;
            public ContentEditorLoadSuppresser(ContentEditor editor)
            {
                _editor = editor;
                _editor.AddLoadSuppressCount();

            }

            public void Dispose()
            {
                _editor.RemoveLoadSuppressCount();
            }
        }

        public event EventHandler EditorLoaded;
        private void FireEditorLoaded()
        {
            if (CurrentEditingMode != EditingMode.Preview)
                LastNonPreviewEditingMode = CurrentEditingMode;

            if (EditorLoaded != null)
                EditorLoaded(this, EventArgs.Empty);
        }

        private void UpdateHtmlForPreview(ref string html)
        {
            foreach (ContentSourceInfo csi in ContentSourceManager.GetActiveHeaderFooterPlugins(_mainFrameWindow, _currentEditorAccount.Id))
            {
                ISmartContent smartContent = GetSmartContentForPublishHook(csi);

                // TODO: Check for unbalanced HTML

                HeaderFooterSource plugin = (HeaderFooterSource)csi.Instance;
                HeaderFooterSource.Position position;
                string generatedHtml;
                try
                {
                    generatedHtml = plugin.GeneratePreviewHtml(smartContent, this, out position);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    DisplayableException ex = new DisplayableException(
                        Res.Get(StringId.UnexpectedErrorPluginTitle),
                        string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.UnexpectedErrorPluginDescription), csi.Name, e.Message));
                    DisplayableExceptionDisplayForm.Show(_mainFrameWindow, ex);
                    continue;
                }

                if (string.IsNullOrEmpty(generatedHtml))
                    continue;

                if (SmartContentInsertionHelper.ContainsUnbalancedDivs(generatedHtml))
                {
                    Trace.Fail("Unbalanced divs detected in HTML generated by " + csi.Name + ": " + html);
                    DisplayMessage.Show(MessageId.MalformedHtmlIgnored, _mainFrameWindow, csi.Name);
                    continue;
                }

                // Don't use float alignment for footers--it causes them to float around
                // stuff that isn't part of the post
                bool noFloat = position == HeaderFooterSource.Position.Footer;
                generatedHtml = SmartContentInsertionHelper.GenerateBlock(ContentSourceManager.HEADERS_FOOTERS, null, smartContent, false, generatedHtml, noFloat, null);
                if (position == HeaderFooterSource.Position.Header)
                    html = generatedHtml + html;
                else if (position == HeaderFooterSource.Position.Footer)
                    html = html + generatedHtml;
                else
                    Debug.Fail("Unknown position: " + position);
            }
        }

        private ISmartContent GetSmartContentForPublishHook(ContentSourceInfo csi)
        {
            // Use the plugin ID as content ID because there can only be
            // one per post anyway

            Debug.Assert(typeof(HeaderFooterSource).IsAssignableFrom(csi.Type));

            string contentId = csi.Id;
            ISmartContent smartContent =
                ((IContentSourceSidebarContext)this).FindSmartContent(contentId);
            if (smartContent == null)
                smartContent = new SmartContent(((IContentSourceSite)this).CreateExtensionData(contentId));
            return smartContent;
        }

        public string SelectedText
        {
            get
            {
                return _currentEditor.SelectedText;
            }
        }

        public string SelectedHtml
        {
            get
            {
                return _currentEditor.SelectedHtml;
            }
        }

        public void InsertHtml(string content, bool moveSelectionRight)
        {
            InsertHtml(content, moveSelectionRight ? HtmlInsertionOptions.MoveCursorAfter : HtmlInsertionOptions.Default);
        }

        public void InsertHtml(string content, HtmlInsertionOptions options)
        {
            string fixedUpHtml = content;

            if (!((options & HtmlInsertionOptions.ExternalContent) == HtmlInsertionOptions.ExternalContent))
                fixedUpHtml = FixUpLocalFileReferenceForInsertion(fixedUpHtml);

            if ((options & HtmlInsertionOptions.PlainText) == HtmlInsertionOptions.PlainText)
            {
                fixedUpHtml = TextHelper.GetHTMLFromText(fixedUpHtml, true, this.DefaultBlockElement);
            }

            // We dont want to apply both the default font tag and 'reset' style to the same block of html
            // We also dont want to apply the reset style if there is nothing to reset.
            if ((options & HtmlInsertionOptions.ApplyDefaultFont) == HtmlInsertionOptions.ApplyDefaultFont)
                fixedUpHtml = _normalHtmlContentEditor.CurrentDefaultFont.ApplyFont(fixedUpHtml);
            else if (((options & HtmlInsertionOptions.ExternalContent) == HtmlInsertionOptions.ExternalContent) && CurrentEditingMode != EditingMode.PlainText && !string.IsNullOrEmpty(fixedUpHtml))
                fixedUpHtml =
                    "<div style='color:#000000;font-size:small;font-family:\"Calibri\";font-weight:normal;display:inline;font-style:normal;text-decoration:none'>" +
                    fixedUpHtml + "</div>";

            if ((options & HtmlInsertionOptions.Indent) == HtmlInsertionOptions.Indent)
                fixedUpHtml = ApplyIndent(fixedUpHtml);

            if ((options & HtmlInsertionOptions.InsertNewLineBefore) == HtmlInsertionOptions.InsertNewLineBefore)
            {
                if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.DivNewLine))
                    fixedUpHtml = "<div>&nbsp;</div>" + fixedUpHtml;
                else
                    fixedUpHtml = "<br/>" + fixedUpHtml;
            }

            if (CurrentEditingMode == EditingMode.PlainText)
                UpdateHtmlForPlainText(ref fixedUpHtml, true);

            _currentEditor.InsertHtml(fixedUpHtml, options);
        }

        public MshtmlFontWrapper CurrentDefaultFont
        {
            get
            {
                return _normalHtmlContentEditor.CurrentDefaultFont;
            }
        }

        private string ApplyIndent(string fixedUpHtml)
        {
            string color = "000000";
            if (!string.IsNullOrEmpty(IndentColor))
                color = IndentColor;

            string direction = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.RTLDirectionDefault)
                                   ? "right"
                                   : "left";
            return string.Format(CultureInfo.InvariantCulture, "<div style='border-{2}:solid ; border-{2}-width: 4; border-color: #{1}; padding-{2}: 5; margin-{2}: 5'>{0}</div>", fixedUpHtml, color, direction);
        }

        public string IndentColor { get; set; }

        private string LocalFileReferenceFixer(BeginTag tag, string reference)
        {
            Uri referenceUri = new Uri(reference);

            string localPath = referenceUri.LocalPath;
            if (!File.Exists(localPath) || Directory.Exists(localPath) || referenceUri.IsUnc)
                return reference;

            try
            {
                string referenceBaseDir = Path.GetDirectoryName(referenceUri.LocalPath);
                if (new DirectoryInfo(referenceBaseDir).FullName.Equals(new DirectoryInfo(_supportingFileStorage.StoragePath).FullName))
                {
                    //then this file is already in the supporting file storage, so don't fix it!
                    //This fixes a bug where moving internal HTML causes linked items to get recreated in storage
                    //(which hoses up image meta data since the image URL gets changed)
                    return reference;
                }
                else
                {
                    using (FileStream referenceStream = new FileStream(referenceUri.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        ISupportingFile supportingFile = _fileService.CreateSupportingFile(Path.GetFileName(referenceUri.LocalPath), referenceStream);
                        return UrlHelper.SafeToAbsoluteUri(supportingFile.FileUri);
                    }
                }
            }
            catch (IOException e)
            {
                Trace.Fail("Error importing local file reference: " + reference, e.ToString());
                return reference;
            }
        }

        private string FixUpLocalFileReferenceForInsertion(string html)
        {
            string fixedUpHtml = HtmlReferenceFixer.FixLocalFileReferences(html, new ReferenceFixer(LocalFileReferenceFixer));
            return fixedUpHtml;
        }

        public bool ShouldComposeHostHandlePhotos()
        {
            return false;
        }

        public void InsertImages(string[] files, ImageInsertEntryPoint entryPoint)
        {
            ImageInsertionManager.InsertImagesCore(_currentEditor, _fileService, _currentEditorAccount, this, files);
        }

        public void InsertSmartContentFromFile(string[] files, string contentSourceID, HtmlInsertionOptions insertionOptions, object context)
        {
            files = RemoveInvalidImages(files);

            if (files == null || files.Length == 0)
                return;

            // Get a reference to the video plugin
            ISupportsDragDropFile contentSource =
                (ISupportsDragDropFile)ContentSourceManager.GetContentSourceInfoById(contentSourceID).Instance;

            if (contentSource == null)
                return;

            IExtensionData extensionData = (this as IContentSourceSite).CreateExtensionData(Guid.NewGuid().ToString());
            ISmartContent sContent = new SmartContent(extensionData);

            if (context == null)
                context = new InternalSmartContentContext(this, contentSourceID);

            using (EditorUndoUnit undo = new EditorUndoUnit(_currentEditor))
            {
                // Have the plugin try to make some new content from the path that we just got through the drag and drop
                if (contentSource.CreateContentFromFile(_mainFrameWindow, sContent, files, context) == DialogResult.OK)
                {
                    string content = ((SmartContentSource)contentSource).GenerateEditorHtml(sContent, this);
                    if (content != null)
                    {
                        ((IContentSourceSite)this).InsertContent(contentSourceID, content, extensionData, insertionOptions);
                        undo.Commit();
                    }
                    Focus();

                }

            }
        }

        private string[] RemoveInvalidImages(string[] files)
        {
            if (files == null)
                return null;

            List<string> images = new List<string>();
            foreach (string file in files)
            {
                try
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        using (Image image = Image.FromStream(fs, false, false))
                        {
                            if (image.Width > 0 && image.Height > 0)
                                images.Add(file);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            return images.ToArray();
        }

        public void InsertSmartContentFromTabbedDialog(string contentSourceID, int selectedTab)
        {
            // Get a reference to the video plugin
            ITabbedInsertDialogContentSource contentSource =
                (ITabbedInsertDialogContentSource)ContentSourceManager.GetContentSourceInfoById(contentSourceID).Instance;

            if (contentSource == null)
                return;

            IExtensionData extensionData = (this as IContentSourceSite).CreateExtensionData(Guid.NewGuid().ToString());
            ISmartContent sContent = new InternalSmartContent(extensionData, this, contentSourceID);
            using (EditorUndoUnit undo = new EditorUndoUnit(_currentEditor))
            {
                if (contentSource.CreateContentFromTabbedDialog(_mainFrameWindow, sContent, selectedTab) == DialogResult.OK)
                {
                    string content = ((SmartContentSource)contentSource).GenerateEditorHtml(sContent, this);
                    if (content != null)
                    {
                        ((IContentSourceSite)this).InsertContent(contentSourceID, content, extensionData);
                        undo.Commit();
                    }
                    Focus();

                }

            }
        }

        public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow)
        {
            _currentEditor.InsertLink(url, linkText, linkTitle, rel, newWindow);
        }

        public void Find()
        {
            // The squiggles placed by real time spell checking interfere with the highlighting applied by MSHTML's
            // Find dialog, so we remove all the squiggles for now.
            _normalHtmlContentEditor.SuspendSpellChecking();

            _currentEditor.CommandSource.Find();

            _normalHtmlContentEditor.ResumeSpellChecking();
        }

        public void Print()
        {
            _currentEditor.CommandSource.Print();
        }

        public void PrintPreview()
        {
            _currentEditor.CommandSource.PrintPreview();
        }

        public bool CheckSpelling()
        {
            // check spelling
            return _currentEditor.CommandSource.CheckSpelling();
        }

        public void Focus()
        {
            _currentEditor.Focus();
        }

        public void FocusTitle()
        {
            _currentEditor.FocusTitle();
        }

        public void FocusBody()
        {
            _currentEditor.FocusBody();
        }

        public bool DocumentHasFocus()
        {
            return _currentEditor.DocumentHasFocus();
        }

        public virtual IFocusableControl[] GetFocusablePanes()
        {
            return new IFocusableControl[]
            {
                EditorFocusControl,
                new FocusableControl(_htmlEditorSidebarHost)
            };
        }

        public IFocusableControl EditorFocusControl
        {
            get
            {
                return new EditorFocusControlProxy(this);
            }
        }

        private class EditorFocusControlProxy : FocusableControlProxy
        {
            private ContentEditor _editor;
            public EditorFocusControlProxy(ContentEditor editor)
            {
                _editor = editor;
            }

            public override IFocusableControl FocusableControlTarget
            {
                get
                {
                    if (_editor._currentEditor != null)
                        return _editor._currentEditor.FocusControl;
                    else
                        return NullFocusableControl.Instance;
                }
            }
        }

        private void SetCurrentEditor()
        {
            IBlogPostHtmlEditor contentEditor = null;

            switch (CurrentEditingMode)
            {
                case EditingMode.Preview:
                case EditingMode.Wysiwyg:
                case EditingMode.PlainText:
                    contentEditor = _normalHtmlContentEditor;
                    break;
                case EditingMode.Source:
                    Debug.Assert(_codeHtmlContentEditor != null, "Changed to source editor without creating one");
                    contentEditor = _codeHtmlContentEditor;
                    break;
            }

            if (contentEditor == null)
                return;

            // state to transfer from editor to editor (default if no existing editor)
            string htmlTitle = String.Empty;
            string htmlContents = String.Empty;
            bool isDirty = false;
            IHtmlEditorComponentContext componentContext = null;
            if (_currentEditor != null)
            {
                //unregisiter from editing events
                _currentEditor.TitleChanged -= new EventHandler(_currentEditor_TitleChanged);
                _currentEditor.EditableRegionFocusChanged -= new EventHandler(_currentEditor_EditableRegionFocusChanged);

                // clear selection (will eliminate editing palettes, etc tied to selection)
                _currentEditor.EmptySelection();

                componentContext = _currentEditor as IHtmlEditorComponentContext;
                if (componentContext != null)
                {
                    componentContext.SelectionChanged -= new EventHandler(ContentEditor_SelectionChanged);
                    componentContext.BeforeInitialInsertion += new EventHandler(ContentEditor_BeforeInitialInsertion);
                    componentContext.AfterInitialInsertion += new EventHandler(ContentEditor_AfterInitialInsertion);
                }

                htmlTitle = _currentEditor.GetEditedTitleHtml();
                htmlContents = Body;

                //refresh smart content HTML (deal with the case where user edits the content directly in the source editor)
                htmlContents = SmartContentWorker.PerformOperation(htmlContents, GetStructuredEditorHtml, true, this, true);
                isDirty = _currentEditor.IsDirty;
                _currentEditor.EditorControl.Visible = false;
            }

            // set current editor
            _currentEditor = contentEditor;

            //register for editing events
            _currentEditor.TitleChanged += new EventHandler(_currentEditor_TitleChanged);
            _currentEditor.EditableRegionFocusChanged += new EventHandler(_currentEditor_EditableRegionFocusChanged);
            componentContext = _currentEditor as IHtmlEditorComponentContext;
            if (componentContext != null)
            {
                componentContext.SelectionChanged += new EventHandler(ContentEditor_SelectionChanged);
                componentContext.BeforeInitialInsertion += new EventHandler(ContentEditor_BeforeInitialInsertion);
                componentContext.AfterInitialInsertion += new EventHandler(ContentEditor_AfterInitialInsertion);
            }

            //make the new current editor visible
            _currentEditor.EditorControl.Visible = true;
            _currentEditor.EditorControl.BringToFront();

            LoadEditorHtml(htmlTitle, htmlContents, _currentEditorAccount.HomepageBaseUrl, true);

            _currentEditor.IsDirty = isDirty;

            // set the focus in the editor
            _currentEditor.Focus();
        }

        private int _makingInitialInsertion = 0;
        private void ContentEditor_AfterInitialInsertion(object sender, EventArgs e)
        {
            Debug.Assert(_makingInitialInsertion > 0, "Mismatched initial insertion notifications.");
            _makingInitialInsertion--;
        }

        private void ContentEditor_BeforeInitialInsertion(object sender, EventArgs e)
        {
            _makingInitialInsertion++;
        }

        public bool IsEditFieldSelected
        {
            get
            {
                IHtmlEditorComponentContext editorContext = _currentEditor as IHtmlEditorComponentContext;

                if (editorContext != null && editorContext.Selection != null)
                {
                    return editorContext.IsEditFieldSelected;
                }

                return false;
            }
        }

        public bool IsSmartContentSelected
        {
            get
            {
                IHtmlEditorComponentContext editorContext = _currentEditor as IHtmlEditorComponentContext;
                return editorContext != null && editorContext.Selection is SmartContentSelection;
            }
        }

        protected bool EditFieldNotSelected
        {
            get
            {
                IHtmlEditorComponentContext editorContext = _currentEditor as IHtmlEditorComponentContext;
                if (editorContext != null && editorContext.Selection != null)
                {
                    return !editorContext.IsEditFieldSelected;
                }

                return true;
            }
        }

        protected bool InSourceOrWysiwygModeAndEditFieldIsNotSelected()
        {
            return (CurrentEditingMode == EditingMode.Source || CurrentEditingMode == EditingMode.Wysiwyg) && EditFieldNotSelected;
        }

        protected virtual void ContentEditor_SelectionChanged(object sender, EventArgs e)
        {
            // Update the set of commands available based on edit field selection
            bool editFieldNotSelected = EditFieldNotSelected;

            if (CurrentEditingMode == EditingMode.Wysiwyg)
            {

                if (editFieldNotSelected)
                {
                    _normalHtmlContentEditor.EnableDefaultFont();
                    SetUrlAutoDetectEnabled(true);
                }
                else
                {
                    _normalHtmlContentEditor.DisableDefaultFont();
                    SetUrlAutoDetectEnabled(false);
                }
            }

            bool inSourceOrWysiwygModeAndEditFieldNotSelected = InSourceOrWysiwygModeAndEditFieldIsNotSelected();

            commandInsertPicture.Enabled = inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertEmoticon.Enabled = inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertTable.Enabled = inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertMap.Enabled = _mapsFeatureEnabled && inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertTable.Enabled = inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertTags.Enabled = _tagProvidersFeatureEnabled && inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertVideoFromFile.Enabled = _videoProvidersFeatureEnabled && inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertVideoFromService.Enabled = _videoProvidersFeatureEnabled && inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertVideoFromWeb.Enabled = _videoProvidersFeatureEnabled && inSourceOrWysiwygModeAndEditFieldNotSelected;
            commandInsertHorizontalLine.Enabled = editFieldNotSelected;
            commandInsertClearBreak.Enabled = inSourceOrWysiwygModeAndEditFieldNotSelected;

            BlogPostEditingManager editingManager = _editingContext as BlogPostEditingManager;
            bool supportsExtendedEntries = editingManager != null ? editingManager.Blog.ClientOptions.SupportsExtendedEntries : false;
            CommandManager.SetEnabled(CommandId.InsertExtendedEntry, inSourceOrWysiwygModeAndEditFieldNotSelected && supportsExtendedEntries && !_editingContext.BlogPost.IsPage);

            UpdateContextAvailability();
        }

        void editor_IsDirtyEvent(object sender, EventArgs e)
        {
            if (WordCountSettings.EnableRealTimeWordCount)
            {
                wordCountTimer.Stop();
                wordCountTimer.Start();
            }
            if (Dirty != null)
                Dirty(this, EventArgs.Empty);
        }

        void wordCountUpdate(object sender, EventArgs e)
        {
            wordCountTimer.Stop();

            if (!WordCountSettings.EnableRealTimeWordCount)
            {
                StatusBar.SetWordCountMessage(null);
                return;
            }

            try
            {
                using (ApplicationPerformance.LogEvent("RealTimeWordCount"))
                {
                    WordCounter wordCounter = new WordCounter(_currentEditor.GetEditedHtmlFast());
                    StatusBar.SetWordCountMessage(
                        string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.Words), wordCounter.Words));
                }
            }
            catch (Exception ex)
            {
                WordCountSettings.EnableRealTimeWordCount = false;
                Trace.Fail("Unexpected error while trying to update word count.  Real time word count is disabled.  Error: " + ex.ToString());
            }

        }

        private void DrawMockPlayer(string message, bool error)
        {
            using (Bitmap img = ResourceHelper.LoadAssemblyResourceBitmap("Video.Images.MockPlayer.png").Clone() as Bitmap)
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    BidiGraphics bidiGraphics = new BidiGraphics(g, img.Size);
                    if (error)
                        VideoSmartContent.DrawErrorMockPlayer(img, bidiGraphics, message);
                    else
                        VideoSmartContent.DrawStatusMockPlayer(img, bidiGraphics, message);
                }
                string path = TempFileManager.Instance.CreateTempFile(Guid.NewGuid().ToString() + ".png");
                img.Save(path, ImageFormat.Png); ;
                InsertImages(new string[] { path }, ImageInsertEntryPoint.MockVideo);
            }
        }

        void commandInsertClearBreak_Execute(object sender, EventArgs e)
        {
            _currentEditor.InsertClearBreak();
        }

        void commandInsertHorizontalLine_Execute(object sender, EventArgs e)
        {
            _currentEditor.InsertHorizontalLine(_currentEditingMode == EditingMode.PlainText);
        }

        private void commandShowImageUploadError_Execute(object sender, EventArgs e)
        {
            FileUploadFailedForm.Show(_mainFrameWindow, new string[0]);
        }

        private void commandViewSource_Execute(object sender, EventArgs e)
        {
            ViewSource();
        }

        private void commandShowVideoErrorMessage_Execute(object sender, EventArgs e)
        {
            DrawMockPlayer(Res.Get(StringId.VideoNetworkError) + Environment.NewLine + Res.Get(StringId.VideoErrorTryAgain), true);
            DrawMockPlayer(Res.Get(StringId.YouTubeVideoError) + Environment.NewLine + Res.Get(StringId.VideoErrorTryAgain), true);
            DrawMockPlayer(Res.Get(StringId.VideoErrorTryAgain), true);
            DrawMockPlayer(Res.Get(StringId.VideoLoading), false);
            DrawMockPlayer(Res.Get(StringId.VideoLocalProcessing), false);
            DrawMockPlayer(Res.Get(StringId.VideoRemoteProcessing), false);
            DrawMockPlayer(Res.Get("YouTubeInvalidResult"), true);
            DrawMockPlayer(Res.Get("YouTubecopyright"), true);
            DrawMockPlayer(Res.Get("YouTubeinappropriate"), true);
            DrawMockPlayer(Res.Get("YouTubeduplicate"), true);
            DrawMockPlayer(Res.Get("YouTubetermsOfUse"), true);
            DrawMockPlayer(Res.Get("YouTubesuspended"), true);
            DrawMockPlayer(Res.Get("YouTubetooLong"), true);
            DrawMockPlayer(Res.Get("YouTubeblocked"), true);
            DrawMockPlayer(Res.Get("YouTubecantProcess"), true);
            DrawMockPlayer(Res.Get("YouTubeinvalidFormat"), true);
            DrawMockPlayer(Res.Get("YouTubeunsupportedCodec"), true);
            DrawMockPlayer(Res.Get("YouTubeempty"), true);
            DrawMockPlayer(Res.Get("YouTubetooSmall"), true);

            DisplayMessage.Show(MessageId.BloggerError, Res.Get(StringId.YouTubeNoAccount));
        }

        private void WordCount_Execute(object sender, EventArgs e)
        {
            string text = "";
            bool bOnlySelectedText = false;
            if (String.IsNullOrEmpty(_currentEditor.SelectedText))
            {
                bOnlySelectedText = false;
                text = _currentEditor.GetEditedHtmlFast();
            }
            else
            {
                bOnlySelectedText = true;
                text = _currentEditor.SelectedHtml;
            }

            WordCountForm wordCountForm = new WordCountForm(text, bOnlySelectedText);
            wordCountForm.ShowDialog(this._mainFrameWindow);
        }

        private void commandViewPlainText_Execute(object sender, EventArgs e)
        {
            if (_currentEditingMode != EditingMode.PlainText)
                ChangeToPlainTextMode();
        }

        private void commandViewNormal_Execute(object sender, EventArgs e)
        {
            if (_currentEditingMode != EditingMode.Wysiwyg)
                ChangeToWysiwygMode();
        }

        private void commandViewWebPreview_Execute(object sender, EventArgs e)
        {
            if (_currentEditingMode != EditingMode.Preview)
                ChangeToPreviewMode();
        }

        void ManageHtmlEditingCommands()
        {
            if (_textEditingCommandDispatcher != null)
                _textEditingCommandDispatcher.ManageCommands();
        }

        private void commandViewCode_Execute(object sender, EventArgs e)
        {
            if (_currentEditingMode != EditingMode.Source)
                ChangeToCodeMode();
        }

        private EditingMode _currentEditingMode = EditingMode.Wysiwyg;
        public EditingMode CurrentEditingMode
        {
            get { return _currentEditingMode; }
            set
            {
                if (_currentEditingMode != value)
                {
                    _currentEditingMode = value;
                    ReloadEditor();
                }
            }
        }

        public void ChangeToCodeMode()
        {
            Trace.Assert(_codeHtmlContentEditor != null, "Changed to source editor without creating one");
            ChangeEditor(EditingMode.Source, false);
            _codeHtmlContentEditor.Focus();
        }

        public void ChangeToPreviewMode()
        {
            _htmlEditorSidebarHost.Visible = false;
            ChangeEditor(EditingMode.Preview, false);
        }

        private string _lastFontString;
        public void ChangeToPlainTextMode()
        {
            ChangeEditor(EditingMode.PlainText, true);
        }

        private void ChangeEditor(EditingMode mode, bool editMode)
        {
            // Clean up any items that are associated with this editing mode.
            DisposeItemsOnEditorChange();

            bool isDirty = _currentEditor != null && _currentEditor.IsDirty;

            // Change the editing mode
            _currentEditingMode = mode;

            // Disable or enable any of commands on the ribbon
            ManageCommandsForEditingMode();

            // Lets the user edit the document
            _normalHtmlContentEditor.SetEditable(editMode);
            SetCurrentEditor();

            // Set the orginal dirty state back
            _currentEditor.IsDirty = isDirty;

            // Let everyone the editor just changed (tabs will update)
            FireEditorLoaded();
        }

        public void ChangeToWysiwygMode()
        {
            ChangeEditor(EditingMode.Wysiwyg, true);
        }

        private void SetUrlAutoDetectEnabled(bool enabled)
        {
            if (!_mshtmlOptions.EditingOptions[IDM.AUTOURLDETECT_MODE].Equals(enabled))
            {
                _mshtmlOptions.EditingOptions[IDM.AUTOURLDETECT_MODE] = enabled;
                _normalHtmlContentEditor.UpdateOptions(_mshtmlOptions, false);
            }
        }

        private void commandViewSidebar_BeforeShowInMenu(object sender, EventArgs e)
        {
            commandViewSidebar.Latched = _htmlEditorSidebarHost.Visible;
        }

        private void commandViewSidebar_Execute(object sender, EventArgs e)
        {
            _htmlEditorSidebarHost.Visible = !_htmlEditorSidebarHost.Visible;
        }

        private void commandViewImageProperties_Execute(object sender, EventArgs e)
        {
            _htmlEditorSidebarHost.Visible = !_htmlEditorSidebarHost.Visible;
        }

        private void htmlEditor_DocumentComplete(object source, EventArgs evt)
        {
            //when the document is loaded, scan it for any uninitialized images that need to be loaded
            //This lets images that are added via "blog this" or the source editor automatically have the
            //default settings applied
            ImageInsertionManager.ScanAndInitializeNewImages(_currentEditor, _fileService, _currentEditorAccount, this, _editorContainer, true, false);
            // @SharedCanvas - does this need to change to "email" for WLM?
            // set editing target name
            _normalHtmlContentEditor.EditingTargetName = _isPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower);

            // WinLive 182722: Setting the default font may attempt to update the body of the HTML document, so make
            // sure we do it on DocumentComplete.
            if (CurrentEditingMode == EditingMode.PlainText)
            {
                if (_mshtmlOptions.EditingOptions.Contains(IDM.COMPOSESETTINGS))
                {
                    _lastFontString = _mshtmlOptions.EditingOptions[IDM.COMPOSESETTINGS].ToString();
                    SetDefaultFont(_lastFontString);
                }
                SetUrlAutoDetectEnabled(false);
            }
            else if (CurrentEditingMode == EditingMode.Wysiwyg)
            {
                if (_lastFontString != null)
                    SetDefaultFont(_lastFontString);
                SetUrlAutoDetectEnabled(true);
            }

            // WinLive 222385 - When switching between plain-text/html, mshtml doesn't report IME
            // info to IMM when queried because the document is not fully loaded. This causes IMM to deactivate IME
            // for canvas. To work around this, explicitly setting focus on body here causes mshtml to
            // report the right IME state and thus activates IMM for canvas.
            // Note: In case of mail, this path is executed before mail's CNote::OnDocumentReady is run, which
            // reset the focus to 'To' line for a new compose note.
            _currentEditor.FocusBody();

            // Invalidate emoticons, this causes the gallery to be loaded if it is not loaded already
            // Delay this via message queue so we don't block this code path while loading.
            if (_mainFrameWindow != null)
                _mainFrameWindow.BeginInvoke(new ThreadStart(commandInsertEmoticon.Invalidate), null);

            FireDocumentComplete();
        }

        public event EventHandler DocumentComplete;
        public void FireDocumentComplete()
        {
            if (DocumentComplete != null)
                DocumentComplete(this, EventArgs.Empty);
        }

        public event EventHandler GotFocus;
        private void htmlEditor_GotFocus(object source, EventArgs evt)
        {
            if (GotFocus != null)
                GotFocus(source, evt);
        }

        public event EventHandler LostFocus;
        private void htmlEditor_LostFocus(object source, EventArgs evt)
        {
            if (LostFocus != null)
                LostFocus(source, evt);
        }

        private OpenFileDialog _insertImageDialogWin7 = null;

        public IETWProvider ETWProvider
        {
            get
            {
                return _mainFrameWindow as IETWProvider;
            }
        }

        private void commandInsertPicture_Execute(object sender, EventArgs e)
        {
            // WinLive 222100: Writer crash: Access violation
            // It appears that we are somehow inserting a picture after the editor has been disposed.
            if (_disposed)
            {
                Trace.Fail("Inserting a picture after the editor has been disposed!");
                return;
            }

            using (new WaitCursor())
            {
                string[] imageFiles = null;
                if (_insertImageDialogWin7 == null)
                {
                    _insertImageDialogWin7 = new OpenFileDialog();
                    _insertImageDialogWin7.Title = Res.Get(StringId.InsertPicture);
                    _insertImageDialogWin7.Multiselect = true;
                    // Bug 769860 - Unexpected folder navigation with photo libraries on Win7
                    //dialog.InitialDirectory = ApplicationEnvironment.InsertImageDirectory;
                    // So instead we just set the user to the my pictures folder.
                    _insertImageDialogWin7.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    _insertImageDialogWin7.Filter = String.Format(CultureInfo.InvariantCulture, "{0}|*.gif;*.jpg;*.jpeg;*.png|{1}|*.*", Res.Get(StringId.ImagesFilterString), Res.Get(StringId.AllFilesFilterString));
                }

                if (DialogResult.OK == _insertImageDialogWin7.ShowDialog(_mainFrameWindow))
                {
                    if (ETWProvider != null)
                        ETWProvider.WriteEvent("InlinePhotoStart");

                    imageFiles = _insertImageDialogWin7.FileNames;
                    _insertImageDialogWin7.InitialDirectory = null;
                }

                // Check to see if the user actually selected anything
                if (imageFiles != null && imageFiles.Length > 0)
                {
                    ApplicationEnvironment.InsertImageDirectory = Path.GetDirectoryName(imageFiles[0]);
                    InsertImages(imageFiles, ImageInsertEntryPoint.Inline);
                }
            }
        }

        private void _normalHtmlContentEditor_SelectedImageResized(Size newImgElementSize, Size originalImgElementSize, bool preserveRatio, IHTMLImgElement image)
        {
            Size newBorderlessImageSize;
            ImagePropertiesInfo imageInfo = ImageEditingPropertyHandler.GetImagePropertiesInfo(image, this); ;

            //Note: In this resize context, the user resized the image in the editor to an explicit size.
            //The user expectation in this scenario is that the entire image (including the border) should fit
            //inside the new size.  To meet this expectation, we need to reduce the size of the image by the
            //border size so that the updated image (including borders) will fit inside of the size the user resized to.
            ImageBorderMargin borderMargin = imageInfo.InlineImageBorderMargin;
            if (preserveRatio)
            {
                //when preserving the image's aspect ratio during resize, don't include the border margins in the scaled
                //size calculation since this will distort the image.
                Size originalBorderlessSize = new Size(originalImgElementSize.Width - borderMargin.Width, originalImgElementSize.Height - borderMargin.Height);
                Size borderlessNewMaxSize = borderMargin.ReverseCalculateImageSize(newImgElementSize);

                newBorderlessImageSize = ImageUtils.GetScaledImageSize(borderlessNewMaxSize.Width, borderlessNewMaxSize.Height, originalBorderlessSize);
            }
            else
            {
                newBorderlessImageSize = borderMargin.ReverseCalculateImageSize(newImgElementSize);
            }

            PictureEditingManager.UpdateInlineImageSize(newBorderlessImageSize, ImageDecoratorInvocationSource.Resize, (HtmlEditorControl)_currentEditor);
        }
        private void _normalHtmlContentEditor_UpdateImageLink(string newLink, string title, bool newWindow, string rel)
        {
            PictureEditingManager.UpdateImageLink(newLink, title, newWindow, rel, ImageDecoratorInvocationSource.Unknown);
        }

        private void _normalHtmlContentEditor_HtmlInserted(object sender, EventArgs e)
        {
            ImageInsertionManager.ScanAndInitializeNewImages(_currentEditor, _fileService, _currentEditorAccount, this, _editorContainer, true, true);
        }

        private void commandInsertTable_Execute(object sender, EventArgs e)
        {
            //Insert gestures are only supported in the body, so force focus to the body
            FocusBody();

            using (TablePropertiesForm tableForm = new TablePropertiesForm())
            {
                // show the dialog
                TableCreationParameters tableCreationParameters = tableForm.CreateTable(_mainFrameWindow);

                // insert
                if (tableCreationParameters != null)
                {
                    using (new WaitCursor())
                    {
                        // fixup bizzaro table selections
                        IHtmlEditorComponentContext editorContext = _currentEditor as IHtmlEditorComponentContext;
                        if (editorContext != null)
                        {
                            // check for a discontigous selection of cells within an existing table and
                            // "fix" the selection accordingly so the editor doesn't barf on it
                            TableSelection tableSelection = new TableSelection(editorContext.Selection.SelectedMarkupRange);
                            if (tableSelection.HasContiguousSelection && !tableSelection.EntireTableSelected)
                            {
                                TableEditor.SelectCell(editorContext, tableSelection.BeginCell);
                            }
                        }

                        // insert the table
                        TableEditor.InsertTable(_currentEditor, editorContext, tableCreationParameters);
                    }
                }

                // focus the editor so we see the cursor in the first cell
                _currentEditor.Focus();
            }

        }

        private void commandAddPlugin_Execute(object sender, EventArgs e)
        {
            ShellHelper.LaunchUrl(GLink.Instance.DownloadPlugins);
        }

        private void _normalHtmlContentEditor_HelpRequest(object sender, EventArgs e)
        {
            // execute the system help command
            Command commandHelp = CommandManager.Get(CommandId.Help);
            if (commandHelp != null)
                commandHelp.PerformExecute();
        }

#if SUPPORT_FILES
        private void commandInsertFile_Execute(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Select a file";
                    if ( openFileDialog.ShowDialog(_mainFrameWindow) == DialogResult.OK )
                    {
                        // TODO: This call to LocalFileReferenceFixer shouldn't be necessary.
                        // InsertLink call should take care of this for us automatically
                        string newPath = LocalFileReferenceFixer(string.Format(CultureInfo.InvariantCulture, "file:\\\\\\{0}", openFileDialog.FileName));;
                        string name = Path.GetFileName(openFileDialog.FileName);
                        InsertLink(newPath, name, name, String.Empty, false);

                    }
                }
            }
        }
#endif

        private PictureEditingManager PictureEditingManager
        {
            get
            {
                if (_pictureEditingManager == null)
                {
                    ImagePropertiesSidebarHostControl imageSidebarHost = _htmlEditorSidebarHost.GetSidebarControlOfType(typeof(ImagePropertiesSidebarHostControl)) as ImagePropertiesSidebarHostControl;
                    if (imageSidebarHost != null && imageSidebarHost.PictureEditingManager != null)
                        _pictureEditingManager = imageSidebarHost.PictureEditingManager;
                    else
                        Debug.Fail("Unable to set PictureEditingManager.");
                }
                return _pictureEditingManager;
            }
        }
        private PictureEditingManager _pictureEditingManager;

        private void FixCommands(bool fullyEditableActive)
        {
            _fullyEditableRegionActive = fullyEditableActive;
            FireFixCommandEvent(fullyEditableActive);
            _currentEditor.FullyEditableRegionActive = fullyEditableActive;

            //flag whether the currently focused region supports formatting commands
            //Note: this disables commands such as bold, bullets, align, etc for the title region
            _focusedRegionSupportsFormattingCommands = fullyEditableActive;

            commandInsertHorizontalLine.Enabled = fullyEditableActive;
            commandInsertClearBreak.Enabled = fullyEditableActive;
        }

        protected delegate void FixCommendsDelegate(bool fullyEditableActive);

        protected event FixCommendsDelegate FixCommandEvent;
        private void FireFixCommandEvent(bool fullyEditableActive)
        {
            if (FixCommandEvent != null)
                FixCommandEvent(fullyEditableActive);
        }

        public bool FullyEditableRegionActive
        {
            get
            {
                return _fullyEditableRegionActive;
            }
        }

        private bool _fullyEditableRegionActive = false;

        protected virtual void ManageCommandsForEditingMode()
        {
            commandViewNormal.Latched = CurrentEditingMode == EditingMode.Wysiwyg;
            commandViewWebPreview.Latched = CurrentEditingMode == EditingMode.Preview;
            commandViewCode.Latched = CurrentEditingMode == EditingMode.Source;
            commandViewPlainText.Latched = CurrentEditingMode == EditingMode.PlainText;

            // disable sidebar for web preview mode
            commandViewSidebar.Enabled = commandViewNormal.Latched || commandViewCode.Latched;

            //disable insert commands if editor is in read-only mode
            bool allowInsertCommands = (CurrentEditingMode == EditingMode.Source || CurrentEditingMode == EditingMode.Wysiwyg);

            // Enable/disable all built in plugins
            commandInsertTable.Enabled = allowInsertCommands;
            commandInsertTable2.Enabled = allowInsertCommands;
            commandInsertPicture.Enabled = allowInsertCommands;
#if SUPPORT_FILES
            commandInsertFile.Enabled = allowInsertCommands;
#endif
            CommandManager.Get(CommandId.TableMenu).Enabled = allowInsertCommands;
            commandInsertTags.Enabled = allowInsertCommands;
            commandInsertMap.Enabled = allowInsertCommands;
            commandInsertEmoticon.Enabled = allowInsertCommands;
            commandInsertVideoFromFile.Enabled = allowInsertCommands;
            commandInsertVideoFromService.Enabled = allowInsertCommands;
            commandInsertVideoFromWeb.Enabled = allowInsertCommands;

            // Toggle the drop handlers for plain text mode
            ((ExtendedHtmlEditorMashallingHandler)_normalHtmlContentEditor.DataFormatHandlerFactory).IsPlainTextOnly = !allowInsertCommands;

        }

        #region IHtmlContentEditorHost Members

        string IHtmlEditorHost.TransformHtml(string html)
        {
            return FixUpLocalFileReferenceForInsertion(html);
        }

        #endregion

        #region IBlogPostSpellCheckingContext Members
        public bool CanSpellCheck
        {
            get
            {
                return _spellingChecker.IsInitialized;
            }
        }

        public string AutoCorrectLexiconFilePath
        {
            get { return _autoCorrectLexiconFile; }
        }

        public ISpellingChecker SpellingChecker
        {
            get { return _spellingChecker; }
        }
        private WinSpellingChecker _spellingChecker = new WinSpellingChecker();

        public void SetSpellingOptions(string bcp47Code, bool useAutoCorrect)
        {
            if (ControlHelper.ControlCanHandleInvoke(_editorContainer) && _editorContainer.InvokeRequired)
            {
                _editorContainer.BeginInvoke(new ThreadStart(
                    () => SetSpellingOptions(bcp47Code, useAutoCorrect)
                    ));
                return;
            }

            _spellingChecker.StopChecking();
            _spellingChecker.SetOptions(bcp47Code);
            _spellingChecker.StartChecking();

            _autoCorrectLexiconFile = null; // TODO: Auto correct custom file
            if (SpellingOptionsChanged != null)
            {
                SpellingOptionsChanged(this, EventArgs.Empty);
            }
        }

        public void DisableSpelling()
        {
            _spellingChecker.StopChecking();
            _spellingChecker.SetOptions(string.Empty);
            if (SpellingOptionsChanged != null)
                SpellingOptionsChanged(this, EventArgs.Empty);
        }

        public event EventHandler SpellingOptionsChanged;

        #endregion

        #region IBlogPostImageEditingContext Members

        public ISupportingFileService SupportingFileService
        {
            get { return _fileService; }
        }

        private LazyLoader<ImageDecoratorsManager> _imageDecoratorsManager;
        public ImageDecoratorsManager DecoratorsManager
        {
            get { return _imageDecoratorsManager.Value; }
        }

        private EmoticonsManager _emoticonsManager;
        public EmoticonsManager EmoticonsManager
        {
            get { return _emoticonsManager; }
        }

        public void ActivateDecoratorsManager()
        {
            DecoratorsManager.Activate();
        }
        public void DeactivateDecoratorsManager()
        {
            if (_imageDecoratorsManager.IsInitialized)
                DecoratorsManager.Deactivate();
        }

        public BlogPostImageDataList ImageList { get { return _imageDataList; } }

        public string CurrentAccountId
        {
            get
            {
                return _currentEditorAccount.Id;
            }
        }

        public string ImageServiceId
        {
            get
            {
                // no longer support image services
                return null;
            }
        }

        IEditorOptions IBlogPostImageEditingContext.EditorOptions
        {
            get { return _currentEditorAccount.EditorOptions; }
        }

        bool IBlogPostSidebarContext.SidebarVisible
        {
            get { return _htmlEditorSidebarHost.Visible; }
            set { _htmlEditorSidebarHost.Visible = value; }
        }

        SmartContentEditor IBlogPostSidebarContext.CurrentEditor
        {
            get
            {
                ContentSourceSidebarControl contentSourceSidebarControl =
                    _htmlEditorSidebarHost.ActiveSidebarControl as ContentSourceSidebarControl;
                if (contentSourceSidebarControl != null)
                {
                    return contentSourceSidebarControl.CurrentEditor;
                }
                return null;
            }
        }

        public event EventHandler SidebarVisibleChanged
        {
            add
            {
                _htmlEditorSidebarHost.VisibleChanged += value;
            }
            remove
            {
                _htmlEditorSidebarHost.VisibleChanged -= value;
            }
        }

        #endregion

        #region IImageTargetEditor
        void IImageTargetEditor.ImageEditFinished()
        {
            _currentEditor.IsDirty = true;
        }
        #endregion

        #region ContentSource Support

        private class SmartContentForceInvalidateNotify : IDisposable
        {
            private ISmartContent _sContent;
            private bool _originalForceInvalidate;
            public SmartContentForceInvalidateNotify(ISmartContent sContent)
            {
                _sContent = sContent;
                _originalForceInvalidate = _sContent.Properties.GetBoolean(ForceInvalidateSmartContent.FORCEINVALIDATE, false);

                ForceInvalidateSmartContent smartContent = new ForceInvalidateSmartContent(sContent);
                smartContent.ForceInvalidate = true;
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                _sContent.Properties.SetBoolean(ForceInvalidateSmartContent.FORCEINVALIDATE, _originalForceInvalidate);
            }

            #endregion
        }

        private void GetStructuredEditorHtml(IPublishingContext site, SmartContentSource source, ISmartContent sContent, ref string content)
        {
            using (new SmartContentForceInvalidateNotify(sContent))
                content = source.GenerateEditorHtml(sContent, site);
        }
        private void GetStructuredPublishHtml(IPublishingContext site, SmartContentSource source, ISmartContent sContent, ref string content)
        {
            content = source.GeneratePublishHtml(sContent, site);
        }

        public IHTMLElement GetSmartContentElement(string sourceId, string contentId)
        {
            // Check to see if the content source wants a snapshot when they generate their publish html
            if (_currentEditor is BlogPostHtmlEditorControl)
            {
                return ((IHTMLDocument3)_normalHtmlContentEditor.GetHtmlDocument()).getElementById(ContentSourceManager.MakeContainingElementId(sourceId, contentId));
            }

            return null;
        }

        string IPublishingContext.ServiceName
        {
            get { return _currentEditorAccount.ServiceName; }
        }

        string IPublishingContext.AccountId
        {
            get { return _currentEditorAccount.Id; }
        }

        string IPublishingContext.BlogName
        {
            get { return _currentEditorAccount.Name; }
        }

        string IPublishingContext.HomepageUrl
        {
            get { return _currentEditorAccount.HomepageUrl; }
        }

        Color? IPublishingContext.BodyBackgroundColor
        {
            get
            {
                Color color = Color.FromArgb(_currentEditorAccount.EditorOptions.PostBodyBackgroundColor);
                if (color.A != 255)
                    return null;
                return color;
            }
        }

        IPostInfo IPublishingContext.PostInfo
        {
            get
            {
                BlogPostEditingManager editingManager = (BlogPostEditingManager)_editingContext;
                editingManager.SaveEditsToPost(true, BlogPostSaveOptions.DefaultOptions);

                return _editingContext.BlogPost;
            }
        }

        public string ProviderId
        {
            get { return _currentEditorAccount.ProviderId; }
        }

        SupportsFeature IPublishingContext.SupportsImageUpload
        {
            get
            {
                return _currentEditorAccount.EditorOptions.SupportsImageUpload;
            }
        }

        SupportsFeature IPublishingContext.SupportsScripts
        {
            get
            {
                return _currentEditorAccount.EditorOptions.SupportsScripts;
            }
        }

        SupportsFeature IPublishingContext.SupportsEmbeds
        {
            get
            {
                return _currentEditorAccount.EditorOptions.SupportsEmbeds;
            }
        }

        IWin32Window IContentSourceSite.DialogOwner
        {
            get { return _mainFrameWindow; }
        }

        IExtensionData IContentSourceSite.CreateExtensionData(string id)
        {
            return _extensionDataList.CreateExtensionData(id);
        }

        bool IContentSourceSite.InsertCommandsEnabled
        {
            get
            {
                // use the insert picture command as a proxy for this
                return commandInsertPicture.Enabled;
            }
        }

        void IContentSourceSite.InsertContent(string content, bool select)
        {
            //Insert gestures are only supported in the body, so force focus to the body
            FocusBody();

            InsertHtml(content, select ? HtmlInsertionOptions.SelectFirstControl : HtmlInsertionOptions.MoveCursorAfter);
        }

        void IContentSourceSite.InsertContent(string contentSourceId, string content, IExtensionData extensionData)
        {
            //Insert gestures are only supported in the body, so force focus to the body
            FocusBody();

            InsertContentBlock(contentSourceId, content, extensionData, HtmlInsertionOptions.MoveCursorAfter);
        }

        void IContentSourceSite.InsertContent(string contentSourceId, string content, IExtensionData extensionData, HtmlInsertionOptions insertionOptions)
        {
            //Insert gestures are only supported in the body, so force focus to the body
            FocusBody();

            InsertContentBlock(contentSourceId, content, extensionData, insertionOptions);
        }

        private void InsertContentBlock(string contentSourceId, string content, IExtensionData extensionData, HtmlInsertionOptions insertionOptions)
        {
            // generate a new unique id for this content block
            string blockId = extensionData.Id;

            using (new HtmlEditorControl.InitialInsertionNotify(_normalHtmlContentEditor))
            {
                // insert the appropriate html
                InsertHtml(SmartContentInsertionHelper.GenerateContentBlock(contentSourceId, blockId, content, extensionData), insertionOptions);
            }
        }

        private IHTMLElement FindSmartContentElementByContentId(string searchContentId)
        {
            return FindSmartContentElemenCore((csid, cid) => cid == searchContentId);
        }

        private IHTMLElement FindSmartContentElementByContentSourceId(string searchContentSourceId)
        {
            return FindSmartContentElemenCore((csid, cid) => csid == searchContentSourceId && _extensionDataList.GetExtensionData(cid) != null /* if you are replying to an email that was a photomail, that does not implicitly make this a photomail */);
        }

        private delegate bool FindSmartContentElemenFilter(string contentSourceId, string contentId);
        private IHTMLElement FindSmartContentElemenCore(FindSmartContentElemenFilter filter)
        {
            IHTMLElement2 postBodyElement = (IHTMLElement2)_normalHtmlContentEditor.PostBodyElement;
            if (postBodyElement != null)
            {
                foreach (IHTMLElement divElement in postBodyElement.getElementsByTagName("div"))
                {
                    // Make sure that the div we have is a smart content, and that it isn't just a div that is part of smart content.
                    if (divElement.className == null || divElement.id == null ||
                        divElement.className == ContentSourceManager.SMART_CONTENT ||
                        !ContentSourceManager.IsSmartContent(divElement))
                        continue;

                    string contentSourceId = "";
                    string contentId = "";

                    // Get the contentid of the div smart content we have
                    ContentSourceManager.ParseContainingElementId(divElement.id, out contentSourceId,
                                                                  out contentId);

                    if (filter(contentSourceId, contentId))
                        return divElement;

                }
            }

            return null;
        }

        IExtensionData[] IContentSourceSite.UpdateContent(IExtensionData[] extensionDataListOrginal)
        {
            IExtensionData[] extensionDataList = (IExtensionData[])extensionDataListOrginal.Clone();
            // Find all the smart content in the list, and tell them to update.
            IHTMLElement2 postBodyElement = (IHTMLElement2)_normalHtmlContentEditor.PostBodyElement;
            if (postBodyElement != null)
            {
                foreach (IHTMLElement divElement in postBodyElement.getElementsByTagName("div"))
                {
                    // Make sure that the div we have is a smart content, and that it isn't just a div that is part of smart content.
                    if (divElement.className == null || divElement.id == null || divElement.className == ContentSourceManager.SMART_CONTENT || !ContentSourceManager.IsSmartContent(divElement))
                        continue;

                    string contentSourceId = "";
                    string contentId = "";

                    // Get the contentid of the div smart content we have
                    ContentSourceManager.ParseContainingElementId(divElement.id, out contentSourceId, out contentId);

                    // Check to see if this smart content is one we want to update
                    int index = ArrayHelper.SearchForIndexOf<string, IExtensionData>(extensionDataList,
                                                             contentId,
                                                             delegate (string a, IExtensionData b) { return b != null && a == b.Id; });

                    if (index == -1)
                        continue;

                    // The smart content we found in the DOM is also one we
                    // want to update, so save a reference to it from the data list
                    IExtensionData extensionData = extensionDataList[index];

                    // We remove it from the list, which at the end of the function will only
                    // leave the IExtensionData that were not found in the editor.
                    extensionDataList[index] = null;

                    // Make a SmartContent for the element we are about to update,
                    // this will allow the content source to  get at the properties bag
                    // for the smart content so it knows how to update the content
                    SmartContent smartContent = new SmartContent(extensionData);

                    IContentSourceSidebarContext sourceContext = this;

                    // Find the content source that we will use to update the smart content
                    ContentSourceInfo contentSourceInfo = sourceContext.FindContentSource(contentSourceId);

                    // Make sure that we found one, and that is it indeed SmartContentSource
                    if (contentSourceInfo != null && contentSourceInfo.Instance is SmartContentSource)
                    {
                        // Get the html that the content source wants to update the element with
                        string newHtml = ((SmartContentSource)contentSourceInfo.Instance).GenerateEditorHtml(smartContent, this);

                        // If the source for this smart content wants to filter some updates
                        // give it a chance to right now.
                        if (contentSourceInfo.Instance is IContentUpdateFilter)
                        {
                            if (!((IContentUpdateFilter)contentSourceInfo.Instance).ShouldUpdateContent(divElement.innerHTML, newHtml))
                                continue;
                        }

                        // If we are actually sure we want to update the element then we insert it
                        // and wrap it in an invisible undo unit so it will undo the last change the user
                        // made if they ctrl+z
                        using (EditorUndoUnit undo = new EditorUndoUnit(_currentEditor, true))
                        {
                            SmartContentInsertionHelper.InsertContentIntoElement(newHtml, smartContent, sourceContext, divElement);
                            _htmlEditorSidebarHost.ForceUpdateSidebarState();
                            undo.Commit();
                        }

                    }
                }
            }

            return (IExtensionData[])ArrayHelper.Compact(extensionDataList);
        }

        private TextEditingCommandDispatcher _textEditingCommandDispatcher;

        #endregion

        private Command commandViewNormal;
        private Command commandViewWebPreview;
        private Command commandViewCode;
        private Command commandViewPlainText;

        private Command commandViewSidebar;
        private Command commandInsertPicture;

        private Command commandInsertHorizontalLine;
        private Command commandInsertClearBreak;

#if SUPPORT_FILES
        private Command commandInsertFile ;
#endif
        private Command commandInsertTable;
        private Command commandInsertTable2;
        private Command commandAddPlugin;

        protected IBlogPostHtmlEditor _currentEditor;
        protected BlogPostHtmlEditorControl _normalHtmlContentEditor;
        protected HtmlEditorSidebarHost _htmlEditorSidebarHost;
        private BlogPostHtmlSourceEditorControl _codeHtmlContentEditor;

        List<IDisposable> _itemsToDisposeOnEditorClose = new List<IDisposable>();

        private IBlogPostEditingContext _editingContext;
        protected bool _isPage;
        private BlogPostSupportingFileStorage _supportingFileStorage;
        private BlogPostImageDataList _imageDataList;
        private BlogPostExtensionDataList _extensionDataList;
        private ISupportingFileService _fileService;

        protected IMainFrameWindow _mainFrameWindow;
        protected IBlogPostEditingSite _postEditingSite;
        private Panel _editorContainer;
        private bool _focusedRegionSupportsFormattingCommands;

        private IContainer components = new Container();
        private Timer wordCountTimer;

        private RefreshableContentManager _refreshSmartContentManager;

        private BlogEditingTemplate GetSurroundingContent()
        {
            // surrounding content based on standard header and footer
            if (_editingTemplateWysiwyg == null)
            {
                throw new Exception("SetTheme was not called before the editor was loaded");
            }

            BlogEditingTemplate template = null;

            switch (CurrentEditingMode)
            {
                case EditingMode.Wysiwyg:
                    template = _editingTemplateWysiwyg;
                    break;

                case EditingMode.Preview:
                    template = _editingTemplatePreview;
                    break;

                case EditingMode.Source:
                    template = new BlogEditingTemplate(_editingTemplateWysiwyg.ContainsTitle);
                    break;

                case EditingMode.PlainText:
                    template = EditingTemplatePlain;
                    break;

            }

            if (template != null)
                return template;

            throw new Exception("Cannot find correct template for EditingMode: " + CurrentEditingMode);
        }

        private BlogEditingTemplate _editingTemplateWysiwyg;
        private BlogEditingTemplate _editingTemplatePreview;
        private BlogEditingTemplate _editingTemplatePlain;

        private BlogEditingTemplate EditingTemplatePlain
        {
            get
            {
                if (_editingTemplatePlain == null)
                {
                    string plainTextHtml =
                        String.Format(CultureInfo.InvariantCulture, "<html><head></head><body style='font-family: {0}'>{{post-body}}</body></html>",
                                      Res.Get(StringId.DefaultTemplateBodyFont));
                    string html = EditingTemplateLoader.CreateTemplateWithBehaviors(plainTextHtml, new NullElementBehaviorManager());
                    _editingTemplatePlain = new BlogEditingTemplate(html, false);
                }
                return _editingTemplatePlain;
            }
        }

        #region IHtmlEditorCommandSource Members

        public void ViewSource()
        {
            ReloadEditor();
            _mainFrameWindow.BeginInvoke(new InvokeInUIThreadDelegate(_currentEditor.CommandSource.ViewSource), new object[] { });
        }

        public void ClearFormatting()
        {
            _currentEditor.CommandSource.ClearFormatting();
        }

        public bool CanApplyFormatting(CommandId? commandId)
        {
            if (CurrentEditingMode == EditingMode.PlainText || IsEditFieldSelected || (IsSmartContentSelected && TextEditingCommandDispatcher.IsFontFormattingCommand(commandId)))
                return false;

            // In most cases we will use the bold command to decide whether a formatting command is enabled or not
            // Though in the case of some of the alignment commands, we will want to enable them if an image
            // or smart content has been selected
            if (commandId == CommandId.AlignCenter || commandId == CommandId.AlignLeft || commandId == CommandId.AlignRight
                || commandId == CommandId.LTRTextBlock || commandId == CommandId.RTLTextBlock)
            {
                IHtmlEditorComponentContext editorIHtmlEditorComponentContext =
                    _currentEditor as IHtmlEditorComponentContext;

                if (editorIHtmlEditorComponentContext != null
                    && editorIHtmlEditorComponentContext.Selection != null
                    && (editorIHtmlEditorComponentContext.Selection.SelectedImage != null
                        || editorIHtmlEditorComponentContext.Selection is SmartContentSelection
                       ))
                {
                    return true;
                }
            }

            return
                _currentEditor != null && _focusedRegionSupportsFormattingCommands &&
                _currentEditor.CommandSource.CanApplyFormatting(commandId);

        }

        public string SelectionFontFamily
        {
            get
            {
                return _currentEditor != null ? _currentEditor.CommandSource.SelectionFontFamily : String.Empty;
            }
        }

        public void ApplyFontFamily(string fontFamily)
        {
            _currentEditor.CommandSource.ApplyFontFamily(fontFamily);
        }

        public float SelectionFontSize
        {
            get
            {
                return _currentEditor.CommandSource.SelectionFontSize;
            }

        }

        public void ApplyFontSize(float fontSize)
        {
            _currentEditor.CommandSource.ApplyFontSize(fontSize);
        }

        public int SelectionForeColor
        {
            get
            {
                return _currentEditor.CommandSource.SelectionForeColor;
            }
        }

        public void ApplyFontForeColor(int color)
        {
            _currentEditor.CommandSource.ApplyFontForeColor(color);
        }

        public int SelectionBackColor
        {
            get
            {
                return _currentEditor.CommandSource.SelectionBackColor;
            }
        }

        public void ApplyFontBackColor(int? color)
        {
            _currentEditor.CommandSource.ApplyFontBackColor(color);
        }

        public string SelectionStyleName
        {
            get
            {
                return _currentEditor != null ? _currentEditor.CommandSource.SelectionStyleName : String.Empty;
            }
        }

        public void ApplyHtmlFormattingStyle(IHtmlFormattingStyle style)
        {
            _currentEditor.CommandSource.ApplyHtmlFormattingStyle(style);
        }

        public bool SelectionBold
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionBold;
            }
        }

        public void ApplyBold()
        {
            _currentEditor.CommandSource.ApplyBold();
        }

        public bool SelectionSuperscript
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionSuperscript;
            }
        }

        public void ApplySubscript()
        {
            _currentEditor.CommandSource.ApplySubscript();
        }

        public bool SelectionSubscript
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionSubscript;
            }
        }

        public void ApplySuperscript()
        {
            _currentEditor.CommandSource.ApplySuperscript();
        }

        public bool SelectionItalic
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionItalic;
            }
        }

        public void ApplyItalic()
        {
            _currentEditor.CommandSource.ApplyItalic();
        }

        public bool SelectionUnderlined
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionUnderlined;
            }
        }

        public void ApplyUnderline()
        {
            _currentEditor.CommandSource.ApplyUnderline();
        }

        public bool SelectionStrikethrough
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionStrikethrough;
            }
        }

        public void ApplyStrikethrough()
        {
            _currentEditor.CommandSource.ApplyStrikethrough();
        }

        public bool SelectionIsLTR
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionIsLTR;
            }
        }

        public void InsertLTRTextBlock()
        {
            _currentEditor.CommandSource.InsertLTRTextBlock();
        }

        public bool SelectionIsRTL
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionIsRTL;
            }
        }

        public void InsertRTLTextBlock()
        {
            _currentEditor.CommandSource.InsertRTLTextBlock();
        }

        public EditorTextAlignment GetSelectionAlignment()
        {
            IHTMLElement element = null;

            try
            {
                IHtmlEditorComponentContext editorIHtmlEditorComponentContext = _currentEditor as IHtmlEditorComponentContext;
                if (editorIHtmlEditorComponentContext != null && editorIHtmlEditorComponentContext.Selection != null)
                {
                    // Check and see if this an image
                    if (editorIHtmlEditorComponentContext.Selection.SelectedImage != null)
                    {
                        element = editorIHtmlEditorComponentContext.Selection.SelectedImage as IHTMLElement;
                    }

                    // Check if it is smart content
                    if (editorIHtmlEditorComponentContext.Selection is SmartContentSelection)
                    {
                        IHTMLElement[] elements =
                            editorIHtmlEditorComponentContext.Selection.SelectedMarkupRange.GetTopLevelElements(
                                ContentSourceManager.IsSmartContentContainer);
                        if (elements.Length > 0)
                        {
                            element = elements[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to detect alignment: " + ex);
                element = null;
            }

            // If we found an image or smart content, try to get the alignment off of it
            if (element != null)
            {
                HtmlAlignDecoratorSettings img = new HtmlAlignDecoratorSettings(element);
                switch (img.GetAlignmentFromHtml())
                {
                    case ImgAlignment.LEFT:
                        return EditorTextAlignment.Left;
                    case ImgAlignment.RIGHT:
                        return EditorTextAlignment.Right;
                    case ImgAlignment.CENTER:
                        return EditorTextAlignment.Center;
                }
                return EditorTextAlignment.None;
            }

            return _currentEditor != null ? _currentEditor.CommandSource.GetSelectionAlignment() : EditorTextAlignment.None;
        }

        private void ApplyAlignmentToImage(EditorTextAlignment alignment, IHtmlEditorComponentContext editor, IHTMLElement element)
        {
            using (EditorUndoUnit undo = new EditorUndoUnit(_currentEditor))
            {
                // Make an alignment helper to help us change the alignment of the image
                HtmlAlignDecoratorSettings alignmentManager = new HtmlAlignDecoratorSettings(element);

                switch (alignment)
                {
                    case EditorTextAlignment.Center:
                        alignmentManager.SetImageHtmlFromAlignment(ImgAlignment.CENTER);
                        break;
                    case EditorTextAlignment.Right:
                        alignmentManager.SetImageHtmlFromAlignment(ImgAlignment.RIGHT);
                        break;
                    case EditorTextAlignment.Left:
                        alignmentManager.SetImageHtmlFromAlignment(ImgAlignment.LEFT);
                        break;
                    case EditorTextAlignment.None:
                        alignmentManager.SetImageHtmlFromAlignment(ImgAlignment.NONE);
                        break;
                    default:
                        break;
                }

                // Tell the side bar to update its settings to refect the new alignment
                PictureEditingManager.UpdateView(editor.Selection.SelectedImage);
                undo.Commit();
            }
        }

        private void ApplyAlignmentToSmartContent(EditorTextAlignment alignment, IHtmlEditorComponentContext editor, IHTMLElement element)
        {
            using (EditorUndoUnit undo = new EditorUndoUnit(_currentEditor))
            {
                // It was smart content but know we need a reference to it as ISmartContent
                string contentId = element.id;
                string sourceId = "";
                string blockId = "";
                ContentSourceManager.ParseContainingElementId(contentId, out sourceId, out blockId);
                ISmartContent content = (this as IContentSourceSidebarContext).FindSmartContent(blockId);

                if (content != null)
                {
                    // Set the alignment on the smart content
                    switch (alignment)
                    {

                        case EditorTextAlignment.Center:
                            content.Layout.Alignment = Alignment.Center;
                            break;
                        case EditorTextAlignment.Right:
                            content.Layout.Alignment = Alignment.Right;
                            break;
                        case EditorTextAlignment.Left:
                            content.Layout.Alignment = Alignment.Left;
                            break;
                        case EditorTextAlignment.None:
                        case EditorTextAlignment.Justify:
                            content.Layout.Alignment = Alignment.None;
                            break;
                    }

                    // Ask its command source for some new html now that we have set its alignment property
                    string newHtml =
                        SmartContentInsertionHelper.GenerateContentBlock(sourceId, blockId,
                                                                         element.innerHTML, content,
                                                                         element);

                    InsertHtml(newHtml, false);

                    string elementId = ContentSourceManager.MakeContainingElementId(sourceId, blockId);
                    element = (_normalHtmlContentEditor.GetHtmlDocument() as IHTMLDocument3).getElementById(elementId);

                    _normalHtmlContentEditor.EmptySelection();
                    _normalHtmlContentEditor.FocusBody();

                    // We need to do this in the future because at this point, the behavior might not be attached yet
                    // and if it isnt, the content will be selected but it wont show the dashes lines around the smart content
                    TimerHelper.CallbackOnDelay(
                        delegate () { SmartContentSelection.SelectIfSmartContentElement(editor, element); }, 25);
                }
                undo.Commit();
            }
        }

        public void ApplyAlignment(EditorTextAlignment alignment)
        {
            // Check to see if our current editor is one that allows selections
            IHtmlEditorComponentContext editorIHtmlEditorComponentContext = _currentEditor as IHtmlEditorComponentContext;
            if (editorIHtmlEditorComponentContext != null
                && editorIHtmlEditorComponentContext.Selection != null)
            {
                // Check to see if the editor has an image selected
                if (editorIHtmlEditorComponentContext.Selection.SelectedImage != null)
                {
                    ApplyAlignmentToImage(alignment, editorIHtmlEditorComponentContext, (IHTMLElement)editorIHtmlEditorComponentContext.Selection.SelectedImage);
                    return;
                }

                // Check to see what is selectedi smart content
                IHTMLElement[] elements =
                    editorIHtmlEditorComponentContext.Selection.SelectedMarkupRange.GetTopLevelElements(
                        MarkupRange.FilterNone);
                if (elements.Length == 1 && ContentSourceManager.IsSmartContent(elements[0]))
                {
                    ApplyAlignmentToSmartContent(alignment, editorIHtmlEditorComponentContext, elements[0]);
                    return;
                }
            }

            // It wasnt smart content or an image, so continue with the normal way of applying alignment
            _currentEditor.CommandSource.ApplyAlignment(alignment);
        }

        public bool SelectionBulleted
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionBulleted;
            }
        }

        public void ApplyBullets()
        {
            _currentEditor.CommandSource.ApplyBullets();
        }

        public bool SelectionNumbered
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionNumbered;
            }
        }

        public void ApplyNumbers()
        {
            _currentEditor.CommandSource.ApplyNumbers();
        }

        public void ApplyBlockquote()
        {
            _currentEditor.CommandSource.ApplyBlockquote();
        }

        public bool SelectionBlockquoted
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.SelectionBlockquoted;
            }
        }

        bool IHtmlEditorCommandSource.CanIndent
        {
            get
            {
                //indent/outdent is implemented via blockquote for blogs since there is
                //no reliable way to indent without requiring CSS (which tends to get stripped)
                return CurrentEditingMode != EditingMode.PlainText && !IsSmartContentSelected && _currentEditor.CommandSource.CanIndent
                    && (!SelectionBlockquoted || GlobalEditorOptions.SupportsFeature(ContentEditorFeature.TabAsIndent));
            }
        }

        public void ApplyIndent()
        {
            _currentEditor.CommandSource.ApplyIndent();
        }

        bool IHtmlEditorCommandSource.CanOutdent
        {
            //indent/outdent is implemented via blockquote for blogs
            get
            {
                return CurrentEditingMode != EditingMode.PlainText && !IsSmartContentSelected && _currentEditor.CommandSource.CanOutdent
                    && (SelectionBlockquoted || GlobalEditorOptions.SupportsFeature(ContentEditorFeature.TabAsIndent));
            }
        }

        public void ApplyOutdent()
        {
            _currentEditor.CommandSource.ApplyOutdent();
        }

        public bool CanInsertLink
        {
            get
            {
                return CurrentEditingMode != EditingMode.PlainText && !IsEditFieldSelected && _currentEditor != null && _currentEditor.CommandSource.CanInsertLink;
            }
        }

        public void InsertLink()
        {
            _currentEditor.CommandSource.InsertLink();
        }

        public bool CanRemoveLink
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanRemoveLink;
            }
        }

        public void RemoveLink()
        {
            _currentEditor.CommandSource.RemoveLink();
        }

        public void OpenLink()
        {
            _currentEditor.CommandSource.OpenLink();
        }

        public void AddToGlossary()
        {
            _currentEditor.CommandSource.AddToGlossary();
        }

        public bool CanFind
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanFind;
            }
        }

        public bool CanPrint
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanPrint;
            }
        }

        public LinkInfo DiscoverCurrentLink()
        {
            return _currentEditor.CommandSource.DiscoverCurrentLink();
        }

        #endregion

        #region ISimpleTextEditorCommandSource Members

        public bool HasFocus
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.HasFocus;
            }
        }

        public bool CanUndo
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanUndo;
            }
        }

        public event EventHandler UndoExecuted;
        public void Undo()
        {
            _currentEditor.CommandSource.Undo();
            if (UndoExecuted != null)
                UndoExecuted(this, EventArgs.Empty);
        }

        public bool CanRedo
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanRedo;
            }
        }

        public event EventHandler RedoExecuted;
        public void Redo()
        {
            _currentEditor.CommandSource.Redo();
            if (RedoExecuted != null)
                RedoExecuted(this, EventArgs.Empty);

            ImageInsertionManager.ScanAndInitializeNewImages(_currentEditor, _fileService, _currentEditorAccount, this, _editorContainer, true, true);
        }

        public bool CanCut
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanCut;
            }
        }

        public void Cut()
        {
            _currentEditor.CommandSource.Cut();
        }

        public bool CanCopy
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanCopy;
            }
        }

        public void Copy()
        {
            _currentEditor.CommandSource.Copy();
        }

        public bool CanPaste
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanPaste;
            }
        }

        public event EventHandler PasteExecuted;
        public void Paste()
        {
            _currentEditor.CommandSource.Paste();
            if (PasteExecuted != null)
                PasteExecuted(this, EventArgs.Empty);
        }

        public bool CanPasteSpecial
        {
            get
            {
                return _currentEditor != null && CurrentEditingMode != EditingMode.PlainText && !IsEditFieldSelected && _currentEditor.CommandSource.CanPasteSpecial;
            }
        }

        public bool AllowPasteSpecial
        {
            get
            {
                return _currentEditor != null && CurrentEditingMode != EditingMode.PlainText && _currentEditor.CommandSource.AllowPasteSpecial && FullyEditableRegionActive;
            }
        }

        public event EventHandler PasteSpecialExecuted;
        public void PasteSpecial()
        {
            _currentEditor.CommandSource.PasteSpecial();
            if (PasteSpecialExecuted != null)
                PasteSpecialExecuted(this, EventArgs.Empty);
        }

        public bool CanClear
        {
            get
            {
                return _currentEditor != null && _currentEditor.CommandSource.CanClear;
            }
        }

        public void Clear()
        {
            // Check to see what is being deleted is smart content and if it is
            // check to see if any of the content sources are expecting to get a callback for it
            IHtmlEditorComponentContext componentContext = _currentEditor as IHtmlEditorComponentContext;
            if (componentContext != null)
            {
                IHTMLElement[] elements = componentContext.Selection.SelectedMarkupRange.GetTopLevelElements(ContentSourceManager.CreateSmartContentElementFilter());

                if (elements.Length == 1 && ContentSourceManager.IsSmartContentClass(elements[0].className))
                {
                    string contentSourceId;
                    string contentItemId;
                    ContentSourceManager.ParseContainingElementId(elements[0].id, out contentSourceId, out contentItemId);
                    ISmartContent content = ((IContentSourceSidebarContext)this).FindSmartContent(contentItemId);

                    ContentSourceInfo contentSourceInfo = ((IContentSourceSidebarContext)this).FindContentSource(contentSourceId);

                    if (contentSourceInfo != null && contentSourceInfo.Instance is ISupportsDeleteHook)
                    {
                        ((ISupportsDeleteHook)contentSourceInfo.Instance).OnSmartContentDelete(content);
                    }
                }
            }

            _currentEditor.CommandSource.Clear();
        }

        public void SelectAll()
        {
            _currentEditor.CommandSource.SelectAll();
        }

        public void InsertEuroSymbol()
        {
            _currentEditor.CommandSource.InsertEuroSymbol();
        }

        public bool ReadOnly
        {
            get { return _currentEditor.CommandSource.ReadOnly; }
        }

        public bool SuspendAutoSave
        {
            get { return _currentEditor.SuspendAutoSave; }
        }

        public event EventHandler CommandStateChanged;

        protected void OnCommandStateChanged()
        {
            if (CommandStateChanged != null)
                CommandStateChanged(this, EventArgs.Empty);
        }

        public event EventHandler AggressiveCommandStateChanged;

        protected void OnAggressiveCommandStateChanged()
        {
            if (AggressiveCommandStateChanged != null)
                AggressiveCommandStateChanged(this, EventArgs.Empty);
        }

        private void editor_CommandStateChanged(object sender, EventArgs e)
        {
            OnCommandStateChanged();
        }

        private void editor_AggressiveCommandStateChanged(object sender, EventArgs e)
        {
            OnAggressiveCommandStateChanged();
        }

        #endregion

        #region IInitialFocusHandler Members

        public void SetInitialFocus()
        {
            Focus();
        }

        #endregion

        #region IContentSourceEditorContext Members

        public event ContentResizedEventHandler ContentResized;

        private void ResizedListener(Size newSize, bool completed)
        {
            if (ContentResized != null)
                ContentResized(newSize, completed);
        }

        ContentSourceInfo IContentSourceSidebarContext.FindContentSource(string contentSourceId)
        {
            return ContentSourceManager.FindContentSource(contentSourceId);
        }

        void IContentSourceSidebarContext.RemoveSmartContent(string contentId)
        {
            _extensionDataList.RemoveExtensionData(contentId);
        }

        void IContentSourceSidebarContext.DeleteSmartContent(string contentId)
        {
            Clear();
        }

        void IContentSourceSidebarContext.SelectSmartContent(string contentId)
        {
            IHTMLElement element = FindSmartContentElementByContentId(contentId);
            SmartContentSelection.SelectIfSmartContentElement(_normalHtmlContentEditor, element);
        }

        ISmartContent IContentSourceSidebarContext.FindSmartContent(string contentId)
        {
            IExtensionData exData = _extensionDataList.GetExtensionData(contentId);
            if (exData != null)
                return new SmartContent(_extensionDataList.GetExtensionData(contentId));
            return null;
        }

        IExtensionData IContentSourceSidebarContext.FindExtentsionData(string contentId)
        {
            return _extensionDataList.GetExtensionData(contentId);
        }

        ISmartContent IContentSourceSidebarContext.CloneSmartContent(string contentId, string newContentId)
        {
            IExtensionData exData = _extensionDataList.CloneExtensionData(contentId, newContentId);
            if (exData != null)
                return new SmartContent(exData);
            return null;
        }

        void IContentSourceSidebarContext.OnSmartContentEdited(string contentId)
        {
            IHTMLElement element = FindSmartContentElementByContentId(contentId);

            Debug.Assert(element != null && element.document != null, "Couldn't find the SmartContent specified or it was invalid!");

            if (element != null && element.document != null)
            {
                // When a photo album is updated inline by EditableSmartContent.SaveEditedSmartContent(), there is no
                // notification to the editor that any HTML was changed. However, because the inline edit field is
                // rewritten, we need to spell check it again.
                MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)element.document);
                _normalHtmlContentEditor.DamageServices.AddDamage(markupServices.CreateMarkupRange(element, false));
            }
        }

        #endregion

        #region EditorUndoUnit
        internal class EditorUndoUnit : IDisposable
        {
            IUndoUnit undoUnit;
            public EditorUndoUnit(IBlogPostHtmlEditor editor)
                : this(editor, false)
            {

            }

            public EditorUndoUnit(IBlogPostHtmlEditor editor, bool invisible)
            {
                if (editor is HtmlEditorControl)
                {
                    if (invisible)
                    {
                        undoUnit = (editor as IHtmlEditorComponentContext).CreateInvisibleUndoUnit();
                    }
                    else
                    {
                        undoUnit = (editor as IHtmlEditorComponentContext).CreateUndoUnit();
                    }
                }
            }

            public void Commit()
            {
                if (undoUnit != null)
                    undoUnit.Commit();
            }
            public void Dispose()
            {
                if (undoUnit != null)
                    undoUnit.Dispose();
            }

        }

        private void _htmlEditorSidebarHost_VisibleChanged(object sender, EventArgs e)
        {
            if (_htmlEditorSidebarHost.Visible)
            {
                if (!GlobalEditorOptions.SupportsFeature(ContentEditorFeature.EnableSidebar))
                {
                    _htmlEditorSidebarHost.Visible = false;
                }

                ControlHelper.FocusControl(_htmlEditorSidebarHost, true);
            }
        }

        private void panel_Resize(object sender, EventArgs e)
        {
            ((Control)sender).Invalidate(false);
        }

        private int _editorLoadSuppressCount;
        private string _autoCorrectLexiconFile;
        private Command commandInsertMap;
        private bool _mapsFeatureEnabled;
        private bool _videoProvidersFeatureEnabled;
        private bool _tagProvidersFeatureEnabled;
        private Command commandInsertVideoFromFile;
        private Command commandInsertVideoFromService;
        private Command commandInsertTags;
        private Command commandInsertVideoFromWeb;
        private EmoticonsGalleryCommand commandInsertEmoticon;

        internal void AddLoadSuppressCount()
        {
            _editorLoadSuppressCount++;
        }

        internal void RemoveLoadSuppressCount()
        {
            _editorLoadSuppressCount--;
            Debug.Assert(_editorLoadSuppressCount > -1, "_editorLoadSuppressCount has gone below zero, the count did not stay in sync");
        }

        internal bool IsEditorLoadSuppressed
        {
            get
            {
                return _editorLoadSuppressCount > 0;
            }
        }
        #endregion

        #region IInternalSmartContentContextSource

        public Size BodySize
        {
            get { return _normalHtmlContentEditor.PostBodySize; }
        }

        public SmartContentEditor GetSmartContentEditor(string contentSourceId)
        {
            return _htmlEditorSidebarHost.GetSmartContentEditor(contentSourceId);
        }
        #endregion

    }

    public delegate string CreateFileCallback(string requestedFileName);

    public class EditingViews
    {
        public const string Normal = "Normal";
        public const string WebLayout = "WebLayout";
        public const string WebPreview = "WebPreview";
        public const string HtmlCode = "HtmlCode";
    }

    public interface IEditingMode
    {
        EditingMode CurrentEditingMode { get; }
    }
}
