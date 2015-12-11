// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.PostEditor.Video.VideoListBox;
using OpenLiveWriter.PostEditor.Video.VideoService;

namespace OpenLiveWriter.PostEditor.Video.VideoListBox
{
    /// <summary>
    /// Class which abstracts downloading and retrieving thumbnail images.
    /// This class should be instantiated and used on the main UI thread.
    /// </summary>
    internal class VideoThumbNailManager : IDisposable
    {
        /// <summary>
        /// Initialize a thumbnail manager with with host control (provides UI-thread
        /// context) and the content-source services interface (used for downloading).
        /// Creating this class initializes a set of 5 worker threads that are used
        /// for downloading (these threads are terminated when the class is Disposed)
        /// </summary>
        public VideoThumbNailManager(Control owner)
        {
            _owner = owner;

            // fire up three downloader threads
            for (int i = 0; i < 5; i++)
            {
                Thread thread =
                    ThreadHelper.NewThread(ThumbnailDownloaderMain, "ThumbnailDownloader" + (i + 1), true, true, true);
                thread.Start();
            }
        }

        /// <summary>
        /// Download the thumbnail for the specified video (only if it isn't already in our cache)
        /// </summary>
        public void DownloadThumbnail(IVideo video)
        {
            if (!_thumbnails.Contains(video))
            {
                // if we can get a version of the thumbnail from the cache
                // then just save this version
                PluginHttpRequest pluginHttpRequest = new PluginHttpRequest(video.ThumbnailUrl, HttpRequestCacheLevel.CacheOnly);
                using (Stream cachedStream = pluginHttpRequest.GetResponse())
                {
                    if (cachedStream != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        StreamHelper.Transfer(cachedStream, memoryStream);
                        _thumbnails[video] = new VideoThumbnail(memoryStream);
                    }
                    // otherwise mark it as 'downloading' and enqueque the download
                    else
                    {
                        _thumbnails[video] = new DownloadingVideoThumbnail();
                        _workQueue.Enqueue(video);
                    }
                }
            }
        }

        /// <summary>
        /// Get the thumbnail for the passed video. First ensure that it either has been
        /// downloaded or we have a "Downloading Preview..." proxy image in the cache
        /// </summary>
        public VideoThumbnail GetThumbnail(IVideo video)
        {
            DownloadThumbnail(video);
            return _thumbnails[video];
        }

        public void ClearPendingDownloads()
        {
            _workQueue.Clear();
        }

        /// <summary>
        /// Notify listeners that a new download has been completed
        /// </summary>
        public event VideoThumbnailDownloadCompleteHandler ThumbnailDownloadCompleted;

        /// <summary>
        /// Dispose the thumbnail manager -- terminate the thread-pool and discard
        /// all of the thumbnail bitmaps and their underlying streams
        /// </summary>
        public void Dispose()
        {
            _workQueue.Terminate(true);
            _thumbnails.Dispose();
        }

        /// <summary>
        /// Main worker thread logic for thumbnailer
        /// </summary>
        private void ThumbnailDownloaderMain()
        {
            try
            {
                // keep waiting for new work items until the queue is terminated
                // via the ThreadSafeQueueTerminatedException
                while (true)
                {
                    // get the next video to download
                    IVideo ivideo = _workQueue.Dequeue() as IVideo;

                    // attempt to download it
                    Stream videoStream = SafeDownloadThumbnail(ivideo);

                    // either get the downloaded thumbnail or if we failed due to a
                    // timeout or other error then put in a special 'Not Available'
                    // thumbnail image
                    VideoThumbnail thumbnail;
                    if (videoStream != null)
                        thumbnail = new VideoThumbnail(videoStream);
                    else
                        thumbnail = new NoAvailableVideoThumbnail();

                    // notification that the download is completed
                    ProcessCompletedDownload(ivideo, thumbnail);
                }
            }
            catch (ThreadSafeQueueTerminatedException)
            {
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception in ThumbnailDownloader: " + ex);
            }
        }

        /// <summary>
        /// Download the thumbnail -- return null if any error (connection, r/w, timeout, etc.) occurs
        /// </summary>
        private static Stream SafeDownloadThumbnail(IVideo ivideo)
        {
            try
            {
                // download the thumbnail
                PluginHttpRequest pluginHttpRequest = new PluginHttpRequest(ivideo.ThumbnailUrl, HttpRequestCacheLevel.CacheIfAvailable);
                using (Stream downloadedStream = pluginHttpRequest.GetResponse(5000))
                {
                    if (downloadedStream != null)
                    {
                        // transfer it to a memory stream so we don't hold open the network connection
                        // (creating a Bitmap from a Stream requires that you hold open the Stream for
                        // the lifetime of the Bitmap and we clearly don't want to do this for Streams
                        // loaded from the network!)
                        Stream memoryStream = new MemoryStream();
                        StreamHelper.Transfer(downloadedStream, memoryStream);
                        return memoryStream;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Callback method invoked from worker threads to indicate that another
        /// thumbnail has been downloaded
        /// </summary>
        private void ProcessCompletedDownload(IVideo iVideo, VideoThumbnail thumbnail)
        {
            try
            {
                // ensure we don't invoke on 'dead' controls (could happen if a background
                // thread completes a download after the parent dialog has been dismissed)
                if (!_workQueue.Terminated && !_owner.IsDisposed)
                {
                    // marshal to the UI thread if necessary
                    if (_owner.InvokeRequired)
                    {
                        _owner.BeginInvoke(new CompletedHandler(ProcessCompletedDownload), new object[] { iVideo, thumbnail });
                    }
                    else
                    {
                        // cache the thumbnail
                        _thumbnails[iVideo] = thumbnail;

                        // notify listeners
                        if (ThumbnailDownloadCompleted != null)
                            ThumbnailDownloadCompleted(iVideo);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception in DownloaderCompletedHandler: " + ex);
            }
        }

        private delegate void CompletedHandler(IVideo iVideo, VideoThumbnail thumbnail);

        private readonly Control _owner;
        private readonly ThreadSafeQueue _workQueue = new ThreadSafeQueue();
        private readonly VideoThumbnailCollection _thumbnails = new VideoThumbnailCollection();
    }

    /// <summary>
    /// Callback used for notification of a new video download by the UI thread
    /// </summary>
    internal delegate void VideoThumbnailDownloadCompleteHandler(IVideo video);

    /// <summary>
    /// Threadsafe collection of Thumbnails
    /// </summary>
    internal class VideoThumbnailCollection : IDisposable
    {
        /// <summary>
        /// Does the collection contain the passed video?
        /// </summary>
        public bool Contains(IVideo video)
        {
            lock (this)
            {
                return _thumbnails.Contains(video);
            }
        }

        /// <summary>
        /// Indexer for getting or setting thumbnails for a video
        /// </summary>
        public VideoThumbnail this[IVideo video]
        {
            get
            {
                lock (this)
                {
                    return _thumbnails[video] as VideoThumbnail;
                }
            }
            set
            {
                lock (this)
                {
                    // if there is an existing value then dispose it
                    if (_thumbnails.Contains(video))
                        ((VideoThumbnail)_thumbnails[video]).Dispose();

                    // set the new value
                    _thumbnails[video] = value;
                }
            }
        }

        /// <summary>
        /// Dispose by disposing all of the underlying thumbnail bitmaps/streams
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                foreach (VideoThumbnail thumbnail in _thumbnails.Values)
                    thumbnail.Dispose();
            }
        }

        private readonly Hashtable _thumbnails = new Hashtable();
    }
}
