using System;

namespace CodeSnippet.Formats
{
	public class CssFormat : CLikeFormat
	{
		protected override string ClassRegex
		{
			get
			{
				return "\\.[-]?[_a-zA-Z][_a-zA-Z0-9-]*|[^\\0-\\177]*\\\\[0-9a-f]{1,6}";
			}
		}

		protected override string Keywords
		{
			get
			{
				return "background-attachment background-color background-image background-position background-repeat background border-collapse border-color border-spacing border-style border-width border bottom caption-side clear clip color content counter-increment counter-reset cursor direction display empty-cells float font-family font-size font-style font-variant font-weight font height left letter-spacing line-height list-style list-style-image list-style-position list-style-type margin max-height max-width min-height min-width orphans outline-color outline-style outline-width outline overflow padding page-break-after page-break-before page-break-inside position quotes right table-layout text-align text-decoration text-indent text-transform top unicode-bidi vertical-align visibility white-space widows width word-spacing z-indexa abbr acronym address applet area b base basefont bdo big blockquote body br button caption center cite code col colgroup dd del dfn dir div dl dt em fieldset font form frame frameset h1 h2 h3 h4 h5 h6 head hr html i iframe img input ins isindex kbd label legend li link map menu meta noframes noscript object ol optgroup option p param pre q s samp script select small span strike strong style sub sup table tbody td textarea tfoot th thead title tr tt u ul var";
			}
		}

		protected override string StringRegex
		{
			get
			{
				return "[^:\\s][\\d\\s\\w#\"'.,%]+;";
			}
		}

		public CssFormat()
		{
		}
	}
}