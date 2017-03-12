// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for NewspaperItemCleanup.
    /// </summary>
    public class UnsafeHtmlFragmentHelper : LightWeightHTMLDocumentIterator
    {
        [Flags]
        public enum Flag : ulong
        {
            NoFlags = 0x000,
            RemoveScriptTags = 0x001,
            RemoveScriptAttributes = 0x002,
            RemovePartialTags = 0x004,
            RemoveStyles = 0x008,
            RemoveUnopenedCloseTags = 0x010,
            RemoveDocumentTags = 0x020,
            RemoveComments = 0x040,
            RemoveMarkupDirectives = 0x080,
            ForceCloseTags = 0x100,
            AllFlags = 0xffff
        }

        private Hashtable _individualTagStack = new Hashtable();
        private StringBuilder _htmlBuilder = new StringBuilder();
        private int suspendTagDepth = 0;
        private StringCollection _tagStack = new StringCollection();
        private ulong _flagMask;

        /// <summary>
        /// Summary description for LightWeightHTMLReplacer.
        /// </summary>
        private UnsafeHtmlFragmentHelper(string html, Flag flags) : base(html)
        {
            _flagMask = (ulong)flags;
        }

        /// <summary>
        /// Utility for sterilizing potentially harmful HTML that gets inserts into a newspaper.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string SterilizeHtml(string html)
        {
            return SterilizeHtml(html, Flag.AllFlags);
        }

        /// <summary>
        /// Utility for sterilizing potentially harmful HTML that gets inserts into a newspaper.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string SterilizeHtml(string html, Flag flags)
        {
            return new UnsafeHtmlFragmentHelper(html, flags).DoReplace();
        }

        private bool FlagNotSet(Flag flag)
        {
            return !FlagIsSet(flag);
        }

        private bool FlagIsSet(Flag flag)
        {
            ulong flagLong = (ulong)flag;
            ulong mask = _flagMask & flagLong;
            return mask == flagLong;
        }

        private string DoReplace()
        {
            Parse();
            string html = _htmlBuilder.ToString();
            _htmlBuilder = new StringBuilder();

            return html;
        }

        protected void Emit(string text)
        {
            if (suspendTagDepth == 0)
                _htmlBuilder.Append(text);
        }

        protected override void DefaultAction(Element el)
        {
            if (suspendTagDepth == 0)
                Emit(el.ToString());
        }

        protected override void OnBeginTag(BeginTag tag)
        {
            if (FlagIsSet(Flag.RemovePartialTags) && tag.Unterminated)
            {
                return;
            }

            //remove all illegal attributes from the tag
            foreach (Attr attr in tag.Attributes)
            {
                if (IsIllegalAttribute(attr))
                    attr.Value = string.Empty;
            }

            if (tag.NameEquals("script"))
                Debug.WriteLine("Script tag");
            if (IsRegexMatch(IllegalTagTreeName, tag.Name))
            {
                suspendTagDepth++;
            }
            else if (!IsIllegalTag(tag) && suspendTagDepth == 0)
            {
                PushStartTag(tag.Name);
                base.OnBeginTag(tag);
            }
        }

        protected override void OnEndTag(EndTag tag)
        {
            if (suspendTagDepth > 0)
            {
                if (IsRegexMatch(IllegalTagTreeName, tag.Name))
                    suspendTagDepth--;
            }
            else if (!IsRegexMatch(IllegalTagName, tag.Name))
            {
                try
                {
                    PopEndTag(tag.Name);
                    base.OnEndTag(tag);
                }
                catch (IllegalEndTagException)
                {
                    //there is no corresponding open tag on the stack, so don't close this one.
                    //This prevents an this HTML from messing up the HTML that surrounds it.
                    if (FlagNotSet(Flag.RemoveUnopenedCloseTags))
                        base.OnEndTag(tag);
                }
            }
        }

        protected override void OnScriptComment(ScriptComment scriptComment)
        {
            if (FlagNotSet(Flag.RemoveScriptTags))
                base.OnScriptComment(scriptComment);
        }

        protected override void OnDocumentBegin()
        {
            base.OnDocumentBegin();
        }

        protected override void OnDocumentEnd()
        {
            //loop over all open tags left in the stack and close them.
            //This will prevent HTML tags that weren't closed from being able to mess up the surrounding HTML
            if (FlagIsSet(Flag.ForceCloseTags))
            {
                for (int i = _tagStack.Count - 1; i >= 0; i--)
                {
                    string tagName = _tagStack[i];
                    if (IsRegexMatch(requiresEndTag, tagName))
                    {
                        Emit(String.Format(CultureInfo.InvariantCulture, "</{0}>", tagName));
                    }
                }
            }
            base.OnDocumentEnd();
        }

        protected override void OnStyleImport(StyleImport styleImport)
        {
            if (FlagNotSet(Flag.RemoveStyles))
                base.OnStyleImport(styleImport);
        }

        protected override void OnStyleText(StyleText text)
        {
            if (FlagNotSet(Flag.RemoveStyles))
                base.OnStyleText(text);
        }

        protected override void OnStyleLiteral(StyleLiteral literal)
        {
            if (FlagNotSet(Flag.RemoveStyles))
                base.OnStyleLiteral(literal);
        }

        protected override void OnScriptLiteral(ScriptLiteral literal)
        {
            if (FlagNotSet(Flag.RemoveScriptTags))
                base.OnScriptLiteral(literal);
        }

        protected override void OnComment(Comment comment)
        {
            if (FlagNotSet(Flag.RemoveComments))
                base.OnComment(comment);
        }

        protected override void OnMarkupDirective(MarkupDirective markupDirective)
        {
            if (FlagNotSet(Flag.RemoveMarkupDirectives))
                base.OnMarkupDirective(markupDirective);
        }

        protected override void OnScriptText(ScriptText scriptText)
        {
            if (FlagNotSet(Flag.RemoveScriptTags))
                base.OnScriptText(scriptText);
        }

        protected override void OnText(Text text)
        {
            if (FlagIsSet(Flag.RemovePartialTags))
            {
                string newText = badStartTag.Replace(text.RawText, "");
                text = new Text(newText, 0, newText.Length);
            }
            base.OnText(text);
        }

        protected override void OnStyleUrl(StyleUrl styleUrl)
        {
            if (FlagNotSet(Flag.RemoveStyles))
                base.OnStyleUrl(styleUrl);
        }

        private bool IsIllegalTag(BeginTag tag)
        {
            if (IsRegexMatch(IllegalTagName, tag.Name))
            {
                return true;
            }
            else if (FlagIsSet(Flag.RemoveStyles) && tag.NameEquals("link"))
            {
                //if this link element is a stylesheet, it is illegal
                Attr relAttr = tag.GetAttribute("rel");
                if (relAttr != null && relAttr.Value != null && relAttr.Value.ToUpperInvariant().Trim() == "STYLESHEET")
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsIllegalAttribute(Attr attribute)
        {
            if (IsRegexMatch(IllegalAttribute, attribute.Name))
            {
                return true;
            }

            if (FlagIsSet(Flag.RemoveScriptAttributes))
            {
                switch (attribute.Name.ToUpperInvariant())
                {
                    // These are all the attributes from the HTML DTD that have %URI values.
                    case "SRC":
                    case "HREF":
                    case "DATASRC":
                    case "BACKGROUND":
                    case "LONGDESC":
                    case "USEMAP":
                    case "CLASSID":
                    case "CODEBASE":
                    case "DATA":
                    case "CITE":
                    case "ACTION":
                    case "PROFILE":
                        return IsScript(attribute.Value);
                }
            }
            return false;
        }

        private static bool IsScript(string attrValue)
        {
            if (attrValue == null)
                return false;

            return HttpUtility
                .UrlDecode(attrValue.Trim())
                .StartsWith("javascript:", StringComparison.OrdinalIgnoreCase);
        }

        private void AppendRegexStringList(StringBuilder sb, string list)
        {
            if (sb.Length > 0)
                sb.Append("|");
            sb.Append(list);
        }

        private Regex CreateStringListRegex(string list)
        {
            if (list == null || list.Trim() == String.Empty)
                return null;

            string regexString = String.Format(CultureInfo.InvariantCulture, @"\b({0})\b", list);
            Regex regex = new Regex(regexString, RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            return regex;
        }

        private bool IsRegexMatch(Regex regex, string input)
        {
            if (regex == null)
                return false;
            else
            {

                bool match = regex.IsMatch(input);
                return match;
            }
        }

        static Regex badStartTag = new Regex(@"\<[a-z\!].*?(?:\>|$)", RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        static Regex requiresEndTag = new Regex(@"\b(a|b|blockquote|cite|code|div|dl|em|font|form|h[1-6]|i|ol|pre|select|span|strong|style|table|textarea|tt|ul)\b", RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        private Regex _llegalTagTreeName;
        private Regex IllegalTagTreeName
        {
            get
            {
                if (_llegalTagTreeName == null)
                {
                    StringBuilder sb = new StringBuilder();
                    if (FlagIsSet(Flag.RemoveDocumentTags))
                        AppendRegexStringList(sb, ILLEGAL_DOCUMENT_TAG_TREE_NAMES);

                    _llegalTagTreeName = CreateStringListRegex(sb.ToString());
                }
                return _llegalTagTreeName;
            }
        }
        private string ILLEGAL_DOCUMENT_TAG_TREE_NAMES = "head";

        private Regex _llegalTagName;
        private Regex IllegalTagName
        {
            get
            {
                if (_llegalTagName == null)
                {
                    StringBuilder sb = new StringBuilder();
                    if (FlagIsSet(Flag.RemoveScriptTags))
                        AppendRegexStringList(sb, ILLEGAL_SCRIPT_TAG_NAMES);
                    if (FlagIsSet(Flag.RemoveStyles))
                        AppendRegexStringList(sb, ILLEGAL_STYLE_TAG_NAMES);
                    if (FlagIsSet(Flag.RemoveDocumentTags))
                        AppendRegexStringList(sb, ILLEGAL_DOCUMENT_TAG_NAMES);

                    _llegalTagName = CreateStringListRegex(sb.ToString());
                }
                return _llegalTagName;
            }
        }
        private string ILLEGAL_SCRIPT_TAG_NAMES = "script";
        private string ILLEGAL_STYLE_TAG_NAMES = "style|font";
        private string ILLEGAL_DOCUMENT_TAG_NAMES = "body|html|base";

        private Regex _illegalAttribute;
        private Regex IllegalAttribute
        {
            get
            {
                if (_illegalAttribute == null)
                {
                    StringBuilder sb = new StringBuilder(ILLEGAL_ATTR_NAMES);
                    if (FlagIsSet(Flag.RemoveStyles))
                        AppendRegexStringList(sb, ILLEGAL_STYLE_ATTR_NAMES);
                    if (FlagIsSet(Flag.RemoveScriptAttributes))
                        AppendRegexStringList(sb, ILLEGAL_SCRIPT_ATTR_NAMES);

                    _illegalAttribute = CreateStringListRegex(sb.ToString());
                }
                return _illegalAttribute;
            }
        }
        private string ILLEGAL_ATTR_NAMES = "^(([^x][^m][^l].*|.?.?):.*)"; //Matches all strings with a colon that do not start with "xml"
        private string ILLEGAL_STYLE_ATTR_NAMES = "font|class|style|face";
        private string ILLEGAL_SCRIPT_ATTR_NAMES = "onload|onclick|onblur|onchange|onerror|onfocus|onmouseout|onmouseover|onreset|onsubmit|onselect|onunload";

        /// <summary>
        /// Pushes the specified tagname onto the stack, and returns the number of tags with the same name now on the stack.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns>the remaining number of tags with the same name on the stack</returns>
        private int PushStartTag(string tagName)
        {
            tagName = tagName.ToLower(CultureInfo.InvariantCulture);
            _tagStack.Add(tagName);
            object stackValue = _individualTagStack[tagName];
            int newStackDepth = stackValue == null ? 1 : ((int)stackValue) + 1;
            _individualTagStack[tagName] = newStackDepth;
            return newStackDepth;
        }

        /// <summary>
        /// Pops the specified tagname off of the stack, and returns the number of tags with the same name left on the stack.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns>the remaining number of tags with the same name on the stack</returns>
        private int PopEndTag(string tagName)
        {
            tagName = tagName.ToLower(CultureInfo.InvariantCulture);
            object stackValue = _individualTagStack[tagName];
            if (stackValue == null)
                throw new IllegalEndTagException(tagName);
            else
            {
                int stackDepth = (int)stackValue;
                if (stackDepth == 0)
                    throw new IllegalEndTagException(tagName);

                stackDepth--;
                _individualTagStack[tagName] = stackDepth;

                //search up the stack and remove this tag from the stack
                for (int i = _tagStack.Count - 1; i >= 0; i--)
                {
                    if (_tagStack[i] == tagName)
                    {
                        _tagStack.RemoveAt(i);
                        break;
                    }
                }

                return stackDepth;
            }
        }

        class IllegalEndTagException : Exception
        {
            public IllegalEndTagException(string tagName) : base("Cannot close unopened tag: " + tagName)
            { }
        }
    }
}
