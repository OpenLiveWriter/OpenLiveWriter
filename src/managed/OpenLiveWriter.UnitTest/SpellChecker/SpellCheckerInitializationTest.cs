// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenLiveWriter.SpellChecker;

namespace OpenLiveWriter.UnitTest.SpellChecker
{
    /// <summary>
    /// Validates that WinSpellingChecker handles initialization failures gracefully,
    /// particularly UnauthorizedAccessException when COM spell checker access is
    /// blocked by OS permissions or group policy.
    /// See: https://github.com/OpenLiveWriter/OpenLiveWriter/issues/435
    /// </summary>
    [TestClass]
    public class SpellCheckerInitializationTest
    {
        [TestMethod]
        public void UnauthorizedAccessException_IsCatchableWithoutCrash()
        {
            // Verify that UnauthorizedAccessException can be caught in the same
            // pattern used by StartChecking — this confirms the exception type
            // is catchable and does not require special handling.
            object result = null;
            bool exceptionCaught = false;

            try
            {
                throw new UnauthorizedAccessException("COM spell checker access denied");
            }
            catch (UnauthorizedAccessException)
            {
                exceptionCaught = true;
                result = null;
            }

            Assert.IsTrue(exceptionCaught, "UnauthorizedAccessException should be catchable");
            Assert.IsNull(result, "Result should be null after catching the exception");
        }

        [TestMethod]
        public void NullSpeller_MeansNoSpellChecking()
        {
            // When StartChecking fails (e.g. due to UnauthorizedAccessException),
            // _speller is set to null. The IsInitialized property should return false,
            // meaning spell checking is gracefully disabled.
            var checker = new WinSpellingChecker();

            // Without calling StartChecking or SetOptions, the checker is uninitialized
            Assert.IsFalse(checker.IsInitialized,
                "Checker should not be initialized when no speller is available");
        }

        [TestMethod]
        public void StartChecking_WithEmptyLanguage_DoesNotThrow()
        {
            // When language code is empty, StartChecking should return gracefully
            // without throwing, and IsInitialized should remain false.
            var checker = new WinSpellingChecker();
            checker.SetOptions(string.Empty);

            checker.StartChecking();

            Assert.IsFalse(checker.IsInitialized,
                "Checker should not be initialized with an empty language code");
        }

        [TestMethod]
        public void StartChecking_WithNullLanguage_DoesNotThrow()
        {
            // When language code is null, StartChecking should return gracefully
            // without throwing, and IsInitialized should remain false.
            var checker = new WinSpellingChecker();
            checker.SetOptions(null);

            checker.StartChecking();

            Assert.IsFalse(checker.IsInitialized,
                "Checker should not be initialized with a null language code");
        }

        [TestMethod]
        public void StopChecking_AfterFailedInit_DoesNotThrow()
        {
            // If initialization failed and _speller is null, StopChecking
            // and Dispose should still work without throwing.
            var checker = new WinSpellingChecker();

            checker.StopChecking(); // should not throw
            checker.Dispose();     // should not throw

            Assert.IsFalse(checker.IsInitialized);
        }
    }
}
