// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using OpenLiveWriter.Extensibility.BlogClient;

namespace BlogRunner.Core.Tests
{
    public class SupportsEmptyTitlesTest : Test
    {
        public override void DoTest(BlogRunner.Core.Config.Blog blog, OpenLiveWriter.Extensibility.BlogClient.IBlogClient blogClient, ITestResults results)
        {
            BlogPost post = new BlogPost();
            post.Contents = "foo";
            post.Title = "";

            string etag;
            XmlDocument remotePost;
            try
            {
                string newPostId = blogClient.NewPost(blog.BlogId, post, null, true, out etag, out remotePost);
                results.AddResult("supportsEmptyTitles", YES);

                if (CleanUpPosts)
                    blogClient.DeletePost(blog.BlogId, newPostId, true);
            }
            catch
            {
                results.AddResult("supportsEmptyTitles", NO);
            }
        }
    }
}
