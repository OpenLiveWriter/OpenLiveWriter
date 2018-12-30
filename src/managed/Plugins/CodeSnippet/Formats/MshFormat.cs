using CodeSnippet.Formats.Base;
using System;

namespace CodeSnippet.Formats
{
	public class MshFormat : CodeFormat
	{
		protected override string CommentRegex
		{
			get
			{
				return "#.*?(?=\\r|\\n)";
			}
		}

		protected override string Keywords
		{
			get
			{
				return "function filter global script local private if else elseif for foreach in while switch continue break return default param begin process end throw trap";
			}
		}

		protected override string Preprocessors
		{
			get
			{
				return "-band -bor -match -notmatch -like -notlike -eq -ne -gt -ge -lt -le -is -imatch -inotmatch -ilike -inotlike -ieq -ine -igt -ige -ilt -ile";
			}
		}

		protected override string StringRegex
		{
			get
			{
				return "@?\"\"|@?\".*?(?!\\\\).\"|''|'.*?(?!\\\\).'";
			}
		}

		public MshFormat()
		{
		}
	}
}