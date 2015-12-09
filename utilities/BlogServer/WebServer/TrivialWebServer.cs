using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace BlogServer.WebServer
{
	internal delegate bool RequestHandler(string method, string uri, HttpHeaders headers, SocketReader reader, Socket socket);

	/// <summary>
	/// TrivialWebServer webServer = new TrivialWebServer(HTTP_PORT, 20);
	/// Trace.WriteLine("Listening on port: " + HTTP_PORT);
	/// ItemViewer.Register(webServer);
	/// FeedViewer.Register(webServer);
	/// FolderLauncher.Register(webServer);
	/// Trace.WriteLine("Ready");
	/// webServer.Accept();
	/// </summary>
	internal class TrivialWebServer : IDisposable
	{
		private readonly bool localOnly;
		private readonly Socket socket;
		private readonly Regex reqLine = new Regex(@"^(\w+) ([^\s]+) ([^\s]+)\r\n.*$", RegexOptions.Compiled | RegexOptions.Singleline);

		private readonly ArrayList handlers = ArrayList.Synchronized(new ArrayList());

		public TrivialWebServer(IPEndPoint endPoint, bool localOnly)
		{
			this.localOnly = localOnly;
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(endPoint);
			socket.Listen(20);
		}
		
		public event EventHandler HandleConnectionBegin;

		public EndPoint Endpoint
		{
			get { return socket.LocalEndPoint; }
		}

		public void RegisterHandler(Regex pattern, RequestHandler handler)
		{
			handlers.Add(new HandlerRegistration(pattern, handler));
		}
		
		public void ClearHandlers()
		{
			handlers.Clear();
		}

		public void Accept()
		{
			while (true)
			{
				try
				{
					Socket remote = socket.Accept();
					HandleConnection(remote);
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (Exception e)
				{
					Trace.WriteLine(e.ToString());
				}
			}
		}

		private void HandleConnection(Socket remote)
		{
			try
			{
				if (localOnly && !((IPEndPoint)remote.RemoteEndPoint).Address.Equals(IPAddress.Loopback))
				{
					Trace.WriteLine("Rejecting non-loopback connection from remote host");
					return;
				}
				else
				{
					SocketReader reader = new SocketReader(remote, 2048);
					
					string val = reader.NextLine();
					Match match = reqLine.Match(val);
					if (!match.Success)
					{
						Trace.WriteLine("Rejecting malformed HTTP query: " + val);
						return;
					}
					else
					{
						HttpHeaders headers = new HttpHeaders(reader);
						
						string method = match.Groups[1].Value;
						string uri = match.Groups[2].Value;
						string version = match.Groups[3].Value;
						
						if (headers.ContentLength >= 0)
							reader.SetBytesRemaining(headers.ContentLength);
						else if (string.Compare(method, "GET", true, CultureInfo.InvariantCulture) == 0)
							reader.SetBytesRemaining(0);

						// Console.WriteLine("URI: " + uri);
						
						OnHandleConnectionBegin();

						bool success = false;
						for (int i = 0; i < handlers.Count; i++)
						{
							HandlerRegistration reg = (HandlerRegistration) handlers[i];
							if (reg.Pattern.IsMatch(uri))
							{
								if (reg.Handler(method.ToUpper(CultureInfo.InvariantCulture), uri, headers, reader, remote))
								{
									success = true;
									break;
								}
							}
						}

						if (!success)
							HttpHelper.SendErrorCode(remote, 404, "Not found");
						return;
					}
				}
			}
			finally
			{
				remote.Shutdown(SocketShutdown.Both);
				remote.Close();
			}
		}

		protected virtual void OnHandleConnectionBegin()
		{
			if (HandleConnectionBegin != null)
				HandleConnectionBegin(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			socket.Close();
		}


		private class HandlerRegistration
		{
			private readonly Regex pattern;
			private readonly RequestHandler handler;

			public HandlerRegistration(Regex pattern, RequestHandler handler)
			{
				this.pattern = pattern;
				this.handler = handler;
			}

			public Regex Pattern
			{
				get { return pattern; }
			}

			public RequestHandler Handler
			{
				get { return handler; }
			}
		}
	}
}
