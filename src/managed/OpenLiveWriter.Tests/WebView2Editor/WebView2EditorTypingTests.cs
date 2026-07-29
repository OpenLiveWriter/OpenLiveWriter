// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using NUnit.Framework;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.WebView2Shim;
using StringAssert = NUnit.Framework.Legacy.StringAssert;

namespace OpenLiveWriter.Tests.WebView2Editor
{
    /// <summary>
    /// Live integration tests for the WebView2-based WYSIWYG editor. Content typed
    /// or inserted into the WebView2 document must flow back to C# through the
    /// EditorContentBridge (JS input listeners sync Title/Body on every edit).
    /// Requires a real desktop session and the WebView2 runtime; the tests
    /// Assert.Ignore when the environment cannot be created (e.g. session 0).
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [Category("WebView2")]
    public class WebView2EditorTypingTests
    {
        private const int ReadyTimeoutMs = 30000;
        private const int SyncTimeoutMs = 10000;

        [OneTimeSetUp]
        public void ConfigureWebView2UserDataFolder()
        {
            // The WebView2 default user-data folder is derived from the host exe,
            // which fails under testhost (controller creation returns
            // CO_E_SERVER_EXEC_FAILURE). Point the loader at a writable temp
            // folder before any environment is created.
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER")))
            {
                Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenLiveWriter.Tests.WebView2"));
            }
        }

        [Test]
        public void InsertHtml_UpdatesEditedContentAndMarksDirty()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);

                // Focus the body so execCommand('insertHTML') has a live selection.
                editor.FocusBody();
                editor.InsertHtml("<p>Hello WebView2 world</p>", HtmlInsertionOptions.MoveCursorAfter);

                string body = WaitForValue(() => editor.GetEditedHtml(true), "Hello WebView2 world");
                StringAssert.Contains("Hello WebView2 world", body);
                Assert.IsTrue(editor.IsDirty, "editor should be dirty after inserting content");
            }
        }

        [Test]
        public void SelectionChange_SyncsSelectedHtml()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);

                WebView2 webView = GetInnerWebView(editor);
                webView.CoreWebView2.ExecuteScriptAsync(
                    "var b = document.getElementById('olw-body');" +
                    "b.innerHTML = '<p>Hello <b>bold</b> world</p>';" +
                    "var range = document.createRange();" +
                    "range.selectNodeContents(b);" +
                    "var sel = window.getSelection();" +
                    "sel.removeAllRanges();" +
                    "sel.addRange(range);" +
                    "document.dispatchEvent(new Event('selectionchange'));");

                string html = WaitForValue(() => editor.SelectedHtml, "<b>bold</b>");
                StringAssert.Contains("<b>bold</b>", html);
            }
        }

        [Test]
        public void TitleEdit_SyncsToGetEditedTitleHtml()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);

                // The title element is only reachable via JS; the editor does not
                // expose a script entry point publicly, so use the private WebView2.
                WebView2 webView = GetInnerWebView(editor);
                webView.CoreWebView2.ExecuteScriptAsync(
                    "var t = document.getElementById('olw-title');" +
                    "t.innerHTML = 'My Test Title';" +
                    "t.dispatchEvent(new Event('input', { bubbles: true }));");

                string title = WaitForValue(() => editor.GetEditedTitleHtml(), "My Test Title");
                StringAssert.Contains("My Test Title", title);
            }
        }

        [Test]
        public void TitleEdit_StripsMarkupFromSyncedTitle()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);

                // The title is plain text (0.6.2 parity): markup injected into
                // the title element must not reach the synced Title value.
                WebView2 webView = GetInnerWebView(editor);
                webView.CoreWebView2.ExecuteScriptAsync(
                    "var t = document.getElementById('olw-title');" +
                    "t.innerHTML = '<b>Bold</b> Plain';" +
                    "t.dispatchEvent(new Event('input', { bubbles: true }));");

                string title = WaitForValue(() => editor.GetEditedTitleHtml(), "Bold Plain");
                Assert.AreEqual("Bold Plain", title);
            }
        }

        [Test]
        public void TitleFormattingCommand_DoesNotApplyInTitle()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);

                // Selecting title text and running a formatting command (as the
                // ribbon's Bold button does) must leave the plain-text title
                // untouched; the resulting markup crashed AutoSave when the
                // title HTML was used as the AutoRecover file name.
                WebView2 webView = GetInnerWebView(editor);
                string titleHtml = null;
                webView.CoreWebView2.ExecuteScriptAsync(
                    "var t = document.getElementById('olw-title');" +
                    "t.textContent = 'My Title';" +
                    "var range = document.createRange();" +
                    "range.selectNodeContents(t);" +
                    "var sel = window.getSelection();" +
                    "sel.removeAllRanges();" +
                    "sel.addRange(range);" +
                    "window.olwExecFormatting('bold');" +
                    "t.innerHTML;")
                    .ContinueWith(t => titleHtml = t.Result);

                titleHtml = WaitForValue(() => titleHtml, "My Title");
                Assert.IsFalse(titleHtml.Contains("<b>"), "title must stay plain text, got: " + titleHtml);
            }
        }

        private static Form CreateEditorForm(out WebView2HtmlEditorControl editor)
        {
            Form form;
            WebView2HtmlEditorControl control;
            try
            {
                form = new Form();
                control = new WebView2HtmlEditorControl { Dock = DockStyle.Fill };
                form.Controls.Add(control);
                form.Show();
            }
            catch (Exception ex)
            {
                Assert.Ignore("Could not create editor host form in this session: " + ex.Message);
                throw; // unreachable; Assert.Ignore throws
            }
            editor = control;
            return form;
        }

        private static void EnsureReadyOrIgnore(WebView2HtmlEditorControl editor)
        {
            if (!WaitForReady(editor))
            {
                string detail = editor.InitializationError != null
                    ? " Initialization error: " + editor.InitializationError
                    : " No initialization error was captured; ReadyForEditing never fired.";
                Assert.Ignore("WebView2 editor did not become ready (runtime or desktop unavailable in this session)." + detail);
            }
        }

        private static bool WaitForReady(WebView2HtmlEditorControl editor)
        {
            bool ready = false;
            editor.ReadyForEditing += (s, e) => ready = true;
            var sw = Stopwatch.StartNew();
            while (!ready && sw.ElapsedMilliseconds < ReadyTimeoutMs)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
            return ready;
        }

        private static string WaitForValue(Func<string> read, string expectedSubstring)
        {
            var sw = Stopwatch.StartNew();
            string value = read() ?? "";
            while (!value.Contains(expectedSubstring) && sw.ElapsedMilliseconds < SyncTimeoutMs)
            {
                Application.DoEvents();
                Thread.Sleep(25);
                value = read() ?? "";
            }
            return value;
        }

        private static WebView2 GetInnerWebView(WebView2HtmlEditorControl editor)
        {
            var field = typeof(WebView2HtmlEditorControl).GetField("_webView", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "WebView2HtmlEditorControl._webView field not found; editor internals changed");
            var webView = (WebView2)field.GetValue(editor);
            Assert.IsNotNull(webView?.CoreWebView2, "inner WebView2 not initialized");
            return webView;
        }
    }
}
