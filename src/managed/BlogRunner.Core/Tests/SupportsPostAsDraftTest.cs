﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.Extensibility.BlogClient;

namespace BlogRunner.Core.Tests
{
    public class SupportsPostAsDraftTest : PostTest
    {
        protected internal override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            blogPost.Title = "Post as draft test";
            blogPost.Contents = "foo bar";
            publish = false;
        }

        protected internal override void HandleResult(string homepageHtml, ITestResults results)
        {
            results.AddResult("supportsPostAsDraft", NO);
        }

        protected internal override bool HandleTimeout(TimeoutException te, ITestResults results)
        {
            results.AddResult("supportsPostAsDraft", YES);
            return true;
        }
    }
}
