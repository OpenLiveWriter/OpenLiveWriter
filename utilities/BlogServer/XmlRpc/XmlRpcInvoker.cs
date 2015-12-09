using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace BlogServer.XmlRpc
{
	public class XmlRpcInvoker
	{
		private readonly object _target;
		private readonly IXmlRpcDynamicInvoke _dynamicInvoke;
		private readonly Hashtable _methods;
		
		public XmlRpcInvoker(object target)
		{
			_target = target;
			_dynamicInvoke = target as IXmlRpcDynamicInvoke;
			_methods = new Hashtable();
			
			foreach (MethodInfo mi in target.GetType().GetMethods())
			{
				object[] attributes = mi.GetCustomAttributes(typeof (XmlRpcMethodAttribute), true);
				if (attributes.Length > 0)
				{
					XmlRpcMethodAttribute methodAttribute = (XmlRpcMethodAttribute) attributes[0];
					string name = methodAttribute.MethodName;
					if (name == null)
						name = mi.Name;
					if (_methods.ContainsKey(name))
						throw new ArgumentException("Too many implementations for " + name);
					_methods.Add(name, mi);
				}
			}
		}
		
		public bool Invoke(XmlDocument xmlRpcRequest, out object result)
		{
			string methodName;
			object[] parameters;
			XmlRpcParser.Parse(xmlRpcRequest, out methodName, out parameters);
			
			MethodInfo mi = (MethodInfo) _methods[methodName];
			if (mi != null)
			{
				ParameterInfo[] parameterInfos = mi.GetParameters();
				for (int i = 0; i < parameters.Length && i < parameterInfos.Length; i++)
				{
					parameters[i] = XmlRpcSerializer.FromXmlRpc(parameters[i], parameterInfos[i].ParameterType);
				}
				object invokeResult;
				try
				{
					invokeResult = mi.Invoke(_target, parameters);
				}
				catch (TargetInvocationException tie)
				{
					throw tie.InnerException;
				}
				result = XmlRpcSerializer.ToXmlRpc(invokeResult);
				return true;
			}

			if (_dynamicInvoke != null)
			{
				if (_dynamicInvoke.Invoke(methodName, parameters, out result))
				{
					result = XmlRpcSerializer.ToXmlRpc(result);
					return true;
				}
			}
			
			result = null;
			return false;
		}
	}
}
