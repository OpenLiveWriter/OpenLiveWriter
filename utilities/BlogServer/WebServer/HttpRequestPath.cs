using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace BlogServer.WebServer
{
	internal class HttpRequestPath
	{
		private HttpRequestFilter _head;
		private HttpRequestFilter _tail;
		private HttpRequestFilter _default;
		
		private ArrayList _handlers = new ArrayList();


		public HttpRequestPath()
		{
			_default = new DefaultFilter(this);
		}

		public void Register(Regex pattern, TrivialWebServer tws)
		{
			tws.RegisterHandler(pattern, new RequestHandler(Handler));
		}
		
		public void AddFilter(HttpRequestFilter filter)
		{
			if (_head == null)
				_head = filter;
			
			if (_tail != null)
			{
				_tail.SetNextRequestFilter(filter);
			}
			_tail = filter;
			_tail.SetNextRequestFilter(_default);
		}

		public void AddHandler(HttpRequestHandler handler)
		{
			_handlers.Add(handler);
		}
		
		private bool Handler(string method, string uri, HttpHeaders headers, SocketReader reader, Socket socket)
		{
			if (_handlers.Count == 0)
				return false;
			
			string path, querystring;
			int qindex = uri.IndexOf('?');
			if (qindex < 0)
			{
				path = uri;
				querystring = null;
			}
			else
			{
				path = uri.Substring(0, qindex);
				querystring = qindex == (uri.Length - 1) ? "" : uri.Substring(qindex + 1);
			}
			
			MemoryStream requestBodyStream = new MemoryStream();
			byte[] buffer = new byte[8192];
			int bytesRead;
			while (0 != (bytesRead = reader.Read(buffer, 0, buffer.Length)))
			{
				requestBodyStream.Write(buffer, 0, bytesRead);
			}
			requestBodyStream.Seek(0, SeekOrigin.Begin);
			
			Stream readOnlyRequestBodyStream = new ReadOnlyStream(requestBodyStream);
			readOnlyRequestBodyStream.Seek(0, SeekOrigin.Begin);

			Request request = new Request(method, path, querystring, headers, readOnlyRequestBodyStream);
			Response response = new Response(new MemoryStream());
			if (StartFilter.Filter(request, response))
			{
				response.Stream.Seek(0, SeekOrigin.Begin);
				StreamHelper.Transfer(response.Stream, new SocketStream(socket, false));
				return true;
			}
			return false;
		}

		private HttpRequestFilter StartFilter
		{
			get { return _head != null ? _head : _default; }
		}

		private bool InvokeHandlers(Request request, Response response)
		{
			foreach (HttpRequestHandler handler in _handlers)
			{
				request.RequestBody.Seek(0, SeekOrigin.Begin);
				
				Response tempResponse = new Response(new MemoryStream());
				if (handler.Handle(request, tempResponse))
				{
					tempResponse.Stream.Seek(0, SeekOrigin.Begin);
					StreamHelper.Transfer(tempResponse.Stream, response.Stream);
					
					return true;
				}
			}
			return false;
		}

		private class DefaultFilter : HttpRequestFilter
		{
			private readonly HttpRequestPath _parent;

			public DefaultFilter(HttpRequestPath parent)
			{
				_parent = parent;
			}

			protected internal override bool Filter(Request request, Response response)
			{
				return _parent.InvokeHandlers(request, response);
			}
		}
	}
}
