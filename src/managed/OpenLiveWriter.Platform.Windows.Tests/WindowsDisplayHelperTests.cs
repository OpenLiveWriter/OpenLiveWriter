// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsDisplayHelperTests
    {
        private WindowsDisplayHelper _helper;

        [SetUp]
        public void SetUp()
        {
            _helper = new WindowsDisplayHelper();
        }

        [Test]
        public void DefaultDpi_Is96()
        {
            Assert.That(_helper.DefaultDpi, Is.EqualTo(96));
        }

        [Test]
        public void TwipsToPixelsX_ConvertsCorrectly()
        {
            float result = _helper.TwipsToPixelsX(1440);
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void TwipsToPixelsX_ZeroTwips_ReturnsZero()
        {
            Assert.That(_helper.TwipsToPixelsX(0), Is.EqualTo(0));
        }

        [Test]
        public void IsCompositionEnabled_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _helper.IsCompositionEnabled());
        }
    }
}
