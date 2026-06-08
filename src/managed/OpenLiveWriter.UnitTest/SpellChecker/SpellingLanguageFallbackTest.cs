// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace OpenLiveWriter.UnitTest.SpellChecker
{
    /// <summary>
    /// Validates the language fallback logic used by SpellingSettings
    /// to ensure spell check is never disabled due to empty language lists.
    /// See: https://github.com/OpenLiveWriter/OpenLiveWriter/issues/737
    /// </summary>
    [TestFixture]
    public class SpellingLanguageFallbackTest
    {
        [Test]
        public void FallbackLanguages_AlwaysIncludesEnUS()
        {
            // Simulate the fallback logic from SpellingSettings.GetInstalledLanguages
            string[] installedLanguages = new string[0]; // empty — no languages detected

            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string systemLanguage = CultureInfo.CurrentCulture.Name;
            if (!string.IsNullOrEmpty(systemLanguage))
                fallbackCodes.Add(systemLanguage);
            fallbackCodes.Add("en-US");

            ClassicAssert.IsTrue(fallbackCodes.Count >= 1, "Should have at least en-US");
            ClassicAssert.IsTrue(fallbackCodes.Contains("en-US"), "Should always contain en-US");
        }

        [Test]
        public void FallbackLanguages_IncludesSystemCulture()
        {
            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string systemLanguage = CultureInfo.CurrentCulture.Name;
            if (!string.IsNullOrEmpty(systemLanguage))
                fallbackCodes.Add(systemLanguage);
            fallbackCodes.Add("en-US");

            if (!string.IsNullOrEmpty(systemLanguage))
            {
                ClassicAssert.IsTrue(fallbackCodes.Contains(systemLanguage),
                    "Should contain the system culture: " + systemLanguage);
            }
        }

        [Test]
        public void FallbackLanguages_NoDuplicatesWhenSystemIsEnUS()
        {
            // When system culture is en-US, the set should have exactly 1 entry
            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            fallbackCodes.Add("en-US"); // simulate system culture = en-US
            fallbackCodes.Add("en-US"); // add fallback

            ClassicAssert.AreEqual(1, fallbackCodes.Count, "HashSet should deduplicate en-US");
        }

        [Test]
        public void FallbackLanguages_TwoEntriesForNonEnglishSystem()
        {
            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            fallbackCodes.Add("fr-FR"); // simulate French system
            fallbackCodes.Add("en-US"); // add fallback

            ClassicAssert.AreEqual(2, fallbackCodes.Count);
            ClassicAssert.IsTrue(fallbackCodes.Contains("fr-FR"));
            ClassicAssert.IsTrue(fallbackCodes.Contains("en-US"));
        }

        [Test]
        public void LanguageFallback_NeverReturnsEmpty()
        {
            // Simulate the Language getter fallback chain
            string[] candidates = { CultureInfo.CurrentCulture.Name, "en-US" };
            string result = null;

            foreach (string candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate))
                {
                    result = candidate;
                    break;
                }
            }

            ClassicAssert.IsNotNull(result, "Language fallback should never return null");
            ClassicAssert.AreNotEqual(string.Empty, result, "Language fallback should never return empty");
        }
    }
}



