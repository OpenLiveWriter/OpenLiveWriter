// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.HtmlParser.Parser
{
    public class LiteralElementMethods
    {
        public static string CssEscape(string literal, char quotChar, params char[] additionalCharsToQuote)
        {
            StringBuilder output = new StringBuilder((int)(literal.Length * 1.1));
            for (int i = 0; i < literal.Length; i++)
            {
                switch (literal[i])
                {
                    case '\\':
                        output.Append("\\\\");
                        break;
                    case '\r':
                        output.Append('\\');
                        output.Append(((int)'\r').ToString("x", CultureInfo.InvariantCulture));
                        output.Append(' ');
                        break;
                    case '\n':
                        output.Append('\\');
                        output.Append(((int)'\n').ToString("x", CultureInfo.InvariantCulture));
                        output.Append(' ');
                        break;
                    default:
                        if (literal[i] == quotChar)
                        {
                            output.Append('\\');
                        }
                        else
                        {
                            foreach (char c in additionalCharsToQuote)
                            {
                                if (c == literal[i])
                                {
                                    output.Append('\\');
                                    break;
                                }
                            }
                        }
                        output.Append(literal[i]);
                        break;
                }
            }

            return output.ToString();
        }

        public static string JsEscape(string literal, char quotChar)
        {
            StringBuilder output = new StringBuilder((int)(literal.Length * 1.1));
            for (int i = 0; i < literal.Length; i++)
            {
                switch (literal[i])
                {
                    case '"':
                        if (quotChar == '"')
                            output.Append(@"\");
                        output.Append('"');
                        break;
                    case '\'':
                        if (quotChar == '\'')
                            output.Append(@"\");
                        output.Append('\'');
                        break;
                    case '\\': output.Append("\\\\"); break;
                    case '\b': output.Append(@"\b"); break;
                    case '\f': output.Append(@"\f"); break;
                    case '\n': output.Append(@"\n"); break;
                    case '\r': output.Append(@"\r"); break;
                    case '\t': output.Append(@"\t"); break;
                    case '\v': output.Append(@"\v"); break;
                    case '<':
                        if (i < literal.Length - 1 && literal[i + 1] == '/')
                            output.Append(@"<\");  // make sure we don't prematurely end the <script> block
                        else
                            goto default;

                        break;
                    default:
                        // TODO: What characters need to be Unicode escaped??
                        output.Append(literal[i]);
                        break;
                }
            }
            return output.ToString();
        }

        public static string CssUnescape(string data)
        {
            Regex r = new Regex(@"\\((?<hex>[a-fA-F0-9]{1,6}\s?)|(?<lf>\r?\n)|(?<other>.))");
            return r.Replace(data, new MatchEvaluator(_CssUnescapeEvaluator));
        }

        private static string _CssUnescapeEvaluator(Match match)
        {
            if (match.Groups["hex"].Success)
            {
                string hexValue = match.Groups["hex"].Value;
                return ((char)int.Parse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture);
            }
            else if (match.Groups["lf"].Success)
            {
                return string.Empty;
            }
            else
            {
                return match.Groups["other"].Value;
            }
        }

        public static string JsUnescape(string s)
        {
            Regex r = new Regex(@"\\((?<single>[\'\""\\bfnrtv])|(?<nul>0(?![0-9]))|(?<hex>x[a-fA-F0-9]{2})|(?<unicode>u[a-fA-F0-9]{4})|(?<lf>\r?\n)|(?<other>.))");
            return r.Replace(s, new MatchEvaluator(_JsUnescapeEvaluator));
        }

        private static string _JsUnescapeEvaluator(Match match)
        {
            if (match.Groups["single"].Success)
            {
                switch (match.Groups["single"].Value[0])
                {
                    case '\'': return "'";
                    case '"': return "\"";
                    case '\\': return "\\";
                    case 'b': return "\b";
                    case 'f': return "\f";
                    case 'n': return "\n";
                    case 'r': return "\r";
                    case 't': return "\t";
                    case 'v': return "\v";
                    default:
                        Trace.Fail("Unexpectedly unknown single character escape: " + match.Groups["single"].Value[1]);
                        return match.Groups["single"].Value;
                }
            }
            else if (match.Groups["nul"].Success)
            {
                return "\u0000";
            }
            else if (match.Groups["hex"].Success)
            {
                string hexValue = match.Groups["hex"].Value.Substring(1);
                return ((char)int.Parse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString();
            }
            else if (match.Groups["unicode"].Success)
            {
                string hexValue = match.Groups["unicode"].Value.Substring(1);
                return ((char)int.Parse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString();
            }
            else if (match.Groups["lf"].Success)
            {
                return string.Empty;
            }
            else
            {
                return match.Groups["other"].Value;
            }
        }
    }
}
