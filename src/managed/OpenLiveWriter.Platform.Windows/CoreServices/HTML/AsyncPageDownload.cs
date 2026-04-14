// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using mshtml;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace Project31.CoreServices
{
    /// <summary>
    /// Summary description for AsyncPageDownload.
    /// </summary>
    public class AsyncPageDownload : AsyncOperation
    {

        /// <summary>
        /// Downloads a page and all its references into a Site Storage
        /// </summary>
        /// <param name="url">The url of the page to download</param>
        /// <param name="storage">The storage into which to store the page and its references</param>
        /// <param name="rootFileName">The rootfile name for the ISiteStorage</param>
        /// <param name="target">An object implementing the
        /// ISynchronizeInvoke interface.  All events will be delivered
        /// through this target, ensuring that they are delivered to the
        /// correct thread.</param>
        public AsyncPageDownload(
            string url,
            ISiteStorage storage,
            string rootFileName,
            ISynchronizeInvoke target) : this (url, storage, rootFileName, "", target)
        {
        }

        /// <summary>
        /// Downloads a page and all its references into a Site Storage
        /// </summary>
        /// <param name="document">The IHTMLDocument2 to storage with its references</param>
        /// <param name="storage">The storage into which to store the page and its references</param>
        /// <param name="footFileName">The rootfile name for the ISiteStorage</param>
        /// <param name="target">An object implementing the
        /// ISynchronizeInvoke interface.  All events will be delivered
        /// through this target, ensuring that they are delivered to the
        /// correct thread.</param>
        public AsyncPageDownload(IHTMLDocument2 document,
            ISiteStorage storage, string rootFileName, ISynchronizeInvoke target) :
            this (document, storage, rootFileName, "", target)
        {
        }

        /// <summary>
        /// Actually downloads the files (note that it is synchronous)
        /// </summary>
        protected override void DoWork()
        {
            if (CancelRequested)
            {
                AcknowledgeCancel();
                return;
            }

            // If the base document hasn't been populated, go get it
            if (m_htmlDocument == null)
                m_htmlDocument = HTMLDocumentHelper.GetHTMLDocFromURL(m_url);

            if (CancelRequested)
            {
                AcknowledgeCancel();
                return;
            }

            // Get a list of referenced URLs from the document
            Hashtable urlList = HTMLDocumentHelper.GetResourceUrlsFromDocument(m_htmlDocument);

            if (CancelRequested)
            {
                AcknowledgeCancel();
                return;
            }

            // Get the HTML from this document- we'll use this as the base HTML and replace
            // paths inside of it.
            string finalHTML = HTMLDocumentHelper.HTMLDocToString(m_htmlDocument);

            IEnumerator urlEnum = urlList.GetEnumerator();
            while (urlEnum.MoveNext())
            {
                DictionaryEntry element = (DictionaryEntry) urlEnum.Current;
                string url = (string)element.Key;
                string urlType = (string)element.Value;

                string fullUrl = HTMLDocumentHelper.EscapeRelativeURL(m_url, url);
                string fileName = FileHelper.GetValidFileName(Path.GetFileName(new Uri(fullUrl).AbsolutePath));
                string relativePath;

                if (fileName != string.Empty)
                {
                    if (urlType != HTMLTokens.Frame && urlType != HTMLTokens.IFrame)
                    {
                        relativePath = "referencedFiles/" + fileName;
                        WebRequestWithCache request = new WebRequestWithCache(fullUrl);

                        // Add the html document to the site Storage.
                        using (Stream requestStream = request.GetResponseStream())
                        {
                            if (requestStream != null)
                            {
                                using (Stream fileStream =
                                            m_siteStorage.Open(m_rootPath + relativePath, AccessMode.Write))
                                {
                                    StreamHelper.Transfer(requestStream, fileStream, 8192, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        fileName = Path.GetFileNameWithoutExtension(fileName) + ".htm";
                        relativePath = "referencedFiles/" + fileName;

                        AsyncPageDownload frameDownload =
                            new AsyncPageDownload(fullUrl,
                                    m_siteStorage,
                                    fileName,
                                    m_rootPath + "referencedFiles/",
                                    this.Target);
                        frameDownload.Start();
                        frameDownload.WaitUntilDone();

                        // Regular expressions would allow more flexibility here, but note that
                        // characters like ? / & have meaning in regular expressions and so need
                        // to be escaped
                    }
                    finalHTML = finalHTML.Replace(UrlHelper.CleanUpUrl(url), relativePath);
                }
                if (CancelRequested)
                {
                    AcknowledgeCancel();
                    return;
                }
            }

            // Escape any high ascii characters
            finalHTML = HTMLDocumentHelper.EscapeHighAscii(finalHTML.ToCharArray());

            // Add the html document to the site Storage.
            Stream  htmlStream = m_siteStorage.Open(m_rootPath + m_rootFile, AccessMode.Write);

            using (StreamWriter writer = new StreamWriter(htmlStream, Encoding.UTF8))
            {
                writer.Write(finalHTML);
            }
            m_siteStorage.RootFile = m_rootFile;
        }

        /// <summary>
        /// Downloads a page and all its references into a Site Storage
        /// </summary>
        /// <param name="url">The url of the page to download</param>
        /// <param name="storage">The storage into which to store the page and its references</param>
        /// <param name="footFileName">The rootfile name for the ISiteStorage</param>
        /// <param name="storageRootPath">The path in which to place the items in the ISiteStorage</param>
        /// <param name="target">An object implementing the
        /// ISynchronizeInvoke interface.  All events will be delivered
        /// through this target, ensuring that they are delivered to the
        /// correct thread.</param>
        private AsyncPageDownload(
            string url,
            ISiteStorage storage,
            string rootFileName,
            string storageRootPath,
            ISynchronizeInvoke target) : base (target)
        {
            m_url = url;
            m_siteStorage = storage;
            m_rootFile = rootFileName;
            m_rootPath = storageRootPath;
        }


        /// <summary>
        /// Downloads a page and all its references into a Site Storage
        /// </summary>
        /// <param name="document">The IHTMLDocument2 to storage with its references</param>
        /// <param name="storage">The storage into which to store the page and its references</param>
        /// <param name="footFileName">The rootfile name for the ISiteStorage</param>
        /// <param name="storageRootPath">The path in which to place the items in the ISiteStorage</param>
        /// <param name="target">An object implementing the
        /// ISynchronizeInvoke interface.  All events will be delivered
        /// through this target, ensuring that they are delivered to the
        /// correct thread.</param>
        private AsyncPageDownload(IHTMLDocument2 document,
            ISiteStorage storage, string rootFileName, string storageRootPath, ISynchronizeInvoke target) :
            base (target)
        {
            m_url = document.url;
            m_htmlDocument = document;
            m_siteStorage = storage;
            m_rootFile = rootFileName;
            m_rootPath = storageRootPath;
        }

        /// <summary>
        /// The base IHTMLDocument2
        /// </summary>
        private IHTMLDocument2 m_htmlDocument;

        /// <summary>
        /// The Site Storage into which to write the page and contents
        /// </summary>
        private ISiteStorage m_siteStorage;

        /// <summary>
        /// The url of the root document
        /// </summary>
        private string m_url;

        /// <summary>
        /// The rootfile name for this document
        /// </summary>
        private string m_rootFile;

        /// <summary>
        /// The base bath in the storage
        /// </summary>
        private string m_rootPath;
    }

}
