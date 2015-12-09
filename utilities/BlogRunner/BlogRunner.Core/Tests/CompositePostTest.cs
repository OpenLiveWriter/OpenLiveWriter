using System;
using System.Collections.Generic;
using System.Text;
using OpenLiveWriter.Extensibility.BlogClient;

namespace BlogRunner.Core.Tests
{
    public class CompositePostTest : PostTest
    {
        private List<PostTest> tests = new List<PostTest>();

        public CompositePostTest(params PostTest[] tests)
        {
            this.tests.AddRange(tests);
        }

        protected internal override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            foreach (PostTest test in tests)
            {
                test.PreparePost(blogPost, ref publish);
            }
        }

        protected internal override void HandleResult(string homepageHtml, ITestResults results)
        {
            foreach (PostTest test in tests)
            {
                test.HandleResult(homepageHtml, results);
            }
        }
    }
}
