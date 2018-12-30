using CodeSnippet.Formats;
using CodeSnippet.Forms;
using CodeSnippet.Options;
using CodeSnippet.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace CodeSnippet.Config
{
	[DefaultProperty("SectionName")]
	public class GeneralConfig : IConfigSection
	{
		[Browsable(false)]
		public SupportedFormatType ActiveLanguage
		{
			get;
			set;
		}

		[Category("Layout")]
		[LocalizedDescription("DescriptionAttribute_ActiveMode")]
		[LocalizedDisplayName("DisplayNameAttribute_ActiveMode")]
		public CodeSnippetViewMode ActiveMode
		{
			get;
			set;
		}

		[Category("Layout")]
		[LocalizedDescription("DescriptionAttribute_ActiveView")]
		[LocalizedDisplayName("DisplayNameAttribute_ActiveView")]
		public CodeSnippetViewType ActiveView
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
				return Resources.general;
			}
		}

		[Category("Behavior")]
		[LocalizedDescription("DescriptionAttribute_RunSilent")]
		[LocalizedDisplayName("DisplayNameAttribute_RunSilent")]
		public bool RunSilent
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
				return Resources.ConfigSectionName_General;
			}
		}

		public GeneralConfig()
		{
			this.RunSilent = false;
			this.ActiveView = CodeSnippetViewType.Both;
			this.ActiveMode = CodeSnippetViewMode.Full;
			this.ActiveLanguage = SupportedFormatType.CSharp;
		}

		public GeneralConfig(GeneralConfig copy)
		{
			this.RunSilent = copy.RunSilent;
			this.ActiveLanguage = copy.ActiveLanguage;
			this.ActiveMode = copy.ActiveMode;
			this.ActiveView = copy.ActiveView;
		}
	}
}