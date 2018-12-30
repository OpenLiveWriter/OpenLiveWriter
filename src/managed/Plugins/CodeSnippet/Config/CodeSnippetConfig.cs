using CodeSnippet.Forms;
using CodeSnippet.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using WLWPluginBase.Win32;

namespace CodeSnippet.Config
{
	public class CodeSnippetConfig
	{
		private GeneralConfig general;

		private EditorConfig editor;

		private LayoutConfig layout;

		private StyleConfig style;

		[XmlIgnore]
		private static string ConfigFile
		{
			get
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string location = executingAssembly.Location;
				if (location == null)
				{
					return string.Empty;
				}
				string directoryName = Path.GetDirectoryName(location);
				AssemblyCompanyAttribute customAttribute = Attribute.GetCustomAttribute(executingAssembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
				AssemblyProductAttribute assemblyProductAttribute = Attribute.GetCustomAttribute(executingAssembly, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
				string str = Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("APPDATA") ?? directoryName, (customAttribute == null ? Application.CompanyName : customAttribute.Company)), (assemblyProductAttribute == null ? Application.ProductName : assemblyProductAttribute.Product));
				if (!Directory.Exists(str))
				{
					Directory.CreateDirectory(str);
				}
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { location };
				string fileName = Path.GetFileName(string.Format(invariantCulture, "{0}.config", objArray));
				return Path.Combine(str, fileName);
			}
		}

		public EditorConfig Editor
		{
			get
			{
				EditorConfig editorConfig = this.editor;
				if (editorConfig == null)
				{
					EditorConfig editorConfig1 = new EditorConfig();
					EditorConfig editorConfig2 = editorConfig1;
					this.editor = editorConfig1;
					editorConfig = editorConfig2;
				}
				return editorConfig;
			}
			set
			{
				this.editor = value;
			}
		}

		public GeneralConfig General
		{
			get
			{
				GeneralConfig generalConfig = this.general;
				if (generalConfig == null)
				{
					GeneralConfig generalConfig1 = new GeneralConfig();
					GeneralConfig generalConfig2 = generalConfig1;
					this.general = generalConfig1;
					generalConfig = generalConfig2;
				}
				return generalConfig;
			}
			set
			{
				this.general = value;
			}
		}

		public object this[string sectionName]
		{
			get
			{
				object obj;
				using (IEnumerator<object> enumerator = this.Sections.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						IConfigSection current = (IConfigSection)enumerator.Current;
						if (!current.SectionName.Equals(sectionName))
						{
							continue;
						}
						obj = current;
						return obj;
					}
					return null;
				}
				// return obj; BillKrat.2017.12.15 - unreachable code warning
			}
		}

		public LayoutConfig Layout
		{
			get
			{
				LayoutConfig layoutConfig = this.layout;
				if (layoutConfig == null)
				{
					LayoutConfig layoutConfig1 = new LayoutConfig();
					LayoutConfig layoutConfig2 = layoutConfig1;
					this.layout = layoutConfig1;
					layoutConfig = layoutConfig2;
				}
				return layoutConfig;
			}
			set
			{
				this.layout = value;
			}
		}

		[XmlIgnore]
		public bool Reposition
		{
			get
			{
				if (this.General.ActiveMode != CodeSnippetViewMode.Compact)
				{
					return this.Layout.FullModePlacement.Length != 0;
				}
				return this.Layout.CompactModePlacement.Length != 0;
			}
		}

		[XmlIgnore]
		public Collection<object> Sections
		{
			get
			{
				return new Collection<object>()
				{
					this.General,
					this.Editor,
					this.Style
				};
			}
		}

		public StyleConfig Style
		{
			get
			{
				StyleConfig styleConfig = this.style;
				if (styleConfig == null)
				{
					StyleConfig styleConfig1 = new StyleConfig();
					StyleConfig styleConfig2 = styleConfig1;
					this.style = styleConfig1;
					styleConfig = styleConfig2;
				}
				return styleConfig;
			}
			set
			{
				this.style = value;
			}
		}

		public CodeSnippetConfig()
		{
		}

		public CodeSnippetConfig(CodeSnippetConfig copy)
		{
			this.General = new GeneralConfig(copy.General);
			this.Editor = new EditorConfig(copy.Editor);
			this.Layout = new LayoutConfig(copy.Layout);
			this.Style = new StyleConfig(copy.Style);
		}

		public static CodeSnippetConfig Load()
		{
			return ConfigHelper.LoadConfig<CodeSnippetConfig>(new CodeSnippetConfig(), CodeSnippetConfig.ConfigFile);
		}

		public void Store()
		{
			ConfigHelper.StoreConfig<CodeSnippetConfig>(this, CodeSnippetConfig.ConfigFile);
		}
	}
}