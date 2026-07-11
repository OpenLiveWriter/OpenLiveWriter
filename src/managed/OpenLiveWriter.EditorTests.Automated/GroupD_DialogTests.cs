// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.EditorTests.Automated.Infrastructure;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Accounts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group D — dialogs / lifecycle. D1 (LinkDialog validation) runs headlessly:
    /// the Insert button must stay disabled for an empty or "https://"-only URL and
    /// enable once a real address is typed. Draft save/open (D4) is implemented and
    /// covered by GroupD_DraftLifecycleTests. Image-insert / account-setup /
    /// word-count remain [Explicit] TDD targets (features not yet built).
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

        // --- D2: Image insert from file (P1-7) — pure build logic, headless ---
        // The file picker + WebView insertion are exercised live; here we verify the
        // <img> HTML built from a known file is well-formed with an inline data URI.

        [Test]
        public void ImageInsert_BuildsWellFormedImgWithDataUri()
        {
            // Minimal 1x1 transparent PNG.
            byte[] png = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
            string tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            File.WriteAllBytes(tmp, png);
            try
            {
                string html = WebViewEditor.BuildImageHtmlFromFile(tmp, "My Alt");
                var img = Dom.Parse(html).QuerySelector("img");

                Assert.That(img, Is.Not.Null, "expected an <img> element");
                Assert.That(img.GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
                Assert.That(img.GetAttribute("alt"), Is.EqualTo("My Alt"));
            }
            finally
            {
                File.Delete(tmp);
            }
        }

        [Test]
        public void ImageInsert_DefaultsAltToFileName_AndGuessesMime()
        {
            byte[] jpg = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
            string tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".jpg");
            File.WriteAllBytes(tmp, jpg);
            try
            {
                string name = Path.GetFileNameWithoutExtension(tmp);
                string html = WebViewEditor.BuildImageHtmlFromFile(tmp);
                var img = Dom.Parse(html).QuerySelector("img");

                Assert.That(img.GetAttribute("src"), Does.StartWith("data:image/jpeg;base64,"));
                Assert.That(img.GetAttribute("alt"), Is.EqualTo(name));
            }
            finally
            {
                File.Delete(tmp);
            }
        }

        [Test]
        public void ImageInsert_EscapesAltText()
        {
            string html = WebViewEditor.BuildImageHtml("https://x/y.png", "Tom & \"Jerry\" <x>");
            var img = Dom.Parse(html).QuerySelector("img");

            Assert.That(img, Is.Not.Null);
            Assert.That(img.GetAttribute("src"), Is.EqualTo("https://x/y.png"));
            Assert.That(img.GetAttribute("alt"), Is.EqualTo("Tom & \"Jerry\" <x>"));
        }

        [TestCase(".png", "image/png")]
        [TestCase(".jpg", "image/jpeg")]
        [TestCase(".jpeg", "image/jpeg")]
        [TestCase(".gif", "image/gif")]
        [TestCase(".webp", "image/webp")]
        [TestCase(".unknown", "image/png")]
        public void ImageInsert_GuessMimeType(string ext, string expected)
        {
            Assert.That(WebViewEditor.GuessImageMimeType("photo" + ext), Is.EqualTo(expected));
        }

        // --- D3: Account setup stores credentials (P2-9) — implemented ---
        // The account setup dialog captures blog metadata + a password. On save the
        // metadata is persisted to the account store while the PASSWORD is stored in the
        // separate credential store (macOS Keychain in the app; an in-memory fake here so
        // no real `security` CLI runs). This verifies that separation end-to-end.

        [Test]
        public void AccountSetup_StoresCredentials()
        {
            string dir = Path.Combine(Path.GetTempPath(), "OLWAccountD3", Guid.NewGuid().ToString("N"));
            try
            {
                var store = new FileAccountStore(dir);
                var creds = new InMemoryCredentialStore();
                var service = new BlogAccountService(store, creds);

                // The result the AccountDialog produces when the user clicks Save.
                var result = new AccountDialogResult
                {
                    Account = new BlogAccount
                    {
                        DisplayName = "My Blog",
                        ApiEndpointUrl = "https://blog.example.com/xmlrpc.php",
                        BlogId = "blog-1",
                        Username = "author"
                    },
                    Password = "s3cret"
                };

                BlogAccount saved = service.SaveAccount(result.Account, result.Password);

                // Password lands in the credential store, NOT the account JSON.
                Assert.That(creds.Retrieve(saved.Id)?.Password, Is.EqualTo("s3cret"));
                string json = File.ReadAllText(Path.Combine(dir, saved.Id + ".olaccount.json"));
                Assert.That(json, Does.Not.Contain("s3cret"), "secret must not be written to account metadata");

                // Metadata round-trips from the store.
                BlogAccount loaded = store.Load(saved.Id);
                Assert.That(loaded.Username, Is.EqualTo("author"));
                Assert.That(loaded.ApiEndpointUrl, Is.EqualTo("https://blog.example.com/xmlrpc.php"));
            }
            finally
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best effort */ }
            }
        }

        [TestCase("https://x/xmlrpc.php", "user", "pw", false, true)]   // new: all present
        [TestCase("https://x/xmlrpc.php", "user", "", false, false)]    // new: no password
        [TestCase("https://x/xmlrpc.php", "", "pw", false, false)]      // new: no username
        [TestCase("https://", "user", "pw", false, false)]             // trivial endpoint
        [TestCase("", "user", "pw", false, false)]                     // no endpoint
        [TestCase("https://x/xmlrpc.php", "user", "", true, true)]      // edit: blank pw ok
        public void AccountDialog_SaveEnableRule(string endpoint, string user, string pw, bool isEdit, bool expected)
        {
            Assert.That(AccountDialog.CanSave(endpoint, user, pw, isEdit), Is.EqualTo(expected));
        }

        [AvaloniaTest]
        public void AccountDialog_SaveButton_DisabledWithEmptyFields()
        {
            var dialog = new AccountDialog();
            var save = FindButton(dialog, "Save");
            Assert.That(save, Is.Not.Null);
            Assert.That(save.IsEnabled, Is.False, "Save should be disabled until required fields are entered");
        }

        // D4 (draft save/open) is now implemented — full lifecycle coverage lives in
        // GroupD_DraftLifecycleTests (round-trip, overwrite, MRU, delete, corrupt/missing).

        // --- D5: Word count (P1-8) — implemented; see GroupD_WordCountTests for
        // full coverage. Sanity check the counter here on a simple document.

        [Test]
        public void WordCount_CountsWords()
        {
            var counter = new WordCounter("<p>The quick brown fox</p>");
            Assert.That(counter.Words, Is.EqualTo(4));
        }
    }
}
