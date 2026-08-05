// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Tests.CoreServices
{
    /// <summary>
    /// version.txt is the single source of truth for the product version.
    /// writer.build.targets used to generate GlobalAssemblyVersionInfo.cs from
    /// it, but nothing imports that file under the SDK, so every assembly built
    /// as 0.0.0.0 and the About box and crash reports said so. Directory.Build
    /// .props now stamps the version directly; guard that it stays stamped.
    /// </summary>
    [TestFixture]
    public class AssemblyVersionTests
    {
        private static string RepoVersion()
        {
            // Walk up from the test binaries to the repo root.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "version.txt")))
                dir = dir.Parent;

            if (dir == null)
                Assert.Ignore("version.txt not found above the test output directory");

            return File.ReadAllText(Path.Combine(dir.FullName, "version.txt")).Trim();
        }

        [Test]
        public void CoreServicesAssembly_IsStampedFromVersionTxt()
        {
            var expected = RepoVersion();
            var assembly = typeof(ApplicationEnvironment).Assembly;

            Assert.That(assembly.GetName().Version.ToString(), Is.EqualTo(expected),
                "AssemblyVersion must come from version.txt");

            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            Assert.NotNull(fileVersion, "AssemblyFileVersion must be stamped");
            Assert.That(fileVersion.Version, Is.EqualTo(expected));
        }

        [Test]
        public void ProductVersionResource_IsNotZero()
        {
            // ApplicationEnvironment reads FileVersionInfo.Product*Part off the
            // entry assembly, which is where the 0.0.0.0 was surfacing.
            var path = typeof(ApplicationEnvironment).Assembly.Location;
            var info = FileVersionInfo.GetVersionInfo(path);

            Assert.That(
                info.ProductMajorPart + info.ProductMinorPart + info.ProductBuildPart + info.ProductPrivatePart,
                Is.GreaterThan(0),
                "the PE product version resource must not be 0.0.0.0");

            var expected = new Version(RepoVersion());
            Assert.That(info.ProductMajorPart, Is.EqualTo(expected.Major));
            Assert.That(info.ProductMinorPart, Is.EqualTo(expected.Minor));
        }
    }
}
