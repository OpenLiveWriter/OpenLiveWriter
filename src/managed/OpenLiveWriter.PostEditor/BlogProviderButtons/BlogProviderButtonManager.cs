// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.PostEditor.Commands;

// P0 TODO
// ==========================================================================================

// M1:
//    - Syndication docs for partners

// M2:
//    - Initial user experience / spaces signup
//    - Embeds/Video/LifeCam (see Flash Embedding Cage Match: http://alistapart.com/articles/flashembedcagematch)
//    - Product vision

// RADAR:
//    - XHTML
//    - Tagging (Spaces and WordPress)
//    - Atom Publishing Protocol
//    - Image Sidebar (UX, effects, etc.)
//    - Data Model (recently posted, online vs. offline, etc.)
//    - Auto linking
//    - Tray icon / notification
//    - Category control bug farm
//    - Control model/architecture: DIV, ActiveX, WinForms, etc.
//    - Me Control / Live sign-in
//    - Generic editor: simple article-based CMS
//    - Generic editor: bootstrap from web page

namespace OpenLiveWriter.PostEditor.BlogProviderButtons
{
    // BlogProviderButtonCommandBarInfo is defined in BlogProviderButtonCommandBar.cs

    internal class BlogProviderButtonManager : GalleryCommand<Command>, IDisposable
    {
        // The first two items in the gallery are:
        // 1. The blog home page
        // 2. The blog admin page
        // The values are available through the editing manager.
        private object _commandsLock = new object();

        public BlogProviderButtonManager(CommandManager commandManager) : base(CommandId.BlogProviderButtonsGallery, false)
        {
            AllowSelection = false;

            lock (_commandsLock)
            {
                // initialize commands
                for (int i = 0; i < _commands.Length; i++)
                {
                    _commands[i] = new Command();
                    _commands[i].Identifier = String.Format(CultureInfo.InvariantCulture, BlogProviderButtonCommandBarInfo.ProviderCommandFormat, i);
                    _commands[i].Execute += new EventHandler(BlogProviderButton_Execute);
                    _commands[i].CommandBarButtonStyle = CommandBarButtonStyle.Provider;
                    _commands[i].On = false;
                }

                ExecuteWithArgs += new ExecuteEventHandler(BlogProviderButtonManager_ExecuteWithArgs);

                // add them to the command manager
                commandManager.Add(new CommandCollection(_commands));

                commandManager.Add(this);

            }

            commandViewWeblog = commandManager.Add(CommandId.ViewWeblog, commandViewWeblog_Execute);
            commandViewWeblogAdmin = commandManager.Add(CommandId.ViewWeblogAdmin, commandViewWeblogAdmin_Execute);

        }

        void BlogProviderButtonManager_ExecuteWithArgs(object sender, EventArgs e)
        {
            lock (_commandsLock)
            {
                if (SelectedIndex < commandsOffset)
                {
                    // Execute the appropriate command that we prepended to the list of provider buttons
                    if (SelectedIndex == 0)
                    {
                        commandViewWeblog.PerformExecute();
                    }
                    else
                    {
                        commandViewWeblogAdmin.PerformExecute();
                    }
                }
                else
                {
                    _commands[SelectedIndex - commandsOffset].PerformExecute();
                }
            }
        }

        public void Initialize(Control synchronizeInvokeControl, BlogPostEditingManager editingManager)
        {
            // initialize notification sink
            _notificationSink = new BlogProviderButtonNotificationSink(synchronizeInvokeControl);
            _notificationSink.BlogProviderButtonNotificationReceived += new BlogProviderButtonNotificationReceivedHandler(_notificationSink_BlogProviderButtonNotificationReceived);
            _notificationSink.CheckForNotifications();

            // save a reference to the editing manager and subscribe to change notifications
            _editingManager = editingManager;
            _editingManager.BlogChanged += new EventHandler(_editingManager_BlogChanged);
            _editingManager.BlogSettingsChanged += new WeblogSettingsChangedHandler(_editingManager_BlogSettingsChanged);

            // connect to the current weblog
            if (editingManager.BlogId != String.Empty)
                ConnectToBlog(editingManager.BlogId);
        }

