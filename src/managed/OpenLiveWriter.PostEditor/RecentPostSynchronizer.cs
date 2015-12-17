// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.PostEditor
{

    public class RecentPostSynchronizer
    {
        /// <summary>
        /// Synchronize the local and remote copies of the recent post to create an
        /// edit context that combines the latest HTML content, etc. from the web
        /// with the local image editing context
        /// </summary>
        /// <param name="editingContext"></param>
        /// <returns></returns>
        public static IBlogPostEditingContext Synchronize(IWin32Window mainFrameWindow, IBlogPostEditingContext editingContext)
        {
            // reloading a local draft does not require synchronization
            if (editingContext.LocalFile.IsDraft && editingContext.LocalFile.IsSaved)
            {
                return editingContext;
            }
            else if (editingContext.LocalFile.IsRecentPost)
            {
                // search for a draft of this post which has already been initialized for offline editing of the post
                // (we don't want to allow opening multiple local "drafts" of edits to the same remote post
                PostEditorFile postEditorFile = PostEditorFile.FindPost(PostEditorFile.DraftsFolder, editingContext.BlogId, editingContext.BlogPost.Id);
                if (postEditorFile != null)
                {
                    // return the draft
                    return postEditorFile.Load();
                }

                //verify synchronization is supported for this blog service
                if (!SynchronizationSupportedForBlog(editingContext.BlogId))
                {
                    Debug.WriteLine("Post synchronization is not supported");
                    return editingContext;
                }

                // opening local copy, try to marry with up to date post content on the server
                // (will return the existing post if an error occurs or the user cancels)
                BlogPost serverBlogPost = SafeGetPostFromServer(mainFrameWindow, editingContext.BlogId, editingContext.BlogPost);
                if (serverBlogPost != null)
                {
                    // if the server didn't return a post-id then replace it with the
                    // known post id
                    if (serverBlogPost.Id == String.Empty)
                        serverBlogPost.Id = editingContext.BlogPost.Id;

                    // merge trackbacks
                    MergeTrackbacksFromClient(serverBlogPost, editingContext.BlogPost);

                    // create new init params
                    IBlogPostEditingContext newEditingContext = new BlogPostEditingContext(
                        editingContext.BlogId,
                        serverBlogPost, // swap-in blog post from server
                        editingContext.LocalFile,
                        null,
                        editingContext.ServerSupportingFileDirectory,
                        editingContext.SupportingFileStorage,
                        editingContext.ImageDataList,
                        editingContext.ExtensionDataList,
                        editingContext.SupportingFileService);

                    SynchronizeLocalContentsWithEditingContext(editingContext.BlogPost.Contents,
                                                                editingContext.BlogPost.ContentsVersionSignature, newEditingContext);

                    // return new init params
                    return newEditingContext;
                }
                else
                {
                    return editingContext;
                }
            }
            else if (editingContext.LocalFile.IsSaved)
            {
                // Opening draft from somewhere other than the official drafts directory
                return editingContext;
            }
            else
            {
                // opening from the server, first see if the user already has a draft
                // "checked out" for this post
                PostEditorFile postEditorFile = PostEditorFile.FindPost(PostEditorFile.DraftsFolder, editingContext.BlogId, editingContext.BlogPost.Id);
                if (postEditorFile != null)
                {
                    return postEditorFile.Load();
                }

                // no draft, try to marry with local copy of recent post
                PostEditorFile recentPost = PostEditorFile.FindPost(
                    PostEditorFile.RecentPostsFolder,
                    editingContext.BlogId,
                    editingContext.BlogPost.Id);

                if (recentPost != null)
                {
                    // load the recent post
                    IBlogPostEditingContext newEditingContext = recentPost.Load();

                    string localContents = newEditingContext.BlogPost.Contents;
                    string localContentsSignature = newEditingContext.BlogPost.ContentsVersionSignature;

                    // merge trackbacks from client
                    MergeTrackbacksFromClient(editingContext.BlogPost, newEditingContext.BlogPost);

                    // copy the BlogPost properties from the server (including merged trackbacks)
                    newEditingContext.BlogPost.CopyFrom(editingContext.BlogPost);

                    SynchronizeLocalContentsWithEditingContext(localContents, localContentsSignature, newEditingContext);

                    // return the init params
                    return newEditingContext;
                }
                else
                {
                    return editingContext;
                }
            }

        }

        private static void MergeTrackbacksFromClient(BlogPost serverBlogPost, BlogPost clientBlogPost)
        {
            // Merge trackbacks from client. Servers don't seem to ever return trackbacks
            // (haven't found one that does). Our protocol code assumes that any trackbacks
            // returned by the server will be considered "already sent" and placed in the
            // PingUrlsSent bucket. So the correct list of trackbacks is whatever we are
            // storing on the client as PingUrlsPending plus the union of the client
            // and server PingUrlsSent fields.

            // ping urls already sent is the union of the client record of pings sent plus the
            // server record (if any) of pings sent
            serverBlogPost.PingUrlsSent = ArrayHelper.Union(clientBlogPost.PingUrlsSent, serverBlogPost.PingUrlsSent);

            // pending ping urls are any pings that the client has pending that are not contained
            // in our list of already sent ping urls
            Debug.Assert(serverBlogPost.PingUrlsPending.Length == 0); // we never read into PingUrls at the protocol layer
            ArrayList pingUrlsSent = new ArrayList(serverBlogPost.PingUrlsSent);
            ArrayList pingUrlsPending = new ArrayList();
            foreach (string pingUrl in clientBlogPost.PingUrlsPending)
                if (!pingUrlsSent.Contains(pingUrl))
                    pingUrlsPending.Add(pingUrl);
            serverBlogPost.PingUrlsPending = pingUrlsPending.ToArray(typeof(string)) as string[];
        }

        /// <summary>
        ///  Does this blog support post-sync? (default to true if we can't figure this out)
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        private static bool SynchronizationSupportedForBlog(string blogId)
        {
            try
            {
                if (!BlogSettings.BlogIdIsValid(blogId))
                    return false;

                // verify synchronization is supported for this blog service
                using (Blog blog = new Blog(blogId))
                    return blog.ClientOptions.SupportsPostSynchronization;
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception getting post sync options: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Synchronizes the editing context contents with the local contents (if the version signatures are equivalent).
        /// </summary>
        /// <param name="localContents"></param>
        /// <param name="localContentsSignature"></param>
        /// <param name="editingContext"></param>
        private static void SynchronizeLocalContentsWithEditingContext(string localContents, string localContentsSignature, IBlogPostEditingContext editingContext)
        {
            bool contentIsUpToDate = localContentsSignature == editingContext.BlogPost.ContentsVersionSignature;
            if (contentIsUpToDate)
            {
                Debug.WriteLine("RecentPostSynchronizer: Use local contents");
                editingContext.BlogPost.Contents = localContents;
            }
            else
            {
                Debug.WriteLine("RecentPostSynchronizer: Using remote contents");

                // convert image references
                ConvertImageReferencesToLocal(editingContext);
            }
        }

        private static BlogPost SafeGetPostFromServer(IWin32Window mainFrameWindow, string destinationBlogId, BlogPost blogPost)
        {
            try
            {
                if (BlogSettings.BlogIdIsValid(destinationBlogId))
                {
                    string entityName = blogPost.IsPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post);

                    if (!VerifyBlogCredentials(destinationBlogId))
                    {
                        // warn the user we couldn't synchronize
                        ShowRecentPostNotSynchronizedWarning(mainFrameWindow, entityName);
                        return null;
                    }

                    // get the recent post (with progress if it takes more than a predefined interval)
                    GetRecentPostOperation getRecentPostOperation = null;
                    using (RecentPostProgressForm progressForm = new RecentPostProgressForm(entityName))
                    {
                        progressForm.CreateControl();
                        getRecentPostOperation = new GetRecentPostOperation(new BlogClientUIContextImpl(progressForm), destinationBlogId, blogPost);
                        getRecentPostOperation.Start();
                        progressForm.ShowDialogWithDelay(mainFrameWindow, getRecentPostOperation, 3000);
                    }

                    // if we got the post then return it
                    if (!getRecentPostOperation.WasCancelled && getRecentPostOperation.ServerBlogPost != null)
                    {
                        return getRecentPostOperation.ServerBlogPost;
                    }
                    // remote server didn't have a copy of the post
                    else if (getRecentPostOperation.NoPostAvailable)
                    {
                        return null;
                    }
                    // some type of error occurred (including the user cancelled)
                    else
                    {
                        // warn the user we couldn't synchronize
                        ShowRecentPostNotSynchronizedWarning(mainFrameWindow, entityName);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error attempting to fetch blog-post from server: " + ex.ToString());
                return null;
            }
        }

        private static void ShowRecentPostNotSynchronizedWarning(IWin32Window owner, string entityName)
        {
            using (RecentPostNotSynchronizedWarningForm warningForm = new RecentPostNotSynchronizedWarningForm(entityName))
                warningForm.ShowDialog(owner);
        }

        private static bool VerifyBlogCredentials(string destinationBlogId)
        {
            using (Blog blog = new Blog(destinationBlogId))
                return blog.VerifyCredentials();
        }

        private class GetRecentPostOperation : OpenLiveWriter.CoreServices.AsyncOperation
        {

            public GetRecentPostOperation(IBlogClientUIContext uiContext, string blogId, BlogPost blogPost)
                : base(uiContext)
            {
                _uiContext = uiContext;
                _blogId = blogId;
                _blogPost = blogPost;
            }

            public BlogPost ServerBlogPost
            {
                get { return _serverBlogPost; }
            }

            /// <summary>
            /// Was a post available on the remote server? Distinguishes between the case
            /// where BlogPost is null due to an error vs. null because we couldn't get
            /// a remote copy of the post
            /// </summary>
            public bool NoPostAvailable
            {
                get
                {
                    return _noPostAvailable;
                }
            }

            public bool WasCancelled
            {
                get { return CancelRequested; }
            }

            protected override void DoWork()
            {
                using (BlogClientUIContextScope uiScope = new BlogClientUIContextScope(_uiContext))
                {
                    using (Blog blog = new Blog(_blogId))
                    {
                        // Fix bug 457160 - New post created with a new category
                        // becomes without a category when opened in WLW
                        //
                        // See also PostEditorPostSource.GetPost(string)
                        try
                        {
                            blog.RefreshCategories();
                        }
                        catch (Exception e)
                        {
                            Trace.Fail("Exception while attempting to refresh categories: " + e.ToString());
                        }

                        _serverBlogPost = blog.GetPost(_blogPost.Id, _blogPost.IsPage);
                        if (_serverBlogPost == null)
                            _noPostAvailable = true;
                    }
                }
            }

            private string _blogId;
            private BlogPost _blogPost;
            private BlogPost _serverBlogPost;
            private bool _noPostAvailable = false;
            private IBlogClientUIContext _uiContext;

        }

        private static void ConvertImageReferencesToLocal(IBlogPostEditingContext editingContext)
        {
            ImageReferenceFixer refFixer = new ImageReferenceFixer(editingContext.ImageDataList, editingContext.BlogId);

            // Create a text writer that the new html will be written to
            TextWriter htmlWriter = new StringWriter(CultureInfo.InvariantCulture);
            // Check an html image fixer that will find references and rewrite them to new paths
            HtmlReferenceFixer referenceFixer = new HtmlReferenceFixer(editingContext.BlogPost.Contents);
            // We need to update the editing context when we change an image
            ContextImageReferenceFixer contextFixer = new ContextImageReferenceFixer(editingContext);
            // Do the fixing
            referenceFixer.FixReferences(htmlWriter, new ReferenceFixer(refFixer.FixImageReferences), contextFixer.ReferenceFixedCallback);
            // Write back the new html
            editingContext.BlogPost.Contents = htmlWriter.ToString();
        }

        class ContextImageReferenceFixer
        {
            private readonly IBlogPostEditingContext _editingContext;
            internal ContextImageReferenceFixer(IBlogPostEditingContext editingContext)
            {
                _editingContext = editingContext;
            }
            public void ReferenceFixedCallback(string oldReference, string newReference)
            {
                ISupportingFile file = _editingContext.SupportingFileService.GetFileByUri(new Uri(newReference));
                if (file != null)
                {
                    foreach (BlogPostImageData imageData in _editingContext.ImageDataList)
                    {
                        // We can no longer trust the target settings for this file, so we must remove them
                        // this means the first time the object is clicked it will read the settings from the DOM
                        if (file.FileId == imageData.InlineImageFile.SupportingFile.FileId)
                        {
                            BlogPostSettingsBag settings =
                                imageData.ImageDecoratorSettings.GetSubSettings(HtmlImageTargetDecorator.Id);
                            foreach (string settingName in HtmlImageTargetDecoratorSettings.ImageReferenceFixedStaleProperties)
                                settings.Remove(settingName);
                        }
                    }
                }
            }
        }

        class ImageReferenceFixer
        {
            Hashtable urlFixupTable = new Hashtable();
            internal ImageReferenceFixer(BlogPostImageDataList list, string blogId)
            {
                string uploadDestinationContext = BlogFileUploader.GetFileUploadDestinationContext(blogId);
                foreach (BlogPostImageData imageData in list)
                {
                    if (imageData.InlineImageFile != null && imageData.InlineImageFile.GetPublishedUri(uploadDestinationContext) != null)
                    {
                        urlFixupTable[imageData.InlineImageFile.GetPublishedUri(uploadDestinationContext)] = imageData.InlineImageFile.Uri;
                        if (imageData.LinkedImageFile != null)
                            urlFixupTable[imageData.LinkedImageFile.GetPublishedUri(uploadDestinationContext)] = imageData.LinkedImageFile.Uri;
                    }
                }
            }
            internal string FixImageReferences(BeginTag tag, string reference)
            {
                if (!UrlHelper.IsUrl(reference))
                {
                    //Warning: fixing of relative paths is not currently supported.  This would only be
                    //a problem for post synchronization if blog servers returned relative paths when uploading
                    //(which they can't since this interface requires a URI), or if the blog service re-wrote the
                    //URL when the content was published (which is likely to cause the URL to be unmatchable
                    //anyhow). Sharepoint images cause this path to be hit, but image synchronization is not
                    //supported for SharePoint anyhow since the URLs are always re-written by the server.
                    Debug.WriteLine("warning: relative image URLs cannot be resolved for edited posts");
                }
                else
                {
                    Uri fixedImageUri = (Uri)urlFixupTable[new Uri(reference)];
                    if (fixedImageUri != null)
                    {
                        Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "RecentPostSyncrhonizer: converting remote image reference [{0}] to local reference", reference));
                        reference = fixedImageUri.ToString();
                    }
                }

                return reference;
            }
        }
    }
}
