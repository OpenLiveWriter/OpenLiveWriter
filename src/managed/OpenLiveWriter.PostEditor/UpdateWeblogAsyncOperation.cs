// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.HTML;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.PostEditor.SupportingFiles;

namespace OpenLiveWriter.PostEditor
{
    public enum UpdateWeblogResult
    {
        Completed,
        Failed,
        Cancelled,
        CancelledTooLate
    }

    public class UpdateWeblogAsyncOperation : OpenLiveWriter.CoreServices.AsyncOperation
    {
        private IBlogClientUIContext _uiContext;

        public UpdateWeblogAsyncOperation(IBlogClientUIContext uiContext, IBlogPostPublishingContext publishingContext, bool publish)
            : base(uiContext)
        {
            _uiContext = uiContext;
            _publishingContext = publishingContext;
            _publish = publish;
        }

        protected override void DoWork()
        {
            using (new BlogClientUIContextScope(_uiContext))
            {
                // NOTE: LocalSupportingFileUploader temporarily modifies the contents of the BlogPost.Contents to
                // have the correct remote references to embedded images, etc. When it is disposed it returns the
                // value of BlogPost.Contents to its original value.
                using (LocalSupportingFileUploader supportingFileUploader = new LocalSupportingFileUploader(_publishingContext))
                {
                    //hook to publish files before the post is published
                    supportingFileUploader.UploadFilesBeforePublish();

                    // now submit the post
                    using (Blog blog = new Blog(_publishingContext.EditingContext.BlogId))
                    {
                        BlogPost blogPost = _publishingContext.GetBlogPostForPublishing();

                        PostResult postResult;
                        if (blogPost.IsNew)
                        {
                            postResult = blog.NewPost(blogPost, _publishingContext, _publish);
                        }
                        else
                        {
                            postResult = blog.EditPost(blogPost, _publishingContext, _publish);
                        }

                        // try to get published post hash and permalink (but failure shouldn't
                        // stop publishing -- if we allow this then the user will end up
                        // "double-posting" content because the actual publish did succeed
                        // whereas Writer's status would indicate it hadn't)
                        string publishedPostHash = null, permaLink = null, slug = null;
                        try
                        {
                            BlogPost publishedPost = blog.GetPost(postResult.PostId, blogPost.IsPage);
                            publishedPostHash = BlogPost.CalculateContentsSignature(publishedPost);
                            permaLink = publishedPost.Permalink;
                            slug = publishedPost.Slug;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Unexpected error retrieving published post: " + ex.ToString());
                        }

                        BlogPostPublishingResult publishingResult = new BlogPostPublishingResult();

                        // Hook to publish files after the post is published (note that if this
                        // fails it is not a fatal error since the publish itself already
                        // succeeded. In the case of a failure note the exception so that the
                        // UI layer can inform/prompt the user as appropriate
                        try
                        {
                            supportingFileUploader.UploadFilesAfterPublish(postResult.PostId);
                        }
                        catch (Exception ex)
                        {
                            publishingResult.AfterPublishFileUploadException = ex;
                        }

                        // populate the publishing result
                        publishingResult.PostResult = postResult;
                        publishingResult.PostPermalink = permaLink;
                        publishingResult.PostContentHash = publishedPostHash;
                        publishingResult.Slug = slug;
                        publishingResult.PostPublished = _publish;

                        // set the post result
                        _publishingContext.SetPublishingPostResult(publishingResult);

                        // send pings if appropriate
                        if (_publish && PostEditorSettings.Ping && !blogPost.IsTemporary)
                            SafeAsyncSendPings(blog.Name, blog.HomepageUrl);
                    }
                }
            }
        }

        private void SafeAsyncSendPings(string blogName, string blogHomepageUrl)
        {
            try
            {
                string[] pingUrls = PostEditorSettings.PingUrls;

                if (pingUrls.Length > 0)
                {
                    PingHelper ph = new PingHelper(blogName, blogHomepageUrl, pingUrls);
                    Thread t = ThreadHelper.NewThread(new ThreadStart(ph.ThreadStart), "AsyncPing", true, false, false);
                    t.Start();

                    UpdateProgress(-1, -1, Res.Get((pingUrls.Length == 1 ? StringId.SendingPing : StringId.SendingPings)));
                    Thread.Sleep(750);
                }

            }
            catch (Exception e)
            {
                Trace.Fail("Exception while running ping logic: " + e.ToString());
            }
        }

