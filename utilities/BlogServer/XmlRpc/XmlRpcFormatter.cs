using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml;

namespace BlogServer.XmlRpc
{
	class XmlRpcFormatter
	{
		public static XmlDocument EncodeRequest(string methodName, params object[] parameters)
		{
			XmlDocument doc = new XmlDocument();
			
			XmlElement methodCall = CreateElement(doc, "methodCall");
			XmlElement methodNameEl = CreateElement(methodCall, "methodName");
			methodNameEl.InnerText = methodName;
			XmlElement paramsEl = CreateElement(methodCall, "params");
			foreach (object o in parameters)
			{
				XmlElement paramEl = CreateElement(paramsEl, "param", "value");
				EncodeValue(paramEl, o);
			}
			
			return doc;
		}
		
		public static XmlDocument EncodeResponse(object val)
		{
			XmlDocument doc = new XmlDocument();
			
			XmlElement param = CreateElement(doc, "methodResponse", "params", "param", "value");
			EncodeValue(param, val);
			
			return doc;
		}
		
		public static XmlDocument EncodeFault(int faultCode, string faultString)
		{
			XmlDocument doc = new XmlDocument();
			
			XmlElement faultStruct = CreateElement(doc, "methodResponse", "fault", "value");
			ListDictionary faultDict = new ListDictionary();
			faultDict["faultCode"] = faultCode;
			faultDict["faultString"] = faultString;
			
			EncodeStruct(faultStruct, faultDict);
			return doc;
		}

		private static void EncodeValue(XmlElement parent, object val)
		{
			if (val == null)
				throw new ArgumentNullException("val");
			else if (val is int)
				EncodeScalarValue(parent, "i4", ((int) val).ToString(CultureInfo.InvariantCulture));
			else if (val is bool)
				EncodeScalarValue(parent, "boolean", (bool) val ? "1" : "0");
			else if (val is string)
				EncodeScalarValue(parent, "string", val.ToString());
			else if (val is double)
				EncodeScalarValue(parent, "double", ((double) val).ToString(CultureInfo.InvariantCulture));
			else if (val is DateTime)
				EncodeScalarValue(parent, "dateTime.iso8601",
				                  ((DateTime) val).ToString("yyyyMMdd'T'HH:mm:ss", CultureInfo.InvariantCulture));
			else if (val is byte[])
				EncodeScalarValue(parent, "base64", Convert.ToBase64String((byte[]) val));
			else if (val is IDictionary)
				EncodeStruct(parent, (IDictionary) val);
			else if (val is Array)
				EncodeArray(parent, (Array) val);
			else
				throw new ArgumentException("Don't know how to encode object of type " + val.GetType());
		}

		private static void EncodeScalarValue(XmlElement parent, string typeName, string val)
		{
			XmlElement leaf = parent.OwnerDocument.CreateElement(typeName);
			parent.AppendChild(leaf);
			leaf.InnerText = val;
		}

		private static void EncodeStruct(XmlElement parent, IDictionary val)
		{
			XmlElement structEl = CreateElement(parent, "struct");

			foreach (DictionaryEntry entry in val)
			{
				// HACK: deal with empty values
				if (entry.Value == null || (entry.Value is DateTime && entry.Value.Equals(DateTime.MinValue)))
					continue;
				
				XmlElement memberEl = CreateElement(structEl, "member");
				
				XmlElement nameEl = CreateElement(memberEl, "name");
				nameEl.InnerText = (string) entry.Key;
				
				XmlElement valueEl = CreateElement(memberEl, "value");
				EncodeValue(valueEl, entry.Value);
			}
		}

		private static void EncodeArray(XmlElement parent, Array val)
		{
			XmlElement dataEl = CreateElement(parent, "array", "data");
			
			foreach (object o in val)
			{
				XmlElement valueEl = CreateElement(dataEl, "value");
				EncodeValue(valueEl, o);
			}
		}

		private static XmlElement CreateElement(XmlNode parent, params string[] elementNames)
		{
			XmlDocument ownerDoc = parent is XmlDocument ? (XmlDocument) parent : parent.OwnerDocument;
			XmlNode currentNode = parent;
			foreach (string elName in elementNames)
			{
				XmlElement el = ownerDoc.CreateElement(elName);
				currentNode.AppendChild(el);
				currentNode = el;
			}
			return (XmlElement) currentNode;
		}
	}
}
