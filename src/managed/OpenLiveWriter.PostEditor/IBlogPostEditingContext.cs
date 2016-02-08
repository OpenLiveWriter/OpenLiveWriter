// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor.Controls;
using OpenLiveWriter.PostEditor.SupportingFiles;

namespace OpenLiveWriter.PostEditor
{
    public interface IBlogPostEditingContext
    {
        string BlogId { get; }
        BlogPost BlogPost { get; }
        BlogPostSupportingFileStorage SupportingFileStorage { get; }
        string ServerSupportingFileDirectory { get; }
        BlogPostImageDataList ImageDataList { get; }
        BlogPostExtensionDataList ExtensionDataList { get; }
        ISupportingFileService SupportingFileService { get; }
        PostEditorFile LocalFile { get; }
        PostEditorFile AutoSaveLocalFile { get; }
        // @SharedCanvas - this should not be here, it is only hear because it was an
        // easy way to get the information from BlogPostManagingEditor into the CE
    }

    public class BlogPostEditingContext : IBlogPostEditingContext
    {
        public BlogPostEditingContext(string destinationBlogId, BlogPost blogPost)
            : this(destinationBlogId, blogPost, PostEditorFile.CreateNew(PostEditorFile.DraftsFolder))
        {
        }

        public BlogPostEditingContext(string destinationBlogId, BlogPost blogPost, BlogPostExtensionDataList extensionDataList)
            : this(destinationBlogId, blogPost, PostEditorFile.CreateNew(PostEditorFile.DraftsFolder))
        {
            _extensionDataList = extensionDataList;
        }

        public BlogPostEditingContext(string destinationBlogId, BlogPost blogPost, PostEditorFile localFile, PostEditorFile autoSaveLocalFile, string serverSupportingFileDirectory, BlogPostSupportingFileStorage supportingFileStorage, BlogPostImageDataList imageDataList, BlogPostExtensionDataList extensionDataList, ISupportingFileService supportingFileService)
            : this(destinationBlogId, blogPost, localFile)
        {
            _serverSupportingFileDirectory = serverSupportingFileDirectory;
            _supportingFileStorage = supportingFileStorage;
            _imageDataList = imageDataList;
            _extensionDataList = extensionDataList;
            _fileService = supportingFileService;
            _autoSaveLocalFile = autoSaveLocalFile;
        }

        private BlogPostEditingContext(string destinationBlogId, BlogPost blogPost, PostEditorFile localFile)
        {
            _blogId = destinationBlogId;
            _blogPost = blogPost;
            _localFile = localFile;
        }

        public string BlogId
        {
            get { return _blogId; }
        }
        private string _blogId;

        public PostEditorFile LocalFile
        {
            get { return _localFile; }
        }
        private PostEditorFile _localFile;

        public PostEditorFile AutoSaveLocalFile
        {
            get { return _autoSaveLocalFile; }
        }
        private PostEditorFile _autoSaveLocalFile;

        public BlogPost BlogPost
        {
            get { return _blogPost; }
        }
        private BlogPost _blogPost;

        public string ServerSupportingFileDirectory
        {
            get
            {
                return _serverSupportingFileDirectory;
            }
        }
        private string _serverSupportingFileDirectory = String.Empty;

        public BlogPostSupportingFileStorage SupportingFileStorage
        {
            get
            {
                if (_supportingFileStorage == null)
                    _supportingFileStorage = new BlogPostSupportingFileStorage();
                return _supportingFileStorage;
            }
        }
        private BlogPostSupportingFileStorage _supportingFileStorage;

        public BlogPostImageDataList ImageDataList
        {
            get
            {
                if (_imageDataList == null)
                    _imageDataList = new BlogPostImageDataList();
                return _imageDataList;
            }
        }
        private BlogPostImageDataList _imageDataList;

        public BlogPostExtensionDataList ExtensionDataList
        {
            get
            {
                if (_extensionDataList == null)
                    _extensionDataList = new BlogPostExtensionDataList(SupportingFileService);
                return _extensionDataList;
            }
        }
        public BlogPostExtensionDataList _extensionDataList;

        public ISupportingFileService SupportingFileService
        {
            get
            {
                if (_fileService == null)
                    _fileService = new SupportingFileService(SupportingFileStorage);
                return _fileService;
            }
        }
        private ISupportingFileService _fileService;

        #region IBlogPostEditingContext Members

        public string GetPostSpellingContextDirectory()
        {
            // @SharedCanvas - if this is null then Ignore All entries will not persist
            return null;
        }

        #endregion
    }

}
