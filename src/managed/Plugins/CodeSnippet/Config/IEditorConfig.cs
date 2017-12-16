using System;

namespace CodeSnippet.Config
{
	public interface IEditorConfig
	{
		byte TabSpaces
		{
			get;
			set;
		}

		bool TrimIndentOnPaste
		{
			get;
			set;
		}

		bool WordWrap
		{
			get;
			set;
		}
	}
}