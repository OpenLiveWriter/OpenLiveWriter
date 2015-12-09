using System;
using System.IO;

namespace BlogServer.WebServer
{
	/// <summary>
	/// Filters wrap the handling of HTTP requests; they are
	/// able to modify the request on the way in and modify the
	/// response stream on the way out. They can also prevent
	/// downstream filters and handlers from seeing the HTTP 
	/// request (by simply not invoking base.Filter(...) or 
	/// NextFilter.Filter(...)).
	/// </summary>
	public abstract class HttpRequestFilter
	{
		private HttpRequestFilter _nextFilter;
		
		public HttpRequestFilter()
		{
		}
		
		internal void SetNextRequestFilter(HttpRequestFilter nextFilter)
		{
			_nextFilter = nextFilter;
		}

		protected HttpRequestFilter NextFilter
		{
			get { return _nextFilter; }
		}
		
		/// <summary>Subclasses should override this to provide filtering behavior.</summary>
		/// <param name="httpMethod">e.g. "GET", "POST".</param>
		/// <param name="path">e.g. "/", "/foo/bar.png"</param>
		/// <param name="querystring">e.g. "foo=bar&x=y"</param>
		/// <param name="headers">The request headers.</param>
		/// <param name="requestBody">The data in the body of the request (does not include headers).</param>
		/// <param name="responseStream">The response stream. Use HttpHelper class to help form valid responses.</param>
		/// <returns>True if handled. If false, writes to the responseStream will be reverted.</returns>
		protected internal virtual bool Filter(Request request, Response response)
		{
			return NextFilter.Filter(request, response);
		}
	}
}
