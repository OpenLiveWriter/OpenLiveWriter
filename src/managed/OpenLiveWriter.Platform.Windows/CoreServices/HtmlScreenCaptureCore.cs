// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Threading;
using Timer = System.Windows.Forms.Timer;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.BrowserControl;
using mshtml;

namespace OpenLiveWriter.CoreServices
{
    public class HtmlScreenCaptureCore
    {
        public HtmlScreenCaptureCore(Uri url, int contentWidth)
        {
            _htmlUrl = UrlHelper.SafeToAbsoluteUri(url);
            _contentWidth = contentWidth;
        }

        public HtmlScreenCaptureCore(string htmlContent, int contentWidth)
        {
            _htmlContent = htmlContent;
            _contentWidth = contentWidth;
        }

        public bool ShowWaitCursor
        {
            get { return _showWaitCursor; }
            set { _showWaitCursor = value; }
        }

        private string[] _ids;
        public string[] Ids
        {
            get { return _ids; }
            set { _ids = value; }
        }

        public int MaximumHeight
        {
            get { return _maximumHeight; }
            set { _maximumHeight = value; }
        }

        public event HtmlDocumentAvailableHandlerCore HtmlDocumentAvailable;

        public event HtmlScreenCaptureAvailableHandlerCore HtmlScreenCaptureAvailable;

        public ElementCaptureProperties GetElementCaptureProperties(string id)
        {
            return _elementCaptureProperties[id];
        }

        /// <summary>
        /// Captures HTML content as a bitmap.
        /// </summary>
        /// <remarks>
        /// STUBBED OUT: This feature relied on MSHTML/IE browser control for rendering HTML and capturing screenshots.
        /// TODO: Re-implement using WebView2.CapturePreviewAsync() when WebView2 migration is complete.
        /// WebView2 provides CapturePreviewAsync(Stream, CoreWebView2CapturePreviewImageFormat) for this purpose.
        /// </remarks>
        public Bitmap CaptureHtml(int timeoutMs)
        {
            // Return null - screenshot capture is not available without MSHTML
            // Callers should handle null gracefully (show placeholder or skip preview)
            System.Diagnostics.Trace.WriteLine("[HtmlScreenCaptureCore] CaptureHtml stubbed out - MSHTML dependency removed. Will be re-implemented with WebView2.");
            return null;
        }

        /// <summary>
        /// Takes a snapshot of an IViewObject.
        /// </summary>
        /// <remarks>
        /// STUBBED OUT: This feature relied on COM IViewObject interface which is MSHTML-specific.
        /// TODO: Re-implement using WebView2.CapturePreviewAsync() when WebView2 migration is complete.
        /// </remarks>
        public static Bitmap TakeSnapshot(IViewObject obj, int width, int height)
        {
            // Return null - snapshot capture is not available without MSHTML
            System.Diagnostics.Trace.WriteLine("[HtmlScreenCaptureCore] TakeSnapshot stubbed out - MSHTML dependency removed. Will be re-implemented with WebView2.");
            return null;
        }

        [STAThread]
        private void ThreadMain(ConditionVariable signal, string[] ids)
        {
            HtmlScreenCaptureForm form = null;
            try
            {
                // housekeeping initialization
                Application.OleRequired();

                // create the form and execute the capture
                form = new HtmlScreenCaptureForm(this);
                this.Ids = ids;
                form.Ids = ids;
                form.DoCapture();

                // Create and run the form
                _applicationContext = new FormLifetimeApplicationContext(form);
                signal.Signal();
                Application.Run(_applicationContext);

                // propagate exceptions that happened inside the AppContext
                if (_applicationContext.Exception != null)
                    throw _applicationContext.Exception;
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
            finally
            {
                if (form != null)
                    form.Close();
            }
        }

        internal string HtmlUrl
        {
            get { return _htmlUrl; }
        }

        internal string HtmlContent
        {
            get { return _htmlContent; }
        }

        internal int ContentWidth
        {
            get { return _contentWidth; }
        }

        internal int TimeoutMs
        {
            get { return _timeoutMs; }
        }

        internal void FireHtmlDocumentAvailable(HtmlDocumentAvailableEventArgsCore e)
        {
            if (HtmlDocumentAvailable != null)
                HtmlDocumentAvailable(this, e);
        }

        internal void FireHtmlScreenCaptureAvailable(HtmlScreenCaptureAvailableEventArgsCore e)
        {
            if (HtmlScreenCaptureAvailable != null)
                HtmlScreenCaptureAvailable(this, e);
        }

        internal void SetCapturedBitmap(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            _capturedBitmap = ms.ToArray();
        }

        internal void SetElementCaptureProperties(string id, Bitmap bitmap, Color backgroundColor, Padding padding)
        {
            byte[] bytes = null;
            if (bitmap != null)
            {
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                bytes = ms.ToArray();
            }
            _elementCaptureProperties[id] = new ElementCaptureProperties(bytes, backgroundColor, padding);
        }

        internal void SetException(Exception exception)
        {
            _exception = exception;
        }

        // capture parameters
        private int _maximumHeight = -1;
        private bool _showWaitCursor = true;
        private string _htmlUrl;
        private string _htmlContent;
        private int _contentWidth;
#pragma warning disable CS0649 // Field is never assigned to - stubbed out methods don't use these
        private int _timeoutMs;

        // successfully captured bitmap
        private byte[] _capturedBitmap;
#pragma warning restore CS0649
        private Dictionary<string, ElementCaptureProperties> _elementCaptureProperties = new Dictionary<string, ElementCaptureProperties>();

        // error that occurred during processing
        private Exception _exception;
        private FormLifetimeApplicationContext _applicationContext;

    }

