using System;

namespace BlogServer.XmlRpc
{
	public interface IXmlRpcDynamicInvoke
	{
		/// <summary>
		/// Return true if handled.
		/// </summary>
		bool Invoke(string methodName, object[] parameters, out object result);
	}
}
