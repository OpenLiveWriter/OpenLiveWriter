using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CodeSnippet.Options
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class PatternRuleAttribute : RuleBaseAttribute
	{
		public string Pattern
		{
			get;
			private set;
		}

		public PatternRuleAttribute(string pattern)
		{
			this.Pattern = pattern;
		}

		public override bool IsValid(object value)
		{
			bool flag = false;
			base.ErrorMessage = string.Empty;
			string str = value as string;
			if (str != null && this.Pattern != null)
			{
				flag = Regex.IsMatch(str, this.Pattern);
			}
			if (!flag)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { value };
				base.ErrorMessage = string.Format(invariantCulture, "The value you entered: {0} doesn't match the given pattern.", objArray);
			}
			return flag;
		}
	}
}