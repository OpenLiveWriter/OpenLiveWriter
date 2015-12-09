using System;
using System.IO;

namespace BlogServer.WebServer
{
	public class Response
	{
		private readonly Stream _stream;

		public Response(Stream stream)
		{
			_stream = stream;
		}

		public Stream Stream
		{
			get { return _stream; }
		}
		
		public void SendHtml(string html)
		{
			HttpHelper.SendHtml(_stream, html);
		}
		
		public void SendRedirect(string url, bool temporary)
		{
			HttpHelper.SendRedirect(_stream, url, temporary);
		}
		
		public void SendErrorCode(int httpErrorCode, string errorMessage)
		{
			HttpHelper.SendErrorCode(_stream, httpErrorCode, errorMessage);
		}
		
		public void SendFile(string path, string contentType)
		{
			HttpHelper.SendFile(_stream, path, contentType);
		}
		
		public void SendData(string contentType, Stream data)
		{
			HttpHelper.SendData(_stream, contentType, data);
		}
		
		public void SendGoBack()
		{
			HttpHelper.SendGoBack(_stream);
		}
	}
}
