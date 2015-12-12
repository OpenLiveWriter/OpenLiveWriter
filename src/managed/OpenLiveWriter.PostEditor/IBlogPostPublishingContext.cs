// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.PostEditor
{

    public interface IBlogPostPublishingContext : INewCategoryContext, IBlogPostEditingContext
    {
        IBlogPostEditingContext EditingContext { get; }
        Blog Blog { get; }
        BlogPost GetBlogPostForPublishing();
        void SetPublishingPostResult(BlogPostPublishingResult postResult);
    }

    public class BlogPostPublishingResult
    {
        public PostResult PostResult = new PostResult();
        public string PostPermalink = null;
        public string Slug = null;
        public string PostContentHash = null;
        public bool PostPublished = false;
        public Exception AfterPublishFileUploadException = null;
    }
}
