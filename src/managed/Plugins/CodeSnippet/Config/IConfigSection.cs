using System;
using System.Drawing;

namespace CodeSnippet.Config
{
	public interface IConfigSection
	{
		Bitmap Image
		{
			get;
		}

		string SectionName
		{
			get;
		}
	}
}