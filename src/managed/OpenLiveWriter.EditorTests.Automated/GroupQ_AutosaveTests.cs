// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.App.Avalonia.Settings;
using OpenLiveWriter.Publishing;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group Q — P0 trust breakers, draft autosave (P0-2). Drives
    /// <see cref="AutosaveController"/> directly (injectable tick — no timers)
    /// against a per-test temp-directory <see cref="FileDraftStore"/>, covering
    /// dirty/clean documents, the enable/disable preference, and the interval
    /// fallback when AutoSaveMinutes is unset.
    /// </summary>
    [TestFixture]
    [Category("GroupQ")]
    public class GroupQ_AutosaveTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "OLWAutosaveTests", Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
            catch { /* best effort */ }
        }

        private DraftSession NewSession() => new DraftSession(new FileDraftStore(_dir));

        private static AppPreferences Prefs(bool enabled = true, int minutes = 5) =>
            new AppPreferences { AutoSaveDrafts = enabled, AutoSaveMinutes = minutes };

        private static Task<(string Title, ContentFormat BodyFormat, string BodyHtml, string BodyMarkdown)> CaptureHtml(
            string title, string html) =>
            Task.FromResult((title, ContentFormat.Html, html, (string)null));

        [Test]
        public async Task Tick_DirtyAndEnabled_SavesDraftAndClearsDirty()
        {
            DraftSession session = NewSession();
            session.UpdateBody("<p>draft body</p>");
            Assert.That(session.IsDirty, Is.True);

            var prefs = Prefs();
            int autosavedCount = 0;
            var controller = new AutosaveController(
                session, () => prefs, () => CaptureHtml("My Title", "<p>draft body</p>"));
            controller.Autosaved += (s, e) => autosavedCount++;

            bool saved = await controller.TickAsync();

            Assert.That(saved, Is.True);
            Assert.That(autosavedCount, Is.EqualTo(1));
            Assert.That(session.IsDirty, Is.False, "autosave must clear the dirty flag");
            Assert.That(session.ListDrafts().Count, Is.EqualTo(1));
            Assert.That(session.Current.Title, Is.EqualTo("My Title"));
        }

        [Test]
        public async Task Tick_CleanDocument_DoesNothing()
        {
            DraftSession session = NewSession();
            var prefs = Prefs();
            var controller = new AutosaveController(
                session, () => prefs, () => CaptureHtml("T", "<p>x</p>"));

            bool saved = await controller.TickAsync();

            Assert.That(saved, Is.False);
            Assert.That(session.ListDrafts(), Is.Empty);
        }

        [Test]
        public async Task Tick_DisabledPreference_LeavesDirtyDocumentAlone()
        {
            DraftSession session = NewSession();
            session.UpdateBody("<p>unsaved</p>");
            var prefs = Prefs(enabled: false);
            var controller = new AutosaveController(
                session, () => prefs, () => CaptureHtml("T", "<p>unsaved</p>"));

            bool saved = await controller.TickAsync();

            Assert.That(saved, Is.False);
            Assert.That(session.IsDirty, Is.True, "disabled autosave must not touch the document");
            Assert.That(session.ListDrafts(), Is.Empty);
        }

        [Test]
        public void Interval_UsesPreferenceMinutes()
        {
            var prefs = Prefs(minutes: 7);
            var controller = new AutosaveController(
                NewSession(), () => prefs, () => CaptureHtml("T", null));
            Assert.That(controller.Interval, Is.EqualTo(TimeSpan.FromMinutes(7)));
        }

        [Test]
        public void Interval_FallsBackToDefaultWhenUnset()
        {
            var prefs = Prefs(minutes: 0);
            var controller = new AutosaveController(
                NewSession(), () => prefs, () => CaptureHtml("T", null));
            Assert.That(controller.Interval,
                Is.EqualTo(TimeSpan.FromMinutes(AutosaveController.DefaultIntervalMinutes)));
        }

        [Test]
        public async Task Tick_NullCapturedBody_KeepsExistingBody()
        {
            // When the editor is not ready the shell captures a null body; autosave
            // must persist the session's last-known body rather than wiping it.
            DraftSession session = NewSession();
            session.UpdateBody("<p>last known</p>");
            var prefs = Prefs();
            var controller = new AutosaveController(
                session, () => prefs, () => CaptureHtml("T", null));

            bool saved = await controller.TickAsync();

            Assert.That(saved, Is.True);
            Assert.That(session.Current.BodyHtml, Is.EqualTo("<p>last known</p>"));
        }
    }
}
