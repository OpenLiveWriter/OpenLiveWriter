using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CodeSnippet.Options
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
	public class StyleEditor : UITypeEditor
	{
		protected IContainer Container
		{
			get;
			private set;
		}

		public StyleEditor()
		{
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			object obj;
			if (context != null && context.Instance != null && provider != null)
			{
				this.Container = context.Container;
				IWindowsFormsEditorService service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (service != null)
				{
					TextBox textBox1 = new TextBox()
					{
						BorderStyle = BorderStyle.None,
						Font = new Font(FontFamily.GenericMonospace, 10f),
						Height = 150,
						Multiline = true,
						ScrollBars = ScrollBars.Both,
						WordWrap = false
					};
					TextBox textBox2 = textBox1;
					string str = (value == null ? string.Empty : value.ToString());
					string[] strArrays = new string[] { ";" };
					string[] strArrays1 = str.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < (int)strArrays1.Length; i++)
					{
						strArrays1[i] = strArrays1[i].Trim();
					}
					textBox2.Lines = strArrays1;
					textBox2.GotFocus += new EventHandler((object argument0, EventArgs argument1) => {
						TextBox textBox = textBox2;
						int num = 0;
						int num1 = num;
						textBox2.SelectionLength = num;
						textBox.SelectionStart = num1;
					});
					service.DropDownControl(textBox2);
					for (int j = 0; j < (int)strArrays1.Length; j++)
					{
						strArrays1[j] = strArrays1[j].Trim().Replace(";", string.Empty);
					}
					str = string.Join("; ", textBox2.Lines).Trim();
					if (str.EndsWith(";"))
					{
						obj = str;
					}
					else
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						object[] objArray = new object[] { str };
						obj = string.Format(invariantCulture, "{0};", objArray);
					}
					value = obj;
					return value;
				}
			}
			return base.EditValue(context, provider, value);
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null)
			{
				return UITypeEditorEditStyle.DropDown;
			}
			return base.GetEditStyle(context);
		}
	}
}