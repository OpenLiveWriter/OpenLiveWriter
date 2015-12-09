using System;
using System.IO;
using System.Net.Sockets;

namespace BlogServer.WebServer
{
	public class ResponseStream : Stream
	{
		private readonly Socket _socket;

		public ResponseStream(Socket socket)
		{
			_socket = socket;
		}
		
		public override void Close()
		{
			_socket.Shutdown(SocketShutdown.Send);
			_socket.Close();
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

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_socket.Send(buffer, offset, count, SocketFlags.None);
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
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
	}
}
