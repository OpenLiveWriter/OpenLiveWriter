using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Resources;

namespace CodeSnippet.Options
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class LocalizedDisplayNameAttribute : DisplayNameAttribute
	{
		public override string DisplayName
		{
			get
			{
				string str = Resources.ResourceManager.GetString(base.DisplayName, Resources.Culture);
				return str ?? base.DisplayName;
			}
		}

		public LocalizedDisplayNameAttribute(string displayName) : base(displayName)
		{
		}
	}
}