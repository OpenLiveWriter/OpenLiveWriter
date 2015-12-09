using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using BlogServer.WebServer;

namespace BlogServer.RequestHandlers
{
	/// <summary>
	/// Causes an HTTP error.
	/// 
	/// Config:
	/// pattern attribute (optional): Only handle requests whose paths match the pattern regex.
	/// errorCode attribute (optional, defaults to 500): The HTTP error code to return.
	/// errorMessage attribute (optional, defaults to "An unknown error has occurred"): The error message to return.
	/// </summary>
	public class HttpErrorHandler : HttpRequestHandler
	{
		private Regex pattern;
		private int errorCode;
		private string errorMessage;
		
		public HttpErrorHandler(XmlElement configEl)
		{
			pattern = new Regex(XmlUtil.ReadString(configEl, "@pattern", ""), RegexOptions.IgnoreCase);
			errorCode = XmlUtil.ReadInt(configEl, "@errorCode", 500);
			errorMessage = XmlUtil.ReadString(configEl, "@errorMessage", "An unknown error has occurred");
		}
		
		public override bool Handle(Request request, Response response)
		{
			if (pattern.IsMatch(request.Path))
			{
				response.SendErrorCode(errorCode, errorMessage);
				return true;
			}
			else
				return false;
		}
	}
}
