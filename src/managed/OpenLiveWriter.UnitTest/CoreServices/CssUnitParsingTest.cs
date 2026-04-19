// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestClass]
    public class CssUnitParsingTest
    {
        [TestMethod]
        public void TrimmedValue_ParsesPtCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("12pt", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(12f, number);
            Assert.AreEqual("pt", unit);
        }

        [TestMethod]
        public void WhitespacePadded_ParsesPtCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  12pt  ", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(12f, number);
            Assert.AreEqual("pt", unit);
        }

        [TestMethod]
        public void WhitespacePadded_ParsesEmCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  1.5em  ", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(1.5f, number);
            Assert.AreEqual("em", unit);
        }

        [TestMethod]
        public void WhitespacePadded_ParsesPxCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  16px  ", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(16f, number);
            Assert.AreEqual("px", unit);
        }

        [TestMethod]
        public void WhitespacePadded_ParsesRemCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  2rem  ", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(2f, number);
            Assert.AreEqual("rem", unit);
        }

        [TestMethod]
        public void ParsesPercentageCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("150%", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(150f, number);
            Assert.AreEqual("%", unit);
        }

        [TestMethod]
        public void ParsesCmCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("2.54cm", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(2.54f, number);
            Assert.AreEqual("cm", unit);
        }

        [TestMethod]
        public void ParsesMmCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("10mm", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(10f, number);
            Assert.AreEqual("mm", unit);
        }

        [TestMethod]
        public void ParsesInchCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("1in", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(1f, number);
            Assert.AreEqual("in", unit);
        }

        [TestMethod]
        public void ParsesPcCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("6pc", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(6f, number);
            Assert.AreEqual("pc", unit);
        }

        [TestMethod]
        public void ParsesBareNumber()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("1.2", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(1.2f, number);
            Assert.AreEqual(string.Empty, unit);
        }

        [TestMethod]
        public void NullString_ReturnsFalse()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength(null, out number, out unit);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EmptyString_ReturnsFalse()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("", out number, out unit);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhitespaceOnly_ReturnsFalse()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("   ", out number, out unit);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DecimalPt_ParsesCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("10.5pt", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(10.5f, number);
            Assert.AreEqual("pt", unit);
        }

        [TestMethod]
        public void LeadingWhitespace_ParsesCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("  14px", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(14f, number);
            Assert.AreEqual("px", unit);
        }

        [TestMethod]
        public void TrailingWhitespace_ParsesCorrectly()
        {
            float number;
            string unit;
            bool result = HTMLElementHelper.TryParseCssLength("14px   ", out number, out unit);
            Assert.IsTrue(result);
            Assert.AreEqual(14f, number);
            Assert.AreEqual("px", unit);
        }
    }
}