    public class ElementCaptureProperties
    {
        private byte[] _capturedBitmap;
        public Padding Padding { get; set; }

        public ElementCaptureProperties(byte[] capturedBitmap, Color backgroundColor, Padding padding)
        {
            _capturedBitmap = capturedBitmap;
            BackgroundColor = backgroundColor;
            Padding = padding;
        }

        public Color BackgroundColor { get; private set; }

        public Bitmap Bitmap
        {
            get
            {
                return _capturedBitmap == null
                           ? null
                           : (Bitmap)Bitmap.FromStream(StreamHelper.AsStream(_capturedBitmap));
            }
        }

    }

    public class HtmlScreenCaptureAvailableEventArgsCore : EventArgs
    {
        public HtmlScreenCaptureAvailableEventArgsCore(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }
        private Bitmap _bitmap;

        public bool CaptureCompleted
        {
            get { return _captureCompletedBytes; }
            set { _captureCompletedBytes = value; }
        }
        private bool _captureCompletedBytes = true;
    }

    public class HtmlDocumentAvailableEventArgsCore : EventArgs
    {
        public HtmlDocumentAvailableEventArgsCore(object document)
        {
            _document = document;
        }

        public object Document
        {
            get { return _document; }
        }
        private object _document;

        public bool DocumentReady
        {
            get { return _documentReady; }
            set { _documentReady = value; }
        }
        private bool _documentReady = true;
    }

    public delegate void HtmlScreenCaptureAvailableHandlerCore(object sender, HtmlScreenCaptureAvailableEventArgsCore e);

    public delegate void HtmlDocumentAvailableHandlerCore(object sender, HtmlDocumentAvailableEventArgsCore e);

    /// <summary>
    /// Special application context that will mirror the lifetime of a Form
    /// without calling Form.Show (as the standard implementation of
    /// ApplicationContext does)
    /// </summary>
    internal class FormLifetimeApplicationContext : ApplicationContext
    {
        public FormLifetimeApplicationContext(Form form)
        {
            _form = form;
            if (form.IsHandleCreated)
                _hwnd = form.Handle;
            else
                form.HandleCreated += delegate { _hwnd = form.Handle; };
            _form.Closed += new EventHandler(form_Closed);
        }

        public Exception Exception
        {
            get { return _exception; }
        }
        private Exception _exception;

        public void CloseFormAndExit()
        {
            // notify the form that it should close (call using PostMessage b/c
            // this call may come from another thread). This will trigger the
            // Form.Closed event and cleanup will proceed from there
            if (_hwnd != IntPtr.Zero)
                User32.PostMessage(_hwnd, WM.CLOSE, UIntPtr.Zero, IntPtr.Zero);
        }

        private void form_Closed(object sender, EventArgs e)
        {
            try
            {
                // safely ensure form is disposed
                try { _form.Dispose(); }
                catch { }
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
            finally
            {
                ExitThread();
            }
        }

        private Form _form;
        private IntPtr _hwnd;
    }

    internal class HtmlScreenCaptureForm : BaseForm
    {

        public HtmlScreenCaptureForm(HtmlScreenCaptureCore htmlScreenCaptureCore)
        {
            // save reference to parent object
            _htmlScreenCaptureCore = htmlScreenCaptureCore;

            // set the timeout time
            _timeoutTime = DateTime.Now.AddMilliseconds(_htmlScreenCaptureCore.TimeoutMs);

            // create and add the underlying browser control
            _browserControl = new ExplorerBrowserControl();
            _browserControl.Silent = true;

            Controls.Add(_browserControl);
        }

