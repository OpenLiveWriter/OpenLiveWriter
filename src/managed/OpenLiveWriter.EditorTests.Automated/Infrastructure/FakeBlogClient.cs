// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
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

        /// <summary>
        /// When set, <see cref="NewMediaObject"/> throws for the matching file name so
        /// tests can exercise the upload-failure path.
        /// </summary>
        public string FailUploadForFileName { get; set; }

        public string NewPost(string blogId, BlogPost post, bool publish)
        {
            NewPostCount++;
            Capture(blogId, post, publish);
            if (string.IsNullOrEmpty(post.Id))
                post.Id = "fake-post-1";
            return post.Id;
        }

        public void EditPost(string blogId, BlogPost post, bool publish)
        {
            EditPostCount++;
            Capture(blogId, post, publish);
        }

        public string NewMediaObject(string blogId, string fileName, string mimeType, byte[] bits)
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
            return url;
        }

        private void Capture(string blogId, BlogPost post, bool publish)
        {
            LastBlogId = blogId;
            LastPost = post;
            LastPublish = publish;
        }
    }
}
