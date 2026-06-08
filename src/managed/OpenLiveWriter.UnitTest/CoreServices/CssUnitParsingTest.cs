// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class CssUnitParsingTest
    {
        [Test]
        public void TrimmedValue_ParsesPtCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("12pt", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(12f, number);
            ClassicAssert.AreEqual("pt", unit);
        }

        [Test]
        public void WhitespacePadded_ParsesPtCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  12pt  ", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(12f, number);
            ClassicAssert.AreEqual("pt", unit);
        }

        [Test]
        public void WhitespacePadded_ParsesEmCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  1.5em  ", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(1.5f, number);
            ClassicAssert.AreEqual("em", unit);
        }

        [Test]
        public void WhitespacePadded_ParsesPxCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  16px  ", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(16f, number);
            ClassicAssert.AreEqual("px", unit);
        }

        [Test]
        public void WhitespacePadded_ParsesRemCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  2rem  ", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(2f, number);
            ClassicAssert.AreEqual("rem", unit);
        }

        [Test]
        public void ParsesPercentageCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("150%", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(150f, number);
            ClassicAssert.AreEqual("%", unit);
        }

        [Test]
        public void ParsesCmCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("2.54cm", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(2.54f, number);
            ClassicAssert.AreEqual("cm", unit);
        }

        [Test]
        public void ParsesMmCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("10mm", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(10f, number);
            ClassicAssert.AreEqual("mm", unit);
        }

        [Test]
        public void ParsesInchCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("1in", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(1f, number);
            ClassicAssert.AreEqual("in", unit);
        }

        [Test]
        public void ParsesPcCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("6pc", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(6f, number);
            ClassicAssert.AreEqual("pc", unit);
        }

        [Test]
        public void ParsesBareNumber()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("1.2", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(1.2f, number);
            ClassicAssert.AreEqual(string.Empty, unit);
        }

        [Test]
        public void NullString_ReturnsFalse()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength(null, out number, out unit);
            ClassicAssert.IsFalse(result);
        }

        [Test]
        public void EmptyString_ReturnsFalse()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("", out number, out unit);
            ClassicAssert.IsFalse(result);
        }

        [Test]
        public void WhitespaceOnly_ReturnsFalse()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("   ", out number, out unit);
            ClassicAssert.IsFalse(result);
        }

        [Test]
        public void DecimalPt_ParsesCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("10.5pt", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(10.5f, number);
            ClassicAssert.AreEqual("pt", unit);
        }

        [Test]
        public void LeadingWhitespace_ParsesCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  14px", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(14f, number);
            ClassicAssert.AreEqual("px", unit);
        }

        [Test]
        public void TrailingWhitespace_ParsesCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("14px   ", out number, out unit);
            ClassicAssert.IsTrue(result);
            ClassicAssert.AreEqual(14f, number);
            ClassicAssert.AreEqual("px", unit);
        }
    }
}


