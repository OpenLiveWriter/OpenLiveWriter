// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.Configuration.Wizard;

namespace OpenLiveWriter.Tests.PostEditor
{
    /// <summary>
    /// Covers the account-setup wizard crash where
    /// WeblogConfigurationWizardPanelProgress.NaturalizeLayout threw a
    /// NullReferenceException because the PublishAnimation images were not
    /// embedded in the PostEditor assembly.
    /// </summary>
    [TestFixture]
    public class WizardProgressPanelTests
    {
        [Test]
        public void PublishAnimationImages_AreEmbedded()
        {
            // The wizard animation loads Images.PublishAnimation.post01..post26
            // via ResourceHelper.LoadAssemblyResourceBitmap; all of them must be
            // embedded (the csproj glob must match subdirectories of Images).
            var assembly = typeof(WeblogConfigurationWizardPanelProgress).Assembly;
            for (var i = 1; i <= 26; i++)
            {
                var name = string.Format("Images.PublishAnimation.post{0:00}.png", i);
                var bitmap = ResourceHelper.LoadAssemblyResourceBitmap(assembly, name);
                Assert.NotNull(bitmap, $"Resource {name} is not embedded");
                bitmap.Dispose();
            }
        }

        [Test]
        public void NaturalizeLayout_DoesNotThrowWhenBitmapsMissing()
        {
            // A fresh panel has no animation bitmaps set; NaturalizeLayout must
            // not throw (it previously dereferenced Bitmaps[0] blindly).
            using (var panel = new WeblogConfigurationWizardPanelProgress())
            {
                Assert.DoesNotThrow(() => panel.NaturalizeLayout());
            }
        }
    }
}
