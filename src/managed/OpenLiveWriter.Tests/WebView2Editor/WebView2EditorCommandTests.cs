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
using OpenLiveWriter.WebView2Shim;
using StringAssert = NUnit.Framework.Legacy.StringAssert;

namespace OpenLiveWriter.Tests.WebView2Editor
{
    /// <summary>
    /// Live integration tests for WebView2 editor formatting commands and undo
    /// behavior. Requires a real desktop session and the WebView2 runtime; the
    /// tests Assert.Ignore when the environment cannot be created (e.g. session 0).
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [Category("WebView2")]
    public class WebView2EditorCommandTests
    {
        private const int ReadyTimeoutMs = 30000;
        private const int SyncTimeoutMs = 10000;

        [OneTimeSetUp]
        public void ConfigureWebView2UserDataFolder()
        {
            // See WebView2EditorTypingTests: the loader needs a writable
            // user-data folder under testhost.
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER")))
            {
                Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER",
                    Path.Combine(Path.GetTempPath(), "OpenLiveWriter.Tests.WebView2"));
            }
        }

        [Test]
        public void Undo_AfterHeadingOnParagraphs_DoesNotDuplicateContent()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);
                WebView2 webView = GetInnerWebView(editor);
                editor.FocusBody();

                // Build three paragraphs the way typing does: text plus paragraph
                // breaks. Chromium creates <div> blocks that the div-to-p observer
                // then converts; those observer edits are what used to corrupt the
                // Chromium undo stack.
                ExecuteScript(webView,
                    "var b = document.getElementById('olw-body');" +
                    "b.focus();" +
                    "document.execCommand('insertHTML', false, 'one');" +
                    "document.execCommand('insertParagraph');" +
                    "document.execCommand('insertHTML', false, 'two');" +
                    "document.execCommand('insertParagraph');" +
                    "document.execCommand('insertHTML', false, 'three');");

                // Wait for the observer to convert the typed blocks to <p>.
                Assert.IsTrue(WaitFor(() =>
                {
                    string html = editor.GetEditedHtml(true);
                    return html.Contains("three") &&
                           html.IndexOf("<p", StringComparison.OrdinalIgnoreCase) >= 0 &&
                           html.IndexOf("<div", StringComparison.OrdinalIgnoreCase) < 0;
                }), "typed paragraphs were not converted to <p> by the observer");

                // Select all and apply Heading 2 (raw formatBlock, as the ribbon did).
                ExecuteScript(webView,
                    "document.execCommand('selectAll');" +
                    "document.execCommand('formatBlock', false, '<H2>');");
                Assert.IsTrue(WaitFor(() =>
                    editor.GetEditedHtml(true).IndexOf("<h2", StringComparison.OrdinalIgnoreCase) >= 0),
                    "formatBlock did not produce a heading");

                editor.CommandSource.Undo();

                // Undo must remove the heading and restore the paragraphs exactly
                // once each (pre-fix the observer mutation during undo left the
                // heading in place AND restored the paragraphs).
                Assert.IsTrue(WaitFor(() =>
                    editor.GetEditedHtml(true).IndexOf("<h2", StringComparison.OrdinalIgnoreCase) < 0),
                    "undo did not remove the heading");

                string body = editor.GetEditedHtml(true);
                Assert.AreEqual(1, CountOccurrences(body, "one"), "paragraph 'one' duplicated by undo: " + body);
                Assert.AreEqual(1, CountOccurrences(body, "two"), "paragraph 'two' duplicated by undo: " + body);
                Assert.AreEqual(1, CountOccurrences(body, "three"), "paragraph 'three' duplicated by undo: " + body);
            }
        }

        [Test]
        public void ApplyHeading_OnMultiParagraphSelection_FormatsEachBlock()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);
                WebView2 webView = GetInnerWebView(editor);

                ExecuteScript(webView,
                    "var b = document.getElementById('olw-body');" +
                    "b.innerHTML = '<p>first</p><p>second</p><p>third</p>';" +
                    "b.focus();" +
                    "var range = document.createRange();" +
                    "range.selectNodeContents(b);" +
                    "var sel = window.getSelection();" +
                    "sel.removeAllRanges();" +
                    "sel.addRange(range);");

                editor.CommandSource.ApplyHtmlFormattingStyle(new TestFormattingStyle("H2"));

                // Each paragraph becomes its own heading (0.6.2 behavior); the
                // buggy path produced one h2 with the paragraphs joined by <br>.
                Assert.IsTrue(WaitFor(() =>
                {
                    string html = editor.GetEditedHtml(true).ToLowerInvariant();
                    return CountOccurrences(html, "<h2") >= 3;
                }), "expected one h2 per paragraph, got: " + editor.GetEditedHtml(true));

                string body = editor.GetEditedHtml(true).ToLowerInvariant();
                Assert.AreEqual(3, CountOccurrences(body, "<h2"), "wrong h2 count: " + body);
                Assert.IsFalse(body.Contains("<br"), "heading merge left <br> separators: " + body);
            }
        }

        private class TestFormattingStyle : OpenLiveWriter.HtmlEditor.IHtmlFormattingStyle
        {
            private readonly string _elementName;

            public TestFormattingStyle(string elementName)
            {
                _elementName = elementName;
            }

            public string DisplayName => _elementName;
            public string ElementName => _elementName;
            public mshtml._ELEMENT_TAG_ID ElementTagId => mshtml._ELEMENT_TAG_ID.TAGID_NULL;
        }

        [Test]
        public void ApplyBullets_OnParagraphs_ProducesListOutsideParagraph()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);
                WebView2 webView = GetInnerWebView(editor);

                ExecuteScript(webView,
                    "var b = document.getElementById('olw-body');" +
                    "b.innerHTML = '<p>alpha</p><p>beta</p>';" +
                    "b.focus();" +
                    "var range = document.createRange();" +
                    "range.selectNodeContents(b);" +
                    "var sel = window.getSelection();" +
                    "sel.removeAllRanges();" +
                    "sel.addRange(range);");

                editor.CommandSource.ApplyBullets();

                // The list must exist and must not be nested inside a paragraph:
                // <p><ul>...</ul></p> is invalid HTML.
                Assert.IsTrue(WaitFor(() =>
                {
                    string html = editor.GetEditedHtml(true).ToLowerInvariant();
                    return html.Contains("<ul") && html.Contains("<li");
                }), "bullet command did not produce a list");

                string body = editor.GetEditedHtml(true).ToLowerInvariant();
                Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(body, @"<p[^>]*>\s*<ul"),
                    "ul nested inside p: " + body);
                StringAssert.Contains("alpha", body);
                StringAssert.Contains("beta", body);
            }
        }

        [Test]
        public void SourceEditor_UndoAfterSetContent_DoesNotWipeContent()
        {
            Form form = null;
            try
            {
                form = new Form();
                var editor = new WebView2SourceEditorControl { Dock = DockStyle.Fill };
                form.Controls.Add(editor);
                try
                {
                    form.Show();
                }
                catch (Exception ex)
                {
                    Assert.Ignore("Could not show editor host form in this session: " + ex.Message);
                }

                WebView2 webView = GetInnerSourceWebView(editor);

                // Wait for CodeMirror to come up.
                if (!WaitFor(() => "\"function\"".Equals(ExecuteScript(webView, "typeof getContent"))))
                {
                    Assert.Ignore("WebView2 source editor did not become ready (runtime or desktop unavailable in this session).");
                }

                editor.SetContent("<p>hello source</p>");
                Assert.IsTrue(WaitFor(() =>
                    (ExecuteScript(webView, "getContent()") ?? "").Contains("hello source")),
                    "SetContent did not reach CodeMirror");

                // Two undos used to walk back past the initial programmatic load
                // and empty the document.
                ExecuteScript(webView, "editor.undo()");
                ExecuteScript(webView, "editor.undo()");
                Pump(200);

                string content = ExecuteScript(webView, "getContent()") ?? "";
                StringAssert.Contains("hello source", content);
            }
            finally
            {
                form?.Dispose();
            }
        }

        private static void Pump(int milliseconds)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            int count = 0;
            int index = 0;
            while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += needle.Length;
            }
            return count;
        }

        private static string ExecuteScript(WebView2 webView, string script)
        {
            if (webView?.CoreWebView2 == null) return null;
            var task = webView.CoreWebView2.ExecuteScriptAsync(script);
            var sw = Stopwatch.StartNew();
            while (!task.IsCompleted && sw.ElapsedMilliseconds < SyncTimeoutMs)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
            return task.IsCompleted ? task.Result : null;
        }

        private static bool WaitFor(Func<bool> condition)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < SyncTimeoutMs)
            {
                if (condition()) return true;
                Application.DoEvents();
                Thread.Sleep(25);
            }
            return condition();
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
            bool ready = false;
            editor.ReadyForEditing += (s, e) => ready = true;
            var sw = Stopwatch.StartNew();
            while (!ready && sw.ElapsedMilliseconds < ReadyTimeoutMs)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
            if (!ready)
            {
                string detail = editor.InitializationError != null
                    ? " Initialization error: " + editor.InitializationError
                    : " No initialization error was captured; ReadyForEditing never fired.";
                Assert.Ignore("WebView2 editor did not become ready (runtime or desktop unavailable in this session)." + detail);
            }
        }

        private static WebView2 GetInnerWebView(WebView2HtmlEditorControl editor)
        {
            var field = typeof(WebView2HtmlEditorControl).GetField("_webView", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "WebView2HtmlEditorControl._webView field not found; editor internals changed");
            var webView = (WebView2)field.GetValue(editor);
            Assert.IsNotNull(webView, "inner WebView2 not created");
            return webView;
        }

        private static WebView2 GetInnerSourceWebView(WebView2SourceEditorControl editor)
        {
            var field = typeof(WebView2SourceEditorControl).GetField("_webView", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "WebView2SourceEditorControl._webView field not found; editor internals changed");
            var webView = (WebView2)field.GetValue(editor);
            Assert.IsNotNull(webView, "inner WebView2 not created");
            return webView;
        }
    }
}
