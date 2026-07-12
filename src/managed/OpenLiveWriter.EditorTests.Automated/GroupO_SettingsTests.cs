// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Platform.Mac;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group O — Options / Preferences. Settings round-trip via an in-memory persister
    /// and the real <see cref="FileSettingsPersister"/> (temp directory), plus
    /// spell-check preference mapping and proxy-field serialization.
    /// </summary>
    [TestFixture]
    [Category("GroupO")]
    public class GroupO_SettingsTests
    {
        [Test]
        public void Preferences_RoundTrip_InMemoryPersister()
        {
            var root = new MemorySettingsPersister();
            var store = AppPreferencesStore.ForPersisterFactory(() => root);
            var original = new AppPreferences
            {
                ShowRealTimeWordCount = true,
                ReplaceSmartQuotes = false,
                ReplaceHyphens = false,
                SpellcheckEnabled = false,
                ProxyEnabled = true,
                ProxyHostname = "proxy.local",
                ProxyPort = 3128,
                ProxyUsername = "user",
                UseParagraphTags = false
            };

            store.Save(original);
            AppPreferences loaded = store.Load();

            Assert.Multiple(() =>
            {
                Assert.That(loaded.ShowRealTimeWordCount, Is.True);
                Assert.That(loaded.ReplaceSmartQuotes, Is.False);
                Assert.That(loaded.SpellcheckEnabled, Is.False);
                Assert.That(loaded.ProxyEnabled, Is.True);
                Assert.That(loaded.ProxyHostname, Is.EqualTo("proxy.local"));
                Assert.That(loaded.ProxyPort, Is.EqualTo(3128));
                Assert.That(loaded.ProxyUsername, Is.EqualTo("user"));
                Assert.That(loaded.UseParagraphTags, Is.False);
            });
        }

        [Test]
        public void Preferences_SpellcheckToggle_MapsToBridgeScript()
        {
            Assert.That(SpellCheckController.BuildSetSpellcheckScript(false),
                Is.EqualTo("OLWBridge.setSpellcheck(false)"));
            Assert.That(SpellCheckController.SpellcheckAttributeValue(false), Is.EqualTo("false"));
        }

        [Test]
        public void Preferences_ProxyPassword_UnsetWhenEmpty()
        {
            var root = new MemorySettingsPersister();
            var store = AppPreferencesStore.ForPersisterFactory(() => root);
            var prefs = AppPreferences.CreateDefault();
            prefs.ProxyPassword = "secret";
            store.Save(prefs);

            prefs.ProxyPassword = null;
            store.Save(prefs);

            AppPreferences loaded = store.Load();
            Assert.That(loaded.ProxyPassword, Is.Null.Or.Empty);
        }

        [Test]
        public void FileSettingsPersister_RoundTrip_OnDisk()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "olw-test-settings-" + Guid.NewGuid().ToString("N"));
            try
            {
                using (var persister = FileSettingsPersister.Create(tempDir, "UnitTest"))
                {
                    persister.Set("Alpha", "one");
                    persister.Set("Beta", 42);
                    persister.Set("Flag", true);
                }

                using (var reload = FileSettingsPersister.Create(tempDir, "UnitTest"))
                {
                    Assert.That(reload.Get("Alpha", typeof(string), null), Is.EqualTo("one"));
                    Assert.That(reload.Get("Beta", typeof(int), 0), Is.EqualTo(42));
                    Assert.That(reload.Get("Flag", typeof(bool), false), Is.True);
                }

                string settingsFile = Path.Combine(tempDir, "UnitTest.json");
                Assert.That(File.Exists(settingsFile), Is.True);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }

        [Test]
        public void FileSettingsPersister_SubSettings_NestedObject()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "olw-test-sub-" + Guid.NewGuid().ToString("N"));
            try
            {
                using (var root = FileSettingsPersister.Create(tempDir, "Nested"))
                using (var child = root.GetSubSettings("Editing"))
                {
                    child.Set("ReplaceHyphens", true);
                }

                using (var reload = FileSettingsPersister.Create(tempDir, "Nested"))
                using (var childReload = reload.GetSubSettings("Editing"))
                {
                    Assert.That(childReload.Get("ReplaceHyphens", typeof(bool), false), Is.True);
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }

        [Test]
        public void AppPreferencesStore_RoundTrip_FilePersister()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "olw-prefs-" + Guid.NewGuid().ToString("N"));
            try
            {
                var store = AppPreferencesStore.ForPersisterFactory(
                    () => FileSettingsPersister.Create(tempDir, "Preferences"));
                var prefs = AppPreferences.CreateDefault();
                prefs.SpellcheckEnabled = false;
                prefs.ProxyEnabled = true;
                prefs.ProxyHostname = "10.0.0.1";
                prefs.ProxyPort = 8888;
                store.Save(prefs);

                AppPreferences loaded = store.Load();
                Assert.Multiple(() =>
                {
                    Assert.That(loaded.SpellcheckEnabled, Is.False);
                    Assert.That(loaded.ProxyEnabled, Is.True);
                    Assert.That(loaded.ProxyHostname, Is.EqualTo("10.0.0.1"));
                    Assert.That(loaded.ProxyPort, Is.EqualTo(8888));
                });
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
