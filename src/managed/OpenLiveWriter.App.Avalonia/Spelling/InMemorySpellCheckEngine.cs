// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenLiveWriter.App.Avalonia.Spelling
{
    /// <summary>
    /// In-memory <see cref="ISpellCheckEngine"/> for headless tests and sample-data UI:
    /// a fixed dictionary word set plus an optional per-word suggestion map. Ignore
    /// and user-dictionary state live in memory only (nothing touches disk).
    /// </summary>
    public sealed class InMemorySpellCheckEngine : ISpellCheckEngine
    {
        private readonly HashSet<string> _dictionary;
        private readonly HashSet<string> _ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _userWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public InMemorySpellCheckEngine(IEnumerable<string> dictionaryWords)
        {
            _dictionary = new HashSet<string>(
                dictionaryWords ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Suggestions returned for a word, keyed case-insensitively.</summary>
        public IDictionary<string, string[]> Suggestions { get; } =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Words added via <see cref="AddToUserDictionary"/> (for assertions).</summary>
        public IReadOnlyCollection<string> UserDictionaryWords => _userWords.ToArray();

        public bool IsAvailable => true;

        public bool Check(string word)
        {
            if (string.IsNullOrEmpty(word)) return true;
            return _dictionary.Contains(word) || _userWords.Contains(word) || _ignored.Contains(word);
        }

        public IReadOnlyList<string> Suggest(string word) =>
            word != null && Suggestions.TryGetValue(word, out string[] list)
                ? list
                : Array.Empty<string>();

        public void IgnoreAll(string word)
        {
            if (!string.IsNullOrEmpty(word))
                _ignored.Add(word);
        }

        public void AddToUserDictionary(string word)
        {
            if (!string.IsNullOrEmpty(word))
                _userWords.Add(word);
        }
    }
}
