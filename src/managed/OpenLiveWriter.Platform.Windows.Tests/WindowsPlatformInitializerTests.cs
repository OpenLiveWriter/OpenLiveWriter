// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsPlatformInitializerTests
    {
        [TearDown]
        public void TearDown()
        {
            PlatformContext.Reset();
        }

        [Test]
        public void Initialize_SetsAllPlatformServices()
        {
            WindowsPlatformInitializer.Initialize();

            Assert.That(PlatformContext.IsInitialized, Is.True);
            Assert.That(PlatformContext.Services, Is.InstanceOf<WindowsPlatformServices>());
            Assert.That(PlatformContext.Display, Is.InstanceOf<WindowsDisplayHelper>());
            Assert.That(PlatformContext.Credentials, Is.InstanceOf<WindowsCredentialStorage>());
            Assert.That(PlatformContext.Bidi, Is.InstanceOf<WindowsBidiSupport>());
            Assert.That(PlatformContext.SpellCheck, Is.InstanceOf<WindowsSpellCheckProvider>());
        }
    }
}
