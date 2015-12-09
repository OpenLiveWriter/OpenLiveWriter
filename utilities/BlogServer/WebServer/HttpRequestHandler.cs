using System;
using System.IO;

namespace BlogServer.WebServer
{
	/// <summary>
	/// A handler provides responses to HTTP requests. For any
	/// given request, the handler can decide whether it is
	/// able to provide a response or not. If not, the server
	/// will try the next handler in the list.
	/// 
	/// If no handlers are able to handle a request, the server 
	/// will return an HTTP 404 error.
	/// </summary>
	public abstract class HttpRequestHandler
	{
		/// <returns>True if handled. If false, writes to the response will be reverted.</returns>
		public abstract bool Handle(Request request, Response response);
	}
}
