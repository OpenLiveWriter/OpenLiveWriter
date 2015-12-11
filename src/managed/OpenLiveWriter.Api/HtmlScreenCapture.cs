// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Net;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Provides the ability to capture HTML content into a Bitmap.
    /// </summary>
    public class HtmlScreenCapture
    {
        /// <summary>
        /// Initialize a screen capture for an HTML page located at a URL.
        /// </summary>
        /// <param name="url">Url of HTML page to capture.</param>
        /// <param name="contentWidth">Width of content.</param>
        public HtmlScreenCapture(Uri url, int contentWidth)
        {
            _htmlScreenCapture = new HtmlScreenCaptureCore(url, contentWidth);
            SubscribeToEvents();
        }

        /// <summary>
        /// Initialize a screen capture for a snippet of HTML content.
        /// </summary>
        /// <param name="htmlContent">HTML snippet to capture.</param>
        /// <param name="contentWidth">Width of content.</param>
        public HtmlScreenCapture(string htmlContent, int contentWidth)
        {
            _htmlScreenCapture = new HtmlScreenCaptureCore(htmlContent, contentWidth);
            SubscribeToEvents();
        }

        /// <summary>
        /// Maximum height to capture. If this value is not specified then
        /// the height will be determined by the size of the page or HTML snippet.
        /// </summary>
        public int MaximumHeight
        {
            get { return _htmlScreenCapture.MaximumHeight; }
            set { _htmlScreenCapture.MaximumHeight = value; }
        }

        /// <summary>
        /// Indicates that the HTML document to be captured is available. This event
        /// allows subscribers to examine the document in order to determine whether
        /// the page is fully loaded and ready for capture).
        /// </summary>
        public event HtmlDocumentAvailableHandler HtmlDocumentAvailable;

        /// <summary>
        /// Indicates that a a candidate screen capture is available. This event
        /// allows subscribers to examine the screen capture bitmap in order to determine
        /// whether the page is fully loaded and ready for capture.
        /// </summary>
        public event HtmlScreenCaptureAvailableHandler HtmlScreenCaptureAvailable;

        /// <summary>
        /// Perform an HTML screen capture.
        /// </summary>
        /// <param name="timeoutMs">Timeout (in ms). A timeout value greater than 0 must be specified for all screen captures.</param>
        /// <returns>Bitmap containing captured HTML (or null if a timeout occurred).</returns>
        public Bitmap CaptureHtml(int timeoutMs)
        {
            return _htmlScreenCapture.CaptureHtml(timeoutMs);
        }

        private void SubscribeToEvents()
        {
            _htmlScreenCapture.HtmlDocumentAvailable += new HtmlDocumentAvailableHandlerCore(_htmlScreenCapture_HtmlDocumentAvailable);
            _htmlScreenCapture.HtmlScreenCaptureAvailable += new HtmlScreenCaptureAvailableHandlerCore(_htmlScreenCapture_HtmlScreenCaptureAvailable);
        }

        private void _htmlScreenCapture_HtmlDocumentAvailable(object sender, HtmlDocumentAvailableEventArgsCore e)
        {
            if (HtmlDocumentAvailable != null)
            {
                HtmlDocumentAvailableEventArgs ea = new HtmlDocumentAvailableEventArgs(e.Document);
                HtmlDocumentAvailable(this, ea);
                e.DocumentReady = ea.DocumentReady;
            }
        }

        private void _htmlScreenCapture_HtmlScreenCaptureAvailable(object sender, HtmlScreenCaptureAvailableEventArgsCore e)
        {
            if (HtmlScreenCaptureAvailable != null)
            {
                HtmlScreenCaptureAvailableEventArgs ea = new HtmlScreenCaptureAvailableEventArgs(e.Bitmap);
                HtmlScreenCaptureAvailable(this, ea);
                e.CaptureCompleted = ea.CaptureCompleted;
            }
        }

        private HtmlScreenCaptureCore _htmlScreenCapture;
    }

    /// <summary>
    /// Provides data for the HtmlScreenCaptureAvailable event.
    /// </summary>
    public class HtmlScreenCaptureAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize a new instance of HtmlScreenCaptureAvailableEventArgs using the specified Bitmap.
        /// </summary>
        /// <param name="bitmap">Currently available HTML screen shot.</param>
        public HtmlScreenCaptureAvailableEventArgs(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        /// <summary>
        /// Currently available HTML screen shot.
        /// </summary>
        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }
        private Bitmap _bitmap;

        /// <summary>
        /// Value indicating whether the screen capture has been completed. Set this value to
        /// false to indicate that the screen capture is not yet completed. This property is useful
        /// in the case where the content to be captured has a secondary loading step (such as
        /// a media player loading a video) which must occur before the screen capture is completed.
        /// </summary>
        public bool CaptureCompleted
        {
            get { return _captureCompleted; }
            set { _captureCompleted = value; }
        }
        private bool _captureCompleted = true;
    }

    /// <summary>
    /// Provides date for the HtmlDocumentAvailable event.
    /// </summary>
    public class HtmlDocumentAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize a new instance of HtmlDocumentAvailableEventArgs with the specified document.
        /// </summary>
        /// <param name="document">HTML document (guaranteed to be castable to an IHTMLDocument2)</param>
        public HtmlDocumentAvailableEventArgs(object document)
        {
            _document = document;
        }

        /// <summary>
        /// HTML document (guaranteed to be castable to an IHTMLDocument2)
        /// </summary>
        public object Document
        {
            get { return _document; }
        }
        private object _document;

        /// <summary>
        /// Value indicating whether the document is ready for a screen capture. Set this value
        /// to false to indicate that the document is not yet ready. This is useful for HTML
        /// documents that load in stages, such as documents that use embedded JavaScript to
        /// fetch and render additional content after the main document has loaded.
        /// </summary>
        public bool DocumentReady
        {
            get { return _documentReady; }
            set { _documentReady = value; }
        }
        private bool _documentReady = true;

    }

    /// <summary>
    /// Represents the method that will handle the HtmlScreenCaptureAvailable event of the HtmlScreenCapture class.
    /// </summary>
    public delegate void HtmlScreenCaptureAvailableHandler(object sender, HtmlScreenCaptureAvailableEventArgs e);

    /// <summary>
    /// Represents the method that will handle the HtmlDocumentAvailable event of the HtmlScreenCapture class.
    /// </summary>
    public delegate void HtmlDocumentAvailableHandler(object sender, HtmlDocumentAvailableEventArgs e);

}
