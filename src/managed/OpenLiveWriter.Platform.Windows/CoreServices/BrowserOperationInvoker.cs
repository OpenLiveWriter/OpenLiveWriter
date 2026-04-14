// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.BrowserControl;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.CoreServices
{
    public class BrowserOperationInvoker
    {
        public delegate TResult BrowserOperation<TResult>(ExplorerBrowserControl browser);

        /// <summary>
        /// This method will create a new thread, make a hidden form and a browser
        /// control on that thread, navigate the browser to the URL you give it,
        /// wait for OnDocumentComplete, and then execute the operation you give it.
        /// </summary>
        /// <typeparam name="TResult">The type of the result</typeparam>
        /// <param name="url">The URL to navigate the browser to</param>
        /// <param name="label">The name to assign to the background thread (for easier debugging)</param>
        /// <param name="browserWidth">The approx. desired width of the browser</param>
        /// <param name="browserHeight">The approx. desired height of the browser</param>
        /// <param name="defaultValue">The value to return if the operation is not successful</param>
        /// <param name="operation">The operation to perform</param>
        /// <returns>The result returned by the operation, or if failed, defaultValue</returns>
        public static TResult InvokeAfterDocumentComplete<TResult>(string url, string label, int browserWidth, int browserHeight, TResult defaultValue, BrowserOperation<TResult> operation)
        {
            InvokerHelper<TResult> invoker = new InvokerHelper<TResult>(url, label, browserWidth, browserHeight, defaultValue, operation);
            invoker.Execute();
            return invoker.Result;
        }

        private class InvokerHelper<TResult>
        {
            private readonly string url;
            private readonly string label;
            private readonly int browserWidth, browserHeight;
            private readonly BrowserOperation<TResult> operation;

            private Form form;
            private ExplorerBrowserControl browser;
            private TResult result = default(TResult);

            public InvokerHelper(string url, string label, int browserWidth, int browserHeight, TResult defaultValue, BrowserOperation<TResult> operation)
            {
                this.result = defaultValue;
                this.browserHeight = browserHeight;
                this.operation = operation;
                this.url = url;
                this.label = label;
                this.browserWidth = browserWidth;
            }

            public TResult Result { get { return result; } }

            public void Execute()
            {
                Thread thread = ThreadHelper.NewThread(BackgroundMain, label, true, true, true);
                thread.Start();
                if (!thread.Join(10000))
                {
                    if (ControlHelper.ControlCanHandleInvoke(browser))
                        browser.Invoke(new BrowserDocumentEventHandler(Handler), new object[] { null, default(BrowserDocumentEventArgs) });
                }
            }

            private void BackgroundMain()
            {
                try
                {
                    using (form = new Form())
                    {
                        form.Size = new Size(browserWidth, browserHeight);

                        browser = new ExplorerBrowserControl();
                        browser.Dock = DockStyle.Fill;
                        browser.Silent = true;
                        browser.DocumentComplete += Handler;
                        form.Controls.Add(browser);
                        form.CreateControl();
                        browser.Navigate(url);

                        Application.Run();
                    }
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                }
            }

            private void Handler(object sender, BrowserDocumentEventArgs args)
            {
                try
                {
                    result = operation(browser);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                }
                finally
                {
                    Application.ExitThread();
                }

            }
        }
    }
}
