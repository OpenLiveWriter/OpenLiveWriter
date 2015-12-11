// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.PostEditor
{

    public sealed class PostHelper
    {
        public static BlogPost SafeRetrievePost(PostInfo postInfo, int timeoutMs)
        {
            try
            {
                // null for invalid blogs
                if (!BlogSettings.BlogIdIsValid(postInfo.BlogId))
                    return null;

                // null if the user can't authenticate
                using (Blog blog = new Blog(postInfo.BlogId))
                {
                    if (!blog.VerifyCredentials())
                        return null;
                }

                // fire up the get post thread
                GetPostThread getPostThread = new GetPostThread(postInfo);
                Thread thread = ThreadHelper.NewThread(new ThreadStart(getPostThread.ThreadMain), "GetPostThread", true, false, true);
                thread.Start();

                // wait for it to complete
                thread.Join(timeoutMs);

                // return the post if we successfully got one
                BlogPost blogPost = getPostThread.BlogPost;
                if (blogPost != null)
                {
                    // Clone in case there are ever issues sharing these across threads
                    // (not aware of any right now)
                    return blogPost.Clone() as BlogPost;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private class GetPostThread
        {
            public GetPostThread(PostInfo postInfo)
            {
                _postInfo = postInfo;
            }

            public void ThreadMain()
            {
                try
                {
                    using (Blog blog = new Blog(_postInfo.BlogId))
                    {
                        _blogPost = blog.GetPost(_postInfo.BlogPostId, _postInfo.IsPage);
                    }
                }
                catch
                {
                }
            }

            public BlogPost BlogPost
            {
                get { return _blogPost; }
            }
            private volatile BlogPost _blogPost;

            private readonly PostInfo _postInfo;
        }
    }
}
