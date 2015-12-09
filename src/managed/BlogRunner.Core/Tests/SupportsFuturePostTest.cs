// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.Extensibility.BlogClient;

namespace BlogRunner.Core.Tests
{
    public class SupportsFuturePostTest : PostTest
    {
        protected internal override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            blogPost.Title = "Future post test";
            blogPost.Contents = "foo bar";
            blogPost.DatePublishedOverride = DateTime.Now.AddDays(12.0);
        }

        protected internal override void HandleResult(string homepageHtml, ITestResults results)
        {
            results.AddResult("futurePublishDateWarning", YES);
        }

        protected internal override bool HandleTimeout(TimeoutException te, ITestResults results)
        {
            results.AddResult("futurePublishDateWarning", NO);
            return true;
        }

    }
}
