// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO ;
using System.Net ;
using System.Reflection;
using mshtml ;
using OpenLiveWriter.CoreServices.Progress;

namespace OpenLiveWriter.CoreServices
{

    public class WebPageCapture
    {

        public WebPageCapture(string targetUrl, string destinationPath)
        {
            _targetUrl = targetUrl ;
            _destinationPath = destinationPath ;
        }

        public string TargetUrl
        {
            get { return _targetUrl; }
        }
        private string _targetUrl ;

        public string DestinationPath
        {
            get { return _destinationPath; }
        }
        private string _destinationPath ;

        public string Capture(int timeoutMs)
        {
            // flag indicating whether we should continue with the capture
            bool continueCapture = true ;

            // request the page
            HttpWebResponse response = RequestPage(TargetUrl, timeoutMs);
            OnHeadersReceived(response.Headers, ref continueCapture) ;
            if ( !continueCapture )
                throw new OperationCancelledException() ;

            // transfer it to a stream
            MemoryStream pageStream = new MemoryStream();
            using ( Stream responseStream = response.GetResponseStream() )
                StreamHelper.Transfer(responseStream, pageStream);
            pageStream.Seek(0, SeekOrigin.Begin) ;

            // allow filter on content
            OnContentReceived( new StreamReader(pageStream).ReadToEnd(), ref continueCapture ) ;
            if ( !continueCapture )
                throw new OperationCancelledException() ;
            pageStream.Seek(0, SeekOrigin.Begin) ;

            // Read the stream into a lightweight HTML doc. We use from LightWeightHTMLDocument.FromIHTMLDocument2
            // instead of LightWeightHTMLDocument.FromStream because from stream improperly shoves a saveFrom declaration
            // above the docType (bug 289357)
            IHTMLDocument2 doc = HTMLDocumentHelper.StreamToHTMLDoc(pageStream, TargetUrl, false);
            LightWeightHTMLDocument ldoc = LightWeightHTMLDocument.FromIHTMLDocument2(doc, TargetUrl, true);

            // download references
            FileBasedSiteStorage siteStorage = new FileBasedSiteStorage(DestinationPath, "index.htm");
            PageToDownload page = new PageToDownload(ldoc, TargetUrl, siteStorage.RootFile);
            PageAndReferenceDownloader downloader = new PageAndReferenceDownloader(new PageToDownload[]{page}, siteStorage) ;
            downloader.Download(new TimeoutProgressHost(timeoutMs)) ;

            // return path to captured page
            return Path.Combine(DestinationPath, siteStorage.RootFile) ;
        }

        public string SafeCapture(int timeoutMs)
        {
            try
            {
                return Capture(timeoutMs) ;
            }
            catch
            {
                return null ;
            }
        }

        protected virtual void OnHeadersReceived(WebHeaderCollection headers, ref bool continueCapture)
        {
        }

        protected virtual void OnContentReceived(string content, ref bool continueCapture)
        {
        }

        private HttpWebResponse RequestPage(string targetUrl, int timeoutMs)
        {
            try
            {
                return HttpRequestHelper.SendRequest(targetUrl, req => req.Timeout = timeoutMs);
            }
            catch(WebResponseTimeoutException)
            {
                // convert "special" timed out exception into conventional .net timed out exception
                throw new OperationTimedOutException() ;
            }
        }

        private class TimeoutProgressHost : IProgressHost
        {
            private DateTime _timeoutTime ;

            public TimeoutProgressHost(int timeoutMs)
            {
                _timeoutTime = DateTime.Now.AddMilliseconds(timeoutMs) ;
            }

            private void CheckForTimeout()
            {
                if ( DateTime.Now.CompareTo(_timeoutTime) > 0 )
                    throw new OperationTimedOutException();
            }

            public void UpdateProgress(int complete, int total, string message)
            {
                CheckForTimeout() ;
            }

            public void UpdateProgress(int complete, int total)
            {
                CheckForTimeout() ;
            }

            public void UpdateProgress(string message)
            {
                CheckForTimeout() ;
            }

            public bool CancelRequested
            {
                get
                {
                    return false;
                }
            }

            public double ProgressCompletionPercentage
            {
                get
                {
                    return 0;
                }
            }
        }

    }
}
