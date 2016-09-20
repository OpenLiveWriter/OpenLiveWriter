// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.ContentSources;
using OpenLiveWriter.PostEditor.OpenPost;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using System.Net;

namespace OpenLiveWriter.PostEditor
{
    public interface IBlogPostEditingManager
    {
        void SwitchBlog(string blogId);
        string CurrentBlog();
    }

    public class BlogPostEditingManager : IBlogPostEditingContext, IBlogPostPublishingContext, IDisposable, IBlogPostEditingManager
    {
        #region Initialization and Disposal

        public BlogPostEditingManager(IBlogPostEditingSite editingSite, IBlogPostEditor[] postEditors, IPublishingContext publishingContext)
        {
            // save reference to owner
            _mainFrameWindow = editingSite.FrameWindow;
            _publishingContext = publishingContext;

            // subscribe to weblog settings edited
            _editingSite = editingSite;
            _editingSite.GlobalWeblogSettingsChanged += new WeblogSettingsChangedHandler(editingSite_GlobalWeblogSettingsEdited);

            // initialize post editors
            _postEditors.Add(_forceDirtyPostEditor);
            _postEditors.AddRange(postEditors);
        }

        public void Dispose()
        {
            if (_editingSite != null)
            {
                _editingSite.GlobalWeblogSettingsChanged -= new WeblogSettingsChangedHandler(editingSite_GlobalWeblogSettingsEdited);
                _editingSite = null;
            }

            // dispose weblog
            DisposeCurrentBlog();

            ResetAutoSave();

            GC.SuppressFinalize(this);
        }

        ~BlogPostEditingManager()
        {
            Trace.Fail("Did not dispose BlogPostEditingManager!");
        }

        #endregion

        #region Explicit Implementation of IBlogPostEditingContext

        BlogPost IBlogPostEditingContext.BlogPost
        {
            get { return BlogPost; }
        }

        BlogPostSupportingFileStorage IBlogPostEditingContext.SupportingFileStorage
        {
            get
            {
                return SupportingFileStorage;
            }
        }

        string IBlogPostEditingContext.ServerSupportingFileDirectory
        {
            get
            {
                return ServerSupportingFileDirectory;
            }
        }

        BlogPostImageDataList IBlogPostEditingContext.ImageDataList
        {
            get
            {
                return ImageDataList;
            }
        }

        BlogPostExtensionDataList IBlogPostEditingContext.ExtensionDataList
        {
            get
            {
                return ExtensionDataList;
            }
        }

        ISupportingFileService IBlogPostEditingContext.SupportingFileService
        {
            get { return SupportingFileService; }
        }

        PostEditorFile IBlogPostEditingContext.LocalFile
        {
            get
            {
                return LocalFile;
            }
        }

        #endregion

        #region Explicit Implementation of IBlogPostPublishingContext

        IBlogPostEditingContext IBlogPostPublishingContext.EditingContext
        {
            get { return this; }
        }

        BlogPost IBlogPostPublishingContext.GetBlogPostForPublishing()
        {
            BlogPost publishingPost = BlogPost.Clone() as BlogPost;

            // trim leading and trailing whitespace from the post
            string contents = publishingPost.Contents;
            if (contents != String.Empty)
            {
                try
                {
                    publishingPost.Contents = HTMLTrimmer.Trim(contents);
                    // remove \r\n sequences inserted when we called StringToHTMLDoc
                    // publishingPost.Contents = HtmlLinebreakStripper.RemoveLinebreaks(contents);
                }
                catch (Exception e)
                {
                    Trace.Fail("Exception while trimming whitespace from post: " + e.ToString());
                }
            }

            return publishingPost;
        }

        void IBlogPostPublishingContext.SetPublishingPostResult(BlogPostPublishingResult publishingResult)
        {
            // save reference to post result
            _lastPublishingResult = publishingResult;

            // update the blog post
            BlogPost.Id = publishingResult.PostResult.PostId;
            BlogPost.DatePublished = publishingResult.PostResult.DatePublished;
            BlogPost.ETag = publishingResult.PostResult.ETag;
            BlogPost.AtomRemotePost = publishingResult.PostResult.AtomRemotePost;

            if (publishingResult.PostPermalink != null)
                BlogPost.Permalink = publishingResult.PostPermalink;

            if (publishingResult.Slug != null)
                BlogPost.Slug = publishingResult.Slug;

            BlogPost.ContentsVersionSignature = publishingResult.PostContentHash;
            BlogPost.CommitPingUrls();
        }
        private BlogPostPublishingResult _lastPublishingResult = new BlogPostPublishingResult();

        void INewCategoryContext.NewCategoryAdded(BlogPostCategory category)
        {
            // commit new category to blog post
            BlogPost.CommitNewCategory(category);

            // see if any of our blog post editors are new category contexts, if so
            // then notify them of the new category being added. Note that this
            // callback occurs during publishing so we don't want UI-layer errors
            // to halt publishing -- catch and log them instead.
            try
            {
                foreach (IBlogPostEditor postEditor in _postEditors)
                    if (postEditor is INewCategoryContext)
                        (postEditor as INewCategoryContext).NewCategoryAdded(category);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception during call to NewCategoryAdded: " + ex.ToString());
            }
        }

        #endregion

        public string CurrentBlog()
        {
            if (_blog != null)
            {
                return _blog.Id;
            }
            else
            {
                return null;
            }
        }

        #region Public Interface: Current Blog

        public string BlogId
        {
            get { return Blog.Id; }
        }

        public string BlogName
        {
            get { return Blog.Name; }
        }

        public Image BlogImage
        {
            get { return Blog.Image; }
        }

        public Icon BlogIcon
        {
            get { return Blog.Icon; }
        }

        public string BlogHomepageUrl
        {
            get { return Blog.HomepageUrl; }
        }

