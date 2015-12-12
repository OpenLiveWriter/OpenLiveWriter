// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices.HTML
{
    public class LightWeightHTMLThinner2
    {
        private const string TAG_PRE = "pre";
        private const string TAG_P = "p";
        private const string TAG_BR = "br";
        private const string TAG_HR = "hr";
        private const string TAG_IMG = "img";

        private static Hashtable _tagSpecs = new Hashtable();
        private static Hashtable _tagSpecsStrict = new Hashtable();
        static LightWeightHTMLThinner2()
        {
            #region tag map
            // Replace with <p>
            TagDesc P = new TagDesc("p", TagType.Block);
            TagDesc BR = new TagDesc("br", TagType.Empty);

            TagDesc BLOCK = new TagDesc(TagType.Block);
            TagDesc INLINE = new TagDesc(TagType.Inline);

            _tagSpecs.Add("p", BLOCK);
            _tagSpecs.Add("pre", BLOCK);

            // lists
            TagDesc LIST = new TagDesc(TagType.Block, "compact");
            _tagSpecs.Add("ol", new TagDesc(TagType.Block, "compact", "start"));
            _tagSpecs.Add("ul", LIST);
            _tagSpecs.Add("dir", LIST);
            _tagSpecs.Add("menu", LIST);
            _tagSpecs.Add("dl", LIST);
            _tagSpecs.Add("li", BLOCK);
            _tagSpecs.Add("dt", INLINE);
            _tagSpecs.Add("dd", BLOCK);

            // styles
            _tagSpecs.Add("tt", INLINE);
            _tagSpecs.Add("i", INLINE);
            _tagSpecs.Add("b", INLINE);
            _tagSpecs.Add("u", INLINE);
            _tagSpecs.Add("s", INLINE);
            _tagSpecs.Add("strike", INLINE);
            _tagSpecs.Add("big", INLINE);
            _tagSpecs.Add("small", INLINE);
            _tagSpecs.Add("em", INLINE);
            _tagSpecs.Add("strong", INLINE);
            _tagSpecs.Add("dfn", INLINE);
            _tagSpecs.Add("code", INLINE);
            _tagSpecs.Add("samp", INLINE);
            _tagSpecs.Add("kbd", INLINE);
            _tagSpecs.Add("var", INLINE);
            _tagSpecs.Add("cite", INLINE);
            _tagSpecs.Add("abbr", INLINE);
            _tagSpecs.Add("acronym", INLINE);
            _tagSpecs.Add("sub", INLINE);
            _tagSpecs.Add("sup", INLINE);

            _tagSpecs.Add("div", P);
            _tagSpecs.Add("center", P);

            // Heading tags get demoted a bit
            _tagSpecs.Add("h1", new TagDesc("h3", TagType.Block));
            _tagSpecs.Add("h2", new TagDesc("h4", TagType.Block));
            _tagSpecs.Add("h3", new TagDesc("h5", TagType.Block));
            _tagSpecs.Add("h4", new TagDesc("h6", TagType.Block));
            _tagSpecs.Add("h5", new TagDesc("h6", TagType.Block));
            _tagSpecs.Add("h6", new TagDesc("h6", TagType.Block));

            _tagSpecs.Add("a", new TagDesc(TagType.Inline, "href", "name"));

            _tagSpecs.Add("br", new TagDesc(TagType.Empty, "clear"));
            _tagSpecs.Add("img", new TagDesc(TagType.Empty, "src", "alt", "longdesc", "title", "border", "align", "height", "width", "hspace", "vspace"));
            _tagSpecs.Add("hr", new TagDesc(TagType.Empty, "align", "noshade", "size", "width"));

            _tagSpecs.Add("blockquote",
                                     new TagDesc(TagType.Block, "cite"));
            _tagSpecs.Add("q", new TagDesc(TagType.Inline, "cite"));
            _tagSpecs.Add("ins", new TagDesc(TagType.Inline, "cite", "datetime"));
            _tagSpecs.Add("del", new TagDesc(TagType.Inline, "cite", "datetime"));

            _tagSpecs.Add("tr", new TagDesc("p", TagType.Empty));
            _tagSpecs.Add("th", new TagDesc("br", TagType.Empty));
            _tagSpecs.Add("td", new TagDesc("br", TagType.Empty));

            #endregion

            #region tag map strict

            _tagSpecsStrict.Add("a", new TagDesc(TagType.Inline, "href", "name"));
            _tagSpecsStrict.Add("img", new TagDesc(TagType.Empty, "src", "alt", "longdesc", "title", "height", "width"));

            _tagSpecsStrict.Add("p", BLOCK);
            _tagSpecsStrict.Add("pre", BLOCK);

            _tagSpecsStrict.Add("ol", P);
            _tagSpecsStrict.Add("ul", P);
            _tagSpecsStrict.Add("dir", P);
            _tagSpecsStrict.Add("menu", P);
            _tagSpecsStrict.Add("dl", BR);
            _tagSpecsStrict.Add("li", BR);
            _tagSpecsStrict.Add("dt", BR);

            _tagSpecsStrict.Add("div", P);
            _tagSpecsStrict.Add("center", P);

            _tagSpecsStrict.Add("h1", P);
            _tagSpecsStrict.Add("h2", P);
            _tagSpecsStrict.Add("h3", P);
            _tagSpecsStrict.Add("h4", P);
            _tagSpecsStrict.Add("h5", P);
            _tagSpecsStrict.Add("h6", P);

            _tagSpecsStrict.Add("br", BR);

            _tagSpecsStrict.Add("blockquote", P);

            _tagSpecsStrict.Add("tr", P);
            _tagSpecsStrict.Add("th", BR);
            _tagSpecsStrict.Add("td", BR);

            #endregion
        }

        public static string Thin(string html, bool preserveImages)
        {
            return Thin(html, preserveImages, false);
        }

        public static string Thin(string html, bool preserveImages, bool strict)
        {
            return new LightWeightHTMLThinner2().ThinInternal(html, preserveImages, strict);
        }

        public static string Thin(string html, bool preserveImages, bool strict, params ModifyReplacement[] modifyReplacements)
        {
            return new LightWeightHTMLThinner2().ThinInternal(html, preserveImages, strict, modifyReplacements);
        }

        public delegate void ModifyReplacement(Hashtable replacements);

        public static void PreserveTables(Hashtable replacements)
        {
            replacements["table"] = new TagDesc(TagType.Block, "border", "cellpadding", "cellspacing");
            replacements["tr"] = new TagDesc(TagType.Block);
            replacements["td"] = new TagDesc(TagType.Block, "width", "valign");
            replacements["th"] = new TagDesc(TagType.Block);
        }

        private string ThinInternal(string html, bool preserveImages, bool strict, params ModifyReplacement[] modifyReplacements)
        {
            Hashtable replacements = _tagSpecs;
            if (strict)
            {
                replacements = _tagSpecsStrict;
            }

            if (modifyReplacements != null)
            {
                replacements = (Hashtable)replacements.Clone();
                foreach (ModifyReplacement modifyReplacement in modifyReplacements)
                    modifyReplacement(replacements);
            }

            // Will hold the results of the leading whitespace buffer.
            // This buffer may or may not make it into the final result,
            // depending on whether any block-level tags are present.
            StringBuilder leadingOutput = new StringBuilder(10);
            // Will hold the results of everything else.
            StringBuilder mainOutput = new StringBuilder(html.Length);

            // references whichever output buffer is current.
            StringBuilder output = leadingOutput;

            SimpleHtmlParser parser = new SimpleHtmlParser(html);
            Element el;

            bool preserveWhitespace = false;  // <pre> blocks should preserve whitespace
            WhitespaceBuffer whitespaceBuffer = new WhitespaceBuffer();
            whitespaceBuffer.Promote(WhitespaceClass.Paragraph);  // Insert an implicit <p> unless the first non-whitespace element is a block
            bool hasBlock = false;

            while (null != (el = parser.Next()))
            {
                if (el is Tag)
                {
                    Tag t = (Tag)el;
                    string lowerName = t.Name.ToLower(CultureInfo.InvariantCulture);

                    TagDesc desc = (TagDesc)replacements[lowerName];
                    // if this tag is not in the table, drop it
                    if (desc == null)
                        continue;

                    // Replace tag with substitute tag if necessary (e.g. <DIV> becomes <P>)
                    string tagName = desc.Substitute;
                    if (tagName == null)
                        tagName = lowerName;

                    // special case for images
                    if (!preserveImages && tagName == TAG_IMG)
                        continue;

                    bool beginTag = el is BeginTag;

                    ElementClass elClass = WhitespaceBuffer.ClassifyTag(tagName, desc.TagType);
                    hasBlock |= (elClass == ElementClass.Block || elClass == ElementClass.Paragraph || elClass == ElementClass.Break);
                    if (!preserveWhitespace && WhitespaceBuffer.ProcessElementClass(ref whitespaceBuffer, output, elClass, true))
                        continue;

                    output = mainOutput;

                    if (beginTag)
                    {
                        WriteBeginTag(desc, tagName, ((BeginTag)el).Attributes, output);
                        if (tagName == TAG_PRE)
                            preserveWhitespace = true;
                    }
                    else if (el is EndTag)
                    {
                        if (!((EndTag)el).Implicit && desc.TagType != TagType.Empty)
                        {
                            output.Append(string.Format(CultureInfo.InvariantCulture, "</{0}>", tagName));
                        }
                        if (tagName == TAG_PRE)
                            preserveWhitespace = false;
                    }
                }
                else if (el is Text)
                {
                    string text = el.RawText;
                    text = HtmlUtils.EscapeEntities(HtmlUtils.UnEscapeEntities(text, HtmlUtils.UnEscapeMode.NonMarkupText));

                    if (!preserveWhitespace && WhitespaceBuffer.ProcessElementClass(ref whitespaceBuffer, output, WhitespaceBuffer.ClassifyText(text), false))
                        continue;

                    output = mainOutput;

                    output.Append(text);
                }
            }

            if (hasBlock && ReferenceEquals(mainOutput, output))
                output.Insert(0, leadingOutput.ToString());

            // The whitespace buffer may not be empty at this point.  That's OK--we want to drop trailing whitespace

            return output.ToString();
        }

        /// <summary>
        /// Write a begin tag to the StringBuilder.  Attributes will be
        /// filtered according to the provided TagDesc, if any.
        /// </summary>
        private void WriteBeginTag(TagDesc desc, string tagName, Attr[] attributes, StringBuilder output)
        {
            output.Append("<" + tagName);
            for (int i = 0; i < attributes.Length; i++)
            {
                Attr attr = attributes[i];
                if (attr != null && (desc == null || desc.AttributeAllowed(attr.Name)))
                {
                    output.Append(" ");
                    output.Append(attributes[i].Name);
                    string val = attributes[i].Value;
                    if (val != null)
                    {
                        // hack for Word 2007 footnotes.
                        if (tagName == "a")
                        {
                            if (attributes[i].NameEquals("name"))
                            {
                                // Do a fast check before doing an expensive one
                                if (val.StartsWith("_ftn", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(val, @"^_ftn(ref)?(\d+)$", RegexOptions.IgnoreCase))
                                    val += WordFootnoteAnchorSuffix;
                            }
                            else if (attributes[i].NameEquals("href"))
                            {
                                // Do a fast check before doing an expensive one
                                if (val.StartsWith("#_ftn", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(val, @"^#_ftn(ref)?(\d+)$", RegexOptions.IgnoreCase))
                                    val += WordFootnoteAnchorSuffix;
                            }
                        }

                        output.Append("=\"" + HtmlUtils.EscapeEntities(val) + "\"");
                    }
                }
            }
            output.Append(">");
        }

        /// <summary>
        /// Disambiguates footnote anchors
        /// </summary>
        private string WordFootnoteAnchorSuffix
        {
            get
            {
                if (_wordFootnoteAnchorSuffix == null)
                    _wordFootnoteAnchorSuffix = "_" + new Random().Next(1000, 9999).ToString(CultureInfo.InvariantCulture);
                return _wordFootnoteAnchorSuffix;
            }
        }
        private string _wordFootnoteAnchorSuffix;

        public enum WhitespaceClass { None, Space, Break, Paragraph }
        public enum ElementClass { Space = WhitespaceClass.Space, Break, Paragraph, NotBlock, Block }

        private class WhitespaceBuffer
        {
            private WhitespaceClass wsclass = WhitespaceClass.None;
            private bool frozen = false;

            public void Promote(WhitespaceClass newClass)
            {
                if (frozen)
                    return;
                wsclass = (WhitespaceClass)Math.Max((int)wsclass, (int)newClass);
            }

            /// <summary>
            /// Return the current state of the WhitespaceBuffer, or not,
            /// depending on whether the element that is causing the whitespace
            /// to be rendered (i.e. the non-whitespace element) is a block
            /// or not.
            /// </summary>
            public string Render(ElementClass forcingElement)
            {
                if (forcingElement == ElementClass.Block)
                    return string.Empty;

                switch (wsclass)
                {
                    case WhitespaceClass.None:
                        return string.Empty;
                    case WhitespaceClass.Space:
                        return " ";
                    case WhitespaceClass.Break:
                        return "<" + TAG_BR + ">";
                    case WhitespaceClass.Paragraph:
                        return "<" + TAG_P + ">";
                    default:
                        Debug.Fail("Unexpected whitespace class " + wsclass.ToString());
                        return string.Empty;
                }
            }

            public static ElementClass ClassifyText(string text)
            {
                if (Regex.IsMatch(text, @"^(\s|&nbsp;)*$", RegexOptions.ExplicitCapture))
                    return ElementClass.Space;
                else
                    return ElementClass.NotBlock;
            }

            public static ElementClass ClassifyTag(string tagName, TagType tagType)
            {
                if (tagName.Equals(TAG_P))
                    return ElementClass.Paragraph;
                else if (tagName.Equals(TAG_BR))
                    return ElementClass.Break;
                else if (tagName.Equals(TAG_HR))
                    return ElementClass.Block;
                else
                    return tagType == TagType.Block ? ElementClass.Block : ElementClass.NotBlock;
            }

            /// <summary>
            /// Examines the given ElementClass and modifies the internal WhitespaceBuffer state
            /// and output buffer accordingly.  Returns true if the given ElementClass was whitespace
            /// and false if not; in the former case, the corresponding element should not be added
            /// to the output buffer.
            /// </summary>
            public static bool ProcessElementClass(ref WhitespaceBuffer whitespace, StringBuilder output, ElementClass elclass, bool isBeginTag)
            {
                switch (elclass)
                {
                    case ElementClass.Paragraph:
                    case ElementClass.Break:
                    case ElementClass.Space:
                        if (whitespace == null)
                            whitespace = new WhitespaceBuffer();
                        whitespace.Promote((WhitespaceClass)elclass);
                        return true;
                    case ElementClass.Block:
                    case ElementClass.NotBlock:
                        if (whitespace != null)
                        {
                            output.Append(whitespace.Render(elclass));
                            whitespace = null;
                        }
                        return false;
                    default:
                        Trace.Fail("Unknown element class " + elclass.ToString());
                        return false;
                }
            }
        }

        public enum TagType
        {
            Block,
            Inline,
            Empty
        }

        public class TagDesc
        {
            private readonly string _substitute;
            private readonly TagType _tagType;
            private readonly HashSet _allowedAttributes;

            public TagDesc(TagType tagType, params string[] allowedAttributes) : this(null, tagType, allowedAttributes)
            {
            }

            public TagDesc(string substitute, TagType tagType, params string[] allowedAttributes)
            {
                _substitute = substitute;
                _tagType = tagType;
                if (allowedAttributes != null && allowedAttributes.Length > 0)
                {
                    _allowedAttributes = new HashSet();
                    foreach (string attr in allowedAttributes)
                        _allowedAttributes.Add(attr.ToUpperInvariant());
                }
            }

            public string Substitute
            {
                get { return _substitute; }
            }

            public TagType TagType
            {
                get { return _tagType; }
            }

            public bool AttributeAllowed(string attr)
            {
                if (_allowedAttributes == null)
                    return false;
                return _allowedAttributes.Contains(attr.ToUpperInvariant());
            }
        }
    }
}
