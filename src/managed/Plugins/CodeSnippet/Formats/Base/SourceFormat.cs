using CodeSnippet.Config;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeSnippet.Formats.Base
{
	public abstract class SourceFormat
	{
		private Regex _codeRegex;

		protected Regex CodeRegex
		{
			get
			{
				Regex regex = this._codeRegex;
				if (regex == null)
				{
					Regex regex1 = this.ConstructCodeRegex();
					Regex regex2 = regex1;
					this._codeRegex = regex1;
					regex = regex2;
				}
				return regex;
			}
			set
			{
				this._codeRegex = value;
			}
		}

		private static Stream CssStream
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetManifestResourceStream("CodeSnippet.Resources.csharp.css");
			}
		}

		public static string CssString
		{
			get
			{
				return (new StreamReader(SourceFormat.CssStream)).ReadToEnd();
			}
		}

		public IEditorConfig Editor
		{
			get;
			set;
		}

		public IStyleConfig Style
		{
			get;
			set;
		}

		protected SourceFormat()
		{
			this.Editor = new EditorConfig();
			this.Style = new StyleConfig();
		}

		protected abstract Regex ConstructCodeRegex();

		public static string EmbedStyles(string source, IStyleConfig styleConfig)
		{
			foreach (DictionaryEntry styleMap in styleConfig.StyleMap)
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] key = new object[] { styleMap.Key };
				string str = string.Format(currentCulture, "class=\"{0}\"", key);
				CultureInfo cultureInfo = CultureInfo.CurrentCulture;
				object[] value = new object[] { styleMap.Value };
				source = source.Replace(str, string.Format(cultureInfo, "style=\"{0}\"", value));
			}
			return source;
		}

		public string FormatCode(Stream source)
		{
			StreamReader streamReader = new StreamReader(source);
			string end = streamReader.ReadToEnd();
			streamReader.Close();
			return this.FormatCode(end, this.Editor, this.Style, false);
		}

		public string FormatCode(string source)
		{
			return this.FormatCode(source, this.Editor, this.Style, false);
		}

		private string FormatCode(string source, IEditorConfig editorConfig, IStyleConfig styleConfig, bool subCode)
		{
			if (string.IsNullOrEmpty(source))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(source);
			if (!subCode)
			{
				stringBuilder.Replace("&", "&amp;");
				stringBuilder.Replace("<", "&lt;");
				stringBuilder.Replace(">", "&gt;");
				stringBuilder.Replace("\t", string.Empty.PadRight((int)editorConfig.TabSpaces));
			}
			SourceFormat sourceFormat = this;
			source = this.CodeRegex.Replace(stringBuilder.ToString(), new MatchEvaluator(sourceFormat.MatchEval));
			stringBuilder = new StringBuilder();
			if (!subCode)
			{
				stringBuilder.AppendFormat("<div id=\"codeSnippetWrapper\"{0}>", (styleConfig.UseContainer ? " class=\"csharpcode-wrapper\"" : string.Empty));
			}
			if (styleConfig.LineNumbers || styleConfig.AlternateLines)
			{
				if (!subCode)
				{
					stringBuilder.AppendLine("<div id=\"codeSnippet\" class=\"csharpcode\">");
				}
				StringReader stringReader = new StringReader(source);
				int num = 0;
				string str = new string(' ', (int)editorConfig.TabSpaces);
				while (true)
				{
					string str1 = stringReader.ReadLine();
					string str2 = str1;
					if (str1 == null)
					{
						break;
					}
					num++;
					if (!styleConfig.AlternateLines || num % 2 != 1)
					{
						stringBuilder.Append("<pre class=\"alteven\">");
					}
					else
					{
						stringBuilder.Append("<pre class=\"alt\">");
					}
					if (styleConfig.LineNumbers)
					{
						int num1 = (int)Math.Log10((double)num);
						stringBuilder.AppendFormat("<span id=\"lnum{0}\" class=\"lnum\">{1}:</span> ", num, string.Concat(str.Substring(0, 3 - num1), num));
					}
					if (str2.Length != 0)
					{
						stringBuilder.Append(str2);
					}
					else
					{
						stringBuilder.Append("&nbsp;");
					}
					stringBuilder.Append("</pre><!--CRLF-->");
				}
				stringReader.Close();
				if (!subCode)
				{
					stringBuilder.Append("</div>");
				}
			}
			else
			{
				if (!subCode)
				{
					stringBuilder.Append("<pre id=\"codeSnippet\" class=\"csharpcode\">");
				}
				stringBuilder.Append(source);
				if (!subCode)
				{
					stringBuilder.Append("</pre>");
				}
				StringReader stringReader1 = new StringReader(stringBuilder.ToString());
				stringBuilder = new StringBuilder();
				while (true)
				{
					string str3 = stringReader1.ReadLine();
					string str4 = str3;
					if (str3 == null)
					{
						break;
					}
					stringBuilder.AppendFormat("{0}<br />", str4);
				}
				stringReader1.Close();
			}
			if (!subCode)
			{
				stringBuilder.Append("</div>");
			}
			if (!styleConfig.EmbedStyles || subCode)
			{
				return stringBuilder.ToString();
			}
			return SourceFormat.EmbedStyles(stringBuilder.ToString(), styleConfig);
		}

		public string FormatSubCode(string source)
		{
			return this.FormatCode(source, this.Editor, this.Style, true);
		}

		protected abstract string MatchEval(Match match);

		public virtual int MatchKeywordCount(string source)
		{
			return 0;
		}
	}
}