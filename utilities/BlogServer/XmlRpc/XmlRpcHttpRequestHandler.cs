using System;
using System.Xml;
using BlogServer.WebServer;

namespace BlogServer.XmlRpc
{
	public class XmlRpcHttpRequestHandler : XmlHttpRequestHandler
	{
		private readonly bool _final;
		private readonly XmlRpcInvoker _invoker;

		public XmlRpcHttpRequestHandler(bool final)
		{
			_invoker = new XmlRpcInvoker(this);
			_final = final;
		}

		public override bool Handle(string httpMethod, string path, string querystring, HttpHeaders headers, XmlDocument request, out XmlDocument response)
		{
			response = null;
			
			try
			{
				object result;
				if (_invoker.Invoke(request, out result))
				{
					response = XmlRpcFormatter.EncodeResponse(result);
					return true;
				}
				if (_final)
				{
					string methodName;
					object[] parameters;
					XmlRpcParser.Parse(request, out methodName, out parameters);
					response = XmlRpcFormatter.EncodeFault(405, "XML-RPC method '" +  methodName + "' not implemented");
					return true;
				}
				return false;
			}
			catch (XmlRpcServerException xpse)
			{
				response = XmlRpcFormatter.EncodeFault(xpse.FaultCode, xpse.FaultString);
			}
			catch (Exception e)
			{
				response = XmlRpcFormatter.EncodeFault(500, e.Message);
			}
			return true;
		}
	}
}
