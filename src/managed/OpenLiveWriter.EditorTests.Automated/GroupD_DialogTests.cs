// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group D — dialogs / lifecycle. D1 (LinkDialog validation) runs headlessly:
    /// the Insert button must stay disabled for an empty or "https://"-only URL and
    /// enable once a real address is typed. Image-insert / account-setup / draft /
    /// word-count are [Explicit] TDD targets (features not yet built).
    /// </summary>
    [TestFixture]
    [Category("GroupD")]
    public class GroupD_DialogTests
    {
        // --- D1: LinkDialog validation logic (pure) ---

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("   ", false)]
        [TestCase("https://", false)]
        [TestCase("http://", false)]
        [TestCase("HTTPS://", false)]
        [TestCase("https://example.com", true)]
        [TestCase("http://a.co", true)]
        [TestCase("mailto:x@y.com", true)]
        [TestCase("/relative/path", true)]
        public void IsValidUrl_MatchesInsertEnableRule(string url, bool expected)
        {
            Assert.That(LinkDialog.IsValidUrl(url), Is.EqualTo(expected));
        }

        // --- D1: LinkDialog Insert button reflects the URL field (headless UI) ---

        [AvaloniaTest]
        public void LinkDialog_InsertButton_DisabledUntilRealUrlTyped()
        {
            var dialog = new LinkDialog();
            var urlBox = FindControl<TextBox>(dialog);           // first TextBox = URL
            var insert = FindButton(dialog, "Insert");

            Assert.That(insert, Is.Not.Null);
            // Default text is "https://" — Insert must be disabled.
            Assert.That(insert.IsEnabled, Is.False, "Insert should be disabled for https:// placeholder");

            urlBox.Text = "https://openlivewriter.org";
            Assert.That(insert.IsEnabled, Is.True, "Insert should enable for a real URL");

            urlBox.Text = "https://";
            Assert.That(insert.IsEnabled, Is.False, "Insert should disable again for https:// only");
        }

        private static T FindControl<T>(Control root) where T : Control =>
            root.GetLogicalDescendants().OfType<T>().FirstOrDefault();

        private static Button FindButton(Control root, string content) =>
            root.GetLogicalDescendants().OfType<Button>()
                .FirstOrDefault(b => (b.Content as string) == content);

        // ---------------------------------------------------------------------
        // TDD targets — features not implemented on the Mac side yet.
        // Run with:  dotnet test --filter "Category=GroupD & Explicit"
        // ---------------------------------------------------------------------

        [Test]
        [Explicit("Image insert from file not implemented (P1-7)")]
        public void ImageInsertDialog_InsertsImgTag()
            => Assert.Fail("Image insert (InsertPictureFromFile) not implemented on macOS.");

        [Test]
        [Explicit("Account setup / blog config UI not implemented (P2-9)")]
        public void AccountSetup_StoresCredentials()
            => Assert.Fail("Account setup UI + MacCredentialStorage wiring not implemented.");

        // D4 (draft save/open) is now implemented — full lifecycle coverage lives in
        // GroupD_DraftLifecycleTests (round-trip, overwrite, MRU, delete, corrupt/missing).

        [Test]
        [Explicit("Word count not implemented (P1-8)")]
        public void WordCount_CountsWords()
            => Assert.Fail("Word count feature not implemented on macOS.");
    }
}