        public string BlogAdminUrl
        {
            get { return Blog.AdminUrl; }
        }

        public string BlogServiceName
        {
            get { return Blog.ServiceName; }
        }

        public string BlogServiceDisplayName
        {
            get { return Blog.ServiceDisplayName; }
        }

        public bool BlogIsAutoUpdatable
        {
            get
            {
                return PostEditorSettings.AllowSettingsAutoUpdate && Blog.ClientOptions.SupportsAutoUpdate;
            }
        }

        public bool BlogRequiresTitles
        {
            get
            {
                return !Blog.ClientOptions.SupportsEmptyTitles;
            }
        }

        public void SwitchBlog(string blogId)
        {
            Trace.Assert(_blog != null, "Can only call SwitchBlog after initialization!");

            // only execute if we are truely switching blogs
            if (Blog != null && blogId != Blog.Id)
            {
                // set current blog
                SetCurrentBlog(blogId);

                // reset post to new
                _blogPost.ResetPostForNewBlog(Blog.ClientOptions);

                // if we are stored in RecentPosts then sever this link
                if (LocalFile.IsRecentPost)
                    LocalFile = PostEditorFile.CreateNew(PostEditorFile.DraftsFolder);

                // notification that the weblog changed
                OnBlogChanged();
            }
        }

        public bool VerifyBlogCredentials()
        {
            return Blog.VerifyCredentials();
        }

        public void DisplayBlogClientOptions()
        {
            BlogClientOptions.ShowInNotepad(Blog.ClientOptions);
        }

        public event EventHandler BlogChanged;

        public event WeblogSettingsChangedHandler BlogSettingsChanged;

        #endregion

        #region Public Interface: Current Post Properties

        public bool PostIsDirty
        {
            get
            {
                // see if a post-editor is dirty
                foreach (IBlogPostEditor postEditor in _postEditors)
                {
                    if (postEditor.IsDirty)
                    {
                        if (ApplicationDiagnostics.AutomationMode || ApplicationDiagnostics.TestMode)
                        {
                            Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "IBlogPostEditor Dirty {0}", postEditor));
                        }
                        return true;
                    }

                }

