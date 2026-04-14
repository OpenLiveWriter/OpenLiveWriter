// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsPlatformServicesTests
    {
        private WindowsPlatformServices _services;

        [SetUp]
        public void SetUp()
        {
            _services = new WindowsPlatformServices();
        }

        [Test]
        public void GetApplicationDataDirectory_ReturnsExistingDirectory()
        {
            string dir = _services.GetApplicationDataDirectory();
            Assert.That(Directory.Exists(dir), Is.True);
            Assert.That(dir, Does.Contain("OpenLiveWriter"));
        }

        [Test]
        public void GetLocalApplicationDataDirectory_ReturnsExistingDirectory()
        {
            string dir = _services.GetLocalApplicationDataDirectory();
            Assert.That(Directory.Exists(dir), Is.True);
            Assert.That(dir, Does.Contain("OpenLiveWriter"));
        }

        [Test]
        public void GetShortPathName_ReturnsNonEmpty_ForExistingPath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string shortPath = _services.GetShortPathName(path);
            Assert.That(shortPath, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GetShortPathName_ReturnsOriginal_ForNullOrEmpty()
        {
            Assert.That(_services.GetShortPathName(null), Is.Null);
            Assert.That(_services.GetShortPathName(""), Is.EqualTo(""));
        }

        [Test]
        public void CreateUserSettingsPersister_ReturnsNonNull()
        {
            using (var persister = _services.CreateUserSettingsPersister("UnitTest_" + Guid.NewGuid().ToString("N")))
            {
                Assert.That(persister, Is.Not.Null);
            }
        }
    }
}
