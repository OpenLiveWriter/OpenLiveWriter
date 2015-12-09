// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Text.RegularExpressions;
using System.Web;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Cost filter for HTML truncation purposes.
    /// </summary>
    public abstract class HTMLBalancerCostFilter
    {
        /// <summary>
        /// Determine the cost of an element.
        /// </summary>
        public abstract int ElementCost(Element el);

        /// <summary>
        /// Truncate text.  maxCost is the cost available for the
        /// string you need to return.
        ///
        /// The default implementation uses CharCount() to calculate
        /// where to split, then attempts to split at a space if
        /// possible.
        /// </summary>
        public virtual string TruncateText(Text el, int maxCost)
        {
            if (maxCost == 0)
                return string.Empty;

            string str = el.ToString();

            // normalizes maxCost to a max char length
            int cost = 0;
            int max = -1;
            for (int i = 0; i < str.Length; i++)
            {
                if (cost >= maxCost)
                {
                    max = i;
                    break;
                }
                cost += CharCost(str[i]);
            }
            if (max == -1)
                return str;  // never hit the max

            // avoid splitting up HTML entities

            string trimmedAtWords = StringHelper.RestrictLengthAtWords(str, max);
            if (ElementCost(new Text(trimmedAtWords, 0, trimmedAtWords.Length)) <= maxCost)
            {
                // It is possible for RestrictLengthAtWords to return
                // strings that are longer than desired (if there are
                // no spaces).  Only if that is not the case, use the
                // result.
                return trimmedAtWords;
            }

            // find last '&' before max
            string result = null;
            int lastAmp = str.LastIndexOf('&', max - 1, max);
            if (lastAmp >= 0)
            {
                Regex regex = new Regex(@"&[^\s&;]{1,20};");
                Match match = regex.Match(str, lastAmp);
                if (match.Success && match.Index + match.Length > max)
                    result = str.Substring(0, match.Index);
            }
            if (result == null)
                result = str.Substring(0, max);

            return result;
        }

        /// <summary>
        /// Determine the cost of a single character in a Text part.
        /// </summary>
        protected virtual int CharCost(char c)
        {
            return 1;
        }
    }

    /// <summary>
    /// Cost filter where each character of HTML counts as 1 unit of cost.
    /// </summary>
    public class DefaultCostFilter : HTMLBalancerCostFilter
    {
        public override int ElementCost(Element el)
        {
            return el.ToString().Length;
        }

        protected override int CharCost(char c)
        {
            return 1;
        }
    }

    /// <summary>
    /// Cost filter where each character of HTML costs as many units as
    /// its URL-encoded representation has characters.
    /// </summary>
    public class UrlEncodingCostFilter : HTMLBalancerCostFilter
    {
        public override int ElementCost(Element el)
        {
            return HttpUtility.UrlEncode(el.ToString()).Length;
        }

        protected override int CharCost(char c)
        {
            return HttpUtility.UrlEncode(c.ToString()).Length;
        }
    }

    /// <summary>
    /// Cost filter which ignores any non-Text elements.
    /// </summary>
    public class TextOnlyCostFilter : HTMLBalancerCostFilter
    {
        public override int ElementCost(Element el)
        {
            if (!(el is Text))
                return 0;
            else
                return HtmlUtils.UnEscapeEntities(el.ToString(), HtmlUtils.UnEscapeMode.NonMarkupText).Length;
        }

        protected override int CharCost(char c)
        {
            return 1;
        }
    }
}