        public void DoCapture()
        {
            // show the form w/o activating then make it invisible
            User32.SetWindowPos(Handle, HWND.BOTTOM, -1, -1, 1, 1, SWP.NOACTIVATE);
            Visible = false;

            // determine the url used for navigation
            string navigateUrl;
            if (_htmlScreenCaptureCore.HtmlUrl != null)
            {
                navigateUrl = _htmlScreenCaptureCore.HtmlUrl;
            }
            else
            {
                _contentFile = TempFileManager.Instance.CreateTempFile("content.htm");
                using (TextWriter textWriter = new StreamWriter(_contentFile, false, Encoding.UTF8))
                {
                    String html = _htmlScreenCaptureCore.HtmlContent;

                    //add the "Mark Of The Web" so that the HTML will execute in the Internet Zone
                    //otherwise, it will execute in the Local Machine zone, which won't allow JavaScript
                    //or object/embed tags.
                    html = HTMLDocumentHelper.AddMarkOfTheWeb(html, "about:internet");

                    textWriter.Write(html);
                }
                navigateUrl = _contentFile;
            }

            // navigate to the file then wait for document complete for further processing
            _browserControl.DocumentComplete += new BrowserDocumentEventHandler(_browserControl_DocumentComplete);
            _browserControl.Navigate(navigateUrl);
        }

        private void _browserControl_DocumentComplete(object sender, BrowserDocumentEventArgs e)
        {
            Timer timer = null;
            try
            {
                // unsubscribe from the event
                _browserControl.DocumentComplete -= new BrowserDocumentEventHandler(_browserControl_DocumentComplete);

                // get the document
                IHTMLDocument2 document = _browserControl.Document as IHTMLDocument2;

                // eliminate borders, scroll bars, and margins
                IHTMLElement element = document.body;
                element.style.borderStyle = "none";
                IHTMLBodyElement body = element as IHTMLBodyElement;
                body.scroll = "no";
                body.leftMargin = 0;
                body.rightMargin = 0;
                body.topMargin = 0;
                body.bottomMargin = 0;

                // set the width and height of the browser control to the correct
                // values for the snapshot

                // width specified by the caller
                _browserControl.Width = _htmlScreenCaptureCore.ContentWidth;

                if (_htmlScreenCaptureCore.MaximumHeight > 0)
                    _browserControl.Height = _htmlScreenCaptureCore.MaximumHeight;
                else
                {
                    // height of the content calculated based on this width
                    IHTMLElement2 element2 = element as IHTMLElement2;
                    _browserControl.Height = element2.scrollHeight;
                }

                // release UI thread to load the video thumbnail on screen
                // (the Tick may need to fire more than once to allow enough
                // time and message processing for an embedded object to
                // be fully initialized)
                timer = new Timer();
                timer.Interval = WAIT_INTERVAL;
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();
            }
            catch (Exception ex)
            {
                _htmlScreenCaptureCore.SetException(ex);
                CleanupAndExit(timer);
            }
        }

        /// <summary>
        /// Prevents asserts that happen within timer_Tick from going bonkers; since
        /// they can happen reentrantly, you can end up with hundreds of assert windows.
        /// </summary>
        private bool reentrant = false;

