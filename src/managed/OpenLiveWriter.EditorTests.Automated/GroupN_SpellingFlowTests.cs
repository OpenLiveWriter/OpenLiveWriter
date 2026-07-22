// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.App.Avalonia.Spelling;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group N (continued) — the P1-7 on-demand spelling flow: the pure
    /// <see cref="SpellingSession"/> walk (scan, context, ignore/change semantics) and
    /// HTML-safe replacement fixtures (<see cref="SpellingHtml"/>), the real
    /// <see cref="HunspellSpellCheckEngine"/> against the embedded en-US dictionary
    /// plus user-dictionary persistence, the headless <see cref="SpellingDialog"/>,
    /// and the check-before-publish preference round-trip.
    /// </summary>
    [TestFixture]
    [Category("GroupN")]
    public class GroupN_SpellingFlowTests
    {
        // ---- SpellingHtml.ReplaceOccurrence (single-occurrence, tag-safe) ----

        [Test]
        public void ReplaceOccurrence_TargetsOnlyRequestedOrdinal()
        {
            const string html = "<p>teh cat sat on teh mat, near teh dog</p>";

            string result = SpellingHtml.ReplaceOccurrence(html, "teh", "the", 1, out bool replaced);

            Assert.That(replaced, Is.True);
            Assert.That(result, Is.EqualTo("<p>teh cat sat on the mat, near teh dog</p>"));
        }

        [Test]
        public void ReplaceOccurrence_NeverTouchesMarkupOrAttributes()
        {
            const string html = "<p class=\"teh\" title=\"teh\">teh teh</p>";

            string result = SpellingHtml.ReplaceOccurrence(html, "teh", "the", 1, out bool replaced);

            Assert.That(replaced, Is.True);
            // Only the second text occurrence changes; attribute values are verbatim.
            Assert.That(result, Is.EqualTo("<p class=\"teh\" title=\"teh\">teh the</p>"));
        }

        [Test]
        public void ReplaceOccurrence_RequiresWholeWord()
        {
            const string html = "<p>teh teh123 _teh teh</p>";

            // Only occurrences 0 ("teh" at start) and 1 (trailing "teh") are whole words.
            string result = SpellingHtml.ReplaceOccurrence(html, "teh", "the", 1, out bool replaced);

            Assert.That(replaced, Is.True);
            Assert.That(result, Is.EqualTo("<p>teh teh123 _teh the</p>"));
        }

        [Test]
        public void ReplaceOccurrence_OutOfRangeOrdinal_IsNoOp()
        {
            const string html = "<p>teh once</p>";
            string result = SpellingHtml.ReplaceOccurrence(html, "teh", "the", 5, out bool replaced);
            Assert.Multiple(() =>
            {
                Assert.That(replaced, Is.False);
                Assert.That(result, Is.EqualTo(html));
            });
        }

        // ---- SpellingSession scan / walk (fake engine) ----

        private static InMemorySpellCheckEngine EngineWithWords(params string[] words) =>
            new InMemorySpellCheckEngine(words);

        [Test]
        public void Scan_FindsMisspellingsInPlainTextOrder()
        {
            var engine = EngineWithWords("the", "cat", "sat", "on", "mat");
            var session = new SpellingSession("<p>the cat sat on teh mat</p>", engine);

            Assert.That(session.MisspellingCount, Is.EqualTo(1));
            Assert.That(session.Current.Word, Is.EqualTo("teh"));
            Assert.That(session.Current.Context, Is.EqualTo("the cat sat on teh mat"));
        }

        [Test]
        public void Scan_SkipsDigitsSingleLettersAndAcronyms()
        {
            var engine = EngineWithWords("version", "is", "great", "a");
            // "abc123" (digits), "x" (single letter), "HTML" (all caps) must not be flagged.
            var session = new SpellingSession(
                "<p>version abc123 is great a x HTML</p>", engine);

            Assert.That(session.MisspellingCount, Is.EqualTo(0));
        }

        [Test]
        public void Scan_StripsTagsBeforeChecking()
        {
            var engine = EngineWithWords("hello", "world");
            // "misspelledword" lives only inside an attribute — never scanned.
            var session = new SpellingSession(
                "<p><span title=\"misspelledword\">hello world</span></p>", engine);
            Assert.That(session.MisspellingCount, Is.EqualTo(0));
        }

        [Test]
        public void Ignore_AdvancesWithoutModifyingHtml()
        {
            var engine = EngineWithWords("good");
            const string html = "<p>badone good badtwo</p>";
            var session = new SpellingSession(html, engine);

            Assert.That(session.Current.Word, Is.EqualTo("badone"));
            session.Ignore();
            Assert.That(session.Current.Word, Is.EqualTo("badtwo"));
            session.Ignore();
            Assert.Multiple(() =>
            {
                Assert.That(session.Current, Is.Null);
                Assert.That(session.WasModified, Is.False);
                Assert.That(session.ResultHtml, Is.EqualTo(html));
            });
        }

        [Test]
        public void IgnoreAll_SuppressesFutureOccurrencesOfTheWord()
        {
            var engine = EngineWithWords("the", "cat", "sat");
            var session = new SpellingSession("<p>teh cat sat teh</p>", engine);

            Assert.That(session.MisspellingCount, Is.EqualTo(2));
            session.IgnoreAll();

            Assert.Multiple(() =>
            {
                Assert.That(session.Current, Is.Null);
                Assert.That(engine.Check("teh"), Is.True, "IgnoreAll must feed engine.Check");
            });
        }

        [Test]
        public void AddToDictionary_PersistsIntoEngine()
        {
            var engine = EngineWithWords("hello");
            var session = new SpellingSession("<p>helloworld hello</p>", engine);

            session.AddToDictionary();

            Assert.Multiple(() =>
            {
                Assert.That(engine.Check("helloworld"), Is.True);
                Assert.That(engine.UserDictionaryWords, Does.Contain("helloworld"));
                Assert.That(session.Current, Is.Null);
            });
        }

        [Test]
        public void Change_ReplacesOnlyTheReviewedOccurrence()
        {
            var engine = EngineWithWords("the", "cat", "sat", "on", "mat");
            var session = new SpellingSession("<p>teh cat sat on teh mat</p>", engine);

            session.Ignore();          // leave the first "teh" alone
            session.Change("the");     // fix only the second

            Assert.Multiple(() =>
            {
                Assert.That(session.WasModified, Is.True);
                Assert.That(session.ResultHtml, Is.EqualTo("<p>teh cat sat on the mat</p>"));
                Assert.That(session.Current, Is.Null);
            });
        }

        [Test]
        public void Change_SequentialChangesHitSuccessiveOccurrences()
        {
            var engine = EngineWithWords("the");
            var session = new SpellingSession("<p>teh teh teh</p>", engine);

            session.Change("the");
            session.Change("the");
            session.Change("the");

            Assert.That(session.ResultHtml, Is.EqualTo("<p>the the the</p>"));
        }

        [Test]
        public void ChangeAll_ReplacesRemainingOccurrences_AndSkipsThem()
        {
            var engine = EngineWithWords("the", "cat", "sat");
            var session = new SpellingSession("<p>teh cat <b>teh</b> sat teh</p>", engine);

            session.ChangeAll("the");

            Assert.Multiple(() =>
            {
                Assert.That(session.ResultHtml, Is.EqualTo("<p>the cat <b>the</b> sat the</p>"));
                Assert.That(session.Current, Is.Null, "later same-word entries are consumed");
                Assert.That(session.WasModified, Is.True);
            });
        }

        [Test]
        public void CountMisspellings_MatchesScan()
        {
            var engine = EngineWithWords("the", "cat");
            Assert.That(
                SpellingSession.CountMisspellings("<p>teh cat teh</p>", engine), Is.EqualTo(2));
            Assert.That(
                SpellingSession.CountMisspellings("<p>the cat</p>", engine), Is.EqualTo(0));
        }

        [Test]
        public void ExtractContext_ReturnsSurroundingSentence()
        {
            const string text = "First sentence here. The quik brown fox jumps. Last one.";
            int idx = text.IndexOf("quik", StringComparison.Ordinal);
            Assert.That(SpellingSession.ExtractContext(text, idx, 4),
                Is.EqualTo("The quik brown fox jumps."));
        }

        // ---- Real Hunspell engine (embedded en-US dictionary) ----

        private static HunspellSpellCheckEngine CreateRealEngine(string userDictionaryPath = null)
        {
            var assembly = typeof(HunspellSpellCheckEngine).Assembly;
            using Stream dic = assembly.GetManifestResourceStream(HunspellSpellCheckEngine.DictionaryResourceName);
            using Stream aff = assembly.GetManifestResourceStream(HunspellSpellCheckEngine.AffixResourceName);
            Assert.That(dic, Is.Not.Null, "en_US.dic must be embedded");
            Assert.That(aff, Is.Not.Null, "en_US.aff must be embedded");
            return new HunspellSpellCheckEngine(dic, aff, userDictionaryPath);
        }

        [Test]
        public void HunspellEngine_ChecksAndSuggests()
        {
            var engine = CreateRealEngine();
            Assert.Multiple(() =>
            {
                Assert.That(engine.IsAvailable, Is.True);
                Assert.That(engine.Check("hello"), Is.True);
                Assert.That(engine.Check("helo"), Is.False);
                Assert.That(engine.Suggest("helo"), Is.Not.Empty);
            });
        }

        [Test]
        public void HunspellEngine_UserDictionary_PersistsAndReloads()
        {
            string dir = Path.Combine(Path.GetTempPath(), "olw-userdict-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(dir, "user-dictionary.txt");
            try
            {
                var engine = CreateRealEngine(path);
                {
                    Assert.That(engine.Check("Avalonia"), Is.False);
                    engine.AddToUserDictionary("Avalonia");
                    Assert.That(engine.Check("Avalonia"), Is.True);
                }

                Assert.That(File.Exists(path), Is.True);
                Assert.That(File.ReadAllText(path), Does.Contain("Avalonia"));

                var reloaded = CreateRealEngine(path);
                Assert.That(reloaded.Check("Avalonia"), Is.True, "user dictionary must reload");
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
        }

        [Test]
        public void HunspellEngine_IgnoreAll_IsSessionScoped()
        {
            string dir = Path.Combine(Path.GetTempPath(), "olw-userdict-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(dir, "user-dictionary.txt");
            try
            {
                var engine = CreateRealEngine(path);
                {
                    engine.IgnoreAll("Avalonia");
                    Assert.That(engine.Check("Avalonia"), Is.True);
                }

                Assert.That(File.Exists(path), Is.False, "IgnoreAll must not persist");

                var reloaded = CreateRealEngine(path);
                Assert.That(reloaded.Check("Avalonia"), Is.False);
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
        }

        // ---- SpellingDialog (headless) ----

        private static Button FindButton(Control root, string content) =>
            root.GetLogicalDescendants().OfType<Button>()
                .FirstOrDefault(b => (b.Content as string) == content);

        private static void Click(Button button) =>
            button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        [AvaloniaTest]
        public void SpellingDialog_ShowsFirstMisspellingWithSuggestions()
        {
            var engine = EngineWithWords("the", "cat", "sat", "on", "mat");
            engine.Suggestions["teh"] = new[] { "the", "ten" };
            var dialog = new SpellingDialog("<p>the cat sat on teh mat</p>", engine);

            Assert.Multiple(() =>
            {
                Assert.That(dialog.Session.Current.Word, Is.EqualTo("teh"));
                Assert.That(dialog.Session.GetSuggestions(), Is.EqualTo(new[] { "the", "ten" }));
                Assert.That(FindButton(dialog, "Change"), Is.Not.Null);
                Assert.That(FindButton(dialog, "Change All"), Is.Not.Null);
                Assert.That(FindButton(dialog, "Ignore"), Is.Not.Null);
                Assert.That(FindButton(dialog, "Ignore All"), Is.Not.Null);
                Assert.That(FindButton(dialog, "Add to Dictionary"), Is.Not.Null);
                Assert.That(FindButton(dialog, "Close"), Is.Not.Null);
            });
        }

        [AvaloniaTest]
        public void SpellingDialog_ChangeButton_AppliesSuggestionToResultHtml()
        {
            var engine = EngineWithWords("the", "cat");
            engine.Suggestions["teh"] = new[] { "the" };
            var dialog = new SpellingDialog("<p>teh cat</p>", engine);

            Click(FindButton(dialog, "Change"));

            Assert.Multiple(() =>
            {
                Assert.That(dialog.WasModified, Is.True);
                Assert.That(dialog.ResultHtml, Is.EqualTo("<p>the cat</p>"));
                Assert.That(dialog.Session.Current, Is.Null);
            });
        }

        [AvaloniaTest]
        public void SpellingDialog_IgnoreAllButton_AdvancesPastEveryOccurrence()
        {
            var engine = EngineWithWords("cat");
            var dialog = new SpellingDialog("<p>teh cat teh</p>", engine);

            Click(FindButton(dialog, "Ignore All"));

            Assert.Multiple(() =>
            {
                Assert.That(dialog.Session.Current, Is.Null);
                Assert.That(dialog.WasModified, Is.False);
            });
        }

        // ---- Check-before-publish preference ----

        [Test]
        public void CheckSpellingBeforePublishing_RoundTripsThroughStore()
        {
            var root = new MemorySettingsPersister();
            var store = AppPreferencesStore.ForPersisterFactory(() => root);

            var prefs = AppPreferences.CreateDefault();
            Assert.That(prefs.CheckSpellingBeforePublishing, Is.False, "default is off");

            prefs.CheckSpellingBeforePublishing = true;
            store.Save(prefs);

            Assert.That(store.Load().CheckSpellingBeforePublishing, Is.True);
        }

        [Test]
        public void AppPreferences_Clone_CopiesSpellingGate()
        {
            var prefs = AppPreferences.CreateDefault();
            prefs.CheckSpellingBeforePublishing = true;
            Assert.That(prefs.Clone().CheckSpellingBeforePublishing, Is.True);
        }
    }
}
