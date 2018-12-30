using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeSnippet.Formats.Base
{
	public abstract class CodeFormat : SourceFormat
	{
		protected virtual string AttributesRegex
		{
			get
			{
				return string.Empty;
			}
		}

		public virtual bool CaseSensitive
		{
			get
			{
				return true;
			}
		}

		protected virtual string ClassRegex
		{
			get
			{
				return string.Empty;
			}
		}

		protected abstract string CommentRegex
		{
			get;
		}

		protected abstract string Keywords
		{
			get;
		}

		protected virtual string Preprocessors
		{
			get
			{
				return string.Empty;
			}
		}

		protected abstract string StringRegex
		{
			get;
		}

		protected CodeFormat()
		{
		}

		private static void AppendGroup(ICollection<string> groups, string groupName, string commentRegEx)
		{
			if (!string.IsNullOrEmpty(commentRegEx))
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { groupName, commentRegEx };
				groups.Add(string.Format(currentCulture, "(?<{0}>{1})", objArray));
			}
		}

		private static string CheckForMatchAndProcess(Match match)
		{
			string str;
			IEnumerator enumerator = Enum.GetValues(typeof(CodeFormat.DisplayTypes)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CodeFormat.DisplayTypes current = (CodeFormat.DisplayTypes)enumerator.Current;
					if (!match.Groups[current.ToString().ToLower(CultureInfo.CurrentCulture)].Success)
					{
						continue;
					}
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					object[] lower = new object[] { current.ToString().ToLower(CultureInfo.CurrentCulture), match };
					str = string.Format(currentCulture, "<span class=\"{0}\">{1}</span>", lower);
					return str;
				}
				return string.Empty;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
            // return obj;  BillKrat.2017.12.15 Unreachable code warning
        }

        protected override Regex ConstructCodeRegex()
		{
			Regex regex = new Regex("\\w+-\\w+-\\w+|\\w+-\\w+|\\w+|-\\w+|#\\w+|@@\\w+|#(?:\\\\(?:s|w)(?:\\*|\\+)?\\w+)+|@\\\\w\\*+");
			string str = regex.Replace(this.Keywords, "(?<=^|\\W)$0(?=\\W)");
			string str1 = regex.Replace(this.Preprocessors, "(?<=^|\\s)$0(?=\\s|$)");
			regex = new Regex(" +");
			str = regex.Replace(str, "|");
			str1 = regex.Replace(str1, "|");
			List<string> strs = new List<string>();
			CodeFormat.AppendGroup(strs, CodeFormat.DisplayTypes.Rem.ToString().ToLower(CultureInfo.CurrentCulture), this.CommentRegex);
			CodeFormat.AppendGroup(strs, CodeFormat.DisplayTypes.Str.ToString().ToLower(CultureInfo.CurrentCulture), this.StringRegex);
			CodeFormat.AppendGroup(strs, CodeFormat.DisplayTypes.PreProc.ToString().ToLower(CultureInfo.CurrentCulture), str1);
			CodeFormat.AppendGroup(strs, CodeFormat.DisplayTypes.Kwrd.ToString().ToLower(CultureInfo.CurrentCulture), str);
			CodeFormat.AppendGroup(strs, CodeFormat.DisplayTypes.Cls.ToString().ToLower(CultureInfo.CurrentCulture), this.ClassRegex);
			CodeFormat.AppendGroup(strs, CodeFormat.DisplayTypes.Attr.ToString().ToLower(CultureInfo.CurrentCulture), this.AttributesRegex);
			RegexOptions regexOption = (this.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
			return new Regex(string.Join("|", strs.ToArray()), RegexOptions.Singleline | regexOption);
		}

		protected override string MatchEval(Match match)
		{
			if (!match.Groups[CodeFormat.DisplayTypes.Rem.ToString().ToLower(CultureInfo.CurrentCulture)].Success)
			{
				return CodeFormat.CheckForMatchAndProcess(match);
			}
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
				CodeFormat.PushOnSpan(stringBuilder, CodeFormat.DisplayTypes.Rem.ToString().ToLower(CultureInfo.CurrentCulture), str1);
			}
			return stringBuilder.ToString();
		}

		public override int MatchKeywordCount(string source)
		{
			Regex regex = new Regex("\\w+-\\w+-\\w+|\\w+-\\w+|\\w+|-\\w+|#\\w+|@@\\w+|#(?:\\\\(?:s|w)(?:\\*|\\+)?\\w+)+|@\\\\w\\*+");
			string str = regex.Replace(this.Keywords, "(?<=^|\\W)$0(?=\\W)");
			str = (new Regex(" +")).Replace(str, "|");
			regex = new Regex(str, RegexOptions.Singleline | (this.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));
			return regex.Matches(source).Count;
		}

		private static void PushOnSpan(StringBuilder sb, string what, string data)
		{
			sb.AppendFormat("<span class=\"{0}\">{1}</span>", what, data);
		}

		private enum DisplayTypes
		{
			Rem,
			Str,
			PreProc,
			Kwrd,
			Cls,
			Attr
		}
	}
}