// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.BlogClient.Clients
{
    internal enum AtomContentValueType { Text, HTML, XHTML };

    /// <summary>
    /// Encapsulates Atom content, which can be text, HTML, or XHTML,
    /// and allows easy conversion to text or HTML regardless of
    /// input type.
    /// </summary>
    internal class AtomContentValue
    {
        private readonly AtomContentValueType _type;
        private readonly string _value;

        public AtomContentValue(AtomContentValueType type, string value)
        {
            _type = type;
            _value = value;
        }

        public AtomContentValueType Type { get { return _type; } }

        public string GetValue(AtomContentValueType type)
        {
            if (_value == null)
                return null;

            switch (type)
            {
                case AtomContentValueType.Text:
                    return ToText();
                case AtomContentValueType.HTML:
                    return ToHTML();
                case AtomContentValueType.XHTML:
                    if (_type == AtomContentValueType.XHTML)
                        return _value;
                    else
                        throw new ArgumentException("Cannot convert text value to XHTML", "type");
            }
            throw new InvalidOperationException("Unknown text type: " + _type);
        }

        public string ToText()
        {
            if (_value == null)
                return null;

            switch (_type)
            {
                case AtomContentValueType.Text:
                    return _value;
                case AtomContentValueType.HTML:
                case AtomContentValueType.XHTML:
                    return HtmlUtils.HTMLToPlainText(_value, false);
            }
            throw new InvalidOperationException("Unknown text type: " + _type);
        }

        public string ToHTML()
        {
            if (_value == null)
                return null;

            switch (_type)
            {
                case AtomContentValueType.Text:
                    return HtmlUtils.EscapeEntities(_value);
                case AtomContentValueType.HTML:
                case AtomContentValueType.XHTML:
                    return _value;
            }
            throw new InvalidOperationException("Unknown text type: " + _type);
        }
    }
}
