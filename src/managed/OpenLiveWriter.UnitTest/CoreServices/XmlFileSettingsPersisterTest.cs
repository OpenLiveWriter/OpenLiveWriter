// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class XmlFileSettingsPersisterTest
    {
        private string _tempDir;

        [SetUp]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "OLW_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            try { Directory.Delete(_tempDir, true); }
            catch (IOException) { }
        }

        private string TempFile(string name = "settings.xml")
        {
            return Path.Combine(_tempDir, name);
        }

        [Test]
        public void Open_NewFile_ReturnsEmptySettings()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                ClassicAssert.AreEqual(0, persister.GetNames().Length);
                ClassicAssert.AreEqual(0, persister.GetSubSettings().Length);
            }
        }

        [Test]
        public void SetAndGet_RoundTrips()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                persister.Set("key1", "value1");
                persister.Set("key2", 42);

                ClassicAssert.AreEqual("value1", persister.Get("key1"));
                ClassicAssert.AreEqual(42, persister.Get("key2"));
            }
        }

        [Test]
        public void Persist_SurvivesReopen()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                persister.Set("name", "test");
                persister.Set("count", 7);
            }

            // Re-open and verify data was persisted
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                ClassicAssert.AreEqual("test", persister.Get("name"));
                ClassicAssert.AreEqual(7, persister.Get("count"));
            }
        }

        [Test]
        public void FileNotLockedAfterOpen()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                persister.Set("key", "value");

                // The file should not be locked — another writer should be able to open it
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    ClassicAssert.IsTrue(fs.Length > 0, "File should have content after Set");
                }
            }
        }

        [Test]
        public void MultipleInstances_CanCoexist()
        {
            string path = TempFile();

            // Open two persisters on the same file simultaneously
            using (var persister1 = XmlFileSettingsPersister.Open(path))
            {
                persister1.Set("from", "instance1");

                // Second instance should be able to open the same file
                using (var persister2 = XmlFileSettingsPersister.Open(path))
                {
                    // Instance 2 should see data written by instance 1
                    ClassicAssert.AreEqual("instance1", persister2.Get("from"));

                    // Instance 2 should be able to write
                    persister2.Set("from", "instance2");
                }

                // Instance 1 can still write without crashing
                persister1.Set("extra", "data");
            }
        }

        [Test]
        public void SubSettings_RoundTrip()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                using (var sub = persister.GetSubSettings("child"))
                {
                    sub.Set("nested", "value");
                }
            }

            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                ClassicAssert.IsTrue(persister.HasSubSettings("child"));
                using (var sub = persister.GetSubSettings("child"))
                {
                    ClassicAssert.AreEqual("value", sub.Get("nested"));
                }
            }
        }

        [Test]
        public void BatchUpdate_DefersWriteUntilEnd()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                using (persister.BatchUpdate())
                {
                    persister.Set("a", "1");
                    persister.Set("b", "2");
                    persister.Set("c", "3");
                    // File may or may not have been written yet (batch defers)
                }

                // After batch completes, all values should be persisted
                ClassicAssert.AreEqual("1", persister.Get("a"));
                ClassicAssert.AreEqual("2", persister.Get("b"));
                ClassicAssert.AreEqual("3", persister.Get("c"));
            }

            // Verify they survived to disk
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                ClassicAssert.AreEqual("1", persister.Get("a"));
                ClassicAssert.AreEqual("2", persister.Get("b"));
                ClassicAssert.AreEqual("3", persister.Get("c"));
            }
        }

        [Test]
        public void Unset_RemovesValue()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                persister.Set("key", "value");
                ClassicAssert.AreEqual("value", persister.Get("key"));

                persister.Unset("key");
                ClassicAssert.IsNull(persister.Get("key"));
            }
        }
    }
}

