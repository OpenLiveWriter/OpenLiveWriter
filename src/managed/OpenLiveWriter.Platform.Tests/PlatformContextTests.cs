// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;

namespace OpenLiveWriter.Platform.Tests
{
    [TestFixture]
    public class PlatformContextTests
    {
        [TearDown]
        public void TearDown()
        {
            PlatformContext.Reset();
        }

        [Test]
        public void IsInitialized_ReturnsFalse_BeforeInitialize()
        {
            Assert.That(PlatformContext.IsInitialized, Is.False);
        }

        [Test]
        public void EnsureInitialized_Throws_BeforeInitialize()
        {
            Assert.Throws<InvalidOperationException>(() => PlatformContext.EnsureInitialized());
        }

        [Test]
        public void Initialize_SetsAllProperties()
        {
            var services = new StubPlatformServices();
            var display = new StubDisplayHelper();
            var credentials = new StubCredentialStorage();
            var bidi = new StubBidiSupport();
            var spellCheck = new StubSpellCheckProvider();

            PlatformContext.Initialize(services, display, credentials, bidi, spellCheck);

            Assert.That(PlatformContext.IsInitialized, Is.True);
            Assert.That(PlatformContext.Services, Is.SameAs(services));
            Assert.That(PlatformContext.Display, Is.SameAs(display));
            Assert.That(PlatformContext.Credentials, Is.SameAs(credentials));
            Assert.That(PlatformContext.Bidi, Is.SameAs(bidi));
            Assert.That(PlatformContext.SpellCheck, Is.SameAs(spellCheck));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForServices()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(null, new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForDisplay()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), null, new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForCredentials()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), null, new StubBidiSupport(), new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForBidi()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), null, new StubSpellCheckProvider()));
        }

        [Test]
        public void Initialize_ThrowsArgumentNull_ForSpellCheck()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), null));
        }

        [Test]
        public void EnsureInitialized_DoesNotThrow_AfterInitialize()
        {
            PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider());
            Assert.DoesNotThrow(() => PlatformContext.EnsureInitialized());
        }

        [Test]
        public void Reset_ClearsAllProperties()
        {
            PlatformContext.Initialize(new StubPlatformServices(), new StubDisplayHelper(), new StubCredentialStorage(), new StubBidiSupport(), new StubSpellCheckProvider());
            PlatformContext.Reset();

            Assert.That(PlatformContext.IsInitialized, Is.False);
            Assert.That(PlatformContext.Services, Is.Null);
            Assert.That(PlatformContext.Display, Is.Null);
            Assert.That(PlatformContext.Credentials, Is.Null);
            Assert.That(PlatformContext.Bidi, Is.Null);
            Assert.That(PlatformContext.SpellCheck, Is.Null);
        }
    }
}
