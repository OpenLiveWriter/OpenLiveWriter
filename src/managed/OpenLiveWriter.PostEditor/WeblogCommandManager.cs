// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.Configuration.Settings;

// @RIBBON TODO: Cleanly remove obsolete code.

namespace OpenLiveWriter.PostEditor
{
    public class SelectBlogGalleryCommand : SelectGalleryCommand<string>
    {
        private readonly IBlogPostEditingManager _editingManager;
        public SelectBlogGalleryCommand(IBlogPostEditingManager editingManager) : base(CommandId.SelectBlog)
        {
            _invalidateGalleryRepresentation = true;
            _editingManager = editingManager;
        }

        public override void LoadItems()
        {
            items.Clear();

            int i = 0;
            string currentBlogId = _editingManager.CurrentBlog();
            foreach (BlogDescriptor blogDescriptor in BlogSettings.GetBlogs(true))
            {
                using (Blog blog = new Blog(blogDescriptor.Id))
                {
                    string blogName = GetShortenedBlogName(blog.Name);

                    if (blog.Image == null)
                    {
                        Bitmap defaultIcon = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.BlogAccount.png");
                        items.Add(new GalleryItem(blogName, defaultIcon.Clone() as Bitmap, blog.Id));
                    }
                    else
                    {
                        items.Add(new GalleryItem(blogName, new Bitmap(blog.Image), blog.Id));
                    }

                    if (currentBlogId == blog.Id)
                        selectedIndex = i;
                }
                i++;
            }
            base.LoadItems();
        }

        public override void Invalidate()
        {
            if (items.Count == 0)
            {
                LoadItems();
                UpdateInvalidationState(PropertyKeys.ItemsSource, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.Categories, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.SelectedItem, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.StringValue, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.Label, InvalidationState.Pending);
                OnStateChanged(EventArgs.Empty);
            }
            else
            {
                UpdateSelectedIndex();
            }
        }

        internal void ReloadAndInvalidate()
        {
            items.Clear();
            Invalidate();
        }

        private void UpdateSelectedIndex()
        {
            selectedIndex = INVALID_INDEX;
            SetSelectedItem(_editingManager.CurrentBlog());
        }

        public static string GetShortenedBlogName(string blogName)
        {
            return TextHelper.GetTitleFromText(blogName, 100, TextHelper.Units.Characters);
        }
    }

    internal class WeblogCommandManager : IDynamicCommandMenuContext, IDisposable
    {
        public WeblogCommandManager(BlogPostEditingManager editingManager, IBlogPostEditingSite editingSite)
        {
            // save reference to editing context and subscribe to blog-changed event
            _editingManager = editingManager;
            _editingManager.BlogChanged += new EventHandler(_editingManager_BlogChanged);
            _editingManager.BlogSettingsChanged += new WeblogSettingsChangedHandler(_editingManager_BlogSettingsChanged);
            
            _editingSite = editingSite;

            BlogSettings.BlogSettingsDeleted += new BlogSettings.BlogSettingsListener(BlogSettings_BlogSettingsDeleted);

            // initialize commands
            InitializeCommands();

            // initialize UI
            InitializeUI();
        }

        private void EditingSiteOnWeblogListChanged(object sender, EventArgs eventArgs)
        {
            commandSelectBlog?.ReloadAndInvalidate();
        }

        void BlogSettings_BlogSettingsDeleted(string blogId)
        {
            commandSelectBlog.ReloadAndInvalidate();
        }

        /// <summary>
        /// Notification that the user has selected a weblog menu
        /// </summary>
        public event WeblogHandler WeblogSelected;

