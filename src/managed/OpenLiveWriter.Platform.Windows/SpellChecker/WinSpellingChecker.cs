// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenLiveWriter.SpellChecker
{
    public class WinSpellingChecker : ISpellingChecker, IDisposable
    {
        private PlatformSpellCheck.SpellChecker _speller;
        private string _bcp47Code;

        public bool IsInitialized
        {
            get
            {
                return _speller != null;
            }
        }

        public event EventHandler WordAdded;
        public event EventHandler WordIgnored;

        public void AddToUserDictionary(string word)
        {
            CheckInitialized();
            _speller.Add(word);

            if (WordAdded == null)
                return;
            WordAdded(word, EventArgs.Empty);
        }

        public SpellCheckResult CheckWord(string word, out string otherWord, out int offset, out int length)
        {
            CheckInitialized();
            otherWord = null;

            if (string.IsNullOrEmpty(word))
            {
                offset = 0;
                length = 0;
                return SpellCheckResult.Correct;
            }

            PlatformSpellCheck.SpellingError spellerStatus = _speller.Check(word).FirstOrDefault();

            if (spellerStatus == null)
            {
                offset = 0;
                length = word.Length;
                return SpellCheckResult.Correct;
            }
            else
            {
                offset = (int)spellerStatus.StartIndex;
                length = (int)spellerStatus.Length;

                switch (spellerStatus.RecommendedAction)
                {
                    case PlatformSpellCheck.RecommendedAction.Delete:
                        otherWord = "";
                        return SpellCheckResult.AutoReplace;
                    case PlatformSpellCheck.RecommendedAction.Replace:
                        otherWord = spellerStatus.RecommendedReplacement;
                        return SpellCheckResult.AutoReplace;
                    case PlatformSpellCheck.RecommendedAction.GetSuggestions:
                        return SpellCheckResult.Misspelled;
                    default:
                        return SpellCheckResult.Correct;
                }
            }
        }

        public void Dispose()
        {
            StopChecking();
        }

        public void IgnoreAll(string word)
        {
            CheckInitialized();
            _speller.Ignore(word);

            if (WordIgnored == null)
                return;
            WordIgnored(word, EventArgs.Empty);
        }

        public void ReplaceAll(string word, string replaceWith)
        {
            CheckInitialized();
            _speller.AutoCorrect(word, replaceWith);
        }

        public void StartChecking()
        {
            if (!PlatformSpellCheck.SpellChecker.IsPlatformSupported() ||
                string.IsNullOrEmpty(_bcp47Code))
            {
                StopChecking();
                return;
            }

            _speller = new PlatformSpellCheck.SpellChecker(_bcp47Code);
        }

        public void StopChecking()
        {
            if (_speller != null)
                _speller.Dispose();

            _speller = null;
        }

        public SpellingSuggestion[] Suggest(string word, short maxSuggestions, short depth)
        {
            CheckInitialized();
            List<SpellingSuggestion> list = new List<SpellingSuggestion>();

            foreach (string suggestion in _speller.Suggestions(word).Take(maxSuggestions))
            {
                list.Add(new SpellingSuggestion(suggestion, 1));
            }

            return list.ToArray();
        }

        public void SetOptions(string bcp47Code)
        {
            _bcp47Code = bcp47Code;
        }

        public static string[] GetInstalledLanguages()
        {
            if (PlatformSpellCheck.SpellChecker.IsPlatformSupported())
            {
                return PlatformSpellCheck.SpellChecker.SupportedLanguages.ToArray();
            }

            return new string[0];
        }

        public static bool IsLanguageSupported(string bcp47Code)
        {
            if (string.IsNullOrEmpty(bcp47Code))
            {
                return false;
            }

            if (PlatformSpellCheck.SpellChecker.IsPlatformSupported())
            {
                return PlatformSpellCheck.SpellChecker.IsLanguageSupported(bcp47Code);
            }

            return false;
        }

        private void CheckInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Operation attempted on an uninitialized WinSpellingChecker");
        }
    }
}
