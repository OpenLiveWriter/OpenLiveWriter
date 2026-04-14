// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.CoreServices
{

    public class DownloadResults
    {

        public DownloadResults()
        {
        }

        public void AddResult(string url, string filePath)
        {
            if (!_result.ContainsKey(url))
            {
                _result.Add(url, filePath);
            }
        }

        public string GetFilePathForUrl(string url)
        {
            return (string)_result[url];
        }

        private Hashtable _result = new Hashtable();

    }
    /// <summary>
    /// Summary description for MultiThreadedPageDownloader.
    /// </summary>
    public class MultiThreadedPageDownloader
    {

        public MultiThreadedPageDownloader(IProgressHost progressHost) : this(2, progressHost)
        {

        }

        public MultiThreadedPageDownloader(int threadCount, IProgressHost progressHost)
        {
            _threadCount = threadCount;
            _progressHost = progressHost;
        }
        private int _threadCount = 2;
        private IProgressHost _progressHost;
        private Hashtable _urlsToDownload = new Hashtable();

        public void AddUrl(string url, int timeout)
        {
            if (!_urlsToDownload.ContainsKey(url))
                _urlsToDownload.Add(url, timeout);
        }

        private ThreadSafeQueue _downloadQueue = new ThreadSafeQueue();

        public DownloadResults Download()
        {
            TickableProgressTick tickableProgress = new TickableProgressTick(_progressHost, _urlsToDownload.Count);

            Hashtable workItems = new Hashtable();
            foreach (string url in _urlsToDownload.Keys)
            {
                DownloadWorkItem workItem = new DownloadWorkItem(url, (int)_urlsToDownload[url], tickableProgress);
                workItems.Add(url, workItem);
                _downloadQueue.Enqueue(workItem);
            }

            ParallelExecution execution = new
                ParallelExecution(new ThreadStart(DoWork), _threadCount);
            execution.Execute();

            DownloadResults results = new DownloadResults();
            foreach (string url in workItems.Keys)
            {
                results.AddResult(url, ((DownloadWorkItem)workItems[url]).FilePath);
            }
            return results;
        }

        private void DoWork()
        {
            while (true)
            {
                if (_progressHost.CancelRequested)
                    throw new OperationCancelledException();

                bool success;
                DownloadWorkItem workItem =
                    (DownloadWorkItem)_downloadQueue.TryDequeue(out success);
                if (!success)
                    return; // no more work in queue; this thread is done

                try
                {
                    workItem.Download();
                }
                catch(Exception e)
                {
                    Trace.WriteLine("Error downloading link while importing favorites: " + e.ToString());
                }
            }
        }

        private class DownloadWorkItem
        {
            public DownloadWorkItem(string url, int timeout,
                TickableProgressTick tickableProgress)
            {
                _url = url;
                _timeout = timeout;
                _tickableProgress = tickableProgress;
            }
            private string _url;
            private int _timeout;
            private TickableProgressTick _tickableProgress;

            public void Download()
            {
                _tickableProgress.Message("Indexing " + _url);
                string filePath = TempFileManager.Instance.CreateTempFile();
                WebRequestWithCache request = new WebRequestWithCache(_url);

                Stream response = request.GetResponseStream(WebRequestWithCache.CacheSettings.CHECKCACHE,_timeout);
                FileStream fileStream = new FileStream(filePath, FileMode.Open);
                using (response)
                    using (fileStream)
                        StreamHelper.Transfer(response, fileStream);

                _filePath = filePath;

                _tickableProgress.Tick();
            }

            public string FilePath
            {
                get
                {
                    return _filePath;
                }
            }
            private string _filePath = null;
        }

    }
}
