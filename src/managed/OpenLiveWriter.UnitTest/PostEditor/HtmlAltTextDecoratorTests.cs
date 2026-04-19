// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestClass]
    public class HtmlAltTextDecoratorTests
    {
        [TestMethod]
        public void ShouldPreserveAltText_WithMeaningfulAlt_ReturnsTrue()
        {
            Assert.IsTrue(HtmlAltTextDecorator.ShouldPreserveAltText("A photo of a sunset"));
        }

        [TestMethod]
        public void ShouldPreserveAltText_WithDefaultImage_ReturnsFalse()
        {
            Assert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText("image"));
        }

        [TestMethod]
        public void ShouldPreserveAltText_WithDefaultImageUpperCase_ReturnsFalse()
        {
            Assert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText("Image"));
        }

        [TestMethod]
        public void ShouldPreserveAltText_WithEmptyString_ReturnsFalse()
        {
            Assert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText(""));
        }

        [TestMethod]
        public void ShouldPreserveAltText_WithNull_ReturnsFalse()
        {
            Assert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText(null));
        }

        [TestMethod]
        public void ShouldPreserveAltText_WithWhitespaceOnly_ReturnsTrue()
        {
            // Whitespace-only alt text is non-empty and non-default, so it is
            // treated as intentionally set (e.g., decorative image convention).
            Assert.IsTrue(HtmlAltTextDecorator.ShouldPreserveAltText("   "));
        }

        [TestMethod]
        public void ShouldPreserveAltText_WithImageSubstring_ReturnsTrue()
        {
            // "image" as part of a longer string is meaningful alt text
            Assert.IsTrue(HtmlAltTextDecorator.ShouldPreserveAltText("An image of a cat"));
        }
    }
}
