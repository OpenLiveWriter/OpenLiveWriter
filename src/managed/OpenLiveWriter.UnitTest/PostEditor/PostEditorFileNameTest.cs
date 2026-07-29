// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    /// <summary>
    /// Tests for PostEditorFile.SanitizePostTitle, the defense-in-depth
    /// scrub applied before a post title is used as the AutoRecover/draft
    /// file name. A rich-text (markup-laden) title previously crashed
    /// AutoSave with "The filename, directory name, or volume label syntax
    /// is incorrect".
    /// </summary>
    [TestFixture]
    public class PostEditorFileNameTest
    {
        [Test]
        public void SanitizePostTitle_PlainTitleUnchanged()
        {
            Assert.AreEqual("My Post Title", PostEditorFile.SanitizePostTitle("My Post Title"));
        }

        [Test]
        public void SanitizePostTitle_StripsHtmlMarkup()
        {
            Assert.AreEqual("QA-TEST post",
                PostEditorFile.SanitizePostTitle("<span style=\"font-weight: normal;\"><i><u>QA-TEST post</u></i></span>"));
        }

        [Test]
        public void SanitizePostTitle_DecodesEntities()
        {
            Assert.AreEqual("Fish & Chips", PostEditorFile.SanitizePostTitle("Fish &amp; Chips"));
        }

        [Test]
        public void SanitizePostTitle_ScrubsInvalidFileNameChars()
        {
            string result = PostEditorFile.SanitizePostTitle("a:b/c\\d?e*f\"g<h>i|j");
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                Assert.IsFalse(result.Contains(c.ToString()), "result contains invalid char: " + result);
        }

        [Test]
        public void SanitizePostTitle_MarkupOnlyTitleBecomesEmpty()
        {
            Assert.AreEqual(String.Empty, PostEditorFile.SanitizePostTitle("<b></b>"));
        }

        [Test]
        public void SanitizePostTitle_NullAndEmptyStayEmpty()
        {
            Assert.AreEqual(String.Empty, PostEditorFile.SanitizePostTitle(null));
            Assert.AreEqual(String.Empty, PostEditorFile.SanitizePostTitle(String.Empty));
        }

        [Test]
        public void SanitizePostTitle_CollapsesToSingleLine()
        {
            Assert.AreEqual("line one line two", PostEditorFile.SanitizePostTitle("line one\r\nline two"));
        }
    }
}
