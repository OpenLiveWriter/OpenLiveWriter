// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// PageDownloader downloads a page and all of its references.  It designed
    /// to download a page (including frames and subframes) rather than a specific
    /// file (for example, a PDF).  Progress will be provided only if necessary (show
    /// progress UI only the first time that the progress event has been fired).
    /// </summary>
    public class PageAndReferenceDownloader
    {
        /// <summary>
        /// Constructs a new asynchrous page download.
        /// </summary>
        /// <param name="pagesToDownload">The array of pagesToDownload</param>
        /// <param name="siteStorage">The File based site storage into which to place the page and references</param>
        /// <param name="target">The ISynchronizeInvoke that will be used to call back</param>
        public PageAndReferenceDownloader(PageToDownload[] pagesToDownload, FileBasedSiteStorage siteStorage) : this(pagesToDownload, siteStorage, true)
        {
        }

        /// <summary>
        /// Constructs a new asynchrous page download.
        /// </summary>
        /// <param name="pagesToDownload">The array of pagesToDownload</param>
        /// <param name="siteStorage">The File based site storage into which to place the page and references</param>
        /// <param name="target">The ISynchronizeInvoke that will be used to call back</param>
        /// <param name="throwOnFailure">Indicates whether downloader should throw on failure, or just
        /// log the failure and continue</param>
        public PageAndReferenceDownloader(PageToDownload[] pagesToDownload, FileBasedSiteStorage siteStorage, bool throwOnFailure)
        {
            _siteStorage = siteStorage;
            _pagesToDownload = pagesToDownload;
            _throwOnFailure = throwOnFailure;
        }

        /// <summary>
        /// The name of the subdirectory that contains any referenced files
        /// </summary>
        public string PathToken
        {
            get
            {
                return _pathToken;
            }
        }
        private string _pathToken;

        /// <summary>
        /// The list of errors that occur during the downlaod (if throwOnFailure is false)
        /// </summary>
        public ArrayList Errors = ArrayList.Synchronized(new ArrayList());

        /// <summary>
        /// Indicates whether the page and reference downloader should preserve scripts
        /// </summary>
        public bool PreserveScripts = true;

        public WinInetCredentialsContext CredentialsContext
        {
            get
            {
                return _credentialsContext;
            }
            set
            {
                _credentialsContext = value;
            }
        }
        private WinInetCredentialsContext _credentialsContext = null;

        /// <summary>
        /// Holds the DownloadWorkItems to be extracted by worker threads.
        /// The workQueue will be fully populated by the time worker threads
        /// begin, i.e., once it's empty it will always be empty.
        /// </summary>
        private ThreadSafeQueue workQueue = new ThreadSafeQueue();

        /// <summary>
        /// The work to be performed by each worker thread.
        /// </summary>
        private void WorkerThreadStart()
        {
            while (true)
            {
                bool success;
                DownloadWorkItem workItem = (DownloadWorkItem)workQueue.TryDequeue(out success);
                if (!success)
                    return; // no more work in queue; this thread is done

                try
                {
                    workItem.progressHost.UpdateProgress(string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.ProgressDownloading), workItem.reference.AbsoluteUrl));
                    DownloadReference(workItem.reference, workItem.siteStorage, new ProgressHostFilter(workItem.progressHost, ProgressHostFilter.MessageType.UpdateMessage));
                }
                catch (Exception e)
                {
                    HandleException(e);
                }

            }
        }

        /// <summary>
        /// Holds parameters that are used by worker threads.
        /// </summary>
        private class DownloadWorkItem
        {
            public readonly ReferenceToDownload reference;
            public readonly FileBasedSiteStorage siteStorage;
            public readonly IProgressHost progressHost;

            public DownloadWorkItem(ReferenceToDownload reference, FileBasedSiteStorage siteStorage, IProgressHost progressHost)
            {
                this.reference = reference;
                this.siteStorage = siteStorage;
                this.progressHost = progressHost;
            }
        }

        /// <summary>
        /// Downloads the pages and their references, providing progress feedback
        /// </summary>
        /// <param name="progressHost">The progresshost to use for feedback</param>
        /// <returns>this</returns>
        public object Download(IProgressHost progressHost)
        {
            // Prepare the list of references to download
            progressHost.UpdateProgress(Res.Get(StringId.ProgressPreparingListOfFiles));
            foreach (PageToDownload pageToDownload in _pagesToDownload)
            {
                // Lay down a placeholder file with the correct file name
                try
                {
                    string destination = Path.Combine(_siteStorage.BasePath, pageToDownload.RelativePath);
                    destination = PathHelper.GetNonConflictingPath(destination);
                    pageToDownload.FileName = Path.GetFileName(destination);

                    using (Stream htmlStream = _siteStorage.Open(destination, AccessMode.Write)) { }
                }
                catch (Exception e)
                {
                    HandleException(e);
                }

                foreach (ReferenceToDownload reference in pageToDownload.References)
                {
                    // Don't add the same item more than once
                    if (!_referencesToDownload.ContainsKey(reference.AbsoluteUrl))
                        _referencesToDownload.Add(reference.AbsoluteUrl, reference);
                }
            }

            // Enqueue the work items
            progressHost.UpdateProgress(Res.Get(StringId.ProgressStartingDownloadOfReferences));
            IProgressHost[] progressHosts = new JointProgressHosts(progressHost, _referencesToDownload.Count, 8000, 10000).ProgressHosts;
            int tickNum = 0;
            foreach (ReferenceToDownload reference in _referencesToDownload.Values)
                workQueue.Enqueue(new DownloadWorkItem(reference, _siteStorage, progressHosts[tickNum++]));

            // Start up the parallel execution of the downloads
            ParallelExecution parallelExecution = new ParallelExecution(new ThreadStart(WorkerThreadStart), 2);
            parallelExecution.Execute();
            parallelExecution = null;

            // Now go through and get HTML for each page, and emit the HTML to disk
            ProgressTick allPagesProgress = new ProgressTick(progressHost, 2000, 10000);
            for (int i = 0; i < _pagesToDownload.Length; i++)
            {
                try
                {
                    allPagesProgress.UpdateProgress(i, _pagesToDownload.Length, string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.ProgressSaving), _pagesToDownload[i].FileName));
                    WriteHtmlToDisk(_pagesToDownload[i], _siteStorage);
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
                if (allPagesProgress.CancelRequested)
                    throw new OperationCancelledException();
            }

            // We're complete!
            progressHost.UpdateProgress(1, 1, Res.Get(StringId.ProgressDownloadFinished));

            return this;
        }

        /// <summary>
        /// Downloads the pages and their references, providing no feedback
        /// </summary>
        public void Download()
        {
            Download(SilentProgressHost.Instance);
        }

        /// <summary>
        /// Helper that handles exceptions
        /// </summary>
        /// <param name="e">The exception to handle</param>
        private void HandleException(Exception e)
        {
            if (_throwOnFailure || null != (e as OperationCancelledException))
            {
                Trace.Fail("PageAndReferenceDownloader: " + e.ToString());
                throw e;
            }
            else
                Errors.Add(e);
        }

        /// <summary>
        /// Download a reference, providing progress
        /// </summary>
        /// <param name="reference">The reference to download</param>
        /// <param name="fileStorage">The storage to download the refernce into</param>
        /// <param name="progressHost">The progressHost to provide feedback to</param>
        private void DownloadReference(ReferenceToDownload reference, FileBasedSiteStorage fileStorage, IProgressHost progressHost)
        {
            if (IsBase64EmbededImage(reference.AbsoluteUrl))
            {
                return;
            }
            UrlDownloadToFile downloader;
            string fullPath;

            downloader = new UrlDownloadToFile();
            downloader.TimeoutMs = 30000;

            if (progressHost.CancelRequested)
                throw new OperationCancelledException();

            // make sure that the directory exists
            fullPath = Path.Combine(fileStorage.BasePath, reference.RelativePath);

            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Make sure there aren't any conflicts
            lock (this)
            {
                string newFileName = Path.GetFileName(fullPath);
                do
                {
                    fullPath = PathHelper.GetNonConflictingPath(fullPath);
                    newFileName = Path.GetFileName(fullPath);

                    reference.SetFileName(ref newFileName);

                } while (newFileName != Path.GetFileName(fullPath));

                using (File.Open(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                }
            }

            string downloadUrl = reference.AbsoluteUrl;
            if (UrlHelper.IsFileUrl(downloadUrl))
                downloadUrl = HttpUtility.UrlDecode(downloadUrl);

            downloader.Url = downloadUrl.Trim();
            downloader.FilePath = fullPath;
            downloader.ShowSecurityUI = true;
            if (CredentialsContext != null)
                downloader.CredentialsContext = CredentialsContext;

            try
            {
                downloader.Download(progressHost);
            }
            catch (COMException e)
            {
                // If the file couldn't be downloaded, this doesn't matter.  But log it
                Trace.WriteLine("Didn't download file: " + downloader.Url + " " + e.ToString());
            }

            // Fix the filename of the downloaded entity to have the correct extension
            string contentType = downloader.ContentType;
            if (contentType != null && File.Exists(fullPath))
            {
                string suggestedExtension = MimeHelper.GetExtensionFromContentType(contentType);
                if (suggestedExtension == null)
                    suggestedExtension = UrlHelper.GetExtensionForUrl(downloader.FinalUrl);

                if (Path.GetExtension(fullPath) != suggestedExtension)
                {
                    string newFilePath = Path.ChangeExtension(fullPath, suggestedExtension);
                    string newFileName = Path.GetFileName(newFilePath);

                    // Try to reset the name until we can both agree
                    while (true)
                    {
                        newFilePath = PathHelper.GetNonConflictingPath(newFilePath);
                        newFileName = Path.GetFileName(newFilePath);

                        FileHelper.Rename(fullPath, newFilePath);
                        reference.SetFileName(ref newFileName);
                        if (newFileName != Path.GetFileName(newFilePath))
                        {
                            try
                            {
                                File.Delete(newFilePath);
                            }
                            catch (Exception e) { Debug.Fail("Unable to delete failed temp file: " + e.ToString()); }
                        }
                        else
                        {
                            break;
                        }
                    }

                    fullPath = newFilePath;
                }
            }

            // Handle anything special we need to do for stylesheet and js file references
            if (Path.GetExtension(fullPath) == ".css" && File.Exists(fullPath))
            {
                string fileContents = string.Empty;
                using (StreamReader reader = new StreamReader(fullPath))
                    fileContents = reader.ReadToEnd();

                if (fileContents != string.Empty)
                {
                    LightWeightCSSReplacer cssReplacer = new LightWeightCSSReplacer(fileContents);
                    // fix up references
                    foreach (ReferenceToDownload referenceInfo in _referencesToDownload.Values)
                        cssReplacer.AddUrlToReplace(new UrlToReplace(referenceInfo.UrlToReplace, referenceInfo.FileName));

                    string newCss = cssReplacer.DoReplace();
                    using (StreamWriter writer = new StreamWriter(fullPath, false))
                        writer.Write(newCss);
                }
            }
        }

        private bool IsBase64EmbededImage(string url)
        {
            return url.StartsWith("data:image/", StringComparison.InvariantCultureIgnoreCase) &&
                            url.ToLowerInvariant().Contains("base64");
        }

        /// <summary>
        /// Used to actually commit the HTML to disk
        /// </summary>
        /// <param name="pageInfo">The PageToDownload to write</param>
        /// <param name="downloadedReferences">A hashtable of download references</param>
        /// <param name="storage">The storage to write the file into</param>
        private void WriteHtmlToDisk(PageToDownload pageInfo, FileBasedSiteStorage storage)
        {
            // Set the character set for this document
            pageInfo.LightWeightHTMLDocument.MetaData.Charset = Encoding.UTF8.WebName;
            string html = string.Empty;

            // Replace references to any URL that we downloaded!
            foreach (PageToDownload pageToDownload in _pagesToDownload)
                if (!pageToDownload.IsRootPage)
                    pageInfo.LightWeightHTMLDocument.AddUrlToEscape(new UrlToReplace(pageToDownload.UrlToReplace, pageInfo.GetRelativeUrlForReferencedPage(pageToDownload)));

            foreach (ReferenceToDownload referenceToDownload in _referencesToDownload.Values)
            {
                ReferenceToDownload downloadedReference = ((ReferenceToDownload)_referencesToDownload[referenceToDownload.AbsoluteUrl]);

                // Since we consolidated references, replace the UrToReplace from the original reference
                // with the relativePath to the reference that actually got downloaded
                string path = downloadedReference.GetRelativeUrlForReference(pageInfo);
                pageInfo.LightWeightHTMLDocument.AddUrlToEscape(new UrlToReplace(referenceToDownload.AbsoluteUrl, path));
            }

            html = pageInfo.LightWeightHTMLDocument.GenerateHtml();

            // finally, write the html out to disk
            string destination = Path.Combine(_siteStorage.BasePath, pageInfo.RelativePath);
            Stream htmlStream = _siteStorage.Open(destination, AccessMode.Write);
            using (StreamWriter writer = new StreamWriter(htmlStream, Encoding.UTF8))
            {
                writer.Write(html);
            }

            // if this is the entry page, write the path token and root file name
            if (pageInfo.IsRootPage)
            {
                this._pathToken = pageInfo.ReferencedFileRelativePath;
                _siteStorage.RootFile = pageInfo.FileName;
            }
        }

        /// <summary>
        /// The file based site storage into which the page is being downloaded
        /// </summary>
        private FileBasedSiteStorage _siteStorage;

        /// <summary>
        /// The list of pages that should be downloaded
        /// </summary>
        private PageToDownload[] _pagesToDownload;

        /// <summary>
        /// The list of references that should be downloaded
        /// </summary>
        private Hashtable _referencesToDownload = new Hashtable();

        /// <summary>
        /// Indicates whether the download should throw when an exception occurs
        /// </summary>
        private bool _throwOnFailure = true;
    }
}
