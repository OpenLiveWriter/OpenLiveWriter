// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.CoreServices
{
    public class CancelableStream : Stream
    {
        private volatile bool _isCancelled;
        private Stream _innerStream;
        public CancelableStream(Stream innerStream)
        {
            _isCancelled = false;
            _innerStream = innerStream;
        }

        public override bool CanRead
        {
            get
            {
                return !_isCancelled && !ThreadHelper.Interrupted && _innerStream.CanRead;
            }
        }

        public override int Read(byte[] array, int offset, int count)
        {
            if (_isCancelled || ThreadHelper.Interrupted)
            {
                throw new OperationCanceledException("Stream has been cancelled.");
            }
            return _innerStream.Read(array, offset, count);
        }

        public override int ReadByte()
        {
            if (_isCancelled || ThreadHelper.Interrupted)
            {
                throw new OperationCanceledException("Stream has been cancelled.");
            }
            return base.ReadByte();
        }

        public override void Close()
        {
            _innerStream.Close();
            _isCancelled = true;
        }

        public void Cancel()
        {
            _isCancelled = true;
            Close();
        }

        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Length
        {
            get { return _innerStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _innerStream.Position;
            }
            set
            {
                _innerStream.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            _innerStream.Dispose();
            base.Dispose(disposing);
        }
    }
}
