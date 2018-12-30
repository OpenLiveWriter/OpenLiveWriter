using CodeSnippet.Formats.Base;
using System;

namespace CodeSnippet.Formats
{
	internal class RegexFormat : CodeFormat
	{
		protected override string AttributesRegex
		{
			get
			{
				return "(\\\\[sSDdWw])|([.])";
			}
		}

		public override bool CaseSensitive
		{
			get
			{
				return false;
			}
		}

		protected override string ClassRegex
		{
			get
			{
				return "(\\\\[zZBbGA])|(\\\\&lt;)|(\\\\&gt;)|([\\^$?*])";
			}
		}

		protected override string CommentRegex
		{
			get
			{
				return "(?:\\(?\\??\\#[^)\\r\\n]+\\)?)";
			}
		}

		protected override string Keywords
		{
			get
			{
				return "\\?&lt;=? &lt; &gt; \\?[:!=]";
			}
		}

		protected override string StringRegex
		{
			get
			{
				return "(?<=\\(\\?&lt;)([^&]+)(?=&gt;)";
			}
		}

		public RegexFormat()
		{
		}
	}
}