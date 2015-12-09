// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Globalization;
using mshtml;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Utility class for implementing IHTMLElementFilter methods (useful for grabbing elements out of MarkupRanges).
    /// </summary>
    public class ElementFilters
    {
        private ElementFilters()
        {
            //no instances
        }

        public static IHTMLElementFilter CreateTagIdFilter(string tagId)
        {
            return new IHTMLElementFilter(new TagIdElementFilter(tagId).Filter);
        }

        public static IHTMLElementFilter CreateIdFilter(string id)
        {
            return new IHTMLElementFilter(new IdElementFilter(id).Filter);
        }

        public static IHTMLElementFilter CreateEqualFilter(IHTMLElement element)
        {
            return new IHTMLElementFilter(new EqualElementFilter(element).Filter);
        }

        public static IHTMLElementFilter CreateControlElementFilter()
        {
            return new IHTMLElementFilter(new ControlElementFilter().Filter);
        }

        public static IHTMLElementFilter CreateClassFilter(string className)
        {
            return new IHTMLElementFilter(new ClassElementFilter(className).Filter);
        }

        public static IHTMLElementFilter CreateElementNameFilter(string name)
        {
            return new IHTMLElementFilter(new ElementNameFilter(name).Filter);
        }

        public static IHTMLElementFilter CreateElementAttributeFilter(string attributeName)
        {
            return new IHTMLElementFilter(new ElementAttributeFilter(attributeName).Filter);
        }

        public static IHTMLElementFilter CreateElementBackgroundColorInlineStyleFilter()
        {
            return new IHTMLElementFilter(new ElementBackgroundColorInlineStyleFilter().Filter);
        }

        public static IHTMLElementFilter CreateElementEqualsFilter(IHTMLElement e)
        {
            return new IHTMLElementFilter(new ElementEqualsFilter(e).Filter);
        }

        public static IHTMLElementFilter CreateElementPassFilter()
        {
            return new IHTMLElementFilter(new ElementPassFilter().Filter);
        }

        public static IHTMLElementFilter CreateCompoundElementFilter(params IHTMLElementFilter[] filters)
        {
            return new IHTMLElementFilter(new CompoundElementFilter(filters).MergeElementFilters);
        }

        private class TagIdElementFilter
        {
            private string _tagId;
            public TagIdElementFilter(string tagId)
            {
                _tagId = tagId;
            }

            public bool Filter(IHTMLElement e)
            {
                return String.Compare(e.tagName, _tagId, StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

        private class IdElementFilter
        {
            private string _id;
            public IdElementFilter(string id)
            {
                _id = id;
            }

            public bool Filter(IHTMLElement e)
            {
                return e.id == _id;
            }
        }

        private class ElementPassFilter
        {
            public bool Filter(IHTMLElement e)
            {
                return true;
            }
        }

        private class EqualElementFilter
        {
            private IHTMLElement _element;
            public EqualElementFilter(IHTMLElement element)
            {
                _element = element;
            }

            public bool Filter(IHTMLElement e)
            {
                return HTMLElementHelper.ElementsAreEqual(e, _element);
            }
        }

        private class ControlElementFilter
        {
            public bool Filter(IHTMLElement e)
            {
                return (e as IHTMLControlElement) != null;
            }
        }

        private class ClassElementFilter
        {
            private string _className;
            public ClassElementFilter(string className)
            {
                _className = className;
            }

            public bool Filter(IHTMLElement e)
            {
                return e.className == _className;
            }
        }

        private class ElementNameFilter
        {
            private string _name;
            public ElementNameFilter(string name)
            {
                _name = name;
            }

            public bool Filter(IHTMLElement e)
            {
                return e.tagName.Equals(_name, StringComparison.OrdinalIgnoreCase);
            }
        }

        private class ElementAttributeFilter
        {
            private string _attributeName;
            public ElementAttributeFilter(string attributeName)
            {
                _attributeName = attributeName.ToUpper(CultureInfo.InvariantCulture);
            }

            public bool Filter(IHTMLElement e)
            {
                string attributeName = e.getAttribute(_attributeName, 2) as string;
                return !String.IsNullOrEmpty(attributeName);
            }
        }

        private class ElementBackgroundColorInlineStyleFilter
        {
            public ElementBackgroundColorInlineStyleFilter()
            {
            }

            public bool Filter(IHTMLElement e)
            {
                return !String.IsNullOrEmpty((string)e.style.backgroundColor);
            }
        }

        private class ElementEqualsFilter
        {
            private IHTMLElement _element;
            public ElementEqualsFilter(IHTMLElement e)
            {
                _element = e;
            }

            public bool Filter(IHTMLElement e)
            {
                return e.sourceIndex == _element.sourceIndex;
            }
        }

        private class CompoundElementFilter
        {
            private IHTMLElementFilter[] _filters;
            public CompoundElementFilter(IHTMLElementFilter[] filters)
            {
                _filters = filters;
            }
            public bool MergeElementFilters(IHTMLElement e)
            {
                foreach (IHTMLElementFilter filter in _filters)
                {
                    if (filter(e))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true if this is an element that triggers a paragraph break.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsTableElement(IHTMLElement e)
        {
            if (TableTagNames[e.tagName] != null)
            {
                return true;
            }
            return false;
        }
        public static IHTMLElementFilter TABLE_ELEMENTS = new IHTMLElementFilter(IsTableElement);

        /// <summary>
        /// Returns true if this element is the body element.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsBodyElement(IHTMLElement e)
        {
            return (e as IHTMLBodyElement) != null;
        }
        public static IHTMLElementFilter BODY_ELEMENT = new IHTMLElementFilter(IsBodyElement);

        /// <summary>
        /// Returns true if this is an element that triggers a paragraph break.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsBlockElement(IHTMLElement e)
        {
            if (BlockTagNames[e.tagName] != null)
            {
                return true;
            }
            return false;
        }
        public static IHTMLElementFilter BLOCK_ELEMENTS = new IHTMLElementFilter(IsBlockElement);

        /// <summary>
        /// Returns true if this is one of the header element
        /// </summary>
        public static bool IsHeaderElement(IHTMLElement e)
        {
            if (HeaderTagNames[e.tagName] != null)
            {
                return true;
            }
            return false;
        }
        public static IHTMLElementFilter HEADER_ELEMENTS = new IHTMLElementFilter(IsHeaderElement);

        public static bool IsBlockOrTableElement(IHTMLElement e)
        {
            return IsBlockElement(e) || IsTableElement(e);
        }

        public static bool IsBlockOrTableCellElement(IHTMLElement e)
        {
            return ((BlockTagNames[e.tagName] != null) || IsTableCellElement(e));
        }
        public static IHTMLElementFilter BLOCK_OR_TABLE_CELL_ELEMENTS = new IHTMLElementFilter(IsBlockOrTableCellElement);

        public static bool IsBlockOrTableCellOrBodyElement(IHTMLElement e)
        {
            return ((BlockTagNames[e.tagName] != null) || IsTableCellElement(e)) || e is IHTMLBodyElement;
        }

        public static bool IsBlockQuoteElement(IHTMLElement e)
        {
            return e.tagName.ToUpperInvariant() == "BLOCKQUOTE";
        }
        public static IHTMLElementFilter BLOCKQUOTE_ELEMENT = new IHTMLElementFilter(IsBlockQuoteElement);

        public static bool IsTableCellElement(IHTMLElement e)
        {
            return (e is IHTMLTableCell);
        }
        public static IHTMLElementFilter TABLE_CELL_ELEMENT = new IHTMLElementFilter(IsTableCellElement);

        public static bool IsInlineElement(IHTMLElement e)
        {
            if (InlineTagNames[e.tagName] != null)
            {
                return true;
            }
            return false;
        }
        public static IHTMLElementFilter INLINE_ELEMENTS = new IHTMLElementFilter(IsInlineElement);

        public static bool IsAnchorElement(IHTMLElement e)
        {
            return e.tagName.Equals("A");
        }
        public static IHTMLElementFilter ANCHOR_ELEMENTS = new IHTMLElementFilter(IsAnchorElement);

        public static bool IsParagraphElement(IHTMLElement e)
        {
            return e.tagName.Equals("P");
        }
        public static IHTMLElementFilter PARAGRAPH_ELEMENTS = new IHTMLElementFilter(IsParagraphElement);

        public static bool IsLTRElement(IHTMLElement e)
        {
            return IsDirElement(e, "ltr");
        }

        public static bool IsRTLElement(IHTMLElement e)
        {
            return IsDirElement(e, "rtl");
        }

        private static bool IsDirElement(IHTMLElement e, string direction)
        {
            if (IsBlockOrTableCellElement(e))
            {
                IHTMLElement2 e2 = (IHTMLElement2)e;
                string dir = e2.currentStyle.direction;
                if (null != dir)
                    return String.Compare(dir, direction, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return false;
        }

        public static bool IsUnorderedListElement(IHTMLElement e)
        {
            return e.tagName.Equals("UL");
        }
        public static IHTMLElementFilter UNORDERED_LIST_ELEMENTS = new IHTMLElementFilter(IsUnorderedListElement);

        public static bool IsListElement(IHTMLElement e)
        {
            return IsUnorderedListElement(e) || IsOrderedListElement(e);
        }
        public static IHTMLElementFilter LIST_ELEMENTS = new IHTMLElementFilter(IsListElement);

        public static bool IsListItemElement(IHTMLElement e)
        {
            return e.tagName.Equals("LI");
        }
        public static IHTMLElementFilter LIST_ITEM_ELEMENTS = new IHTMLElementFilter(IsListItemElement);

        public static bool IsOrderedListElement(IHTMLElement e)
        {
            return e.tagName.Equals("OL");
        }
        public static IHTMLElementFilter ORDERED_LIST_ELEMENTS = new IHTMLElementFilter(IsOrderedListElement);

        public static bool IsImageElement(IHTMLElement e)
        {
            return e.tagName.Equals("IMG");
        }
        public static IHTMLElementFilter IMAGE_ELEMENTS = new IHTMLElementFilter(IsImageElement);

        /// <summary>
        /// Returns true if the specified element triggers something to be visible in the document
        /// when it contains no text.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsVisibleEmptyElement(IHTMLElement e)
        {
            return e.tagName.Equals("HR") || e.tagName.Equals("IMG") || e.tagName.Equals("EMBED") ||
                   e.tagName.Equals("OBJECT") || IsBlockElement(e) || IsTableElement(e);
        }
        public static IHTMLElementFilter VISIBLE_EMPTY_ELEMENTS = new IHTMLElementFilter(IsVisibleEmptyElement);

        public static bool RequiresEndTag(string tagName)
        {
            return NoEndTagRequired[tagName.ToUpper(CultureInfo.InvariantCulture)] == null;
        }
        /// <summary>
        /// Returns true if the specified tag needs an end tag in its text representation.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool RequiresEndTag(IHTMLElement e)
        {
            return NoEndTagRequired[e.tagName] == null && !(e is IHTMLUnknownElement);
        }
        public static IHTMLElementFilter END_TAG_REQUIRED = new IHTMLElementFilter(RequiresEndTag);

        /// <summary>
        /// Returns true if this element is supposed to have an end tag, but IE will still render it correctly
        /// without one.
        /// </summary>
        public static bool IsEndTagOptional(IHTMLElement e)
        {
            if (EndTagOptionalNames[e.tagName] != null)
            {
                return true;
            }
            return false;
        }
        public static IHTMLElementFilter END_TAG_OPTIONAL_ELEMENTS = new IHTMLElementFilter(IsEndTagOptional);

        /// <summary>
        /// Look up table holding the names of all block-formatted tags.
        /// </summary>
        private static Hashtable TableTagNames
        {
            get
            {
                if (_tableTagNames == null)
                {
                    _tableTagNames = new Hashtable();
                    _tableTagNames["TABLE"] = "TABLE";
                    _tableTagNames["TR"] = "TR";
                    _tableTagNames["TD"] = "TD";
                    _tableTagNames["TH"] = "TH";
                    _tableTagNames["CAPTION"] = "CAPTION";
                    _tableTagNames["COL"] = "COL";
                    _tableTagNames["COLGROUP"] = "COLGROUP";
                    _tableTagNames["THEAD"] = "THEAD";
                    _tableTagNames["TBODY"] = "TBODY";
                    _tableTagNames["TFOOT"] = "TFOOT";
                }
                return _tableTagNames;
            }
        }
        private static Hashtable _tableTagNames;

        /// <summary>
        /// Look up table holding the names of all block-formatted tags.
        /// </summary>
        private static Hashtable BlockTagNames
        {
            get
            {
                if (_blockTagNames == null)
                {
                    _blockTagNames = new Hashtable();
                    _blockTagNames["DIV"] = "DIV";
                    _blockTagNames["BODY"] = "BODY";
                    _blockTagNames["P"] = "P";
                    _blockTagNames["PRE"] = "PRE";
                    _blockTagNames["BR"] = "BR";
                    _blockTagNames["H1"] = "H1";
                    _blockTagNames["H2"] = "H2";
                    _blockTagNames["H3"] = "H3";
                    _blockTagNames["H4"] = "H4";
                    _blockTagNames["H5"] = "H5";
                    _blockTagNames["H6"] = "H6";
                    _blockTagNames["HR"] = "HR";
                    _blockTagNames["BLOCKQUOTE"] = "BLOCKQUOTE";
                    //_blockTagNames["TABLE"] = "TABLE";
                    //_blockTagNames["TR"] = "TR";
                    //_blockTagNames["TD"] = "TD";
                    //_blockTagNames["TH"] = "TH";
                    //_blockTagNames["UL"] = "UL";
                    //_blockTagNames["OL"] = "OL";
                    _blockTagNames["LI"] = "LI";
                }
                return _blockTagNames;
            }
        }
        private static Hashtable _blockTagNames;

        /// <summary>
        /// Look up table holding the names of all tags displayed "inline" (they don't force line breaks).
        /// </summary>
        private static Hashtable InlineTagNames
        {
            get
            {
                if (_inlineTagNames == null)
                {
                    _inlineTagNames = new Hashtable();
                    _inlineTagNames["A"] = "A";
                    _inlineTagNames["ABBR"] = "ABBR";
                    _inlineTagNames["ACRONYM"] = "ACRONYM";
                    _inlineTagNames["APPLET"] = "APPLET";
                    _inlineTagNames["B"] = "B";
                    _inlineTagNames["BASEFONT"] = "BASEFONT";
                    _inlineTagNames["BDO"] = "BDO";
                    _inlineTagNames["BIG"] = "BIG";
                    _inlineTagNames["BUTTON"] = "BUTTON";
                    _inlineTagNames["CITE"] = "CITE";
                    _inlineTagNames["CODE"] = "CODE";
                    _inlineTagNames["DEL"] = "DEL";
                    _inlineTagNames["DFN"] = "DFN";
                    _inlineTagNames["EM"] = "EM";
                    _inlineTagNames["FONT"] = "FONT";
                    _inlineTagNames["I"] = "I";
                    _inlineTagNames["IFRAME"] = "IFRAME";
                    _inlineTagNames["IMG"] = "IMG";
                    _inlineTagNames["INPUT"] = "INPUT";
                    _inlineTagNames["INS"] = "INS";
                    _inlineTagNames["KBD"] = "KBD";
                    _inlineTagNames["LABEL"] = "LABEL";
                    _inlineTagNames["MAP"] = "MAP";
                    _inlineTagNames["OBJECT"] = "OBJECT";
                    _inlineTagNames["Q"] = "Q";
                    _inlineTagNames["S"] = "S";
                    _inlineTagNames["SAMP"] = "SAMP";
                    _inlineTagNames["SCRIPT"] = "SCRIPT";
                    _inlineTagNames["SELECT"] = "SELECT";
                    _inlineTagNames["SMALL"] = "SMALL";
                    _inlineTagNames["SPAN"] = "SPAN";
                    _inlineTagNames["STRONG"] = "STRONG";
                    _inlineTagNames["STRIKE"] = "STRIKE";
                    _inlineTagNames["STYLE"] = "STYLE";
                    _inlineTagNames["SUB"] = "SUB";
                    _inlineTagNames["SUP"] = "SUP";
                    _inlineTagNames["TT"] = "TT";
                    _inlineTagNames["TEXTAREA"] = "TEXTAREA";
                    _inlineTagNames["U"] = "U";
                    _inlineTagNames["VAR"] = "VAR";
                }
                return _inlineTagNames;
            }
        }
        private static Hashtable _inlineTagNames;

        /// <summary>
        /// Look up table holding the names of all header tags.
        /// </summary>
        private static Hashtable HeaderTagNames
        {
            get
            {
                if (_headerTagNames == null)
                {
                    _headerTagNames = new Hashtable();
                    _headerTagNames["H1"] = "H1";
                    _headerTagNames["H2"] = "H2";
                    _headerTagNames["H3"] = "H3";
                    _headerTagNames["H4"] = "H4";
                    _headerTagNames["H5"] = "H5";
                    _headerTagNames["H6"] = "H6";
                }
                return _headerTagNames;
            }
        }
        private static Hashtable _headerTagNames;

        /// <summary>
        /// Look up table holding the names of all tags that render correctly in IE even if they don't have a
        /// corresponding end tag.
        /// </summary>
        private static Hashtable EndTagOptionalNames
        {
            get
            {
                if (_endTagOptionalNames == null)
                {
                    _endTagOptionalNames = new Hashtable();
                    _endTagOptionalNames["P"] = "P";
                    _endTagOptionalNames["LI"] = "LI";
                    _endTagOptionalNames["H1"] = "H1";
                    _endTagOptionalNames["H2"] = "H2";
                    _endTagOptionalNames["H3"] = "H3";
                    _endTagOptionalNames["H4"] = "H4";
                    _endTagOptionalNames["H5"] = "H5";
                    _endTagOptionalNames["H6"] = "H6";
                    _endTagOptionalNames["TR"] = "TR";
                    _endTagOptionalNames["TD"] = "TD";
                    _endTagOptionalNames["TH"] = "TH";
                    _endTagOptionalNames["COLGROUP"] = "COLGROUP";
                    _endTagOptionalNames["THEAD"] = "THEAD";
                    _endTagOptionalNames["TBODY"] = "TBODY";
                    _endTagOptionalNames["TFOOT"] = "TFOOT";
                    _endTagOptionalNames["OPTION"] = "OPTION";
                    _endTagOptionalNames["DT"] = "DT";
                    _endTagOptionalNames["DD"] = "DD";
                }
                return _endTagOptionalNames;
            }
        }
        private static Hashtable _endTagOptionalNames;

        /// <summary>
        /// A table of tags that require don't support end tags.
        /// This operation useful for testing elements that don't need explicit end tags in XHTML.
        /// </summary>
        private static Hashtable NoEndTagRequired
        {
            get
            {
                if (m_noEndTagRequired == null)
                {
                    m_noEndTagRequired = new Hashtable();
                    m_noEndTagRequired.Add(HTMLTokens.Area, HTMLTokens.Area);
                    m_noEndTagRequired.Add(HTMLTokens.Base, HTMLTokens.Base);
                    m_noEndTagRequired.Add(HTMLTokens.BaseFont, HTMLTokens.BaseFont);
                    m_noEndTagRequired.Add(HTMLTokens.Br, HTMLTokens.Br);
                    m_noEndTagRequired.Add(HTMLTokens.Col, HTMLTokens.Col);
                    m_noEndTagRequired.Add(HTMLTokens.Embed, HTMLTokens.Embed);
                    m_noEndTagRequired.Add(HTMLTokens.Hr, HTMLTokens.Hr);
                    m_noEndTagRequired.Add(HTMLTokens.Img, HTMLTokens.Img);
                    m_noEndTagRequired.Add(HTMLTokens.Input, HTMLTokens.Input);
                    m_noEndTagRequired.Add(HTMLTokens.Isindex, HTMLTokens.Isindex);
                    m_noEndTagRequired.Add(HTMLTokens.Link, HTMLTokens.Link);
                    m_noEndTagRequired.Add(HTMLTokens.Meta, HTMLTokens.Meta);
                    m_noEndTagRequired.Add(HTMLTokens.Param, HTMLTokens.Param);
                }
                return m_noEndTagRequired;
            }
        }
        private static Hashtable m_noEndTagRequired;
    }
}
