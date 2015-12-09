using System;
using System.IO;
using System.Net.Sockets;
using System.Web;

namespace BlogServer.WebServer
{
	public class HttpHelper
	{
		public static void SendRedirect(Socket socket, string url, bool temporary)
		{
			SendRedirect(new SocketStream(socket, false), url, temporary);
		}
		
		public static void SendRedirect(Stream stream, string url, bool temporary)
		{
			StreamHelper.Write(stream, 
				"HTTP/1.0 " + (temporary ? "302" : "301") + " OK\r\n" +
				"Content-Type: text/html\r\n" +
				"Location: " + url + "\r\n" +
				"Connection: close\r\n" +
				"\r\nRedirecting to " + url
				);
		}

		public static void SendHtml(Socket socket, string html)
		{
			SendHtml(new SocketStream(socket, false), html);
		}
		
		public static void SendHtml(Stream stream, string html)
		{
			StreamHelper.Write(stream,
				"HTTP/1.0 200 OK\r\n" +
				"Content-Type: text/html\r\n" +
				"\r\n" +
				html);
		}

		public static void SendGoBack(Socket socket)
		{
			SendGoBack(new SocketStream(socket, false));
		}
		
		public static void SendGoBack(Stream stream)
		{
			SendHtml(stream, @"<html><head><script language=""JavaScript"">history.back();</script></head></html>");
		}

		public static void SendFile(Socket socket, string path, string contentType)
		{
			SendFile(new SocketStream(socket, false), path, contentType);
		}
		
		public static void SendFile(Stream stream, string path, string contentType)
		{
			FileInfo file = new FileInfo(path);

			using (Stream s = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				SendData(stream, contentType, s);
			}
		}

		public static void SendErrorCode(Socket socket, int errorCode, string errorMessage)
		{
			SendErrorCode(new SocketStream(socket, false), errorCode, errorMessage);
		}
		
		public static void SendErrorCode(Stream stream, int errorCode, string errorMessage)
		{
			StreamHelper.Write(stream,
				"HTTP/1.0 {0} {1}\r\n\r\n<html><head><title>{2}</title></head><body><h1>{0} {2}</h1></body></html>",
				errorCode,
				errorMessage,
				HttpUtility.HtmlEncode(errorMessage)
				);
		}

		public static void SendData(Socket socket, string contentType, Stream stream)
		{
			SendData(new SocketStream(socket, false), contentType, stream);
		}
		
		public static void SendData(Stream outStream, string contentType, Stream stream)
		{
			StreamHelper.Write(outStream, "HTTP/1.0 200 OK\r\n");
			if (contentType != null)
				StreamHelper.Write(outStream, "Content-Type: " + contentType + "\r\n");
			if (stream.CanSeek)
				StreamHelper.Write(outStream, "Content-Length: " + (stream.Length - stream.Position) + "\r\n");
			StreamHelper.Write(outStream, "\r\n");
			
			StreamHelper.Transfer(stream, outStream);
		}
	}
}
