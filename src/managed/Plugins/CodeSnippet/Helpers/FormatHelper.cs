using CodeSnippet.Config;
using CodeSnippet.Formats;
using CodeSnippet.Formats.Base;
using System;

namespace CodeSnippet.Helpers
{
	public static class FormatHelper
	{
		public static string Format(SupportedFormatType lang, IEditorConfig editorConfig, IStyleConfig styleConfig, string source, bool trim)
		{
			source = FormatHelper.PreprocessSource(source, new string(' ', (int)editorConfig.TabSpaces), trim);
			SourceFormat sourceFormat = SupportedFormat.GetItem(lang).NewFormatInstance();
			sourceFormat.Editor = editorConfig;
			sourceFormat.Style = styleConfig;
			return sourceFormat.FormatCode(source);
		}

		private static string PreprocessSource(string source, string tabReplacement, bool trim)
		{
			string str = source.Replace("\t", tabReplacement);
			if (trim)
			{
				string[] strArrays = str.Split(new char[] { '\n' });
				if ((int)strArrays.Length > 0)
				{
					int length = strArrays[0].Length;
					string str1 = strArrays[0];
					char[] chrArray = new char[] { '\t', ' ' };
					int num = length - str1.TrimStart(chrArray).Length;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						if (strArrays[i].Length >= num)
						{
							strArrays[i] = strArrays[i].Remove(0, num);
						}
					}
				}
				str = string.Join("\n", strArrays);
			}
			return str;
		}
	}
}