        private class PingHelper
        {
            private readonly XmlRpcString _blogName;
            private readonly XmlRpcString _blogUrl;

            private readonly string[] _pingUrls;

            public PingHelper(string blogName, string blogUrl, string[] pingUrls)
            {
                _blogName = new XmlRpcString(blogName);
                _blogUrl = new XmlRpcString(blogUrl);
                _pingUrls = pingUrls;
            }

            public void ThreadStart()
            {
                foreach (string url in _pingUrls)
                {
                    try
                    {
                        Uri uri = new Uri(url);
                        if (uri.Scheme.ToLower(CultureInfo.InvariantCulture) == "http" || uri.Scheme.ToLower(CultureInfo.InvariantCulture) == "https")
                        {
                            XmlRpcClient client = new XmlRpcClient(url, ApplicationEnvironment.UserAgent);
                            client.CallMethod("weblogUpdates.ping", _blogName, _blogUrl);
                        }
                    }
                    catch (Exception e)
                    {
                        if (ApplicationDiagnostics.VerboseLogging)
                            Trace.Fail("Failure while pinging " + url + ": " + e.ToString());
                    }
                }
            }
        }

        private IBlogPostPublishingContext _publishingContext;
        private bool _publish;

        /// <summary>
        /// Class which manages resolution of local file references by uploading
        /// the files to the publishing target, modifying the post contents to
        /// reflect the appropriate target URLs, and finally restoring the post
        /// contents to point at the local files for additional local editing).
        /// </summary>
        private class LocalSupportingFileUploader : IDisposable
        {
            private Blog _blog;
            BlogPostReferenceFixer _referenceFixer;
            public LocalSupportingFileUploader(IBlogPostPublishingContext publishingContext)
            {
                // save references to parameters/post contents
                _publishingContext = publishingContext;
                _originalPostContents = _publishingContext.EditingContext.BlogPost.Contents;
                _blog = new Blog(_publishingContext.EditingContext.BlogId);
            }

            public void UploadFilesBeforePublish()
            {
                // create a file uploader
                using (BlogFileUploader fileUploader = BlogFileUploader.CreateFileUploader(_blog, _publishingContext.EditingContext.ServerSupportingFileDirectory))
                {
                    // connect to the file uploader
                    fileUploader.Connect();

                    // upload the files and fixup references within the contents of the blog post
                    string htmlContents = _publishingContext.EditingContext.BlogPost.Contents;

                    _referenceFixer = new BlogPostReferenceFixer(htmlContents, _publishingContext);
                    _referenceFixer.Parse();

                    string fixedHtml = HtmlReferenceFixer.FixLocalFileReferences(htmlContents, _referenceFixer.GetFileUploadReferenceFixer(fileUploader));
                    _publishingContext.EditingContext.BlogPost.Contents = fixedHtml;
                }
            }

            public void UploadFilesAfterPublish(string postId)
            {
                // create a file uploader
                using (BlogFileUploader fileUploader = BlogFileUploader.CreateFileUploader(_blog, _publishingContext.EditingContext.ServerSupportingFileDirectory))
                {
                    // connect to the file uploader
                    fileUploader.Connect();

                    _referenceFixer.UploadFilesAfterPublish(postId, fileUploader);
                }
            }

            public void Dispose()
            {
                try
                {
                    _blog.Dispose();
                    // restore the contents of the BlogPost
                    _publishingContext.EditingContext.BlogPost.Contents = _originalPostContents;
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception occurred during Dispose of UpdateWeblogAsyncOperation:" + ex.ToString());
                }

                GC.SuppressFinalize(this);
            }

            ~LocalSupportingFileUploader()
            {
                Debug.Fail("Failed to dispose LocalSupportingFileUploader");
            }

            private IBlogPostPublishingContext _publishingContext;
            private string _originalPostContents;
        }
    }

