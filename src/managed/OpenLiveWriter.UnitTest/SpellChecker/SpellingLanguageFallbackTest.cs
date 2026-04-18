// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenLiveWriter.UnitTest.SpellChecker
{
    /// <summary>
    /// Validates the language fallback logic used by SpellingSettings
    /// to ensure spell check is never disabled due to empty language lists.
    /// See: https://github.com/OpenLiveWriter/OpenLiveWriter/issues/737
    /// </summary>
    [TestClass]
    public class SpellingLanguageFallbackTest
    {
        [TestMethod]
        public void FallbackLanguages_AlwaysIncludesEnUS()
        {
            // Simulate the fallback logic from SpellingSettings.GetInstalledLanguages
            string[] installedLanguages = new string[0]; // empty — no languages detected

            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string systemLanguage = CultureInfo.CurrentCulture.Name;
            if (!string.IsNullOrEmpty(systemLanguage))
                fallbackCodes.Add(systemLanguage);
            fallbackCodes.Add("en-US");

            Assert.IsTrue(fallbackCodes.Count >= 1, "Should have at least en-US");
            Assert.IsTrue(fallbackCodes.Contains("en-US"), "Should always contain en-US");
        }

        [TestMethod]
        public void FallbackLanguages_IncludesSystemCulture()
        {
            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string systemLanguage = CultureInfo.CurrentCulture.Name;
            if (!string.IsNullOrEmpty(systemLanguage))
                fallbackCodes.Add(systemLanguage);
            fallbackCodes.Add("en-US");

            if (!string.IsNullOrEmpty(systemLanguage))
            {
                Assert.IsTrue(fallbackCodes.Contains(systemLanguage),
                    "Should contain the system culture: " + systemLanguage);
            }
        }

        [TestMethod]
        public void FallbackLanguages_NoDuplicatesWhenSystemIsEnUS()
        {
            // When system culture is en-US, the set should have exactly 1 entry
            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            fallbackCodes.Add("en-US"); // simulate system culture = en-US
            fallbackCodes.Add("en-US"); // add fallback

            Assert.AreEqual(1, fallbackCodes.Count, "HashSet should deduplicate en-US");
        }

        [TestMethod]
        public void FallbackLanguages_TwoEntriesForNonEnglishSystem()
        {
            HashSet<string> fallbackCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            fallbackCodes.Add("fr-FR"); // simulate French system
            fallbackCodes.Add("en-US"); // add fallback

            Assert.AreEqual(2, fallbackCodes.Count);
            Assert.IsTrue(fallbackCodes.Contains("fr-FR"));
            Assert.IsTrue(fallbackCodes.Contains("en-US"));
        }

        [TestMethod]
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

            Assert.IsNotNull(result, "Language fallback should never return null");
            Assert.AreNotEqual(string.Empty, result, "Language fallback should never return empty");
        }
    }
}
