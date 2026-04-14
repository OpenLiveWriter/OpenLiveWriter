// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;

namespace OpenLiveWriter.Platform.Windows.Tests
{
    [TestFixture]
    public class WindowsCredentialStorageTests
    {
        private WindowsCredentialStorage _storage;
        private const string TEST_KEY = "TestBlog_UnitTest_" + nameof(WindowsCredentialStorageTests);

        [SetUp]
        public void SetUp()
        {
            _storage = new WindowsCredentialStorage();
            _storage.DeleteCredential(TEST_KEY);
        }

        [TearDown]
        public void TearDown()
        {
            _storage.DeleteCredential(TEST_KEY);
        }

        [Test]
        public void StoreAndRetrieve_RoundTripsCredentials()
        {
            _storage.StoreCredential(TEST_KEY, "user@example.com", "s3cret!P@ss");
            var result = _storage.RetrieveCredential(TEST_KEY);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.username, Is.EqualTo("user@example.com"));
            Assert.That(result.Value.password, Is.EqualTo("s3cret!P@ss"));
        }

        [Test]
        public void RetrieveCredential_ReturnsNull_WhenNotStored()
        {
            var result = _storage.RetrieveCredential("nonexistent_key_" + Guid.NewGuid());
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CredentialExists_ReturnsTrue_AfterStore()
        {
            _storage.StoreCredential(TEST_KEY, "user", "pass");
            Assert.That(_storage.CredentialExists(TEST_KEY), Is.True);
        }

        [Test]
        public void CredentialExists_ReturnsFalse_WhenNotStored()
        {
            Assert.That(_storage.CredentialExists("nonexistent_key_" + Guid.NewGuid()), Is.False);
        }

        [Test]
        public void DeleteCredential_RemovesCredential()
        {
            _storage.StoreCredential(TEST_KEY, "user", "pass");
            _storage.DeleteCredential(TEST_KEY);
            Assert.That(_storage.CredentialExists(TEST_KEY), Is.False);
        }

        [Test]
        public void DeleteCredential_DoesNotThrow_WhenNotExists()
        {
            Assert.DoesNotThrow(() => _storage.DeleteCredential("nonexistent_key_" + Guid.NewGuid()));
        }

        [Test]
        public void StoreCredential_OverwritesExisting()
        {
            _storage.StoreCredential(TEST_KEY, "user1", "pass1");
            _storage.StoreCredential(TEST_KEY, "user2", "pass2");
            var result = _storage.RetrieveCredential(TEST_KEY);
            Assert.That(result.Value.username, Is.EqualTo("user2"));
            Assert.That(result.Value.password, Is.EqualTo("pass2"));
        }

        [Test]
        public void StoreAndRetrieve_HandlesUnicodePassword()
        {
            _storage.StoreCredential(TEST_KEY, "user", "пароль_密码_🔑");
            var result = _storage.RetrieveCredential(TEST_KEY);
            Assert.That(result.Value.password, Is.EqualTo("пароль_密码_🔑"));
        }
    }
}
