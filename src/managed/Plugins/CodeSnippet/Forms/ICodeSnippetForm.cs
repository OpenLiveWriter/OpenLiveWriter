using CodeSnippet.Config;
using System;
using System.Windows.Forms;

namespace CodeSnippet.Forms
{
	internal interface ICodeSnippetForm
	{
		string CodeSnippet
		{
			get;
		}

		string CodeSnippetToEdit
		{
			set;
		}

		CodeSnippetConfig Config
		{
			get;
			set;
		}

		string Text
		{
			get;
		}

		DialogResult ShowDialog(IWin32Window owner);
	}
}