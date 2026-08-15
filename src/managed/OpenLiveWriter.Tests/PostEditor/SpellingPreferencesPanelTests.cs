// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using OpenLiveWriter.SpellChecker;

namespace OpenLiveWriter.Tests.PostEditor
{
    /// <summary>
    /// Covers the spelling preferences panel fallback when the saved dictionary
    /// language is not installed on the machine. The panel previously asserted
    /// (Debug.Fail), which crashed the Options dialog in debug builds whenever
    /// the registry held a language the local spell checker did not list.
    /// </summary>
    [TestFixture]
    public class SpellingPreferencesPanelTests
    {
        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            // SpellingSettings resolves its registry root via ApplicationEnvironment,
            // which the test host does not initialize by default. Use a non-default
            // product name: with the default product name Initialize() throws when the
            // profile has no Personal folder (e.g. the SYSTEM account in a headless
            // test session).
            if (OpenLiveWriter.CoreServices.ApplicationEnvironment.InstallationDirectory == null)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                OpenLiveWriter.CoreServices.ApplicationEnvironment.Initialize(assembly,
                    System.IO.Path.GetDirectoryName(assembly.Location),
                    "Software\\OpenLiveWriter.Tests", "Open Live Writer Tests");
            }
        }

        private static IEnumerable<T> GetAll<T>(Control root) where T : Control
        {
            foreach (Control control in root.Controls)
            {
                if (control is T match)
                    yield return match;
                foreach (var child in GetAll<T>(control))
                    yield return child;
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void UnsupportedSavedLanguage_FallsBackToNone()
        {
            var preferences = new SpellingPreferences
            {
                Language = "xx-NOTSUPPORTED"
            };
            using (var panel = new SpellingPreferencesPanel(preferences))
            {
                var combo = GetAll<ComboBox>(panel).First();
                Assert.AreEqual(0, combo.SelectedIndex,
                    "an uninstalled saved language must fall back to the None entry, not assert");
            }
        }
    }
}
