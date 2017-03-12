// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using OpenLiveWriter.CoreServices;
using mshtml;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// FontWrapper will take a string formatted for IDM_COMPOSESETTINGS and create the HTML
    /// string that has the same formatting.
    /// Bold: 1 for bold; 0 or blank for non-bold.
    /// Italic: 1 for italic; 0 or blank for non-italic.
    /// Underline: 1 for underline; 0 or blank for non-underline.
    /// Size: Integer that specifies the font size within a range of 1 through 7, with 7 representing the largest font.
    /// Font Color: RGB color value given by a period-delimited list of three integers (for example, 100.19.0). Each number should be between 0 and 255, but if a number is not, the value for the color will be interpreted as the number mod 255.
    /// Background Color: RGB color value given by a period delimited list of three integers (e.g., 100.19.0). Each number should be between 0 and 255, but if a number is not, the value for the color will be the number mod 255.
    /// Name: String that specifies a font name.
    /// </summary>
    public class MshtmlFontWrapper
    {
        public readonly bool ValidFont;
        public readonly string HtmlWrap;
        public readonly string FontFamily;
        /// <summary>
        /// HTML font tag size, e.g. 1-7
        /// </summary>
        public readonly int FontSize;
        /// <summary>
        /// Inline style size, e.g. 12 pt.
        /// </summary>
        public readonly float FontPointSize;
        public readonly bool Underlined;
        public readonly bool Bold;
        public readonly bool Italics;
        public readonly string FontColor;

        public MshtmlFontWrapper()
        {
            ValidFont = false;
        }

        public string ApplyFont(string text)
        {
            if (ValidFont)
                return string.Format(CultureInfo.InvariantCulture, HtmlWrap, text);
            else
                return text;
        }

        public void ApplyFontToBody(IHTMLDocument2 doc)
        {
            doc.body.style.fontFamily = FontFamily;
            doc.body.style.fontSize = FontPointSize + "pt";
            doc.body.style.color = FontColor;
        }

        public string ApplyFontToBody(string bodyInnerHtml)
        {
            return String.Format(CultureInfo.InvariantCulture, @"<div style=""font-family:'{0}';font-size:{1}pt;color:{2};"">{3}</div>",
                FontFamily,     // {0}
                FontPointSize,  // {1}
                FontColor,      // {2}
                bodyInnerHtml); // {3}
        }

        /// <summary>
        /// This constructor takes a IDM.COMPOSESETTINGS string
        /// Note that the font size specified by such a string is limited to the HTML font tag sizes 1-7.
        /// </summary>
        /// <param name="fontString"></param>
        public MshtmlFontWrapper(string fontString)
        {
            string[] parts = fontString.Split(',');
            StringBuilder start = new StringBuilder();
            StringBuilder end = new StringBuilder();

            if (parts.Length < 7)
            {
                Trace.Fail("Incorrectly formatted font string: ", fontString);
                ValidFont = false;
                return;
            }

            start.Append("<DIV STYLE=\"");
            end.Insert(0, "</DIV>");

            start.Append("font-size:");
            if (Int32.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out FontSize))
            {
                if (FontSize > 7)
                    FontSize = 7;
                if (FontSize < 1)
                    FontSize = 1;
            }
            else
            {
                FontSize = 2;
            }

            FontPointSize = HTMLElementHelper.HtmlFontSizeToPointFontSize(FontSize);
            start.Append(FontPointSize);
            start.Append("pt;");

            if (!string.IsNullOrEmpty(parts[4]))
            {

                string[] rgb = parts[4].Split('.');

                if (rgb.Length != 3)
                {
                    ValidFont = false;
                    return;
                }

                try
                {
                    Color c = Color.FromArgb(Int32.Parse(rgb[0], CultureInfo.InvariantCulture), Int32.Parse(rgb[1], CultureInfo.InvariantCulture), Int32.Parse(rgb[2], CultureInfo.InvariantCulture));
                    FontColor = ColorHelper.ColorToString(c);
                    start.Append("color:");
                    start.Append(FontColor);
                    start.Append(";");

                }
                catch (ArgumentException e) // Invalid color
                {
                    Trace.Fail("Exception thrown while parsing font string: " + fontString + "\r\n" + e);
                    ValidFont = false;
                    return;
                }
                catch (FormatException e) // Invalid numbers in the string
                {
                    Trace.Fail("Exception thrown while parsing font string: " + fontString + "\r\n" + e);
                    ValidFont = false;
                    return;
                }

            }

            // Parse font family, which is a comma delimited list of font names
            start.Append("font-family:'");
            if (!string.IsNullOrEmpty(parts[6]))
            {
                FontFamily = parts[6];

                int fontFamilyIndex = 7;
                while (fontFamilyIndex < parts.Length && !String.IsNullOrEmpty(parts[fontFamilyIndex]))
                {
                    FontFamily = String.Format(CultureInfo.InvariantCulture, "{0},{1}", FontFamily, parts[fontFamilyIndex]);
                    fontFamilyIndex++;
                }

                // We will normalize to single quotes (if any) in order to facilitate string comparisons of font families
                FontFamily = FontFamily.Replace('\"', '\'');
            }
            else
            {
                FontFamily = "Calibri";
            }
            start.Append(FontFamily);
            start.Append("';\"");

            start.Append(">");

            if (parts[0] == "1")
            {
                start.Append("<STRONG>");
                end.Insert(0, "</STRONG>");
                Bold = true;
            }

            if (parts[1] == "1")
            {
                start.Append("<EM>");
                end.Insert(0, "</EM>");
                Italics = true;
            }

            if (parts[2] == "1")
            {
                start.Append("<U>");
                end.Insert(0, "</U>");
                Underlined = true;
            }

            start.Append("{0}");

            HtmlWrap = start.ToString() + end.ToString();

            ValidFont = true;

        }
    }
}
