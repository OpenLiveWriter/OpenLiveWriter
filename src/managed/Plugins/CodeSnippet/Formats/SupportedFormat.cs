using CodeSnippet.Formats.Base;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CodeSnippet.Formats
{
	public class SupportedFormat
	{
		private readonly static ArrayList items;

		private readonly Type _format;

		public static ArrayList Items
		{
			get
			{
				return SupportedFormat.items;
			}
		}

		public SupportedFormatType Key
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}

		static SupportedFormat()
		{
			SupportedFormat.items = SupportedFormat.GetSupportedFormatsList();
		}

		private SupportedFormat(SupportedFormatType key, string value, Type format)
		{
			this.Key = key;
			this.Value = value;
			this._format = format;
		}

		public static SupportedFormat GetItem(SupportedFormatType key)
		{
			SupportedFormat supportedFormat = null;
			foreach (SupportedFormat item in SupportedFormat.Items)
			{
				if (item.Key != key)
				{
					continue;
				}
				supportedFormat = item;
				break;
			}
			return supportedFormat;
		}

		public static SupportedFormatType GetItemKey(object value)
		{
			return (value as SupportedFormat).Key;
		}

		public static string GetItemValue(object value)
		{
			return (value as SupportedFormat).Value;
		}

		private static ArrayList GetSupportedFormatsList()
		{
			ArrayList arrayLists = new ArrayList();
			arrayLists.Add(new SupportedFormat(SupportedFormatType.AutoIt, "AutoIt", typeof(AutoIt)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.CCpp, "C/C++", typeof(CCppFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.CSharp, "C#", typeof(CSharpFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.ColdFusion, "ColdFusion", typeof(ColdFusionFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.Css, "Cascading Style Sheets", typeof(CssFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.Html, "HTML", typeof(HtmlFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.Java, "Java", typeof(JavaFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.JavaScript, "JavaScript", typeof(JavaScriptFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.Msh, "MSH", typeof(MshFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.Php, "PHP", typeof(PhpFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.Regex, "Regular Expression", typeof(RegexFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.Tsql, "TSQL", typeof(TsqlFormat)));
			arrayLists.Add(new SupportedFormat(SupportedFormatType.VisualBasic, "Visual Basic", typeof(VisualBasicFormat)));
			return arrayLists;
		}

		public SourceFormat NewFormatInstance()
		{
			ConstructorInfo constructor = this._format.GetConstructor(new Type[0]);
			return constructor.Invoke(null) as SourceFormat;
		}

		public static SupportedFormatType PredictType(string source)
		{
			SupportedFormatType key = SupportedFormatType.CSharp;
			int num = 0;
			foreach (SupportedFormat item in SupportedFormat.Items)
			{
				int num1 = item.NewFormatInstance().MatchKeywordCount(source);
				if (num1 <= num)
				{
					continue;
				}
				num = num1;
				key = item.Key;
			}
			return key;
		}

		public override string ToString()
		{
			return this.Value;
		}
	}
}