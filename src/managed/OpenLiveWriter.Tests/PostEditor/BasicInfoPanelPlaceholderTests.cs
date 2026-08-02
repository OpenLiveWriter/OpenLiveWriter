// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Threading;
using NUnit.Framework;
using OpenLiveWriter.PostEditor.Configuration.Wizard;

namespace OpenLiveWriter.Tests.PostEditor
{
    /// <summary>
    /// Covers the "https://" placeholder shown in the blog registration
    /// (homepage URL) field during first setup and new blog setup, and the
    /// validation rule that a bare scheme still counts as no URL entered.
    /// </summary>
    [TestFixture]
    public class BasicInfoPanelPlaceholderTests
    {
        [Test]
        [Apartment(ApartmentState.STA)]
        public void Constructor_PrefillsHttpsPlaceholder()
        {
            using (var panel = new WeblogConfigurationWizardPanelBasicInfo())
            {
                Assert.AreEqual(WeblogConfigurationWizardPanelBasicInfo.DefaultUrlPlaceholder,
                    panel.HomepageUrl);
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Setter_EmptyValue_ShowsPlaceholder_RealUrlPreserved()
        {
            using (var panel = new WeblogConfigurationWizardPanelBasicInfo())
            {
                panel.HomepageUrl = "";
                Assert.AreEqual(WeblogConfigurationWizardPanelBasicInfo.DefaultUrlPlaceholder,
                    panel.HomepageUrl);

                panel.HomepageUrl = "   ";
                Assert.AreEqual(WeblogConfigurationWizardPanelBasicInfo.DefaultUrlPlaceholder,
                    panel.HomepageUrl);

                panel.HomepageUrl = "https://example.com/blog";
                Assert.AreEqual("https://example.com/blog", panel.HomepageUrl);
            }
        }

        [Test]
        public void IsEmptyHomepageUrl_BareSchemeCountsAsEmpty()
        {
            Assert.IsTrue(WeblogConfigurationWizardPanelBasicInfo.IsEmptyHomepageUrl(null));
            Assert.IsTrue(WeblogConfigurationWizardPanelBasicInfo.IsEmptyHomepageUrl(""));
            Assert.IsTrue(WeblogConfigurationWizardPanelBasicInfo.IsEmptyHomepageUrl("http://"));
            Assert.IsTrue(WeblogConfigurationWizardPanelBasicInfo.IsEmptyHomepageUrl("https://"));
            Assert.IsFalse(WeblogConfigurationWizardPanelBasicInfo.IsEmptyHomepageUrl("https://example.com"));
            Assert.IsFalse(WeblogConfigurationWizardPanelBasicInfo.IsEmptyHomepageUrl("http://example.com"));
        }
    }
}
