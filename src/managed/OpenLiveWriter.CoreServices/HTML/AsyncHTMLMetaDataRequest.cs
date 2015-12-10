// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using mshtml;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// AsyncHTMLMetaDataRequest is an asynchronous mechanism for retrieving
    /// MetaData for a given URL.
    /// </summary>
    public class AsyncHTMLMetaDataRequest
    {
        /// <summary>
        /// Constructs a new Asynchronous HTML MetaDataRequest
        /// </summary>
        /// <param name="url"></param>
        public AsyncHTMLMetaDataRequest(string url)
        {
            m_url = url;
            m_webRequest = new AsyncWebRequestWithCache(url);
        }

        /// <summary>
        /// The MetaData.  This is null until the MetaData has been retrieved
        /// </summary>
        public HTMLMetaData MetaData = null;

        /// <summary>
        /// Event that is fired when the MetaData has been retrieved
        /// </summary>
        public event EventHandler MetaDataComplete;
        protected void OnMetaDataComplete(EventArgs e)
        {
            if (MetaDataComplete != null)
                MetaDataComplete(this, e);
        }

        /// <summary>
        /// Begins the metadata retrieval for a url
        /// </summary>
        public void BeginGetMetaData()
        {
            isRunning = true;
            m_webRequest.RequestComplete += new EventHandler(WebRequestComplete);
            m_webRequest.StartRequest();
        }

        /// <summary>
        /// Cancels the retrieval of meta data
        /// </summary>
        public void Cancel()
        {
            if (isRunning)
                m_webRequest.Cancel();
        }

        /// <summary>
        /// Event handler that is called when the Async Web request is complete
        /// </summary>
        private void WebRequestComplete(object send, EventArgs e)
        {
            IHTMLDocument2 htmlDoc = null;
            Stream stream = m_webRequest.ResponseStream;
            if (stream != null)
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    htmlDoc = HTMLDocumentHelper.StringToHTMLDoc(reader.ReadToEnd(), m_url);
                }
                if (htmlDoc != null)
                {
                    MetaData = new HTMLMetaData(htmlDoc);
                }
            }
            FireMetaDataComplete();
        }

        /// <summary>
        /// Helper method that notifies of request completion
        /// </summary>
        private void FireMetaDataComplete()
        {
            OnMetaDataComplete(EventArgs.Empty);
            isRunning = false;
        }

        private bool isRunning = false;
        private AsyncWebRequestWithCache m_webRequest;
        private string m_url;
    }
}
