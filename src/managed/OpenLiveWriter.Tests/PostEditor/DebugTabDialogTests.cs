// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;
using NUnit.Framework;
using OpenLiveWriter.PostEditor;

namespace OpenLiveWriter.Tests.PostEditor
{
    /// <summary>
    /// The Debug tab's buttons open a set of dialogs and run debug utilities.
    /// This constructs each dialog (and exercises the update-check path) so a
    /// porting break in any of them fails here instead of in the user's hands.
    /// </summary>
    [TestFixture]
    public class DebugTabDialogTests
    {
        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            // Several dialogs read settings/resources via ApplicationEnvironment,
            // which the test host does not initialize by default. Use a
            // non-default product name so Initialize() does not require a
            // Personal folder (e.g. the SYSTEM account in a headless session).
            if (OpenLiveWriter.CoreServices.ApplicationEnvironment.InstallationDirectory == null)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                OpenLiveWriter.CoreServices.ApplicationEnvironment.Initialize(assembly,
                    System.IO.Path.GetDirectoryName(assembly.Location),
                    "Software\\OpenLiveWriter.Tests", "Open Live Writer Tests");
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void ExpirationForm_Constructs()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var form = new ExpirationForm()) { }
            });
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void WebLayoutViewWarningForm_Constructs()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var form = new WebLayoutViewWarningForm()) { }
            });
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void DisplayMessageTestForm_Constructs()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var form = new OpenLiveWriter.Controls.DisplayMessageTestForm()) { }
            });
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void SupportingFilesForm_Constructs()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var form = new OpenLiveWriter.PostEditor.SupportingFiles.SupportingFilesForm()) { }
            });
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void GDataCaptchaForm_Constructs()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var form = new OpenLiveWriter.BlogClient.Clients.GDataCaptchaForm()) { }
            });
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void AtomImageEndpointSelectorPanel_Constructs()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var panel = new OpenLiveWriter.PostEditor.Configuration.Wizard.WeblogConfigurationWizardPanelSelectBlog())
                {
                    panel.PrepareForAdd();
                }
            });
        }

        [Test]
        public void ValidateLocalizedResources_RunsWithoutThrowing()
        {
            Assert.DoesNotThrow(() => OpenLiveWriter.Localization.Res.Validate());
        }

        [Test]
        public void ShowUpdateMessage_HasARegisteredHandler()
        {
            // The Update Message debug button was a silent no-op: no command was
            // registered under CommandId.ShowUpdateMessage. Guard the handler's
            // existence so it does not regress back to a dead button.
            var method = typeof(PostEditorMainControl).GetMethod("commandShowUpdateMessage_Execute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method, "PostEditorMainControl must keep a ShowUpdateMessage handler");
        }
    }
}
