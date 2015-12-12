// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for DataObjectMeister.
    /// </summary>
    public class DataObjectMeister
    {
        /// <summary>
        /// DataObjectMeister provides a wrapper around a data object that exposes
        /// the data in various IDataObject formats as properties.
        /// </summary>
        /// <param name="ido">The data object from which to construct
        /// the DataObjectMeister</param>
        public DataObjectMeister(IDataObject ido)
        {
            m_dataObject = ido;
        }

        /// <summary>
        /// Dip directly in and grab the underlying data object (provided so we can
        /// the Meister into a method that needs both its high-level data parsing
        /// as well as low-level access to the underlying data object
        /// </summary>
        public IDataObject IDataObject
        {
            get
            {
                return m_dataObject;
            }
        }

        public BrowserData BrowserData
        {
            get
            {
                if (!haveAttemptedBrowserCreate)
                {
                    m_browserData = BrowserData.Create(m_dataObject);
                    haveAttemptedBrowserCreate = true;
                }
                return m_browserData;
            }
        }
        private BrowserData m_browserData;
        private bool haveAttemptedBrowserCreate = false;

        /// <summary>
        /// The html data associated with the IDataObject.  This property will be
        /// null if there is no file data associated with the IDataObject.
        /// </summary>
        public HTMLData HTMLData
        {
            get
            {
                if (!haveAttemptedHTMLCreate)
                {
                    m_htmlData = HTMLData.Create(m_dataObject);
                    haveAttemptedHTMLCreate = true;
                }

                return m_htmlData;
            }
        }
        private HTMLData m_htmlData;
        private bool haveAttemptedHTMLCreate = false;

        /// <summary>
        /// The file data associated with the IDataObject.  This property will be
        /// null if there is no file data associated with the IDataObject.
        /// </summary>
        public FileData FileData
        {
            get
            {
                if (!haveAttemptedFileCreate)
                {
                    m_fileData = FileData.Create(m_dataObject);
                    haveAttemptedFileCreate = true;
                }
                return m_fileData;
            }
        }
        private FileData m_fileData;
        private bool haveAttemptedFileCreate = false;

        /// <summary>
        /// The url data associated with the IDataObject.  This property will be
        /// null if there is no file data associated with the IDataObject.
        /// </summary>
        public URLData URLData
        {
            get
            {
                if (!haveAttempedURLCreate)
                {
                    m_urlData = URLData.Create(m_dataObject);
                    haveAttempedURLCreate = true;
                }
                return m_urlData;
            }
        }
        private URLData m_urlData;
        private bool haveAttempedURLCreate = false;

        /// <summary>
        /// The text data associated with the IDataObject.  This property will be
        /// null if there is no file data associated with the IDataObject.
        /// </summary>
        public TextData TextData
        {
            get
            {
                if (!haveAttemptedTextCreate)
                {
                    m_textData = TextData.Create(m_dataObject);
                    haveAttemptedTextCreate = true;
                }
                return m_textData;
            }
        }
        private TextData m_textData;
        private bool haveAttemptedTextCreate = false;

        /// <summary>
        /// The text data associated with the IDataObject.  This property will be
        /// null if there is no file data associated with the IDataObject.
        /// </summary>
        public ImageData ImageData
        {
            get
            {
                if (!haveAttemptedImageCreate)
                {
                    m_imageData = ImageData.Create(m_dataObject);
                    haveAttemptedImageCreate = true;
                }
                return m_imageData;
            }
        }
        private ImageData m_imageData;
        private bool haveAttemptedImageCreate = false;

        public HTMLMetaData GetMetaDataFromCache(string url)
        {
            if (!m_metaData.ContainsKey(url))
            {
                HTMLMetaDataRequest request = new HTMLMetaDataRequest(url);
                m_metaData[url] = request.GetMetaDataFromCache();
            }
            return (HTMLMetaData)m_metaData[url];
        }
        private Hashtable m_metaData = new Hashtable();

        public LightWeightHTMLDocumentData LightWeightHTMLDocumentData
        {
            get
            {
                if (!haveAttemptedLightWeightCreate)
                {
                    m_lightWeightHTMLDocumentData = LightWeightHTMLDocumentData.Create(m_dataObject);
                    haveAttemptedLightWeightCreate = true;
                }
                return m_lightWeightHTMLDocumentData;
            }
        }
        private LightWeightHTMLDocumentData m_lightWeightHTMLDocumentData;
        private bool haveAttemptedLightWeightCreate = false;

        public LiveClipboardData LiveClipboardData
        {
            get
            {
                if (!haveAttemptedLiveClipboardCreate)
                {
                    _liveClipboardData = LiveClipboardData.Create(m_dataObject);
                    haveAttemptedLiveClipboardCreate = true;
                }
                return _liveClipboardData;
            }
        }
        private LiveClipboardData _liveClipboardData;
        private bool haveAttemptedLiveClipboardCreate = false;

        /// <summary>
        /// The DataObjectMeister's underlying IDataObject.
        /// </summary>
        private IDataObject m_dataObject;
    }
}