        private void ConnectToBlog(string blogId)
        {
            lock (_commandsLock)
            {
                if (BlogSettings.BlogIdIsValid(blogId))
                {
                    using (Blog blog = new Blog(blogId))
                        ConnectToBlog(blog);
                }
                else
                {
                    ClearBlogProvider();
                }
            }
        }

        private string blogAdminUrl;
        private int commandsOffset;
        private Command commandViewWeblog;
        private Command commandViewWeblogAdmin;

        /// <summary>
        /// Must be called under the protection of _commandsLock.
        /// </summary>
        /// <param name="blog"></param>
        private void ConnectToBlog(Blog blog)
        {
            items.Clear();
            UpdateInvalidationState(PropertyKeys.ItemsSource, InvalidationState.Pending);
            InvalidateSelectedItemProperties();

            // always clear existing providers to start
            ClearBlogProvider();

            blogAdminUrl = _editingManager.BlogAdminUrl;

            // Add the homepage link
            commandsOffset = 1;
            string homepageLinkText = _editingManager.Blog.ClientOptions.HomepageLinkText;
            items.Add(new GalleryItem(!String.IsNullOrEmpty(homepageLinkText) ? homepageLinkText : Res.Get(StringId.ViewWeblog), Images.BlogWebPreview_SmallImage, commandViewWeblog));

            // Add the admin link, if we've got one.
            if (blogAdminUrl != String.Empty)
            {
                commandsOffset++;
                // @RIBBON TODO: Add an icon for the admin link item
                string adminLinkText = _editingManager.Blog.ClientOptions.AdminLinkText;
                items.Add(new GalleryItem(!String.IsNullOrEmpty(adminLinkText) ? adminLinkText : Res.Get(StringId.ManageWeblog), Images.BlogAccount_SmallImage, commandViewWeblogAdmin));
            }

            if (PostEditorSettings.AllowProviderButtons)
            {
                // Now add the custom blog provider buttons
                IBlogProviderButtonDescription[] providerButtonDescriptions = blog.ButtonDescriptions;
                if (providerButtonDescriptions.Length > 0)
                {
                    // create buttons and attach to commands
                    for (int i = 0; (i < providerButtonDescriptions.Length) && (i < _commands.Length); i++)
                    {
                        // create button
                        BlogProviderButton providerButton = new BlogProviderButton(blog.Id, blog.HostBlogId, blog.HomepageUrl, blog.PostApiUrl, providerButtonDescriptions[i].Id);

                        // notify button we are connecting (allows it to reset notification image and
                        // force an immediate notification check)
                        providerButton.ConnectToFrame();

                        // connect button to frame
                        _commands[i].On = true;
                        _commands[i].Tag = providerButton;
                        _commands[i].Enabled = PostEditorSettings.AllowProviderButtons;
                        _commands[i].AccessibleName = providerButton.CurrentText;
                        _commands[i].LabelTitle = providerButton.CurrentText;
                        _commands[i].LargeImage = providerButton.CurrentImage;

                        // @RIBBON TODO: Cleanly remove obsolete code
                        string clippedName = TextHelper.GetTitleFromText(providerButton.CurrentText, RibbonHelper.GalleryItemTextMaxChars, TextHelper.Units.Characters);
                        Bitmap bitmap = (providerButton.CurrentImage != null) ? new Bitmap(providerButton.CurrentImage) : null;
                        items.Add(new GalleryItem(clippedName, bitmap, _commands[i]));

                        if (providerButton.SupportsContent)
                        {
                            // install drop down content source
                            _commands[i].CommandBarButtonContextMenuHandler = new CommandBarButtonContextMenuHandler(
                                new BlogProviderContentViewer(providerButton).ViewContent);

                            // make separate drop down for split buttons
                            if (providerButton.SupportsClick)
                                _commands[i].CommandBarButtonContextMenuDropDown = true;
                            else
                                _commands[i].CommandBarButtonContextMenuDropDown = false;
                        }
                        else
                        {
                            RemoveDropDownMenu(_commands[i]);
                        }
                    }

                    // attach notification sink
                    _notificationSink.Attach(blog);
                }
                OnStateChanged(EventArgs.Empty);
            }

        }

