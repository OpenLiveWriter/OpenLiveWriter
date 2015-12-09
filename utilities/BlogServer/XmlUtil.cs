using System;
using System.Globalization;
using System.IO;
using System.Xml;
using BlogServer.Config;

namespace BlogServer
{
	public class XmlUtil
	{
		/// <summary>
		/// Similar to ReadString, except that relative paths will be made absolute
		/// using the "baseDir" property as the root. If baseDir is not explicitly set
		/// somewhere, it defaults to the directory that contains the config XML file.
		/// </summary>
		public static string ReadPath(XmlElement el, string name, string defaultValue)
		{
			string val = ReadString(el, name, defaultValue);

			if (val == null)
				return null;
			val = val.Replace('/', '\\');
			if (Path.IsPathRooted(val))
				return val;
			else
				return Path.Combine(ConfigProperties.Instance[ConfigProperties.BASE_DIR] as string, val);
		}
		
		public static int ReadInt(XmlElement el, string name, int defaultValue)
		{
			try
			{
				string strVal = ReadString(el, name, "").Trim();
				if (strVal == null || strVal == string.Empty)
					return defaultValue;
				return int.Parse(strVal, CultureInfo.InvariantCulture);
			}
			catch
			{
				return defaultValue;
			}
		}
		
		public static bool ReadBool(XmlElement el, string name, bool defaultValue)
		{
			switch (ReadString(el, name, "default").Trim().ToLower())
			{
				case "1":
				case "true":
				case "yes":
					return true;
				case "0":
				case "false":
				case "no":
					return false;
				default:
					return defaultValue;
			}
		}
		
		public static string ReadString(XmlElement el, string name, string defaultValue)
		{
			string val = ReadStringInternal(el, name, defaultValue);
			return (val == null) ? null : ConfigProperties.Instance.EvaluateInterpolatedString(val);
		}
		
		private static string ReadStringInternal(XmlElement el, string name, string defaultValue)
		{
			XmlNode node = el.SelectSingleNode(name);
			if (node is XmlAttribute)
				return ((XmlAttribute) node).Value;
			else if (node is XmlElement)
				return ((XmlElement) node).InnerText;
			else
				return defaultValue;
		}
	}
}
