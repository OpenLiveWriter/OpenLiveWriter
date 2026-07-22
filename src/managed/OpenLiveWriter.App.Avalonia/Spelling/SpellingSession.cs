// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenLiveWriter.App.Avalonia.Editor;

namespace OpenLiveWriter.App.Avalonia.Spelling
{
    /// <summary>
    /// Pure, WebView-independent spell-check walk over a post's HTML. The body is
    /// converted to plain text via <see cref="WordCounter.HtmlToPlainText"/>,
    /// tokenized into words, and each token is checked against an
    /// <see cref="ISpellCheckEngine"/>; misspellings are exposed one at a time with
    /// sentence context and suggestions for the Spelling dialog to render.
    ///
    /// Replacement contract (unit-tested with HTML fixtures):
    ///  - <see cref="Change"/> replaces exactly the occurrence under review, located
    ///    by its per-word ordinal via <see cref="SpellingHtml.ReplaceOccurrence"/>
    ///    (whole-word, case-sensitive, never inside tags/attributes).
    ///  - <see cref="ChangeAll"/> replaces every remaining whole-word, case-sensitive
    ///    occurrence via <see cref="Editor.TextFinder.ReplaceAllInHtml"/>; later
    ///    entries for that word are skipped.
    /// The entry list is computed once at construction; replacements do not rescan,
    /// so a replacement word that is itself misspelled is not re-flagged.
    /// </summary>
    public sealed class SpellingSession
    {
        // Words = letters/digits runs with optional internal apostrophes (don't, it’s).
        private static readonly Regex TokenRegex = new(
            @"[\p{L}\p{N}]+(?:['’][\p{L}]+)*", RegexOptions.Compiled);

        private readonly ISpellCheckEngine _engine;
        private readonly List<Entry> _entries;
        private readonly Dictionary<string, int> _changedCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        // Words whose remaining entries are done (Change All / Ignore All / Add to
        // Dictionary) — the walk skips them without further user input.
        private readonly HashSet<string> _changeAllWords = new HashSet<string>(StringComparer.Ordinal);
        private string _html;
        private int _index;

        public SpellingSession(string html, ISpellCheckEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _html = html ?? string.Empty;
            _entries = Scan(_html, _engine);
            _index = 0;
            SkipConsumed();
        }

        /// <summary>One misspelled word occurrence from the scan.</summary>
        public sealed class Entry
        {
            public string Word { get; internal set; }
            public string Context { get; internal set; }

            /// <summary>0-based ordinal among same-word (case-sensitive) occurrences.</summary>
            public int Ordinal { get; internal set; }
        }

        /// <summary>The misspelling currently under review; null when the walk is done.</summary>
        public Entry Current => _index < _entries.Count ? _entries[_index] : null;

        /// <summary>Total misspelled occurrences found by the initial scan.</summary>
        public int MisspellingCount => _entries.Count;

        /// <summary>The post HTML with all accepted replacements applied.</summary>
        public string ResultHtml => _html;

        /// <summary>True when at least one Change / Change All was applied.</summary>
        public bool WasModified { get; private set; }

        /// <summary>Suggestions for the current misspelling from the engine.</summary>
        public IReadOnlyList<string> GetSuggestions() =>
            Current != null ? _engine.Suggest(Current.Word) : Array.Empty<string>();

        /// <summary>Skips the current occurrence only.</summary>
        public void Ignore() => Advance();

        /// <summary>Ignores this word for the rest of the session and moves on.</summary>
        public void IgnoreAll()
        {
            if (Current != null)
            {
                _engine.IgnoreAll(Current.Word);
                _changeAllWords.Add(Current.Word); // later entries for this word are done
            }
            Advance();
        }

        /// <summary>Adds the word to the user dictionary (persisted) and moves on.</summary>
        public void AddToDictionary()
        {
            if (Current != null)
            {
                _engine.AddToUserDictionary(Current.Word);
                _changeAllWords.Add(Current.Word); // now correct — skip its later entries
            }
            Advance();
        }