        /// <summary>
        /// Must be called under the protection of _commandsLock.
        /// </summary>
        private void ClearBlogProvider()
        {
            // detach from the notification sink
            _notificationSink.Detach();

            // turn off all of the commands
            foreach (Command command in _commands)
                DisableCommand(command);
        }

        /// <summary>
        /// Must be called under the protection of _commandsLock.
        /// </summary>
        /// <param name="command"></param>
        private void DisableCommand(Command command)
        {
            command.On = false;
            if (command.Tag != null)
            {
                // optionally dispose
                IDisposable disposableTag = command.Tag as IDisposable;
                if (disposableTag != null)
                    disposableTag.Dispose();

                command.Tag = null;
            }
            command.CommandBarButtonBitmapEnabled = null;
            command.Text = String.Empty;
            RemoveDropDownMenu(command);
        }

        /// <summary>
        /// Must be called under the protection _commandsLock.
        /// </summary>
        /// <param name="command"></param>
        private void RemoveDropDownMenu(Command command)
        {
            command.CommandBarButtonContextMenuHandler = null;
            command.CommandBarButtonContextMenuDropDown = false;
        }

        private void _editingManager_BlogChanged(object sender, EventArgs e)
        {
            ConnectToBlog(_editingManager.BlogId);
        }

        private void _editingManager_BlogSettingsChanged(string blogId, bool templateChanged)
        {
            ConnectToBlog(blogId);
        }

        private void BlogProviderButton_Execute(object sender, EventArgs e)
        {
            lock (_commandsLock)
            {
                // get the command and frame button
                Command command = sender as Command;
                BlogProviderButton blogProviderButton = command.Tag as BlogProviderButton;

                // notify the button that it is being clicked
                blogProviderButton.RecordButtonClicked();

                // launch the browser
                if (!String.IsNullOrEmpty(blogProviderButton.ClickUrl))
                    ShellHelper.LaunchUrl(blogProviderButton.ClickUrl);
                else if (!String.IsNullOrEmpty(blogProviderButton.ContentUrl))
                    ShellHelper.LaunchUrl(blogProviderButton.ContentUrl);
            }
        }

        private void _notificationSink_BlogProviderButtonNotificationReceived(string blogId, string buttonId)
        {
            lock (_commandsLock)
            {
                // see if we are currently managing this button (can get "stale" notifications if they
                // come in from a background thread after we have switched blogs)
                if (_editingManager != null && _editingManager.BlogId == blogId)
                {
                    bool refreshRequired = false;
                    // Find the gallery item corresponding to this buttonId
                    foreach (GalleryItem item in items)
                    {
                        Command command = item.Cookie;
                        if (command != null)
                        {
                            BlogProviderButton providerButton = command.Tag as BlogProviderButton;
                            if (providerButton != null && providerButton.Id == buttonId)
                            {
                                command.LargeImage = providerButton.CurrentImage;
                                command.LabelTitle = providerButton.CurrentText;
                                command.CommandBarButtonBitmapEnabled = providerButton.CurrentImage;
                                command.Text = providerButton.CurrentText;
                                command.AccessibleName = providerButton.CurrentText;

                                // GalleryItem calls dispose on the bitmap, so give it a new copy
                                item.Image = new Bitmap(providerButton.CurrentImage);
                                item.Label = TextHelper.GetTitleFromText(providerButton.CurrentText, RibbonHelper.GalleryItemTextMaxChars, TextHelper.Units.Characters);

                                // We are done updating the gallery item, flag for refresh
                                refreshRequired = true;
                                break;
                            }
                        }
                    }

                    // Reload the shortcuts gallery to actually see any changes made above.
                    // Invalidating 'ItemsSource' property will cause ribbon to reload items
                    if (refreshRequired)
                    {
                        Invalidate(new PropertyKey[] { PropertyKeys.ItemsSource });
                    }
                }
            }
        }

