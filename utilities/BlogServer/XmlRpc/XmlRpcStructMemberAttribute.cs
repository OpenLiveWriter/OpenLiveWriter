using System;

namespace BlogServer.XmlRpc
{
	/// <summary>
	/// Summary description for XmlRpcStructMemberAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class XmlRpcStructMemberAttribute : Attribute
	{
		private readonly string _name;

		public XmlRpcStructMemberAttribute(string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}
	}
}
