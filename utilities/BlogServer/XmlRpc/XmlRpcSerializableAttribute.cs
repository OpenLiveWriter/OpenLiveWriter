using System;

namespace BlogServer.XmlRpc
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
	public class XmlRpcSerializableAttribute : Attribute
	{
	}
}