    internal class BlogPostReferenceFixer : LightWeightHTMLDocumentIterator
    {
        FileUploadWorker _fileUploadWorker;
        IBlogPostPublishingContext _uploadContext;
        SupportingFileReferenceList _fileReferenceList;
        internal BlogPostReferenceFixer(string html, IBlogPostPublishingContext publishingContext)
            : base(html)
        {
            _uploadContext = publishingContext;
            _fileUploadWorker = new FileUploadWorker(_uploadContext.BlogPost.Id);
            _fileReferenceList = SupportingFileReferenceList.CalculateReferencesForPublish(publishingContext.EditingContext);
        }

        public void UploadFilesAfterPublish(string postId, BlogFileUploader fileUploader)
        {
            _fileUploadWorker.DoAfterPostUploadWork(fileUploader, postId);
        }

        public ReferenceFixer GetFileUploadReferenceFixer(BlogFileUploader uploader)
        {
            return new ReferenceFixer(new LocalFileTransformer(this, uploader).Transform);
        }

        class LocalFileTransformer
        {
            BlogPostReferenceFixer _referenceFixer;
            ISupportingFileService _fileService;
            BlogFileUploader _uploader;
            public LocalFileTransformer(BlogPostReferenceFixer referenceFixer, BlogFileUploader uploader)
            {
                _referenceFixer = referenceFixer;
                _fileService = _referenceFixer._uploadContext.EditingContext.SupportingFileService;
                _uploader = uploader;
            }
            internal string Transform(BeginTag tag, string reference)
            {
                if (UrlHelper.IsUrl(reference))
                {
                    Uri localReferenceUri = new Uri(reference);

                    /*
                     * If we need to drop a hint to the photo uploader about
                     * whether Lightbox-like preview is enabled, so that we know to link to
                     * the image itself rather than the photo "self" page on photos.live.com;
                     * this is where we would figure that out (by looking at the tag) and
                     * pass that info through to the DoUploadWork call.
                     */
                    bool isLightboxCloneEnabled = false;

                    _referenceFixer._fileUploadWorker.DoUploadWork(reference, _uploader, isLightboxCloneEnabled);

                    ISupportingFile supportingFile = _fileService.GetFileByUri(localReferenceUri);
                    if (supportingFile != null)
                    {
                        Uri uploadUri = supportingFile.GetUploadInfo(_uploader.DestinationContext).UploadUri;
                        if (uploadUri != null)
                            return UrlHelper.SafeToAbsoluteUri(uploadUri);
                    }
                }
                return reference;
            }
        }

        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag != null && LightWeightHTMLDocument.AllUrlElements.ContainsKey(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
            {
                Attr attr = tag.GetAttribute((string)LightWeightHTMLDocument.AllUrlElements[tag.Name.ToUpper(CultureInfo.InvariantCulture)]);
                if (attr != null && attr.Value != null)
                {
                    if (UrlHelper.IsUrl(attr.Value))
                    {
                        Uri reference = new Uri(attr.Value);
                        if (_fileReferenceList.IsReferenced(reference))
                        {
                            ISupportingFile supportingFile = _fileReferenceList.GetSupportingFileByUri(reference);
                            if (supportingFile.Embedded)
                                AddFileReference(supportingFile);
                        }
                    }
                }
            }
            base.OnBeginTag(tag);
        }

        private void AddFileReference(ISupportingFile supportingFile)
        {
            _fileUploadWorker.AddFile(supportingFile);
        }

        private class FileUploadWorker
        {
            ArrayList _fileList = new ArrayList();
            Hashtable _uploadedFiles = new Hashtable();
            private string _postId;

