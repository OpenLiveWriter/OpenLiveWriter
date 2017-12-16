using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Resources;

namespace CodeSnippet.Options
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
	{
		public override string Description
		{
			get
			{
				string str = Resources.ResourceManager.GetString(base.Description, Resources.Culture);
				return str ?? base.Description;
			}
		}

		public LocalizedDescriptionAttribute(string description) : base(description)
		{
		}
	}
}