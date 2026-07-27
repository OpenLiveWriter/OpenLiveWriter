// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using NUnit.Framework;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.WebView2Shim;
using StringAssert = NUnit.Framework.Legacy.StringAssert;

namespace OpenLiveWriter.Tests.WebView2Editor
{
    /// <summary>
    /// Live integration tests for WebView2 Preview mode. In Preview the editor
    /// must render the post inside the blog editing template as a read-only
    /// document (no contenteditable surface), with the extended-entry break
    /// marker stripped, and switching back must restore the editable shell.
    /// Requires a real desktop session and the WebView2 runtime; the tests
    /// Assert.Ignore when the environment cannot be created (e.g. session 0).
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [Category("WebView2")]
    public class WebView2PreviewModeTests
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
                    Path.Combine(Path.GetTempPath(), "OpenLiveWriter.Tests.WebView2"));
            }
        }

        [SetUp]
        public void EnsureApplicationEnvironment()
        {
            // GetDefaultTemplateHtml resolves template/default.htm relative to the
            // installation directory, which is the test output directory here.
            // Use a non-default product name: with the default product name
            // Initialize() throws when the profile has no Personal folder (e.g.
            // the SYSTEM account in a headless test session).
            if (ApplicationEnvironment.InstallationDirectory == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                ApplicationEnvironment.Initialize(assembly, Path.GetDirectoryName(assembly.Location),
                    "Software\\OpenLiveWriter.Tests", "Open Live Writer Tests");
            }
        }

        [Test]
        public void PreviewMode_RendersTemplateReadOnly_AndStripsMoreMarker()
        {
            using (var form = CreateEditorForm(out WebView2BlogPostHtmlEditorControl control))
            {
                WebView2HtmlEditorControl editor = GetInnerEditor(control);
                EnsureReadyOrIgnore(editor);

                control.PreviewMode = true;
                control.LoadHtmlFragment("My Preview Title",
                    "<p>Before break</p><!--more--><p>After break</p>",
                    "https://example.com/", CreateTemplate());

                string rendered = WaitForRenderedHtml(control, "My Preview Title");

                // The blog editing template was applied (default template uses
                // div.title / div.body containers).
                StringAssert.Contains("class=\"title\"", rendered, "template title container missing from preview");
                StringAssert.Contains("class=\"body\"", rendered, "template body container missing from preview");
                StringAssert.Contains("Before break", rendered);
                StringAssert.Contains("After break", rendered, "extended content missing from preview");
                Assert.IsFalse(rendered.Contains("<!--more-->"), "extended-entry break marker left in preview");
                Assert.IsFalse(rendered.Contains("contenteditable"), "preview document is editable");
            }
        }

        [Test]
        public void SwitchingBackToEditMode_RestoresEditableShell()
        {
            using (var form = CreateEditorForm(out WebView2BlogPostHtmlEditorControl control))
            {
                WebView2HtmlEditorControl editor = GetInnerEditor(control);
                EnsureReadyOrIgnore(editor);

                control.PreviewMode = true;
                control.LoadHtmlFragment("Round Trip", "<p>round trip body</p>",
                    "https://example.com/", CreateTemplate());
                WaitForRenderedHtml(control, "Round Trip");

                control.PreviewMode = false;
                control.LoadHtmlFragment("Round Trip", "<p>round trip body</p>",
                    "https://example.com/", CreateTemplate());

                string rendered = WaitForRenderedHtml(control, "olw-body");
                StringAssert.Contains("contenteditable", rendered, "editing shell was not restored after preview");
                StringAssert.Contains("round trip body", rendered, "post body missing after switching back to edit mode");
            }
        }

        private static BlogEditingTemplate CreateTemplate()
        {
            return new BlogEditingTemplate(BlogEditingTemplate.GetDefaultTemplateHtml(true), true);
        }

        private static Form CreateEditorForm(out WebView2BlogPostHtmlEditorControl control)
        {
            Form form;
            WebView2BlogPostHtmlEditorControl editorControl;
            try
            {
                form = new Form();
                editorControl = new WebView2BlogPostHtmlEditorControl();
                editorControl.EditorControl.Dock = DockStyle.Fill;
                form.Controls.Add(editorControl.EditorControl);
                form.Show();
            }
            catch (Exception ex)
            {
                Assert.Ignore("Could not create editor host form in this session: " + ex.Message);
                throw; // unreachable; Assert.Ignore throws
            }
            control = editorControl;
            return form;
        }

        private static WebView2HtmlEditorControl GetInnerEditor(WebView2BlogPostHtmlEditorControl control)
        {
            var field = typeof(WebView2BlogPostHtmlEditorControl).GetField("_editor", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "WebView2BlogPostHtmlEditorControl._editor field not found; editor internals changed");
            var editor = (WebView2HtmlEditorControl)field.GetValue(control);
            Assert.IsNotNull(editor, "inner WebView2 editor not created");
            return editor;
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

        /// <summary>
        /// Polls document.documentElement.outerHTML in the live WebView2 until it
        /// contains the expected marker or the sync timeout elapses.
        /// </summary>
        private static string WaitForRenderedHtml(WebView2BlogPostHtmlEditorControl control, string expectedSubstring)
        {
            WebView2 webView = GetInnerWebView(GetInnerEditor(control));
            var sw = Stopwatch.StartNew();
            string rendered = ExecuteScriptAndWait(webView, "document.documentElement.outerHTML") ?? "";
            while (!rendered.Contains(expectedSubstring) && sw.ElapsedMilliseconds < SyncTimeoutMs)
            {
                Application.DoEvents();
                Thread.Sleep(25);
                rendered = ExecuteScriptAndWait(webView, "document.documentElement.outerHTML") ?? "";
            }
            return rendered;
        }

        private static string ExecuteScriptAndWait(WebView2 webView, string script)
        {
            if (webView.CoreWebView2 == null)
                return null;
            Task<string> task = webView.CoreWebView2.ExecuteScriptAsync(script);
            var sw = Stopwatch.StartNew();
            while (!task.IsCompleted && sw.ElapsedMilliseconds < SyncTimeoutMs)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
            if (!task.IsCompleted || task.IsFaulted)
                return null;
            // ExecuteScriptAsync returns the JSON-encoded result; strip the
            // surrounding quotes for substring assertions.
            string json = task.Result;
            if (json != null && json.Length >= 2 && json.StartsWith("\"") && json.EndsWith("\""))
                json = json.Substring(1, json.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
            return json;
        }

        private static WebView2 GetInnerWebView(WebView2HtmlEditorControl editor)
        {
            var field = typeof(WebView2HtmlEditorControl).GetField("_webView", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "WebView2HtmlEditorControl._webView field not found; editor internals changed");
            var webView = (WebView2)field.GetValue(editor);
            Assert.IsNotNull(webView, "inner WebView2 not created");
            return webView;
        }
    }
}
