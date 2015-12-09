using System;
using System.IO;
using System.Xml;

namespace BlogServer.WebServer
{
	public abstract class XmlHttpRequestHandler : HttpRequestHandler
	{
		public override bool Handle(Request request, Response response)
		{
			XmlDocument requestDocument = new XmlDocument();
			requestDocument.Load(request.RequestBody);
			XmlDocument responseDocument;
			if (Handle(request.HttpMethod, request.Path, request.Querystring, request.Headers, requestDocument, out responseDocument))
			{
				Stream docStream = new MemoryStream();
				responseDocument.Save(docStream);
				docStream.Seek(0, SeekOrigin.Begin);
				response.SendData(ContentType, docStream);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Override in subclasses to change the content type of the response.
		/// </summary>
		protected virtual string ContentType
		{
			get { return "text/xml"; }
		}

		public abstract bool Handle(string httpMethod, string path, string querystring, HttpHeaders headers, XmlDocument request, out XmlDocument response);
	}
}
