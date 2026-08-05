// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
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
    /// Live integration tests for the WebView2 rich-text formatting path. Font
    /// family, size, and color commands must produce span elements with inline
    /// CSS (font-family, font-size in exact pt, color) rather than deprecated
    /// font tags, and the resulting markup must flow back through the
    /// EditorContentBridge. Requires a real desktop session and the WebView2
    /// runtime; the tests Assert.Ignore when the environment cannot be created.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [Category("WebView2")]
    public class WebView2EditorFontMarkupTests
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
        public void ApplyFontFamily_EmitsSpanWithFontFamily_NotFontTag()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);
                SelectAllBodyText(editor);

                editor.CommandSource.ApplyFontFamily("Cambria");

                string body = WaitForValue(() => editor.GetEditedHtml(true), "font-family: Cambria");
                StringAssert.Contains("<span", body);
                StringAssert.Contains("font-family: Cambria", body);
                Assert.IsFalse(body.Contains("<font"), "body should not contain deprecated <font> tags: " + body);
            }
        }

        [Test]
        public void ApplyFontSize_EmitsSpanWithExactPointSize_NotFontTag()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);
                SelectAllBodyText(editor);

                editor.CommandSource.ApplyFontSize(24);

                string body = WaitForValue(() => editor.GetEditedHtml(true), "font-size: 24pt");
                StringAssert.Contains("<span", body);
                StringAssert.Contains("font-size: 24pt", body);
                Assert.IsFalse(body.Contains("<font"), "body should not contain deprecated <font> tags: " + body);
            }
        }

        [Test]
        public void ApplyFontForeColor_EmitsSpanWithColor_NotFontTag()
        {
            using (var form = CreateEditorForm(out WebView2HtmlEditorControl editor))
            {
                EnsureReadyOrIgnore(editor);
                SelectAllBodyText(editor);

                editor.CommandSource.ApplyFontForeColor(Color.Red.ToArgb());

                string body = WaitForValue(() => editor.GetEditedHtml(true), "rgb(255, 0, 0)");
                StringAssert.Contains("<span", body);
                StringAssert.Contains("color: rgb(255, 0, 0)", body);
                Assert.IsFalse(body.Contains("<font"), "body should not contain deprecated <font> tags: " + body);
            }
        }

        /// <summary>
        /// Puts known text in the body and selects it so formatting commands
        /// have a live range to act on.
        /// </summary>
        private static void SelectAllBodyText(WebView2HtmlEditorControl editor)
        {
            WebView2 webView = GetInnerWebView(editor);
            webView.CoreWebView2.ExecuteScriptAsync(
                "var b = document.getElementById('olw-body');" +
                "b.innerHTML = '<p>Style me cleanly</p>';" +
                "var range = document.createRange();" +
                "range.selectNodeContents(b);" +
                "var sel = window.getSelection();" +
                "sel.removeAllRanges();" +
                "sel.addRange(range);");
            // Give the script a beat to run before the command fires.
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 300)
            {
                Application.DoEvents();
                Thread.Sleep(10);
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
