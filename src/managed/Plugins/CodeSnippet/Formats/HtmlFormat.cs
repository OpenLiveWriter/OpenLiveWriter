using CodeSnippet.Formats.Base;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeSnippet.Formats
{
	public class HtmlFormat : SourceFormat
	{
		private readonly CSharpFormat csf;

		private readonly JavaScriptFormat jsf;

		private readonly Regex attribRegex;

		public HtmlFormat()
		{
			this.attribRegex = new Regex("(=?\".*?\"|=?'.*?')|([\\w:-]+)", RegexOptions.Singleline);
			this.csf = new CSharpFormat();
			this.jsf = new JavaScriptFormat();
		}

		private static string AttributeMatchEval(Match match)
		{
			if (match.Groups[1].Success)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { match };
				return string.Format(currentCulture, "<span class=\"kwrd\">{0}</span>", objArray);
			}
			if (!match.Groups[2].Success)
			{
				return match.ToString();
			}
			CultureInfo cultureInfo = CultureInfo.CurrentCulture;
			object[] objArray1 = new object[] { match };
			return string.Format(cultureInfo, "<span class=\"attr\">{0}</span>", objArray1);
		}

		protected override Regex ConstructCodeRegex()
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			object[] objArray = new object[] { "(?<=&lt;script(?:\\s.*?)?&gt;).+?(?=&lt;/script&gt;)", "&lt;!--.*?--&gt;", "&lt;%@.*?%&gt;|&lt;%|%&gt;", "(?<=&lt;%).*?(?=%&gt;)", "(?:&lt;/?!?\\??(?!%)|(?<!%)/?&gt;)+", "(?<=&lt;/?!?\\??(?!%))[\\w\\.:-]+(?=.*&gt;)", "(?<=&lt;(?!%)/?!?\\??[\\w:-]+).*?(?=(?<!%)/?&gt;)", "&amp;\\w+;" };
			return new Regex(string.Format(currentCulture, "({0})|({1})|({2})|({3})|({4})|({5})|({6})|({7})", objArray), RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		protected override string MatchEval(Match match)
		{
			if (match.Groups[1].Success)
			{
				return this.jsf.FormatSubCode(match.ToString());
			}
			if (match.Groups[2].Success)
			{
				StringReader stringReader = new StringReader(match.ToString());
				StringBuilder stringBuilder = new StringBuilder();
				while (true)
				{
					string str = stringReader.ReadLine();
					string str1 = str;
					if (str == null)
					{
						break;
					}
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] objArray = new object[] { str1 };
					stringBuilder.AppendFormat(currentCulture, "<span class=\"rem\">{0}</span>", objArray);
				}
				return stringBuilder.ToString();
			}
			if (match.Groups[3].Success)
			{
				CultureInfo cultureInfo = CultureInfo.CurrentCulture;
				object[] objArray1 = new object[] { match };
				return string.Format(cultureInfo, "<span class=\"asp\">{0}</span>", objArray1);
			}
			if (match.Groups[4].Success)
			{
				return this.csf.FormatSubCode(match.ToString());
			}
			if (match.Groups[5].Success)
			{
				CultureInfo currentCulture1 = CultureInfo.CurrentCulture;
				object[] objArray2 = new object[] { match };
				return string.Format(currentCulture1, "<span class=\"kwrd\">{0}</span>", objArray2);
			}
			if (match.Groups[6].Success)
			{
				CultureInfo cultureInfo1 = CultureInfo.CurrentCulture;
				object[] objArray3 = new object[] { match };
				return string.Format(cultureInfo1, "<span class=\"html\">{0}</span>", objArray3);
			}
			if (match.Groups[7].Success)
			{
				return this.attribRegex.Replace(match.ToString(), new MatchEvaluator(HtmlFormat.AttributeMatchEval));
			}
			if (!match.Groups[8].Success)
			{
				return match.ToString();
			}
			CultureInfo currentCulture2 = CultureInfo.CurrentCulture;
			object[] objArray4 = new object[] { match };
			return string.Format(currentCulture2, "<span class=\"attr\">{0}</span>", objArray4);
		}
	}
}