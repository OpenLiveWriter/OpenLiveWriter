using CodeSnippet.Formats.Base;
using System;

namespace CodeSnippet.Formats
{
	public abstract class CLikeFormat : CodeFormat
	{
		protected override string CommentRegex
		{
			get
			{
				return "/\\*.*?\\*/|//.*?(?=\\r|\\n)";
			}
		}

		protected override string StringRegex
		{
			get
			{
				return "@?\"\"|@?\".*?(?!\\\\).\"|''|'.*?(?!\\\\).'";
			}
		}

		protected CLikeFormat()
		{
		}
	}
}