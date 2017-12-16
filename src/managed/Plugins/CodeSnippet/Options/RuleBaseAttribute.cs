using System;
using System.Runtime.CompilerServices;

namespace CodeSnippet.Options
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public abstract class RuleBaseAttribute : Attribute
	{
		public string ErrorMessage
		{
			get;
			protected set;
		}

		protected RuleBaseAttribute()
		{
		}

		public abstract bool IsValid(object value);
	}
}