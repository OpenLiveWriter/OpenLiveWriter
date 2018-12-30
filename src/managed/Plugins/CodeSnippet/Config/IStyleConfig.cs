using System;
using System.Collections;

namespace CodeSnippet.Config
{
	public interface IStyleConfig
	{
		bool AlternateLines
		{
			get;
			set;
		}

		bool EmbedStyles
		{
			get;
			set;
		}

		bool LineNumbers
		{
			get;
			set;
		}

		Hashtable StyleMap
		{
			get;
		}

		bool UseContainer
		{
			get;
			set;
		}
	}
}