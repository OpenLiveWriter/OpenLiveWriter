// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenLiveWriter.Platform;
using WeCantSpell.Hunspell;

namespace OpenLiveWriter.App.Avalonia.Spelling
{
    /// <summary>
    /// Real <see cref="ISpellCheckEngine"/> backed by the pure-managed
    /// <c>WeCantSpell.Hunspell</c> engine (no native dependencies) and the LibreOffice
    /// en-US Hunspell dictionary embedded as assembly resources (see
    /// <c>Spelling/Dictionaries/NOTICE.md</c> for provenance and the
    /// GPL/LGPL/MPL tri-license statement).
    ///
    /// The user dictionary is a plain-text file (one word per line) whose path is
    /// resolved through the platform services application-data directory — never
    /// hardcoded; pass an explicit path (or null for no persistence) in tests.
    /// </summary>
    public sealed class HunspellSpellCheckEngine : ISpellCheckEngine
    {
        /// <summary>Embedded-resource name of the en-US word list.</summary>
        public const string DictionaryResourceName =
            "OpenLiveWriter.App.Avalonia.Spelling.Dictionaries.en_US.dic";

        /// <summary>Embedded-resource name of the en-US affix file.</summary>
        public const string AffixResourceName =
            "OpenLiveWriter.App.Avalonia.Spelling.Dictionaries.en_US.aff";

        /// <summary>File name of the plain-text user dictionary under the app data dir.</summary>
        public const string UserDictionaryFileName = "user-dictionary.txt";

        private const int MaxSuggestions = 10;

        private readonly WordList _wordList;
        private readonly string _userDictionaryPath;
        private readonly HashSet<string> _userWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Loads the Hunspell word list from the given streams (fully consumed during
        /// construction) and the user dictionary from <paramref name="userDictionaryPath"/>
        /// when supplied (null disables persistence).
        /// </summary>
        public HunspellSpellCheckEngine(Stream dictionaryStream, Stream affixStream, string userDictionaryPath)
        {
            if (dictionaryStream == null) throw new ArgumentNullException(nameof(dictionaryStream));
            if (affixStream == null) throw new ArgumentNullException(nameof(affixStream));

            _wordList = WordList.CreateFromStreams(dictionaryStream, affixStream);
            _userDictionaryPath = userDictionaryPath;

            if (!string.IsNullOrEmpty(_userDictionaryPath) && File.Exists(_userDictionaryPath))
            {
                foreach (string line in File.ReadAllLines(_userDictionaryPath))
                {
                    string word = line.Trim();
                    if (word.Length > 0)
                        _userWords.Add(word);
                }
            }
        }

        /// <summary>
        /// Builds the production engine from the embedded en-US dictionary, resolving
        /// the user-dictionary path through the initialized platform context. Returns
        /// null when the dictionary resources are unavailable.
        /// </summary>
        public static HunspellSpellCheckEngine CreateDefault()
        {
            Assembly assembly = typeof(HunspellSpellCheckEngine).Assembly;
            using Stream dictionary = assembly.GetManifestResourceStream(DictionaryResourceName);
            using Stream affix = assembly.GetManifestResourceStream(AffixResourceName);
            if (dictionary == null || affix == null)
                return null;

            return new HunspellSpellCheckEngine(dictionary, affix, ResolveUserDictionaryPath());
        }

        /// <summary>
        /// The platform-resolved user-dictionary path
        /// (<c>&lt;ApplicationData&gt;/Spelling/user-dictionary.txt</c>), or null when the
        /// platform context is not initialized (headless tests) — persistence is then off.
        /// </summary>
        internal static string ResolveUserDictionaryPath()
        {
            if (!PlatformContext.IsInitialized)
                return null;
            return Path.Combine(
                PlatformContext.Services.GetApplicationDataDirectory(),
                "Spelling",
                UserDictionaryFileName);
        }

        public bool IsAvailable => _wordList != null;

        public bool Check(string word)
        {
            if (string.IsNullOrEmpty(word) || _wordList == null)
                return true;
            if (_userWords.Contains(word) || _ignored.Contains(word))
                return true;
            if (_wordList.Check(word))
                return true;
            // ALL-CAPS tokens (acronyms) validate against their lowercase form.
            return word.Length > 1 && word.All(char.IsUpper) && _wordList.Check(word.ToLowerInvariant());
        }

        public IReadOnlyList<string> Suggest(string word)
        {
            if (string.IsNullOrEmpty(word) || _wordList == null)
                return Array.Empty<string>();
            return _wordList.Suggest(word).Take(MaxSuggestions).ToArray();
        }

        public void IgnoreAll(string word)
        {
            if (!string.IsNullOrEmpty(word))
                _ignored.Add(word);
        }

        public void AddToUserDictionary(string word)
        {
            if (string.IsNullOrWhiteSpace(word) || !_userWords.Add(word.Trim()))
                return;

            if (string.IsNullOrEmpty(_userDictionaryPath))
                return; // persistence disabled (tests / uninitialized platform)

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_userDictionaryPath));
                File.AppendAllText(_userDictionaryPath, word.Trim() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // The word still applies for this session; a read-only settings
                // directory must not break spell checking.
                Console.WriteLine($"[OLW-Spelling] Could not persist user dictionary: {ex.Message}");
            }
        }
    }
}
