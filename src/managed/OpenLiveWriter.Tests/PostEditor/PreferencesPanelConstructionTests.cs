// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.Tests.PostEditor
{
    /// <summary>
    /// The Options dialog (PreferencesHandler.ShowPreferences) constructs every
    /// registered preferences panel up front, so a single panel that throws or
    /// asserts in its constructor crashes the whole dialog. Construct them all
    /// the same way here to keep that path covered.
    /// </summary>
    [TestFixture]
    public class PreferencesPanelConstructionTests
    {
        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            // Several panels read settings via ApplicationEnvironment, which the
            // test host does not initialize by default. Use a non-default product
            // name: with the default product name Initialize() throws when the
            // profile has no Personal folder (e.g. the SYSTEM account in a
            // headless test session).
            if (OpenLiveWriter.CoreServices.ApplicationEnvironment.InstallationDirectory == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                OpenLiveWriter.CoreServices.ApplicationEnvironment.Initialize(assembly,
                    System.IO.Path.GetDirectoryName(assembly.Location),
                    "Software\\OpenLiveWriter.Tests", "Open Live Writer Tests");
            }

            // Panels that enumerate content sources (e.g. Live Clipboard) need the
            // content source manager initialized, which the app does at startup.
            if (OpenLiveWriter.PostEditor.ContentSources.ContentSourceManager.ActiveContentSources == null)
                OpenLiveWriter.PostEditor.ContentSources.ContentSourceManager.Initialize(false);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void AllPreferencesPanels_ConstructWithoutThrowing()
        {
            var handlerType = typeof(PreferencesHandler);
            var loadMethod = handlerType.GetMethod("LoadPreferencesPanels",
                BindingFlags.NonPublic | BindingFlags.Static);
            loadMethod.Invoke(null, null);

            var typesField = handlerType.GetField("preferencesPanelTypes",
                BindingFlags.NonPublic | BindingFlags.Static);
            var types = (Type[])typesField.GetValue(null);
            Assert.IsNotEmpty(types, "no preferences panels registered");

            foreach (var type in types)
            {
                Assert.DoesNotThrow(() =>
                {
                    var panel = (OpenLiveWriter.ApplicationFramework.Preferences.PreferencesPanel)
                        Activator.CreateInstance(type);
                    panel.Dispose();
                }, $"preferences panel {type.Name} threw in its constructor");
            }
        }
    }
}