        /// <summary>Replaces exactly the occurrence under review and moves on.</summary>
        public void Change(string replacement)
        {
            Entry current = Current;
            if (current == null || string.IsNullOrEmpty(replacement))
                return;

            _changedCounts.TryGetValue(current.Word, out int alreadyChanged);
            int ordinalInCurrentHtml = current.Ordinal - alreadyChanged;

            _html = SpellingHtml.ReplaceOccurrence(
                _html, current.Word, replacement, ordinalInCurrentHtml, out bool replaced);
            if (replaced)
            {
                WasModified = true;
                _changedCounts[current.Word] = alreadyChanged + 1;
            }
            Advance();
        }

        /// <summary>Replaces every remaining whole-word occurrence of the current word.</summary>
        public void ChangeAll(string replacement)
        {
            Entry current = Current;
            if (current == null || string.IsNullOrEmpty(replacement))
                return;

            _html = TextFinder.ReplaceAllInHtml(
                _html, current.Word, replacement,
                matchCase: true, wholeWord: true, out int count);
            if (count > 0)
                WasModified = true;
            _changeAllWords.Add(current.Word);
            Advance();
        }

        /// <summary>Counts misspelled occurrences in a post body (publish gate).</summary>
        public static int CountMisspellings(string html, ISpellCheckEngine engine) =>
            Scan(html ?? string.Empty, engine ?? throw new ArgumentNullException(nameof(engine))).Count;

        /// <summary>
        /// Tokenizes the plain-text form of <paramref name="html"/> and returns every
        /// misspelled occurrence in document order. Tokens containing digits, single
        /// letters, and ALL-CAPS acronyms are skipped (matches common editor behavior
        /// and keeps URLs/codes out of the misspelling list).
        /// </summary>
        internal static List<Entry> Scan(string html, ISpellCheckEngine engine)
        {
            var entries = new List<Entry>();
            if (engine == null || !engine.IsAvailable)
                return entries;

            string plain = WordCounter.HtmlToPlainText(html);
            if (plain.Length == 0)
                return entries;

            var ordinals = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (Match match in TokenRegex.Matches(plain))
            {
                string word = match.Value;
                if (ShouldSkipToken(word))
                    continue;
                if (engine.Check(word))
                    continue;

                ordinals.TryGetValue(word, out int ordinal);
                ordinals[word] = ordinal + 1;

                entries.Add(new Entry
                {
                    Word = word,
                    Ordinal = ordinal,
                    Context = ExtractContext(plain, match.Index, match.Length)
                });
            }
            return entries;
        }

        private static bool ShouldSkipToken(string word)
        {
            if (word.Length < 2)
                return true;
            bool hasLetter = false;
            bool hasLower = false;
            foreach (char c in word)
            {
                if (char.IsDigit(c))
                    return true; // codes, versions, addresses
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                    hasLower |= char.IsLower(c);
                }
            }
            // ALL-CAPS runs (acronyms) are not flagged by the dialog walk.
            return !hasLetter || !hasLower;
        }

        /// <summary>Extracts the sentence containing the token at [index, index+length).</summary>
        internal static string ExtractContext(string text, int index, int length)
        {
            int start = index;
            while (start > 0 && !IsSentenceBreak(text[start - 1]))
                start--;

            int end = index + length;
            while (end < text.Length && !IsSentenceBreak(text[end]))
                end++;
            if (end < text.Length)
                end++; // include the closing punctuation

            return text.Substring(start, end - start).Trim();
        }

        private static bool IsSentenceBreak(char c) =>
            c == '.' || c == '!' || c == '?' || c == '\n';

        private void Advance()
        {
            _index++;
            SkipConsumed();
        }

        // Skips entries whose word is done (Change All removed them from the HTML;
        // Ignore All / Add to Dictionary made them no longer actionable).
        private void SkipConsumed()
        {
            while (_index < _entries.Count && _changeAllWords.Contains(_entries[_index].Word))
                _index++;
        }
    }
}
