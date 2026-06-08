// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

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

            PlatformSpellCheck.SpellingError spellerStatus;
            try
            {
                spellerStatus = _speller.Check(word).FirstOrDefault();
            }
            catch (COMException)
            {
                // The Windows spell-check COM API can throw COMException when
                // the document is being modified during background spell-checking.
                // Treat as correct rather than crashing.
                offset = 0;
                length = 0;
                return SpellCheckResult.Correct;
            }

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

            try
            {
                _speller = new PlatformSpellCheck.SpellChecker(_bcp47Code);
            }
            catch (UnauthorizedAccessException)
            {
                // COM spell checker access can be blocked by OS permissions or
                // group policy. Fall back gracefully — the app continues without
                // spell checking rather than crashing.
                _speller = null;
            }
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

            try
            {
                foreach (string suggestion in _speller.Suggestions(word).Take(maxSuggestions))
                {
                    list.Add(new SpellingSuggestion(suggestion, 1));
                }
            }
            catch (COMException)
            {
                // The Windows spell-check COM API can throw COMException when
                // the document is being modified during background spell-checking.
                // Return whatever suggestions we have so far rather than crashing.
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
                try
                {
                    return PlatformSpellCheck.SpellChecker.SupportedLanguages.ToArray();
                }
                catch (UnauthorizedAccessException)
                {
                    // COM spell checker access blocked by permissions or group policy.
                }
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
                try
                {
                    return PlatformSpellCheck.SpellChecker.IsLanguageSupported(bcp47Code);
                }
                catch (UnauthorizedAccessException)
                {
                    // COM spell checker access blocked by permissions or group policy.
                }
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
