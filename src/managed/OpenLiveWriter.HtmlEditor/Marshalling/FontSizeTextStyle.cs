// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class FontSizeTextStyle : TextStyle, IEquatable<FontSizeTextStyle>
    {
        public FontSizeTextStyle(float fontSizeInPoints)
        {
            FontSizeInPoints = fontSizeInPoints;
        }

        public float FontSizeInPoints { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            string fontSizeAttribute = String.Format(CultureInfo.InvariantCulture, "style=\"font-size:{0:F1}pt\"", FontSizeInPoints);
            ElementFactory factory = () => markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_FONT, fontSizeAttribute);
            WrapInElement(factory, markupServices, markupRange);
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as FontSizeTextStyle);
        }

        public bool Equals(FontSizeTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return FontSizeInPoints == obj.FontSizeInPoints;
        }

        public override int GetHashCode()
        {
            return FontSizeInPoints.GetHashCode();
        }
    }

}
