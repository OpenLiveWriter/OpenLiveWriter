using System;
using System.IO;
using System.Net;
using System.Xml;
using BlogServer;
using BlogServer.XmlRpc;

namespace BlogServer.RequestHandlers
{
	/// <summary>
	/// Proxies calls to a remote XML-RPC endpoint.
	/// 
	/// Config:
	/// url attribute (required): The URL of the remote XML-RPC endpoint.
	/// </summary>
	public class XmlRpcProxyHandler : XmlRpcHttpRequestHandler, IXmlRpcDynamicInvoke
	{
		private readonly string _url;

		public XmlRpcProxyHandler(XmlElement configElement) : this(XmlUtil.ReadString(configElement, "@url", null))
		{
		}
		
		public XmlRpcProxyHandler(string url) : base(true)
		{
			if (url == null)
				throw new ArgumentNullException("XmlRpcProxyHandler requires a url attribute");
			_url = url;
		}

		public bool Invoke(string methodName, object[] parameters, out object result)
		{
			XmlDocument doc = XmlRpcFormatter.EncodeRequest(methodName, parameters);
			HttpWebRequest req = (HttpWebRequest) HttpWebRequest.Create(_url);
			req.ContentType = "text/xml";
			req.Method = "POST";
			using (Stream s = req.GetRequestStream())
			{
				doc.Save(s);
			}
			
			using (WebResponse response = req.GetResponse())
			{
				using (Stream s = response.GetResponseStream())
				{
					XmlDocument responseDoc = new XmlDocument();
					responseDoc.Load(s);
					result = XmlRpcParser.ParseResponse(responseDoc);
					return true;
				}
			}
		}
	}
}
