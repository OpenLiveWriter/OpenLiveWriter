// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators;

namespace OpenLiveWriter.UnitTest.PostEditor
{
    [TestFixture]
    public class HtmlAltTextDecoratorTests
    {
        [Test]
        public void ShouldPreserveAltText_WithMeaningfulAlt_ReturnsTrue()
        {
            ClassicAssert.IsTrue(HtmlAltTextDecorator.ShouldPreserveAltText("A photo of a sunset"));
        }

        [Test]
        public void ShouldPreserveAltText_WithDefaultImage_ReturnsFalse()
        {
            ClassicAssert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText("image"));
        }

        [Test]
        public void ShouldPreserveAltText_WithDefaultImageUpperCase_ReturnsFalse()
        {
            ClassicAssert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText("Image"));
        }

        [Test]
        public void ShouldPreserveAltText_WithEmptyString_ReturnsFalse()
        {
            ClassicAssert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText(""));
        }

        [Test]
        public void ShouldPreserveAltText_WithNull_ReturnsFalse()
        {
            ClassicAssert.IsFalse(HtmlAltTextDecorator.ShouldPreserveAltText(null));
        }

        [Test]
        public void ShouldPreserveAltText_WithWhitespaceOnly_ReturnsTrue()
        {
            // Whitespace-only alt text is non-empty and non-default, so it is
            // treated as intentionally set (e.g., decorative image convention).
            ClassicAssert.IsTrue(HtmlAltTextDecorator.ShouldPreserveAltText("   "));
        }

        [Test]
        public void ShouldPreserveAltText_WithImageSubstring_ReturnsTrue()
        {
            // "image" as part of a longer string is meaningful alt text
            ClassicAssert.IsTrue(HtmlAltTextDecorator.ShouldPreserveAltText("An image of a cat"));
        }
    }
}


