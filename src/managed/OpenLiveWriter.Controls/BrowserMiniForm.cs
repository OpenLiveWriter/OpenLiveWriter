// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.BrowserControl;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.ActiveDocuments;
using OpenLiveWriter.Interop.Windows;

using mshtml;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Summary description for BrowserMiniForm.
    /// </summary>
    public class BrowserMiniForm : MiniForm
    {
        private OpenLiveWriter.BrowserControl.ExplorerBrowserControl _explorerBrowserControl;
        private string _url;

        public BrowserMiniForm(string url, int downloadOptions, WinInetCredentialsContext credentialsContext)
        {
            // record url to navigate to
            _url = url;

            // standard "flyout" form behavior
            DismissOnDeactivate = true;

            // dock inset for drawing of border
            DockPadding.All = 1;

            // background color is white to minimize flashing problem when dismissing
            BackColor = Color.White;

            // initialize browser control
            _explorerBrowserControl = new ExplorerBrowserControl();
            _explorerBrowserControl.Dock = DockStyle.Fill;
            Controls.Add(_explorerBrowserControl);

            // install download options if requested
            if (downloadOptions > 0)
            {
                _explorerBrowserControl.DownloadOptions = downloadOptions;
            }

            // install network credential if requested
            if (credentialsContext != null)
            {
                if (credentialsContext.NetworkCredential != null)
                    _explorerBrowserControl.NetworkCredential = credentialsContext.NetworkCredential;

                if (credentialsContext.CookieString != null)
                    _explorerBrowserControl.SetCookies(credentialsContext.CookieString.Url, credentialsContext.CookieString.Cookies);
            }

            // other options
            _explorerBrowserControl.Silent = true;

            // Navigate to about:blank for installation of ui customizations. Note that this step is CRITICAL
            // to ensuring that not only our custom ui hooks get installed but also to ensure that our DLCTL
            // options (which control security) are registered prior to the fectching of the content.
            _explorerBrowserControl.DocumentComplete += new BrowserDocumentEventHandler(_explorerBrowserControl_AboutBlankDocumentComplete);
            _explorerBrowserControl.Navigate("about:blank");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            NavigateToProgressPage();
        }

        private void NavigateToProgressPage()
        {
            try
            {
                // write progress page if we need to
                string tifDirectory = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                string progressPageDirectory = Path.Combine(tifDirectory, "OpenLiveWriter\\ProgressPage");
                string progressPagePath = Path.Combine(progressPageDirectory, "index.htm");
                if (!File.Exists(progressPagePath))
                {
                    // create the directory if necessary
                    if (!Directory.Exists(progressPageDirectory))
                        Directory.CreateDirectory(progressPageDirectory);

                    // write the animation
                    string animationGifName = "BallSpinner.gif";
                    string animationGifPath = Path.Combine(progressPageDirectory, animationGifName);
                    ResourceHelper.SaveAssemblyResourceToFile("Images." + animationGifName, animationGifPath);

                    // write the web page
                    string progressText = Res.Get(StringId.BrowserMiniFormProgress);
                    int bodyHeight = _explorerBrowserControl.Height - 50;
                    int progressTop = Math.Max((bodyHeight / 2) - 50, 0);
                    using (StreamWriter writer = new StreamWriter(progressPagePath, false, Encoding.UTF8))
                    {
                        writer.Write(@"
                            <html>
                                <head></head>
                                <body>
                                    <div style=""height={0}px;"">
                                    <div style=""position: absolute; top: {1}; width: 100%; text-align: center"" >
                                    <img src=""{2}""></img>
                                    <p>
                                        <font size=""2"" face=""Tahoma, Arial"" color=""rgb(190,200,210)"">
                                            <b>{3}</b>
                                        </font>
                                    </p>
                                    </div>
                                    </div>
                                </body>
                            </html>
                        ", bodyHeight, progressTop, animationGifName, HtmlUtils.EscapeEntities(progressText));
                    }
                }

                // navigate to progress page
                _explorerBrowserControl.DocumentComplete += new BrowserDocumentEventHandler(_explorerBrowserControl_ProgressDocumentComplete);
                _explorerBrowserControl.Navigate(progressPagePath);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception occurred while attempting to display progress page: " + ex.ToString());
            }
        }

        private void _explorerBrowserControl_ProgressDocumentComplete(object sender, BrowserDocumentEventArgs e)
        {
            try
            {
                // unsubscribe from the event
                _explorerBrowserControl.DocumentComplete -= new BrowserDocumentEventHandler(_explorerBrowserControl_ProgressDocumentComplete);

                // navigate to the actual target (pulse)
                BeginInvoke(new InvokeInUIThreadDelegate(NavigateBrowser));
            }

            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception in BrowserMiniForm.ProgressDocumentComplete: " + ex.ToString());
            }
        }

        private void NavigateBrowser()
        {
            _explorerBrowserControl.Navigate(_url);
        }

        private void _explorerBrowserControl_AboutBlankDocumentComplete(object sender, BrowserDocumentEventArgs e)
        {
            try
            {
                // unsubscribe from the event
                _explorerBrowserControl.DocumentComplete -= new BrowserDocumentEventHandler(_explorerBrowserControl_AboutBlankDocumentComplete);

                // set borders to none
                IHTMLDocument2 document = _explorerBrowserControl.Document as IHTMLDocument2;
                if (document != null)
                {
                    ICustomDoc customDoc = (ICustomDoc)document;
                    customDoc.SetUIHandler(new BrowserDocHostUIHandler());

                    if (document.body != null && document.body.style != null)
                        document.body.style.borderStyle = "none";
                    else
                        Debug.Fail("Couldn't set document body style after document completed!");
                }
                else
                {
                    Debug.Fail("Couldn't get document after document completed!");
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception in BrowserMiniForm.AboutBlankDocumentComplete: " + ex.ToString());
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawRectangle(SystemPens.ControlDark, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected exception disposing BrowserMiniForm: " + ex.ToString());
                }

            }
            base.Dispose(disposing);
        }

        private class BrowserDocHostUIHandler : IDocHostUIHandlerBaseImpl
        {

            public override int ShowContextMenu(int dwID, ref OpenLiveWriter.Interop.Windows.POINT ppt, object pcmdtReserved, object pdispReserved)
            {
                return HRESULT.S_OK;
            }

            public override void GetHostInfo(ref DOCHOSTUIINFO pInfo)
            {
                // NOTE: this does not seem to be working for web pages we navigate to by url
                pInfo.dwFlags |= (DOCHOSTUIFLAG.NO3DBORDER | DOCHOSTUIFLAG.NO3DOUTERBORDER);
            }

        }

    }
}