        private void timer_Tick(object sender, EventArgs e)
        {
            if (reentrant)
                return;
            reentrant = true;

            Timer timer = null;
            try
            {
                timer = sender as Timer;
                IHTMLDocument2 document = _browserControl.Document as IHTMLDocument2;

                // make sure the document is ready
                if (!DocumentReady(document))
                {
                    if (TimedOut)
                        CleanupAndExit(timer);

                    return;
                }

                if (Ids != null)
                {
                    IHTMLDocument3 doc3 = document as IHTMLDocument3;

                    foreach (string elementId in Ids)
                    {
                        IHTMLStyle3 style3 = (IHTMLStyle3)(doc3.getElementById(elementId)).style;
                        style3.wordWrap = "normal";
                    }

                    int originalWidth = _browserControl.Width;
                    int originalHeight = _browserControl.Height;
                    foreach (string elementId in Ids)
                    {
                        try
                        {
                            IHTMLElement element = doc3.getElementById(elementId);

                            _browserControl.Height = element.offsetHeight + 2 + 2;

                            element.scrollIntoView(true);

                            Padding padding = HTMLElementHelper.PaddingInPixels(element);

                            Color backgroundColor = HTMLColorHelper.GetBackgroundColor(element, true, null, Color.White);

                            using (Bitmap elementBitmap = GetElementPreviewImage(doc3, element, _browserControl.Width, _browserControl.Height))
                            {
                                _htmlScreenCaptureCore.SetElementCaptureProperties(elementId, elementBitmap, backgroundColor, padding);
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.Fail("Failed to capture element " + elementId + ": " + ex);
                        }
                    }

                    // Restore the browser control size
                    _browserControl.Width = originalWidth;
                    _browserControl.Height = originalHeight;
                }

                // fire event to see if the Bitmap is ready
                using (Bitmap bitmap = HtmlScreenCaptureCore.TakeSnapshot((IViewObject)_browserControl.Document, _browserControl.Width, _browserControl.Height))
                {
                    HtmlScreenCaptureAvailableEventArgsCore ea = new HtmlScreenCaptureAvailableEventArgsCore(bitmap);
                    _htmlScreenCaptureCore.FireHtmlScreenCaptureAvailable(ea);

                    if (ea.CaptureCompleted)
                    {
                        // provide the bitmap to our parent object
                        _htmlScreenCaptureCore.SetCapturedBitmap(bitmap);
                        //bitmap.Save(@"c:\temp\captured.bmp");

                        // exit
                        CleanupAndExit(timer);
                    }
                    else if (TimedOut) // if we have timed out then exit
                    {
                        CleanupAndExit(timer);
                    }

                    // otherwise just let the Timer call us again
                    // at the next interval...
                }
            }
            catch (Exception ex)
            {
                _htmlScreenCaptureCore.SetException(ex);
                CleanupAndExit(timer);
            }
            finally
            {
                reentrant = false;
            }
        }

        private void CleanupAndExit(Timer timer)
        {
            // safely dispose timer
            try
            {
                if (timer != null)
                {
                    timer.Tick -= new EventHandler(timer_Tick);
                    timer.Stop();
                    timer.Dispose();
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception disposing exception: " + ex.ToString());
            }

            // close the form
            Close();
        }

        private string[] _ids;
        public string[] Ids
        {
            get { return _ids; }
            set { _ids = value; }
        }

        private bool DocumentReady(IHTMLDocument2 document)
        {
            // check for a browser that is still busy
            if (_browserControl.Browser.Busy)
                return false;

            // check for embeds that are not ready
            foreach (object embedObject in document.embeds)
            {
                mshtml.DispHTMLEmbed dispEmbed = embedObject as mshtml.DispHTMLEmbed;
                if (dispEmbed == null || dispEmbed.readyState?.ToString() != "complete")
                    return false;
            }

            // fire documented completed hook to clients and return result
            HtmlDocumentAvailableEventArgsCore ea = new HtmlDocumentAvailableEventArgsCore(document);
            _htmlScreenCaptureCore.FireHtmlDocumentAvailable(ea);

            return ea.DocumentReady;
        }

        private static Bitmap GetElementPreviewImage(IHTMLDocument3 doc3, IHTMLElement element, int snapshotWidth, int snapshotHeight)
        {
            try
            {
                // @RIBBON TODO: Need to make this work for RTL as well.
                IDisplayServices displayServices = ((IDisplayServices)doc3);

                element.scrollIntoView(true);

                tagPOINT offset = new tagPOINT();
                offset.x = 0;
                offset.y = 0;
                displayServices.TransformPoint(ref offset, _COORD_SYSTEM.COORD_SYSTEM_CONTENT, _COORD_SYSTEM.COORD_SYSTEM_GLOBAL, element);

                using (Bitmap snapshotAfter = HtmlScreenCaptureCore.TakeSnapshot((IViewObject)doc3, snapshotWidth, snapshotHeight))
                {
                    //snapshotAfter.Save(@"c:\temp\snapshot" + element.id + ".bmp");

                    Rectangle elementRect;
                    elementRect = new Rectangle(Math.Max(2, offset.x), 2, Math.Min(element.offsetWidth, element.offsetParent.offsetWidth), element.offsetHeight);

                    if (element.offsetWidth <= 0 || element.offsetHeight <= 0)
                        return null;

                    Bitmap cropped = ImageHelper2.CropBitmap(snapshotAfter, elementRect);
                    //cropped.Save(@"c:\temp\snapshot" + element.id + ".cropped.bmp");
                    return cropped;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to get element preview image for id " + element.id + ": " + ex);
                return null;
            }
        }

        private bool TimedOut
        {
            get
            {
                return DateTime.Now > _timeoutTime;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_contentFile != null && File.Exists(_contentFile))
                {
                    try { File.Delete(_contentFile); }
                    catch { }
                }

                try { _browserControl.Dispose(); }
                catch { }
            }
            base.Dispose(disposing);
        }

        private ExplorerBrowserControl _browserControl;
        private string _contentFile;

        private HtmlScreenCaptureCore _htmlScreenCaptureCore;
        private DateTime _timeoutTime;

        private const int WAIT_INTERVAL = 100;
    }
}
