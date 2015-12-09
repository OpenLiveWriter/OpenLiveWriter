using System;
using System.IO;
using System.Net.Sockets;

namespace BlogServer.WebServer
{
	public class SocketStream : Stream
	{
		private Socket _socket;
		private readonly bool _owned;

		public SocketStream(Socket socket, bool owned)
		{
			_socket = socket;
			_owned = owned;
		}
		
		public override void Close()
		{
			if (_owned)
			{
				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();
			}
			base.Close ();
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
			if (_socket.Available == 0)
				return 0;
			
			return _socket.Receive(buffer, offset, count, SocketFlags.None);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_socket.Send(buffer, offset, count, SocketFlags.None);
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
