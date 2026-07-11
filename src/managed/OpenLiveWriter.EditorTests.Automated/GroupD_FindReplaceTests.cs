// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group D — Find and Find &amp; Replace (P1). The in-page highlight uses the
    /// browser's native find inside the WKWebView (verified live), but the matching
    /// and replacement contract lives in the pure <see cref="TextFinder"/> and is
    /// covered here headlessly, including the HTML-aware Replace All that must leave
    /// tags untouched.
    /// </summary>
    [TestFixture]
    [Category("GroupD")]
    public class GroupD_FindReplaceTests
    {
        // --- Plain-text find ---

        [Test]
        public void FindAll_CountsNonOverlappingMatches()
        {
            var matches = TextFinder.FindAll("the cat sat on the mat", "at", matchCase: false, wholeWord: false);
            // "cat", "sat", "mat" contain "at" → 3.
            Assert.That(matches.Count, Is.EqualTo(3));
        }

        [Test]
        public void FindAll_CaseSensitivity()
        {
            Assert.That(TextFinder.Count("Cat cat CAT", "cat", matchCase: true, wholeWord: false), Is.EqualTo(1));
            Assert.That(TextFinder.Count("Cat cat CAT", "cat", matchCase: false, wholeWord: false), Is.EqualTo(3));
        }

        [Test]
        public void FindAll_WholeWordOnly()
        {
            Assert.That(TextFinder.Count("cat category catalog cat", "cat", matchCase: false, wholeWord: true),
                Is.EqualTo(2));
        }

        [TestCase("", "x")]
        [TestCase("abc", "")]
        [TestCase("abc", "xyz")]
        public void FindAll_NoMatch_ReturnsEmpty(string text, string query)
        {
            Assert.That(TextFinder.Count(text, query, matchCase: false, wholeWord: false), Is.EqualTo(0));
        }

        [Test]
        public void IndexOfNext_FindsForwardThenWraps()
        {
            const string text = "aXbXc";
            Assert.That(TextFinder.IndexOfNext(text, "X", 0, false, false, wrap: true), Is.EqualTo(1));
            Assert.That(TextFinder.IndexOfNext(text, "X", 2, false, false, wrap: true), Is.EqualTo(3));
            // Past the last match, wrap back to the first.
            Assert.That(TextFinder.IndexOfNext(text, "X", 4, false, false, wrap: true), Is.EqualTo(1));
            Assert.That(TextFinder.IndexOfNext(text, "X", 4, false, false, wrap: false), Is.EqualTo(-1));
        }

        // --- Plain-text replace ---

        [Test]
        public void ReplaceAll_ReplacesEveryMatch()
        {
            string result = TextFinder.ReplaceAll("foo bar foo", "foo", "baz", false, false, out int count);
            Assert.That(result, Is.EqualTo("baz bar baz"));
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void ReplaceAll_WholeWord_LeavesSubstringsAlone()
        {
            string result = TextFinder.ReplaceAll("cat category cat", "cat", "dog", false, wholeWord: true, out int count);
            Assert.That(result, Is.EqualTo("dog category dog"));
            Assert.That(count, Is.EqualTo(2));
        }

        // --- HTML-aware replace (tags preserved) ---

        [Test]
        public void ReplaceAllInHtml_ReplacesTextNotTags()
        {
            const string html = "<p class=\"cat\">cat and cat</p>";
            string result = TextFinder.ReplaceAllInHtml(html, "cat", "dog", false, false, out int count);
            // The class attribute value "cat" must NOT be replaced.
            Assert.That(result, Is.EqualTo("<p class=\"cat\">dog and dog</p>"));
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void ReplaceAllInHtml_LeavesMarkupWellFormed()
        {
            const string html = "<p>alpha</p><h2>alpha beta</h2>";
            string result = TextFinder.ReplaceAllInHtml(html, "alpha", "ALPHA", false, false, out int count);
            Assert.That(result, Is.EqualTo("<p>ALPHA</p><h2>ALPHA beta</h2>"));
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void CountInHtml_IgnoresTagContent()
        {
            const string html = "<a href=\"http://find.me\">find me</a>";
            // Only the visible "find" (in "find me") counts, not the href "find.me".
            Assert.That(TextFinder.CountInHtml(html, "find", false, false), Is.EqualTo(1));
        }

        // --- Dialog field capture (headless UI) ---

        [AvaloniaTest]
        public void FindReplaceDialog_CapturesFieldsIntoRequest()
        {
            var dialog = new FindReplaceDialog(_ => Task.CompletedTask, _ => Task.CompletedTask);
            var findBox = dialog.GetLogicalDescendants().OfType<TextBox>().First();
            findBox.Text = "needle";

            var request = dialog.CurrentRequest();
            Assert.That(request.Query, Is.EqualTo("needle"));
            Assert.That(request.MatchCase, Is.False);
            Assert.That(request.WholeWord, Is.False);
        }
    }
}
