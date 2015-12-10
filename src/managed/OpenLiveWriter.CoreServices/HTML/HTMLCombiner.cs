// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Takes zero or more HTML documents or snippets and combines them.
    /// </summary>
    public class HTMLCombiner
    {
        private HTMLCombiner() { }

        private static Regex bodyFinder = new Regex(@"<body(\s[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex bodyEndFinder = new Regex(@"</body\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex fragmentMarkerFinder = new Regex(@"<!--\s*(startfragment|endfragment)\s*-->", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static Regex framesetFinder = new Regex(@"<frameset\s", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex noframesFinder = new Regex(@"<noframes(?:\s[^>]*)?>(.*?)</noframes\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static string Combine(params string[] strings)
        {
            if (strings.Length == 0)
                return string.Empty;

            StringBuilder buf = new StringBuilder(2048);
            foreach (string html_readonly in strings)
            {
                if (html_readonly == null || html_readonly.Length == 0)
                    continue;

                string html = html_readonly;
                // Delete everything before last body tag
                MatchCollection matches = bodyFinder.Matches(html);
                if (matches.Count != 0)
                {
                    Match lastMatch = matches[matches.Count - 1];
                    html = html.Substring(lastMatch.Index + lastMatch.Length);
                }

                // Delete everything after first body end tag
                MatchCollection matches2 = bodyEndFinder.Matches(html);
                if (matches2.Count != 0)
                {
                    Match lastMatch = matches2[matches2.Count - 1];
                    html = html.Substring(0, lastMatch.Index);
                }

                // Delete StartFragment/EndFragment comments
                html = fragmentMarkerFinder.Replace(html, "");

                Match framesMatch = framesetFinder.Match(html);
                if (framesMatch != null && framesMatch.Success)
                {
                    Match noframesMatch = noframesFinder.Match(html);
                    if (noframesMatch != null && noframesMatch.Success)
                        html = noframesMatch.Captures[0].Value;
                    else
                        html = "";
                }

                buf.Append("<p>");
                buf.Append(html);
            }
            return buf.ToString();
        }
    }
}
