using System;
using System.Xml;

namespace BlogServer.XmlRpc
{
	public class XmlRpcMalformedResponseException : Exception
	{
		private readonly XmlDocument _response;

		public XmlRpcMalformedResponseException(XmlDocument response)
		{
			_response = response;
		}


		public XmlDocument Response
		{
			get { return _response; }
		}
	}

	public class XmlRpcServerException : Exception
	{
		private readonly int _faultCode;
		private readonly string _faultString;

		public XmlRpcServerException(int faultCode, string faultString) : base("Error " + faultCode + ": " + faultString)
		{
			_faultCode = faultCode;
			_faultString = faultString;
		}

		public int FaultCode
		{
			get { return _faultCode; }
		}

		public string FaultString
		{
			get { return _faultString; }
		}
	}
}
