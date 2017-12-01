// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Interop.Com.StructuredStorage;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// Adapter class that exposes COM IStreams as .NET streams.  For detailed documentation
    /// on each method and property, see documentation on stream.
    ///
    /// Based upon the ComStream implementation example found in
    /// Adam Nathan's ".NET and COM: The Complete Interoperability Guide" Page 883.
    /// </summary>
    public class ComStream : Stream
    {
        /// <summary>
        /// ComStream constructor
        /// </summary>
        /// <param name="stream">The COM Stream from which to create the .NET stream</param>
        public ComStream(IStream stream, bool commitOnDispose)
        {
            if (stream != null)
            {
                m_comStream = stream;
            }
            else
            {
                throw new ArgumentNullException("stream");
            }

            CommitOnDispose = commitOnDispose;
        }

        public ComStream(IStream stream)
            : this(stream, true)
        {

        }

        private bool CommitOnDispose { get; set; }

        private bool supportNonZeroOffset = false;

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ComStream()
        {
            Debug.Assert(m_comStream == null, "Object not disposed properly - Use Close or Dispose!");
        }

        /// <summary>
        /// Set to true if non-zero offset support is needed.  Using non-zero
        /// offsets will slow down reads and writes.
        /// </summary>
        public bool SupportNonZeroOffset
        {
            get { return supportNonZeroOffset; }
            set { supportNonZeroOffset = value; }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the
        /// current position within the stream by the number of bytes read
        /// </summary>
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            ValidateUnderlyingStream();
            ValidateOffset(offset);

            byte[] oldBuffer = buffer;
            if (offset != 0)
            {
                buffer = GetTempBuffer(count);
            }

            int bytesRead;
            IntPtr pBytesRead = new IntPtr(&bytesRead);
            m_comStream.Read(buffer, count, pBytesRead);

            if (offset != 0)
            {
                Array.Copy(buffer, 0, oldBuffer, offset, bytesRead);
            }

            return bytesRead;
        }

        // Writes a sequence of bytes to the current stream and advances the
        // current position within this stream by the number of bytes written
        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateUnderlyingStream();
            ValidateOffset(offset);

            if (offset != 0)
            {
                byte[] oldBuffer = buffer;
                buffer = GetTempBuffer(count);
                Array.Copy(oldBuffer, offset, buffer, 0, count);
            }

            // Pass "null" for the last parameter since we don't use the value
            m_comStream.Write(buffer, count, IntPtr.Zero);
        }

        // Sets the position within the current stream
        public unsafe override long Seek(long offset, SeekOrigin origin)
        {
            ValidateUnderlyingStream();

            // The enum values of SeekOrigin match the enum values of
            // STREAM_SEEK, so we can just cast the origin to an integer.
            long position = 0;
            IntPtr pPosition = new IntPtr(&position);
            m_comStream.Seek(offset, (int)origin, pPosition);
            return position;
        }

        // Returns the length, in bytes, of the stream
        public override long Length
        {
            get
            {
                ValidateUnderlyingStream();

                // Call IStream.Stat to retrieve info about the stream,
                // which includes the length. STATFLAG_NONAME means that we don't
                // care about the name (STATSTG.pwcsName), so there is no need for
                // the method to allocate memory for the string.
                System.Runtime.InteropServices.ComTypes.STATSTG statstg;
                m_comStream.Stat(out statstg, (int)STATFLAG.NONAME);
                return statstg.cbSize;
            }
        }

        // Determines the position within the current stream
        public override long Position
        {
            get { return Seek(0, SeekOrigin.Current); }
            set { Seek(value, SeekOrigin.Begin); }
        }

        // Sets the length of the current stream
        public override void SetLength(long value)
        {
            ValidateUnderlyingStream();
            m_comStream.SetSize(value);
        }

        // Closes (disposes) the stream
        public override void Close()
        {
            if (m_comStream != null)
            {
                if (CommitOnDispose)
                {
                    m_comStream.Commit((int)STGC.DEFAULT);
                }

                Marshal.ReleaseComObject(m_comStream);
                m_comStream = null;
                GC.SuppressFinalize(this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Close();
            base.Dispose(disposing);
        }

        // Updates the underlying data source or repository with the current
        // state of the buffer and then clears the buffer
        public override void Flush()
        {
            m_comStream.Commit((int)STGC.DEFAULT);
        }

        // Determines whether the current stream supports reading
        public override bool CanRead
        {
            get { return true; }
        }

        // Determines whether the current stream supports writing
        public override bool CanWrite
        {
            // There isn't an exposed way to know whether the stream supports writing
            get { return true; }
        }

        // Determines whether the current stream supports seeking
        public override bool CanSeek
        {
            get { return true; }
        }

        public System.Runtime.InteropServices.ComTypes.STATSTG Stat
        {
            get
            {
                System.Runtime.InteropServices.ComTypes.STATSTG pstatstg;
                m_comStream.Stat(out pstatstg, 0);
                return pstatstg;
            }
        }

        // The com stream being wrapped
        private IStream m_comStream;

        /// <summary>
        /// Helper that validates that the COM stream is available (throws
        /// exception if the stream isn't available).
        /// </summary>
        /// <param name="stream">The UCOMIStream to validate</param>
        private void ValidateUnderlyingStream()
        {
            if (m_comStream == null)
                throw new ObjectDisposedException("m_comStream");
        }

        /// <summary>
        /// Helper the validates that the offset is a valid value (current
        /// implementation requires a zero offset).  Throws a NotSupportedException if
        /// the offset is invalid.
        /// </summary>
        /// <param name="offset">The offset to validate.</param>
        private void ValidateOffset(int offset)
        {
            if (!supportNonZeroOffset && offset != 0)
                throw new NotSupportedException("Only a zero offset is supported.");
        }

        private byte[] tempBuffer = new byte[0];
        private byte[] GetTempBuffer(int minSize)
        {
            if (tempBuffer.Length < minSize)
                tempBuffer = new byte[minSize];
            return tempBuffer;
        }
    }
}
