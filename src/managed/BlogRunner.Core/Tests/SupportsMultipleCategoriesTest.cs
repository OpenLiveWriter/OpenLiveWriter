// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using BlogRunner.Core.Config;
using OpenLiveWriter.Extensibility.BlogClient;
using System.Configuration;

namespace BlogRunner.Core.Tests
{
    public class SupportsMultipleCategoriesTest : RoundtripTest
    {
        protected internal override void PreparePost(Blog blog, IBlogClient blogClient, OpenLiveWriter.Extensibility.BlogClient.BlogPost blogPost, ref bool? publish)
        {
            BlogPostCategory[] categories = blogClient.GetCategories(blog.BlogId);
            if (categories.Length < 2)
                throw new InvalidOperationException("Blog " + blog.HomepageUrl + " does not have enough categories for the SupportsMultipleCategories test to be performed");
            BlogPostCategory[] newCategories = new BlogPostCategory[2];
            newCategories[0] = categories[0];
            newCategories[1] = categories[1];
            blogPost.Categories = newCategories;
        }

        protected internal override void HandleResult(OpenLiveWriter.Extensibility.BlogClient.BlogPost blogPost, ITestResults results)
        {
            if (blogPost.Categories.Length == 2)
                results.AddResult("supportsMultipleCategories", YES);
            else
                results.AddResult("supportsMultipleCategories", NO);
        }
    }
}
