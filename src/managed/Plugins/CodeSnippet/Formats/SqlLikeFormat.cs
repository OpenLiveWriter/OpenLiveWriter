using CodeSnippet.Formats.Base;
using System;

namespace CodeSnippet.Formats
{
	public abstract class SqlLikeFormat : CodeFormat
	{
		public override bool CaseSensitive
		{
			get
			{
				return false;
			}
		}

		protected override string CommentRegex
		{
			get
			{
				return "((?:--\\s)|(/\\*.*?\\*/|//)).*?(?=\\r|\\n)";
			}
		}

		protected override string StringRegex
		{
			get
			{
				return "''|'.*?'";
			}
		}

		protected SqlLikeFormat()
		{
		}
	}
}