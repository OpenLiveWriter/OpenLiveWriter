// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OpenLiveWriter.HtmlParser.Parser
{
    /// <summary>
    /// Parser that is suitable for parsing HTML.
    ///
    /// The HTML does not need to be well-formed XML (i.e. mismatched tags are fine)
    /// or even well-formed HTML. In all but the most pathological cases, the parser
    /// will behave in a reasonable way that is similar to IE and Firefox.
    /// </summary>
    public class SimpleHtmlParser : IElementSource
    {
        private bool supportTrailingEnd = false;

        private readonly Stack<Element> elementStack = new Stack<Element>(5);
        private readonly List<Element> peekElements = new List<Element>();

        private readonly string data;  // the complete, raw HTML string
        private int pos = 0;  // the current parsing position

        private static readonly Regex comment = new Regex(@"<!--.*?--\s*>", RegexOptions.Compiled | RegexOptions.Singleline);
        // private static readonly Regex directive = new Regex(@"<![a-z][a-z0-9\.\-_:]*.*?>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex directive = new Regex(@"<!(?!--).*?>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly Regex endScript = new Regex(@"</script\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex endStyle = new Regex(@"</style\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex begin = new Regex(@"<(?<tagname>[a-z][a-z0-9\.\-_:]*)",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        private static readonly Regex attrName = new Regex(@"\s*([a-z][a-z0-9\.\-_:]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex quotedAttrValue = new Regex(@"\s*=\s*([""'])(.*?)\1", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex unquotedAttrValue = new Regex(@"\s*=\s*([^\s>]+)", RegexOptions.Compiled);
        private static readonly Regex endBeginTag = new Regex(@"\s*(/)?>", RegexOptions.Compiled);
        private static readonly Regex end = new Regex(@"</([a-z][a-z0-9\.\-_:]*)\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly StatefulMatcher commentMatcher;
        private readonly StatefulMatcher directiveMatcher;
        private readonly StatefulMatcher beginMatcher;
        private readonly StatefulMatcher attrNameMatcher;
        private readonly StatefulMatcher quotedAttrValueMatcher;
        private readonly StatefulMatcher unquotedAttrValueMatcher;
        private readonly StatefulMatcher endBeginTagMatcher;
        private readonly StatefulMatcher endMatcher;

        /// <param name="data">The HTML string.</param>
        public SimpleHtmlParser(string data)
        {
            this.data = data;

            commentMatcher = new StatefulMatcher(data, comment);
            directiveMatcher = new StatefulMatcher(data, directive);
            beginMatcher = new StatefulMatcher(data, begin);
            endMatcher = new StatefulMatcher(data, end);
            attrNameMatcher = new StatefulMatcher(data, attrName);
            quotedAttrValueMatcher = new StatefulMatcher(data, quotedAttrValue);
            unquotedAttrValueMatcher = new StatefulMatcher(data, unquotedAttrValue);
            endBeginTagMatcher = new StatefulMatcher(data, endBeginTag);
        }

        public int Position
        {
            get
            {
                if (peekElements.Count != 0)
                    return peekElements[0].Offset;
                if (elementStack.Count != 0)
                    return elementStack.Peek().Offset;
                else
                    return pos;
            }
        }

        public Element Peek(int offset)
        {
            Element e;
            while (peekElements.Count <= offset && (e = Next(false)) != null)
                peekElements.Add(e);

            if (peekElements.Count > offset)
                return peekElements[offset];
            else
                return null;
        }

        public Element Next()
        {
            return Next(true);
        }
        /// <summary>
        /// Retrieves the next element from the stream, or null
        /// if the end of the stream has been reached.
        /// </summary>
        private Element Next(bool allowPeekElement)
        {
            if (allowPeekElement && peekElements.Count > 0)
            {
                Element peekElement = peekElements[0];
                peekElements.RemoveAt(0);
                return peekElement;
            }

            if (elementStack.Count != 0)
            {
                return elementStack.Pop();
            }

            int dataLen = data.Length;
            if (dataLen == pos)
            {
                // If we're at EOF, return

                return null;
            }

            // None of the special cases are true.  Start consuming characters

            int tokenStart = pos;

            while (true)
            {
                // Consume everything until a tag-looking thing
                while (pos < dataLen && data[pos] != '<')
                    pos++;

                if (pos >= dataLen)
                {
                    // EOF has been reached.
                    if (tokenStart != pos)
                        return new Text(data, tokenStart, pos - tokenStart);
                    else
                        return null;
                }

                // We started parsing right on a tag-looking thing.  Try
                // parsing it as such.  If it doesn't turn out to be a tag,
                // we'll return it as text

                int oldPos = pos;

                Element element;
                EndTag trailingEnd;
                int len = ParseMarkup(out element, out trailingEnd);
                if (len >= 0)
                {
                    pos += len;

                    if (trailingEnd != null)
                    {
                        // empty-element tag detected, add implicit end tag
                        elementStack.Push(trailingEnd);
                    }
                    else if (element is BeginTag)
                    {
                        // look for <script> or <style> body

                        Regex consumeTextUntil = null;

                        BeginTag tag = (BeginTag)element;
                        if (tag.NameEquals("script"))
                            consumeTextUntil = endScript;
                        else if (tag.NameEquals("style"))
                            consumeTextUntil = endStyle;

                        if (consumeTextUntil != null)
                        {
                            int structuredTextLen = ConsumeStructuredText(data, pos, consumeTextUntil);
                            pos += structuredTextLen;
                        }
                    }

                    elementStack.Push(element);
                    if (oldPos != tokenStart)
                    {
                        elementStack.Push(new Text(data, tokenStart, oldPos - tokenStart));
                    }

                    return elementStack.Pop();
                }
                else
                {
                    // '<' didn't begin a tag after all;
                    // consume it and continue
                    pos++;
                    continue;
                }
            }
        }

        /// <summary>
        /// Collects text between the current parsing position and
        /// the matching endTagName.  (Nested instances of the tag
        /// will not stop the collection.)
        /// </summary>
        /// <param name="endTagName">The name of the end tag, e.g. "div"
        /// (do NOT include angle brackets).</param>
        public string CollectTextUntil(string endTagName)
        {
            int tagCount = 1;
            StringBuilder buf = new StringBuilder();

            while (true)
            {
                Element el = Next();

                if (el == null)
                {
                    break;
                }
                if (el is BeginTag && ((BeginTag)el).NameEquals(endTagName))
                {
                    tagCount++;
                }
                else if (el is EndTag && ((EndTag)el).NameEquals(endTagName))
                {
                    if (--tagCount == 0)
                        break;
                }
                else if (el is Text)
                {
                    // TODO: Instead of adding a single space
                    // between text nodes, we could add the appropriate space as
                    // we encounter space-influencing nodes.  i.e. when we encounter
                    // <p>, append "\r\n\r\n" to the buffer.
                    //
                    // Alternatively, we could add all of the tags to the buffer,
                    // and then call HTMLDocumentHelper.HTMLToPlainText() on the
                    // buf before returning.
                    if (buf.Length != 0)
                        buf.Append(' ');
                    buf.Append(((Text)el).ToString());
                }
            }

            return buf.ToString();
        }

        /// <summary>
        /// Collects all HTML between the current parsing position and
        /// the matching endTagName.  (Nested instances of the tag
        /// will not stop the collection.)
        /// </summary>
        /// <param name="endTagName">The name of the end tag, e.g. "div"
        /// (do NOT include angle brackets).</param>
        public string CollectHtmlUntil(string endTagName)
        {
            int tagCount = 1;
            StringBuilder buf = new StringBuilder();

            while (true)
            {
                Element el = Next();

                if (el == null)
                {
                    break;
                }
                if (el is BeginTag && ((BeginTag)el).NameEquals(endTagName))
                {
                    tagCount++;
                }
                else if (el is EndTag && ((EndTag)el).NameEquals(endTagName))
                {
                    if (--tagCount == 0)
                        break;
                }
                buf.Append(data, el.Offset, el.Length);
            }

            return buf.ToString();
        }

        private int ParseMarkup(out Element element, out EndTag trailingEnd)
        {
            trailingEnd = null;

            Match m;

            // commentMatcher MUST be checked before directiveMatcher!
            m = commentMatcher.Match(pos);
            if (m != null)
            {
                element = new Comment(data, pos, m.Length);
                return m.Length;
            }

            // commentMatcher MUST be checked before directiveMatcher!
            m = directiveMatcher.Match(pos);
            if (m != null)
            {
                element = new MarkupDirective(data, pos, m.Length);
                return m.Length;
            }

            m = endMatcher.Match(pos);
            if (m != null)
            {
                element = new EndTag(data, pos, m.Length, m.Groups[1].Value);
                return m.Length;
            }

            m = beginMatcher.Match(pos);
            if (m != null)
            {
                return ParseBeginTag(m, out element, out trailingEnd);
            }

            element = null;
            return -1;
        }

        private int ParseBeginTag(Match beginMatch, out Element element, out EndTag trailingEnd)
        {
            trailingEnd = null;

            Group tagNameGroup = beginMatch.Groups["tagname"];
            string tagName = tagNameGroup.Value;

            int tagPos = tagNameGroup.Index + tagNameGroup.Length;

            ArrayList attributes = null;
            LazySubstring extraResidue = null;
            bool isComplete = false;

            while (true)
            {
                Match match = endBeginTagMatcher.Match(tagPos);
                if (match != null)
                {
                    tagPos += match.Length;
                    if (match.Groups[1].Success)
                    {
                        isComplete = true;
                        if (supportTrailingEnd)
                            trailingEnd = new EndTag(data, tagPos, 0, tagName, true);
                    }
                    break;
                }

                match = attrNameMatcher.Match(tagPos);
                if (match == null)
                {
                    int residueStart = tagPos;
                    int residueEnd;

                    residueEnd = tagPos = data.IndexOfAny(new char[] { '<', '>' }, tagPos);
                    if (tagPos == -1)
                    {
                        residueEnd = tagPos = data.Length;
                    }
                    else if (data[tagPos] == '>')
                    {
                        tagPos++;
                    }
                    else
                    {
                        Debug.Assert(data[tagPos] == '<');
                    }

                    extraResidue = residueStart < residueEnd ? new LazySubstring(data, residueStart, residueEnd - residueStart) : null;
                    break;
                }
                else
                {
                    tagPos += match.Length;
                    LazySubstring attrName = new LazySubstring(data, match.Groups[1].Index, match.Groups[1].Length);
                    LazySubstring attrValue = null;
                    match = quotedAttrValueMatcher.Match(tagPos);
                    if (match != null)
                    {
                        attrValue = new LazySubstring(data, match.Groups[2].Index, match.Groups[2].Length);
                        tagPos += match.Length;
                    }
                    else
                    {
                        match = unquotedAttrValueMatcher.Match(tagPos);
                        if (match != null)
                        {
                            attrValue = new LazySubstring(data, match.Groups[1].Index, match.Groups[1].Length);
                            tagPos += match.Length;
                        }
                    }

                    // no attribute value; that's OK

                    if (attributes == null)
                        attributes = new ArrayList();
                    attributes.Add(new Attr(attrName, attrValue));
                }
            }

            int len = tagPos - beginMatch.Index;
            element = new BeginTag(data, beginMatch.Index, len, tagName, attributes == null ? null : (Attr[])attributes.ToArray(typeof(Attr)), isComplete, extraResidue);
            return len;
        }

        private int ConsumeStructuredText(string data, int offset, Regex stopAt)
        {
            Match match = stopAt.Match(data, offset);
            /*
                        if (!match.Success)
                        {
                            // Failure.  If an end tag is never encountered, the
                            // begin tag does not count.
                            // We can remove this whole clause if we want to behave
                            // more like IE than Gecko.
                            retval = string.Empty;
                            return 0;
                        }
            */

            int end = match.Success ? match.Index : data.Length;

            // HACK: this code should not be aware of parser types
            IElementSource source = (stopAt == endScript) ? (IElementSource)new JavascriptParser(data, offset, end - offset) : (IElementSource)new CssParser(data, offset, end - offset);
            Stack stack = new Stack();
            Element element;
            int last = pos;
            while (null != (element = source.Next()))
            {
                stack.Push(element);
            }
            foreach (Element el in stack)
            {
                elementStack.Push(el);
            }

            return end - offset;
        }

        private class StatefulMatcher
        {
#if DEBUG
            bool warned;
#endif

            private readonly string input;
            private readonly Regex regex;
            private int lastStartOffset;
            private Match lastMatch;

            public StatefulMatcher(string input, Regex regex)
            {
                this.input = input;
                this.regex = regex;
                this.lastStartOffset = int.MaxValue;
                this.lastMatch = null;
#if DEBUG
                this.warned = false;
#endif
            }

            public Match Match(int pos)
            {
                /* We need to reexecute the search under any of these three conditions:
                 *
                 * 1) The search has never been run
                 * 2) The last search successfully matched before it got to the desired position
                 * 3) The last search was started past the desired position
                 */
                if (lastMatch == null || (lastMatch.Success && lastMatch.Index < pos) || lastStartOffset > pos)
                {
#if DEBUG
                    if (lastStartOffset > pos && lastStartOffset != int.MaxValue)
                    {
                        Debug.Assert(!warned, "StatefulMatcher moving backwards; this will work but is inefficient");
                        warned = true;
                    }
#endif
                    PerformMatch(pos);
                }

                if (lastMatch.Success && pos == lastMatch.Index)
                    return lastMatch;
                else
                    return null;
            }

            private void PerformMatch(int pos)
            {
                lastStartOffset = pos;
                lastMatch = regex.Match(input, pos);
            }
        }

        public static void Create()
        {
            // Touch a static variable to make sure all static variables are created
            if (comment == null)
            {

            }
        }
    }

    /// <summary>
    /// String.Substring is very expensive, so we avoid calling it
    /// until the caller demands it.
    /// </summary>
    internal class LazySubstring
    {
        private readonly string baseString;
        private readonly int offset;
        private readonly int length;

        private string substring;

        public LazySubstring(string baseString, int offset, int length)
        {
            this.baseString = baseString;
            this.offset = offset;
            this.length = length;
        }

        public string Value
        {
            get
            {
                if (substring == null)
                    substring = baseString.Substring(offset, length);
                return substring;
            }
        }

        public int Offset
        {
            get { return offset; }
        }

        public int Length
        {
            get { return length; }
        }

        public static LazySubstring MaybeCreate(string val)
        {
            if (val == null)
                return null;
            else
                return new LazySubstring(val, 0, val.Length);
        }
    }

}
