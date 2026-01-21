// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.Ribbon.Managed.Rendering;

namespace OpenLiveWriter.Tests.Ribbon
{
    [TestFixture]
    public class RibbonRendererTests
    {
        [Test]
        public void StripAccelerator_RemovesSingleAmpersand()
        {
            Assert.That(RibbonRenderer.StripAccelerator("&Paste"), Is.EqualTo("Paste"));
        }

        [Test]
        public void StripAccelerator_RemovesMiddleAmpersand()
        {
            Assert.That(RibbonRenderer.StripAccelerator("Publis&h"), Is.EqualTo("Publish"));
        }

        [Test]
        public void StripAccelerator_PreservesDoubleAmpersand()
        {
            Assert.That(RibbonRenderer.StripAccelerator("Tom && Jerry"), Is.EqualTo("Tom & Jerry"));
        }

        [Test]
        public void StripAccelerator_HandlesMultipleAmpersands()
        {
            Assert.That(RibbonRenderer.StripAccelerator("&File && &Edit"), Is.EqualTo("File & Edit"));
        }

        [Test]
        public void StripAccelerator_HandlesNullString()
        {
            Assert.That(RibbonRenderer.StripAccelerator(null), Is.Null);
        }

        [Test]
        public void StripAccelerator_HandlesEmptyString()
        {
            Assert.That(RibbonRenderer.StripAccelerator(""), Is.EqualTo(""));
        }

        [Test]
        public void StripAccelerator_NoAmpersand()
        {
            Assert.That(RibbonRenderer.StripAccelerator("Normal Text"), Is.EqualTo("Normal Text"));
        }

        [Test]
        public void StripAccelerator_OnlyAmpersand()
        {
            Assert.That(RibbonRenderer.StripAccelerator("&"), Is.EqualTo(""));
        }

        [Test]
        public void StripAccelerator_TrailingAmpersand()
        {
            Assert.That(RibbonRenderer.StripAccelerator("Save&"), Is.EqualTo("Save"));
        }
    }
}
