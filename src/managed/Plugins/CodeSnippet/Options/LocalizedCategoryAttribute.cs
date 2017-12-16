using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Resources;

namespace CodeSnippet.Options
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class LocalizedCategoryAttribute : CategoryAttribute
	{
		public LocalizedCategoryAttribute(string category) : base(category)
		{
		}

		protected override string GetLocalizedString(string value)
		{
			string str = Resources.ResourceManager.GetString(value, Resources.Culture);
			return str ?? value;
		}
	}
}