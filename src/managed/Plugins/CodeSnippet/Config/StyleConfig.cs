using CodeSnippet.Options;
using CodeSnippet.Properties;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace CodeSnippet.Config
{
	[DefaultProperty("SectionName")]
	public class StyleConfig : IStyleConfig, IConfigSection
	{
		[Category("Format")]
		[LocalizedDescription("DescriptionAttribute_AlternateLines")]
		[LocalizedDisplayName("DisplayNameAttribute_AlternateLines")]
		public bool AlternateLines
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_AltevenStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string AltevenStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_AltStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string AltStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_AspStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string AspStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_AttrStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string AttrStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_ClsStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string ClsStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_CodeStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string CodeStyle
		{
			get;
			set;
		}

		[Category("Format")]
		[LocalizedDescription("DescriptionAttribute_EmbedStyles")]
		[LocalizedDisplayName("DisplayNameAttribute_EmbedStyles")]
		public bool EmbedStyles
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_HtmlStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string HtmlStyle
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
				return Resources.style;
			}
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_KwrdStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string KwrdStyle
		{
			get;
			set;
		}

		[Category("Format")]
		[LocalizedDescription("DescriptionAttribute_LineNumbers")]
		[LocalizedDisplayName("DisplayNameAttribute_LineNumbers")]
		public bool LineNumbers
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_LnumStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string LnumStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_OpStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string OpStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_PreprocStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string PreprocStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_PreStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string PreStyle
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_RemStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string RemStyle
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
				return Resources.ConfigSectionName_Styles;
			}
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_StrStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string StrStyle
		{
			get;
			set;
		}

		[Browsable(false)]
		[XmlIgnore]
		public Hashtable StyleMap
		{
			get
			{
				Hashtable hashtables = new Hashtable()
				{
					{ "alt", this.AltStyle },
					{ "alteven", this.AltevenStyle },
					{ "asp", this.AspStyle },
					{ "attr", this.AttrStyle },
					{ "cls", this.ClsStyle },
					{ "csharpcode", (this.LineNumbers || this.AlternateLines ? this.CodeStyle : this.PreStyle) },
					{ "csharpcode-wrapper", this.WrapperStyle },
					{ "html", this.HtmlStyle },
					{ "kwrd", this.KwrdStyle },
					{ "lnum", this.LnumStyle },
					{ "op", this.OpStyle },
					{ "preproc", this.PreprocStyle },
					{ "rem", this.RemStyle },
					{ "str", this.StrStyle }
				};
				return hashtables;
			}
		}

		[Category("Format")]
		[LocalizedDescription("DescriptionAttribute_UseContainer")]
		[LocalizedDisplayName("DisplayNameAttribute_UseContainer")]
		public bool UseContainer
		{
			get;
			set;
		}

		[Editor(typeof(StyleEditor), typeof(UITypeEditor))]
		[LocalizedCategory("CategoryAttribute_Styles")]
		[LocalizedDescription("DescriptionAttribute_WrapperStyle")]
		[TypeConverter(typeof(StyleConverter))]
		public string WrapperStyle
		{
			get;
			set;
		}

		public StyleConfig()
		{
			this.AlternateLines = true;
			this.EmbedStyles = true;
			this.LineNumbers = true;
			this.UseContainer = true;
			this.AttrStyle = "color: #ff0000;";
			this.AspStyle = "background-color: #ffff00;";
			this.AltStyle = "background-color: #f4f4f4; font-family: 'Courier New', Courier, Monospace; font-size: 8pt; line-height: 12pt; border-style: none; color: black; overflow: visible; padding: 0px; width: 100%; margin: 0em; background-color: white; direction: ltr; text-align: left;";
			this.AltevenStyle = "background-color: #f4f4f4; font-family: 'Courier New', Courier, Monospace; font-size: 8pt; line-height: 12pt; border-style: none; color: black; overflow: visible; padding: 0px; width: 100%; margin: 0em; direction: ltr; text-align: left;";
			this.ClsStyle = "color: #cc6633;";
			this.CodeStyle = "background-color: #f4f4f4; font-family: 'Courier New', Courier, Monospace; font-size: 8pt; line-height: 12pt; border-style: none; color: black; overflow: visible; padding: 0px; width: 100%; direction: ltr; text-align: left;";
			this.HtmlStyle = "color: #800000;";
			this.KwrdStyle = "color: #0000ff;";
			this.LnumStyle = "color: #606060;";
			this.OpStyle = "color: #0000c0;";
			this.PreprocStyle = "color: #cc6633;";
			this.PreStyle = "background-color: #f4f4f4; font-family: 'Courier New', Courier, Monospace; font-size: 8pt; line-height: 12pt; border-style: none; color: black; overflow: visible; padding: 0px; width: 100%; margin: 0em; direction: ltr; text-align: left;";
			this.RemStyle = "color: #008000;";
			this.StrStyle = "color: #006080;";
			this.WrapperStyle = "background-color: #f4f4f4; font-family: 'Courier New', Courier, Monospace; font-size: 8pt; line-height: 12pt; border: solid 1px silver; cursor: text; margin: 20px 0px 10px 0px; max-height: 200px; overflow: auto; padding: 4px; width: 97.5%; direction: ltr; text-align: left;";
		}

		public StyleConfig(StyleConfig copy)
		{
			this.AlternateLines = copy.AlternateLines;
			this.EmbedStyles = copy.EmbedStyles;
			this.LineNumbers = copy.LineNumbers;
			this.UseContainer = copy.UseContainer;
			this.AttrStyle = copy.AttrStyle;
			this.AspStyle = copy.AspStyle;
			this.AltStyle = copy.AltStyle;
			this.AltevenStyle = copy.AltevenStyle;
			this.ClsStyle = copy.ClsStyle;
			this.CodeStyle = copy.CodeStyle;
			this.HtmlStyle = copy.HtmlStyle;
			this.KwrdStyle = copy.KwrdStyle;
			this.LnumStyle = copy.LnumStyle;
			this.OpStyle = copy.OpStyle;
			this.PreprocStyle = copy.PreprocStyle;
			this.PreStyle = copy.PreStyle;
			this.RemStyle = copy.RemStyle;
			this.StrStyle = copy.StrStyle;
			this.WrapperStyle = copy.WrapperStyle;
		}
	}
}