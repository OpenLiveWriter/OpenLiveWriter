using System;
using System.IO;
using System.Xml;
using BlogServer.WebServer;

namespace BlogServer.RequestFilters
{
	public class TraceFilter : HttpRequestFilter
	{
		public TraceFilter(XmlElement configEl)
		{
		}
		
		protected internal override bool Filter(Request request, Response response)
		{
			Console.WriteLine("REQUEST: {0} {1}{2}", request.HttpMethod, request.Path, request.Querystring != null ? "?" + request.Querystring : "");
			PrintStream(request.RequestBody);
			request.RequestBody.Seek(0, SeekOrigin.Begin);
			bool result = base.Filter (request, response);
			if (result)
			{
				response.Stream.Seek(0, SeekOrigin.Begin);
				Console.WriteLine();
				Console.WriteLine("RESPONSE:");
				PrintStream(response.Stream);
				Console.WriteLine();
			}
			return result;
		}

		private void PrintStream(Stream stream)
		{
			Console.WriteLine(new StreamReader(stream).ReadToEnd());
		}
	}
}
