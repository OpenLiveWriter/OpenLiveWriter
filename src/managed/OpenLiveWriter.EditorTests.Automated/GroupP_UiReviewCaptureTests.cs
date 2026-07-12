// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.IO;
using System.Linq;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Visual review capture harness. Writes PNG screenshots + layout dumps under
    /// <c>artifacts/ui-review/</c>. Marked Explicit so the default <c>dotnet test</c>
    /// run stays fast; invoke via Category=UiReview (see <c>scripts/ui-review.sh</c>).
    /// Always-on golden layout invariants live in <see cref="GroupP_RibbonChromeTests"/>.
    /// </summary>
    [TestFixture]
    [Category("UiReview")]
    [Category("GroupP")]
    [Explicit("Writes PNG/layout artifacts; run via scripts/ui-review.sh or --filter Category=UiReview")]
    public class GroupP_UiReviewCaptureTests
    {
        [SetUp]
        public void SetUp()
        {
            WebViewEditor.UseLayoutPlaceholder = true;
        }

        [TearDown]
        public void TearDown()
        {
            WebViewEditor.UseLayoutPlaceholder = false;
        }

        [AvaloniaTest]
        public void Capture_MainWindow_AllReviewSizes_WritesPngsAndLayoutDumps()
        {
            string outDir = UiReviewHarness.ResolveOutputDirectory();
            var results = UiReviewHarness.CaptureDefaultSizes(outDir);

            Assert.That(results, Has.Count.EqualTo(UiReviewHarness.DefaultSizes.Length));
            foreach (var result in results)
            {
                Assert.That(result.Files.Any(f => f.EndsWith($".png") && f.Contains($"main-{result.Tag}")),
                    Is.True,
                    $"Expected main-{result.Tag}.png under {outDir}. Flags: {string.Join("; ", result.Flags)}");
                Assert.That(File.Exists(Path.Combine(outDir, $"layout-{result.Tag}.json")), Is.True);
                Assert.That(File.Exists(Path.Combine(outDir, $"layout-{result.Tag}.md")), Is.True);

                // Soft-fail on layout flags so screenshots still land; print for the agent.
                if (result.Flags.Count > 0)
                    TestContext.WriteLine($"[{result.Tag}] flags: {string.Join("; ", result.Flags)}");
            }

            Assert.That(File.Exists(Path.Combine(outDir, "INDEX.md")), Is.True);
            TestContext.WriteLine($"UI review artifacts written to: {outDir}");
            foreach (var f in Directory.EnumerateFiles(outDir).OrderBy(f => f))
                TestContext.WriteLine("  " + f);
        }

        [AvaloniaTest]
        [TestCase(800, 600, "800x600")]
        [TestCase(1280, 800, "1280x800")]
        public void Capture_SingleSize_WritesArtifacts(double width, double height, string tag)
        {
            string outDir = UiReviewHarness.ResolveOutputDirectory();
            var result = UiReviewHarness.CaptureAtSize(width, height, tag, outDir);
            Assert.That(result.Files, Is.Not.Empty);
            Assert.That(File.Exists(Path.Combine(outDir, $"main-{tag}.png")), Is.True);
        }
    }
}
