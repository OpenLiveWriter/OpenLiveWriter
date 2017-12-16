using System;
using System.ComponentModel;
using System.Globalization;

namespace CodeSnippet.Options
{
	public class StyleConverter : StringConverter
	{
		public StyleConverter()
		{
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string str;
			string str1 = base.ConvertFrom(context, culture, value) as string;
			if (str1 != null)
			{
				string[] strArrays = new string[] { ";" };
				string[] strArrays1 = str1.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < (int)strArrays1.Length; i++)
				{
					strArrays1[i] = strArrays1[i].Trim();
				}
				str1 = string.Join("; ", strArrays1).Trim();
				if (str1.EndsWith(";"))
				{
					str = str1;
				}
				else
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { str1 };
					str = string.Format(invariantCulture, "{0};", objArray);
				}
				str1 = str;
			}
			return str1;
		}
	}
}