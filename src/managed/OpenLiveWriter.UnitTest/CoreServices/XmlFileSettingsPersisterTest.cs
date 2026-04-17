// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestClass]
    public class XmlFileSettingsPersisterTest
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "OLW_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void TearDown()
        {
            try { Directory.Delete(_tempDir, true); }
            catch (IOException) { }
        }

        private string TempFile(string name = "settings.xml")
        {
            return Path.Combine(_tempDir, name);
        }

        [TestMethod]
        public void Open_NewFile_ReturnsEmptySettings()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                Assert.AreEqual(0, persister.GetNames().Length);
                Assert.AreEqual(0, persister.GetSubSettings().Length);
            }
        }

        [TestMethod]
        public void SetAndGet_RoundTrips()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                persister.Set("key1", "value1");
                persister.Set("key2", 42);

                Assert.AreEqual("value1", persister.Get("key1"));
                Assert.AreEqual(42, persister.Get("key2"));
            }
        }

        [TestMethod]
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
                Assert.AreEqual("test", persister.Get("name"));
                Assert.AreEqual(7, persister.Get("count"));
            }
        }

        [TestMethod]
        public void FileNotLockedAfterOpen()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                persister.Set("key", "value");

                // The file should not be locked — another writer should be able to open it
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    Assert.IsTrue(fs.Length > 0, "File should have content after Set");
                }
            }
        }

        [TestMethod]
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
                    Assert.AreEqual("instance1", persister2.Get("from"));

                    // Instance 2 should be able to write
                    persister2.Set("from", "instance2");
                }

                // Instance 1 can still write without crashing
                persister1.Set("extra", "data");
            }
        }

        [TestMethod]
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
                Assert.IsTrue(persister.HasSubSettings("child"));
                using (var sub = persister.GetSubSettings("child"))
                {
                    Assert.AreEqual("value", sub.Get("nested"));
                }
            }
        }

        [TestMethod]
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
                Assert.AreEqual("1", persister.Get("a"));
                Assert.AreEqual("2", persister.Get("b"));
                Assert.AreEqual("3", persister.Get("c"));
            }

            // Verify they survived to disk
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                Assert.AreEqual("1", persister.Get("a"));
                Assert.AreEqual("2", persister.Get("b"));
                Assert.AreEqual("3", persister.Get("c"));
            }
        }

        [TestMethod]
        public void Unset_RemovesValue()
        {
            string path = TempFile();
            using (var persister = XmlFileSettingsPersister.Open(path))
            {
                persister.Set("key", "value");
                Assert.AreEqual("value", persister.Get("key"));

                persister.Unset("key");
                Assert.IsNull(persister.Get("key"));
            }
        }
    }
}
