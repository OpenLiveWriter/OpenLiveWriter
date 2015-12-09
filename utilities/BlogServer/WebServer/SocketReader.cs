using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace BlogServer.WebServer
{
	public class SocketReader : Stream
	{
		private readonly Socket socket;
		private int expectedLength;
		private int totalBytesRead = 0;
		private readonly byte[] buffer;
		private int pos;
		private int len;

		public SocketReader(Socket socket, int bufferSize) : this(socket, bufferSize, int.MaxValue)
		{
		}
		
		public SocketReader(Socket socket, int bufferSize, int bytesRemaining)
		{
			this.socket = socket;
			this.expectedLength = bytesRemaining;
			this.buffer = new byte[bufferSize];
			this.pos = 0;
			this.len = 0;
		}

		#region Stream
		
		public void SetBytesRemaining(int bytesRemaining)
		{
			int bytesInBuffer = len - pos;
			expectedLength = totalBytesRead - bytesInBuffer + bytesRemaining;
		}
		
		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buf, int offset, int count)
		{
			if (pos >= len && Fill() <= 0)
			{
				return 0;
			}
			
			int bytesToCopy = Math.Min(count, len - pos);
			Array.Copy(buffer, pos, buf, offset, bytesToCopy);
			pos += bytesToCopy;
			return bytesToCopy;
		}

		public override void Write(byte[] buf, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		#endregion

		public string NextLine()
		{
			StringBuilder result = new StringBuilder();
			int c;
			while (-1 != (c = NextChar()))
			{
				result.Append((char)c);
				if (c == '\n' && result.Length > 1 && result[result.Length - 2] == '\r')
					break;
			}
			if (result.Length > 0)
				return result.ToString();
			else
				return null;
		}

		public int NextChar()
		{
			if (pos < len || Fill() > 0)
				return buffer[pos++];
			return -1;
		}

		private int Fill()
		{
			this.pos = 0;
			this.len = 0;

			if (this.totalBytesRead >= this.expectedLength)
				return 0;

			this.len = socket.Receive(buffer);
			this.totalBytesRead += len;
			return this.len;
		}

	}
}
