// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Marketization;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.PostEditor.PostPropertyEditing;
using OpenLiveWriter.SpellChecker;

// @RIBBON TODO: Cleanly remove obsolete code

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    internal class BlogPostHtmlEditor : OpenLiveWriter.PostEditor.ContentEditor, INewCategoryContext, IBlogPostEditor
    {
        private string instanceId = Guid.NewGuid().ToString();

        // The panel that holds blog this band, formatting bar, editor, tabs, and general post properties
        private Panel _editorContainer;

        // Post properties tray, BlogPostHtmlEditor is in charge of pushing IBlogPostEditor calls through to it
        private PostPropertiesBandControl _postPropertyEditor;

        // Edit / Preview / Source tabs, and status bar
        private PostEditorFooter _postEditorFooter;

        protected SemanticHtmlGalleryCommand commandSemanticHtml;

        // Formatting bar
        //private CommandBarControl _commandBarControl;
        //private EditorCommandBarLightweightControl _editorCommandBar = new EditorCommandBarLightweightControl();
        private Command commandInsertExtendedEntry;
        private Command commandViewUseStyles;

        private BlogPostHtmlEditor(IMainFrameWindow mainFrameWindow, Panel panelEditorContainer, IBlogPostEditingSite postEditingSite)
            : base(mainFrameWindow, panelEditorContainer, postEditingSite, new BlogPostHtmlEditorControl.BlogPostHtmlEditorSecurityManager(), new BlogPostTemplateStrategy(), MshtmlOptions.DEFAULT_DLCTL)
        {
            _editorContainer = panelEditorContainer;
            //InitializeCommandBar(commandBarDefinition);

            //_editorCommandBar.VerticalLineX = (BidiHelper.IsRightToLeft ? _htmlEditorSidebarHost.Right - 1 : _htmlEditorSidebarHost.Left);
            //_editorCommandBar.DrawVerticalLine = _htmlEditorSidebarHost.Visible;

            panelEditorContainer.SizeChanged += new EventHandler(editorContainer_SizeChanged);

            _htmlEditorSidebarHost.VisibleChanged += new EventHandler(_htmlEditorSidebarHost_VisibleChanged);

            CreateTabs();

            InitializePropertyEditors();

            ApplySpellingSettings(null, EventArgs.Empty);
            SpellingSettings.SpellingSettingsChanged += ApplySpellingSettings;

            EditorLoaded += new EventHandler(BlogPostHtmlEditor_EditorLoaded);
            FixCommandEvent += new FixCommendsDelegate(BlogPostHtmlEditor_FixCommandEvent);
        }

        public override void Dispose()
        {
            SpellingSettings.SpellingSettingsChanged -= ApplySpellingSettings;
            base.Dispose();
        }

        public override void OnEditorAccountChanged(IEditorAccount newEditorAccount)
        {
            Command cmd = CommandManager.Get(CommandId.IgnoreOnce);
            if (cmd != null)
            {
                cmd.On = GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SpellCheckIgnoreOnce);
            }
            base.OnEditorAccountChanged(newEditorAccount);

            // If any of these are null(or more likely all of them all null) then the editor has not been
            // fully loaded.  This will get called 1 time before the editor is fully loaded, this is
            // why SetAccountId is called at the end of Initialize()
            if (commandSemanticHtml != null && _currentEditor != null)
                commandSemanticHtml.SetAccountId(_currentBlog.Id, IsRTLTemplate, false);
        }

        private void ApplySpellingSettings(object sender, EventArgs args)
        {
            string language = SpellingSettings.Language;

            if (string.IsNullOrEmpty(language))
            {
                // No language selected. Disable the speller and return.
                DisableSpelling();
            }
            else
            {
                SetSpellingOptions(
                    language,
                    SpellingSettings.EnableAutoCorrect);
            }
        }

        void BlogPostHtmlEditor_FixCommandEvent(bool fullyEditableActive)
        {
            commandInsertExtendedEntry.Enabled = SupportsExtendedEntries && fullyEditableActive;
        }

        void commandSemanticHtml_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            int selectedIndex = args.GetInt(CommandId.SemanticHtmlGallery.ToString());
            IHtmlFormattingStyle style = commandSemanticHtml.Items[selectedIndex] as IHtmlFormattingStyle;
            ApplyHtmlFormattingStyle(style);
        }

        void BlogPostHtmlEditor_EditorLoaded(object sender, EventArgs e)
        {

            if (CurrentEditingMode == EditingMode.PlainText)
            {
                // Somehow the editor changed itself to plain text, dont change the tab
                Debug.Fail("PlainText mode should not be used inside of BlogPostHtmlEditor.");
                return;
            }

            if (CurrentEditingMode == EditingMode.Wysiwyg)
                _postEditorFooter.SelectTab(0);
            else if (CurrentEditingMode == EditingMode.Preview)
                _postEditorFooter.SelectTab(1);
            else
                _postEditorFooter.SelectTab(2);
        }

        public void Initialize(IBlogPostEditingContext editingContext, IBlogClientOptions clientOptions)
        {
            // this needs to happen before an editor is loaded, it is needed to know if
            // if the blog this band should show or not.
            bool firstTimeInitialization = _currentEditor == null;

            InitDefaultEditorForBlog();

            base.Initialize(editingContext, clientOptions, GetStyledHtml(), GetPreviewHtml(), true);

            commandSemanticHtml.SetAccountId(_currentBlog.Id, IsRTLTemplate, false);

            _postPropertyEditor.Initialize(editingContext, clientOptions);
        }

        public override void SaveChanges(BlogPost post, BlogPostSaveOptions options)
        {
            base.SaveChanges(post, options);

            _postPropertyEditor.SaveChanges(post, options);
        }

        private bool CheckTagsReminder()
        {
            if (!PostEditorSettings.TagReminder)
                return true;

            bool hasTags;
            if (_currentBlog.ClientOptions.SupportsKeywords)
            {
                hasTags = _postPropertyEditor.HasKeywords;
            }
            else
            {
                hasTags = _normalHtmlContentEditor.HasTags;
            }

            if (hasTags)
                return true;

            if (DisplayMessage.Show(MessageId.TagReminder, this) == DialogResult.No)
            {
                return false;
            }

            return true;
        }

        public override void OnPublishSucceeded(BlogPost blogPost, PostResult postResult)
        {
            // delegate to property editor
            _postPropertyEditor.OnPublishSucceeded(blogPost, postResult);
            IsDirty = false;
        }

        void INewCategoryContext.NewCategoryAdded(BlogPostCategory newCategory)
        {
            (_postPropertyEditor as INewCategoryContext).NewCategoryAdded(newCategory);
        }

        public override bool IsDirty
        {
            get
            {
                return base.IsDirty || (_postPropertyEditor != null && _postPropertyEditor.IsDirty);
            }
        }

        public void OnBlogSettingsChanged(bool templateChanged)
        {
            if (templateChanged)
                UpdateTemplateToBlogTheme();

            commandSemanticHtml.SetAccountId(_currentBlog.Id, IsRTLTemplate, templateChanged);

            UpdateExtendedEntryStatus();

            _postPropertyEditor.OnBlogSettingsChanged(templateChanged);
        }

        private Blog _currentBlog;
        public void OnBlogChanged(Blog newBlog)
        {
            _currentBlog = newBlog;
            // save dirty state of current editor
            bool isDirty = _currentEditor != null ? _currentEditor.IsDirty : false;

            OnEditorAccountChanged(newBlog);
            _postPropertyEditor.OnBlogChanged(newBlog);
            _editorContainer.DockPadding.Bottom = _postPropertyEditor.Visible ? 0 : 5;

            // determine what type of template we should be using for this weblog
            if (_currentEditor != null) //if null, then the editors haven't been loaded yet
            {
                InitDefaultEditorForBlog();
                UpdateTemplateToBlogTheme();
            }

            commandSemanticHtml.SetAccountId(_currentBlog.Id, IsRTLTemplate, false);

            // restore dirty state
            if (_currentEditor != null)
                _currentEditor.IsDirty = isDirty;
        }

        public void CreateTabs()
        {
            string[] tabNames = new string[_views.Length];
            string[] shortcuts = new string[_views.Length];
            for (int i = 0; i < tabNames.Length; i++)
            {
                tabNames[i] = _views[i].Text;
                if (_views[i].Shortcut != Shortcut.None)
                    shortcuts[i] = KeyboardHelper.FormatShortcutString(_views[i].Shortcut);
            }

            _postEditorFooter = new PostEditorFooter();
            _postEditorFooter.TabNames = tabNames;
            _postEditorFooter.Shortcuts = shortcuts;
            _postEditorFooter.Dock = DockStyle.Bottom;

            _postEditorFooter.SelectedTabChanged += tabsControl_SelectedTabChanged;
            _postEditorFooter.SetStatusMessage(Res.Get(StringId.StatusDraftUnsaved));

            _editorContainer.Controls.Add(_postEditorFooter);

        }

        void tabsControl_SelectedTabChanged(object sender, EventArgs args)
        {
            Command viewCommand = _views[_postEditorFooter.SelectedTabIndex];

            if (!viewCommand.Latched)
                viewCommand.PerformExecute();
        }

        public override bool ValidatePublish()
        {
            // property editors (categories)
            if (!_postPropertyEditor.ValidatePublish())
            {
                return false;
            }

            if (!CheckTagsReminder())
            {
                return false;
            }

            return base.ValidatePublish();
        }

        void _htmlEditorSidebarHost_VisibleChanged(object sender, EventArgs e)
        {
            //_editorCommandBar.DrawVerticalLine = _htmlEditorSidebarHost.Visible;
        }

        public override IStatusBar StatusBar
        {
            get
            {
                return new StatusBarShim(this);
            }
        }

        private class StatusBarShim : IStatusBar
        {
            private readonly BlogPostHtmlEditor parent;

            public StatusBarShim(BlogPostHtmlEditor parent)
            {
                this.parent = parent;
            }

            public void SetWordCountMessage(string msg)
            {
                parent._postEditorFooter.SetWordCountMessage(msg);
            }

            public void PushStatusMessage(string msg)
            {
                parent._postEditorFooter.PushStatusMessage(msg);
            }

            public void PopStatusMessage()
            {
                parent._postEditorFooter.PopStatusMessage();
            }

            public void SetStatusMessage(string msg)
            {
                parent._postEditorFooter.SetStatusMessage(msg);
            }
        }

        public static BlogPostHtmlEditor Create(IMainFrameWindow mainFrameWindow, Control editorContainer, IBlogPostEditingSite postEditingSite)
        {
            Panel panelBase = new Panel();
            panelBase.Dock = DockStyle.Fill;
            editorContainer.Controls.Add(panelBase);
            return new BlogPostHtmlEditor(mainFrameWindow, panelBase, postEditingSite);

        }

        private void InitializePropertyEditors()
        {
            _postPropertyEditor = new PostPropertiesBandControl(CommandManager);
            _postPropertyEditor.TabStop = true;
            _postPropertyEditor.TabIndex = 2;
            _postPropertyEditor.Dock = DockStyle.Top;
            _postPropertyEditor.AccessibleName = Res.Get(StringId.PropertiesPanel);
            _editorContainer.Controls.Add(_postPropertyEditor);
            Trace.WriteLine(_postPropertyEditor.Width + " " + _postPropertyEditor.Parent.Width);
        }

        void editorContainer_SizeChanged(object sender, EventArgs e)
        {
            //_editorCommandBar.VerticalLineX = (BidiHelper.IsRightToLeft ? _htmlEditorSidebarHost.Right - 1 : _htmlEditorSidebarHost.Left);
        }

        private void commandUpdateWeblogStyle_Execute(object sender, EventArgs e)
        {
            if (_postEditingSite.UpdateWeblogTemplate(_currentBlog.Id))
            {
                if (commandViewUseStyles.Latched)
                    ReloadEditor();
                else
                    commandViewUseStyles.PerformExecute();
            }
        }

        protected override void InitializeCommands()
        {
            CommandManager.BeginUpdate();
            base.InitializeCommands();

            CommandManager.Add(CommandId.UpdateWeblogStyle, commandUpdateWeblogStyle_Execute);

            commandViewUseStyles = CommandManager.Add(CommandId.ViewUseStyles, commandViewUseStyles_Execute);

            commandSemanticHtml = new SemanticHtmlGalleryCommand(CommandId.SemanticHtmlGallery, _postEditingSite, GetPreviewHtml, CommandManager, _currentEditor as IHtmlEditorComponentContext);
            commandSemanticHtml.ExecuteWithArgs += new ExecuteEventHandler(commandSemanticHtml_ExecuteWithArgs);
            commandSemanticHtml.ComponentContext = () => _currentEditor as IHtmlEditorComponentContext;
            CommandManager.Add(commandSemanticHtml);

            commandInsertablePlugins = new InsertablePluginsGalleryCommand();
            commandInsertablePlugins.ExecuteWithArgs += new ExecuteEventHandler(commandInsertablePlugins_ExecuteWithArgs);
            commandInsertablePlugins.LoadItems();
            CommandManager.Add(commandInsertablePlugins);

            commandInsertWebImage = new Command(CommandId.WebImage);
            commandInsertWebImage.Execute += new EventHandler(commandInsertWebImage_Execute);
            CommandManager.Add(commandInsertWebImage);

            commandInsertExtendedEntry = CommandManager.Add(CommandId.InsertExtendedEntry, commandInsertExtendedEntry_Execute);

            EditorLoaded += new EventHandler(ContentEditor_EditorHtmlReloaded);

            // QAT
            CommandManager.Add(new GalleryCommand<CommandId>(CommandId.QAT));

            // Outspace
            CommandManager.Add(new RecentItemsCommand(_postEditingSite));

            CommandManager.Add(new GroupCommand(CommandId.InsertImageSplit, CommandManager.Get(CommandId.InsertPictureFromFile)));

            // WinLive 181138 - A targetted fix to ensure the InsertVideoSplit command is disabled if we don't support InsertVideo (e.g zh-CN doesn't support video)
            // The problem is related to Windows 7 #712524 & #758433 and this is a work around for this particular case.
            // The dropdown commands for this (InsertVideoFromFile etc) are already disabled based on the feature support. We explicitly set the state of
            // group command here so that it has the right state to begin with (otherwise a switch tab/app is required to refresh).
            GroupCommand commandInsertVideoSplit = new GroupCommand(CommandId.InsertVideoSplit, CommandManager.Get(CommandId.InsertVideoFromFile));
            commandInsertVideoSplit.Enabled = MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.VideoProviders);
            CommandManager.Add(commandInsertVideoSplit);

            foreach (CommandId commandId in new CommandId[] {
                CommandId.SplitNew,
                CommandId.SplitSave,
                CommandId.SplitPrint,
                CommandId.PasteSplit,
                CommandId.FormatTablePropertiesSplit})
            {
                CommandManager.Add(new Command(commandId));
            }

            _commandClosePreview = CommandManager.Add(CommandId.ClosePreview, commandClosePreview_Execute, false);

            CommandManager.EndUpdate();
        }

        private Command _commandClosePreview;

        protected override void ManageCommandsForEditingMode()
        {
            base.ManageCommandsForEditingMode();

            bool allowInsertCommands = (CurrentEditingMode == EditingMode.Source || CurrentEditingMode == EditingMode.Wysiwyg);

            // Enable/disable heading styles
            commandSemanticHtml.Enabled = allowInsertCommands;

            // Enable/disable 3rd party plugins
            commandInsertablePlugins.Enabled = allowInsertCommands;

            // Enable/disable inserting web images
            commandInsertWebImage.Enabled = allowInsertCommands;

            _commandClosePreview.Enabled = CurrentEditingMode == EditingMode.Preview;
        }

        void commandInsertablePlugins_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            string pluginId = commandInsertablePlugins.Items[args.GetInt(commandInsertablePlugins.CommandId.ToString())].Cookie;
            Command command = CommandManager.Get(pluginId);
            command.PerformExecute();
        }

        void commandInsertWebImage_Execute(object sender, EventArgs e)
        {
            CommandManager.Get(ImageInsertion.WebImages.WebImageContentSource.ID).PerformExecute();
        }

        void commandClosePreview_Execute(object sender, EventArgs e)
        {
            switch (LastNonPreviewEditingMode)
            {
                case EditingMode.Source:
                    ChangeToCodeMode();
                    break;
                case EditingMode.Wysiwyg:
                    ChangeToWysiwygMode();
                    break;
                case EditingMode.PlainText:
                    ChangeToPlainTextMode();
                    break;
                default:
                    break;
            }
        }

        private InsertablePluginsGalleryCommand commandInsertablePlugins;
        private Command commandInsertWebImage;
        protected override void ContentEditor_SelectionChanged(object sender, EventArgs e)
        {
            // For managing Writer-specific commands
            commandInsertablePlugins.Enabled = InSourceOrWysiwygModeAndEditFieldIsNotSelected();
            commandInsertWebImage.Enabled = InSourceOrWysiwygModeAndEditFieldIsNotSelected();

            base.ContentEditor_SelectionChanged(sender, e);
        }

        private void commandInsertExtendedEntry_Execute(object sender, EventArgs e)
        {
            _currentEditor.InsertExtendedEntryBreak();
        }

        void ContentEditor_EditorHtmlReloaded(object sender, EventArgs e)
        {
            UpdateExtendedEntryStatus();
        }

        void UpdateExtendedEntryStatus()
        {
            //toggle commands based on the new blog's capabilities
            commandInsertExtendedEntry.Enabled = SupportsExtendedEntries;
            /*
            CT: Bug 607202 - just disable this command so we're consistent with the toolbar, which also just disables.
            if (commandInsertExtendedEntry.On != SupportsExtendedEntries)
            {
                commandInsertExtendedEntry.On = SupportsExtendedEntries;
                CommandManager.OnChanged(EventArgs.Empty);
            }
            */
        }

        private bool SupportsExtendedEntries
        {
            get
            {
                return _currentBlog.ClientOptions.SupportsExtendedEntries && !_isPage;
            }
        }

        private void commandViewUseStyles_Execute(object sender, EventArgs e)
        {
            using (PostHtmlEditingSettings editSettings = new PostHtmlEditingSettings(_currentBlog.Id))
            {
                commandViewUseStyles.Latched = !commandViewUseStyles.Latched;
                editSettings.EditUsingBlogStyles = commandViewUseStyles.Latched;
                ShowWebLayoutWarningIfNecessary();
                // When we update the editors theme because the user toggled 'Edit using Themes'
                // we suppress the editor reload.  The reload will happen in the following call to ChangeToWysiwygMode()
                using (SuppressEditorLoad())
                    UpdateTemplateToBlogTheme();
                ReloadEditor();
            }
        }

        private void InitDefaultEditorForBlog()
        {
            using (PostHtmlEditingSettings editSettings = new PostHtmlEditingSettings(_currentBlog.Id))
            {
                // initialize the editing template based on the last used view
                bool useStyles = EditUsingWebLayout(editSettings);
                commandViewUseStyles.Latched = useStyles;
            }
        }

        private bool EditUsingWebLayout(PostHtmlEditingSettings editSettings)
        {
            if (!editSettings.EditUsingBlogStylesIsSet && string.IsNullOrEmpty(editSettings.LastEditingView))
                editSettings.EditUsingBlogStyles = _currentBlog.DefaultView != EditingViews.Normal;

            return editSettings.EditUsingBlogStyles;
        }

        private void ShowWebLayoutWarningIfNecessary()
        {
            // if the blog's DefaultView is not WebLayout and the user has not chosen
            // to supress the web-layout warning dialog then show a warning prior
            // to proceeding
            if (_currentBlog.DefaultView != EditingViews.WebLayout)
            {
                using (PostHtmlEditingSettings editSettings = new PostHtmlEditingSettings(_currentBlog.Id))
                {
                    if (editSettings.DisplayWebLayoutWarning)
                    {
                        using (WebLayoutViewWarningForm warningForm = new WebLayoutViewWarningForm())
                        {
                            warningForm.ShowDialog(_mainFrameWindow);
                            if (warningForm.DontShowMessageAgain)
                                editSettings.DisplayWebLayoutWarning = false;
                        }
                    }
                }
            }
        }

        private void UpdateTemplateToBlogTheme()
        {
            SetTheme(GetStyledHtml(), GetPreviewHtml(), true);
        }

        protected override void BeforeSetTheme(ref string wysiwygHTML, ref string previewHTML, bool containsTitle)
        {
            // Remove a very common snippet of code from the theme that has been copied and pasted
            // into a lot of the blogger templates.  The snippet of CSS should fix the user's theme for IE5
            // however, when inside of Writer causes problems with new lines in the editor.
            wysiwygHTML = Regex.Replace(wysiwygHTML, "\\.post-body\\s+p\\s+\\{\\s+/\\*\\s+Fix\\s+bug\\s+in\\s+IE5/Win\\s+with\\s+italics\\s+in\\s+posts\\s+\\*/\\s+margin:\\s+0px\\s+0px\\s+0px\\s+0px;\\s+padding:\\s+3px\\s+0px\\s+3px\\s+0px;\\s+display:\\s+inline;\\s+/\\*\\s+to\\s+fix\\s+floating-ads\\s+wrapping\\s+problem\\s+in\\s+IE\\s+\\*/\\s+height:\\s+1%;\\s+overflow:\\s+visible;\\s*}", "");
            // Remove noscript tags from the editing template.  If a user has a blog theme of
            // <html><head><noscript></noscript></head><body><div></div></body></html> we will parse it to
            // <html><head><noscript></head><body><div></div></body></noscript></html> and this code will change it to
            // <html><head></head><body><div></div></body></html> which will allow us to attach our beavhiors to the body element
            wysiwygHTML = Regex.Replace(wysiwygHTML, "</?NOSCRIPT>", "", RegexOptions.IgnoreCase);
            // Remove any scroll=no attributes. Sharepoint 2010 adds these.
            wysiwygHTML = Regex.Replace(wysiwygHTML, "scroll=[\"']?no[\"']?", "", RegexOptions.IgnoreCase);
        }

        private string GetStyledHtml()
        {
            BlogEditingTemplateType type;

            if (commandViewUseStyles.Latched)
                type = BlogEditingTemplateType.Framed;
            else
                type = BlogEditingTemplateType.Normal;

            return EditingTemplateLoader.LoadBlogTemplate(_currentBlog.Id, type, IsRTLTemplate);
        }

        protected override string GetPreviewHtml()
        {
            return GetPreviewHtml(_currentBlog.Id);
        }

        private string GetPreviewHtml(string blogId)
        {
            return EditingTemplateLoader.LoadBlogTemplate(blogId, BlogEditingTemplateType.Webpage, IsRTLTemplate);
        }

        protected override string GetPostBodyInlineStyleOverrides()
        {
            return "min-height: 400px;";
        }

        // @SharedCanvas - is there a better way to do this using the CE?
        public override IFocusableControl[] GetFocusablePanes()
        {
            return new IFocusableControl[]
            {
                EditorFocusControl,
                new FocusableControl(_postPropertyEditor),
                new FocusableControl(_htmlEditorSidebarHost)
            };
        }

        internal class BlogPostTemplateStrategy : BlogPostHtmlEditorControl.TemplateStrategy
        {
            public override string OnBodyInserted(string bodyContents)
            {
                return String.Format(CultureInfo.InvariantCulture, "<div id=\"{0}\" class='postBody' style='margin: 4px 0px 0px 0px; padding: 0px 0px 0px 0px; border: 0px;'>{1}</div>", BODY_FRAGMENT_ID, bodyContents); ;
            }

            public override string OnTitleInserted(string title)
            {
                return String.Format(CultureInfo.InvariantCulture, "<span id=\"{0}\" class='postTitle' style='margin: 0px 0px 0px 0px; padding: 0px 0px 0px 0px; border: 0px;'>{1}</span>", TITLE_FRAGMENT_ID, HtmlUtils.EscapeEntities(title));
            }

            public override void OnDocumentComplete(IHTMLDocument2 doc)
            {
                return;
            }

            public override IHTMLElement PostBodyElement(IHTMLDocument2 doc)
            {
                return HTMLElementHelper.GetFragmentElement((IHTMLDocument3)doc, BODY_FRAGMENT_ID);
            }

            public override IHTMLElement TitleElement(IHTMLDocument2 doc)
            {
                return HTMLElementHelper.GetFragmentElement((IHTMLDocument3)doc, TITLE_FRAGMENT_ID);
            }
        }
    }
}
