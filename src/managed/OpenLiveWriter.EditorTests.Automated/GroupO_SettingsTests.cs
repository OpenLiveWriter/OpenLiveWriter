// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Platform.Mac;
using OpenLiveWriter.Publishing;

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

        [Test]
        public void PublishingHttpClientFactory_ProxyEnabled_ConfiguresHandler()
        {
            var config = new WebProxyConfiguration
            {
                Enabled = true,
                Hostname = "proxy.example",
                Port = 3128,
                Username = "user",
                Password = "pass"
            };

            using var handler = PublishingHttpClientFactory.CreateHandler(config);
            Assert.That(handler.UseProxy, Is.True);
            var proxy = handler.Proxy as WebProxy;
            Assert.That(proxy, Is.Not.Null);
            Assert.That(proxy.Address.Host, Is.EqualTo("proxy.example"));
            Assert.That(proxy.Address.Port, Is.EqualTo(3128));
        }

        [Test]
        public void WindowLayout_RoundTrip_Persister()
        {
            var root = new MemorySettingsPersister();
            var store = AppPreferencesStore.ForPersisterFactory(() => root);
            var original = new WindowLayout
            {
                Width = 1100,
                Height = 720,
                X = 120,
                Y = 80,
                Maximized = true
            };

            store.SaveWindowLayout(original);
            WindowLayout loaded = store.LoadWindowLayout();

            Assert.Multiple(() =>
            {
                Assert.That(loaded.Width, Is.EqualTo(1100));
                Assert.That(loaded.Height, Is.EqualTo(720));
                Assert.That(loaded.X, Is.EqualTo(120));
                Assert.That(loaded.Y, Is.EqualTo(80));
                Assert.That(loaded.Maximized, Is.True);
                Assert.That(loaded.HasSavedPosition, Is.True);
            });
        }

        [Test]
        public void WindowLayout_ClampsCorruptMinimums()
        {
            var root = new MemorySettingsPersister();
            var store = AppPreferencesStore.ForPersisterFactory(() => root);
            store.SaveWindowLayout(new WindowLayout { Width = 100, Height = 50 });
            WindowLayout loaded = store.LoadWindowLayout();
            Assert.That(loaded.Width, Is.GreaterThanOrEqualTo(WindowLayout.MinWidth));
            Assert.That(loaded.Height, Is.GreaterThanOrEqualTo(WindowLayout.MinHeight));
        }

        [Test]
        public void PublishingHttpClientFactory_ProxyDisabled_DoesNotSetCustomProxy()
        {
            using var handler = PublishingHttpClientFactory.CreateHandler(new WebProxyConfiguration());
            Assert.That(handler.Proxy as WebProxy, Is.Null);
        }

        [Test]
        public void WebProxyMapper_MapsPreferencesFields()
        {
            var prefs = new AppPreferences
            {
                ProxyEnabled = true,
                ProxyHostname = "10.0.0.5",
                ProxyPort = 8888,
                ProxyUsername = "alice"
            };
            var config = WebProxyMapper.ToConfiguration(prefs);
            Assert.Multiple(() =>
            {
                Assert.That(config.IsActive, Is.True);
                Assert.That(config.Hostname, Is.EqualTo("10.0.0.5"));
                Assert.That(config.Port, Is.EqualTo(8888));
                Assert.That(config.Username, Is.EqualTo("alice"));
            });
        }

        [Test]
        public void AutoreplaceTransformer_SmartQuotes_ReplacesStraightQuotes()
        {
            var options = new AutoreplaceOptions { ReplaceSmartQuotes = true };
            string result = AutoreplaceTransformer.TransformPlainText("\"Hello\" she said.", options);
            Assert.That(result, Does.Contain("\u201C"));
            Assert.That(result, Does.Contain("\u201D"));
        }

        [Test]
        public void AutoreplaceTransformer_Disabled_LeavesTextUntouched()
        {
            var options = new AutoreplaceOptions { ReplaceSmartQuotes = false };
            const string input = "\"plain\"";
            Assert.That(AutoreplaceTransformer.TransformPlainText(input, options), Is.EqualTo(input));
        }

        [Test]
        public void AutoreplaceController_BuildsBridgeScript()
        {
            var options = new AutoreplaceOptions
            {
                ReplaceSmartQuotes = true,
                ReplaceHyphens = false
            };
            string script = AutoreplaceController.BuildSetAutoreplaceScript(options);
            Assert.That(script, Does.Contain("smartQuotes:true"));
            Assert.That(script, Does.Contain("hyphens:false"));
        }
    }
}
