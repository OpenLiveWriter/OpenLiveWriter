using System;
using System.IO;
using System.Text;
using System.Xml;
using BlogServer.WebServer;

namespace BlogServer.RequestHandlers
{
	/// <summary>
	/// Returns the body of the XML config element as a response.
	/// 
	/// Config:
	/// contentType attribute (optional, defaults to "text/html"): The content type to return.
	/// 
	/// Example:
	/// <handler className="BlogServer.RequestHandlers.StringHandler" contentType="text/css"><![CDATA[
	/// body {
	///		font-family: Verdana;
	/// }
	/// ]]></![CDATA[></handler>
	/// 
	/// </summary>
	public class StringHandler : HttpRequestHandler
	{
		private readonly string _contentType;
		private readonly string _strValue;

		public StringHandler(XmlElement configEl) : this(XmlUtil.ReadString(configEl, "@contentType", null), configEl.InnerText, XmlUtil.ReadBool(configEl, "@preserveWhitespace", false))
		{
		}
		
		public StringHandler(string contentType, string strValue, bool preserveWhitespace)
		{
			_contentType = contentType == null ? "text/html" : contentType;
			_strValue = preserveWhitespace ? strValue : StringUtil.StripIndentation(strValue).Trim();
		}
		
		public override bool Handle(Request request, Response response)
		{
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(_strValue)))
			{
				response.SendData(_contentType, ms);
			}
			return true;
		}
	}
}
