// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.HtmlParser.Parser
{
    public class HtmlUtils
    {
        private HtmlUtils()
        {
        }

        public static string NormalizeWhitespace(string html)
        {
            return Regex.Replace(html, @"\s+", " ");
        }

        public static string HTMLToPlainText(string html)
        {
            return HTMLToPlainText(html, false);
        }

        public static string HTMLToPlainText(string html, bool forIndexing)
        {
            html = HTMLToPlainTextNoTrim(html, forIndexing);

            if (html == null)
                return null;

            // Finally, trim any additional whitespace
            return html.Trim();
        }

        public static string HTMLToPlainTextNoTrim(string html, bool forIndexing)
        {
            if (html == null)
                return null;

            // Clean out any already existing newlines
            html = Regex.Replace(html, @"[\r\n]", " ");

            // Remove the head
            html = Regex.Replace(html, @"<head.*?</head>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any javascript
            html = Regex.Replace(html, @"<script.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Remove any CSS
            html = Regex.Replace(html, @"<style.*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // compress whitespace
            html = Regex.Replace(html, @"\s+", " ");

            // remove any smart content
            // because the smart content might have <divs> nested in it, we use regex balancing groups to make sure to parse the nesting correctly.
            //html = Regex.Replace(html, @"<div(\s[^>]*)?id(\s*)?=(\s*)?""scid:([^""]*)?""([^>]*)?>(?>(?!<div|</div>).|<div(?<Depth>)|</div>(?<-Depth>))*(?(Depth)(?!))</div>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<div(\s[^>]*)?id(\s*)?=(\s*)?[""]?scid:([^""]*)?[""]?([^>]*)?>(?>(?!<div|<(/|\\/)div>).|<div(?<Depth>)|<(/|\\/)div>(?<-Depth>))*(?(Depth)(?!))</div>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // turn heading tags into <p>
            html = Regex.Replace(html, @"<(/?)h[1-7](\s[^>]*)?>", "<$1p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // turn ul/ol tags into <p>
            html = Regex.Replace(html, @"<(/?)[uo]l(\s[^>]*)?>", "<$1p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // separate <li>s with newlines
            html = Regex.Replace(html, @"(?<!<p(\s[^>]*)?>\s*)<li(\s[^>]*)?>", "\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // clean out the <p> tags
            // Adjacent (or whitespace-separated) </p><p> tags should be treated as <p>.
            html = Regex.Replace(html, @"</p(\s[^>]*)?>\s*(<p(\s[^>]*)?>)", "<p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Whitespace-separated <div>&nbsp;</div> tags should be treated as just one <div></div>.
            html = Regex.Replace(html, @"<p(\s[^>]*)?>\s*&nbsp;\s*</p(\s[^>]*)?>", "<p></p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // clean out the <div> tags
            // Adjacent (or whitespace-separated) <div><div> tags should be treated as just one <div>.
            html = Regex.Replace(html, @"<div(\s[^>]*)?>(\s*<div(\s[^>]*)?>)+", "<div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Adjacent (or whitespace-separated) </div></div> tags should be treated as just one </div>.
            html = Regex.Replace(html, @"</div(\s[^>]*)?>(\s*</div(\s[^>]*)?>)+", "</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Adjacent (or whitespace-separated) <br></div> tags should be treated as just one </div>.
            html = Regex.Replace(html, @"<br(\s[^>]*)?>\s*</div(\s[^>]*)?>", "</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Whitespace-separated <div>&nbsp;</div> tags should be treated as just one <div></div>.
            html = Regex.Replace(html, @"<div(\s[^>]*)?>\s*&nbsp;\s*</div(\s[^>]*)?>", "<div></div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Adjacent (or whitespace-separated) </div><div> tags should be treated as <div>.
            html = Regex.Replace(html, @"</div(\s[^>]*)?>\s*<div(\s[^>]*)?>", "<div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // <p><div> and <div><p> (or corresponding end tags) should all be collapsed into <p>
            html = Regex.Replace(html, @"</?p(\s[^>]*)?>\s*</?div(\s[^>]*)?>", "</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"</?div(\s[^>]*)?>\s*</?p(\s[^>]*)?>", "<p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // <p> becomes 2 newlines
            html = Regex.Replace(html, @"</?p(\s[^>]*)?>", "\r\n\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // <br> becomes a newline
            html = Regex.Replace(html, @"<br(\s[^>]*)?>", "\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // <div> becomes a newline
            html = Regex.Replace(html, @"</?div(\s[^>]*)?>", "\r\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // null characters ("\0") become two newlines (Mail includes one of these at the end of plain-text reply headers)
            html = Regex.Replace(html, @"\x00", "\r\n\r\n");

            // Clean out all the other tags
            html = Regex.Replace(html, @"</?[^>]+>", forIndexing ? " " : string.Empty);

            // Unescape all entities
            html = UnEscapeEntities(html, UnEscapeMode.Default);

            // remove leading whitespace
            html = Regex.Replace(html, @"^[ \t]*(.+?)[ \t]*$", "$1", RegexOptions.Multiline);

            return html;
        }

        public static string EscapeEntity(char c)
        {
            return EntityEscaper.Char(c);
        }

        /// <summary>
        /// In general you can't put named entities directly in XML PCDATA.
        /// Most entities must use numeric instead.
        /// </summary>
        /// <param name="attribute">If true, escape \r and \n to their numeric equivalents</param>
        public static string EscapeEntitiesForXml(string plaintext, bool attribute)
        {
            if (plaintext == null)
                return null;

            StringBuilder output = new StringBuilder();
            foreach (char c in plaintext)
            {
                switch (c)
                {
                    case '"':
                    case '&':
                    case '<':
                    case '>':
                        output.Append(EntityEscaper.Char(c));
                        break;
                    case '\r':
                    case '\n':
                        if (attribute)
                            AppendNumericEntity(c, output);
                        else
                            output.Append(c);
                        break;
                    case '\'':
                        if (attribute)
                            AppendNumericEntity(c, output);
                        else
                            output.Append(c);
                        break;
                    case (char)160:
                        AppendNumericEntity(c, output);
                        break;
                    default:
                        output.Append(c);
                        break;
                }

            }
            return output.ToString();

        }

        private static void AppendNumericEntity(char c, StringBuilder output)
        {
            output.Append('&').Append('#');
            output.Append(((int)c).ToString(CultureInfo.InvariantCulture));
            output.Append(';');
        }

        public static string EscapeEntities(string plaintext)
        {
            if (plaintext == null)
                return null;

            StringBuilder output = new StringBuilder();
            foreach (char c in plaintext)
            {
                output.Append(EntityEscaper.Char(c));
            }
            return output.ToString();
        }

        public static int DecodeEntityReference(string charref)
        {
            // most common case--entity reference
            int charCode = EntityEscaper.Code(charref, false);
            if (charCode != -1)
            {
                return (char)charCode;
            }

            // no?  maybe it's a numeric reference
            if (charref[0] == '#')
            {
                charref = charref.Substring(1);

                // maybe it's a decimal character reference
                if (Regex.IsMatch(charref, @"^[0-9]+$"))
                {
                    try
                    {
                        int decCode = int.Parse(charref, CultureInfo.InvariantCulture);
                        if (decCode < 0xFFFF)
                        {
                            return (char)decCode;
                        }
                    }
                    catch (FormatException) { }  // parsing error
                    catch (OverflowException) { }  // number too big
                }

                // if not, maybe it's a hex character reference
                if (charref[0] == 'x')
                {
                    try
                    {
                        int decCode = int.Parse(charref.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        if (decCode < 0xFFFF)
                        {
                            return (char)decCode;
                        }
                    }
                    catch (FormatException) { }  // parsing error
                    catch (OverflowException) { }  // number too big
                }
            }

            return -1;
        }

        [Obsolete("Use overload with UnEscapeMode")]
        public static string UnEscapeEntities(string html)
        {
            return UnEscapeEntities(html, UnEscapeMode.Default);
        }

        public enum UnEscapeMode { Default, Attribute, NonMarkupText = Default }

        /// <summary>
        /// The unsafeForAttributesMode tells the method to perform more aggressive
        /// matching of "basic" entities, like IE does on non-markup HTML text.
        /// However we can't do this kind of matching for attributes, since it
        /// breaks URLs.  When in doubt, use false.
        ///
        /// Example:
        ///
        /// UnEscapeEntities("&pounda", true) => "£a"
        /// UnEscapeEntities("&pounda", false) => "&pounda"
        /// </summary>
        public static string UnEscapeEntities(string html, UnEscapeMode unEscapeMode)
        {
            if (html == null)
                return null;

            StringBuilder output = new StringBuilder(html.Length);
            int len = html.Length;
            for (int i = 0; i < len; i++)
            {
                char c0 = html[i];
                if (c0 == '&')
                {
                    if (i + 1 < len)
                    {
                        char c1 = html[i + 1];
                        switch (c1)
                        {
                            case '#':
                                {
                                    if (i + 2 < len)
                                    {
                                        char c2 = html[i + 2];
                                        switch (c2)
                                        {
                                            case 'x':
                                            case 'X':
                                                {
                                                    // do hexadecimal match

                                                    bool semicolonTerminated = false;
                                                    int charVal = 0;
                                                    int j;
                                                    for (j = i + 3; j < len; j++)
                                                    {
                                                        int hexVal = ToHexValue(html[j]);
                                                        if (hexVal == -1)
                                                        {
                                                            // skip one more char if currently on semicolon
                                                            if (html[j] == ';')
                                                                semicolonTerminated = true;
                                                            break;
                                                        }
                                                        charVal *= 16;
                                                        charVal += hexVal;
                                                    }
                                                    if (semicolonTerminated && charVal != 0)
                                                    {
                                                        i = j;
                                                        output.Append((char)charVal);
                                                        continue;
                                                    }
                                                    // if total is 0, continue
                                                    break;
                                                }
                                            case '0':
                                            case '1':
                                            case '2':
                                            case '3':
                                            case '4':
                                            case '5':
                                            case '6':
                                            case '7':
                                            case '8':
                                            case '9':
                                                {
                                                    // do decimal match

                                                    int charVal = 0;
                                                    int j;
                                                    for (j = i + 2; j < len; j++)
                                                    {
                                                        char c = html[j];
                                                        if (c < '0' || c > '9')
                                                        {
                                                            if (c == ';')
                                                                ++j;
                                                            break;
                                                        }

                                                        int cVal = c - '0';
                                                        charVal *= 10;
                                                        charVal += cVal;
                                                    }
                                                    if (charVal != 0)
                                                    {
                                                        i = j - 1;
                                                        output.Append((char)charVal);
                                                        continue;
                                                    }
                                                    // if total is 0, continue
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    int j;
                                    int end = Math.Min(len, i + 12);
                                    for (j = i + 1; j < end; j++)
                                    {
                                        char c = html[j];
                                        if (c == ';' || (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9')))
                                        {
                                            break;
                                        }
                                    }

                                    string entityRef = html.Substring(i + 1, j - (i + 1));

                                    if (unEscapeMode != UnEscapeMode.Attribute)
                                    {
                                        // k = number of characters in entityRef that we are using
                                        int k, code = -1;
                                        for (k = 1; k < entityRef.Length; k++)
                                        {
                                            if (-1 != (code = EntityEscaper.Code(entityRef.Substring(0, k), true)))
                                                break;
                                        }

                                        if (code == -1)
                                        {
                                            code = EntityEscaper.Code(entityRef, false);
                                        }

                                        if (code != -1)
                                        {
                                            output.Append((char)code);
                                            i += 1 + k;
                                            if (i < end && html[i] == ';')
                                                ++i;
                                            --i;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        int code = EntityEscaper.Code(entityRef, false);
                                        if (code != -1)
                                        {
                                            output.Append((char)code);
                                            i += 1 + entityRef.Length;
                                            if (i < end && html[i] == ';')
                                                ++i;
                                            --i;
                                            continue;
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
                output.Append(c0);
            }
            return output.ToString();
        }

        private static int ToHexValue(char c)
        {
            return
                (c >= '0' && c <= '9') ? c - '0' :
                (c >= 'A' && c <= 'F') ? c - 'A' + 10 :
                (c >= 'a' && c <= 'f') ? c - 'a' + 10 :
                -1;
        }

        /// <summary>
        /// Removes all &nbsp;
        /// </summary>
        public static string TidyNbsps(string html)
        {
            // watch out for special case: <p>&nbsp;</p>, <td>&nbsp;</td>, etc.
            if (html == "&nbsp;")
                return html;

            //return Regex.Replace(html, @"(?<!(\s|&nbsp;))&nbsp;(?!(\s|&nbsp;))", " ");
            return Regex.Replace(
                html,
                @"(&nbsp;|(?>\s+))*&nbsp;(&nbsp;|(?>\s+))*",
                new MatchEvaluator(new TidyNbspsHelper(html).Evaluator)
                );
        }

        private class TidyNbspsHelper
        {
            private string _html;

            public TidyNbspsHelper(string html)
            {
                _html = html;
            }

            public string Evaluator(Match match)
            {
                int count = match.Groups[1].Captures.Count + match.Groups[2].Captures.Count + 1;

                if (count == 1) // special case for standalone &nbsp;
                {
                    // watch out for special case: <p>&nbsp;</p>, <td>&nbsp;</td>, etc.
                    if (match.Index > 0 && _html[match.Index - 1] == '>'
                        && match.Index + match.Length < _html.Length - 1 && _html[match.Index + match.Length] == '<')
                    {
                        return "&nbsp;";
                    }
                    else
                    {
                        return " ";
                    }
                }

                int strLen = ("&nbsp;".Length * (count - 1)) + 1;
                StringBuilder sb = new StringBuilder(strLen);
                for (int i = 0; i < count - 1; i++)
                    sb.Append("&nbsp;");
                sb.Append(" ");
                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// Escapes all character entity references from HTML 4.01 spec.
    /// Data is parsed from:
    /// http://www.w3.org/TR/REC-html40/sgml/entities.html
    /// </summary>
    public class EntityEscaper
    {
        private readonly static Dictionary<string, int> basicCodes;
        private readonly static Dictionary<string, int> codes;
        private readonly static Dictionary<int, string> rBasicCodes;
        private readonly static Dictionary<int, string> rcodes;

        static EntityEscaper()
        {
            basicCodes = new Dictionary<string, int>((int)(96 * 1.3));
            #region ISO 8859-1 characters
            basicCodes.Add("nbsp", 160);
            basicCodes.Add("iexcl", 161);
            basicCodes.Add("cent", 162);
            basicCodes.Add("pound", 163);
            basicCodes.Add("curren", 164);
            basicCodes.Add("yen", 165);
            basicCodes.Add("brvbar", 166);
            basicCodes.Add("sect", 167);
            basicCodes.Add("uml", 168);
            basicCodes.Add("copy", 169);
            basicCodes.Add("ordf", 170);
            basicCodes.Add("laquo", 171);
            basicCodes.Add("not", 172);
            basicCodes.Add("shy", 173);
            basicCodes.Add("reg", 174);
            basicCodes.Add("macr", 175);
            basicCodes.Add("deg", 176);
            basicCodes.Add("plusmn", 177);
            basicCodes.Add("sup2", 178);
            basicCodes.Add("sup3", 179);
            basicCodes.Add("acute", 180);
            basicCodes.Add("micro", 181);
            basicCodes.Add("para", 182);
            basicCodes.Add("middot", 183);
            basicCodes.Add("cedil", 184);
            basicCodes.Add("sup1", 185);
            basicCodes.Add("ordm", 186);
            basicCodes.Add("raquo", 187);
            basicCodes.Add("frac14", 188);
            basicCodes.Add("frac12", 189);
            basicCodes.Add("frac34", 190);
            basicCodes.Add("iquest", 191);
            basicCodes.Add("Agrave", 192);
            basicCodes.Add("Aacute", 193);
            basicCodes.Add("Acirc", 194);
            basicCodes.Add("Atilde", 195);
            basicCodes.Add("Auml", 196);
            basicCodes.Add("Aring", 197);
            basicCodes.Add("AElig", 198);
            basicCodes.Add("Ccedil", 199);
            basicCodes.Add("Egrave", 200);
            basicCodes.Add("Eacute", 201);
            basicCodes.Add("Ecirc", 202);
            basicCodes.Add("Euml", 203);
            basicCodes.Add("Igrave", 204);
            basicCodes.Add("Iacute", 205);
            basicCodes.Add("Icirc", 206);
            basicCodes.Add("Iuml", 207);
            basicCodes.Add("ETH", 208);
            basicCodes.Add("Ntilde", 209);
            basicCodes.Add("Ograve", 210);
            basicCodes.Add("Oacute", 211);
            basicCodes.Add("Ocirc", 212);
            basicCodes.Add("Otilde", 213);
            basicCodes.Add("Ouml", 214);
            basicCodes.Add("times", 215);
            basicCodes.Add("Oslash", 216);
            basicCodes.Add("Ugrave", 217);
            basicCodes.Add("Uacute", 218);
            basicCodes.Add("Ucirc", 219);
            basicCodes.Add("Uuml", 220);
            basicCodes.Add("Yacute", 221);
            basicCodes.Add("THORN", 222);
            basicCodes.Add("szlig", 223);
            basicCodes.Add("agrave", 224);
            basicCodes.Add("aacute", 225);
            basicCodes.Add("acirc", 226);
            basicCodes.Add("atilde", 227);
            basicCodes.Add("auml", 228);
            basicCodes.Add("aring", 229);
            basicCodes.Add("aelig", 230);
            basicCodes.Add("ccedil", 231);
            basicCodes.Add("egrave", 232);
            basicCodes.Add("eacute", 233);
            basicCodes.Add("ecirc", 234);
            basicCodes.Add("euml", 235);
            basicCodes.Add("igrave", 236);
            basicCodes.Add("iacute", 237);
            basicCodes.Add("icirc", 238);
            basicCodes.Add("iuml", 239);
            basicCodes.Add("eth", 240);
            basicCodes.Add("ntilde", 241);
            basicCodes.Add("ograve", 242);
            basicCodes.Add("oacute", 243);
            basicCodes.Add("ocirc", 244);
            basicCodes.Add("otilde", 245);
            basicCodes.Add("ouml", 246);
            basicCodes.Add("divide", 247);
            basicCodes.Add("oslash", 248);
            basicCodes.Add("ugrave", 249);
            basicCodes.Add("uacute", 250);
            basicCodes.Add("ucirc", 251);
            basicCodes.Add("uuml", 252);
            basicCodes.Add("yacute", 253);
            basicCodes.Add("thorn", 254);
            basicCodes.Add("yuml", 255);
            #endregion

            codes = new Dictionary<string, int>(basicCodes);

            #region Symbols, mathematical symbols, and Greek letters
            codes.Add("fnof", 402);
            codes.Add("Alpha", 913);
            codes.Add("Beta", 914);
            codes.Add("Gamma", 915);
            codes.Add("Delta", 916);
            codes.Add("Epsilon", 917);
            codes.Add("Zeta", 918);
            codes.Add("Eta", 919);
            codes.Add("Theta", 920);
            codes.Add("Iota", 921);
            codes.Add("Kappa", 922);
            codes.Add("Lambda", 923);
            codes.Add("Mu", 924);
            codes.Add("Nu", 925);
            codes.Add("Xi", 926);
            codes.Add("Omicron", 927);
            codes.Add("Pi", 928);
            codes.Add("Rho", 929);
            codes.Add("Sigma", 931);
            codes.Add("Tau", 932);
            codes.Add("Upsilon", 933);
            codes.Add("Phi", 934);
            codes.Add("Chi", 935);
            codes.Add("Psi", 936);
            codes.Add("Omega", 937);
            codes.Add("alpha", 945);
            codes.Add("beta", 946);
            codes.Add("gamma", 947);
            codes.Add("delta", 948);
            codes.Add("epsilon", 949);
            codes.Add("zeta", 950);
            codes.Add("eta", 951);
            codes.Add("theta", 952);
            codes.Add("iota", 953);
            codes.Add("kappa", 954);
            codes.Add("lambda", 955);
            codes.Add("mu", 956);
            codes.Add("nu", 957);
            codes.Add("xi", 958);
            codes.Add("omicron", 959);
            codes.Add("pi", 960);
            codes.Add("rho", 961);
            codes.Add("sigmaf", 962);
            codes.Add("sigma", 963);
            codes.Add("tau", 964);
            codes.Add("upsilon", 965);
            codes.Add("phi", 966);
            codes.Add("chi", 967);
            codes.Add("psi", 968);
            codes.Add("omega", 969);
            codes.Add("thetasym", 977);
            codes.Add("upsih", 978);
            codes.Add("piv", 982);
            codes.Add("bull", 8226);
            codes.Add("hellip", 8230);
            codes.Add("prime", 8242);
            codes.Add("Prime", 8243);
            codes.Add("oline", 8254);
            codes.Add("frasl", 8260);
            codes.Add("weierp", 8472);
            codes.Add("image", 8465);
            codes.Add("real", 8476);
            codes.Add("trade", 8482);
            codes.Add("alefsym", 8501);
            codes.Add("larr", 8592);
            codes.Add("uarr", 8593);
            codes.Add("rarr", 8594);
            codes.Add("darr", 8595);
            codes.Add("harr", 8596);
            codes.Add("crarr", 8629);
            codes.Add("lArr", 8656);
            codes.Add("uArr", 8657);
            codes.Add("rArr", 8658);
            codes.Add("dArr", 8659);
            codes.Add("hArr", 8660);
            codes.Add("forall", 8704);
            codes.Add("part", 8706);
            codes.Add("exist", 8707);
            codes.Add("empty", 8709);
            codes.Add("nabla", 8711);
            codes.Add("isin", 8712);
            codes.Add("notin", 8713);
            codes.Add("ni", 8715);
            codes.Add("prod", 8719);
            codes.Add("sum", 8721);
            codes.Add("minus", 8722);
            codes.Add("lowast", 8727);
            codes.Add("radic", 8730);
            codes.Add("prop", 8733);
            codes.Add("infin", 8734);
            codes.Add("ang", 8736);
            codes.Add("and", 8743);
            codes.Add("or", 8744);
            codes.Add("cap", 8745);
            codes.Add("cup", 8746);
            codes.Add("int", 8747);
            codes.Add("there4", 8756);
            codes.Add("sim", 8764);
            codes.Add("cong", 8773);
            codes.Add("asymp", 8776);
            codes.Add("ne", 8800);
            codes.Add("equiv", 8801);
            codes.Add("le", 8804);
            codes.Add("ge", 8805);
            codes.Add("sub", 8834);
            codes.Add("sup", 8835);
            codes.Add("nsub", 8836);
            codes.Add("sube", 8838);
            codes.Add("supe", 8839);
            codes.Add("oplus", 8853);
            codes.Add("otimes", 8855);
            codes.Add("perp", 8869);
            codes.Add("sdot", 8901);
            codes.Add("lceil", 8968);
            codes.Add("rceil", 8969);
            codes.Add("lfloor", 8970);
            codes.Add("rfloor", 8971);
            codes.Add("lang", 9001);
            codes.Add("rang", 9002);
            codes.Add("loz", 9674);
            codes.Add("spades", 9824);
            codes.Add("clubs", 9827);
            codes.Add("hearts", 9829);
            codes.Add("diams", 9830);
            #endregion
            #region Markup-significant and internationalization characters
            codes.Add("quot", 34);
            codes.Add("amp", 38);
            codes.Add("lt", 60);
            codes.Add("gt", 62);
            codes.Add("OElig", 338);
            codes.Add("oelig", 339);
            codes.Add("Scaron", 352);
            codes.Add("scaron", 353);
            codes.Add("Yuml", 376);
            codes.Add("circ", 710);
            codes.Add("tilde", 732);
            codes.Add("ensp", 8194);
            codes.Add("emsp", 8195);
            codes.Add("thinsp", 8201);
            codes.Add("zwnj", 8204);
            codes.Add("zwj", 8205);
            codes.Add("lrm", 8206);
            codes.Add("rlm", 8207);
            codes.Add("ndash", 8211);
            codes.Add("mdash", 8212);
            codes.Add("lsquo", 8216);
            codes.Add("rsquo", 8217);
            codes.Add("sbquo", 8218);
            codes.Add("ldquo", 8220);
            codes.Add("rdquo", 8221);
            codes.Add("bdquo", 8222);
            codes.Add("dagger", 8224);
            codes.Add("Dagger", 8225);
            codes.Add("permil", 8240);
            codes.Add("lsaquo", 8249);
            codes.Add("rsaquo", 8250);
            codes.Add("euro", 8364);
            #endregion

            PopulateReverse(codes, ref rcodes);
            PopulateReverse(basicCodes, ref rBasicCodes);
        }

        private static void PopulateReverse(Dictionary<string, int> fwd, ref Dictionary<int, string> rev)
        {
            rev = new Dictionary<int, string>((int)(fwd.Count * 1.3));
            foreach (KeyValuePair<string, int> entry in fwd)
            {
                rev[(char)entry.Value] = entry.Key;
            }
        }

        /// <summary>
        /// Returns -1 if not found
        /// </summary>
        public static int Code(string name, bool unterminated)
        {
            Dictionary<string, int> codesToUse = unterminated ? basicCodes : codes;
            int retVal;
            if (codesToUse.TryGetValue(name, out retVal))
                return retVal;
            else
                return -1;
        }

        public static bool HasChar(char c)
        {
            return rcodes.ContainsKey(c);
        }

        public static string Char(char c)
        {
            if (rcodes.ContainsKey(c))
                return "&" + rcodes[c] + ";";
            else
                return c.ToString();
        }
    }

}
