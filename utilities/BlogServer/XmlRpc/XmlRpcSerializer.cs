using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace BlogServer.XmlRpc
{
	public class XmlRpcSerializer
	{
		public static object ToXmlRpc(object val)
		{
			if (val == null)
				throw new ArgumentNullException("val");
			
			if (val is int || val is bool || val is string || val is double || val is DateTime || val is byte[])
				return val;
			
			if (val is IDictionary)
			{
				IDictionary results = new HybridDictionary();
				IDictionary dict = (IDictionary) val;
				foreach (string key in dict.Keys)
				{
					results[key] = ToXmlRpc(dict[key]);
				}
				return results;
			}

			object[] attributes = val.GetType().GetCustomAttributes(typeof (XmlRpcSerializableAttribute), false);
			if (attributes.Length > 0)
			{
				return ToDictionary(val);
			}
			
			if (val is IEnumerable)
			{
				ArrayList arrList = new ArrayList();
				foreach (object o in (IEnumerable) val)
					arrList.Add(ToXmlRpc(o));
				return arrList.ToArray();
			}

			throw new ArgumentException("Unable to serialize object of type " + val.GetType().Name);
		}
		
		public static object FromXmlRpc(object val, Type desiredType)
		{
			if (desiredType.Equals(typeof(int)) 
				|| desiredType.Equals(typeof(bool))
				|| desiredType.Equals(typeof(string))
				|| desiredType.Equals(typeof(double)) 
				|| desiredType.Equals(typeof(DateTime)) 
				|| desiredType.Equals(typeof(byte[]))
				|| desiredType.Equals(typeof(object[])))
			{
				if (desiredType.IsInstanceOfType(val))
					return val;
			}
			
			object[] attributes = desiredType.GetCustomAttributes(typeof (XmlRpcSerializableAttribute), false);
			if (attributes.Length > 0)
			{
				return FromStruct((IDictionary) val, desiredType);
			}
			
			if (typeof(IDictionary).IsAssignableFrom(desiredType) && desiredType.IsInstanceOfType(val))
			{
				return val;
			}
			
			if (desiredType.IsArray)
			{
				object[] arr = (object[]) val;
				Type elType = desiredType.GetElementType();
				Array instance = Array.CreateInstance(elType, arr.Length);
				for (int i = 0; i < arr.Length; i++)
					instance.SetValue(arr[i], i);
				return instance;
			}
			
			throw new ArgumentException("Cannot coerce type " + val.GetType().Name + " into " + desiredType.Name);
		}

		private static object FromStruct(IDictionary val, Type type)
		{
			object result = Activator.CreateInstance(type);
			foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				object[] attributes = prop.GetCustomAttributes(typeof (XmlRpcStructMemberAttribute), false);
				if (attributes.Length > 0)
				{
					XmlRpcStructMemberAttribute attr = (XmlRpcStructMemberAttribute) attributes[0];
					string name = attr.Name;
					object propValue = val[name];
					if (propValue != null)
					{
						Type propType = prop.PropertyType;
						prop.SetValue(result, FromXmlRpc(propValue, propType), null);
					}
				}
			}
			return result;
		}

		private static IDictionary ToDictionary(object obj)
		{
			HybridDictionary results = new HybridDictionary();
			foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				object[] attributes = prop.GetCustomAttributes(typeof (XmlRpcStructMemberAttribute), false);
				if (attributes.Length > 0)
				{
					XmlRpcStructMemberAttribute attr = (XmlRpcStructMemberAttribute) attributes[0];
					string name = attr.Name;
					object val = prop.GetValue(obj, null);
					if (val != null)
						results[name] = ToXmlRpc(val);
				}
			}
			return results;
		}
		
		private static Array ToArray(IEnumerable obj)
		{
			ArrayList results = new ArrayList();
			foreach (object o in obj)
			{
				results.Add(ToXmlRpc(o));
			}
			return results.ToArray();
		}
	}
}
