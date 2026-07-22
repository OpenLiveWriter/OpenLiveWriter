// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Platform;
using OpenLiveWriter.Platform.Mac;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group N — spell-check UI. Checking/underlining is native (macOS/WebKit), so the
    /// headless coverage is the pure toggle-command mapping (attribute value + bridge
    /// script), the status-message logic (driven by an injected provider — never the
    /// real OS service), and the <see cref="MacSpellCheckProvider"/> contract defaults.
    /// </summary>
    [TestFixture]
    [Category("GroupN")]
    public class GroupN_SpellCheckTests
    {
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void SpellcheckAttributeValue_MapsBool(bool enabled, string expected)
        {
            Assert.That(SpellCheckController.SpellcheckAttributeValue(enabled), Is.EqualTo(expected));
        }

        [Test]
        public void BuildSetSpellcheckScript_TogglesBodyAttribute()
        {
            Assert.That(SpellCheckController.BuildSetSpellcheckScript(true),
                Is.EqualTo("OLWBridge.setSpellcheck(true)"));
            Assert.That(SpellCheckController.BuildSetSpellcheckScript(false),
                Is.EqualTo("OLWBridge.setSpellcheck(false)"));
        }

        [Test]
        public void DescribeStatus_Disabled_MentionsPreferences()
        {
            string status = SpellCheckController.DescribeStatus(new FakeProvider(true), "en", enabled: false);
            Assert.That(status, Does.Contain("turned off"));
        }

        [Test]
        public void DescribeStatus_Enabled_WithSystemDictionary()
        {
            string status = SpellCheckController.DescribeStatus(new FakeProvider(available: true), "en", enabled: true);
            Assert.That(status, Does.Contain("system dictionary"));
        }

        [Test]
        public void DescribeStatus_Enabled_WithoutProvider_FallsBackToEditorUnderlines()
        {
            Assert.That(SpellCheckController.DescribeStatus(null, "en", enabled: true),
                Does.Contain("editor underlines"));
            Assert.That(SpellCheckController.DescribeStatus(new FakeProvider(available: false), "en", enabled: true),
                Does.Contain("editor underlines"));
        }

        // ---- SpellCheckService surface (over an injected provider) ----

        [Test]
        public void Service_ReportsAvailabilityAndSuggestionsFromProvider()
        {
            var provider = new FakeProvider(available: true)
            {
                Suggestions = new[] { "colour", "color" }
            };
            var service = new SpellCheckService(provider);

            Assert.Multiple(() =>
            {
                Assert.That(service.IsAvailable("en"), Is.True);
                Assert.That(service.GetSuggestions("colur"), Is.EqualTo(new[] { "colour", "color" }));
                Assert.That(service.StatusMessage(enabled: true), Does.Contain("system dictionary"));
            });
        }

        [Test]
        public void Service_NullProvider_IsUnavailableAndEmpty()
        {
            var service = new SpellCheckService(null);
            Assert.Multiple(() =>
            {
                Assert.That(service.IsAvailable(), Is.False);
                Assert.That(service.GetSuggestions("x"), Is.Empty);
            });
        }

        // ---- MacSpellCheckProvider contract (no real OS calls) ----

        [Test]
        public void MacProvider_ContractDefaults()
        {
            var provider = new MacSpellCheckProvider();
            Assert.Multiple(() =>
            {
                // Superseded stub (the real engine is HunspellSpellCheckEngine in
                // App.Avalonia): treats words as correct, exposes no
                // suggestions/availability, and must not throw.
                Assert.That(provider.IsWordCorrect("anyword", "en"), Is.True);
                Assert.That(provider.GetSuggestions("anyword", "en"), Is.Empty);
                Assert.That(provider.IsAvailable("en"), Is.False);
                Assert.DoesNotThrow(() => provider.AddToUserDictionary("word", "en"));
            });
        }

        private sealed class FakeProvider : ISpellCheckProvider
        {
            private readonly bool _available;
            public FakeProvider(bool available) => _available = available;
            public string[] Suggestions { get; set; } = Array.Empty<string>();
            public bool IsWordCorrect(string word, string language) => true;
            public string[] GetSuggestions(string word, string language) => Suggestions;
            public void AddToUserDictionary(string word, string language) { }
            public bool IsAvailable(string language) => _available;
        }
    }
}
