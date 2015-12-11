// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;

namespace OpenLiveWriter.HtmlParser.Parser
{
    public abstract class Element
    {
        private readonly string data;
        private readonly int offset;
        private readonly int len;
        private string rawText;

        protected Element(string data, int offset, int len)
        {
            this.data = data;
            this.offset = offset;
            this.len = len;
        }

        public virtual string RawText
        {
            get
            {
                if (rawText == null)
                    rawText = data.Substring(offset, len);
                return rawText;
            }
        }

        protected string Data
        {
            get { return data; }
        }

        public int Offset
        {
            get { return offset; }
        }

        public int Length
        {
            get { return len; }
        }

        public override string ToString()
        {
            return RawText;
        }
    }

    public interface ILiteralElement
    {
        string LiteralText { get; set; }
    }

    public class Comment : Element
    {
        public Comment(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    public class MarkupDirective : Element
    {
        public MarkupDirective(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    public abstract class ScriptElement : Element
    {
        public ScriptElement(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    /// <summary>
    /// Represents text between script tags.
    /// </summary>
    public class ScriptText : ScriptElement
    {
        public ScriptText(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    /// <summary>
    /// Represents text between single or double quotes in a script.
    /// </summary>
    public class ScriptLiteral : ScriptElement, ILiteralElement
    {
        private readonly int literalOffset;
        private readonly int literalLen;
        private readonly char quotChar;
        private string literalOverride;

        public ScriptLiteral(string data, int offset, int len, int literalOffset, int literalLen, char quotChar) : base(data, offset, len)
        {
            this.literalOffset = literalOffset;
            this.literalLen = literalLen;
            this.quotChar = quotChar;
        }

        public string LiteralText
        {
            get
            {
                if (literalOverride == null)
                    return LiteralElementMethods.JsUnescape(Data.Substring(literalOffset, literalLen));
                else
                    return literalOverride;
            }
            set
            {
                if (value == null)
                    value = "";
                literalOverride = value;
            }
        }

        public override string ToString()
        {
            if (literalOverride == null)
                return RawText;
            return Data.Substring(Offset, literalOffset - Offset) +
                LiteralElementMethods.JsEscape(literalOverride, quotChar) +
                Data.Substring(literalOffset + literalLen, (Offset + Length) - (literalOffset + literalLen));
        }
    }

    /// <summary>
    /// Represents a comment in a script.
    /// </summary>
    public class ScriptComment : ScriptElement
    {
        public ScriptComment(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    public abstract class StyleElement : Element
    {
        public StyleElement(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    /// <summary>
    /// Represents text between Style tags.
    /// </summary>
    public class StyleText : StyleElement
    {
        public StyleText(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    /// <summary>
    /// Represents text between single or double quotes in a Style.
    /// </summary>
    public class StyleLiteral : StyleElement, ILiteralElement
    {
        private readonly int literalOffset;
        private readonly int literalLen;
        private readonly char quotChar;
        private string literalOverride;

        public StyleLiteral(string data, int offset, int len, int literalOffset, int literalLen, char quotChar) : base(data, offset, len)
        {
            this.literalOffset = literalOffset;
            this.literalLen = literalLen;
            this.quotChar = quotChar;
        }

        public string LiteralText
        {
            get
            {
                if (literalOverride == null)
                    return LiteralElementMethods.CssUnescape(Data.Substring(literalOffset, literalLen));
                else
                    return literalOverride;
            }
            set
            {
                if (value == null)
                    value = "";
                literalOverride = value;
            }
        }

        public override string ToString()
        {
            if (literalOverride == null)
                return RawText;
            return Data.Substring(Offset, literalOffset - Offset) +
                LiteralElementMethods.CssEscape(literalOverride, quotChar) +
                Data.Substring(literalOffset + literalLen, (Offset + Length) - (literalOffset + literalLen));
        }

    }

    /// <summary>
    /// Represents a comment in a Style.
    /// </summary>
    public class StyleComment : StyleElement
    {
        public StyleComment(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    /// <summary>
    /// Represents a URL in a Style.
    /// </summary>
    public class StyleUrl : StyleElement, ILiteralElement
    {
        private readonly int literalOffset;
        private readonly int literalLen;
        private readonly char quotChar;
        private string literalOverride;

        public StyleUrl(string data, int offset, int len, int literalOffset, int literalLen, char quotChar) : base(data, offset, len)
        {
            this.literalOffset = literalOffset;
            this.literalLen = literalLen;
            this.quotChar = quotChar;
        }

        public string LiteralText
        {
            get
            {
                if (literalOverride == null)
                    return LiteralElementMethods.CssUnescape(Data.Substring(literalOffset, literalLen));
                else
                    return literalOverride;
            }
            set
            {
                if (value == null)
                    value = "";
                literalOverride = value;
            }
        }

        public override string ToString()
        {
            if (literalOverride == null)
                return RawText;
            return Data.Substring(Offset, literalOffset - Offset) +
                (quotChar == 0 ? "\"" : "") +
                LiteralElementMethods.CssEscape(literalOverride, quotChar, '(', ')', ' ', '\t', '"', '\'', ',') +
                (quotChar == 0 ? "\"" : "") +
                Data.Substring(literalOffset + literalLen, (Offset + Length) - (literalOffset + literalLen));
        }
    }

    /// <summary>
    /// Represents an @import in a Style.
    /// </summary>
    public class StyleImport : StyleElement, ILiteralElement
    {
        private readonly int literalOffset;
        private readonly int literalLen;
        private readonly char quotChar;
        private string literalOverride;

        public StyleImport(string data, int offset, int len, int literalOffset, int literalLen, char quotChar) : base(data, offset, len)
        {
            this.literalOffset = literalOffset;
            this.literalLen = literalLen;
            this.quotChar = quotChar;
        }

        public string LiteralText
        {
            get
            {
                if (literalOverride == null)
                    return LiteralElementMethods.CssUnescape(Data.Substring(literalOffset, literalLen));
                else
                    return literalOverride;
            }
            set
            {
                if (value == null)
                    value = "";
                literalOverride = value;
            }
        }

        public override string ToString()
        {
            if (literalOverride == null)
                return RawText;
            return Data.Substring(Offset, literalOffset - Offset) +
                (quotChar == 0 ? "\"" : "") +
                LiteralElementMethods.CssEscape(literalOverride, quotChar, '(', ')', ' ', '\t', '"', '\'', ',') +
                (quotChar == 0 ? "\"" : "") +
                Data.Substring(literalOffset + literalLen, (Offset + Length) - (literalOffset + literalLen));
        }
    }

    public class Text : Element
    {
        public Text(string data, int offset, int len) : base(data, offset, len)
        {
        }
    }

    /// <summary>
    /// Common interface for BeginTag and EndTag.
    /// </summary>
    public abstract class Tag : Element
    {
        private readonly string name;

        public Tag(string data, int offset, int len, string name) : base(data, offset, len)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
        }

        /// <summary>
        /// The name of the tag.  Case is identical to what
        /// was used in the HTML (for case-insensitive equality
        /// testing, use the NameEquals method).
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Case-insensitive equality testing of the tag's name and
        /// the given name.
        /// </summary>
        public bool NameEquals(string name)
        {
            return string.Compare(name, this.name, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    /// <summary>
    /// A begin tag. Examples:
    /// <a href="#">
    /// <option selected>
    /// <br>
    /// </summary>
    public class BeginTag : Tag
    {
        private bool complete;

        private bool modified = false;

        private readonly Attr[] _attributes;
        private readonly LazySubstring extraResidue;

        public BeginTag(string data, int offset, int len, string name, Attr[] attributes, bool complete, string extraResidue) : this(data, offset, len, name, attributes, complete, LazySubstring.MaybeCreate(extraResidue))
        {
        }

        internal BeginTag(string data, int offset, int len, string name, Attr[] attributes, bool complete, LazySubstring extraResidue) : base(data, offset, len, name)
        {
            this.complete = complete;
            _attributes = attributes == null ? new Attr[0] : attributes;
            this.extraResidue = extraResidue;
        }

        public Attr[] Attributes
        {
            get { return _attributes; }
        }

        public bool Complete
        {
            get { return complete; }
            set
            {
                if (complete != value)
                {
                    modified = true;
                    complete = value;
                }
            }
        }

        /// <summary>
        /// True if this begin tag did not properly terminate with a ">".
        /// For example:
        ///   <a href="foo" <br>
        /// In this example, the "a" tag is unterminated.
        /// </summary>
        public bool Unterminated
        {
            get
            {
                return Length == 0 || Data[Offset + Length - 1] != '>';
            }
        }

        /// <summary>
        /// Inexpensively determine whether this tag has residue. This
        /// property is only beneficial if you only want to know if
        /// residue is present, and are not planning on retrieving the
        /// value of the residue.
        /// </summary>
        public bool HasResidue
        {
            get { return extraResidue != null; }
        }

        /// <summary>
        /// Any junk between the end of the last syntactically valid
        /// attribute and the end of the tag.
        /// </summary>
        public string Residue
        {
            get { return extraResidue == null ? null : extraResidue.Value; }
        }

        public Attr GetAttribute(string attrName)
        {
            int foundAt;
            return GetAttribute(attrName, 0, out foundAt);
        }

        public Attr GetAttribute(string attrName, int startIndex, out int foundAt)
        {
            return GetAttribute(attrName, false, startIndex, out foundAt);
        }

        public Attr GetAttribute(string attrName, bool allowNoValue, int startIndex, out int foundAt)
        {
            foundAt = -1;

            if (attrName == null)
                return null;

            Attr[] attributes = Attributes;
            for (int i = startIndex; i < attributes.Length; i++)
            {
                Attr attr = attributes[i];
                if (attr != null && attr.NameEquals(attrName) && (allowNoValue || attr.Value != null))
                {
                    foundAt = i;
                    return attr;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first non-null attribute value whose name
        /// matches the given name (case insensitive).
        /// </summary>
        public string GetAttributeValue(string attrName)
        {
            Attr attr = GetAttribute(attrName);
            return (attr == null) ? null : attr.Value;
        }

        public bool RemoveAttribute(string attrName)
        {
            return RemoveAttribute(attrName, 0);
        }

        public bool RemoveAttribute(string attrName, int startIndex)
        {
            Attr[] attributes = Attributes;
            for (int i = startIndex; i < attributes.Length; i++)
            {
                Attr attr = attributes[i];
                if (attr != null && attr.NameEquals(attrName))
                {
                    attributes[i] = null;
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            bool isModified = modified;
            if (!isModified)
            {
                foreach (Attr attr in Attributes)
                {
                    if (attr == null || attr.Modified)
                    {
                        isModified = true;
                        break;
                    }
                }
            }

            if (isModified)
            {
                StringBuilder result = new StringBuilder();
                result.Append("<").Append(Name);
                if (Attributes.Length != 0)
                {
                    result.Append(" ");
                    string delim = "";
                    foreach (Attr attr1 in Attributes)
                    {
                        if (attr1 == null)
                            continue;
                        result.Append(delim);
                        result.Append(attr1.ToString());
                        delim = " ";
                    }
                }
                if (HasResidue)
                    result.Append(Residue);

                if (complete)
                    result.Append(" /");
                result.Append(">");
                return result.ToString();
            }

            return RawText;
        }
    }

    /// <summary>
    /// An end tag. Example:
    /// </a>
    ///
    /// If Implicit is true, then it came from a "complete" tag:
    /// <br />
    ///
    /// </summary>
    public class EndTag : Tag
    {
        private readonly bool implied;

        public EndTag(string data, int offset, int len, string name) : this(data, offset, len, name, false)
        {
        }

        public EndTag(string data, int offset, int len, string name, bool implied) : base(data, offset, len, name)
        {
            this.implied = implied;
        }

        public bool Implicit
        {
            get { return implied; }
        }

        public override string ToString()
        {
            if (implied)
                return string.Empty;
            return string.Format(CultureInfo.InvariantCulture, "</{0}>", Name);
        }
    }

    /// <summary>
    /// A single HTML attribute.  Value may be null.
    /// </summary>
    public class Attr
    {
        private readonly LazySubstring name;
        private readonly LazySubstring value;
        private string overrideValue;
        private bool modified;

        internal Attr(LazySubstring name, LazySubstring value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
            this.value = value;
            this.modified = false;
        }

        public string Name
        {
            get { return name.Value; }
        }

        public string Value
        {
            get
            {
                if (modified)
                    return overrideValue;

                // Attributes like 'Nowrap' can have no value
                if (value == null)
                    return null;

                return HtmlUtils.UnEscapeEntities(value.Value, HtmlUtils.UnEscapeMode.Attribute);
            }
            set
            {
                modified = true;
                overrideValue = value;
            }
        }

        public bool Modified
        {
            get { return modified; }
        }

        public override string ToString()
        {
            string valToUse = null;
            if (modified)
                valToUse = overrideValue == null ? null : HtmlUtils.EscapeEntities(overrideValue);
            else
                valToUse = value == null ? null : value.Value;

            return valToUse == null ? name.Value : string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"", name.Value, valToUse);
        }

        public override bool Equals(object obj)
        {
            Attr other = (Attr)obj;
            return string.Equals(name.Value, other.name.Value) &&
                string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() + (value == null ? 0 : value.GetHashCode());
        }

        public bool NameEquals(string name)
        {
            return string.Compare(name, this.name.Value, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

}
