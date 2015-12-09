// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for URLData.
    /// </summary>
    public class URLData
    {

        /// <summary>
        /// Creates a URLData based upon an IDataObject
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the URLData</param>
        /// <returns>The URLData, null if no URLData could be created</returns>
        public static URLData Create(IDataObject iDataObject)
        {
            // WinLive Bug 198371: Look at rolling the fix for WinLive 182698 into the OleDataObjectHelper.GetDataPresentSafe method
            // For now, we keep the changes targeted.
            bool canGetDataPresentDirectlyFromIDataObject = false;

            try
            {
                canGetDataPresentDirectlyFromIDataObject = iDataObject.GetDataPresent(DataFormatsEx.URLFormat) &&
                                                           iDataObject.GetDataPresent(DataFormatsEx.FileGroupDescriptorWFormat);
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
            }

            // Validate required format
            // WinLive Bug 182698: Assert when pasting a hyperlink from IE
            if (canGetDataPresentDirectlyFromIDataObject &&
                OleDataObjectHelper.GetDataPresentSafe(iDataObject, DataFormatsEx.URLFormat) &&
                OleDataObjectHelper.GetDataPresentSafe(iDataObject, DataFormatsEx.FileGroupDescriptorWFormat))
                return new URLData(iDataObject);
            else
            {
                //check to see if the dataObject contains a single .url file,
                //if so, create the URLData from that.
                FileData fileData = FileData.Create(iDataObject);
                if (fileData != null && fileData.Files.Length == 1)
                {
                    string filePath = fileData.Files[0].ContentsPath;
                    if (PathHelper.IsPathUrlFile(filePath))
                    {
                        URLData urlData = new URLData(iDataObject, UrlHelper.GetUrlFromShortCutFile(filePath), Path.GetFileNameWithoutExtension(filePath));
                        urlData.DateCreated = File.GetCreationTime(filePath);
                        urlData.DateModified = File.GetLastWriteTime(filePath);
                        return urlData;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Reads a URL string from the UniformResourceLocator format of an IDataObject
        /// </summary>
        /// <returns>The URL, or null if no URL could be returned from the IDataObject's
        /// UniformResourceLocator format</returns>
        public string URL
        {
            get
            {
                if (m_url == null)
                {
                    // Read the URL into a string
                    Stream stream = (Stream)m_dataObject.GetData(DataFormatsEx.URLFormat);
                    StreamReader reader = new StreamReader(stream);

                    using (reader)
                    {
                        m_url = reader.ReadToEnd().Trim((char)0);
                    }
                }
                return m_url;
            }
        }
        private string m_url;

        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set { _dateCreated = value; }
        }
        private DateTime _dateCreated;

        public DateTime DateModified
        {
            get { return _dateModified; }
            set { _dateModified = value; }
        }

        private DateTime _dateModified;

        /// <summary>
        /// Gets the title of a URL by inspecting the accompanying file drop
        /// </summary>
        /// <returns>The title, empty string if no title could be found</returns>
        public string Title
        {
            get
            {
                if (m_title == null)
                {
                    // Get the bytes from the stream
                    Stream memoryStream =
                        (Stream)m_dataObject.GetData(DataFormatsEx.FileGroupDescriptorWFormat);
                    byte[] memoryStreamBytes = new Byte[memoryStream.Length];

                    using (memoryStream)
                    {
                        memoryStream.Read(memoryStreamBytes, 0, (int)memoryStream.Length);
                    }

                    // Just get the characters at this byte position in the stream, until the end
                    // of the null terminated string.
                    const int FILENAME_OFFSET = 76;
                    const int BYTE_INCREMENT = 2;
                    char currentChar;
                    StringBuilder sBuilder = new StringBuilder();

                    // Read character by character into a string
                    int i = FILENAME_OFFSET;
                    currentChar = BitConverter.ToChar(memoryStreamBytes, i);
                    while (currentChar != (char)0)
                    {
                        sBuilder.Append(currentChar);

                        i = i + BYTE_INCREMENT;
                        currentChar = BitConverter.ToChar(memoryStreamBytes, i);
                    }

                    m_title = Path.GetFileNameWithoutExtension(FileHelper.GetValidFileName(sBuilder.ToString()));
                }
                return m_title;
            }
        }
        private string m_title;

        /// <summary>
        /// Constructor for URLData
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the URLData</param>
        private URLData(IDataObject iDataObject)
        {
            m_dataObject = iDataObject;
            _dateCreated = DateTime.Now;
            _dateModified = DateTime.Now;
        }

        private URLData(IDataObject iDataObject, string url, string title) : this(iDataObject)
        {
            m_url = url;
            m_title = title;
        }
        private IDataObject m_dataObject;
    }
}
