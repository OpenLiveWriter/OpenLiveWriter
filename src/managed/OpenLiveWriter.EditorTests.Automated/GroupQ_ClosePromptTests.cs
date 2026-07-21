// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using NUnit.Framework;
using OpenLiveWriter.App.Avalonia;
using OpenLiveWriter.App.Avalonia.Dialogs;
using OpenLiveWriter.App.Avalonia.Editor;
using OpenLiveWriter.Publishing.Drafts;

namespace OpenLiveWriter.EditorTests.Automated
{
    /// <summary>
    /// Group Q — P0 trust breakers, unsaved-changes prompt on window close (P0-1).
    /// The prompt logic is exercised headlessly via the null-owner safe path (the
    /// same pattern New/Open use): a dirty draft must cancel the close, never
    /// discard silently. The live Closing event is covered by cancelling a real
    /// headless close and asserting the window stays open.
    /// </summary>
    [TestFixture]
    [Category("GroupQ")]
    public class GroupQ_ClosePromptTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            WebViewEditor.UseLayoutPlaceholder = true;
            _dir = Path.Combine(Path.GetTempPath(), "OLWClosePromptTests", Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            WebViewEditor.UseLayoutPlaceholder = false;
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
            catch { /* best effort */ }
        }

        private DraftSession NewSession() => new DraftSession(new FileDraftStore(_dir));

        [AvaloniaTest]
        public async Task Prompt_DirtyDraft_NullOwner_Cancels()
        {
            var window = new MainWindow();
            window.DraftSession = NewSession();
            window.DraftSession.UpdateBody("<p>unsaved work</p>");

            ConfirmResult result = await window.PromptUnsavedChangesForCloseAsync(null);

            Assert.That(result, Is.EqualTo(ConfirmResult.Cancel),
                "headless/null-owner must resolve to the safe cancel path");
            Assert.That(window.DraftSession.IsDirty, Is.True,
                "nothing may be saved or discarded without the user's choice");
        }

        [AvaloniaTest]
        public async Task Prompt_CleanDraft_ProceedsWithoutPrompting()
        {
            var window = new MainWindow();
            window.DraftSession = NewSession();

            ConfirmResult result = await window.PromptUnsavedChangesForCloseAsync(null);

            Assert.That(result, Is.EqualTo(ConfirmResult.Discard),
                "a clean document has nothing to lose — close may proceed");
        }

        [AvaloniaTest]
        public void Close_DirtyDraft_CancelsCloseAndShowsPrompt()
        {
            var window = new MainWindow();
            try
            {
                window.Show();
                window.DraftSession = NewSession();
                window.DraftSession.UpdateBody("<p>unsaved work</p>");

                window.Close();

                Assert.That(window.IsVisible, Is.True,
                    "closing a dirty document must be cancelled, not discard the work");

                // The unsaved-changes prompt should be up as an owned dialog.
                var prompt = window.OwnedWindows.OfType<ConfirmDialog>().FirstOrDefault();
                Assert.That(prompt, Is.Not.Null, "the close must surface the unsaved-changes prompt");

                // Answering Cancel leaves the window open.
                prompt.Close(ConfirmResult.Cancel);
                Dispatcher.UIThread.RunJobs();
                Assert.That(window.IsVisible, Is.True);

                // With the document saved (clean), a close proceeds.
                window.DraftSession.Save();
                Dispatcher.UIThread.RunJobs();
                window.Close();
                Dispatcher.UIThread.RunJobs();
                Assert.That(window.IsVisible, Is.False, "a clean document closes without prompting");
            }
            finally
            {
                if (window.IsVisible)
                    window.Close();
            }
        }
    }
}
