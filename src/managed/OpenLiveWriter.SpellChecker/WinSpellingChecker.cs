// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace OpenLiveWriter.SpellChecker
{
    public class WinSpellingChecker : ISpellingChecker, IDisposable
    {
        public bool IsInitialized
        {
            get
            {
                // TODO
                return false;
            }
        }

        public event EventHandler WordAdded;
        public event EventHandler WordIgnored;

        public void AddToUserDictionary(string word)
        {
            // TODO
            if (WordAdded == null)
                return;
            WordAdded(word, EventArgs.Empty);
        }

        public SpellCheckResult CheckWord(string word, out string otherWord, out int offset, out int length)
        {
            // TODO
            otherWord = null;
            offset = 0;
            length = word.Length;
            return SpellCheckResult.Correct;
        }

        public void Dispose()
        {
            // TODO
        }

        public void IgnoreAll(string word)
        {
            // TODO
            if (WordIgnored == null)
                return;
            WordIgnored(word, EventArgs.Empty);
        }

        public void ReplaceAll(string word, string replaceWith)
        {
            // TODO
        }

        public void Reset()
        {
            // TODO
        }

        public void StartChecking(string contextDictionaryLocation)
        {
            // TODO
        }

        public void StopChecking()
        {
            // TODO
        }

        public SpellingSuggestion[] Suggest(string word, short maxSuggestions, short depth)
        {
            // TODO
            List<SpellingSuggestion> list = new List<SpellingSuggestion>();
            return list.ToArray();
        }

        public void SetOptions(string engineDllPath, ushort lcid, string[] mainLexPaths, string userLexiconPath, uint sobitOptions)
        {
            // TODO
        }
    }
}