        private void InitializeCommands()
        {
            CommandManager.BeginUpdate();

            commandWeblogPicker = new CommandWeblogPicker();
            _editingSite.CommandManager.Add(commandWeblogPicker);
            _editingSite.WeblogListChanged -= EditingSiteOnWeblogListChanged;
            _editingSite.WeblogListChanged += EditingSiteOnWeblogListChanged;

            commandAddWeblog = new Command(CommandId.AddWeblog);
            commandAddWeblog.Execute += new EventHandler(commandAddWeblog_Execute);
            CommandManager.Add(commandAddWeblog);

            commandConfigureWeblog = new Command(CommandId.ConfigureWeblog);
            commandConfigureWeblog.Execute += new EventHandler(commandConfigureWeblog_Execute);
            CommandManager.Add(commandConfigureWeblog);

            commandSelectBlog = new SelectBlogGalleryCommand(_editingManager);
            commandSelectBlog.ExecuteWithArgs += new ExecuteEventHandler(commandSelectBlog_ExecuteWithArgs);
            commandSelectBlog.Invalidate();
            CommandManager.Add(commandSelectBlog);

            CommandManager.EndUpdate();

            // create the dynamic menu items that correspond to the available weblogs
            _switchWeblogCommandMenu = new DynamicCommandMenu(this);
        }

        void commandSelectBlog_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
        {
            if (WeblogSelected != null)
                WeblogSelected(BlogSettings.GetBlogs(true)[commandSelectBlog.SelectedIndex].Id);
        }

        private void InitializeUI()
        {
            // hookup menu definition to command bar button
            commandWeblogPicker.CommandBarButtonContextMenuDefinition = GetWeblogContextMenuDefinition(false);
        }

        private CommandContextMenuDefinition GetWeblogContextMenuDefinition(bool includeAllCommands)
        {
            // initialize contenxt-menu definition
            CommandContextMenuDefinition weblogContextMenuDefinition = new CommandContextMenuDefinition(this.components);
            weblogContextMenuDefinition.CommandBar = true;

            if (includeAllCommands)
            {
                weblogContextMenuDefinition.Entries.Add(CommandId.ViewWeblog, false, false);
                weblogContextMenuDefinition.Entries.Add(CommandId.ViewWeblogAdmin, false, false);
                weblogContextMenuDefinition.Entries.Add(CommandId.ConfigureWeblog, true, true);
            }

            // weblog switching commands
            foreach (string commandIdentifier in _switchWeblogCommandMenu.CommandIdentifiers)
                weblogContextMenuDefinition.Entries.Add(commandIdentifier, false, false);

            weblogContextMenuDefinition.Entries.Add(CommandId.AddWeblog, true, false);
            return weblogContextMenuDefinition;
        }

        private void _editingManager_BlogChanged(object sender, EventArgs e)
        {
            commandSelectBlog.Invalidate();
            UpdateWeblogPicker();
        }

        private void _editingManager_BlogSettingsChanged(string blogId, bool templateChanged)
        {
            using (Blog blog = new Blog(blogId))
            {
                // Find the item that is changing.
                var blogItem = commandSelectBlog.Items.Find(item => item.Cookie.Equals(blogId, StringComparison.Ordinal));

                if (blogItem != null &&
                    !blogItem.Label.Equals(SelectBlogGalleryCommand.GetShortenedBlogName(blog.Name), StringComparison.Ordinal))
                {
                    // The blog name has changed so we need to do a full reload to refresh the UI.
                    commandSelectBlog.ReloadAndInvalidate();
                }
                else
                {
                    // WinLive 43696: The blog settings have changed, but the UI doesn't need to be refreshed. In
                    // order to avoid "Windows 8 Bugs" 43242, we don't want to do a full reload.
                    commandSelectBlog.Invalidate();
                }
            }

            UpdateWeblogPicker();
        }

        private void UpdateWeblogPicker()
        {
            Control c = (Control)_editingSite;
            CommandWeblogPicker.WeblogPicker wpbc = new CommandWeblogPicker.WeblogPicker(
                Res.DefaultFont,
                _editingManager.BlogImage,
                _editingManager.BlogIcon,
                _editingManager.BlogServiceDisplayName,
                _editingManager.BlogName);
            using (Graphics g = c.CreateGraphics())
            {
                wpbc.Layout(g);
                commandWeblogPicker.WeblogPickerHelper = wpbc;
            }
        }

