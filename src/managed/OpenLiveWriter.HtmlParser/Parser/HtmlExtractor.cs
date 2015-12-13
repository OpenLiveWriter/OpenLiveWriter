// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OpenLiveWriter.HtmlParser.Parser
{
    /// <summary>
    /// Wrapper class for the HTML parser that makes it easy to
    /// search for and extract particular elements in the HTML.
    /// </summary>
    public class HtmlExtractor
    {
        private readonly string html;
        private SimpleHtmlParser parser;

        private Element lastMatch = null;

        public HtmlExtractor(Stream data) : this(data, Encoding.UTF8)
        {
        }

        public HtmlExtractor(Stream data, Encoding encoding)
        {
            using (StreamReader reader = new StreamReader(data, encoding))
                html = reader.ReadToEnd();
            this.parser = new SimpleHtmlParser(html);
        }

        public HtmlExtractor(string html)
        {
            this.html = html;
            this.parser = new SimpleHtmlParser(html);
        }

        /// <summary>
        /// Returns the underlying parser that the HtmlExtractor is wrapping.
        /// </summary>
        public SimpleHtmlParser Parser
        {
            get { return parser; }
        }

        /// <summary>
        /// Indicates whether the last match succeeded.
        /// </summary>
        public bool Success
        {
            get
            {
                return lastMatch != null;
            }
        }

        /// <summary>
        /// Gets the element that was last matched. If the last
        /// match failed, then returns null.
        /// </summary>
        public Element Element
        {
            get
            {
                return lastMatch;
            }
        }

        /// <summary>
        /// Reposition the extractor back to the beginning of the
        /// HTML.
        /// </summary>
        /// <returns>Returns this. This allows chaining together of calls,
        /// like this:
        ///
        /// if (ex.Seek(...).Success || ex.Reset().Seek(...).Success) { ... }
        /// </returns>
        public HtmlExtractor Reset()
        {
            lastMatch = null;
            parser = new SimpleHtmlParser(html);
            return this;
        }

        public HtmlExtractor Seek(IElementPredicate predicate)
        {
            lastMatch = null;

            SeekWithin(predicate, null);
            return this;
        }

        public HtmlExtractor SeekWithin(IElementPredicate predicate, IElementPredicate withinPredicate)
        {
            lastMatch = null;

            Element e;
            while (null != (e = parser.Next()))
            {
                if (predicate.IsMatch(e))
                {
                    lastMatch = e;
                    break;
                }
                if (withinPredicate != null && withinPredicate.IsMatch(e))
                    break;
            }

            return this;
        }

        /// <summary>
        /// Seeks forward from the current position for the criterion.
        ///
        /// If the seek fails, the parser will be positioned at the end of the file--all
        /// future seeks will also fail (until Reset() is called).
        /// </summary>
        /// <param name="criterion">
        /// Can be either a begin tag or end tag, or a run of text, or a comment.
        ///
        /// Examples of start tags:
        /// <a> (any anchor tag)
        /// <a name> (any anchor tag that has at least one "name" attribute (with or without value)
        /// <a name='title'> (any anchor tag that has a name attribute whose value is "title")
        ///
        /// Example of end tag:
        /// </a> (any end anchor tag)
        ///
        /// Examples of invalid criteria:
        /// <a></a> (only one criterion allowed per seek; chain Seek() calls if necessary)
        /// foo (only begin tags and end tags are allowed)
        ///
        /// TODO: Allow regular expression matching on attribute values, e.g. <a class=/^heading.*$/>
        /// </param>
        public HtmlExtractor Seek(string criterion)
        {
            lastMatch = null;

            SeekWithin(Parse(criterion), null);
            return this;
        }

        public HtmlExtractor SeekWithin(string criterion, string withinCriterion)
        {
            lastMatch = null;

            SeekWithin(Parse(criterion), Parse(withinCriterion));
            return this;
        }

        public HtmlExtractor MatchNext(IElementPredicate predicate, bool ignoreWhitespace)
        {
            lastMatch = null;

            Element e;
            do
            {
                e = parser.Next();
            }
            while (e != null && ignoreWhitespace && IsWhitespaceOrZeroLengthText(e));

            if (e != null && predicate.IsMatch(e))
                lastMatch = e;

            return this;
        }

        public HtmlExtractor MatchNext(string criterion)
        {
            return MatchNext(criterion, false);
        }

        public HtmlExtractor MatchNext(string criterion, bool ignoreWhitespace)
        {
            lastMatch = null;

            MatchNext(Parse(criterion), ignoreWhitespace);
            return this;
        }

        /// <summary>
        /// Does tag balancing.  Pass just the tag *name*, not
        /// an actual end tag.
        /// </summary>
        public string CollectTextUntil(string endTagName)
        {
            return HtmlUtils.HTMLToPlainText(parser.CollectHtmlUntil(endTagName));
            /*

                        string text = parser.CollectTextUntil(endTagName);
                        if (convertToPlainText && text != null)
                            return HtmlUtils.HTMLToPlainText(text);
                        else
                            return text;
            */
        }

        /// <summary>
        /// Does not do tag balancing.
        /// </summary>
        public string CollectTextUntilCriterion(string criterion)
        {
            return CollectTextUntilPredicate(Parse(criterion));
        }

        /// <summary>
        /// Does not do tag balancing.
        /// </summary>
        public string CollectTextUntilPredicate(IElementPredicate elementp)
        {
            return HtmlUtils.HTMLToPlainText(CollectHtmlUntilPredicate(elementp));
        }

        /// <summary>
        /// Does not do tag balancing.
        /// </summary>
        public string CollectHtmlUntilPredicate(IElementPredicate elementp)
        {
            StringBuilder result = new StringBuilder();
            Element el;
            while (null != (el = Next()))
            {
                if (elementp.IsMatch(el))
                    break;
                result.Append(html, el.Offset, el.Length);
            }
            return result.ToString();
        }

        public Element Next()
        {
            return Next(false);
        }

        public Element Next(bool ignoreWhitespace)
        {
            Element e;
            do
            {
                e = parser.Next();
            }
            while (e != null && ignoreWhitespace && IsWhitespaceOrZeroLengthText(e));
            return e;
        }

        internal static IElementPredicate Parse(string criterion)
        {
            SimpleHtmlParser parser = new SimpleHtmlParser(criterion);
            Element el = parser.Next();
            if (el == null)
            {
                Trace.Fail("Criterion was null");
                throw new ArgumentException("Criterion was null");
            }
            if (parser.Next() != null)
            {
                Trace.Fail("Too many criteria");
                throw new ArgumentException("Too many criteria");
            }

            if (el is BeginTag)
            {
                BeginTag tag = (BeginTag)el;

                if (tag.HasResidue || tag.Unterminated)
                {
                    Trace.Fail("Malformed criterion");
                    throw new ArgumentException("Malformed criterion");
                }

                RequiredAttribute[] attributes = new RequiredAttribute[tag.Attributes.Length];
                for (int i = 0; i < attributes.Length; i++)
                    attributes[i] = new RequiredAttribute(tag.Attributes[i].Name, tag.Attributes[i].Value);
                return new BeginTagPredicate(tag.Name, attributes);
            }
            else if (el is EndTag)
            {
                return new EndTagPredicate(((EndTag)el).Name);
            }
            else if (el is Text)
            {
                return new TextPredicate(el.RawText);
            }
            else if (el is Comment)
            {
                return new CommentPredicate(el.RawText);
            }
            else
            {
                Trace.Fail("Invalid criterion \"" + criterion + "\"");
                throw new ArgumentException("Invalid criterion \"" + criterion + "\"");
            }
        }

        private bool IsWhitespaceOrZeroLengthText(Element e)
        {
            if (!(e is Text))
                return false;
            int end = e.Offset + e.Length;
            for (int i = e.Offset; i < end; i++)
            {
                if (!char.IsWhiteSpace(html[i]))
                    return false;
            }
            return true;
        }
    }

    public interface IElementPredicate
    {
        bool IsMatch(Element e);
    }

    public class PredicatePair
    {
        private readonly IElementPredicate _match;
        private readonly IElementPredicate _stop;

        public PredicatePair(IElementPredicate match, IElementPredicate stop)
        {
            _match = match;
            _stop = stop;
        }

        public IElementPredicate Match
        {
            get
            {
                return _match;
            }
        }

        public IElementPredicate Stop
        {
            get
            {
                return _stop;
            }
        }

    }

    public class AndPredicate : IElementPredicate
    {
        private readonly IElementPredicate[] predicates;

        public AndPredicate(params IElementPredicate[] predicates)
        {
            this.predicates = predicates;
        }

        public bool IsMatch(Element e)
        {
            foreach (IElementPredicate predicate in predicates)
                if (!predicate.IsMatch(e))
                    return false;
            return true;
        }
    }

    public class OrPredicate : IElementPredicate
    {
        private readonly IElementPredicate[] predicates;

        public OrPredicate(params IElementPredicate[] predicates)
        {
            this.predicates = predicates;
        }

        public bool IsMatch(Element e)
        {
            foreach (IElementPredicate predicate in predicates)
                if (predicate.IsMatch(e))
                    return true;
            return false;
        }
    }

    public class EndTagPredicate : IElementPredicate
    {
        private readonly string tagName;

        public EndTagPredicate(string tagName)
        {
            this.tagName = tagName;
        }

        public bool IsMatch(Element e)
        {
            EndTag tag = e as EndTag;
            if (tag == null)
                return false;
            return tag.NameEquals(tagName);
        }
    }

    public class BeginTagPredicate : IElementPredicate
    {
        private string tagName;
        private RequiredAttribute[] attrs;

        public BeginTagPredicate(string tagName, params RequiredAttribute[] attrs)
        {
            this.tagName = tagName;
            this.attrs = attrs;
        }

        public bool IsMatch(Element e)
        {
            BeginTag tag = e as BeginTag;
            if (tag == null)
                return false;

            if (tagName != null && !tag.NameEquals(tagName))
                return false;

            foreach (RequiredAttribute reqAttr in attrs)
            {
                int foundAt;
                Attr attr = tag.GetAttribute(reqAttr.Name, true, 0, out foundAt);
                if (attr == null)
                    return false;
                if (reqAttr.Value != null && reqAttr.Value != attr.Value)
                    return false;
            }

            return true;
        }
    }

    public class TextPredicate : IElementPredicate
    {
        private readonly string textToMatch;

        public TextPredicate(string textToMatch)
        {
            this.textToMatch = textToMatch;
        }

        public bool IsMatch(Element e)
        {
            Text text = e as Text;
            if (text == null)
                return false;
            return text.RawText == textToMatch;
        }
    }

    public class CommentPredicate : IElementPredicate
    {
        private readonly string textToMatch;

        public CommentPredicate(string textToMatch)
        {
            this.textToMatch = textToMatch;
        }

        public bool IsMatch(Element e)
        {
            Comment comment = e as Comment;
            if (comment == null)
                return false;
            return comment.RawText == textToMatch;
        }
    }

    public class SmartPredicate : IElementPredicate
    {
        private readonly IElementPredicate actualPredicate;

        public SmartPredicate(string criterion)
        {
            actualPredicate = HtmlExtractor.Parse(criterion);
        }

        public bool IsMatch(Element e)
        {
            return actualPredicate.IsMatch(e);
        }
    }

    public class TypePredicate : IElementPredicate
    {
        private readonly Type type;
        private readonly bool allowSubtypes;

        public TypePredicate(Type type) : this(type, true)
        {
        }

        public TypePredicate(Type type, bool allowSubtypes)
        {
            this.type = type;
            this.allowSubtypes = allowSubtypes;
        }

        public bool IsMatch(Element e)
        {
            if (!allowSubtypes)
                return e.GetType().Equals(type);
            else
                return type.IsInstanceOfType(e);
        }
    }

    public class RequiredAttribute
    {
        private readonly string name;
        private readonly string value;

        public RequiredAttribute(string name) : this(name, null)
        {
        }

        public RequiredAttribute(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            get { return name; }
        }

        public string Value
        {
            get { return this.value; }
        }
    }
}
