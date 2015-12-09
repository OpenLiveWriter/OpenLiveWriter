// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{
    public class SmallCapsTextStyle : TextStyle, IEquatable<SmallCapsTextStyle>
    {
        public SmallCapsTextStyle(bool smallCaps)
        {
            SmallCaps = smallCaps;
        }

        public bool SmallCaps { get; private set; }

        public override void Apply(MshtmlMarkupServices markupServices, MarkupRange markupRange, MshtmlCoreCommandSet commands)
        {
            const string smallCapsAttribute = "style=\"font-variant: small-caps\"";
            ElementFactory factory = () => markupServices.CreateElement(_ELEMENT_TAG_ID.TAGID_FONT, smallCapsAttribute);
            WrapInElement(factory, markupServices, markupRange);
        }

        public override bool Equals(TextStyle obj)
        {
            return Equals(obj as SmallCapsTextStyle);
        }

        public bool Equals(SmallCapsTextStyle obj)
        {
            if ((object)obj == null)
            {
                return false;
            }

            return SmallCaps.Equals(obj.SmallCaps);
        }

        public override int GetHashCode()
        {
            return SmallCaps.GetHashCode();
        }
    }

}