        private void commandConfigureWeblog_Execute(object sender, EventArgs e)
        {
            _editingSite.ConfigureWeblog(_editingManager.BlogId, typeof(AccountPanel));
        }

        private void commandAddWeblog_Execute(object sender, EventArgs e)
        {
            _editingSite.AddWeblog();
        }

        public void Dispose()
        {
            if (_editingManager != null)
            {
                _editingManager.BlogChanged -= new EventHandler(_editingManager_BlogChanged);
                _editingManager = null;

                BlogSettings.BlogSettingsDeleted -= new BlogSettings.BlogSettingsListener(BlogSettings_BlogSettingsDeleted);
            }

            _switchWeblogCommandMenu.Dispose();

            if (components != null)
                components.Dispose();
        }

        public CommandManager CommandManager
        {
            get
            {
                return _editingSite.CommandManager;
            }
        }

        DynamicCommandMenuOptions IDynamicCommandMenuContext.Options
        {
            get
            {
                if (_options == null)
                {
                    _options = new DynamicCommandMenuOptions(
                        new Command(CommandId.ViewWeblog).MainMenuPath.Split('/')[0], 100, Res.Get(StringId.MoreWeblogs), Res.Get(StringId.SwitchWeblogs));
                    _options.UseNumericMnemonics = false;
                    _options.MaxCommandsShownOnMenu = 25;
                    _options.SeparatorBegin = true;
                }
                return _options;
            }
        }
        private DynamicCommandMenuOptions _options;


        IMenuCommandObject[] IDynamicCommandMenuContext.GetMenuCommandObjects()
        {
            // generate an array of command objects for the current list of weblogs
            ArrayList menuCommands = new ArrayList();
            foreach (BlogDescriptor blog in BlogSettings.GetBlogs(true))
                menuCommands.Add(new SwitchWeblogMenuCommand(blog.Id, blog.Id == _editingManager.BlogId));
            return (IMenuCommandObject[])menuCommands.ToArray(typeof(IMenuCommandObject));
        }

        void IDynamicCommandMenuContext.CommandExecuted(IMenuCommandObject menuCommandObject)
        {
            // get reference to underlying command object
            SwitchWeblogMenuCommand menuCommand = menuCommandObject as SwitchWeblogMenuCommand;

            // fire notification to listeners
            if (WeblogSelected != null)
                WeblogSelected(menuCommand.BlogId);
        }

        private DynamicCommandMenu _switchWeblogCommandMenu;

        private BlogPostEditingManager _editingManager;
        private IBlogPostEditingSite _editingSite;

        private CommandWeblogPicker commandWeblogPicker;
        private Command commandConfigureWeblog;
        private SelectBlogGalleryCommand commandSelectBlog;
        private Command commandAddWeblog;

        private IContainer components = new Container();

        private class SwitchWeblogMenuCommand : IMenuCommandObject
        {
            public SwitchWeblogMenuCommand(string blogId, bool latched)
            {
                _blogId = blogId;
                using (BlogSettings settings = BlogSettings.ForBlogId(_blogId))
                    _caption = StringHelper.Ellipsis(settings.BlogName + "", 65);
                _latched = latched;
            }

            public string BlogId { get { return _blogId; } }
            private string _blogId;

            Bitmap IMenuCommandObject.Image { get { return null; } }

            string IMenuCommandObject.Caption { get { return _caption; } }
            string IMenuCommandObject.CaptionNoMnemonic { get { return _caption; } }
            private string _caption;

            bool IMenuCommandObject.Latched { get { return _latched; } }
            private bool _latched;

            bool IMenuCommandObject.Enabled { get { return true; } }
        }

    }
}
