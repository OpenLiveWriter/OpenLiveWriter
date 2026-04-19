// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.Tests.BlogClient.Clients
{
    [TestClass]
    public class BloggerMoreTagConversionTests
    {
        /// <summary>
        /// The anchor tag that Google Blogger v3 API uses for the extended entry break.
        /// </summary>
        private const string BloggerAnchorTag = "<a name=\"more\"></a>";

        [TestMethod]
        public void ConvertFromBlogger_AnchorTagIsConvertedToMoreComment()
        {
            string bloggerContent = "<p>First part</p>" + BloggerAnchorTag + "<p>Second part</p>";
            string expected = "<p>First part</p>" + BlogPost.ExtendedEntryBreak + "<p>Second part</p>";

            string result = bloggerContent.Replace(BloggerAnchorTag, BlogPost.ExtendedEntryBreak);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConvertFromBlogger_ContentWithoutAnchorTagIsUnchanged()
        {
            string content = "<p>Some content without the anchor tag</p>";

            string result = content.Replace(BloggerAnchorTag, BlogPost.ExtendedEntryBreak);

            Assert.AreEqual(content, result);
        }

        [TestMethod]
        public void ConvertFromBlogger_MultipleAnchorTagsAreAllConverted()
        {
            string bloggerContent = "<p>Part 1</p>" + BloggerAnchorTag + "<p>Part 2</p>" + BloggerAnchorTag + "<p>Part 3</p>";
            string expected = "<p>Part 1</p>" + BlogPost.ExtendedEntryBreak + "<p>Part 2</p>" + BlogPost.ExtendedEntryBreak + "<p>Part 3</p>";

            string result = bloggerContent.Replace(BloggerAnchorTag, BlogPost.ExtendedEntryBreak);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConvertToBlogger_MoreCommentIsConvertedToAnchorTag()
        {
            string localContent = "<p>First part</p>" + BlogPost.ExtendedEntryBreak + "<p>Second part</p>";
            string expected = "<p>First part</p>" + BloggerAnchorTag + "<p>Second part</p>";

            string result = localContent.Replace(BlogPost.ExtendedEntryBreak, BloggerAnchorTag);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConvertFromBlogger_NullContentIsHandled()
        {
            string content = null;

            // Mirrors the null check in ConvertToBlogPost
            string result = content;
            if (result != null)
            {
                result = result.Replace(BloggerAnchorTag, BlogPost.ExtendedEntryBreak);
            }

            Assert.IsNull(result);
        }
    }
}
