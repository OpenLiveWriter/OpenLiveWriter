using System;
using System.Collections.Generic;
using System.Text;
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
