using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml;

namespace BlogServer.XmlRpc
{
	class XmlRpcParser
	{
		public static void Parse(XmlDocument request, out string methodName, out object[] parameters)
		{
			XmlText name = (XmlText) request.SelectSingleNode("methodCall/methodName/text()");
			if (name == null)
				throw new ArgumentException("Invalid XML-RPC request: methodName not found");
			methodName = name.Value;
			
			ArrayList paramList = new ArrayList();
			foreach (XmlElement param in request.SelectNodes("methodCall/params/param/value"))
				paramList.Add(ParseValue(param));
			parameters = paramList.ToArray();
		}

		public static object ParseResponse(XmlDocument responseDoc)
		{
			XmlElement valueEl = (XmlElement) responseDoc.SelectSingleNode("methodResponse/params/param/value");
			if (valueEl != null)
				return ParseValue(valueEl);
			
			XmlElement faultValueEl = (XmlElement) responseDoc.SelectSingleNode("methodResponse/fault/value");
			if (faultValueEl != null)
			{
				IDictionary dict = ParseValue(faultValueEl) as IDictionary;
				int faultCode = (int) dict["faultCode"];
				string faultString = (string) dict["faultString"];
				throw new XmlRpcServerException(faultCode, faultString);
			}
			
			throw new XmlRpcMalformedResponseException(responseDoc);
		}

		private static object ParseValue(XmlElement el)
		{
			XmlNode child = el.FirstChild;
			if (child is XmlElement)
			{
				XmlElement childEl = (XmlElement) child;
				string rawValue = childEl.InnerText;
				switch (childEl.Name)
				{
					case "i4":
					case "int":
						return int.Parse(rawValue.Trim(), CultureInfo.InvariantCulture);
					case "boolean":
					switch (rawValue.Trim())
					{
						case "0":
							return false;
						case "1":
							return true;
						default:
							throw new ArgumentException("Couldn't parse bool value \"" + rawValue + "\"");
					}
					case "string":
						return rawValue;
					case "double":
						return double.Parse(rawValue.Trim(), CultureInfo.InvariantCulture);
					case "dateTime.iso8601":
						return DateTime.ParseExact(rawValue.Trim(), "yyyyMMdd'T'HH:mm:ss", CultureInfo.InvariantCulture);
					case "base64":
						return Convert.FromBase64String(rawValue.Trim());
					case "struct":
						return ParseStruct(childEl);
					case "array":
						return ParseArray(childEl);
					default:
						throw new ArgumentException("Unknown XML-RPC data type \"" + childEl.Name + "\"");
				}
			}
			else
			{
				return el.InnerText;
			}
		}

		private static IDictionary ParseStruct(XmlElement el)
		{
			HybridDictionary dictionary = new HybridDictionary();
			foreach (XmlElement member in el.SelectNodes("member"))
			{
				XmlText name = (XmlText) member.SelectSingleNode("name/text()");
				XmlElement val = (XmlElement) member.SelectSingleNode("value");
				
				if (name == null)
					throw new ArgumentException("Invalid XML-RPC struct; each member requires a name");
				if (val == null)
					throw new ArgumentException("Invalid XML-RPC struct; each member requires a value");
				
				dictionary.Add(name.Value, ParseValue(val));
			}
			return dictionary;
		}

		private static object[] ParseArray(XmlElement el)
		{
			ArrayList array = new ArrayList();
			foreach (XmlElement val in el.SelectNodes("data/value"))
			{
				array.Add(ParseValue(val));
			}
			return array.ToArray();
		}
	}
}
