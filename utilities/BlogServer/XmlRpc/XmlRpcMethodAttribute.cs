using System;

namespace BlogServer.XmlRpc
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class XmlRpcMethodAttribute : Attribute
	{
		private readonly string methodName;

		public XmlRpcMethodAttribute(string methodName)
		{
			this.methodName = methodName;
		}

		public XmlRpcMethodAttribute()
		{
			this.methodName = null;
		}

		/// <summary>
		/// If null, use the name of the class method to 
		/// which the attribute is applied.
		/// </summary>
		public string MethodName
		{
			get { return methodName; }
		}
	}
}
