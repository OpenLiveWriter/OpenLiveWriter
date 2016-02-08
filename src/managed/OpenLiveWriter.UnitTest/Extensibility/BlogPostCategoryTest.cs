// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.UnitTest.Extensibility
{
    [TestFixture]
    public class BlogPostCategoryTest
    {
        [Test]
        public void BlogPostCategoryEquality()
        {
            /*
             * Known WordPress.com bug:
             *
             * If a category "Foo" exists and has a parent, attempting
             * to create a new category "Foo" with no parent returns the
             * existing category.
             */

            // Simple cases
            AreEqual(new BlogPostCategory("Foo", "Foo"), new BlogPostCategory("Foo", "Foo"));
            AreEqual(new BlogPostCategory("", "Foo"), new BlogPostCategory("Foo", "Foo"));
            AreEqual(new BlogPostCategory(null, "Foo"), new BlogPostCategory("Foo", "Foo"));
            AreEqual(new BlogPostCategory(null, "Foo"), new BlogPostCategory("", "Foo"));
            AreEqual(new BlogPostCategory("10", "Foo"), new BlogPostCategory("10", "Foo"));
            AreEqual(new BlogPostCategory("10", "Foo", "Bar"), new BlogPostCategory("10", "Foo", "Bar"));
            AreEqual(new BlogPostCategory("10", "Foo", "Bar"), new BlogPostCategory("10", "Foo", "Baz"));

            // "Real" IDs take precedence
            AreNotEqual(new BlogPostCategory("10", "Foo", "Bar"), new BlogPostCategory("11", "Foo", "Bar"));
            AreEqual(new BlogPostCategory("10", "Foo", "Bar"), new BlogPostCategory("10", "Baz", "Bar"));

            //
            // Compare uncommitted to committed
            //

            // Cooked-up IDs aren't relevant to comparison
            AreEqual(new BlogPostCategory("Foo", "Foo"), new BlogPostCategory("20", "Foo"));
            AreEqual(new BlogPostCategory("", "Foo"), new BlogPostCategory("20", "Foo"));
            AreEqual(new BlogPostCategory(null, "Foo"), new BlogPostCategory("20", "Foo"));

            // Cooked-up IDs aren't relevant to comparison, parents are relevant to comparison
            AreEqual(new BlogPostCategory("Foo", "Foo", "Bar"), new BlogPostCategory("20", "Foo", "Bar"));
            AreEqual(new BlogPostCategory("", "Foo", "Bar"), new BlogPostCategory("20", "Foo", "Bar"));
            AreEqual(new BlogPostCategory(null, "Foo", "Bar"), new BlogPostCategory("20", "Foo", "Bar"));
            AreNotEqual(new BlogPostCategory("Foo", "Foo", "Bar"), new BlogPostCategory("20", "Foo", "Blargh"));
            AreNotEqual(new BlogPostCategory("", "Foo", "Bar"), new BlogPostCategory("20", "Foo", "Blargh"));
            AreNotEqual(new BlogPostCategory(null, "Foo", "Bar"), new BlogPostCategory("20", "Foo", "Blargh"));

            // Cooked-up IDs aren't relevant to comparison, names are (obviously) relevant to comparison
            AreNotEqual(new BlogPostCategory("Foo", "Foo", "Bar"), new BlogPostCategory("20", "Blargh", "Bar"));
            AreNotEqual(new BlogPostCategory("", "Foo", "Bar"), new BlogPostCategory("20", "Blargh", "Bar"));
            AreNotEqual(new BlogPostCategory(null, "Foo", "Bar"), new BlogPostCategory("20", "Blargh", "Bar"));

            // Compare uncommitted to uncommitted
            AreNotEqual(new BlogPostCategory("Foo", "Foo"), new BlogPostCategory("Foo", "Foo", "Bar"));
            AreNotEqual(new BlogPostCategory("Foo", "Foo"), new BlogPostCategory("", "Foo", "Bar"));
            AreNotEqual(new BlogPostCategory("Foo", "Foo"), new BlogPostCategory(null, "Foo", "Bar"));
            AreNotEqual(new BlogPostCategory("", "Foo"), new BlogPostCategory("Foo", "Foo", "Bar"));
            AreNotEqual(new BlogPostCategory(null, "Foo"), new BlogPostCategory("Foo", "Foo", "Bar"));

            // WordPress uses "0" for no parent
            AreEqual(new BlogPostCategory("foo", "bar", "0"), new BlogPostCategory("foo", "bar", "0"));
            AreEqual(new BlogPostCategory("foo", "bar", "0"), new BlogPostCategory("foo", "bar"));
            AreEqual(new BlogPostCategory("foo", "bar", "0"), new BlogPostCategory("foo", "bar", ""));
            AreEqual(new BlogPostCategory("foo", "bar", "0"), new BlogPostCategory("foo", "bar", null));
            AreEqual(new BlogPostCategory("foo", "bar", "baz"), new BlogPostCategory("foo", "bar", "0"));
            AreEqual(new BlogPostCategory("foo", "bar", "baz"), new BlogPostCategory("foo", "bar"));
            AreEqual(new BlogPostCategory("foo", "bar", "baz"), new BlogPostCategory("foo", "bar", ""));
            AreEqual(new BlogPostCategory("foo", "bar", "baz"), new BlogPostCategory("foo", "bar", null));

            Assert.IsTrue(BlogPostCategory.Equals(new BlogPostCategory("foo&bar"), new BlogPostCategory("foo&bar"), true));
            Assert.IsTrue(BlogPostCategory.Equals(new BlogPostCategory("foo&bar"), new BlogPostCategory("foo&amp;bar"), true));
            Assert.IsTrue(BlogPostCategory.Equals(new BlogPostCategory("foo&amp;bar"), new BlogPostCategory("foo&bar"), true));
            Assert.IsTrue(BlogPostCategory.Equals(new BlogPostCategory("foo&amp;bar"), new BlogPostCategory("foo&amp;bar"), true));
        }

        private static void AreEqual(BlogPostCategory a, BlogPostCategory b)
        {
            Assert.AreEqual(a, b);
            Assert.AreEqual(b, a);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.IsTrue(BlogPostCategory.Equals(a, b, true));
            Assert.IsTrue(BlogPostCategory.Equals(a, b, false));
            Assert.IsTrue(BlogPostCategory.Equals(b, a, true));
            Assert.IsTrue(BlogPostCategory.Equals(b, a, false));
        }

        private static void AreNotEqual(BlogPostCategory a, BlogPostCategory b)
        {
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, a);
            Assert.IsFalse(BlogPostCategory.Equals(a, b, true));
            Assert.IsFalse(BlogPostCategory.Equals(a, b, false));
            Assert.IsFalse(BlogPostCategory.Equals(b, a, true));
            Assert.IsFalse(BlogPostCategory.Equals(b, a, false));
        }
    }
}
