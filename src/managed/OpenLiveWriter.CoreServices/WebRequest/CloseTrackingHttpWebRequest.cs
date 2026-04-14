// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Provides debug-time tracking of HTTP response lifecycle to detect resource leaks.
    /// This is a .NET 10 compatible implementation that replaces the original RealProxy-based version.
    /// 
    /// In DEBUG builds, this tracks HttpWebResponse objects and warns if they are garbage collected
    /// without being properly closed/disposed.
    /// 
    /// Usage:
    /// <code>
    /// var request = HttpRequestHelper.CreateHttpWebRequest(url, false);
    /// var response = (HttpWebResponse)request.GetResponse();
    /// CloseTrackingHttpWebRequest.TrackResponse(response, out var tracker);
    /// try
    /// {
    ///     // Use response...
    /// }
    /// finally
    /// {
    ///     tracker.MarkClosed();
    ///     response.Close();
    /// }
    /// </code>
    /// 
    /// Or use the stream-based tracking:
    /// <code>
    /// using var stream = CloseTrackingHttpWebRequest.GetTrackedResponseStream(response);
    /// // Stream disposal automatically marks the response as closed
    /// </code>
    /// </summary>
    public static class CloseTrackingHttpWebRequest
    {
        // Track responses that haven't been closed yet
        private static readonly ConcurrentDictionary<int, ResponseTracker> _trackedResponses = new();
        private static int _nextTrackerId;

        /// <summary>
        /// Wraps an HttpWebRequest to track response closing in DEBUG builds.
        /// This method exists for API compatibility with existing code.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Wrap(ref HttpWebRequest request)
        {
            // In the new implementation, tracking is done at the response level.
            // This method is kept for API compatibility.
        }

        /// <summary>
        /// Begins tracking an HttpWebResponse for proper disposal.
        /// Returns a tracker object that should be marked as closed when the response is disposed.
        /// </summary>
        /// <param name="response">The response to track.</param>
        /// <returns>A tracker object - call MarkClosed() when disposing the response. In Release builds, returns a no-op tracker.</returns>
        public static ResponseCloseTracker TrackResponse(HttpWebResponse response)
        {
#if DEBUG
            return new ResponseCloseTracker(response);
#else
            return ResponseCloseTracker.NoOp;
#endif
        }

        /// <summary>
        /// Gets a tracked response stream. Disposing this stream automatically marks
        /// the parent response as properly closed.
        /// </summary>
        /// <param name="response">The response to get the stream from.</param>
        /// <returns>A stream that tracks disposal.</returns>
        public static Stream GetTrackedResponseStream(HttpWebResponse response)
        {
#if DEBUG
            var tracker = TrackResponse(response);
            var innerStream = response.GetResponseStream();
            return new CloseTrackingStream(innerStream, tracker);
#else
            return response.GetResponseStream();
#endif
        }

        /// <summary>
        /// Gets the number of responses currently being tracked (for testing).
        /// </summary>
        internal static int TrackedResponseCount => _trackedResponses.Count;

        /// <summary>
        /// Clears all tracked responses (for testing).
        /// </summary>
        internal static void ClearTrackedResponses()
        {
            _trackedResponses.Clear();
        }

        internal static int RegisterResponse(string stackTrace)
        {
            int id = System.Threading.Interlocked.Increment(ref _nextTrackerId);
            _trackedResponses[id] = new ResponseTracker(stackTrace);
            return id;
        }

        internal static void UnregisterResponse(int id)
        {
            _trackedResponses.TryRemove(id, out _);
        }

        private class ResponseTracker
        {
            public string StackTrace { get; }

            public ResponseTracker(string stackTrace)
            {
                StackTrace = stackTrace;
            }
        }
    }

    /// <summary>
    /// Tracks whether an HttpWebResponse was properly closed.
    /// If this tracker is finalized without being marked as closed, it logs a warning.
    /// </summary>
    public class ResponseCloseTracker
    {
        /// <summary>
        /// A no-op tracker for Release builds.
        /// </summary>
        internal static readonly ResponseCloseTracker NoOp = new ResponseCloseTracker();
        
        private readonly int _trackerId;
        private bool _closed;

        // Store stack trace in unmanaged memory to survive until finalizer runs
        private IntPtr _pStackTrace;

        // Private constructor for NoOp instance
        private ResponseCloseTracker()
        {
            _closed = true;
        }

        internal ResponseCloseTracker(HttpWebResponse response)
        {
            if (response == null)
            {
                _closed = true; // Nothing to track
                return;
            }

            string stackTrace = Environment.StackTrace;
            _pStackTrace = Marshal.StringToCoTaskMemUni(stackTrace);
            _trackerId = CloseTrackingHttpWebRequest.RegisterResponse(stackTrace);
        }

        /// <summary>
        /// Call this when the response has been properly closed/disposed.
        /// </summary>
        public void MarkClosed()
        {
            if (!_closed)
            {
                _closed = true;
                FreeStackTrace();
                CloseTrackingHttpWebRequest.UnregisterResponse(_trackerId);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Gets whether this tracker has been marked as closed.
        /// </summary>
        public bool IsClosed => _closed;

        ~ResponseCloseTracker()
        {
            if (!_closed)
            {
                string stackTrace = _pStackTrace != IntPtr.Zero
                    ? Marshal.PtrToStringUni(_pStackTrace)
                    : "Stack trace unavailable";

                // Use Trace instead of Debug.Fail as Debug.Fail calls Environment.FailFast in .NET 10
                Trace.TraceWarning($"[CloseTrackingHttpWebRequest] Unclosed HttpWebResponse detected. " +
                    $"The response was obtained here:\r\n{stackTrace}");
            }

            FreeStackTrace();
            CloseTrackingHttpWebRequest.UnregisterResponse(_trackerId);
        }

        private void FreeStackTrace()
        {
            if (_pStackTrace != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(_pStackTrace);
                _pStackTrace = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// A wrapper around Stream that notifies when the stream is closed/disposed.
    /// Closing/disposing the stream automatically marks the parent response tracker as closed.
    /// </summary>
    internal class CloseTrackingStream : Stream
    {
        private readonly Stream _wrapped;
        private readonly ResponseCloseTracker _tracker;

        internal CloseTrackingStream(Stream wrapped, ResponseCloseTracker tracker)
        {
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
            _tracker = tracker;
        }

        public override bool CanRead => _wrapped.CanRead;
        public override bool CanSeek => _wrapped.CanSeek;
        public override bool CanWrite => _wrapped.CanWrite;
        public override long Length => _wrapped.Length;
        public override long Position
        {
            get => _wrapped.Position;
            set => _wrapped.Position = value;
        }
        public override bool CanTimeout => _wrapped.CanTimeout;
        public override int ReadTimeout
        {
            get => _wrapped.ReadTimeout;
            set => _wrapped.ReadTimeout = value;
        }
        public override int WriteTimeout
        {
            get => _wrapped.WriteTimeout;
            set => _wrapped.WriteTimeout = value;
        }

        public override void Flush() => _wrapped.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _wrapped.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _wrapped.Seek(offset, origin);
        public override void SetLength(long value) => _wrapped.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _wrapped.Write(buffer, offset, count);
        public override int ReadByte() => _wrapped.ReadByte();
        public override void WriteByte(byte value) => _wrapped.WriteByte(value);

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => _wrapped.BeginRead(buffer, offset, count, callback, state);
        public override int EndRead(IAsyncResult asyncResult) => _wrapped.EndRead(asyncResult);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => _wrapped.BeginWrite(buffer, offset, count, callback, state);
        public override void EndWrite(IAsyncResult asyncResult) => _wrapped.EndWrite(asyncResult);

        public override System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            => _wrapped.ReadAsync(buffer, offset, count, cancellationToken);
        public override System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            => _wrapped.WriteAsync(buffer, offset, count, cancellationToken);
        public override System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken)
            => _wrapped.FlushAsync(cancellationToken);
        public override System.Threading.Tasks.Task CopyToAsync(Stream destination, int bufferSize, System.Threading.CancellationToken cancellationToken)
            => _wrapped.CopyToAsync(destination, bufferSize, cancellationToken);

        public override void Close()
        {
            _tracker?.MarkClosed();
            _wrapped.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tracker?.MarkClosed();
                _wrapped.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
