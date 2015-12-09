using System;
using System.Text;

namespace BlogServer
{
	public class StringUtil
	{
		public static string StripIndentation(string strVal)
		{
			if (strVal == null)
				return null;
			
			string[] lines = strVal.Split('\n');
			string common = null;
			foreach (string line in lines)
			{
				if (line.Trim().Length == 0)
					continue;
				string thisIndent = GetIndent(line);
				if (common == null)
					common = thisIndent;
				else
					common = CommonPrefix(thisIndent, common);
			}
			
			if (common.Length == 0)
				return strVal;
			
			StringBuilder sb = new StringBuilder(strVal.Length);
			foreach (string line in lines)
			{
				if (line.StartsWith(common))
					sb.Append(line.Substring(common.Length)).Append('\n');
				else
					sb.Append(line);
			}
			return sb.ToString();
		}

		private static string CommonPrefix(string s1, string s2)
		{
			int i;
			for (i = 0; i < s1.Length && i < s2.Length; i++)
			{
				if (s1[i] != s2[i])
					return s1.Substring(0, i);
			}
			return s1.Substring(0, i);
		}

		private static string GetIndent(string strVal)
		{
			int pos;
			for (pos = 0; pos < strVal.Length; pos++)
			{
				switch (strVal[pos])
				{
					case ' ':
					case '\t':
						break;
					default:
						return strVal.Substring(0, pos);
				}
			}
			return strVal;  // all whitespace
		}
	}
}
