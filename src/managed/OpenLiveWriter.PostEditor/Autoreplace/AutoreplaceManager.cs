// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.Autoreplace
{
    public class AutoreplaceManager : IAutoReplaceFinder
    {
        private static readonly object _lock = new object();
        private static Dictionary<string, AutoReplaceFinder> finderFileCache = new Dictionary<string, AutoReplaceFinder>();
        private AutoReplaceFinder finderFile;
        private AutoReplaceFinder finderRegistry;
        private readonly CanMatch canMatch;

        public AutoreplaceManager()
        {
            canMatch = (text, charactersMatched) => (charactersMatched >= text.Length ||
                                                           !char.IsLetterOrDigit(text[charactersMatched]));

            SetAutoCorrectFile(null);
            ReloadPhraseSettings();
        }

        /// <summary>
        /// Sets the autocorrect file path, or null if none.
        /// </summary>
        public void SetAutoCorrectFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                finderFile = new AutoReplaceFinder(canMatch);
                return;
            }

            lock (_lock)
            {
                if (!finderFileCache.TryGetValue(path, out finderFile))
                {
                    AutoReplaceFinder newFinderFile = new AutoReplaceFinder(canMatch);

                    using (Stream s = File.OpenRead(path))
                    {
                        foreach (var pair in AutoCorrectLexiconReader.Read(s))
                        {
                            newFinderFile.Add(NormalizeInputValue(pair.From), pair.To);
                        }
                    }

                    finderFile = newFinderFile;
                    finderFileCache.Add(path, finderFile);
                }
            }

        }

        public void ReloadPhraseSettings()
        {
            lock (_lock)
            {
                AutoReplaceFinder newFinderRegistry = new AutoReplaceFinder(canMatch);
                foreach (var phrase in AutoreplaceSettings.AutoreplacePhrases)
                {
                    newFinderRegistry.Add(NormalizeInputValue(phrase.Phrase), phrase.ReplaceValue);
                }
                finderRegistry = newFinderRegistry;
            }
        }

        public string FindMatch(string text, out int length)
        {
            lock (_lock)
            {
                text = NormalizeInputValue(text);

                // We dont need to do the replacement here because
                // we do it in TypographicCharacterHandler and that is
                // where we check to make sure the hosting application
                // can handle unicode ellipsis
                string invariantText = text.ToLowerInvariant();
                if (TypographicCharacterHandler.SpecialCharacters.Exists(typographic => invariantText.EndsWith(typographic)))
                {
                    length = -1;
                    return null;
                }

                string result = finderFile.FindMatch(text, out length);
                if (result == null)
                    result = finderRegistry.FindMatch(text, out length);

                if (result == null)
                    return null;

                return MakeCaseConsistent(text.Substring(text.Length - length), NormalizeOutputValue(result));
            }
        }

        private static string MakeCaseConsistent(string input, string output)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
                return output;

            bool isFirstUpper = char.IsUpper(input[0]);
            bool isRestUpper = isFirstUpper;
            for (int i = 1; isRestUpper && i < input.Length; i++)
                if (char.IsLetter(input[i]) && !char.IsUpper(input[i]))
                    isRestUpper = false;

            if (isRestUpper)
                output = output.ToUpperInvariant();
            else if (isFirstUpper && !char.IsUpper(output[0]))
                output = char.ToUpperInvariant(output[0]) + output.Substring(1);

            return output;
        }

        public int MaxLengthHint
        {
            get
            {
                lock (_lock)
                {
                    return Math.Max(finderFile.MaxLengthHint, finderRegistry.MaxLengthHint);
                }
            }
        }

        private string NormalizeInputValue(string s)
        {
            if (s == null)
                return null;

            return s
                .Replace('\u2018', '\'')
                .Replace('\u2019', '\'')
                .Replace('\u201C', '"')
                .Replace('\u201D', '"');
        }

        private string NormalizeOutputValue(string s)
        {
            if (s == null)
                return null;

            return AutoreplaceSettings.EnableSmartQuotes
                ? s.Replace('\'', '\u2019')
                : s.Replace('\u2019', '\'');
        }
    }

    internal class AutoCorrectLexiconReader
    {
        public struct Pair
        {
            public readonly string From;
            public readonly string To;

            public Pair(string @from, string to)
            {
                From = from;
                To = to;
            }
        }

        public static IEnumerable<Pair> Read(Stream data)
        {
            if (data.Length < 20)
                yield break;

            // Skip header
            data.Seek(20, SeekOrigin.Begin);

            byte[] buffer = new byte[100];
            while (true)
            {
                string from = ReadNextWord(data, buffer);
                if (string.IsNullOrEmpty(from))
                    yield break;
                string to = ReadNextWord(data, buffer);
                if (string.IsNullOrEmpty(to))
                {
                    Debug.Fail("Odd number of words found in autocorrect lexicon");
                    yield break;
                }
                yield return new Pair(from, to);
            }
        }

        private static string ReadNextWord(Stream stream, byte[] buffer)
        {
            int bytesRead = stream.Read(buffer, 0, 2);
            if (bytesRead != 2)
            {
                Debug.Assert(bytesRead == 0, "Malformed autocorrect lexicon--extra byte before EOF");
                return null;
            }

            short charCount = BitConverter.ToInt16(buffer, 0);

            if (charCount == 0)
            {
                // The ACL file has a bunch of stuff after charCount==0 occurs, that doesn't relate to autocorrect.
                // We want to skip all that stuff.
                stream.Seek(0, SeekOrigin.End);
                return null;
            }

            if (charCount * 2 > buffer.Length)
            {
                Debug.Fail("Autocorrect lexicon word too long for buffer: " + charCount);
                return null;
            }

            // Read characters plus null terminator
            bytesRead = stream.Read(buffer, 0, 2 * charCount);
            if (bytesRead != 2 * charCount)
            {
                Debug.Fail("Unexpected EOF");
                return null;
            }

            if (stream.ReadByte() != 0 || stream.ReadByte() != 0)
            {
                Debug.Fail("Autocorrect lexicon word was not properly null-terminated or had incorrect char count");
                return null;
            }

            return Encoding.Unicode.GetString(buffer, 0, charCount * 2);
        }
    }

}
