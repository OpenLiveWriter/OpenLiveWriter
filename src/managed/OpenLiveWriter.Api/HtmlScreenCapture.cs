// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace OpenLiveWriter.Api
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Drawing;

    using JetBrains.Annotations;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Provides the ability to capture HTML content into a Bitmap.
    /// </summary>
    public class HtmlScreenCapture
    {
        /// <summary>
        /// The HTML screen capture
        /// </summary>
        [NotNull]
        private readonly HtmlScreenCaptureCore htmlScreenCapture;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlScreenCapture"/> class.
        /// </summary>
        /// <param name="url">
        /// Url of HTML page to capture.
        /// </param>
        /// <param name="contentWidth">
        /// Width of content.
        /// </param>
        public HtmlScreenCapture([NotNull] Uri url, int contentWidth)
        {
            this.htmlScreenCapture = new HtmlScreenCaptureCore(url, contentWidth);
            this.SubscribeToEvents();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlScreenCapture"/> class.
        /// </summary>
        /// <param name="htmlContent">
        /// HTML snippet to capture.
        /// </param>
        /// <param name="contentWidth">
        /// Width of content.
        /// </param>
        public HtmlScreenCapture([NotNull] string htmlContent, int contentWidth)
        {
            this.htmlScreenCapture = new HtmlScreenCaptureCore(htmlContent, contentWidth);
            this.SubscribeToEvents();
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
        /// Gets or sets the maximum height to capture. If this value is not specified then
        /// the height will be determined by the size of the page or HTML snippet.
        /// </summary>
        public int MaximumHeight
        {
            get
            {
                return this.htmlScreenCapture.MaximumHeight;
            }

            set
            {
                this.htmlScreenCapture.MaximumHeight = value;
            }
        }

        /// <summary>
        /// Perform an HTML screen capture.
        /// </summary>
        /// <param name="timeoutMs">Timeout (in milliseconds). A timeout value greater than 0 must be specified for all screen captures.</param>
        /// <returns>Bitmap containing captured HTML (or null if a timeout occurred).</returns>
        [NotNull]
        public Bitmap CaptureHtml(int timeoutMs) => this.htmlScreenCapture.CaptureHtml(timeoutMs);

        /// <summary>
        /// HTMLs the screen capture HTML document available.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void HtmlScreenCaptureHtmlDocumentAvailable(
            [NotNull] object sender,
            [NotNull] HtmlDocumentAvailableEventArgsCore e)
        {
            if (this.HtmlDocumentAvailable == null)
            {
                return;
            }

            var ea = new HtmlDocumentAvailableEventArgs(e.Document);
            this.HtmlDocumentAvailable(this, ea);
            e.DocumentReady = ea.DocumentReady;
        }

        /// <summary>
        /// HTMLs the screen capture HTML screen capture available.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void HtmlScreenCaptureHtmlScreenCaptureAvailable(
            [NotNull] object sender,
            [NotNull] HtmlScreenCaptureAvailableEventArgsCore e)
        {
            if (this.HtmlScreenCaptureAvailable == null)
            {
                return;
            }

            var ea = new HtmlScreenCaptureAvailableEventArgs(e.Bitmap);
            this.HtmlScreenCaptureAvailable(this, ea);
            e.CaptureCompleted = ea.CaptureCompleted;
        }

        /// <summary>
        /// This method is used to define code contract statements.
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.htmlScreenCapture != null);
        }

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        private void SubscribeToEvents()
        {
            this.htmlScreenCapture.HtmlDocumentAvailable += new HtmlDocumentAvailableHandlerCore(this.HtmlScreenCaptureHtmlDocumentAvailable);
            this.htmlScreenCapture.HtmlScreenCaptureAvailable += new HtmlScreenCaptureAvailableHandlerCore(this.HtmlScreenCaptureHtmlScreenCaptureAvailable);
        }
    }
}