            public FileUploadWorker(string postId)
            {
                _postId = postId;
            }
            public void AddFile(ISupportingFile supportingFile)
            {
                _fileList.Add(supportingFile);
            }
            public void DoUploadWork(string fileReference, BlogFileUploader fileUploader, bool isLightboxCloneEnabled)
            {
                // Get both strings into the same state which is unescaped
                string unescapedFileReference = new Uri(fileReference).ToString();
                ISupportingFile file = null;
                foreach (ISupportingFile supportingFile in _fileList)
                {
                    if (supportingFile.FileUri.ToString() == unescapedFileReference)
                    {
                        file = supportingFile;
                        break;
                    }
                }

                if (file == null)
                {
                    string listString = "";
                    foreach (ISupportingFile supportingFile in _fileList)
                    {
                        listString += supportingFile.FileUri + "\r\n";
                    }
                    Trace.Fail(String.Format(CultureInfo.InvariantCulture, "Reference found to file that does not exist in SupportingFileService \r\nfileReference: {0}\r\n_fileList:\r\n{1}", fileReference, listString));
                    return;
                }

                string uploadContext = fileUploader.DestinationContext;

                ISupportingFileUploadInfo uploadInfo = file.GetUploadInfo(uploadContext);
                FileUploadContext fileUploadContext = new FileUploadContext(fileUploader, _postId, file, uploadInfo, isLightboxCloneEnabled);

                if (fileUploader.DoesFileNeedUpload(file, fileUploadContext))
                {
                    if (!_uploadedFiles.ContainsKey(file.FileId))
                    {
                        _uploadedFiles[file.FileId] = file;
                        Uri uploadUri = fileUploader.DoUploadWorkBeforePublish(fileUploadContext);
                        if (uploadUri != null)
                        {
                            file.MarkUploaded(uploadContext, uploadUri);
                            Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "File Uploaded: {0}", file.FileName));
                        }
                    }
                    else
                    {
                        Trace.Fail("This file has already been uploaded during this publish operation: " + file.FileName);
                    }
                }

            }

            public void DoAfterPostUploadWork(BlogFileUploader fileUploader, string postId)
            {
                foreach (ISupportingFile file in _fileList)
                {
                    string uploadContext = fileUploader.DestinationContext;
                    if (_uploadedFiles.ContainsKey(file.FileId))
                    {
                        ISupportingFileUploadInfo uploadInfo = file.GetUploadInfo(uploadContext);
                        FileUploadContext fileUploadContext = new FileUploadContext(fileUploader, postId, file, uploadInfo, false);
                        fileUploader.DoUploadWorkAfterPublish(fileUploadContext);
                    }
                }
            }

            private class FileUploadContext : IFileUploadContext
            {
                private string _blogId;
                private string _postId;
                private ISupportingFile _supportingFile;
                private ISupportingFileUploadInfo _uploadInfo;
                private readonly bool _forceDirectImageLink;
                private BlogFileUploader _fileUploader;
                public FileUploadContext(BlogFileUploader fileUploader, string postId, ISupportingFile supportingFile, ISupportingFileUploadInfo uploadInfo, bool forceDirectImageLink)
                {
                    _fileUploader = fileUploader;
                    _blogId = fileUploader.BlogId;
                    _postId = postId;
                    _supportingFile = supportingFile;
                    _uploadInfo = uploadInfo;
                    _forceDirectImageLink = forceDirectImageLink;
                }

                public string BlogId
                {
                    get { return _blogId; }
                }

                public string PostId
                {
                    get { return _postId; }
                }

                public string PreferredFileName
                {
                    get { return _supportingFile.FileName; }
                }

                public FileUploadRole Role
                {
                    get
                    {
                        ImageFileData data = new ImageFileData(_supportingFile);
                        switch (data.Relationship)
                        {
                            case ImageFileRelationship.Inline:
                                return FileUploadRole.InlineImage;
                            case ImageFileRelationship.Linked:
                                return FileUploadRole.LinkedImage;
                            default:
                                return FileUploadRole.File;
                        }
                    }
                }

                public Stream GetContents()
                {
                    return new FileStream(_supportingFile.FileUri.LocalPath, FileMode.Open, FileAccess.Read);
                }

                public string GetContentsLocalFilePath()
                {
                    return _supportingFile.FileUri.LocalPath;
                }

                public string FormatFileName(string filename)
                {
                    string conflictToken = filename == _supportingFile.FileName
                        ? _supportingFile.FileNameUniqueToken
                        : Guid.NewGuid().ToString();

                    if (conflictToken == "0") //don't include the conflict token for the first version of the file.
                        conflictToken = null;
                    return _fileUploader.FormatUploadFileName(filename, conflictToken);
                }

                public IProperties Settings
                {
                    get
                    {
                        return _uploadInfo.UploadSettings;
                    }
                }

                public bool ForceDirectImageLink
                {
                    get { return _forceDirectImageLink; }
                }
            }
        }
    }
}
