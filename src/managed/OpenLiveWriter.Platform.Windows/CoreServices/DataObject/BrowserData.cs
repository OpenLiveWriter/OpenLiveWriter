// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Interop.SHDocVw;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for BrowserData.
    /// </summary>
    public class BrowserData
    {

        /// <summary>
        /// Creates a BrowserData based upon an IDataObject
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the BrowserData</param>
        /// <returns>The BrowserData, null if no BrowserData could be created</returns>
        public static BrowserData Create(IDataObject iDataObject)
        {
            // Validate required format
            BrowserClipboardData browserClip = (BrowserClipboardData)iDataObject.GetData(FORMAT_NAME);
            if (browserClip != null)
            {
                return new BrowserData(iDataObject, browserClip);
            }
            else
                return null;
        }

        /// <summary>
        /// The IWebBrowser2 provided to the Meister
        /// </summary>
        public IWebBrowser2 WebBrowser
        {
            get
            {
                if (m_webBrowser == null)
                    m_webBrowser = (IWebBrowser2)m_dataObject.GetData(typeof(IWebBrowser2));
                return m_webBrowser;
            }
        }
        private IWebBrowser2 m_webBrowser;

        public bool IsFormPost
        {
            get
            {
                return m_isFormPost;
            }
        }
        private bool m_isFormPost;

        /// <summary>
        /// The browser's current url
        /// </summary>
        public string Url
        {
            get
            {
                string url = null;
                if (m_htmlDocument != null)
                    url = m_htmlDocument.url;

                if (url == null || url == string.Empty)
                    url = m_webBrowser.LocationURL;

                return url;
            }
        }

        /// <summary>
        /// The title of the current document
        /// </summary>
        public string Title
        {
            get
            {
                string title = null;
                if (m_htmlDocument != null)
                    title = m_htmlDocument.title;

                if (title == null || title == string.Empty)
                    title = m_webBrowser.LocationName;

                return title;
            }
        }

        /// <summary>
        /// The metadata associated with the browser instance
        /// </summary>
        public HTMLMetaData HTMLMetaData
        {
            get
            {
                if (m_htmlMetaData == null && HTMLDocument != null)
                    m_htmlMetaData = new HTMLMetaData(HTMLDocument);
                return m_htmlMetaData;
            }
        }
        private HTMLMetaData m_htmlMetaData;

        public LightWeightHTMLDocument LightWeightDocument
        {
            get
            {
                if (_lightWeightDocument == null && HTMLDocument != null && Url != null)
                    _lightWeightDocument = LightWeightHTMLDocument.FromIHTMLDocument2(HTMLDocument, Url);
                return _lightWeightDocument;
            }
        }
        private LightWeightHTMLDocument _lightWeightDocument = null;

        /// <summary>
        /// The IHTMLDocument2 of the document currently loaded in the Browser
        /// </summary>
        private IHTMLDocument2 HTMLDocument
        {
            get
            {
                return m_htmlDocument;
            }
        }
        private IHTMLDocument2 m_htmlDocument;

        public static string FORMAT_NAME = "IWebBrowser2";

        public class BrowserClipboardData
        {
            /// <summary>
            /// Browser Clipboard data
            /// </summary>
            /// <param name="browser">The IWebBrowser2</param>
            /// <param name="htmlDocument">The IHTMLDocument2.  This is present because directly accessing the HTMLDocument
            /// from the browser itself can cause problems with PDFs in particular.  Use MindShareBrowserUtils to safely
            /// fetch the HTMLDocument and pass it in.</param>
            /// <param name="isFormPost"></param>
            public BrowserClipboardData(IWebBrowser2 browser, IHTMLDocument2 htmlDocument, bool isFormPost)
            {
                this.Browser = browser;
                this.IsFormPost = isFormPost;
                this.htmlDocument = htmlDocument;
            }

            public IWebBrowser2 Browser;
            public bool IsFormPost = false;
            public IHTMLDocument2 htmlDocument;
        }

        /// <summary>
        /// Constructor for BrowserData
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the BrowserData</param>
        private BrowserData(IDataObject iDataObject, BrowserClipboardData browserClip)
        {
            m_dataObject = iDataObject;
            m_webBrowser = browserClip.Browser;
            m_isFormPost = browserClip.IsFormPost;
            m_htmlDocument = browserClip.htmlDocument;
        }
        private IDataObject m_dataObject;
    }
}
