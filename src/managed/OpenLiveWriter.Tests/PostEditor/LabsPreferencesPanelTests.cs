// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.Tests.PostEditor
{
    /// <summary>
    /// Covers the Labs preferences panel, which hosts the switch between the
    /// managed ribbon and the classic native Windows ribbon. The selection must
    /// round-trip through PostEditorSettings.UseNativeRibbon.
    /// </summary>
    [TestFixture]
    public class LabsPreferencesPanelTests
    {
        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            // PostEditorSettings resolves its registry root via ApplicationEnvironment,
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
        public void Panel_ReflectsSetting_AndSavePersistsSelection()
        {
            var original = PostEditorSettings.UseNativeRibbon;
            try
            {
                PostEditorSettings.UseNativeRibbon = false;
                using (var panel = new LabsPreferencesPanel())
                {
                    var radios = GetAll<RadioButton>(panel).ToList();
                    Assert.AreEqual(2, radios.Count, "Labs panel should offer exactly the two ribbon choices");

                    var managed = radios.First(r => r.Text == Res.Get(StringId.LabsRibbonManaged));
                    var native = radios.First(r => r.Text == Res.Get(StringId.LabsRibbonNative));
                    Assert.IsTrue(managed.Checked, "Managed radio should be checked when UseNativeRibbon is false");
                    Assert.IsFalse(native.Checked);

                    native.Checked = true;
                    panel.Save();
                    Assert.IsTrue(PostEditorSettings.UseNativeRibbon, "Save must persist the native ribbon selection");
                }

                using (var panel = new LabsPreferencesPanel())
                {
                    var radios = GetAll<RadioButton>(panel).ToList();
                    var native = radios.First(r => r.Text == Res.Get(StringId.LabsRibbonNative));
                    Assert.IsTrue(native.Checked, "A fresh panel must reflect the persisted selection");
                }
            }
            finally
            {
                PostEditorSettings.UseNativeRibbon = original;
            }
        }

        [Test]
        public void LabsPanel_IsRegisteredInPreferencesHandler()
        {
            // PreferencesHandler.LoadPreferencesPanels is private; drive it via reflection
            // and confirm the Labs panel is listed, otherwise the tab never appears.
            var handlerType = typeof(PreferencesHandler);
            var loadMethod = handlerType.GetMethod("LoadPreferencesPanels",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            loadMethod.Invoke(null, null);

            var tableField = handlerType.GetField("preferencesPanelTypeTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var table = (System.Collections.Hashtable)tableField.GetValue(null);
            Assert.AreEqual(typeof(LabsPreferencesPanel), table["labs"]);
        }
    }
}
