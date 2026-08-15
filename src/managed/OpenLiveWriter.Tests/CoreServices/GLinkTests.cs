// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using NUnit.Framework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Tests.CoreServices
{
    /// <summary>
    /// The openlivewriter.com/WriterRedirect/* indirection is mostly dead: nine
    /// of its twelve endpoints 404 outright, and the three that survive serve a
    /// broken Jekyll redirect page (the template emits a raw Ruby hash instead
    /// of the target URL), so the browser lands on a 404. Links the app ships
    /// should point at pages that actually resolve.
    /// </summary>
    [TestFixture]
    public class GLinkTests
    {
        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            if (ApplicationEnvironment.InstallationDirectory == null)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                ApplicationEnvironment.Initialize(assembly,
                    System.IO.Path.GetDirectoryName(assembly.Location),
                    "Software\\OpenLiveWriter.Tests", "Open Live Writer Tests");
            }
        }

        [Test]
        public void DownloadPlugins_PointsAtTheLivePluginsPage()
        {
            var url = GLink.Instance.DownloadPlugins;

            Assert.That(url, Does.StartWith("https://openlivewriter.com/plugins/"),
                "Add Plugin must open the plugins page");
            Assert.That(url, Does.Not.Contain("WriterRedirect"),
                "the WriterRedirect indirection is broken and lands on a 404");
        }
    }
}
