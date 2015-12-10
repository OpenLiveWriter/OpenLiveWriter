// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient.Detection
{
    internal delegate HttpWebResponse HttpRequestHandler(string requestUri, int timeoutMs, HttpRequestFilter filter);

    /// <summary>
    /// Summary description for LazyHomepageDownloader.
    /// </summary>
    internal class LazyHomepageDownloader
    {
        LightWeightHTMLDocument _htmlDocument;
        bool _homepageDownloadAttempted = false;
        string _homepageUrl;
        HttpRequestHandler _requestHandler;
        byte[] _rawBytes;

        public LazyHomepageDownloader(string homepageUrl, HttpRequestHandler requestHandler)
        {
            _homepageUrl = homepageUrl;
            _requestHandler = requestHandler;
        }

        public LightWeightHTMLDocument HtmlDocument
        {
            get
            {
                // download the homepage to look for the link tag
                if (_htmlDocument == null && RawBytes != null)
                {
                    try
                    {
                        using (Stream homepageStream = StreamHelper.AsStream(RawBytes))
                            _htmlDocument = LightWeightHTMLDocument.FromStream(homepageStream, _homepageUrl);
                    }
                    catch (Exception e)
                    {
                        Trace.Fail(e.ToString());
                    }
                }
                return _htmlDocument;
            }
        }

        public byte[] RawBytes
        {
            get
            {
                if (_rawBytes == null && !_homepageDownloadAttempted)
                {
                    try
                    {
                        using (Stream homepageStream = _requestHandler(_homepageUrl, 5000, null).GetResponseStream())
                        {
                            if (homepageStream != null)
                                _rawBytes = StreamHelper.AsBytes(homepageStream);
                        }
                    }
                    catch (WebException e)
                    {
                        HttpRequestHelper.LogException(e);
                    }
                    catch (Exception e)
                    {
                        if (e is BlogClientAuthenticationException)
                        {
                            Trace.WriteLine(e.ToString());
                        }
                        else
                        {
                            Trace.Fail(e.ToString());
                        }
                    }
                    finally
                    {
                        _homepageDownloadAttempted = true;
                    }
                }
                return _rawBytes;
            }
        }

        /// <summary>
        /// The homepage HTML without sanitization or relative URL escaping applied.
        /// </summary>
        public string OriginalHtml
        {
            get
            {
                Encoding encoding = null;
                try
                {
                    if (HtmlDocument.MetaData != null && HtmlDocument.MetaData.Charset != null)
                        encoding = Encoding.GetEncoding(HtmlDocument.MetaData.Charset);
                }
                catch (NotSupportedException)
                {
                }
                if (encoding == null)
                    encoding = new UTF8Encoding(false, false);

                return encoding.GetString(RawBytes);
            }
        }
    }
}
