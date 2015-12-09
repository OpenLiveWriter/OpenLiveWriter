using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace BlogServer.WebServer
{
	public class StreamHelper
	{
		public static void Write(Stream stream, string format, params object[] parameters)
		{
			string s = format;
			if (parameters != null && parameters.Length > 0)
				s = string.Format(format, parameters);

			const int BLOCK_SIZE = 4096;
			byte[] buf = new byte[Math.Min(BLOCK_SIZE, s.Length)];
			for (int i = 0; i < s.Length; i += BLOCK_SIZE)
			{
				int bytec = Encoding.ASCII.GetBytes(s, i, Math.Min(BLOCK_SIZE, s.Length - i), buf, 0);
				stream.Write(buf, 0, bytec);
			}
		}

		public static void WriteLine(Stream stream, string s, params object[] parameters)
		{
			Write(stream, s + "\r\n", parameters);
		}

		public static void Transfer(Stream inStream, Stream outStream)
		{
			const int BLOCK_SIZE = 8192;
			byte[] buf = new byte[BLOCK_SIZE];
			int cnt;
			while (0 != (cnt = inStream.Read(buf, 0, BLOCK_SIZE)))
			{
				outStream.Write(buf, 0, cnt);
			}
		}
	}
}
