// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenLiveWriter.Publishing;

// NOTE: This file lives under Infrastructure/ (not a Publish/ folder) on purpose — the
// repository .gitignore ignores "publish/", which on a case-insensitive filesystem also
// swallows a "Publish/" test folder and would leave this essential test double untracked.
namespace OpenLiveWriter.EditorTests.Automated.Publish
{
    // ---------------------------------------------------------------------------
    // Test double for the REAL cross-platform publish contract.
    //
    // Group C/E/G exercise the ported OpenLiveWriter.Publishing types directly
    // (BlogPost, IBlogClient, MetaWeblogXmlRpcClient, EditorContentPublisher,
    // ImagePublisher, XmlCharacterHelper). FakeBlogClient is a stand-in transport that
    // implements the real IBlogClient so we can assert the NewPost/EditPost/
    // NewMediaObject/GetCategories round-trips without hitting a network endpoint.
    // ---------------------------------------------------------------------------

    /// <summary>Captures the last post submitted so tests can assert on the payload.</summary>
    public sealed class FakeBlogClient : IBlogClient
    {
        public FakeBlogClient(IBlogClientOptions options = null)
        {
            Options = options ?? BlogClientOptions.Default;
        }

        public IBlogClientOptions Options { get; }

        public string LastBlogId { get; private set; }
        public BlogPost LastPost { get; private set; }
        public bool LastPublish { get; private set; }
        public int NewPostCount { get; private set; }
        public int EditPostCount { get; private set; }

        /// <summary>Records every <c>newMediaObject</c> upload so tests can assert on it.</summary>
        public sealed class MediaUpload
        {
            public string BlogId { get; set; }
            public string FileName { get; set; }
            public string MimeType { get; set; }
            public byte[] Bits { get; set; }
            public string ReturnedUrl { get; set; }
        }

        public List<MediaUpload> MediaUploads { get; } = new List<MediaUpload>();
        public int NewMediaObjectCount => MediaUploads.Count;

        /// <summary>Categories the fake returns from <see cref="GetCategoriesAsync"/>.</summary>
        public List<BlogPostCategory> AvailableCategories { get; } = new List<BlogPostCategory>();
        public int GetCategoriesCount { get; private set; }
        public string LastGetCategoriesBlogId { get; private set; }

        /// <summary>Posts the fake returns from <see cref="GetRecentPostsAsync"/>.</summary>
        public List<ServerPost> RecentPosts { get; } = new List<ServerPost>();
        public int GetRecentPostsCount { get; private set; }
        public string LastGetRecentPostsBlogId { get; private set; }
        public int LastGetRecentPostsRequestedCount { get; private set; }

        /// <summary>Pages the fake returns from <see cref="GetPagesAsync"/>.</summary>
        public List<ServerPost> Pages { get; } = new List<ServerPost>();
        public int GetPagesCount { get; private set; }
        public string LastGetPagesBlogId { get; private set; }

        /// <summary>The post <see cref="GetPostAsync"/> returns (set by the test).</summary>
        public ServerPost NextGetPost { get; set; }
        public int GetPostCount { get; private set; }
        public string LastGetPostId { get; private set; }

        public int NewPageCount { get; private set; }
        public int EditPageCount { get; private set; }

        /// <summary>
        /// When set, <see cref="NewMediaObjectAsync"/> throws for the matching file name so
        /// tests can exercise the upload-failure path.
        /// </summary>
        public string FailUploadForFileName { get; set; }

        public Task<string> NewPostAsync(string blogId, BlogPost post, bool publish)
        {
            NewPostCount++;
            Capture(blogId, post, publish);
            if (string.IsNullOrEmpty(post.Id))
                post.Id = "fake-post-1";
            return Task.FromResult(post.Id);
        }

        public Task EditPostAsync(string blogId, BlogPost post, bool publish)
        {
            EditPostCount++;
            Capture(blogId, post, publish);
            return Task.CompletedTask;
        }

        public Task<string> NewMediaObjectAsync(string blogId, string fileName, string mimeType, byte[] bits)
        {
            if (!string.IsNullOrEmpty(FailUploadForFileName) &&
                string.Equals(FailUploadForFileName, fileName, StringComparison.Ordinal))
            {
                throw new BlogClientPublishException($"Simulated upload failure for '{fileName}'.");
            }

            string url = $"https://cdn.example.com/uploads/{fileName}";
            MediaUploads.Add(new MediaUpload
            {
                BlogId = blogId,
                FileName = fileName,
                MimeType = mimeType,
                Bits = bits,
                ReturnedUrl = url
            });
            return Task.FromResult(url);
        }

        public Task<IReadOnlyList<BlogPostCategory>> GetCategoriesAsync(string blogId)
        {
            GetCategoriesCount++;
            LastGetCategoriesBlogId = blogId;
            return Task.FromResult<IReadOnlyList<BlogPostCategory>>(AvailableCategories.AsReadOnly());
        }

        public Task<IReadOnlyList<ServerPost>> GetRecentPostsAsync(string blogId, int count)
        {
            GetRecentPostsCount++;
            LastGetRecentPostsBlogId = blogId;
            LastGetRecentPostsRequestedCount = count;
            // Honor the requested count like a real server would.
            int take = count < 0 ? RecentPosts.Count : Math.Min(count, RecentPosts.Count);
            return Task.FromResult<IReadOnlyList<ServerPost>>(RecentPosts.GetRange(0, take).AsReadOnly());
        }

        public Task<ServerPost> GetPostAsync(string postId)
        {
            GetPostCount++;
            LastGetPostId = postId;
            return Task.FromResult(NextGetPost);
        }

        public Task<IReadOnlyList<ServerPost>> GetPagesAsync(string blogId)
        {
            GetPagesCount++;
            LastGetPagesBlogId = blogId;
            return Task.FromResult<IReadOnlyList<ServerPost>>(Pages.AsReadOnly());
        }

        public Task<string> NewPageAsync(string blogId, BlogPost post, bool publish)
        {
            NewPageCount++;
            Capture(blogId, post, publish);
            if (string.IsNullOrEmpty(post.Id))
                post.Id = "fake-page-1";
            return Task.FromResult(post.Id);
        }

        public Task EditPageAsync(string blogId, BlogPost post, bool publish)
        {
            EditPageCount++;
            Capture(blogId, post, publish);
            return Task.CompletedTask;
        }

        private void Capture(string blogId, BlogPost post, bool publish)
        {
            LastBlogId = blogId;
            LastPost = post;
            LastPublish = publish;
        }
    }
}
