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
	public class EditorConfig : IEditorConfig, IConfigSection
	{
		[Browsable(false)]
		[XmlIgnore]
		public Bitmap Image
		{
			get
			{
				return Resources.editor;
			}
		}

		[LocalizedCategory("CategoryAttribute_Attributes")]
		[LocalizedDescription("DescriptionAttribute_SectionName")]
		[LocalizedDisplayName("DisplayNameAttribute_SectionName")]
		[XmlIgnore]
		public string SectionName
		{
			get
			{
				return Resources.ConfigSectionName_Editor;
			}
		}

		[Category("Format")]
		[LocalizedDescription("DescriptionAttribute_TabSpaces")]
		[LocalizedDisplayName("DisplayNameAttribute_TabSpaces")]
		public byte TabSpaces
		{
			get;
			set;
		}

		[Category("Format")]
		[LocalizedDescription("DescriptionAttribute_TrimIndentOnPaste")]
		[LocalizedDisplayName("DisplayNameAttribute_TrimIndentOnPaste")]
		public bool TrimIndentOnPaste
		{
			get;
			set;
		}

		[Category("Format")]
		[LocalizedDescription("DescriptionAttribute_WordWrap")]
		[LocalizedDisplayName("DisplayNameAttribute_WordWrap")]
		public bool WordWrap
		{
			get;
			set;
		}

		public EditorConfig()
		{
			this.TabSpaces = 4;
			this.TrimIndentOnPaste = true;
			this.WordWrap = false;
		}

		public EditorConfig(IEditorConfig copy)
		{
			this.TabSpaces = copy.TabSpaces;
			this.TrimIndentOnPaste = copy.TrimIndentOnPaste;
			this.WordWrap = copy.WordWrap;
		}
	}
}