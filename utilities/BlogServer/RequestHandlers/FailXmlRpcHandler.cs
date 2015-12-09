using System;
using System.Xml;
using BlogServer.XmlRpc;

namespace BlogServer.RequestHandlers
{
	/// <summary>
	/// Causes XML-RPC requests to fail with a specific fault code and fault string.
	/// 
	/// Config:
	/// methodName attribute (optional): Specifies the XML-RPC method that should fail.
	///		If this attribute is not provided, all methods will fail.
	/// faultCode attribute (optional, defaults to 0): The XML-RPC fault code to use.
	/// faultString attribute (optional, defaults to "[Unspecified error]"): The fault
	///		string to use.
	/// </summary>
	public class FailXmlRpcHandler : XmlRpcHttpRequestHandler, IXmlRpcDynamicInvoke
	{
		private readonly string _methodName;
		private readonly int _faultCode;
		private readonly string _faultString;
		
		public FailXmlRpcHandler(XmlElement configElement) : this(
			XmlUtil.ReadString(configElement, "@methodName", null),
			XmlUtil.ReadInt(configElement, "@faultCode", 0),
			XmlUtil.ReadString(configElement, "@faultString", "[Unspecified error]"))
		{
		}
		
		public FailXmlRpcHandler(string methodName, int faultCode, string faultString) : base(false)
		{
			_methodName = methodName;
			_faultCode = faultCode;
			_faultString = faultString;
		}

		public bool Invoke(string methodName, object[] parameters, out object result)
		{
			if (_methodName == null || methodName == _methodName)
				throw new XmlRpcServerException(_faultCode, _faultString);
			result = null;
			return false;
		}
	}
}
