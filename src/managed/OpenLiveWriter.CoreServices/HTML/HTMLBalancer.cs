// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    public class HTMLBalancer
    {
        /// <summary>
        /// Tries to make the HTML string safe for inclusion in other HTML
        /// documents, as far as tag balancing goes.  Happens to also strip
        /// markup directives, comments, inline styles, and inline scripts--
        /// however if you require that behavior, it's better to first do the
        /// stripping using a more robust mechanism than this.
        /// </summary>
        public static string Balance(string html)
        {
            return Balance(html, int.MaxValue);
        }

        /// <summary>
        /// Balances the HTML and safely truncates it to no more than maxLen
        /// characters. Note that any Unicode character counts as one character
        /// for these purposes.
        /// </summary>
        public static string Balance(string html, int maxLen)
        {
            return Balance(html, maxLen, new DefaultCostFilter(), false);
        }

        /// <summary>
        /// Balances the HTML and safely truncates it to a length that will
        /// fit within maxChars after URL encoding.
        /// </summary>
        public static string BalanceForUrl(string html, int maxChars)
        {
            return Balance(html, maxChars, new UrlEncodingCostFilter(), false);
        }

        /// <summary>
        /// Balances the HTML and safely truncates it, using a custom algorithm
        /// to determine how much each character/string counts against maxCost.
        /// </summary>
        public static string Balance(string html, int maxCost, HTMLBalancerCostFilter costFilter, bool ellipsis)
        {
            bool appendEllipsis = false;
            SimpleHtmlParser parser = new SimpleHtmlParser(html);

            ArrayList openTags = new ArrayList();
            StringBuilder output = new StringBuilder();
            long balance = 0;  // long to make sure that int32.MaxValue does not cause overflow

            if (costFilter == null)
                costFilter = new DefaultCostFilter();

            Element el;
            while (null != (el = parser.Next()))
            {
                if (el is StyleElement ||
                   el is ScriptElement ||
                   el is Comment ||
                   el is MarkupDirective)
                {
                    continue;
                }

                long lenLeft = Math.Max(0, maxCost - balance - LengthToClose(costFilter, openTags));

                if (el is Tag)
                {
                    if (el is BeginTag && ((BeginTag)el).Unterminated)
                        continue;  // skip corrupted tags

                    if (TagCost(costFilter, openTags, (Tag)el) > lenLeft)
                        break;  // don't use this tag; we're done
                    else
                    {
                        RegisterTag(openTags, (Tag)el);
                        output.Append(el.ToString());
                        balance += costFilter.ElementCost(el);
                    }
                }
                else if (el is Text)
                {
                    if (costFilter.ElementCost(el) > lenLeft)
                    {
                        // shrink down the text to fit
                        output.Append(costFilter.TruncateText((Text)el, (int)lenLeft));
                        appendEllipsis = true;
                        break;
                    }
                    else
                    {
                        // plenty of room
                        output.Append(el.ToString());
                        balance += costFilter.ElementCost(el);
                    }

                    //update the text end index
                }
                else
                {
                    if (costFilter.ElementCost(el) > lenLeft)
                        break;
                    else
                    {
                        output.Append(el.ToString());
                        balance += costFilter.ElementCost(el);
                    }
                }
            }

            // Append an ellipsis if we truncated text
            // We use "..." here rather than TextHelper.Ellipsis, because some mail clients don't understand "\u2026".
            if (ellipsis && appendEllipsis)
                output.Append("...");

            for (int i = openTags.Count - 1; i >= 0; i--)
            {
                output.Append(MakeEndTag((string)openTags[i]));
            }

            return output.ToString();
        }

        private static void RegisterTag(ArrayList openTags, Tag tag)
        {
            if (tag is BeginTag && TagRequiresClose(tag.Name))
            {
                openTags.Add(tag.Name);
            }
            else if (tag is EndTag)
            {
                int idx = LastIndexOf(openTags, tag);
                if (idx != -1)
                    openTags.RemoveAt(idx);
            }
        }

        private static int LastIndexOf(ArrayList openTags, Tag tag)
        {
            for (int i = openTags.Count - 1; i >= 0; i--)
            {
                if (tag.NameEquals((string)openTags[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        private static long LengthToClose(HTMLBalancerCostFilter costFilter, ArrayList openTags)
        {
            long total = 0;
            try
            {
                checked
                {
                    for (int i = openTags.Count - 1; i >= 0; i--)
                    {
                        total += costFilter.ElementCost(MakeEndTag((string)openTags[i]));
                    }
                }
            }
            catch (OverflowException)
            {
                return int.MaxValue;
            }
            return total;
        }

        private static int TagCost(HTMLBalancerCostFilter costFilter, ArrayList openTags, Tag tag)
        {
            try
            {
                checked
                {
                    int baseTagCost = costFilter.ElementCost(tag);
                    if (tag is BeginTag)
                    {
                        return baseTagCost + ((TagRequiresClose(tag.Name)) ? costFilter.ElementCost(MakeEndTag(tag.Name)) : 0);
                    }
                    else if (tag is EndTag)
                    {
                        if (LastIndexOf(openTags, tag) != -1)
                            return baseTagCost - costFilter.ElementCost(MakeEndTag(tag.Name));
                        else
                            return baseTagCost;
                    }
                    else
                    {
                        Trace.Fail("Unknown tag type");
                        return baseTagCost;
                    }
                }
            }
            catch (OverflowException)
            {
                return int.MaxValue;
            }
        }

        private static EndTag MakeEndTag(string name)
        {
            string data = "</" + name + ">";
            return new EndTag(data, 0, data.Length, name);
        }

        protected static bool TagRequiresClose(string name)
        {
            // this is just a wild-ass guess based off of the DTD for HTML 4.01 Transitional
            switch (name.ToUpper(CultureInfo.InvariantCulture))
            {
                // end tag is required
                case "A":
                case "ABBR":
                case "ACRONYM":
                case "ADDRESS":
                case "APPLET":
                case "B":
                case "BDO":
                case "BIG":
                case "BLOCKQUOTE":
                case "BUTTON":
                case "CAPTION":
                case "CENTER":
                case "CITE":
                case "CODE":
                case "DEL":
                case "DFN":
                case "DIR":
                case "DIV":
                case "DL":
                case "EM":
                case "FIELDSET":
                case "FONT":
                case "FORM":
                case "H1":
                case "H2":
                case "H3":
                case "H4":
                case "H5":
                case "H6":
                case "I":
                case "IFRAME":
                case "INS":
                case "KBD":
                case "LABEL":
                case "LEGEND":
                case "MAP":
                case "MENU":
                case "NOFRAMES":
                case "NOSCRIPT":
                case "OBJECT":
                case "OL":
                case "OPTGROUP":
                case "PRE":
                case "Q":
                case "S":
                case "SAMP":
                case "SCRIPT":
                case "SELECT":
                case "SMALL":
                case "SPAN":
                case "STRIKE":
                case "STRONG":
                case "STYLE":
                case "SUB":
                case "SUP":
                case "TABLE":
                case "TEXTAREA":
                case "TITLE":
                case "TT":
                case "U":
                case "UL":
                case "VAR":

                // both start and end are optional
                case "BODY":
                case "HEAD":
                case "HTML":
                case "TBODY":

                // Netscape-specific
                case "ILAYER":

                // whatever
                case "FRAMESET":
                case "FRAME":
                    return true;
                default:
                    return false;
            }
        }

#if FALSE
        public static void Test()
        {
            Verify(Balance("<a href=foo>test</a>", 13), "");
            Verify(Balance("<a href=foo>test", 13), "");

            Verify(Balance("<a href=foo>test</a>", 100), "<a href=foo>test</a>");
            Verify(Balance("<a href=foo>test", 100), "<a href=foo>test</a>");
            Verify(Balance("<b><a href=foo>test</b>", 100), "<b><a href=foo>test</b></a>");

            Verify(Balance("<b><a href=foo>test</b>", 10), "<b></b>");
            Verify(Balance("<B><a href=foo>test</b>", 10), "<B></B>");
            Verify(Balance("abcd&blacksquare;efghijklmnop", 7), "abcd");
            Verify(Balance("abcd&blacksquare;efg", 17), "abcd&blacksquare;");
            Verify(Balance("abcd&blacksquare;efg", 34, new DoubleCostFilter()), "abcd&blacksquare;");

            Verify(Balance("<a><b><c><table><tr><td>", int.MaxValue), "<a><b><c><table><tr><td></table></b></a>");
            Verify(BalanceForUrl("<b>test</b>", 20), "<b>tes</b>");

            Verify(Balance("<b>test</b>", 2, new TextOnlyCostFilter()), "<b>te</b>");
            Verify(Balance("<b>test test</b>", 8, new TextOnlyCostFilter()), "<b>test</b>");
        }

        private static void Verify(string a, string b)
        {
            if (a != b)
                throw new Exception(a + " != " + b);
        }

        private class DoubleCostFilter : HTMLBalancerCostFilter
        {
            public override int ElementCost(Element el)
            {
                return el.ToString().Length * 2;
            }

            protected override int CharCost(char c)
            {
                return 2;
            }
        }
#endif
    }
}
