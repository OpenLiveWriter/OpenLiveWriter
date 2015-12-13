// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// HTMLData is a Mindshare Data Object that conatins HTML text fragments.
    /// It includes an html document based upon the HTML DOM (IHTMLDocument2) as well
    /// as various properties derived from the HTML fragment contained in the IDataObject.
    /// </summary>
    public class HTMLData
    {

        /// <summary>
        /// Attempts to create a new HTMLData.  This can return null if the DataObject
        /// couldn't be created based upon the IDataObject.
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the HTML Data Object</param>
        /// <returns>The HTMLData, null if it couldn't be created.</returns>
        public static HTMLData Create(IDataObject iDataObject)
        {
            string[] loser = iDataObject.GetFormats();

            if (OleDataObjectHelper.GetDataPresentSafe(iDataObject, DataFormats.Html))
            {

                try
                {
                    HTMLData data = new HTMLData(iDataObject, null);
                    return string.IsNullOrEmpty(data.HTML) ? null : data;
                }
                catch (FormatException)
                {
                    // EML files with HTML inside of them report that they are HTML
                    // However, when we try to read the format, we have problems reading it
                    // So we will skip loading html that we cannot load
                    return null;
                }

            }
            else if (HtmlDocumentClassFormatPresent(iDataObject))
            {
                return new HTMLData(iDataObject, (IHTMLDocument2)iDataObject.GetData(typeof(HTMLDocumentClass)));
            }
            else
                return null;
        }

        private static bool HtmlDocumentClassFormatPresent(IDataObject iDataObject)
        {
            return OleDataObjectHelper.GetDataPresentSafe(iDataObject, typeof(HTMLDocumentClass));
        }

        /// <summary>
        /// The source url from which the HTML text fragment contained in the IDataObject
        /// was taken.
        /// </summary>
        public string SourceURL
        {
            get
            {
                if (m_sourceURL == null)
                    m_sourceURL = GetHTMLFormatHeader(HTMLDataObject.Headers.SourceURL);
                return m_sourceURL;
            }
        }
        private string m_sourceURL = null;

        /// <summary>
        /// The HTML string from an IDataObject (uses the HTML clipboard format)
        /// The HTML clipboard format includes a complete syntactically valid HTML DOM
        /// as well as markers indicating selected text.
        /// </summary>
        public string HTML
        {
            get
            {
                if (m_HTML == null)
                {
                    // UTF8-decode the bytes and get the characters
                    // Escape any high ascii characters to the valid HTML views
                    m_HTML =
                        HTMLDataObject.StripFragmentMarkers(
                        HTMLDocumentHelper.EscapeHighAscii(
                            Encoding.UTF8.GetChars(HTMLFormatBytes,
                        StartHTML,
                        EndHTML - StartHTML)));
                }

                return m_HTML;
            }
        }
        private string m_HTML = null;

        /// <summary>
        /// Returns the selected HTML text as a string from an IDataObject (using the
        /// HTML clipboard format).
        /// </summary>
        public string HTMLSelection
        {
            get
            {
                if (m_HTMLSelection == null)
                {
                    // Get the selected text
                    // Use fragment instead of selection! (Fragment creates a complete valid HTML fragment
                    // while selection is the literally interpreted selection which could be missing
                    // open or closing tags).
                    m_HTMLSelection =
                        HTMLDataObject.StripFragmentMarkers(
                        HTMLDocumentHelper.EscapeHighAscii(
                        Encoding.UTF8.GetChars(HTMLFormatBytes,
                        StartFragment,
                        EndFragment - StartFragment)));
                }

                return m_HTMLSelection;

            }
        }
        private string m_HTMLSelection = null;

        public HTMLMetaData HTMLMetaData
        {
            get
            {
                if (m_htmlMetaData == null)
                    m_htmlMetaData = new HTMLMetaData(HTMLDocument);
                return m_htmlMetaData;
            }
        }
        private HTMLMetaData m_htmlMetaData;

        /// <summary>
        /// Plain Text representation of the html selection
        /// </summary>
        public string SelectionPlainText
        {
            get
            {
                if (m_selectionPlainText == null)
                    m_selectionPlainText = HTMLDocumentHelper.HTMLToPlainText(HTMLSelection);
                return m_selectionPlainText;
            }
        }
        private string m_selectionPlainText;

        /// <summary>
        /// The title of the HTML Data.  This could be derived from the HTML document
        /// or from other data about the HTML.
        /// </summary>
        public string Title
        {
            get
            {
                if (SelectionPlainText != string.Empty)
                    return TextHelper.GetTitleFromText(SelectionPlainText, TextHelper.TITLE_LENGTH, TextHelper.Units.Characters);
                else if (HTMLDocument.title != string.Empty)
                    return HTMLDocument.title;
                else
                    return string.Empty;
            }
        }

        public LightWeightHTMLDocument LightWeightDocument
        {
            get
            {
                if (_lightWeightDocument == null)
                {
                    if (HTML == null)
                        _lightWeightDocument = LightWeightHTMLDocument.FromIHTMLDocument2(HTMLDocument, SourceURL);
                    else
                        _lightWeightDocument = LightWeightHTMLDocument.FromString(HTML, SourceURL);
                }
                return _lightWeightDocument;
            }
        }
        private LightWeightHTMLDocument _lightWeightDocument = null;

        /// <summary>
        /// Converts an IDataObject that contains an HTML Format into
        /// an HTMLDocument2.
        /// </summary>
        public IHTMLDocument2 HTMLDocument
        {
            get
            {
                if (m_HTMLDocument == null)
                    m_HTMLDocument = HTMLDocumentHelper.StringToHTMLDoc(HTML, SourceURL, true);
                return m_HTMLDocument;
            }
        }
        private IHTMLDocument2 m_HTMLDocument = null;

        /// <summary>
        /// Converts an IDataObject that contains an HTML Format into with the HTML comment markers
        /// an HTMLDocument2.
        /// </summary>
        public IHTMLDocument2 HTMLDocumentWithMarkers
        {
            get
            {
                if (m_HTMLDocumentWithMarkers == null)
                    m_HTMLDocumentWithMarkers = HTMLDocumentHelper.StringToHTMLDoc(HTMLWithMarkers, SourceURL, true);
                return m_HTMLDocumentWithMarkers;
            }
        }
        private IHTMLDocument2 m_HTMLDocumentWithMarkers = null;

        /// <summary>
        /// Returns HTML with comment markers around the selected fragment from an IDataObject that contains an HTML Format.
        /// </summary>
        public string HTMLWithMarkers
        {
            get
            {
                if (m_HTMLWithMarkers == null)
                    m_HTMLWithMarkers = Encoding.UTF8.GetString(HTMLFormatBytes, StartHTML, EndHTML - StartHTML);
                return m_HTMLWithMarkers;
            }
        }
        private string m_HTMLWithMarkers = null;

        /// <summary>
        /// Gets a valid IHTMLDocument2 from the selected text contained
        /// </summary>
        public IHTMLDocument2 SelectionHTMLDocument
        {
            get
            {
                if (m_selectionHTMLDocument == null)
                    m_selectionHTMLDocument = HTMLDocumentHelper.StringToHTMLDoc(HTMLSelection, SourceURL);
                return m_selectionHTMLDocument;
            }
        }
        private IHTMLDocument2 m_selectionHTMLDocument = null;

        /// <summary>
        /// The url of a link the represents the only element in the selection
        /// </summary>
        public string OnlyLinkUrl
        {
            get
            {
                if (OnlyLink == null)
                    return null;

                if (m_onlyLinkUrl == null)
                    m_onlyLinkUrl = UrlHelper.EscapeRelativeURL(
                                        SourceURL,
                                        (string)OnlyLink.getAttribute(HTMLTokens.Href, 0));

                return m_onlyLinkUrl;
            }
        }
        private string m_onlyLinkUrl;

        /// <summary>
        /// The title of the link that is the only element in the selection
        /// </summary>
        public string OnlyLinkTitle
        {
            get
            {
                if (OnlyLink == null)
                    return null;

                if (m_onlyLinkTitle == null)
                {
                    m_onlyLinkTitle = (string)OnlyLink.innerText;
                }
                return m_onlyLinkTitle;
            }
        }
        private string m_onlyLinkTitle;

        /// <summary>
        /// The path to the image that represents the only element in the selection
        /// </summary>
        public string OnlyImagePath
        {
            get
            {
                if (m_onlyImagePath == null && OnlyImageElement != null)
                    m_onlyImagePath = UrlHelper.EscapeRelativeURL(
                                        SourceURL,
                                        (string)OnlyImageElement.getAttribute(HTMLTokens.Src, 0));
                return m_onlyImagePath;
            }
        }
        private string m_onlyImagePath;

        /// <summary>
        /// The alt text for the image that represents the only element in the selection
        /// </summary>
        public string OnlyImageAltText
        {
            get
            {
                if (m_onlyImageAltText == null)
                    m_onlyImageAltText = (string)OnlyImageElement.getAttribute(HTMLTokens.Alt, 0);
                return m_onlyImageAltText;
            }
        }
        private string m_onlyImageAltText;

        /// <summary>
        /// The title of the image that represents the only element in the selection
        /// </summary>
        public string OnlyImageTitle
        {
            get
            {
                if (m_onlyImageTitle == null && OnlyImageElement != null)
                {
                    m_onlyImageTitle = UrlHelper.GetFileNameWithoutExtensionForUrl(OnlyImagePath);
                    if (m_onlyImageTitle != null)
                        m_onlyImageTitle = HttpUtility.UrlDecode(m_onlyImageTitle);

                    if (m_onlyImageTitle == null || m_onlyImageTitle == string.Empty)
                        m_onlyImageTitle = OnlyImageAltText;

                    if (m_onlyImageTitle == null || m_onlyImageTitle == string.Empty)
                        m_onlyImageTitle = "img";

                }
                return m_onlyImageTitle;
            }
        }
        private string m_onlyImageTitle;

        /// <summary>
        /// The Html Element of the image that represents the only element in the selection
        /// </summary>
        public IHTMLElement OnlyImageElement
        {
            get
            {
                // Check to see if the selected HTML Dom includes only one image
                if (HTMLDocument.images.length == 1)
                {
                    IHTMLElement image = (IHTMLElement)HTMLDocument.images.item(0, 0);

                    // if the text inside the one image (nothing, basically) is identical to the text in the
                    // document's body, then we've selected just a single image!
                    if (NormalizeInnerText(HTMLDocument.body.innerText) == NormalizeInnerText(image.innerText))
                        m_onlyImageElement = image;
                }
                return m_onlyImageElement;
            }
        }
        private IHTMLElement m_onlyImageElement;

        private string NormalizeInnerText(string innerText)
        {
            if (innerText == null)
                return string.Empty;
            return innerText.Trim();
        }

        /// <summary>
        /// The Html Element of the link that represents the only element in the selection
        /// </summary>
        private IHTMLElement OnlyLink
        {
            get
            {
                if (m_rootLink == null)
                {
                    // If there is only a single link in the body of the html selection
                    if (HTMLDocument.links.length == 1 && HTMLDocument.body != null)
                    {
                        // Get the link
                        IHTMLElement link = (IHTMLElement)HTMLDocument.links.item(0, 0);

                        // if the text inside the one link is identical to the text in the
                        // document's body, then we've selected just a link!
                        string bodyText = (string)HTMLDocument.body.innerText;
                        string linkText = (string)link.innerText;
                        if ((bodyText != null && linkText != null) &&
                            (bodyText.Trim() == linkText.Trim()))
                            m_rootLink = link;
                    }
                }
                return m_rootLink;
            }
        }
        private IHTMLElement m_rootLink;

        /// <summary>
        /// Get the value of a given header in HTML clipboard formatted data
        /// </summary>
        /// <param name="headerName">The name of the header being requested</param>
        /// <returns>string, the value of the header</returns>
        private string GetHTMLFormatHeader(string headerName)
        {
            // If the header hasn't already been read and cached in the hash table
            if (m_HTMLFormatHeaders.Count == 0)
            {
                // get a string that represents the header text by reading until we hit the first tag
                IEnumerator byteEnum = HTMLFormatBytes.GetEnumerator();
                int i = 0;
                while (byteEnum.MoveNext())
                {
                    if (HTMLFormatBytes[i] == '<')
                        break;
                    i++;
                }

                // Since the headers are guaranteed to be low ascii, we can use ascii to
                // convert the bytes into a string
                string HeaderString = Encoding.ASCII.GetString(HTMLFormatBytes, 0, i);
                string[] lines = HeaderString.Split('\n');
                for (int it = 0; it < lines.Length; it++)
                {
                    string[] pair = lines[it].Split(new Char[] { ':' }, 2);
                    if (pair.Length == 2)
                    {
                        m_HTMLFormatHeaders.Add(pair[0].Trim(), pair[1].Trim());
                    }
                }
            }

            // If the header isn't in the table, just return an empty string
            string headerValue = string.Empty;
            if (m_HTMLFormatHeaders.Contains(headerName))
                headerValue = (string)m_HTMLFormatHeaders[headerName];
            return headerValue;
        }

        // Hashtable caches headers as they get read
        private Hashtable m_HTMLFormatHeaders = new Hashtable(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Helper function to extract and UTF8-decode the underlying CF_HTML.
        /// </summary>
        /// <returns>decoded CF_HTML</returns>
        public byte[] HTMLFormatBytes
        {
            get
            {
                if (m_HTMLBytes == null)
                {
                    // Not all dataobjects can be converted into OleDataObjects.
                    // We normally expect that objects that can't be converted are
                    // likely to be .NET created dataobjects, which will have correctly
                    // UTF8 encoded strings on the clipboard.  As a result, we can
                    // just decode the bytes from that string using the UTF8 decoder.

                    // attempt to get the HTML content assuming that the objects
                    // are Ole objects
                    OleDataObject odo = OleDataObject.CreateFrom(m_dataObject);
                    if (odo != null)
                    {
                        OleStgMediumHGLOBAL storage =
                            (OleStgMediumHGLOBAL)odo.GetData(CF.HTML, TYMED.HGLOBAL);
                        if (storage != null)
                        {
                            using (storage)
                                m_HTMLBytes = GetHTMLBytes(storage);
                        }
                    }

                    // if we couldn't get the HTML bytes then just return the decoded string
                    if (m_HTMLBytes == null)
                        m_HTMLBytes = Encoding.UTF8.GetBytes((string)m_dataObject.GetData(DataFormats.Html));
                }
                // return the bytes
                return m_HTMLBytes;

            }
        }
        private byte[] m_HTMLBytes;

        /// <summary>
        /// Retrieves the bytes representing the data in the HTML format of the HTMLData's
        /// IDataObject.
        /// </summary>
        /// <returns>The bytes representing the HTML in the IDataObject</returns>
        private byte[] GetHTMLBytes(OleStgMediumHGLOBAL storage)
        {
            // NOTE: Our theory about where/why the .NET implementation is failing
            // is that they probably assumed that the CF_HTML clipboard data was raw
            // Unicode and called Marshal.PtrToStringUni to convert it! CF_HTML is
            // in fact UTF8-encoded, so we need to first move it into a .NET byte
            // array and then UTF8-decode it.

            // get a non-movable pointer to the global memory
            IntPtr htmlBytes = Kernel32.GlobalLock(storage.Handle);
            if (htmlBytes == IntPtr.Zero)
            {
                Debug.Assert(false, "Failed to get the pointer for HTML Bytes");
                return null;
            }

            // move the unmanaged global memory block into a managed byte array
            try
            {
                // scan to see where the null terminator is
                byte b = 0;
                int byteCount = 0;

                // Winlive 267804:
                // In some instances office doesn't supply a \0 at the end of the body.
                // Now we add a check to make sure we don't AV
                int maxArraySize = (int)Kernel32.GlobalSize(htmlBytes).ToUInt32();

                do
                {
                    b = Marshal.ReadByte(htmlBytes, byteCount);
                    byteCount++;
                } while (b != 0 && byteCount < maxArraySize);

                // allocate a byte array and copy the unmanged memory to it
                byte[] bytes = new byte[byteCount];
                Marshal.Copy(htmlBytes, bytes, 0, byteCount);

                return bytes;

            }
            finally
            {
                // always unlock the global memory handle
                Kernel32.GlobalUnlock(storage.Handle);
            }

        }

        /// <summary>
        /// The position (in bytes) to the start of the HTML in the CF_HTML formatted
        /// byte array.
        /// </summary>
        private int StartHTML
        {
            get
            {
                int result;
                Int32.TryParse(GetHTMLFormatHeader(HTMLDataObject.Headers.StartHTML), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
                return result;
            }

        }

        /// <summary>
        /// The position (in bytes) to the end of the HTML in the CF_HTML formatted
        /// byte array.
        /// </summary>
        private int EndHTML
        {
            get
            {
                int result;
                Int32.TryParse(GetHTMLFormatHeader(HTMLDataObject.Headers.EndHTML), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
                return result;
            }

        }

        /// <summary>
        /// The position (in bytes) to the start of the HTML fragment in the CF_HTML formatted
        /// byte array.  Note that the fragment contains a complete HTML tree of the selection
        /// (meaning that if the selection doesn't include a closing font tag, for example, the
        /// fragment will include the appropriate matching font tag).  This generally makes the
        /// fragment a preferable way to access the selection.
        /// </summary>
        private int StartFragment
        {
            get
            {
                return Convert.ToInt32(GetHTMLFormatHeader(HTMLDataObject.Headers.StartFragment), CultureInfo.InvariantCulture);
            }

        }

        /// <summary>
        /// The position (in bytes) to the end of the HTML fragment in the CF_HTML formatted
        /// byte array.
        /// </summary>
        private int EndFragment
        {
            get
            {
                return Convert.ToInt32(GetHTMLFormatHeader(HTMLDataObject.Headers.EndFragment), CultureInfo.InvariantCulture);
            }

        }

        /// <summary>
        /// The position (in bytes) to the start of the HTML selection in the CF_HTML formatted
        /// byte array.
        /// </summary>
        private int StartSelection
        {
            get
            {
                return Convert.ToInt32(GetHTMLFormatHeader(HTMLDataObject.Headers.StartSelection), CultureInfo.InvariantCulture);
            }

        }

        /// <summary>
        /// The position (in bytes) to the end of the HTML selection in the CF_HTML formatted
        /// byte array.
        /// </summary>
        private int EndSelection
        {
            get
            {
                return Convert.ToInt32(GetHTMLFormatHeader(HTMLDataObject.Headers.EndSelection), CultureInfo.InvariantCulture);
            }

        }

        /// <summary>
        /// Constructor for HTMLData.  Use the static create method to create a new
        /// HTMLData.
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the HTMLData</param>
        private HTMLData(IDataObject iDataObject, IHTMLDocument2 iHTMLDocument)
        {
            m_dataObject = iDataObject;
            m_HTMLDocument = iHTMLDocument;
        }
        private IDataObject m_dataObject;
    }
}
