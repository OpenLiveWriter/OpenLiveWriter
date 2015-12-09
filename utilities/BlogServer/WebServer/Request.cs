using System;
using System.IO;

namespace BlogServer.WebServer
{
	public class Request
	{
		private readonly string _httpMethod;
		private readonly string _path;
		private readonly string _querystring;
		private readonly HttpHeaders _headers;
		private readonly Stream _requestBody;

		public Request(string httpMethod, string path, string querystring, HttpHeaders headers, Stream requestBody)
		{
			_httpMethod = httpMethod;
			_path = path;
			_querystring = querystring;
			_headers = headers;
			_requestBody = requestBody;
		}

		/// <summary>
		/// "GET", "POST", etc.
		/// </summary>
		public string HttpMethod
		{
			get { return _httpMethod; }
		}

		/// <summary>
		/// e.g. "/", "/foo/bar.png"
		/// </summary>
		public string Path
		{
			get { return _path; }
		}

		/// <summary>
		/// e.g. "foo=bar&x=y"
		/// </summary>
		public string Querystring
		{
			get { return _querystring; }
		}

		public HttpHeaders Headers
		{
			get { return _headers; }
		}

		/// <summary>
		/// The data in the body of the request (does not include headers).
		/// </summary>
		public Stream RequestBody
		{
			get { return _requestBody; }
		}
	}
}
