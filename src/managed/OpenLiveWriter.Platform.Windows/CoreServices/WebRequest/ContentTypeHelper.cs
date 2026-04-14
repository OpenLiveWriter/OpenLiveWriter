// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.CoreServices
{

    /// <summary>
    /// Class the holds information about a Urls content type.
    /// </summary>
    public class UrlContentTypeInfo
    {
        public UrlContentTypeInfo(string contentType, string url) : this(contentType, null, url, -1)
        {
        }

        public UrlContentTypeInfo(string contentType, string contentEncoding, string url, int contentLength)
        {
            m_contentType = contentType;
            m_contentEncoding = contentEncoding;
            m_url = url;
            _contentLength = contentLength;
        }

        /// <summary>
        /// The content type of the given Url
        /// </summary>
        public string ContentType
        {
            get
            {
                return m_contentType;
            }
        }
        private string m_contentType;

        /// <summary>
        /// The Url (after redirects) that actually provided the content type information
        /// </summary>
        public string FinalUrl
        {
            get
            {
                return m_url;
            }
        }
        private string m_url;

        /// <summary>
        /// The content encoding provided for this url (if known).
        /// </summary>
        public string ContentEncoding
        {
            get
            {
                return m_contentEncoding;
            }
        }
        private string m_contentEncoding;

        /// <summary>
        /// The content length provided for the url (if known).
        /// </summary>
        public int ContentLength
        {
            get
            {
                return _contentLength;
            }
        }
        private int _contentLength = -1;
    }

    /// <summary>
    /// Helper for determining and using content type information
    /// </summary>
    public class ContentTypeHelper
    {

        /// <summary>
        /// Determine the content type of a URL using only inexpensive operations like looking
        /// in the cache or guessing based upon extension.  This may not always return
        /// the correct content type, especially for redirected URLs.
        /// </summary>
        /// <param name="url">The url for which to check content type</param>
        /// <returns>The content type</returns>
        public static UrlContentTypeInfo InexpensivelyGetUrlContentType(string url)
        {
            UrlContentTypeInfo contentTypeInfo = null;
            contentTypeInfo = GetContentTypeFromBrowserCache(url);

            if (contentTypeInfo == null)
            {
                string contentType = GuessContentTypeLocally(url);
                contentTypeInfo = new UrlContentTypeInfo(contentType, url);
            }

            return contentTypeInfo;
        }

        /// <summary>
        /// Determine the content type of a URL using inexpensive operations as well as
        /// potentially using network IO to determine content type.  This will return the
        /// correct content type.
        /// </summary>
        /// <param name="url">The url for which to check content type</param>
        /// <returns>The content type</returns>
        public static UrlContentTypeInfo ExpensivelyGetUrlContentType(string url)
        {
            return ExpensivelyGetUrlContentType(url, -1);
        }

        /// <summary>
        /// Determine the content type of a URL using inexpensive operations as well as
        /// potentially using network IO to determine content type.  This will return the
        /// correct content type.
        /// </summary>
        /// <param name="url">The url for which to check content type</param>
        /// <param name="timeOutMs">MS to execute before timing out</param>
        /// <returns>The content type</returns>
        public static UrlContentTypeInfo ExpensivelyGetUrlContentType(string url, int timeOutMs)
        {
            string contentType = null;
            UrlContentTypeInfo urlContentTypeInfo = null;

            // If the url ends with a pdf, treat it as a PDF file no matter what the server says!
            if (UrlHelper.GetExtensionForUrl(url) == ".pdf")
            {
                contentType = GuessContentTypeLocally(url);
                return new UrlContentTypeInfo(contentType, url);
            }

            urlContentTypeInfo = GetContentTypeFromBrowserCache(url);
            if (urlContentTypeInfo != null)
                return urlContentTypeInfo;

            urlContentTypeInfo = GetContentTypeUsingNetworkIO(url, timeOutMs);
            if (urlContentTypeInfo != null)
                return urlContentTypeInfo;

            contentType = GuessContentTypeLocally(url);
            if (contentType != null)
                return new UrlContentTypeInfo(contentType, url);

            return null;
        }

        /// <summary>
        /// Looks up the content type in browser cache
        /// </summary>
        /// <param name="url">The url for which to check content type</param>
        /// <returns>The content type</returns>
        private static UrlContentTypeInfo GetContentTypeFromBrowserCache(string url)
        {
            UrlContentTypeInfo contentType = null;

            // throw out the query string and other bits of the url, if we can
            if (UrlHelper.IsUrl(url))
            {
                Uri uri = new Uri(url);
                // by using the absolute uri, we're more likely to hit the cache
                url = UrlHelper.SafeToAbsoluteUri(uri);
            }

            // Get the header for this URL out of the cache and see if we
            // can get the content type out of the header
            Internet_Cache_Entry_Info info;
            if (WinInet.GetUrlCacheEntryInfo(url, out info))
            {
                // Get the header string for the info struct
                string header = Marshal.PtrToStringAnsi(info.lpHeaderInfo);

                // scan through the lines until we find the content type line
                if (header != null)
                {
                    string contentTypeString = null;
                    string contentLengthString = null;
                    string contentEncodingString = null;

                    string[] lines = header.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.IndexOf(":", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            string[] parts = line.Split(':');
                            if (parts[0].ToUpperInvariant() == "CONTENT-TYPE")
                            {
                                // be aware the character encoding can be appended to the end of this line
                                // following a semicolon
                                if (parts[0].IndexOf(";", StringComparison.OrdinalIgnoreCase) > -1)
                                {
                                    string[] subParts = parts[0].Split(';');
                                    contentTypeString = subParts[0].Trim();
                                    contentEncodingString = subParts[1].Trim();
                                }
                                else
                                    contentTypeString = parts[1].Trim();

                            }
                            else if (parts[0].ToUpperInvariant() == "CONTENT-LENGTH")
                            {
                                contentLengthString = parts[1].Trim();
                            }

                            if (contentTypeString != null && contentLengthString != null)
                                break;
                        }
                    }
                    contentType = new UrlContentTypeInfo(contentTypeString, contentEncodingString, url, int.Parse(contentLengthString, CultureInfo.InvariantCulture));
                }
            }
            return contentType;
        }

        /// <summary>
        /// Guesses the content type using the URL's extension.  This is easily fooled by redirected
        /// URLs, so should only be used as a last resort.
        /// </summary>
        /// <param name="url">The url for which to check content type</param>
        /// <returns>The content type</returns>
        private static string GuessContentTypeLocally(string url)
        {
            string contentType = null;

            string extension = UrlHelper.GetExtensionForUrl(url);

            if (extension != string.Empty)
            {
                // If that didn't work, let's try looking up based upon extension
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    if (key != null)
                        contentType = (string)key.GetValue("Content Type");
                }
            }

            // If we couldn't figure it out from the registry, guess
            if (contentType == null)
                contentType = GuessContentType(url);

            return contentType;
        }

        /// <summary>
        /// Guess the content type from looking at the extension of a URL
        /// </summary>
        /// <param name="url">The url from which to guess</param>
        /// <returns>The content type</returns>
        private static string GuessContentType(string url)
        {
            string contentType = null;
            bool isFileUrl = UrlHelper.IsFileUrl(url);
            switch (UrlHelper.GetExtensionForUrl(url).ToUpperInvariant())
            {
                case (".HTM"):
                case (".HTML"):
                case (".JSP"):
                case (".ASP"):
                case (".ASPX"):
                case (".CFM"):
                case (".DBM"):
                case (".PHP"):
                case (".PL"):
                case (".SHTML"):
                    contentType = MimeHelper.TEXT_HTML;
                    break;

                case (".DLL"):
                    if (isFileUrl)
                        contentType = MimeHelper.APP_OCTET_STREAM;
                    else
                        contentType = MimeHelper.TEXT_HTML;
                    break;

                // if it has no extension, but is a url, its probably a page.
                case (""):
                    if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        contentType = MimeHelper.TEXT_HTML;
                    }
                    else if (UrlHelper.IsUrl(url) && !UrlHelper.IsFileUrl(url))
                    {
                        contentType = MimeHelper.APP_OCTET_STREAM;
                    }
                    else
                        contentType = MimeHelper.CONTENTTYPE_UNKNOWN;
                    break;

                case (".AVI"):
                case (".MPG"):
                case (".MPEG"):
                case (".M3U"):
                case (".PLS"):
                case (".MOV"):
                case (".SWF"):
                case (".DOC"):
                case (".XLS"):
                case (".PPT"):
                case (".PDF"):
                case (".RM"):
                case (".RA"):
                case (".RAM"):
                case (".RMVB"):
                case (".RP"):
                case (".RT"):
                case (".ZIP"):
                case (".EXE"):
                case (".MHT"):
                case (".URL"):
                case (".CFS"):
                case (".LNK"):
                case (".MP4"):
                case (".SMI"):
                case (".SMIL"):
                case (".CSS"):
                case (".ASX"):
                case (".ASF"):
                case (".MP3"):
                case (".PS"):
                case (".RTF"):
                case (".WAX"):
                case (".WVX"):
                case (".WMA"):
                case (".AU"):
                case (".WAV"):
                case (".AIF"):
                case (".AIFF"):
                case (".AIFC"):
                case (".IEF"):
                case (".SND"):
                case (".WMV"):
                    contentType = MimeHelper.APP_OCTET_STREAM;
                    break;

                case (".JPG"):
                case (".JPEG"):
                    contentType = MimeHelper.IMAGE_JPG;
                    break;

                case (".GIF"):
                    contentType = MimeHelper.IMAGE_GIF;
                    break;

                case (".PNG"):
                    contentType = MimeHelper.IMAGE_PNG;
                    break;

                case (".BMP"):
                    contentType = MimeHelper.IMAGE_BMP;
                    break;

                case (".TIF"):
                    contentType = MimeHelper.IMAGE_TIF;
                    break;

                case ("ICO"):
                    contentType = MimeHelper.IMAGE_ICON;
                    break;

                case (".TXT"):
                    contentType = MimeHelper.TEXT_PLAIN;
                    break;

                default:
                    // If its a local file, guess that its a file if we don't already know that
                    if (UrlHelper.IsUrl(url) && new Uri(url).IsFile)
                        contentType = MimeHelper.APP_OCTET_STREAM;
                    else
                        contentType = MimeHelper.TEXT_HTML;

                    break;
            }
            return contentType;
        }

        /// <summary>
        /// Retrieve the content type by requesting the content type from the server hosting the URL
        /// </summary>
        /// <param name="url">The url for which to check content type</param>
        /// <param name="timeOutMs">The duration in MS that the operation will execute before failing</param>
        /// <returns>The content type</returns>
        private static UrlContentTypeInfo GetContentTypeUsingNetworkIO(string url, int timeOutMs)
        {
            UrlContentTypeInfo contentType = null;

            if (UrlHelper.IsFileUrl(url))
            {
                string content = GuessContentTypeLocally(url);
                if (content != null)
                    contentType = new UrlContentTypeInfo(content, url);
            }

            if (contentType == null)
            {
                WebRequestWithCache webRequest = new WebRequestWithCache(url);

                using var response = timeOutMs == -1
                    ? webRequest.GetHeadOnly()
                    : webRequest.GetHeadOnly(timeOutMs);

                if (response != null)
                {
                    var mediaType = response.Content?.Headers?.ContentType;
                    if (mediaType != null && !string.IsNullOrEmpty(mediaType.MediaType))
                    {
                        string contentTypeString = mediaType.MediaType;
                        string contentEncodingString = mediaType.CharSet;
                        var responseUri = response.RequestMessage?.RequestUri ?? new Uri(url);
                        long contentLength = response.Content?.Headers?.ContentLength ?? -1;
                        contentType = new UrlContentTypeInfo(contentTypeString, contentEncodingString, UrlHelper.SafeToAbsoluteUri(responseUri), (int)contentLength);
                    }
                }
            }
            return contentType;
        }

    }
}
