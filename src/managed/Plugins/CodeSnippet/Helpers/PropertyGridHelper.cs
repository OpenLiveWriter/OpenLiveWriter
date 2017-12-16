using System;
using System.Reflection;
using System.Windows.Forms;

namespace CodeSnippet.Helpers
{
	internal static class PropertyGridHelper
	{
		public static void ResizePropertyGridColumns(Control pg, int percent)
		{
			Type type = pg.GetType();
			object value = type.GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pg);
			Type type1 = value.GetType();
			int width = (int)((float)pg.Width * ((float)percent / 100f));
			MethodInfo method = type1.GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);
			object[] objArray = new object[] { width };
			method.Invoke(value, objArray);
		}
	}
}