using CodeSnippet.Options;
using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using WLWPluginBase.Win32;

namespace CodeSnippet.Config
{
	[DefaultProperty("SectionName")]
	public class LayoutConfig : IConfigSection
	{
		[Browsable(false)]
		[CLSCompliant(false)]
		public Win32Structures.WINDOWPLACEMENT CompactModePlacement
		{
			get;
			set;
		}

		[Browsable(false)]
		[CLSCompliant(false)]
		public Win32Structures.WINDOWPLACEMENT FullModePlacement
		{
			get;
			set;
		}

		[Browsable(false)]
		[XmlIgnore]
		public Bitmap Image
		{
			get
			{
				return Resources.layout;
			}
		}

		[Browsable(false)]
		[CLSCompliant(false)]
		public Win32Structures.WINDOWPLACEMENT OptionsPlacement
		{
			get;
			set;
		}

		[LocalizedCategory("CategoryAttribute_Attributes")]
		[LocalizedDescription("DescriptionAttribute_SectionName")]
		[LocalizedDisplayName("DisplayNameAttribute_SectionName")]
		[XmlIgnore]
		public string SectionName
		{
			get
			{
				return Resources.ConfigSectionName_Layout;
			}
		}

		public LayoutConfig()
		{
		}

		public LayoutConfig(LayoutConfig copy)
		{
			this.FullModePlacement = copy.FullModePlacement;
			this.CompactModePlacement = copy.CompactModePlacement;
			this.OptionsPlacement = copy.OptionsPlacement;
		}
	}
}