        private void commandViewWeblog_Execute(object sender, EventArgs e)
        {
            ShellHelper.LaunchUrl(_editingManager.BlogHomepageUrl);
        }

        private void commandViewWeblogAdmin_Execute(object sender, EventArgs e)
        {
            ShellHelper.LaunchUrl(_editingManager.BlogAdminUrl);
        }

        public void Dispose()
        {
            if (_editingManager != null)
            {
                _editingManager.BlogChanged -= new EventHandler(_editingManager_BlogChanged);
                _editingManager = null;
            }

            lock (_commandsLock)
            {
                // dispose any objects attached to the Tag property of our commands
                foreach (Command command in _commands)
                {
                    IDisposable disposableTag = command.Tag as IDisposable;
                    if (disposableTag != null)
                        disposableTag.Dispose();
                }
            }

            if (_notificationSink != null)
            {
                _notificationSink.BlogProviderButtonNotificationReceived -= new BlogProviderButtonNotificationReceivedHandler(_notificationSink_BlogProviderButtonNotificationReceived);
                _notificationSink.Dispose();
            }
        }

        public override void LoadItems()
        {
        }

        private BlogPostEditingManager _editingManager;

        private Command[] _commands = new Command[BlogProviderButtonCommandBarInfo.MaximumProviderCommands];

        private BlogProviderButtonNotificationSink _notificationSink;
    }

    internal class BlogProviderContentViewer
    {
        public BlogProviderContentViewer(BlogProviderButton button)
        {
            _button = button;
        }

        public void ViewContent(Control parent, Point menuLocation, int alternativeLocation, IDisposable disposeWhenDone)
        {
            try
            {
                if (!WinInet.InternetConnectionAvailable)
                {
                    disposeWhenDone.Dispose();
                    DisplayMessage.Show(MessageId.InternetConnectionWarning);
                    return;
                }

                // track resource that need to be disposed
                _disposeWhenDone = disposeWhenDone;

                using (new WaitCursor())
                {
                    // if the user is holding the CTRL button down then invalidate the cache
                    bool forceResynchronize = (KeyboardHelper.GetModifierKeys() == Keys.Control);

                    // setup download options
                    int downloadOptions =
                        DLCTL.DLIMAGES |
                        DLCTL.NO_CLIENTPULL |
                        DLCTL.NO_BEHAVIORS |
                        DLCTL.NO_DLACTIVEXCTLS |
                        DLCTL.SILENT;
                    if (forceResynchronize)
                        downloadOptions |= DLCTL.RESYNCHRONIZE;

                    // determine cookies and/or network credentials
                    WinInetCredentialsContext credentialsContext = null;
                    try
                    {
                        credentialsContext = BlogClientHelper.GetCredentialsContext(_button.BlogId, _button.ContentUrl);
                    }
                    catch (BlogClientOperationCancelledException)
                    {
                        _disposeWhenDone.Dispose();
                        return;
                    }

                    // note that we have viewed the content
                    _button.RecordButtonClicked();

                    // create the form and position it
                    BrowserMiniForm form = new BrowserMiniForm(_button.ContentQueryUrl, downloadOptions, credentialsContext);
                    form.StartPosition = FormStartPosition.Manual;
                    form.Size = _button.ContentDisplaySize;
                    if (!Screen.FromPoint(menuLocation).Bounds.Contains(menuLocation.X + form.Width, menuLocation.Y))
                        menuLocation.X = alternativeLocation - form.Width;
                    form.Location = new Point(menuLocation.X + 1, menuLocation.Y);

                    // subscribe to close event for disposal
                    form.Closed += new EventHandler(form_Closed);

                    // float above parent if we have one
                    IMiniFormOwner miniFormOwner = parent.FindForm() as IMiniFormOwner;
                    if (miniFormOwner != null)
                        form.FloatAboveOwner(miniFormOwner);

                    // show the form
                    form.Show();
                }
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

        private readonly BlogProviderButton _button;
        private IDisposable _disposeWhenDone;
    }

}