                // none dirty, return false
                return false;
            }
        }

        /// <summary>
        /// Doesn't indicate that the latest changes have been
        /// saved, only that a file is on disk for this post.
        /// </summary>
        public bool PostIsSaved
        {
            get
            {
                return LocalFile.IsSaved;
            }
        }

        public bool PostIsDraft
        {
            get
            {
                return LocalFile.IsDraft;
            }
        }

        public bool EditingPage
        {
            get
            {
                return BlogPost.IsPage;
            }
        }

        public DateTime? PostDateSaved
        {
            get
            {
                return LocalFile.DateSaved;
            }
        }

        public DateTime PostDatePublished
        {
            get { return BlogPost.DatePublished; }
        }

        public string BlogPostId
        {
            get { return BlogPost.Id; }
        }

        public event EventHandler EditingStatusChanged;

        #endregion

        #region Public Interface: Post Operations (New, Open, Edit, Save, Publish, View etc.)

        public void NewPost()
        {
            // do the edit
            DispatchEditPost(new BlogPost());
        }

        public void NewPage()
        {
            // new blog post
            BlogPost blogPost = new BlogPost();

            // add page support
            if (Blog.ClientOptions.SupportsPages)
                blogPost.IsPage = true;
            else
                Trace.Fail("Attempted to create a Page for a Weblog that does not support pages!");

            // edit the post
            DispatchEditPost(blogPost);
        }

        public void OpenPost(OpenPostForm.OpenMode openMode)
        {
            using (new WaitCursor())
            {
                using (OpenPostForm openPostForm = new OpenPostForm(openMode))
                {
                    openPostForm.UserDeletedPost += new UserDeletedPostEventHandler(openPostForm_UserDeletedPost);
                    if (openPostForm.ShowDialog(_mainFrameWindow) == DialogResult.OK)
                    {
                        // get the editing context
                        OpenPost(openPostForm.BlogPostEditingContext);
                    }
                    openPostForm.UserDeletedPost -= new UserDeletedPostEventHandler(openPostForm_UserDeletedPost);
                }
            }
        }

        public void OpenLocalPost(PostInfo postInfo)
        {
            using (new WaitCursor())
            {
                // get file path
                string postFilePath = postInfo.Id;

                // screen non-existent files
                if (!File.Exists(postFilePath))
                {
                    DisplayMessage.Show(MessageId.PostFileNoExist, _mainFrameWindow, postInfo.Title);
                }
                // screen invalid files
                else if (!PostEditorFile.IsValid(postInfo.Id))
                {
                    DisplayMessage.Show(MessageId.PostFileInvalid, _mainFrameWindow, postInfo.Title);
                }
                else
                {
                    PostEditorFile postEditorFile = PostEditorFile.GetExisting(new FileInfo(postInfo.Id));
                    IBlogPostEditingContext editingContext = postEditorFile.Load();
                    OpenPost(editingContext);
                }
            }
        }

        public void OpenPost(IBlogPostEditingContext editingContext)
        {
            using (new WaitCursor())
            {
                editingContext = RecentPostSynchronizer.Synchronize(_mainFrameWindow, editingContext);

                DispatchEditPost(editingContext, false);
            }
        }

        public void EditPost(IBlogPostEditingContext editingContext)
        {
            EditPost(editingContext, false);
        }

        public void EditPost(IBlogPostEditingContext editingContext, bool forceDirty)
        {
            // not yet initialized
            _initialized = false;

            // note start time of editing session
            _editStartTime = DateTime.UtcNow;

            // defend against invalid blog id
            bool resetPostId = false;
            string blogId = editingContext.BlogId;
            if (!BlogSettings.BlogIdIsValid(blogId))
            {
                blogId = BlogSettings.DefaultBlogId;
                resetPostId = true;
            }

            // initialize state
            SetCurrentBlog(blogId);
            BlogPost = editingContext.BlogPost;
            LocalFile = editingContext.LocalFile;
            _autoSaveLocalFile = editingContext.AutoSaveLocalFile;
            if (_autoSaveLocalFile != null)
                forceDirty = true;
            ServerSupportingFileDirectory = editingContext.ServerSupportingFileDirectory;
            SupportingFileStorage = editingContext.SupportingFileStorage;
            ImageDataList = editingContext.ImageDataList;
            ExtensionDataList = editingContext.ExtensionDataList;
            SupportingFileService = editingContext.SupportingFileService;

            // can now fire events
            _initialized = true;

            // Fix bug 574769: Post ID for a weblog sometimes gets carried over to another weblog so post-publishing fails.
            if (resetPostId)
                BlogPost.ResetPostForNewBlog(Blog.ClientOptions);

            // only fire events after we are fully initialized (event handlers call back into
            // this object and expect everything to be initialized)
            OnBlogChanged();
            OnBlogPostChanged();
            OnEditingStatusChanged();

            // force dirty if requested
            if (forceDirty)
                ForceDirty();
        }

        /// <summary>
        /// Clear the currnet post (does not prompt to save changes)
        /// </summary>
        public void ClearPost()
        {
            EditPost(new BlogPostEditingContext(BlogId, new BlogPost()));
        }

        public bool SaveDraft()
        {
            try
            {
                return SaveToDrafts();
            }
            finally
            {
                // always note that the user tried to save
                if (UserSavedPost != null)
                    UserSavedPost(this, EventArgs.Empty);
            }

        }

        public bool ShouldAutoSave
        {
            get
            {
                // screen out invalid states (shouldn't need to do this but we
                // are adding this feature late in the cycle and want to make
                // sure there isn't anything we are missing)
                if (BlogPost == null || LocalFile == null)
                    return false;

                return PostEditorSettings.AutoSaveDrafts && PostIsDirty;
            }
        }

        public bool AutoSaveIfRequired(bool forceSave)
        {
            if (ShouldAutoSave || forceSave)
            {
                AutoSave();
                return true;
            }
            return false;
        }

        private DateTime AutoSaveStartTime
        {
            get
            {
                if (LocalFile.DateSaved == DateTime.MinValue)
                    return _editStartTime;
                else
                    return LocalFile.DateSaved;
            }
        }

        public void DeleteLocalPost(PostInfo postInfo)
        {
            // get the post file associated with this post
            PostEditorFile postFile = PostEditorFile.GetExisting(new FileInfo(postInfo.Id));

            // prompt the user to confirm deletion
            string type = BlogPost.IsPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower);
            MessageId messageId = postFile.IsDraft ? MessageId.ConfirmDeleteDraft : MessageId.ConfirmDeletePost;
            if (DisplayMessage.Show(messageId, _mainFrameWindow, type, postInfo.Title) == DialogResult.Yes)
            {
                // update main window to eliminate artifacts
                _mainFrameWindow.Update();

                // see if we are deleting the currently active file
                bool deletingCurrentDraft = (postFile.Equals(LocalFile));

                if (DeletePostFile(postFile))
                {
                    // fire notification of the delete
                    if (UserDeletedPost != null)
                        UserDeletedPost(this, EventArgs.Empty);

                    // open a new untitled post if we just deleted the current draft
                    if (deletingCurrentDraft)
                        ClearPost();
                }
            }

        }

        public void DeleteCurrentDraft()
        {
            // check for valid/sane state
            if (!PostIsDraft)
            {
                Trace.Fail("Attempted to delete a post that is not a draft!");
                return;
            }

            // prompt the user to confirm deletion
            string title = BlogPost.Title != String.Empty ? BlogPost.Title : BlogPost.IsPage ? PostInfo.UntitledPage : PostInfo.UntitledPost;
            string type = BlogPost.IsPage ? Res.Get(StringId.PageLower) : Res.Get(StringId.PostLower);
            if (DisplayMessage.Show(MessageId.ConfirmDeleteDraft, _mainFrameWindow, type, title) == DialogResult.Yes)
            {
                if (DeletePostFile(LocalFile))
                {
                    // fire notification of the delete
                    if (UserDeletedPost != null)
                        UserDeletedPost(this, EventArgs.Empty);

                    // open a new untitled post
                    ClearPost();
                }
            }
        }

        public bool PublishAsDraft()
        {
            if (ValidatePublish())
                return PostToWeblog(false);
            else
                return false;
        }

        public bool Publish()
        {
            if (ValidatePublish())
                return PostToWeblog(true);
            else
                return false;
        }

        public void ViewPost()
        {
            if (BlogPost.IsPage && BlogPost.Permalink != String.Empty)
                ShellHelper.LaunchUrl(BlogPost.Permalink);
            else
                ShellHelper.LaunchUrl(Blog.HomepageUrl);
        }

        public void EditPostOnline(bool clearPostWindow)
        {
            // calculate the url (no-op if there is no url)
            string postEditingUrl = Blog.GetPostEditingUrl(BlogPost.Id);
            if (!UrlHelper.IsUrl(postEditingUrl))
            {
                Trace.Fail("Invalid URL provided for online post editing: " + postEditingUrl);
                return;
            }

            // clear the post editor if requested (ideally we would do this AFTER
            // launching the browser but if we do then our window steals focus
            // back from the browser)
            if (clearPostWindow)
                ClearPost();

            // launch the editing url
            ShellHelper.LaunchUrl(postEditingUrl);
        }

        #endregion

        #region Public Interface: Events

        public event EventHandler UserSavedPost;

        public event EventHandler UserPublishedPost;

        public event EventHandler UserDeletedPost;

        #endregion

        #region Private Helpers

        private void DispatchEditPost(BlogPost blogPost)
        {
            DispatchEditPost(new BlogPostEditingContext(BlogId, blogPost), true);
        }

        /// <summary>
        /// Dispatches an edit post request to either the current editor window
        /// or to a new editor form depending upon the user's preferences and
        /// the current editing state
        /// </summary>
        /// <param name="editingContext">editing conext</param>
        /// <param name="isNewPost">if set to <c>true</c> [is new post].</param>
        private void DispatchEditPost(IBlogPostEditingContext editingContext, bool isNewPost)
        {
            // calcluate whether the user has a "blank" unsaved post
            bool currentPostIsEmptyAndUnsaved =
                ((BlogPost != null) && BlogPost.IsNew && (BlogPost.Contents == null || BlogPost.Contents == String.Empty)) &&
                 !LocalFile.IsSaved && !PostIsDirty;

            // edge case: current post is empty and unsaved and this is a new post,
            // re-using the window in this case will just make the New button appear
            // to not work at all, therefore we force a new window. We make an exception
            // for creation of new pages, as firing up a new writer instance and then
            // switching into "page authoring" mode is a natual thing to do (and shouldn't
            // result in a new window for no apparent reason). In this case the user will
            // get "feedback" by seeing the default title change to "Enter Page Title Here"
            // as well as the contents of the property tray changing.
            if (currentPostIsEmptyAndUnsaved && isNewPost && !editingContext.BlogPost.IsPage)
            {
                PostEditorForm.Launch(editingContext);
                return;
            }

            // Notify all the editors that the post is about to close
            // This will give them an chance to do any clean up, or in the
            // case of video publish to hold up the close operation till videos
            // have finished uploading.
            CancelEventArgs e = new CancelEventArgs();
            foreach (IBlogPostEditor editor in _postEditors)
            {
                editor.OnPostClosing(e);
                if (e.Cancel)
                    break;
            }

            if (e.Cancel)
            {
                return;
            }

            switch (PostEditorSettings.PostWindowBehavior)
            {
                case PostWindowBehavior.UseSameWindow:

                    // allow the user a chance to save if necessary
                    if (PostIsDirty)
                    {
                        // cancel aborts the edit post operation
                        DialogResult saveChangesResult = PromptToSaveChanges();
                        if (saveChangesResult == DialogResult.Cancel)
                        {
                            return;
                        }
                        // special case -- we are actually re-editing the currently active post
                        // in this instance we need to "re-open" it to reflect the saved changes
                        else if (saveChangesResult == DialogResult.Yes && editingContext.LocalFile.Equals(LocalFile))
                        {
                            EditPostWithPostCloseEvent(LocalFile.Load());
                            break; 
                        }
                    }

                    // edit the post using the existing window
                    EditPostWithPostCloseEvent(editingContext);

                    break;

                case PostWindowBehavior.OpenNewWindow:

                    // special case: if the current frame contains an empty, unsaved
                    // post then replace it (covers the case of the user opening
                    // writer in order to edit an existing post -- in this case they
                    // should never have to deal with managing two windows
                    if (currentPostIsEmptyAndUnsaved)
                    {
                        EditPostWithPostCloseEvent(editingContext);
                    }
                    else
                    {
                        // otherwise open a new window
                        PostEditorForm.Launch(editingContext);
                    }

                    break;

                case PostWindowBehavior.OpenNewWindowIfDirty:

                    if (PostIsDirty)
                    {
                        PostEditorForm.Launch(editingContext);
                    }
                    else
                    {
                        EditPostWithPostCloseEvent(editingContext);
                    }

                    break;
            }
        }

        private void EditPostWithPostCloseEvent(IBlogPostEditingContext blogPostEditingContext)
        {
            // If the editor has already been initialized once, then we need to tell the editor it is getting its post closed.
            if (_initialized)
            {
                foreach (IBlogPostEditor editor in _postEditors)
                {
                    editor.OnPostClosed();
                }
            }
            EditPost(blogPostEditingContext);
        }

        private DialogResult PromptToSaveChanges()
        {
            DialogResult result = DisplayMessage.Show(MessageId.QueryForUnsavedChanges, _mainFrameWindow);
            if (result == DialogResult.Yes)
            {
                using (new WaitCursor())
                    SaveDraft();
            }

            return result;
        }

        private void AutoSave()
        {
            using (new QuickTimer("AutoSave"))
            {
                // save all pending edits to the post
                BlogPostSaveOptions options = new BlogPostSaveOptions();
                options.AutoSave = true;
                SaveEditsToPost(true, options);

                try
                {
                    PostEditorFile fileToSave = AutoSaveLocalFile;
                    fileToSave.AutoSave(this, LocalFile);
                }
                catch (Exception ex)
                {
                    Trace.Fail("AutoSave failed! " + ex.Message);
                    if (ApplicationDiagnostics.VerboseLogging)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private bool SaveToDrafts()
        {
            // save all pending edits to the post
            SaveEditsToPost();

            // save the post
            try
            {
                // determine file to save (default to current one)
                PostEditorFile fileToSave = LocalFile;

                // if this is a Recent Post then don't actually save into Recent Posts since
                // it has now changed and is a "Draft" pending re-publishing
                if (fileToSave.IsRecentPost)
                    fileToSave = PostEditorFile.CreateNew(PostEditorFile.DraftsFolder);

                // save the file and update the reference
                fileToSave.SaveBlogPost((this as IBlogPostEditingContext));
                LocalFile = fileToSave;

                return true;
            }
            catch (Exception ex)
            {
                DisplayableExceptionDisplayForm.Show(_mainFrameWindow, ex);
                ForceDirty();
                return false;
            }
        }

        private bool DeletePostFile(PostEditorFile postFile)
        {
            try
            {
                // screen files which have already been deleted through other means
                if (!postFile.IsDeleted)
                    postFile.Delete();
                return true;
            }
            catch (Exception ex)
            {
                DisplayableException displayableException = new DisplayableException(
                    StringId.ErrorOccurredDeletingDraft, StringId.ErrorOccurredDeletingDraftDetails, ex.Message);
                DisplayableExceptionDisplayForm.Show(_mainFrameWindow, displayableException);
                return false;
            }
        }

        private void openPostForm_UserDeletedPost(PostInfo deletedPost)
        {
            // See if the file currently being edited was deleted. In this case
            // clear out the post editor
            if (LocalFile != null && LocalFile.IsDeleted)
                ClearPost();

            // Fire notification
            if (UserDeletedPost != null)
                UserDeletedPost(this, EventArgs.Empty);
        }

        private bool ValidatePublish()
        {
            if (this.Blog.IsSpacesBlog)
            {
                DisplayMessage.Show(MessageId.SpacesPublishingNotEnabled);
                return false;
            }

            // check all of our post editors
            foreach (IBlogPostEditor postEditor in _postEditors)
            {
                if (!postEditor.ValidatePublish())
                    return false;
            }

            // survived, ok to publish
            return true;
        }

        public void Closing(CancelEventArgs e)
        {
            // Notify all the editors that the application is about to close
            // This will give them an chance to do any clean up, or in the
            // case of video publish to hold up the close operation till videos
            // have finished uploading.
            foreach (IBlogPostEditor postEditor in _postEditors)
            {
                postEditor.OnPostClosing(e);
                if (e.Cancel)
                    return;

            }

            foreach (IBlogPostEditor postEditor in _postEditors)
            {
                postEditor.OnClosing(e);
                if (e.Cancel)
                    return;
            }
        }

        internal void OnClosed()
        {
            foreach (IBlogPostEditor postEditor in _postEditors)
            {
                postEditor.OnPostClosed();
                postEditor.OnClosed();
            }
        }

        private bool PostToWeblog(bool publish)
        {
            try
            {
                // save all pending edits
                // we will preserve the dirty-ness of the post so in case if the publish
                // failed or was cancelled, the post will remain dirty.
                SaveEditsToPost(true, BlogPostSaveOptions.DefaultOptions);

                // validate that we aren't attempting to post local files to a blog not configured to do so
                if (!ValidateSupportingFileUsage())
                    return false;

                // validate that we have the credentials required to publish
                if (!VerifyBlogCredentials())
                    return false;

                // try to update the weblog
                bool isNewPost = BlogPost.IsNew;
                if (UpdateWeblog(publish))
                {
                    // save a copy to recent posts (note: we used to only save to recent
                    // posts only if a publish occurred, however we now do this always
                    // because we want the post to participate in syncing to online
                    // changes AND we don't want the post "trapped" in drafts if the
                    // user intends to edit and publish over the web.
                    // NOTE: if we want to make this behavior conditional then the logic is:
                    //    if (published) SaveToRecentPosts(); else SaveToDrafts();
                    SaveToRecentPosts();

                    // notify editors that we successfully published
                    Debug.Assert(_lastPublishingResult.PostResult.PostId != String.Empty);
                    foreach (IBlogPostEditor postEditor in _postEditors)
                        postEditor.OnPublishSucceeded(BlogPost, _lastPublishingResult.PostResult);

                    // show file upload failed warning if necessary
                    DisplayAfterPublishFileUploadFailedWarningIfNecessary();

                    // success
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                // always note that the user tried to publish
                if (UserPublishedPost != null)
                    UserPublishedPost(this, EventArgs.Empty);
            }
        }

        private void BeforePublish(object sender, UpdateWeblogProgressForm.PublishingEventArgs args)
        {
            UpdateWeblogProgressForm form = (UpdateWeblogProgressForm)sender;

            bool noPluginsEnabled;
            if (!HeaderAndFooterPrerequisitesMet(form, out noPluginsEnabled))
            {
                Debug.Assert(!noPluginsEnabled, "No plugins enabled yet header/footer prerequisites met!?");
                args.RepublishOnSuccess = true;
                return;
            }

            if (noPluginsEnabled)
                return;

            BlogPost.Contents = SmartContentInsertionHelper.StripDivsWithClass(BlogPost.Contents, ContentSourceManager.HEADERS_FOOTERS);

            foreach (ContentSourceInfo csi in ContentSourceManager.GetActiveHeaderFooterPlugins(form, Blog.Id))
            {
                form.SetProgressMessage(string.Format(CultureInfo.InvariantCulture, Res.Get(StringId.PluginPublishProgress), csi.Name));

                HeaderFooterSource plugin = (HeaderFooterSource)csi.Instance;
                if (plugin.RequiresPermalink && string.IsNullOrEmpty(BlogPost.Permalink))
                {
                    Trace.WriteLine("Skipping plugin " + csi.Name + " because the post has no permalink");
                    continue;
                }
                IExtensionData extData =
                    ((IBlogPostEditingContext)this).ExtensionDataList.GetOrCreateExtensionData(csi.Id);
                SmartContent smartContent = new SmartContent(extData);
                HeaderFooterSource.Position position;
                string html;
                try
                {
                    html = plugin.GeneratePublishHtml(form, smartContent, _publishingContext, args.Publish, out position);
                }
                catch (Exception e)
                {
                    DisplayPluginException(form, csi, e);
                    continue;
                }

                if (string.IsNullOrEmpty(html))
                    continue;

                if (SmartContentInsertionHelper.ContainsUnbalancedDivs(html))
                {
                    Trace.Fail("Unbalanced divs detected in HTML generated by " + csi.Name + ": " + html);
                    DisplayMessage.Show(MessageId.MalformedHtmlIgnored, form, csi.Name);
                    continue;
                }

                // Don't use float alignment for footers--it causes them to float around
                // stuff that isn't part of the post
                bool noFloat = position == HeaderFooterSource.Position.Footer;
                string generatedHtml = SmartContentInsertionHelper.GenerateBlock(ContentSourceManager.HEADERS_FOOTERS, null, smartContent, false, html, noFloat, null);

                if (position == HeaderFooterSource.Position.Header)
                    BlogPost.Contents = generatedHtml + BlogPost.Contents;
                else if (position == HeaderFooterSource.Position.Footer)
                    BlogPost.Contents = BlogPost.Contents + generatedHtml;
                else
                    Debug.Fail("Unknown HeaderFooter position: " + position);
            }
            form.SetProgressMessage(null);

            BlogPost.Contents = HtmlLinebreakStripper.RemoveLinebreaks(BlogPost.Contents);
        }

        private bool HeaderAndFooterPrerequisitesMet(IWin32Window owner, out bool noPluginsEnabled)
        {
            noPluginsEnabled = true;
            foreach (ContentSourceInfo csi in ContentSourceManager.GetActiveHeaderFooterPlugins(owner, Blog.Id))
            {
                noPluginsEnabled = false;

                // Short circuit because IsNew is the only requirement
                if (!BlogPost.IsNew)
                    return true;

                HeaderFooterSource plugin = (HeaderFooterSource)csi.Instance;
                if (plugin.RequiresPermalink)
                    return false;
            }
            return true;
        }

        public class CancelPublishException : Exception
        {

        }

        private class PublishOperationManager
        {
            private UpdateWeblogProgressForm _form;
            private Blog _blog;
            public PublishOperationManager(UpdateWeblogProgressForm form, Blog blog)
            {
                _form = form;
                _blog = blog;
            }

            internal void DoPublishWork(IPublishingContext site, SmartContentSource source, ISmartContent sContent, ref string content)
            {
                if (source is IPublishTimeWorker)
                {
                    try
                    {
                        // For each piece of IPublishTimeWorker smart content we find we need to ask if it has
                        // work to do while publishing.  We pass null as the external context because that object
                        // is only for use when providing external code a chance to interact with the publish.
                        content = ((IPublishTimeWorker)source).DoPublishWork(_form, sContent, _blog.Id, site, null);

                    }
                    catch (WebException ex)
                    {
                        HttpRequestHelper.LogException(ex);
                        _exception = ex;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Content Source failed to do publish work: " + ex);
                        _exception = ex;
                        throw;
                    }

                }
            }

            private Exception _exception;
            public Exception Exception
            {
                get
                {
                    return _exception;
                }
            }

        }

        private void PrePublishHooks(object sender, UpdateWeblogProgressForm.PublishEventArgs args)
        {
            Debug.Assert(!_mainFrameWindow.InvokeRequired, "PrePublishHooks invoked on non-UI thread");

            UpdateWeblogProgressForm form = (UpdateWeblogProgressForm)sender;
            bool publish = args.Publish;

            // Let built in plugins do any extra processing they need to do during publish time
            PublishOperationManager publishOperationManager = new PublishOperationManager(form, Blog);
            string contents = SmartContentWorker.PerformOperation(_publishingContext.PostInfo.Contents, publishOperationManager.DoPublishWork, true, (IContentSourceSidebarContext)_publishingContext, false);

            // One of the built in plugins threw an exception and we must cancel publish
            if (publishOperationManager.Exception != null)
            {
                if (!(publishOperationManager.Exception is CancelPublishException))
                {
                    UnhandledExceptionErrorMessage message = new UnhandledExceptionErrorMessage();
                    message.ShowMessage(_mainFrameWindow, publishOperationManager.Exception);
                }
                args.Cancel = true;
                args.CancelReason = null;
                return;
            }

            // We only save the new html if there was no exception while creating it.
            BlogPost.Contents = contents;
            LocalFile.SaveBlogPost(this as IBlogPostEditingContext);

            foreach (ContentSourceInfo csi in ContentSourceManager.GetActivePublishNotificationPlugins(form, Blog.Id))
            {
                form.SetProgressMessage(string.Format(CultureInfo.InvariantCulture, Res.Get(StringId.PluginPublishProgress), csi.Name));
                IProperties properties =
                    ((IBlogPostEditingContext)this).ExtensionDataList.GetOrCreateExtensionData(csi.Id).Settings;

                try
                {
                    if (!((PublishNotificationHook)csi.Instance).OnPrePublish(form, properties, _publishingContext, publish))
                    {
                        args.Cancel = true;
                        args.CancelReason = csi.Name;
                        return;
                    }
                }
                catch (Exception e)
                {
                    DisplayPluginException(form, csi, e);
                    continue;
                }
            }
            form.SetProgressMessage(null);
        }

        private static void DisplayPluginException(IWin32Window owner, ContentSourceInfo csi, Exception e)
        {
            Trace.Fail(e.ToString());
            DisplayableException ex = new DisplayableException(
                Res.Get(StringId.UnexpectedErrorPluginTitle),
                string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.UnexpectedErrorPluginDescription), csi.Name, e.Message));
            DisplayableExceptionDisplayForm.Show(owner, ex);
        }

        private void PostPublishHooks(object sender, UpdateWeblogProgressForm.PublishEventArgs args)
        {
            Debug.Assert(!_mainFrameWindow.InvokeRequired, "PostPublishHooks invoked on non-UI thread");

            UpdateWeblogProgressForm form = (UpdateWeblogProgressForm)sender;
            bool publish = args.Publish;

            foreach (ContentSourceInfo csi in ContentSourceManager.GetActivePublishNotificationPlugins(form, Blog.Id))
            {
                form.SetProgressMessage(string.Format(CultureInfo.InvariantCulture, Res.Get(StringId.PluginPublishProgress), csi.Name));
                IProperties properties =
                    ((IBlogPostEditingContext)this).ExtensionDataList.GetOrCreateExtensionData(csi.Id).Settings;

                try
                {
                    ((PublishNotificationHook)csi.Instance).OnPostPublish(form, properties, _publishingContext, publish);
                }
                catch (Exception ex)
                {
                    DisplayPluginException(form, csi, ex);
                    continue;
                }
            }
            form.SetProgressMessage(null);
        }

        /// <summary>
        /// Saves info to the active blog post, but does not persist them
        /// to disk. This will cause editors to become un-dirty.
        /// </summary>
        public void SaveEditsToPost()
        {
            SaveEditsToPost(false, BlogPostSaveOptions.DefaultOptions);
        }

        /// <summary>
        /// Saves info to the active blog post, but does not persist them
        /// to disk. Optionally preserve dirty state.
        /// </summary>
        public void SaveEditsToPost(bool preserveDirty, BlogPostSaveOptions options)
        {
            bool makeDirty = preserveDirty && PostIsDirty;
            try
            {
                foreach (IBlogPostEditor postEditor in _postEditors)
                    postEditor.SaveChanges(BlogPost, options);
            }
            finally
            {
                if (makeDirty)
                    ForceDirty();
            }
        }

        private bool UpdateWeblog(bool publish)
        {
            using (new WaitCursor())
            {
                // save all pending edits
                if (!SaveToDrafts())
                    return false;

                // upload to the weblog
                using (UpdateWeblogProgressForm updateWeblogProgressForm =
                    new UpdateWeblogProgressForm(_mainFrameWindow, this, BlogPost.IsPage, BlogName, publish))
                {
                    updateWeblogProgressForm.PrePublish += PrePublishHooks;
                    updateWeblogProgressForm.Publishing += BeforePublish;
                    updateWeblogProgressForm.PostPublish += PostPublishHooks;

                    // show the progress form
                    DialogResult result = updateWeblogProgressForm.ShowDialog(_mainFrameWindow);

                    // return success or failure
                    if (result == DialogResult.OK)
                    {
                        return true;
                    }
                    else if (result == DialogResult.Abort)
                    {
                        if (updateWeblogProgressForm.CancelReason != null)
                            DisplayMessage.Show(MessageId.PublishCanceledByPlugin, _mainFrameWindow, updateWeblogProgressForm.CancelReason);
                        return false;
                    }
                    else
                    {
                        if (updateWeblogProgressForm.Exception != null)
                            Trace.Fail(updateWeblogProgressForm.Exception.ToString());

                        // if this was a failure to upload images using the Metaweblog API newMediaObject
                        // then provide a special error message form that allows the user to reconfigure
                        // their file upload settings to another destination (e.g. FTP)
                        if (updateWeblogProgressForm.Exception is BlogClientFileUploadNotSupportedException)
                        {
                            result = FileUploadFailedForm.Show(_mainFrameWindow, GetUniqueImagesInPost());
                            if (result == DialogResult.Yes)
                                _editingSite.ConfigureWeblogFtpUpload(Blog.Id);
                        }
                        else if (updateWeblogProgressForm.Exception is BlogClientOperationCancelledException)
                        {
                            // show no UI for operation cancelled
                            Debug.WriteLine("BlogClient operation cancelled");
                        }
                        else
                        {
                            _blog.DisplayException(_mainFrameWindow, updateWeblogProgressForm.Exception);
                        }

                        // return failure
                        return false;
                    }
                }
            }
        }

        private void SaveToRecentPosts()
        {
            try
            {
                // note if this was saved in the drafts folder
                PostEditorFile draftFile = null;
                if (LocalFile.IsDraft)
                    draftFile = LocalFile;

                // determine recent post file to save into (always try to find an existing recent post
                // with this blog/post id -- if unable to find one then create new one)
                PostEditorFile recentPostFile = PostEditorFile.FindPost(PostEditorFile.RecentPostsFolder, Blog.Id, BlogPost.Id);
                if (recentPostFile == null)
                    recentPostFile = PostEditorFile.CreateNew(PostEditorFile.RecentPostsFolder);

                // do the save and update the reference to the local file
                recentPostFile.SaveBlogPost((this as IBlogPostEditingContext));
                LocalFile = recentPostFile;

                // delete copy from drafts if it was there
                if (draftFile != null)
                    draftFile.Delete();
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception saving recent post: " + ex.ToString());
            }
        }

        private bool ValidateSupportingFileUsage()
        {
            if (Blog.SupportsImageUpload == SupportsFeature.No)
            {
                string[] images = GetUniqueImagesInPost();

                if (images.Length > 0)
                {
                    DialogResult result = FileUploadFailedForm.Show(_mainFrameWindow, images);
                    if (result == DialogResult.Yes)
                        _editingSite.ConfigureWeblogFtpUpload(Blog.Id);

                    return false;
                }
            }

            // if we got this far then return true
            return true;
        }

        private void DisplayAfterPublishFileUploadFailedWarningIfNecessary()
        {
            if (_lastPublishingResult != null && _lastPublishingResult.AfterPublishFileUploadException != null)
            {
                using (AfterPublishFileUploadFailedForm uploadFailedForm = new AfterPublishFileUploadFailedForm(_lastPublishingResult.AfterPublishFileUploadException, BlogPost.IsPage))
                    uploadFailedForm.ShowDialog(_mainFrameWindow);
            }
        }

        private string[] GetUniqueImagesInPost()
        {
            ArrayList images = new ArrayList();
            string[] postSupportingFiles = SupportingFileStorage.GetSupportingFilesInPost(BlogPost.Contents);
            foreach (string supportingFile in postSupportingFiles)
            {
                if (PathHelper.IsPathImage(supportingFile) && IsNotExtraImage(supportingFile))
                    images.Add(Path.GetFileName(supportingFile));
            }
            return (string[])images.ToArray(typeof(string));
        }

        private bool IsNotExtraImage(string imagePath)
        {
            // : This is designed to filter out the extra images posted (e.g. "_thumb")
            // so that the list displayed matches the number of images within the document
            // we should make this filter 100% reliable then reintroduce it
            return true;

            /*
            string fileName = Path.GetFileNameWithoutExtension();
            return !fileName.EndsWith("_thumb") && !fileName.EndsWith("[1]");
            */
        }

        public void ForceDirty()
        {
            _forceDirtyPostEditor.ForceDirty();
        }

        #endregion

        #region Private Event Handlers (Weblog Settings Edited, etc.

        private void editingSite_GlobalWeblogSettingsEdited(string blogId, bool templateChanged)
        {
            // if the settings for our blog changed then notify the editors
            if (blogId == Blog.Id)
            {
                // force our blog to update its client options
                Blog.InvalidateClient();

                foreach (IBlogPostEditor postEditor in _postEditors)
                    postEditor.OnBlogSettingsChanged(templateChanged);

                // notify listeners that blog settings changed
                if (BlogSettingsChanged != null)
                    BlogSettingsChanged(blogId, templateChanged);

                // force a perform layout so the main frame can dynamically re-flow for
                // any UI changes that were made as a result of the blog settings changing
                _editingSite.FrameWindow.PerformLayout();
                _editingSite.FrameWindow.Invalidate();
            }
        }

        #endregion

        #region Private Manipulation of Internal State (Current Blog, Current Post, LocalFile, etc.)

        public Blog Blog
        {
            get { return _blog; }
        }

        private void OnBlogChanged()
        {
            if (_initialized)
            {
                // notify post editors
                foreach (IBlogPostEditor postEditor in _postEditors)
                    postEditor.OnBlogChanged(Blog);

                // fire the event
                if (BlogChanged != null)
                    BlogChanged(this, EventArgs.Empty);

                // force a perform layout so the main frame can dynamically re-flow for
                // any UI changes that were made as a result of the blog changing
                _editingSite.FrameWindow.PerformLayout();
            }
        }

        private void SetCurrentBlog(string blogId)
        {
            DisposeCurrentBlog();
            _blog = new Blog(blogId);

            // update the default
            BlogSettings.DefaultBlogId = blogId;
        }

        private void DisposeCurrentBlog()
        {
            if (_blog != null)
            {
                _blog.Dispose();
                _blog = null;
            }
        }

        private BlogPost BlogPost
        {
            get
            {
                return _blogPost;
            }
            set
            {
                _blogPost = value;
                OnBlogPostChanged();
            }
        }

        private void OnBlogPostChanged()
        {
            if (_initialized)
            {
                // notify post editors
                foreach (IBlogPostEditor postEditor in _postEditors)
                    postEditor.Initialize(this, Blog.ClientOptions);
            }
        }

        private BlogPostSupportingFileStorage SupportingFileStorage
        {
            get
            {
                return _supportingFileStorage;
            }
            set
            {
                _supportingFileStorage = value;
            }
        }

        private string ServerSupportingFileDirectory
        {
            get
            {
                // create on demand based on the title of the post
                if (_serverSupportingFileDirectory == String.Empty)
                {
                    // convert title to usable directory name
                    string baseDirName = SupportingFileStorage.CleanPathForServer(BlogPost.Title);

                    // append seconds since midnight as an additional randomizer to make conflicts less likely
                    _serverSupportingFileDirectory = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", baseDirName, (DateTime.Now.TimeOfDay.Ticks / TimeSpan.TicksPerSecond).ToString("X", CultureInfo.InvariantCulture));
                }
                return _serverSupportingFileDirectory;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Cannot set ServerSupportingFileDirectory to null!!!");

                _serverSupportingFileDirectory = value;

            }
        }

        private BlogPostImageDataList ImageDataList
        {
            get { return _imageDataList; }
            set { _imageDataList = value; }
        }

        private BlogPostExtensionDataList ExtensionDataList
        {
            get { return _extensionDataList; }
            set { _extensionDataList = value; }
        }

        private ISupportingFileService SupportingFileService
        {
            get { return _fileService; }
            set { _fileService = value; }
        }

        public PostEditorFile AutoSaveLocalFile
        {
            get
            {
                if (_autoSaveLocalFile == null)
                {
                    _autoSaveLocalFile = PostEditorFile.CreateNew(new DirectoryInfo(PostEditorSettings.AutoSaveDirectory));
                }
                return _autoSaveLocalFile;
            }
        }

        private void ResetAutoSave()
        {
            if (_autoSaveLocalFile != null)
            {
                PostEditorFile file = _autoSaveLocalFile;
                _autoSaveLocalFile = null;
                file.Delete();
            }
        }

        private PostEditorFile LocalFile
        {
            get
            {
                return _localFile;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Cannot set LocalFile to null!");

                _localFile = value;
                ResetAutoSave();
                OnEditingStatusChanged();
            }
        }

        private void OnEditingStatusChanged()
        {
            if (_initialized)
            {
                if (EditingStatusChanged != null)
                    EditingStatusChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Private Members

        private bool _initialized = false;

        private IMainFrameWindow _mainFrameWindow;
        private IPublishingContext _publishingContext; // used for headers and footers

        private IBlogPostEditingSite _editingSite;

        private ArrayList _postEditors = new ArrayList();
        private ForceDirtyPostEditor _forceDirtyPostEditor = new ForceDirtyPostEditor();

        private Blog _blog;
        private BlogPost _blogPost;
        private BlogPostSupportingFileStorage _supportingFileStorage;
        private string _serverSupportingFileDirectory = String.Empty;
        private BlogPostImageDataList _imageDataList;
        private BlogPostExtensionDataList _extensionDataList;
        private ISupportingFileService _fileService;
        private PostEditorFile _localFile;
        private PostEditorFile _autoSaveLocalFile;
        private DateTime _editStartTime;

        #endregion

    }
}
